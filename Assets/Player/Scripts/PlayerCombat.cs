using UnityEngine;
using UnityEngine.InputSystem;

// Maneja el ataque cuerpo a cuerpo (principal) y el disparo de proyectil (secundario) en 3D.
// Usa el paquete nuevo "Input System" (Mouse.current / Gamepad.current).
[RequireComponent(typeof(PlayerStats))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Referencias")]
    private PlayerManager playerManager;
    private PlayerStats stats;
    private PlayerController playerController;
    [SerializeField] private Transform aimPoint; // Objeto vacío, hijo del jugador, colocado un poco delante de él (usado como punto de disparo)

    [Header("Ataque cuerpo a cuerpo")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Proyectil")]
    [SerializeField] private GameObject projectilePrefab;
    private float lastMeleeTime = -999f;
    private float lastProjectileTime = -999f;

    void Awake()
    {
        if (playerController == null) playerController = GetComponent<PlayerController>();
        stats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        playerManager = PlayerManager.Instance;
    }

    public void SetCombatEnabled(bool enabled)
    {
        PlayerManager.Instance.canAttack = enabled;
    }

    void Update()
    {
        if (!PlayerManager.Instance.canAttack) return;

        float currentMeleeCooldown = stats.PhysicalAttackCooldown;
        float currentProjectileCooldown = stats.ShootingCooldown;

        // Se revisa cada dispositivo por separado y solo si está presente, para poder
        // jugar indistintamente con mouse o con gamepad sin que uno dependa del otro.
        bool meleePressed = false;
        bool shootPressed = false;

        if (Mouse.current != null)
        {
            meleePressed |= Mouse.current.leftButton.wasPressedThisFrame;
            shootPressed |= Mouse.current.rightButton.wasPressedThisFrame;
        }

        if (Gamepad.current != null)
        {
            meleePressed |= Gamepad.current.rightShoulder.wasPressedThisFrame;
            shootPressed |= Gamepad.current.rightTrigger.wasPressedThisFrame;
        }

        // Click izquierdo / RB = ataque cuerpo a cuerpo (ataque principal del juego)
        if (meleePressed && Time.time >= lastMeleeTime + currentMeleeCooldown)
        {
            MeleeAttack();
        }

        // Click derecho / RT = disparo de proyectil (ataque secundario)
        if (shootPressed && Time.time >= lastProjectileTime + currentProjectileCooldown)
        {
            ShootProjectile();
        }
    }

    void MeleeAttack()
    {
        lastMeleeTime = Time.time;

        Vector3 aimDir = playerController.GetAimDirection();
        float physicalAttackSize = stats.physicalAttackSize;
        Vector3 hitCenter = transform.position + aimDir * (physicalAttackSize * 0.5f);

        Collider[] hits = Physics.OverlapSphere(hitCenter, physicalAttackSize * 0.5f, enemyLayer);
        foreach (Collider hit in hits)
        {
            var damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(stats.physicalDamage);
            }
        }
    }

    void ShootProjectile()
    {
        if (projectilePrefab == null) return;
        int currentShootDamage = stats.shootingDamage;
        
        if (playerManager.playerHealth.CurrentHealth <= stats.shootingCost) return;
        playerManager.playerHealth.TakeDamage(stats.shootingCost);

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
            projScript.Configure(currentShootDamage, stats.projectileBounces, stats.maxTravelDistance,
                stats.bouncyGun, stats.explosiveGun, stats.hugeGun, stats.explosiveDamage, stats.explosiveRange, 
                stats.hugeDamagePercent);
            
            projScript.Launch(aimDir, stats.projectileSpeed);
        }
    }

    // Ayuda visual en el editor para ver el rango del ataque cuerpo a cuerpo
    void OnDrawGizmosSelected()
    {
        if (playerController == null) return;
        Gizmos.color = Color.red;
        Vector3 aimDir = Application.isPlaying ? playerController.GetAimDirection() : transform.forward;
        float physicalAttackSize = stats != null ? stats.physicalAttackSize : 1.2f;
        Vector3 hitCenter = transform.position + aimDir * (physicalAttackSize * 0.5f);
        Gizmos.DrawWireSphere(hitCenter, physicalAttackSize * 0.5f);
    }
}