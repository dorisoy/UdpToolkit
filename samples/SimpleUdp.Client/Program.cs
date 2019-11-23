using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MessagePack;
using SimpleUdp.Contracts;

namespace SimpleUdp.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new UdpClient();
            
            var me = new IPEndPoint(
                IPAddress.Parse("0.0.0.0"),
                5000);
            
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(me);
            
            var serverInput = new IPEndPoint(
                IPAddress.Parse("0.0.0.0"),
                7000);
            
            var serverOutput1 = new IPEndPoint(
                IPAddress.Parse("0.0.0.0"),
                8000);

            var serverOutput2 = new IPEndPoint(
                IPAddress.Parse("0.0.0.0"),
                8001);

            new Thread(() => StartSend(client, serverInput)).Start();
            new Thread(() => StartReceive(client, serverOutput1)).Start();
            new Thread(() => StartReceive(client, serverOutput2)).Start();
            
            Console.WriteLine("Simple udp client started...");
            Console.ReadKey();
        }

        private static void StartReceive(UdpClient client, IPEndPoint ipEndPoint)
        {
            while (true)
            {
                var bytes = client.Receive(ref ipEndPoint);
                var model = MessagePackSerializer.Deserialize<AddResponse>(bytes);
                Console.WriteLine($"Summ - {model.Sum} From - {ipEndPoint.Address}:{ipEndPoint.Port}");
            }
        }

        private static void StartSend(UdpClient client, IPEndPoint ipEndPoint)
        {
            var addRequest = new AddRequest
            {
                X = 7,
                Y = 12
            };

            var scopeIdBytes = BitConverter.GetBytes(1024); //TODO generate scopeId for client
            var bytes = MessagePackSerializer.Serialize(addRequest);
            var protocolHeader = new byte[] { 0, 0, scopeIdBytes[0], scopeIdBytes[1] };
            //var protocolHeader = new byte[] { 0, 1, scopeIdBytes[0], scopeIdBytes[1] };

            var datagram = protocolHeader
                .Concat(bytes)
                .ToArray();
            
            while (true)
            {
                client.Send(
                    datagram,
                    datagram.Length,
                    ipEndPoint);

                Thread.Sleep(100);
            }
        }
    }
}