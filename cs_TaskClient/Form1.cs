using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
//using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace cs_TaskClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            //Thread.Sleep(5000);
            InitializeComponent();
            textBoxIPAddress.Text = @"127.0.0.1";
        }

        private async void ButtonStart_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                var client = new TcpClient();
                client.Connect(textBoxIPAddress.Text, 45678);

                var stream = client.GetStream();
                var bw = new BinaryWriter(stream);

                bw.Write("Connect");

                while (true)
                {
                    FromServer(stream);
                }
            });
        }

        private async void FromServer(NetworkStream stream)
        {
            var listener = new UdpClient(45679);
            var ep = new IPEndPoint(IPAddress.Any, 0);

            var br = new BinaryReader(stream);
            byte[] bytes = null;
            var imageBytes = new List<byte>();

            while (true)
            {
                var length = br.ReadInt32();
                do
                {
                    Task.Run(() => { bytes = listener.Receive(ref ep); }).Wait();

                    imageBytes.AddRange(bytes);
                    length -= bytes.Length;
                } while (length > 0);

                await using var ms = new MemoryStream(imageBytes.ToArray());
                pictureBoxScreenShot.Image = Image.FromStream(ms);
                imageBytes.Clear();
            }
        }
    }
}
