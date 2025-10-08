using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;

    private List<PlayerLobbyManager> playerList = new List<PlayerLobbyManager>();

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (IsServer)
        {
            ServerManager.OnRemoteConnectionState += OnRemoteConnectionStateChange;

            ClientManager.Connection.OnLoadedStartScenes += OnClientLoadedStartScenes;
        }
    }

    void OnClientLoadedStartScenes(NetworkConnection connection, bool asServer)
    {
        ClientManager.Connection.OnLoadedStartScenes -= OnClientLoadedStartScenes;

        // Spawn player object for the connected client
        SpawnPlayer(connection);
    }

    // Handle remote players connecting to the lobby
    void OnRemoteConnectionStateChange(NetworkConnection connection, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            // Spawn player object for the connected client
            SpawnPlayer(connection);
        }
    }

    private void SpawnPlayer(NetworkConnection connection)
    {
        NetworkObject obj = InstanceFinder.NetworkManager.GetPooledInstantiated(playerPrefab, true);
        // Set player state to lobby
        obj.GetComponent<PlayerManager>().stateManager.playerState.Value = PlayerStateManager.PlayerState.Lobby;

        // Spawn it on the server, assign ownership to the new connection, and add it to the scene
        InstanceFinder.ServerManager.Spawn(obj, connection, gameObject.scene);

        // Ensure the connection is associated with the current scene
        InstanceFinder.SceneManager.AddConnectionToScene(connection, gameObject.scene);

        // Add the new player to the player list
        playerList.Add(obj.GetComponent<PlayerLobbyManager>());

        // Update UI to show the new player in the lobby
        LobbyUIManager.Instance.UpdatePlayerListUI(playerList);
    }


    // Check wether all players are ready
    // Transition to the game scene when the host starts the game
}
