using FishNet;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    [SerializeField] private NetworkObject lobbyManagerPrefab;

    private NetworkManager networkManager;

    private const string PlayerNamePrefsKey = "PlayerName";

    public void Awake()
    {
        networkManager = InstanceFinder.NetworkManager;
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

    public void SetIpAddress(string text)
    {
        networkManager.TransportManager.Transport.SetClientAddress(text);
    }

    // TODO: Move to a dedicated PlayerPrefs manager class
    // Load currently saved player name to show in the input field (In case a player as set one during previous play sessions.)

    public void SetPlayerName(string name)
    {
        string nameToSave = name;

        // Simple validation: use a default if the field is empty
        if (string.IsNullOrEmpty(nameToSave))
        {
            nameToSave = $"Guest_{Random.Range(100, 999)}";
        }

        PlayerPrefs.SetString(PlayerNamePrefsKey, nameToSave);
        PlayerPrefs.Save();
        Debug.Log($"Player name set to: {nameToSave} and saved to PlayerPrefs");
    }
    private void OnDestroy()
    {
        networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
    }
}