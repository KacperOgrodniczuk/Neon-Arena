using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

public class PlayerSpawnerManager : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;

    public override void OnStartServer()
    {
        SceneManager.OnClientLoadedStartScenes += OnClientLoadedStartScenes;
    }

    public override void OnStopServer()
    {
        SceneManager.OnClientLoadedStartScenes -= OnClientLoadedStartScenes;
    }

    public override void OnSpawnServer(NetworkConnection connection)
    {
        if (connection.LoadedStartScenes(true))
            SpawnPlayer(connection);
    }

    void OnClientLoadedStartScenes(NetworkConnection connection, bool asServer)
    {
        if (asServer && Observers.Contains(connection))
            SpawnPlayer(connection);
    }

    private void SpawnPlayer(NetworkConnection connection)
    {
        NetworkObject obj = NetworkManager.GetPooledInstantiated(playerPrefab, asServer: true);
        Spawn(obj, connection, gameObject.scene);
    }
}
