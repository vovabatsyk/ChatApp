using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChatAppServer
{
    public partial class Form1 : Form
    {
        static readonly object Lock = new object();
        static readonly Dictionary<int, TcpClient> ListClients = new Dictionary<int, TcpClient>();
        volatile bool _isConnected;
        Thread _thread;
        int _count;
        TcpListener _serverSocket;
        public Form1()
        {
            InitializeComponent();
        }

        private async void buttonConnect_Click(object sender, EventArgs e)
        {
            if (_isConnected == false)
            {
                _count = 1;

                string serverIp = textBoxIP.Text;
                int serverPort = Int32.Parse(textBoxPort.Text);

                _serverSocket = new TcpListener(IPAddress.Parse(serverIp), serverPort);
                _isConnected = true;

                try
                {
                    _serverSocket.Start();
                    textBoxChat.Text += @"Server start!" + Environment.NewLine;

                    buttonConnect.Enabled = false;
                    textBoxIP.Enabled = false;
                    textBoxPort.Enabled = false;

                }
                catch
                {
                    textBoxChat.Text += @"Failed to connect to server" + Environment.NewLine;
                }
            }
            else
            {
                textBoxChat.AppendText(@"Already Connected!" + Environment.NewLine);
            }

            while (_isConnected)
            {
                TcpClient client = await _serverSocket.AcceptTcpClientAsync();
                lock (Lock) ListClients.Add(_count, client);

                _thread = new Thread(HandleClients);
                _thread.Start(_count);
                _count++;
            }
        }

        public void HandleClients(object obj)
        {
            int id = (int)obj;
            TcpClient client;

            lock (Lock) client = ListClients[id];

            while (true)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int byteCount = stream.Read(buffer, 0, buffer.Length);

                if (byteCount == 0)
                {
                    break;
                }

                string data = Encoding.ASCII.GetString(buffer, 0, byteCount);
                Listen(data);
                textBoxChat.Invoke(new Action(() =>
                {
                    textBoxChat.AppendText(data + Environment.NewLine);
                }));

            }


            lock (Lock) ListClients.Remove(id);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        public static void Listen(string data)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data + Environment.NewLine);

            lock (Lock)
            {
                foreach (TcpClient c in ListClients.Values)
                {
                    NetworkStream stream = c.GetStream();

                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }
}
