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

        // Update UI to show the new player in the lobby
        LobbyUIManager.Instance.UpdatePlayerListUI(playerList);
    }

    private void SpawnPlayer(NetworkConnection connection)
    {
        NetworkObject obj = InstanceFinder.NetworkManager.GetPooledInstantiated(playerPrefab, true);

        // Spawn it on the server, assign ownership to the new connection, and add it to the scene
        InstanceFinder.ServerManager.Spawn(obj, connection, gameObject.scene);

        // Ensure the connection is associated with the current scene
        InstanceFinder.SceneManager.AddConnectionToScene(connection, gameObject.scene);

        // Set player state to lobby
        obj.GetComponent<PlayerManager>().stateManager.playerState.Value = PlayerStateManager.PlayerState.Lobby;

        // Add the new player to the player list
        playerList.Add(obj.GetComponent<PlayerLobbyManager>());
    }

    // Handle UI cleanup if players disconnect
    // Check wether all players are ready
    // Transition to the game scene when the host starts the game
}
