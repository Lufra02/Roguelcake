using UnityEngine;
using System.Collections;

// Punto único para disparar y controlar las animaciones del jugador. Cualquier otro
// script (movimiento, combate, vida, etc.) le pide a este manager que reproduzca algo,
// en vez de tocar el Animator directamente — así los nombres de los parámetros, y el
// propio Animator, solo se conocen aquí adentro.
public class PlayerAnimationManager : MonoBehaviour
{
    [Tooltip("Si se deja vacío, se busca automáticamente en este objeto o en sus hijos (normal si el Animator está en el modelo 3D, separado de la lógica).")]
    [SerializeField] private Animator animator;

    [Header("Efectos de Ataque")]
    [SerializeField] private GameObject slashEffectPrefab;
    [SerializeField] private Transform slashSpawnPoint;

    [Header("Efecto de daño")]
    [Tooltip("Objeto ya colocado en el Player. Debe contener un ParticleSystem y empezar desactivado.")]
    [SerializeField] private GameObject damageEffect;
    private ParticleSystem[] damageParticles;
    private bool isDamageEffectPlaying;

    // Parámetros del Blend Tree 2D: el movimiento se expresa respecto al apuntado.
    private static readonly int HorizontalHash = Animator.StringToHash("Horizontal");
    private static readonly int VerticalHash = Animator.StringToHash("Vertical");

    // Triggers: se disparan una sola vez y el propio Animator decide cuándo "consumirlos".
    // Son el patrón correcto para ataques u otras acciones puntuales que no necesitan
    // un parámetro continuo (a diferencia de Movement, que sí es un valor que cambia cada frame).
    private static readonly int MeleeAttackHash = Animator.StringToHash("MeleeAttack");
    private static readonly int ShootHash = Animator.StringToHash("Shoot");

    private GameManager gameManager;





    private void Awake()
    {
        // GetComponentInChildren (no GetComponent): el Animator normalmente vive en el
        // modelo 3D, que es un hijo de este GameObject, no el mismo objeto.
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (animator == null)
        {
            Debug.LogWarning($"[PlayerAnimationManager] No se encontró ningún Animator en {name} ni en sus hijos. Asígnalo manualmente en el Inspector.");
        }

        if (damageEffect != null)
        {
            damageParticles = damageEffect.GetComponentsInChildren<ParticleSystem>(true);
            damageEffect.SetActive(false);
        }
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.onChangeGameState += OnChangeGameStateCallback;
        }
    }

    private void Update()
    {
        if (!isDamageEffectPlaying) return;

        foreach (ParticleSystem particle in damageParticles)
        {
            if (particle != null && particle.IsAlive(true)) return;
        }

        damageEffect.SetActive(false);
        isDamageEffectPlaying = false;
    }

    // Congela/reanuda el Animator en el frame exacto donde iba, sin perder el estado actual.
    private void OnChangeGameStateCallback(GameState newState)
    {
        if (animator == null) return;
        animator.speed = newState == GameState.Pause ? 0f : 1f;
    }

    private void OnDestroy()
    {
        if (gameManager != null)
            gameManager.onChangeGameState -= OnChangeGameStateCallback;
    }

    // --- Movimiento ---

    // Llamado desde PlayerController con valores locales al apuntado.
    // Horizontal: -1 izquierda, 1 derecha. Vertical: -1 atrás, 1 adelante.
    public void SetMovement(float horizontal, float vertical)
    {
        if (animator == null) return;
        animator.SetFloat(HorizontalHash, horizontal);
        animator.SetFloat(VerticalHash, vertical);
    }

    public void PlayOnTakeAnimation(string animationName)
    {
        if (animator == null) return;
        animator.Play(animationName);
    }

    // --- Ataques ---

    public void PlayMeleeAttack()
    {
        PlayTrigger(MeleeAttackHash);
    }

    public void PlayShootAttack()
    {
        PlayTrigger(ShootHash);
    }

    private void TriggerSlashEffect()
    {
        Debug.Log("¡Se intentó lanzar el slash!");
        if (slashEffectPrefab != null && slashSpawnPoint != null)
        {
            GameObject fx = Instantiate(slashEffectPrefab, slashSpawnPoint.position, slashSpawnPoint.rotation);
            Destroy(fx, 2f); // Se destruye solo a los 2 segundos para no acumular basura en memoria
        }
        else
        {
            Debug.LogError("¡Falta asignar el Prefab o el SpawnPoint en el Inspector del PlayerAnimationManager!");
        }
    }

    // --- Genérico ---

    // Punto de entrada por nombre, para cuando agregues animaciones nuevas (esquiva, recibir
    // daño, morir...) sin tener que escribir un método dedicado cada vez que necesites una.
    public void PlayTrigger(string triggerName)
    {
        PlayTrigger(Animator.StringToHash(triggerName));
    }

    public void PlayTrigger(int triggerHash)
    {
        if (animator == null) return;
        animator.SetTrigger(triggerHash);
    }

    // Útil para cancelar un trigger en cola, por ejemplo si el jugador muere a mitad de un ataque.
    public void CancelTrigger(string triggerName)
    {
        if (animator == null) return;
        animator.ResetTrigger(triggerName);
    }
    public void InvokeSlashWithDelay(float delaySeconds)
    {
        StartCoroutine(SpawnSlashDelayed(delaySeconds));
    }

    private IEnumerator SpawnSlashDelayed(float delaySeconds)
    {
        // Espera exactamente los segundos indicados antes de mostrar el slash
        yield return new WaitForSeconds(delaySeconds);

        if (slashEffectPrefab != null && slashSpawnPoint != null)
        {
            GameObject fx = Instantiate(slashEffectPrefab, slashSpawnPoint.position, slashSpawnPoint.rotation, gameObject.transform);

            ParticleSystem ps = fx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop();
                ps.Play();
            }

            Destroy(fx, 2f);
        }
    }
    public void PlayDamageEffect()
    {
        if (damageEffect == null)
        {
            Debug.LogWarning("[PlayerAnimationManager] Falta asignar Damage Effect.", this);
            return;
        }

        damageEffect.SetActive(true);
        foreach (ParticleSystem particle in damageParticles)
        {
            if (particle == null) continue;
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle.Play(true);
        }

        isDamageEffectPlaying = damageParticles.Length > 0;
        // shortcut: el efecto debe no estar en Loop; agrega un cierre explícito si se vuelve continuo.
    }
    public void PlayDirectionalDamageAnimation(Vector3 hitSourcePosition)
    {
        // 1. Vector desde el jugador hacia donde ocurrió el golpe
        Vector3 dirToSource = hitSourcePosition - transform.position;
        dirToSource.y = 0f; // Evitamos inclinaciones arriba/abajo

        // Si no hay diferencia de posición, usamos la animación general de respaldo
        if (dirToSource.sqrMagnitude <= 0.0001f)
        {
            PlayOnTakeAnimation("Take_Damage_01");
            return;
        }

        // 2. Pasamos la dirección a coordenadas locales del jugador:
        // localDir.z > 0 = delante | localDir.z < 0 = atrás
        // localDir.x > 0 = derecha | localDir.x < 0 = izquierda
        Vector3 localDir = transform.InverseTransformDirection(dirToSource.normalized);
        string animName;

        if (Mathf.Abs(localDir.z) >= Mathf.Abs(localDir.x))
        {
            animName = localDir.z >= 0f ? "Take_Damage_Front" : "Take_Damage_Back";
        }
        else
        {
            animName = localDir.x >= 0f ? "Take_Damage_Right" : "Take_Damage_Left";
        }

        // 3. Reproducimos la animación elegida
        PlayOnTakeAnimation(animName);
    }
}
