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
        private TcpClient _client { get; set; }
        private BinaryWriter _bw { get; set; }
        private NetworkStream _ns { get; set; }

        private UdpClient _listener { get; set; }

        private IPEndPoint _ep;

        public Form1()
        {
            //Thread.Sleep(5000);
            InitializeComponent();

            textBoxIPAddress.Text = "127.0.0.1";
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

            do
            {
                bytes = _listener.Receive(ref _ep);
                imageBytes.AddRange(bytes);

            } while (bytes.Length == (ushort.MaxValue - 28));

            // Burda butun byte-lari oxuyandan sonra shekile cevirmelisen.
            //Thread.Sleep(3000); // exception

            using var ms = new MemoryStream(imageBytes.ToArray());
            pictureBoxScreenShot.Image = Image.FromStream(ms);
        }
    }
}
