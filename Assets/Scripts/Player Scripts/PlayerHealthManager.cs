using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Connection;
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
    public void TakeDamage(float amount, NetworkObject damageDealer)
    {
        if (isDead.Value) return;

        currentHealth.Value -= amount;

        currentHealth.Value = Mathf.Max(currentHealth.Value, 0);

        ConfirmHitTargetRPC(damageDealer.Owner);

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

        // Set player state to dead in Player State Manager to disable scripts e.g. movement, shooting, etc.
        playerManager.stateManager.playerState.Value = PlayerStateManager.PlayerState.Dead;

        PlayDeathEffects();

        StartCoroutine(RespawnTimer());

        //Track kills for scoring
    }

    [ObserversRpc]
    void PlayDeathEffects()
    {
        // Play death effects
        ParticleEffectManager.Instance.PlayEffect("PlayerDeath", transform.position, Quaternion.identity);
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

        // Set State To Alive in Player State Manager to enable scripts.
        playerManager.stateManager.playerState.Value = PlayerStateManager.PlayerState.Alive;
    }

    [ObserversRpc]
    void RespawnPlayerRPC(Vector3 position, Quaternion rotation)
    {
        // Move player to a spawn point
        transform.position = position;
        transform.rotation = rotation;

        // Show the player's model and collider
        GetComponent<Collider>().enabled = true;
        GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
    }

    [TargetRpc]
    void ConfirmHitTargetRPC(NetworkConnection target)
    {
        // Call UI function to show a hit marker
        HUDManager.Instance.TriggerHitMarkerEffect();
    }

    void OnHealthChange(float previousValue, float nextValue, bool asServer)
    {
        // Play flinch animation
        // Update any ui
        // Play flash effect
        if (IsOwner)
        {
            if (nextValue < previousValue)
            {
                OutlineFlashEffectManager.Instance.TriggerOutlineEffect(
                    OutlineFlashEffectManager.Instance.colourPallete.TakeDamageColour, 
                    OutlineFlashEffectManager.Instance.defaultFlashDuration);
            }
        }
    }
}
