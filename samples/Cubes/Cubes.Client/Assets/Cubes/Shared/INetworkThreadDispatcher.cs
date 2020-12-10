namespace Cubes.Shared
{
    using System;

    public interface INetworkThreadDispatcher
    {
        void Enqueue(Action action);
    }
}