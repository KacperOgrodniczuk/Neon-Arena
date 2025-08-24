using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : NetworkBehaviour
{
    public float speed = 20f;
    public float damage = 10f;

    new Rigidbody rigidbody;
    new Collider collider;
    [HideInInspector] public GameObject owner;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }

    public void ShootProjectile(Vector3 direction, float speed, float damage, GameObject owner)
    {
        this.speed = speed;
        this.damage = damage;
        this.owner = owner;

        rigidbody.linearVelocity = direction * speed;
    }

    // Called on clients (via ObserversRpc)
    public void SimulateLocally(Vector3 direction, float speed)
    {
        if (IsServerInitialized) return; // server already set physics
        rigidbody.isKinematic = false; // allow local movement
        rigidbody.linearVelocity = direction * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServerInitialized) return;

        Despawn(gameObject);
    }

}
