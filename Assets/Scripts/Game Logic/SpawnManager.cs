using FishNet.Object;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float playerCheckDistance = 10f;
    [SerializeField] private LayerMask playersMask = -1;

    private struct ScoredSpawnPoint
    {
        public Transform spawnPoint;
        public float score;

        public ScoredSpawnPoint(Transform t)
        { 
            spawnPoint = t;
            score = 0;
        }
    }

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

    public void FindSpawnPoints()
    {
        // Auto-find spawn points in scene
        GameObject[] spawnObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");
        spawnPoints = new Transform[spawnObjects.Length];

        for (int i = 0; i < spawnObjects.Length; i++)
        {
            spawnPoints[i] = spawnObjects[i].transform;
        }

        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points found! Make sure to tag spawn points with 'SpawnPoint' tag.");
        }
    }

    public Transform GetLeastBusySpawnPoint(NetworkObject excludePlayer = null)
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points available! Using world origin.");
        }

        List<ScoredSpawnPoint> scoredSpawnPoints = new List<ScoredSpawnPoint>();

        foreach (Transform spawnPoint in spawnPoints)
        {
            scoredSpawnPoints.Add(CalculateSpawnScore(spawnPoint));
        }

        // Find the lowest score in the list.
        float minScore = scoredSpawnPoints.Min(scoredSpawnPoint => scoredSpawnPoint.score);

        // Lowest being the best since these are the least occupied. Where score values are similar to the lowest value add them to a list together.
        List<ScoredSpawnPoint> lowestScoredSpawnPoints = scoredSpawnPoints.Where(scoredSpawnPoint => Mathf.Approximately(scoredSpawnPoint.score, minScore)).ToList();

        Transform bestSpawnPoint;

        // Return random best spawn point, if there is only one with the lowest score then it will be returned either way.
        if (lowestScoredSpawnPoints.Count > 0)
        {
            int randomIndex = Random.Range(0, lowestScoredSpawnPoints.Count);
            bestSpawnPoint = lowestScoredSpawnPoints[randomIndex].spawnPoint;

            return bestSpawnPoint;
        }

        // This should never run since the list would have to be empty for this.

        return lowestScoredSpawnPoints[0].spawnPoint; ;
    }

    
    // Use a score system to evaluate how "busy" the spawn point is with other nearby players
    ScoredSpawnPoint CalculateSpawnScore(Transform spawnPoint)
    {
        // Check if there's something blocking the spawn point
        Collider[] nearbyPlayers = Physics.OverlapSphere(spawnPoint.position, playerCheckDistance, playersMask);

        float score = 0;

        if (nearbyPlayers.Length > 0)
        {
            foreach (Collider c in nearbyPlayers)
            { 
                float distance = Vector3.Distance(spawnPoint.position, c.transform.position);
                float proximityScore = playerCheckDistance - distance;

                if (distance > 0)
                {
                    score += proximityScore;
                }
            }
        }

        ScoredSpawnPoint scoredSpawnPoint = new ScoredSpawnPoint();
        scoredSpawnPoint.spawnPoint = spawnPoint;
        scoredSpawnPoint.score = score;

        return scoredSpawnPoint;
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
            Gizmos.DrawWireSphere(spawn.position, playerCheckDistance);

            Vector3 forwardPoint = spawn.position + spawn.transform.forward;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(spawn.position, forwardPoint);
        }
    }
}