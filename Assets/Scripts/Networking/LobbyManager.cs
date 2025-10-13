using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;

using UnityEngine;

using System.Linq;
using FishNet.Managing;

public class LobbyManager : NetworkBehaviour
{
    private NetworkManager networkManager;
    [SerializeField] private NetworkObject playerPrefab;

    private readonly SyncList<PlayerLobbyManager> playerList = new SyncList<PlayerLobbyManager>();

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        networkManager = InstanceFinder.NetworkManager;

        networkManager.SceneManager.OnClientLoadedStartScenes += OnClientLoadedStartScenes;
        // Callback for client side cleanup e.g. loading the main menu scene since you've left the lobby.
        networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
        // Callback for server side cleanup e.g. unloading the global scene.
        networkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
        
        playerList.OnChange += OnPlayerListChange;
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();

        networkManager.SceneManager.OnClientLoadedStartScenes -= OnClientLoadedStartScenes;
        networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
        networkManager.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;

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
        networkManager.SceneManager.AddConnectionToScene(connection, gameObject.scene);

        NetworkObject obj = NetworkManager.GetPooledInstantiated(playerPrefab, true);
        // Set player state to lobby
        obj.GetComponent<PlayerManager>().stateManager.playerState.Value = PlayerStateManager.PlayerState.Lobby;

        // Spawn it on the server, assign ownership to the new connection, and add it to the scene
        networkManager.ServerManager.Spawn(obj, connection, gameObject.scene);

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
}
