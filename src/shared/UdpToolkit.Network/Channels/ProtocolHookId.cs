namespace UdpToolkit.Network.Channels
{
    using System;

    public enum ProtocolHookId : byte
    {
        P2P = 249,
        Ping = 250,
        Pong = 251,
        Disconnect = 254,
        Connect = 255,
    }
}
