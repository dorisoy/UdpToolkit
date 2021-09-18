#pragma warning disable SA0001, SA1600
namespace UdpToolkit.Sandbox
{
    using UnityEngine;

    public class LerpPositionObserver : MonoBehaviour, IPositionObserver
    {
        [SerializeField]
        private float interpolateLerpSpeed = 10f;

        private Transform _transform;
        private Vector3 _position;
        private Quaternion _rotation;

        public void OnChanged(
            Vector3 networkPosition,
            Quaternion networkRotation)
        {
            _position = networkPosition;
            _rotation = networkRotation;
        }

        private static Vector3 ExtrapolatePosition() => Vector3.zero;

        private void Start()
        {
            _transform = GetComponent<Transform>();
        }

        private void Update()
        {
            _transform.localPosition = LerpPosition();
            _transform.localRotation = LerpRotation();
        }

        private Vector3 LerpPosition()
        {
            var currentPosition = transform.localPosition;
            var targetPosition = _position + ExtrapolatePosition();

            return Vector3.Lerp(currentPosition, targetPosition, Time.deltaTime * this.interpolateLerpSpeed);
        }

        private Quaternion LerpRotation()
        {
            return Quaternion.Lerp(transform.localRotation, _rotation, Time.deltaTime * this.interpolateLerpSpeed);
        }
    }
}
#pragma warning restore SA0001, SA1600