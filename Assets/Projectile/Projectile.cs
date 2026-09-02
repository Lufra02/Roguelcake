using UnityEngine;

// Proyectil simple: viaja en línea recta en la dirección hacia el mouse
// y aplica daño al primer objeto con IDamageable que golpee.
// El GameObject necesita un Collider 3D marcado como "Is Trigger".
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public float damage = 5f;
    public float lifeTime = 3f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // el proyectil viaja recto; actívalo si quieres que caiga por gravedad
        Destroy(gameObject, lifeTime);
    }

    public void Launch(Vector3 direction, float speed)
    {
        // Nota: en Unity 6 es "linearVelocity"; en versiones anteriores usa "velocity"
        rb.linearVelocity = direction.normalized * speed;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) return; // ignora al propio jugador

        var damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}