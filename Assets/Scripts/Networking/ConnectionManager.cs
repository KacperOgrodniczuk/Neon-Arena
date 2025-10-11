using FishNet;
using FishNet.Managing;
using FishNet.Managing.Client;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    private const string PlayerNamePrefsKey = "PlayerName";

    public void StartHost()
    {
        StartServer();
        StartClient();

        InstanceFinder.NetworkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
    }

    public void StartServer()
    {
        InstanceFinder.ServerManager.StartConnection();
    }

    public void StartClient()
    {
        InstanceFinder.ClientManager.StartConnection();
    }

    public void SetIpAddress(string text)
    {
        InstanceFinder.TransportManager.Transport.SetClientAddress(text);
    }

    void OnServerConnectionState(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            InstanceFinder.NetworkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;

            SceneLoadData sceneLoadData = new SceneLoadData("LobbyScene");
            sceneLoadData.ReplaceScenes = ReplaceOption.All;
            InstanceFinder.NetworkManager.SceneManager.LoadGlobalScenes(sceneLoadData);
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