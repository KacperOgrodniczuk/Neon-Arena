using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    // AI generated, need to rework this, not a fan of how it currently works.
    public static SpawnManager Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float minDistanceBetweenPlayers = 5f;
    [SerializeField] private float spawnProtectionTime = 3f;
    [SerializeField] private LayerMask playersMask = -1;

    private Dictionary<Transform, float> spawnPointLastUsed = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        FindSpawnPoints();
    }

    void FindSpawnPoints()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            // Auto-find spawn points in scene
            GameObject[] spawnObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");
            spawnPoints = new Transform[spawnObjects.Length];

            for (int i = 0; i < spawnObjects.Length; i++)
            {
                spawnPoints[i] = spawnObjects[i].transform;
            }
        }

        // Initialize timing dictionary
        foreach (Transform spawn in spawnPoints)
        {
            spawnPointLastUsed[spawn] = -spawnProtectionTime; // Allow immediate use
        }

        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points found! Make sure to tag spawn points with 'SpawnPoint' tag.");
        }
    }

    public Transform GetBestSpawnPoint(NetworkObject excludePlayer = null)
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points available! Using world origin.");
        }

        Transform bestSpawn = null;
        float bestScore = float.MinValue;

        foreach (Transform spawnPoint in spawnPoints)
        {
            float score = EvaluateSpawnPoint(spawnPoint, excludePlayer);

            if (score > bestScore)
            {
                bestScore = score;
                bestSpawn = spawnPoint;
            }
        }

        // Mark this spawn as recently used
        if (bestSpawn != null)
        {
            spawnPointLastUsed[bestSpawn] = Time.time;
        }

        return bestSpawn != null ? bestSpawn : spawnPoints[0];
    }

    float EvaluateSpawnPoint(Transform spawnPoint, NetworkObject excludePlayer)
    {
        float score = 100f; // Base score

        // Check if spawn point is blocked by another player.
        if (IsSpawnBlocked(spawnPoint.position))
        {
            return float.MinValue; // Never use blocked spawns
        }

        // Penalty for recently used spawn points
        float timeSinceUsed = Time.time - spawnPointLastUsed[spawnPoint];
        if (timeSinceUsed < spawnProtectionTime)
        {
            score -= 50f * (spawnProtectionTime - timeSinceUsed);
        }

        // Penalty for nearby players
        Collider[] nearbyPlayers = Physics.OverlapSphere(spawnPoint.position, minDistanceBetweenPlayers);
        foreach (Collider col in nearbyPlayers)
        {
            NetworkObject netObj = col.GetComponent<NetworkObject>();
            if (netObj != null && netObj != excludePlayer && netObj.GetComponent<PlayerHealthManager>() != null)
            {
                float distance = Vector3.Distance(spawnPoint.position, col.transform.position);
                score -= 30f * (minDistanceBetweenPlayers - distance) / minDistanceBetweenPlayers;
            }
        }

        return score;
    }

    bool IsSpawnBlocked(Vector3 position)
    {
        // Check if there's something blocking the spawn point
        Collider[] nearbyPlayers = Physics.OverlapSphere(position, minDistanceBetweenPlayers, playersMask);
        return nearbyPlayers.Length > 0;
    }

    // Visualise spawn points in scene.
    void OnDrawGizmos()
    {
        if (spawnPoints == null) return;

        foreach (Transform spawn in spawnPoints)
        {
            if (spawn == null) continue;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawn.position, 1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(spawn.position, minDistanceBetweenPlayers);
        }
    }
}