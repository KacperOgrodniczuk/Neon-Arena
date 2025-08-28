using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionManager : MonoBehaviour
{
    [SerializeField] NetworkManager networkManager;

    private void OnEnable()
    {
        networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
    }

    private void OnDisable()
    {
        networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
    }

    public void StartHost()
    {
        StartServer();
        StartClient();
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

    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        switch (args.ConnectionState)
        {
            case LocalConnectionState.Started:
                //udpate connection state
                SceneManager.LoadScene("GameScene");
                break;

            case LocalConnectionState.Stopped:
                Debug.Log("Failed to connect");
                break;
        }
    }
}
