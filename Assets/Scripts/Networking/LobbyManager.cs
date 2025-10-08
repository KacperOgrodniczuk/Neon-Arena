using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;

    private List<PlayerLobbyManager> playerList;

    public override void OnStartServer()
    {
        base.OnStartServer();
        // Subscribe to connection events
        ServerManager.OnRemoteConnectionState += OnClientConnect;
    }

    // Handle new players connecting to the lobby
    void OnClientConnect(NetworkConnection connection, RemoteConnectionStateArgs args)
    {
        // Spawn player object for the connected client
        SpawnPlayer(connection);


        // add a player card for each connected player on the lobby UI

    }

    private void SpawnPlayer(NetworkConnection connection)
    {
        NetworkObject obj = InstanceFinder.NetworkManager.GetPooledInstantiated(playerPrefab, true);

        // Spawn it on the server, assign ownership to the new connection, and add it to the scene
        InstanceFinder.ServerManager.Spawn(obj, connection, gameObject.scene);

        // Ensure the connection is associated with the current scene
        InstanceFinder.SceneManager.AddConnectionToScene(connection, gameObject.scene);
    }

    // Handle UI cleanup if players disconnect
    // Check wether all players are ready
    // Transition to the game scene when the host starts the game
}
