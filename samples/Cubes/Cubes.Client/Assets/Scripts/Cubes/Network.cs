using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using Shared.Join;
using Shared.Move;
using Shared.Spawn;
using UdpToolkit.Core;
using UdpToolkit.Framework.Client.Core;
using UdpToolkit.Framework.Client.Host;
using UdpToolkit.Serialization.MsgPack;
using UnityEngine;

public class Network : MonoBehaviour
{
    private readonly Dictionary<byte, GameObject> _players = new Dictionary<byte, GameObject>();
    private readonly Dictionary<byte, SyncPlayer> _moves = new Dictionary<byte, SyncPlayer>();

    [SerializeField]
    private GameObject myPlayerPrefab;

    [SerializeField]
    private GameObject playerPrefab;

    private string myNickName = Guid.NewGuid().ToString();

    private IClientHost _clientHost;

    void Start()
    {
        _clientHost = Host
            .CreateClientBuilder()
            .Configure(cfg =>
            {
                cfg.ServerHost = "0.0.0.0";
                cfg.Serializer = new Serializer();
                cfg.ServerInputPorts = new[] { 7000, 7001 };
                cfg.ServerOutputPorts = new[] { 8000, 8001 };
                cfg.Receivers = 2;
                cfg.Senders = 2;
            })
            .Build();

        _clientHost.OnEvent<JoinedEvent>(
            handler: (joined) =>
            {
                Debug.Log("on join...");
                Debug.Log($"Player - {joined.Nickname} joined!");
            },
            hubId: 0,
            rpcId: 0);

        _clientHost.OnEvent<MoveEvent>(
            handler: (move) =>
            {
                Debug.Log("on move...");

                _moves[move.PlayerId].ApplyMove(move);
            },
            hubId: 0,
            rpcId: 2);

        _clientHost.OnEvent<SpawnedEvent>(
            handler: (spawned) =>
            {
                Log.Logger.Debug("spawned log - {@spawned}", spawned);

                if (spawned.Nickname == myNickName)
                {
                    Debug.Log("spawn me");

                    _players[spawned.PlayerId] = Instantiate(
                        original: myPlayerPrefab,
                        position: spawned.Position,
                        rotation: Quaternion.identity);

                    // TODO di
                    _players[spawned.PlayerId].GetComponent<MovePlayer>().ClientHost = _clientHost;
                    _players[spawned.PlayerId].GetComponent<MovePlayer>().PlayerId = spawned.PlayerId;
                }
                else
                {
                    Debug.Log("spawn other");

                    _players[spawned.PlayerId] = Instantiate(
                        original: playerPrefab,
                        position: spawned.Position,
                        rotation: Quaternion.identity);

                    _moves[spawned.PlayerId] = _players[spawned.PlayerId].GetComponent<SyncPlayer>();
                }

                Debug.Log("end spawn..");
            },
            hubId: 0,
            rpcId: 1);

        Task.Run(() => _clientHost.RunAsync());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("spawn...");
            _clientHost.Publish(
                @event: new SpawnEvent
                {
                    RoomId = 0,
                    Nickname = myNickName,
                },
                hubId: 0,
                rpcId: 1,
                udpMode: UdpMode.Udp);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("join...");
            _clientHost.Publish(
                @event: new JoinEvent
                {
                    RoomId = 0,
                    Nickname = myNickName,
                },
                hubId: 0,
                rpcId: 0,
                udpMode: UdpMode.Udp);
        }
    }
}
