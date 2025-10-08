using FishNet.Object;
using UnityEngine;

public class PlayerShootingManager : NetworkBehaviour
{
    private PlayerManager playerManager;

    public Transform projectileSpawn;
    public GameObject projectilePrefab;

    public bool isAiming { get; private set; }

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
        if (!isAiming)
            return;

        if (PlayerInputManager.Instance.attackInput && Time.time >= nextShootTime)
        {
            Vector3 targetPoint = CameraManager.Instance.GetAimTargetPoint();
            Vector3 targetDirection = (targetPoint - projectileSpawn.position).normalized;

            // Spawn local projectile for the player that is shooting
            SpawnLocalProjectile(projectileSpawn.position, targetDirection, 0f);

            // Tell the server to spawn projectiles on other machines.
            SpawnServerProjectile(projectileSpawn.position, targetDirection, base.TimeManager.Tick);

            nextShootTime = Time.time + fireRate;
        }
    }

    // Spawn projectile Locally
    void SpawnLocalProjectile(Vector3 startPosition, Vector3 shootDirection, float passedTime)
    {
        GameObject localProjectile = ProjectilePool.Instance.GetProjectile(gameObject);
        Projectile projectile = localProjectile.GetComponent<Projectile>();
        projectile.SetOwnerAndIgnoreCollisions(gameObject);

        localProjectile.transform.position = startPosition;
        localProjectile.transform.rotation = Quaternion.LookRotation(shootDirection);
        localProjectile.SetActive(true);

        projectile.ShootProjectile(shootDirection, projectileSpeed, projectileDamage, passedTime);
    }

    // Client sending code to run on the server
    [ServerRpc]
    void SpawnServerProjectile(Vector3 startPosition, Vector3 shootDirection, uint tick)
    {
        // Tell other clients to spawn the projectil. 
        SpawnObserversProjectile(startPosition, shootDirection, tick);
    }

    // Server sending code to run on clients
    [ObserversRpc(ExcludeOwner = true)]
    void SpawnObserversProjectile(Vector3 startPosition, Vector3 shootDirection, uint tick)
    {
        float passedTime = (float)base.TimeManager.TimePassed(tick, false);
        passedTime = Mathf.Min(maxPassedTime, passedTime);

        SpawnLocalProjectile(startPosition, shootDirection, passedTime);
    }

    void HandleAiming()
    {
        isAiming = PlayerInputManager.Instance.aimInput;
    }
}
