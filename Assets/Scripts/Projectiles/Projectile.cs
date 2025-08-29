using FishNet;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public float damage = 10f;
    [HideInInspector] public GameObject owner;

    new Rigidbody rigidbody;
    new Collider collider;

    public IObjectPool<GameObject> Pool { get; set; }

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }

    public void SetOwnerAndIgnoreCollisions(GameObject owner)
    { 
        this.owner = owner;
        Physics.IgnoreCollision(collider, owner.GetComponent<Collider>(), true);
    }

    public void ShootProjectile(Vector3 direction, float speed, float damage)
    {
        this.speed = speed;
        this.damage = damage;

        rigidbody.linearVelocity = direction * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Says it's obsolete, at the time of writing instance finder does not have "IsClientInitialized" or "IsServerInitialized.
        if (InstanceFinder.IsServer)
        {
            collision.gameObject.GetComponent<PlayerHealthManager>()?.TakeDamage(damage);
        }

        // play particle effect for collision
        // play audio
        Pool.Release(gameObject);
    }

    private void OnDisable()
    {
        if (owner != null)
        {
            Physics.IgnoreCollision(collider, owner.GetComponent<Collider>(), false);
        }
    }
}
