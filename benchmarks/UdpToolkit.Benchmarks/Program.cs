namespace UdpToolkit.Benchmarks
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var client = new UdpClient();
            var array = new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 60; i++)
            {
#pragma warning disable
                client.BeginSend(array, 10, new IPEndPoint(IPAddress.Loopback, 7777), CallBack, null);
                // 00:00:00.0034297

                await client
                    .SendAsync(array, bytes: 10, endPoint: new IPEndPoint(IPAddress.Loopback, 7777))
                    .ConfigureAwait(false);
            }

            Console.WriteLine($"Elapsed: {sw.Elapsed.ToString()}");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        private static void CallBack(IAsyncResult asyncResult)
        {
            // Console.WriteLine("Callback");
        }
    }
}