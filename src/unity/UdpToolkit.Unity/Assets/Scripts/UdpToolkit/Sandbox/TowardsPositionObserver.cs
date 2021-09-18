#pragma warning disable SA0001, SA1600
namespace UdpToolkit.Sandbox
{
    using UnityEngine;

    public sealed class TowardsPositionObserver : MonoBehaviour, IPositionObserver
    {
        [SerializeField]
        private bool useLocal = true;
        [SerializeField]
        private int receiveRate = 10;

        private Transform _transform;
        private Vector3 _position;
        private Quaternion _rotation;

        private float _distance;
        private float _angle;

        public void OnChanged(
            Vector3 networkPosition,
            Quaternion networkRotation)
        {
            _position = networkPosition;
            _rotation = networkRotation;

            if (useLocal)
            {
                _distance = Vector3.Distance(_transform.localPosition, _position);
                _angle = Quaternion.Angle(_transform.localRotation, _rotation);
            }
            else
            {
                _distance = Vector3.Distance(_transform.position, _position);
                _angle = Quaternion.Angle(_transform.rotation, _rotation);
            }
        }

        private void Start()
        {
            _transform = GetComponent<Transform>();
            Debug.Log(nameof(TowardsPositionObserver));
        }

        private void Update()
        {
            if (useLocal)
            {
                _transform.localPosition = Vector3.MoveTowards(_transform.localPosition, _position, _distance * (1.0f / receiveRate));
                _transform.localRotation = Quaternion.RotateTowards(_transform.localRotation, _rotation, _angle * (1.0f / receiveRate));
            }
            else
            {
                _transform.position = Vector3.MoveTowards(_transform.position, _position, _distance * (1.0f / receiveRate));
                _transform.rotation = Quaternion.RotateTowards(_transform.rotation, _rotation, _angle * (1.0f / receiveRate));
            }
        }
    }
}
#pragma warning restore SA0001, SA1600