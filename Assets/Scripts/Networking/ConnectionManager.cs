using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    [SerializeField] NetworkManager networkManager;

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

        networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
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

            SceneLoadData sceneLoadData = new SceneLoadData("GameScene");
            sceneLoadData.ReplaceScenes = ReplaceOption.All;
            networkManager.SceneManager.LoadGlobalScenes(sceneLoadData);
        }
    }

    void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;

            // If conneting on a client unload the scene locally.
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("MainMenuScene");
        }
    }
}