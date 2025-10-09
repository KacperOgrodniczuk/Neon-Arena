using FishNet.Managing;
using FishNet.Managing.Client;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    [SerializeField] NetworkManager networkManager;

    private const string PlayerNamePrefsKey = "PlayerName";

    public void StartHost()
    {
        StartServer();
        StartClient();

        networkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
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

    void OnServerConnectionState(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;

            SceneLoadData sceneLoadData = new SceneLoadData("LobbyScene");
            sceneLoadData.ReplaceScenes = ReplaceOption.All;
            networkManager.SceneManager.LoadGlobalScenes(sceneLoadData);
        }
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
}