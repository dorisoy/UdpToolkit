namespace UdpToolkit.Core
{
    using System;

    public interface ITimersPool : IDisposable
    {
        void EnableResend(
            IPeer peer);

        bool DisableResend(
            Guid peerId);
    }
}