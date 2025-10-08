using FishNet;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class PlayerSpawnerManager : MonoBehaviour // Changed to MonoBehaviour
{
    // TODO:
    // This script will either need to be drastically changed or deleted, since lobby manager is dealing with a lot of this
    // Will need to use proper game state manager to handle all scene transitions.

    [SerializeField] private NetworkObject playerPrefab;

    private void Awake()
    {
        // Subscribe to connection events when the script is created
        InstanceFinder.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
    }

    private void OnDestroy()
    {
        InstanceFinder.SceneManager.OnClientLoadedStartScenes -= SceneManager_OnClientLoadedStartScenes;
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