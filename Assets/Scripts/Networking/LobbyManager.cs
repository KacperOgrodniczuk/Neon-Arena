using FishNet;
using FishNet.Connection;

using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;

    private List<PlayerLobbyManager> playerList = new List<PlayerLobbyManager>();

    public List<PlayerLobbyManager> PlayerList => playerList;

    public void Awake()
    {
        InstanceFinder.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;

        InstanceFinder.SceneManager.OnClientLoadedStartScenes += OnClientLoadedStartScenes;
    }

    private void OnRemoteConnectionState(NetworkConnection conn, FishNet.Transporting.RemoteConnectionStateArgs args)
    {
        // We only care about new connections starting
        if (args.ConnectionState == FishNet.Transporting.RemoteConnectionState.Started)
        {
            // Note: We don't spawn immediately here anymore
            // Instead, we wait for OnClientLoadedStartScenes
            // Add connection to scene immediately so they can receive scene objects
            SceneManager.AddConnectionToScene(conn, gameObject.scene);
        }
    }

    private void OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
    {
        if (!asServer)
            return;

        // Spawn player only after the client has loaded the scene
        SpawnPlayer(conn);
    }

    private void SpawnPlayer(NetworkConnection connection)
    {
        NetworkObject obj = NetworkManager.GetPooledInstantiated(playerPrefab, true);
        // Set player state to lobby
        obj.GetComponent<PlayerManager>().stateManager.playerState.Value = PlayerStateManager.PlayerState.Lobby;

        // Spawn it on the server, assign ownership to the new connection, and add it to the scene
        ServerManager.Spawn(obj, connection, gameObject.scene);

        // Ensure the connection is associated with the current scene
        SceneManager.AddConnectionToScene(connection, gameObject.scene);

        // Add the new player to the player list
        playerList.Add(obj.GetComponent<PlayerLobbyManager>());

        //Subscribe to name change events
        obj.GetComponent<PlayerLobbyManager>().SubscribeToNameChange(OnAnyPlayerNameChanged);
    }

    private void OnAnyPlayerNameChanged(string oldValue, string newValue, bool asServer)
    { 
        LobbyUIManager.Instance.UpdatePlayerListUI(playerList);
    }

    // Check wether all players are ready
    // Transition to the game scene when the host starts the game
}
