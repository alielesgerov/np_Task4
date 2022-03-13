using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace cs_TaskServer
{
    internal static class Program
    {
        private static UdpClient _client;
        private static IPEndPoint _connectEp;
        private static TcpListener _listener;
        private static NetworkStream _stream;

        private static void Main()
        {
            // UDP
            _connectEp = new IPEndPoint(IPAddress.Loopback, 45679);
            _client = new UdpClient();




            // TCP
            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 45678);
            _listener.Start(5);

            var client = _listener.AcceptTcpClient();
            _stream = client.GetStream();

            var br = new BinaryReader(_stream);

            var data = br.ReadString();

            PrintScreen.CaptureScreen().Save("Test.png", ImageFormat.Png);

            if (data != "Connect") return;

            Console.WriteLine("Connected");
            while (true)
            {
                ToClient();
                //Thread.Sleep(24);
            }
        }

        private static byte[] ImageToByteArray(Image image)
        {
            var imageConverter = new ImageConverter();
            return (byte[])imageConverter.ConvertTo(image, typeof(byte[]));
        }

        #region Test

        //private static Image ResizeImage(Image image)
        //{
        //    return new Bitmap(image, new Size(image.Width/2, image.Height/2));
        //}

        #endregion

        private static void ToClient()
        {
            var image = PrintScreen.CaptureScreen();

            var imageBytes = ImageToByteArray(/*ResizeImage*/(image));

            var skipCount = 0;
            const int maxValue = ushort.MaxValue - 28;
            var bytesLength = imageBytes.Length;

            if (imageBytes.Length > maxValue)
            {
                while (skipCount + maxValue <= bytesLength)
                {
                    _client.Send(imageBytes
                        .Skip(skipCount)
                        .Take(maxValue)
                        .ToArray(), maxValue, _connectEp);

                    skipCount += maxValue;
                }

                if (skipCount != bytesLength)
                {
                    _client.Send(imageBytes
                        .Skip(skipCount)
                        .Take(bytesLength)
                        .ToArray(), bytesLength - skipCount, _connectEp);
                }
            }
            else
                _client.Send(imageBytes, imageBytes.Length, _connectEp);

            _client.Send(new byte[]{}, 0, _connectEp);

        }

    }
}
