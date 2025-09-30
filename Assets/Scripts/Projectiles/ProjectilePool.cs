using UnityEngine;
using UnityEngine.Pool;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance;

    public GameObject projectilePrefab;
    public int initialPoolSize = 20;
    public int maxPoolSize = 100;

    IObjectPool<GameObject> projectilePool;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitialisePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitialisePool()
    {
        projectilePool = new ObjectPool<GameObject>(
            CreateProjectile,
            GetProjectileFromPool,
            ReleaseProjectileToPool,
            DestroyProjectile,
            false,
            initialPoolSize,
            maxPoolSize
            );
    }

    GameObject CreateProjectile()
    {
        GameObject newProjectile = Instantiate(projectilePrefab);
        newProjectile.GetComponent<Projectile>().Pool = projectilePool;
        return newProjectile;
    }

    void GetProjectileFromPool(GameObject projectile)
    {
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        // We set the projectiles as active in playershooting
        // to ensure that we set ignore collision owner before any collision detection
        // takes place.
    }

    void ReleaseProjectileToPool(GameObject projectile)
    {
        projectile.SetActive(false);
    }

    void DestroyProjectile(GameObject projectile)
    {
        Destroy(projectile);
    }

    public GameObject GetProjectile(GameObject projectileOwner)
    { 
        return projectilePool.Get();
    }
}
