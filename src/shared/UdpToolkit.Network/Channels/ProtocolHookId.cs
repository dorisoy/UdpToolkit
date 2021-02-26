namespace UdpToolkit.Network.Channels
{
    using System;

    public enum ProtocolHookId : byte
    {
        P2P = 252,
        Ping = 253,
        Disconnect = 254,
        Connect = 255,
    }
}
