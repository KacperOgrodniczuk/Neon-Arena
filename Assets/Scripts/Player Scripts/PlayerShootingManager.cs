using FishNet.Object;
using UnityEngine;

public class PlayerShootingManager : NetworkBehaviour
{
    private PlayerManager playerManager;

    public Transform projectileSpawn;
    public NetworkObject projectilePrefab;

    [Header("Attack Stats")]
    public float fireRate = 0.33f;
    public float projectileDamage = 10f;
    public float projectileSpeed = 20f;

    private float nextShootTime;

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

            SpawnProjectileServerRpc(targetDirection);

            nextShootTime = Time.time + fireRate;
        }
    }

    [ServerRpc]
    void SpawnProjectileServerRpc(Vector3 shootDirection)
    {
        NetworkObject projectileObject = Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.identity);
        Projectile projectile = projectileObject.GetComponent<Projectile>();

        projectile.ShootProjectile(shootDirection, projectileSpeed, projectileDamage, gameObject);

        Physics.IgnoreCollision(playerManager.GetComponent<Collider>(), projectile.GetComponent<Collider>());

        Spawn(projectileObject);

        SpawnProjectileClientRpc(projectileObject, shootDirection);
    }

    [ObserversRpc]
    void SpawnProjectileClientRpc(NetworkObject projectileObject, Vector3 direction)
    {
        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.SimulateLocally(direction, projectileSpeed);
    }

    void HandleAiming()
    {
        playerManager.isAiming = PlayerInputManager.Instance.aimInput;
    }
}
