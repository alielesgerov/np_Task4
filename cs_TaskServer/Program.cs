using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace cs_TaskServer
{
    internal static class Program
    {
        private static void Main()
        {
            var listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 45678);
            listener.Start(5);

            var client = listener.AcceptTcpClient();
            var stream = client.GetStream();

            var br = new BinaryReader(stream);

            var data = br.ReadString();

            CaptureScreen().Save("Test.png", ImageFormat.Png);

            if (data == "Connect")
            {
                Console.WriteLine("Connected");

                while (true)
                {
                    ToClient(CaptureScreen());
                    Thread.Sleep(50);
                }
            }

        }

        private static byte[] ImageToByteArray(Image image)
        {
            var imageConverter = new ImageConverter();
            return (byte[])imageConverter.ConvertTo(image, typeof(byte[]));
        }

        private static void ToClient(Image image)
        {
            var client = new UdpClient();

            IPEndPoint connectEp = new(IPAddress.Loopback, 45679);

            var imageBytes = ImageToByteArray(image);

            var skipCount = 0;
            var maxValue = ushort.MaxValue - 28;
            var bytesLength = imageBytes.Length;

            if (imageBytes.Length > maxValue)
            {
                while (skipCount + maxValue <= bytesLength)
                {
                    client.Send(imageBytes
                        .Skip(skipCount)
                        .Take(maxValue)
                        .ToArray(), maxValue, connectEp);

                    skipCount += maxValue;
                }

                if (skipCount != bytesLength)
                {
                    client.Send(imageBytes
                        .Skip(skipCount)
                        .Take(bytesLength)
                        .ToArray(), bytesLength - skipCount, connectEp);
                }
            }
            else
                client.Send(imageBytes, imageBytes.Length, connectEp);

        }

        private static Image CaptureScreen()
        {
            return PrintScreen.CaptureScreen();
        }
    }
}
