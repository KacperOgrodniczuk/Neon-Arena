using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 3f;
    public int damage = 10;

    new Rigidbody rigidbody;
    new Collider collider;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }
}
