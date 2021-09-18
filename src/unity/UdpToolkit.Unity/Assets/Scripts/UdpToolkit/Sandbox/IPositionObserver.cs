#pragma warning disable SA0001, SA1600
namespace UdpToolkit.Sandbox
{
    using UnityEngine;

    public interface IPositionObserver
    {
        void OnChanged(
            Vector3 networkPosition,
            Quaternion networkRotation);
    }
}
#pragma warning restore SA0001, SA1600