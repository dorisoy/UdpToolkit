using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using UdpToolkit.Core;

namespace UdpToolkit.Network
{
    public sealed class UdpReceiver : IUdpReceiver
    {
        private readonly AsyncQueue<InputUdpPacket> _inputQueue;
        private readonly UdpClient _receiver;
        private readonly IPeerTracker _peerTracker;

        public UdpReceiver(
            AsyncQueue<InputUdpPacket> inputQueue,
            UdpClient receiver, 
            IPeerTracker peerTracker)

        {
            _inputQueue = inputQueue;
            _receiver = receiver;
            _peerTracker = peerTracker;
        }
        
        //TODO thread safe inside method
        public async Task StartReceive()
        {
            while (true)
            {
                var result = await _receiver.ReceiveAsync();
                var peer = new Peer(result.RemoteEndPoint);

                if (result.Buffer.Length < Consts.UdpProtocolHeaderLength)
                {
                    //TODO log invalid udp packet
                    
                    continue;
                }

                //TODO generate scopeId for client
                var scopeId = ReadScopeId(result.Buffer);
                
                _peerTracker.TryAddPeerToScope(scopeId, peer);

                var inputUdpPacket = new InputUdpPacket(
                    peerId: peer.Id,
                    hubId: result.Buffer[0],
                    rpcId: result.Buffer[1],
                    scopeId: scopeId,
                    request: new ArraySegment<byte>(
                        array: result.Buffer,
                        offset: Consts.UdpProtocolHeaderLength, 
                        count: result.Buffer.Length - Consts.UdpProtocolHeaderLength));

                _inputQueue.Publish(inputUdpPacket);
            }
        }

        private ushort ReadScopeId(byte[] buffer)
        {
            return BitConverter.ToUInt16(new byte[] { buffer[2], buffer[3] }); //TODO span api
        }

        public void Dispose()
        {
            _receiver.Dispose();
        }
    }
}
