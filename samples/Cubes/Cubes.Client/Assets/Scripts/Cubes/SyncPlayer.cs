using System.Collections.Generic;
using System.Linq;
using Cubes.Shared.Events;
using UnityEngine;

public class SyncPlayer : MonoBehaviour
{
    public Queue<MoveEvent> _moves = new Queue<MoveEvent>();

    private float _lerpTime = 0;

    void Update()
    {
        if (!_moves.Any())
        {
            return;
        }

        var moveEvent = _moves.Dequeue();

        var currentPosition = transform.position;
        var currentRotation = transform.rotation;

        var networkPosition = moveEvent.Position;
        var networkRotation = moveEvent.Rotation;

        _lerpTime += Time.deltaTime / Time.fixedDeltaTime;

        transform.position = Vector3.Lerp(a: currentPosition, b: networkPosition, t: _lerpTime);
        transform.rotation = Quaternion.Lerp(a: currentRotation, b: networkRotation, t: _lerpTime);
    }

    public void ApplyMove(MoveEvent moveEvent)
    {
        _moves.Enqueue(moveEvent);
        _lerpTime = 0f;
    }
}
