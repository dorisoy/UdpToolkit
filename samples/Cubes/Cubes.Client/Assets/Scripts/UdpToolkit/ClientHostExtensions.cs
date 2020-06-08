using System;
using UdpToolkit.Framework.Client.Core;

public static class ClientHostExtensions
{
    public static void OnEvent<TEvent>(this IClientHost clientHost, Action<TEvent> handler)
    {
        if (clientHost == null)
        {
            throw new ArgumentNullException(nameof(clientHost));
        }

        clientHost.On<TEvent>((evnt) => NetworkThreadToMainThreadDispatcher.Instance().Enqueue(() => handler(evnt)));
    }
}