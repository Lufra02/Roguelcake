using UnityEngine;
using UnityEngine.InputSystem;

// Maneja el ataque cuerpo a cuerpo (principal) y el disparo de proyectil (secundario) en 3D.
// Usa el paquete nuevo "Input System" (Mouse.current).
public class PlayerCombat : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerController playerController;
    public Transform aimPoint; // Objeto vacío, hijo del jugador, colocado un poco delante de él (usado como punto de disparo)

    [Header("Ataque cuerpo a cuerpo")]
    public float meleeRange = 1.2f;
    public float meleeDamage = 10f;
    public float meleeCooldown = 0.4f;
    public LayerMask enemyLayer;

    [Header("Proyectil")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 12f;
    public float projectileCooldown = 0.6f;

    private float lastMeleeTime = -999f;
    private float lastProjectileTime = -999f;

    // Cuando está en false (ej. tienda abierta, diálogo, cinemática), el jugador no puede atacar ni disparar.
    public bool CanAttack { get; private set; } = true;

    void Awake()
    {
        if (playerController == null) playerController = GetComponent<PlayerController>();
    }

    public void SetCombatEnabled(bool enabled)
    {
        CanAttack = enabled;
    }

    void Update()
    {
        if (!CanAttack) return;
        if (Mouse.current == null) return;

        // Click izquierdo = ataque cuerpo a cuerpo (ataque principal del juego)
        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= lastMeleeTime + meleeCooldown)
        {
            MeleeAttack();
        }

        // Click derecho = disparo de proyectil (ataque secundario)
        if (Mouse.current.rightButton.wasPressedThisFrame && Time.time >= lastProjectileTime + projectileCooldown)
        {
            ShootProjectile();
        }
    }

    void MeleeAttack()
    {
        lastMeleeTime = Time.time;

        Vector3 aimDir = playerController.GetAimDirection();
        Vector3 hitCenter = transform.position + aimDir * (meleeRange * 0.5f);

        Collider[] hits = Physics.OverlapSphere(hitCenter, meleeRange * 0.5f, enemyLayer);
        foreach (Collider hit in hits)
        {
            var damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(meleeDamage);
            }
        }
    }

    void ShootProjectile()
    {
        if (projectilePrefab == null) return;

        lastProjectileTime = Time.time;

        Vector3 aimDir = playerController.GetAimDirection();
        Vector3 spawnPos = aimPoint != null ? aimPoint.position : transform.position;

        // El offset corrige solo la orientación visual del proyectil (por si su modelo tampoco
        // apunta hacia +Z). La dirección real de vuelo se define aparte en Launch() con aimDir puro.
        Quaternion rotation = aimDir.sqrMagnitude > 0.0001f
            ? Quaternion.LookRotation(aimDir, Vector3.up) * Quaternion.Euler(0f, playerController.modelRotationOffset, 0f)
            : transform.rotation;

        GameObject proj = Instantiate(projectilePrefab, spawnPos, rotation);

        Projectile projScript = proj.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.Launch(aimDir, projectileSpeed);
        }
    }

    // Ayuda visual en el editor para ver el rango del ataque cuerpo a cuerpo
    void OnDrawGizmosSelected()
    {
        if (playerController == null) return;
        Gizmos.color = Color.red;
        Vector3 aimDir = Application.isPlaying ? playerController.GetAimDirection() : transform.forward;
        Vector3 hitCenter = transform.position + aimDir * (meleeRange * 0.5f);
        Gizmos.DrawWireSphere(hitCenter, meleeRange * 0.5f);
    }
}