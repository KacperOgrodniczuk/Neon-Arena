using FishNet.Object;
using System.Diagnostics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerShootingManager : NetworkBehaviour
{
    private PlayerManager playerManager;

    public Transform projectileSpawn;
    public GameObject projectilePrefab;

    [Header("Attack Stats")]
    public float fireRate = 0.33f;
    public float projectileDamage = 10f;
    public float projectileSpeed = 20f;

    private float nextShootTime;
    private const float maxPassedTime = 0.3f;

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        HandleShooting();
        HandleAiming();
    }

    void HandleShooting()
    {
        // Can only shoot if aiming
        if (!playerManager.isAiming)
            return;

        if (PlayerInputManager.Instance.attackInput && Time.time >= nextShootTime)
        {
            Vector3 targetPoint = CameraManager.Instance.GetAimTargetPoint();
            Vector3 targetDirection = (targetPoint - projectileSpawn.position).normalized;

            // Tell the server to spawn projectiles on other machines.
            SpawnServerProjectile(projectileSpawn.position, targetDirection, base.TimeManager.Tick);

            nextShootTime = Time.time + fireRate;
        }
    }

    // Spawn projectile Locally
    void SpawnLocalProjectile(Vector3 startPosition, Vector3 shootDirection, float passedTime)
    {
        GameObject localProjectile = Instantiate(projectilePrefab.gameObject, projectileSpawn.position, Quaternion.identity);
        localProjectile.GetComponent<Projectile>().ShootProjectile(shootDirection, projectileSpeed, projectileDamage);
    }

    // Client sending code to run on the server
    [ServerRpc]
    void SpawnServerProjectile(Vector3 startPosition, Vector3 shootDirection, uint tick)
    {
        float passedTime = (float)base.TimeManager.TimePassed(tick, false);

        passedTime = Mathf.Min(maxPassedTime / 2f, passedTime);

        // Tell other clients to spawn the projectil. 
        SpawnObserversProjectile(startPosition, shootDirection, tick);
    }

    // Server sending code to run on clients
    [ObserversRpc]
    void SpawnObserversProjectile(Vector3 startPosition, Vector3 shootDirection, uint tick)
    {
        float passedTime = (float)base.TimeManager.TimePassed(tick, false);
        passedTime = Mathf.Min(maxPassedTime, passedTime);

        SpawnLocalProjectile(startPosition, shootDirection, passedTime);
    }

    void HandleAiming()
    {
        playerManager.isAiming = PlayerInputManager.Instance.aimInput;
    }
}
