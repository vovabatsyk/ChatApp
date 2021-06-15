using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChatAppClient
{
    public partial class Form1 : Form
    {

        string _stringIp;
        int _port;
        TcpClient _client;
        StreamWriter _sw;
        string _name;
        NetworkStream _ns;
        Thread _thread;
        string _message;
        volatile bool _isConnected;
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (_isConnected == false)
            {
                _stringIp = textBoxIp.Text;
                _port = Int32.Parse(textBoxPort.Text);
                _client = new TcpClient(_stringIp, _port);
                _sw = new StreamWriter(_client.GetStream());
                _name = textBoxUserName.Text;

                _sw.Write(_name + ": connected.");
                _sw.Flush();

                _ns = _client.GetStream();
                _thread = new Thread(o => ReceiveData((TcpClient)o));
                _thread.Start(_client);

                _isConnected = true;
                textBoxIp.Enabled = false;
                textBoxUserName.Enabled = false;
                textBoxPort.Enabled = false;
                button1.Enabled = true;
                buttonConnect.Enabled = false;
                buttonSend.Enabled = true;
            }
            else
            {
                textBoxChat.AppendText("Already Connected" + Environment.NewLine);
            }


        }

        public void ReceiveData(TcpClient client)
        {
            _ns = client.GetStream();
            byte[] receivedBytes = new byte[1024];
            int byteCount;

            while ((byteCount = _ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
            {
                var count = byteCount;
                textBoxChat.Invoke(new Action(() =>
                {
                    textBoxChat.AppendText(Encoding.ASCII.GetString(receivedBytes, 0, count));
                }));
            }
        }

        public void DisconnectAndClose()
        {
            _sw.Write(_name + " has disconnected from the Server.");
            _sw.Flush();
            _client.Client.Shutdown(SocketShutdown.Send);

            _ns.Close();
            _client.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DisconnectAndClose();
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            _message = _name + ": " + textBoxMessage.Text;
            byte[] buffer = Encoding.ASCII.GetBytes(_message);
            _ns.Write(buffer, 0, buffer.Length);
            _ns.Flush();
            textBoxMessage.Text = String.Empty;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
           DisconnectAndClose();
        }
    }
}
