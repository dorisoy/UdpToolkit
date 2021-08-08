namespace UdpToolkit.Network.Contracts.Protocol
{
    public enum ProtocolHookId : byte
    {
        P2P = 251,
        Heartbeat = 252,
        Disconnect = 253,
        Connect2Peer = 254,
        Connect = 255,
    }
}
