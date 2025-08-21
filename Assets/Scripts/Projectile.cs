using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public float damage = 10f;
    float lifetime = 10f;

    new Rigidbody rigidbody;
    new Collider collider;
    [HideInInspector] public GameObject owner;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();

        Destroy(gameObject, lifetime);
    }

    public void ShootProjectile(Vector3 direction, float speed, float damage)
    { 
        this.speed = speed;
        this.damage = damage;

        rigidbody.linearVelocity = direction * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Projectile otherProjectile = collision.gameObject.GetComponent<Projectile>();

        //projectiles should not collide with other projectiles with the same owner
        if (otherProjectile != null && otherProjectile.owner == owner)
        {
            Physics.IgnoreCollision(collider, collision.collider);
            return;
        }


        Destroy(gameObject);
    }
}
