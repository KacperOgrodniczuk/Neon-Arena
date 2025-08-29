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
            createFunc: CreateProjectile,
            actionOnGet: GetProjectileFromPool,
            actionOnRelease: ReleaseProjectileToPool,
            actionOnDestroy: DestroyProjectile,
            collectionCheck: true,   //should be set to false in main build.
            defaultCapacity: initialPoolSize,
            maxSize: maxPoolSize
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
