using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using UnityEngine;
using System.Linq;
using FishNet.Managing;
using System;
using FishNet.Managing.Scened;
using UnityEngine.SceneManagement;

/// <summary>
/// The term lobby is used to represent the list of players within the given game. This lobby class keeps track of all players within the lobby and game scene screen, and persists accross both of the scenes.
/// </summary>
public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;

    private NetworkManager networkManager;
    [SerializeField] private NetworkObject playerPrefab;

    public readonly SyncList<PlayerInfo> playerList = new SyncList<PlayerInfo>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        networkManager = InstanceFinder.NetworkManager;
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
    }

    public void SpawnPlayer(NetworkConnection connection)
    {
        // Ensure the connection is associated with the current scene
        //networkManager.SceneManager.AddConnectionToScene(connection, gameObject.scene);

        NetworkObject obj = NetworkManager.GetPooledInstantiated(playerPrefab, true);

        // Set player state to lobby
        obj.GetComponent<PlayerManager>().stateManager.playerState.Value = PlayerStateManager.PlayerState.Lobby;

        // Spawn it on the server, assign ownership to the new connection, and add it to the scene
        networkManager.ServerManager.Spawn(obj, connection, gameObject.scene);
        Debug.Log("new object assigned to connection " + connection);
    }

    public void AddPlayerToLobby(NetworkObject playerObj)
    {
        // Add the new player to the player list
        playerList.Add(playerObj.GetComponent<PlayerInfo>());
    }

    public void RemovePlayerFromLobby(NetworkObject playerObj)
    {
        playerList.Remove(playerObj.GetComponent<PlayerInfo>());
    }

    [ObserversRpc]
    public void FadeInOnAllClients()
    {
        TransitionManager.Instance.FadeIn();
    }
}
