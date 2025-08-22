using UnityEngine;

public class PlayerShootingManager : MonoBehaviour
{
    private PlayerManager playerManager;

    public Transform projectileSpawn;
    public GameObject projectilePrefab;

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

            Projectile projectile = Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.identity).GetComponent<Projectile>();
            projectile.owner = gameObject;
            projectile.ShootProjectile(targetDirection, projectileSpeed, projectileDamage);

            Physics.IgnoreCollision(playerManager.GetComponent<Collider>(), projectile.GetComponent<Collider>());

            nextShootTime = Time.time + fireRate;
        }
    }

    void HandleAiming()
    {
        playerManager.isAiming = PlayerInputManager.Instance.aimInput;
    }
}
