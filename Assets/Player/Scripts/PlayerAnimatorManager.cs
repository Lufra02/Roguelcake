using UnityEngine;

// Punto único para disparar y controlar las animaciones del jugador. Cualquier otro
// script (movimiento, combate, vida, etc.) le pide a este manager que reproduzca algo,
// en vez de tocar el Animator directamente — así los nombres de los parámetros, y el
// propio Animator, solo se conocen aquí adentro.
public class PlayerAnimationManager : MonoBehaviour
{
    [Tooltip("Si se deja vacío, se busca automáticamente en este objeto o en sus hijos (normal si el Animator está en el modelo 3D, separado de la lógica).")]
    [SerializeField] private Animator animator;

    // Parámetro float continuo (0 a 1) que controla el blend Idle <-> Movimiento.
    private static readonly int MovementHash = Animator.StringToHash("Movement");

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
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.onChangeGameState += OnChangeGameStateCallback;
        }
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

    // Llamado cada frame desde PlayerController con moveInput.magnitude (0 = quieto, 1 = velocidad máxima).
    public void SetMovement(float normalizedSpeed)
    {
        if (animator == null) return;
        animator.SetFloat(MovementHash, normalizedSpeed);
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
}