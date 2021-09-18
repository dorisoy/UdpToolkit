#pragma warning disable SA0001, SA1600
namespace UdpToolkit.Sandbox
{
    using System;
    using System.Collections.Concurrent;
    using UnityEngine;

    public sealed class MainThreadDispatcher : MonoBehaviour, IMainThreadDispatcher
    {
        private readonly ConcurrentQueue<Action> _executionQueue = new ConcurrentQueue<Action>();

        public void Update()
        {
            while (_executionQueue.TryDequeue(out var action))
            {
                action.Invoke();
            }
        }

        public void Enqueue(Action action)
        {
            _executionQueue.Enqueue(action);
        }
    }
}
#pragma warning restore SA0001, SA1600