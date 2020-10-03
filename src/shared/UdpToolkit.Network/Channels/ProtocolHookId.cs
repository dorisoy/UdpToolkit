namespace UdpToolkit.Network.Channels
{
    using System;

    public enum ProtocolHookId : byte
    {
        P2P = 248,
        Ping = 249,
        Pong = 250,
        Ack = 251,
        Connect = 252,
        Disconnect = 253,
        Disconnected = 254,
        Connected = 255,
    }
}
