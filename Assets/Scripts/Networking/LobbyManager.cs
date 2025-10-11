using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;

using UnityEngine;

using System.Linq;
using UnityEngine.SceneManagement;
using NUnit.Framework.Constraints;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;

    private readonly SyncList<PlayerLobbyManager> playerList = new SyncList<PlayerLobbyManager>();

    void Awake()
    {
        InstanceFinder.SceneManager.OnClientLoadedStartScenes += OnClientLoadedStartScenes;
        
        // Callback for client side cleanup e.g. loading the main menu scene since you've left the lobby.
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;

        // Callback for server side cleanup e.g. unloading the global scene.
        InstanceFinder.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
        
        playerList.OnChange += OnPlayerListChange;
    }

    void OnDestroy()
    {
        InstanceFinder.SceneManager.OnClientLoadedStartScenes -= OnClientLoadedStartScenes;
        InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionState;
        InstanceFinder.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;

        playerList.OnChange -= OnPlayerListChange;

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
        // Ensure the connection is associated with the current scene
        InstanceFinder.SceneManager.AddConnectionToScene(connection, gameObject.scene);

        NetworkObject obj = NetworkManager.GetPooledInstantiated(playerPrefab, true);
        // Set player state to lobby
        obj.GetComponent<PlayerManager>().stateManager.playerState.Value = PlayerStateManager.PlayerState.Lobby;

        // Spawn it on the server, assign ownership to the new connection, and add it to the scene
        InstanceFinder.ServerManager.Spawn(obj, connection, gameObject.scene);

        // Add the new player to the player list
        playerList.Add(obj.GetComponent<PlayerLobbyManager>());

        //Subscribe to name change events
        obj.GetComponent<PlayerLobbyManager>().SubscribeToNameChange(OnAnyPlayerNameChanged);

        //Send an observerRPC to inform clients they should also subscribe to the name change event
        SubscriveToNameChangeObserverRpc(obj);
    }

    [ObserversRpc]
    private void SubscriveToNameChangeObserverRpc(NetworkObject obj)
    {
        obj.GetComponent<PlayerLobbyManager>().SubscribeToNameChange(OnAnyPlayerNameChanged);
    }

    [ObserversRpc]
    private void UnsubscribeFromNameChangeObserverRpc(NetworkObject obj)
    {
        obj.GetComponent<PlayerLobbyManager>().UnsubscribeFromNameChange(OnAnyPlayerNameChanged);
    }

    private void OnAnyPlayerNameChanged(string oldValue, string newValue, bool asServer)
    { 
        LobbyUIManager.Instance.UpdatePlayerListUI(playerList.ToList());
    }

    private void OnPlayerListChange(SyncListOperation operation, int index, PlayerLobbyManager oldItem, PlayerLobbyManager newItem, bool asServer)
    {
        LobbyUIManager.Instance.UpdatePlayerListUI(playerList.ToList());
    }

    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        switch (args.ConnectionState)
        {
            case LocalConnectionState.Stopped:
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
                break;
        }
    }

    private void OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
    {
        switch (args.ConnectionState)
        {
            case RemoteConnectionState.Stopped:
                NetworkObject obj = connection.FirstObject;
                playerList.Remove(obj.GetComponent<PlayerLobbyManager>());
                UnsubscribeFromNameChangeObserverRpc(obj);

                break;
        }
    }

    // Check wether all players are ready

    // Transition to the game scene when the host starts the game

    /// <summary>
    /// Stops connections when quitting the lobby. Cleanup and transition logic is handled in OnClientConnectionState function subscvribed to an event with the same name.
    /// </summary>
    public void QuitLobby()
    {
        // If server, stop the server connection
        if (IsServerInitialized)
        {
            ServerManager.StopConnection(true);
        }
        // if client, stop the client connection
        else if (IsClientInitialized)
        {
            ClientManager.StopConnection();
        }
    }
}
