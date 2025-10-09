using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Linq;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;

    private readonly SyncList<PlayerLobbyManager> playerList = new SyncList<PlayerLobbyManager>();

    void Awake()
    {
        InstanceFinder.SceneManager.OnClientLoadedStartScenes += OnClientLoadedStartScenes;

        playerList.OnChange += OnPlayerListChange;
    }

    void OnDestroy()
    {
        SceneManager.OnClientLoadedStartScenes -= OnClientLoadedStartScenes;

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
        SceneManager.AddConnectionToScene(connection, gameObject.scene);

        NetworkObject obj = NetworkManager.GetPooledInstantiated(playerPrefab, true);
        // Set player state to lobby
        obj.GetComponent<PlayerManager>().stateManager.playerState.Value = PlayerStateManager.PlayerState.Lobby;

        // Spawn it on the server, assign ownership to the new connection, and add it to the scene
        ServerManager.Spawn(obj, connection, gameObject.scene);

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

    private void OnAnyPlayerNameChanged(string oldValue, string newValue, bool asServer)
    { 
        LobbyUIManager.Instance.UpdatePlayerListUI(playerList.ToList());
    }

    private void OnPlayerListChange(SyncListOperation operation, int index, PlayerLobbyManager oldItem, PlayerLobbyManager newItem, bool asServer)
    {
        LobbyUIManager.Instance.UpdatePlayerListUI(playerList.ToList());
    }

    // Check wether all players are ready

    // Transition to the game scene when the host starts the game

    // Handle the players or host disconnecting from the lobby.

    public void QuitLobby()
    {
        if (IsServerInitialized)
        {
            ServerManager.StopConnection(true);
        }
    }
}
