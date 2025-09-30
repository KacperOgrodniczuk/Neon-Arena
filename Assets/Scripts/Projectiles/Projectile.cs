using FishNet;
using FishNet.Object;
using System.Collections;
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

    private Coroutine catchupCoroutine;

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

    public void ShootProjectile(Vector3 direction, float speed, float damage, float lagCompensationTime = 0f)
    {
        this.speed = speed;
        this.damage = damage;

        float catchupMultiplier = Mathf.Clamp(lagCompensationTime * 2f, 0f, 1f); // Max 100% speed boost
        float initialSpeed = speed * catchupMultiplier;

        rigidbody.linearVelocity = direction * speed;

        if (catchupCoroutine != null)
        { 
            StopCoroutine(catchupCoroutine);
        }

        //calculate initial velocity with catchup.
        if (lagCompensationTime > 0f)
        {
            catchupCoroutine = StartCoroutine(CatchupVelocityDecay());
        }
    }

    IEnumerator CatchupVelocityDecay()
    {
        float currentSpeed = rigidbody.linearVelocity.magnitude;

        while (currentSpeed > speed * 1.01f)    // Small tolerance to avoid floating point issues.
        {
            // Gradually reduce speed to normal speed;
            currentSpeed = Mathf.MoveTowards(currentSpeed, speed, speed * Time.deltaTime * 3f);
            rigidbody.linearVelocity = rigidbody.linearVelocity.normalized * currentSpeed;
            yield return new WaitForFixedUpdate();
        }

        // Ensure we end up at the exact right speed;
        rigidbody.linearVelocity = rigidbody.linearVelocity.normalized * speed;
        catchupCoroutine = null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Says it's obsolete, at the time of writing instance finder does not have "IsClientInitialized" or "IsServerInitialized.
        // Collisions only happen on the server, client projectiles are cosmetic only.
        // Pass the owner of the projectile to send a target rpc to confirm a hit using a hitmarker on client side.
        if (InstanceFinder.IsServer)
        {
            owner.TryGetComponent(out NetworkObject ownerNetworkObject);
            collision.gameObject.GetComponent<PlayerHealthManager>()?.TakeDamage(damage, ownerNetworkObject);
        }

        // play particle effect for collision
        ContactPoint contactPoint = collision.contacts[0];
        Vector3 hitPoint = contactPoint.point;
        Vector3 hitNormal = contactPoint.normal;
        Quaternion hitRotation = Quaternion.LookRotation(hitNormal);
        ParticleEffectManager.Instance.PlayEffect("ProjectileHit", hitPoint, hitRotation);

        // play audio
        Pool.Release(gameObject);
    }

    private void OnDisable()
    {
        if (owner != null)
        {
            Physics.IgnoreCollision(collider, owner.GetComponent<Collider>(), false);
        }

        // Clean up coroutine when projectile is returned to pool
        if (catchupCoroutine != null)
        {
            StopCoroutine(catchupCoroutine);
            catchupCoroutine = null;
        }
    }
}
