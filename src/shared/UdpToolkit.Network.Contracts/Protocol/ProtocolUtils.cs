namespace UdpToolkit.Network.Contracts.Protocol
{
    internal static class ProtocolUtils
    {
        internal static bool IsProtocolEvent(byte hookId)
        {
            return hookId >= (byte)ProtocolHookId.P2P;
        }
    }
}