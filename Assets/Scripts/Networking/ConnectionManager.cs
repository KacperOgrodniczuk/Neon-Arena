using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance;

    [SerializeField] private NetworkObject lobbyManagerPrefab;
    private LobbyManager lobbyManager;

    private NetworkManager networkManager;

    public void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        networkManager = InstanceFinder.NetworkManager;

        // Callback for remote connections to the server
        networkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;

        // Callback for client side e.g. loading the main menu scene since you've left the lobby.
        networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;

        // Callback for spawning in player objects for clients (remote and host) based on scene load.
        networkManager.SceneManager.OnClientLoadedStartScenes += OnClientLoadedStartScenes;
    }

    public void StartHost()
    {
        networkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
        
        StartServer();
        StartClient();
    }

    void OnServerConnectionState(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;

            // Spawn lobby manager before we switch scenes to prevent race conditions.
            NetworkObject lobbyManagerObj = networkManager.GetPooledInstantiated(lobbyManagerPrefab, true);
            networkManager.ServerManager.Spawn(lobbyManagerObj);
            lobbyManager = lobbyManagerObj.GetComponent<LobbyManager>();

            SceneLoadData sceneLoadData = new SceneLoadData("LobbyScene");
            sceneLoadData.ReplaceScenes = ReplaceOption.All;
            networkManager.SceneManager.LoadGlobalScenes(sceneLoadData);
        }
    }

    public void StartServer()
    {
        networkManager.ServerManager.StartConnection();
    }

    public void StartClient()
    {
        networkManager.ClientManager.StartConnection();
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
                lobbyManager.RemovePlayerFromLobby(obj);
                break;
        }
    }

    private void OnClientLoadedStartScenes(NetworkConnection connection, bool asServer)
    {
        if (!asServer)
            return;

        // Spawn player only after the client has loaded the scene
        if (lobbyManager == null)
            Debug.LogError("Missing Lobby Manager Reference. This could be due to one not existing yet");
        else
        {
            lobbyManager.SpawnPlayer(connection);

            NetworkObject playerObj = connection.FirstObject;
            lobbyManager.AddPlayerToLobby(playerObj);
        }
    }

    public void StopConnection()
    {
        // If server, stop the server connection
        if (networkManager.IsServer)
        {
            networkManager.ServerManager.StopConnection(true);
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
        }
        // if client, stop the client connection
        else if (networkManager.IsClient)
        {
            networkManager.ClientManager.StopConnection();
        }
    }

    private void OnDestroy()
    {
        networkManager.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
        networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
        networkManager.SceneManager.OnClientLoadedStartScenes -= OnClientLoadedStartScenes;

        networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
    }
}