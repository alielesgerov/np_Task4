using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace cs_TaskClient
{
    public partial class Form1 : Form
    {
        private readonly TcpClient _client;
        private BinaryWriter _bw;
        private NetworkStream _ns;
        private readonly UdpClient _listener;
        private IPEndPoint _ep;

        public Form1()
        {
            //Thread.Sleep(5000);
            InitializeComponent();

            textBoxIPAddress.Text = @"127.0.0.1";
            _client = new TcpClient();


            _listener = new UdpClient(45679);
            _ep = new IPEndPoint(IPAddress.Any, 0);
        }

        private async void ButtonStart_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                _client.Connect(textBoxIPAddress.Text, 45678);

                _ns = _client.GetStream();
                _bw = new BinaryWriter(_ns);

                _bw.Write("Connect");

                //FromServer();

                while (true)
                {
                    FromServer();
                }
            });
        }

        private void FromServer()
        {

            byte[] bytes;
            var imageBytes = new List<byte>();

            while (true)
            {
                while (true)
                {
                    bytes = _listener.Receive(ref _ep);
                    if (bytes.Length != 0)
                        imageBytes.AddRange(bytes);
                    else
                        break;
                }

                using var ms = new MemoryStream(imageBytes.ToArray());
                pictureBoxScreenShot.Image = Image.FromStream(ms);
                imageBytes.Clear();
            }
        }
    }
}
