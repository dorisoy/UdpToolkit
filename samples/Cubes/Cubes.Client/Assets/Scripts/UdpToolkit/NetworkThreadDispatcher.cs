using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Cubes.Shared;
using UnityEngine;

public class NetworkThreadDispatcher : MonoBehaviour, INetworkThreadDispatcher
{
    private static readonly ConcurrentQueue<Action> ExecutionQueue = new ConcurrentQueue<Action>();

    public void Update()
    {
        while (ExecutionQueue.TryDequeue(out var action))
        {
            action.Invoke();
        }
    }

    public void Enqueue(IEnumerator action)
    {
        ExecutionQueue.Enqueue(() =>
        {
            StartCoroutine(action);
        });
    }

    public void Enqueue(Action action)
    {
        Enqueue(ActionWrapper(action));
    }

    public Task EnqueueAsync(Action action)
    {
        var tcs = new TaskCompletionSource<bool>();

        void WrappedAction()
        {
            try
            {
                action();
                tcs.TrySetResult(true);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }

        Enqueue(ActionWrapper(WrappedAction));
        return tcs.Task;
    }

    IEnumerator ActionWrapper(Action a)
    {
        a();
        yield return null;
    }


    private static NetworkThreadDispatcher _instance = null;

    private static bool Exists()
    {
        return _instance != null;
    }

    public static NetworkThreadDispatcher Instance()
    {
        if (!Exists())
        {
            throw new Exception("NetworkMainThreadDispatcher could not be found. Please ensure you have added the Network Prefab to your scene.");
        }

        return _instance;
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    void OnDestroy()
    {
            _instance = null;
    }
}
