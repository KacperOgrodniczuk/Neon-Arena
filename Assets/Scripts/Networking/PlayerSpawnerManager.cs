using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

public class PlayerSpawnerManager : MonoBehaviour // Changed to MonoBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;

    private void Awake()
    {
        // Subscribe to connection events when the script is created
        InstanceFinder.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
        if (InstanceFinder.SceneManager != null)
        {
            InstanceFinder.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe when the script is destroyed
        if (InstanceFinder.ServerManager != null)
        {
            InstanceFinder.ServerManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;
        }
        if (InstanceFinder.SceneManager != null)
        {
            InstanceFinder.SceneManager.OnClientLoadedStartScenes -= SceneManager_OnClientLoadedStartScenes;
        }
    }

    private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs args)
    {
        // This is where you might handle logic when the server starts/stops, 
        // but for spawning, we typically use OnClientLoadedStartScenes
    }


    void SceneManager_OnClientLoadedStartScenes(NetworkConnection connection, bool asServer)
    {
        if (asServer)
        {
            SpawnPlayer(connection);
        }
    }

    private void SpawnPlayer(NetworkConnection connection)
    {
        NetworkObject obj = InstanceFinder.NetworkManager.GetPooledInstantiated(playerPrefab, true);

        // Spawn it on the server, assign ownership to the new connection, and add it to the scene
        InstanceFinder.ServerManager.Spawn(obj, connection, gameObject.scene);

        InstanceFinder.SceneManager.AddConnectionToScene(connection, gameObject.scene);
    }
}