using FishNet;
using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public float damage = 10f;

    new Rigidbody rigidbody;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
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
        Destroy(gameObject);
    }

}
