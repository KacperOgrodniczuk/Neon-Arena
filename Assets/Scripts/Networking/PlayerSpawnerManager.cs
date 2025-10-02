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
        // This event fires on the server when a client finishes loading the initial scenes.
        // The 'asServer' check ensures this only runs if we are the server (which we are, 
        // but it's good practice for the NetworkManager.SceneManager event).
        if (asServer)
        {
            // You might need a check to prevent running on the host connection if you 
            // handle the host spawn separately, but for general use, this is where to spawn.
            SpawnPlayer(connection);
        }
    }

    private void SpawnPlayer(NetworkConnection connection)
    {
        // Instantiate the player object using FishNet's pooling
        NetworkObject obj = InstanceFinder.NetworkManager.GetPooledInstantiated(playerPrefab, true);

        // Spawn it on the server, assign ownership to the new connection, and add it to the scene
        InstanceFinder.ServerManager.Spawn(obj, connection, gameObject.scene);

        // This line is a common necessity if you aren't using the default PlayerSpawner
        // and aren't using global scenes, to ensure the new connection observes the scene
        // and its content (including the player and other players).
        InstanceFinder.SceneManager.AddConnectionToScene(connection, gameObject.scene);
    }
}