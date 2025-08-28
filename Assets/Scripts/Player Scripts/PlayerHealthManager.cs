using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerHealthManager : NetworkBehaviour
{
    public readonly SyncVar<float> currentHealth = new SyncVar<float>();
    public float maxHealth = 100f;
    public float currentHealthDebug;

    private void Awake()
    {
        currentHealth.Value = maxHealth;
        currentHealthDebug = currentHealth.Value;

        currentHealth.OnChange += OnHealthChange;
    }

    // Damage is server validated and therefore should only be called on the server.
    [Server]
    public void TakeDamage(float amount)
    {
        currentHealth.Value -= amount;

        currentHealth.Value = Mathf.Max(currentHealth.Value, 0);

        if (currentHealth.Value <= 0f)
        {
            Die();
        }
    }

    void OnHealthChange(float previousValue, float nextValue, bool asServer) 
    {
        currentHealthDebug = currentHealth.Value;
        // Play flinch animation
        // Update any ui
    }

    void Die()
    {
        Debug.Log(gameObject.name + " died");
    }
}
