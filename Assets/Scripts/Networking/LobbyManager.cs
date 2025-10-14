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
        
        playerList.OnChange += OnPlayerListChange;
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();

        playerList.OnChange -= OnPlayerListChange;
    }

    public void SpawnPlayer(NetworkConnection connection)
    {
        // Ensure the connection is associated with the current scene
        networkManager.SceneManager.AddConnectionToScene(connection, gameObject.scene);

        NetworkObject obj = NetworkManager.GetPooledInstantiated(playerPrefab, true);
        // Set player state to lobby
        obj.GetComponent<PlayerManager>().stateManager.playerState.Value = PlayerStateManager.PlayerState.Lobby;

        // Spawn it on the server, assign ownership to the new connection, and add it to the scene
        networkManager.ServerManager.Spawn(obj, connection, gameObject.scene);
    }

    public void AddPlayerToLobby(NetworkObject playerObj)
    {
        // Add the new player to the player list
        playerList.Add(playerObj.GetComponent<PlayerLobbyManager>());
        
        //Subscribe to name change events
        playerObj.GetComponent<PlayerLobbyManager>().SubscribeToNameChange(OnAnyPlayerNameChanged);

        //Send an observerRPC to inform clients they should also subscribe to the name change event
        SubscriveToNameChangeObserverRpc(playerObj);
    }

    public void RemovePlayerFromLobby(NetworkObject playerObj)
    {
        playerList.Remove(playerObj.GetComponent<PlayerLobbyManager>());
        UnsubscribeFromNameChangeObserverRpc(playerObj);
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
        LobbyUIManager.Instance?.UpdatePlayerListUI(playerList.ToList());
    }

    private void OnPlayerListChange(SyncListOperation operation, int index, PlayerLobbyManager oldItem, PlayerLobbyManager newItem, bool asServer)
    {
        LobbyUIManager.Instance?.UpdatePlayerListUI(playerList.ToList());
    }

    // Transition to the game scene when the host starts the game
}
