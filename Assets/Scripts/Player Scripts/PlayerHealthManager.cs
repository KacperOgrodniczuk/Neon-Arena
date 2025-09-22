using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using UnityEngine;

public class PlayerHealthManager : NetworkBehaviour
{
    public PlayerManager playerManager { get; private set; }

    public float maxHealth = 100f;

    public readonly SyncVar<float> currentHealth = new SyncVar<float>();
    public readonly SyncVar<bool> isDead = new SyncVar<bool>(false);

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();

        currentHealth.Value = maxHealth;

        currentHealth.OnChange += OnHealthChange;
    }

    // Damage is server validated and therefore should only be called on the server.
    [Server]
    public void TakeDamage(float amount)
    {
        if (isDead.Value) return;

        currentHealth.Value -= amount;

        currentHealth.Value = Mathf.Max(currentHealth.Value, 0);

        if (currentHealth.Value <= 0f)
        {
            Die();
        }
    }

    [Server]
    void Die()
    {
        // Only ever runs on the server since TakeDamage() above can't be called by clients.
        isDead.Value = true;

        DisablePlayerComponentsRPC();

        StartCoroutine(RespawnTimer());

        //Track kills for scoring
    }

    [ObserversRpc]
    void DisablePlayerComponentsRPC()
    {
        // Prevent the player from moving or shooting when dead
        playerManager.locomotionManager.enabled = false;
        playerManager.shootingManager.enabled = false;

        // Play death effects

        // Hide the player's model and collider
        GetComponent<Collider>().enabled = false;
        GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
    }

    [Server]
    IEnumerator RespawnTimer()
    {
        yield return new WaitForSeconds(3f); // Respawn after 3 seconds
        RespawnPlayer();
    }

    [Server]
    void RespawnPlayer()
    {
        // Reset health
        currentHealth.Value = maxHealth;
        isDead.Value = false;
        
        Transform spawnPoint = SpawnManager.Instance.GetBestSpawnPoint(this.NetworkObject);

        RespawnPlayerRPC(spawnPoint.position, spawnPoint.rotation);
    }

    [ObserversRpc]
    void RespawnPlayerRPC(Vector3 position, Quaternion rotation)
    {
        // Move player to a spawn point
        transform.position = position;
        transform.rotation = rotation;

        // Re-enable player movement and shooting
        playerManager.locomotionManager.enabled = true;
        playerManager.shootingManager.enabled = true;

        // Show the player's model and collider
        GetComponent<Collider>().enabled = true;
        GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
    }

    void OnHealthChange(float previousValue, float nextValue, bool asServer)
    {
        // Play flinch animation
        // Update any ui
    }
}
