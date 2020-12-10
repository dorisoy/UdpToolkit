using System;
using System.Threading.Tasks;
using Cubes.Shared.Events;
using Cubes.Shared.Server;
using UdpToolkit.Core;
using UdpToolkit.Core.ProtocolEvents;
using UdpToolkit.Framework;
using UdpToolkit.Serialization.MsgPack;
using UnityEngine;

public class Network : MonoBehaviour
{
    private const byte roomId = 123;

    [SerializeField]
    private GameObject myPlayerPrefab;

    [SerializeField]
    private GameObject playerPrefab;

    private IServerHostClient serverHostClient;
    private IHost host;
    private IEventHandler<Connect> connectEventHandler;
    private IEventHandler<JoinEvent> joinEventHandler;
    private IEventHandler<SpawnEvent> spawnEventHandler;
    private IEventHandler<MoveEvent> moveEventHandler;

    void Start()
    {
        host = UdpHost
            .CreateHostBuilder()
            .ConfigureHost((settings) =>
            {
                settings.Host = "127.0.0.1";
                settings.Serializer = new Serializer();
                settings.InputPorts = new[] { 5000, 5001 };
                settings.OutputPorts = new[] { 6000, 6001 };
                settings.Workers = 2;
                settings.ResendPacketsTimeout = TimeSpan.FromSeconds(120);
                settings.PeerInactivityTimeout = TimeSpan.FromSeconds(120);
            })
            .ConfigureServerHostClient((settings) =>
            {
                settings.ResendPacketsTimeout = TimeSpan.FromSeconds(120);
                settings.ConnectionTimeout = TimeSpan.FromSeconds(120);
                settings.ClientHost = "127.0.0.1";
                settings.ServerHost = "127.0.0.1";
                settings.ServerInputPorts = new[] { 7000, 7001 };
                settings.ServerOutputPorts = new[] { 8000, 8001 };
                settings.PingDelayInMs = null; // pass null for disable pings
            })
            .Build();

        serverHostClient = host.ServerHostClient;

        Task.Run(() => host.RunAsync());

        connectEventHandler = new ConnectEventHandler(host, NetworkThreadDispatcher.Instance());
        joinEventHandler = new JoinEventHandler(host, NetworkThreadDispatcher.Instance());
        spawnEventHandler = new SpawnEventHandler(host, NetworkThreadDispatcher.Instance());
        moveEventHandler = new MoveEventHandler(host, NetworkThreadDispatcher.Instance());

        connectEventHandler.OnTimeout += peerId => Debug.Log($"Connection timeout for peer {peerId}");
        connectEventHandler.OnAck += peerId => Debug.Log($"You connected with peerId {peerId}");

        joinEventHandler.OnEvent += joinEvent => Debug.Log($"{joinEvent.Nickname} joined to {joinEvent.RoomId}");
        joinEventHandler.OnAck += peerId => Debug.Log($"You joined to room with id {peerId}");

        spawnEventHandler.OnEvent += spawnEvent => Instantiate(
            original: myPlayerPrefab,
            position: spawnEvent.Position,
            rotation: Quaternion.identity);

        moveEventHandler.OnEvent += moveEvent => Debug.Log("Move");
    }

    void OnApplicationQuit()
    {
        host.Stop();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Connect to server...");
            serverHostClient.ConnectAsync();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("Join to server...");

            serverHostClient.Publish(
                @event: new JoinEvent(roomId: roomId, nickname: "Nickname " + DateTime.UtcNow),
                hookId: 1,
                udpMode: UdpMode.ReliableUdp);
        }
    }
}
