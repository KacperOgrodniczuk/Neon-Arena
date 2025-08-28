using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class PlayerSpawnerManager : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;

    public override void OnSpawnServer(NetworkConnection connection)
    {
        NetworkObject networkObject = NetworkManager.GetPooledInstantiated(playerPrefab, asServer: true);
        Spawn(networkObject, connection, gameObject.scene);
    }
}
