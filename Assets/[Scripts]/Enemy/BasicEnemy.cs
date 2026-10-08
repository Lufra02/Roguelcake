using UnityEngine;

public class BasicEnemy : Enemy
{
    private enum EnemyState { Normal, Charging, Dashing }
    private EnemyState currentState = EnemyState.Normal;

    [Header("Special Abilities")]
    [SerializeField] private int dashUnlockRound = 3;
    [SerializeField] private int enrageUnlockRound = 5;

    [Header("Dash Settings")]
    [SerializeField] private float dashCooldown = 8f;
    [SerializeField] private float dashSpeedMultiplier = 3f;
    [SerializeField] private float dashChargeTime = 0.6f;   // antes hardcodeado en la corrutina
    [SerializeField] private float dashDuration = 0.6f;

    [Header("Enrage Settings")]
    private bool isEnraged = false;
    private float baseMoveSpeed;

    [Header("Health & Damage Scaling")]
    [SerializeField] private float healthBonusPerRound = 15f;
    [SerializeField] private float damageBonusPerRound = 2.5f;

    // --- Estado del dash (reemplaza a la corrutina) ---
    private float dashCooldownTimer;   // reemplaza a nextDashTime
    private float dashStateTimer;      // tiempo restante de Charging / Dashing
    private Vector3 dashDirection;
    private float dashOriginalSpeed;
    private float dashOriginalAcceleration;


    protected override void Awake()
    {
        base.Awake();
        baseMoveSpeed = moveSpeed;
    }

    public override void Initialize(GameObject xpPrefab, Transform playerTarget)
    {
        base.Initialize(xpPrefab, playerTarget);

        if (RoundManager.GetInstance() != null)
        {
            int roundsCompleted = RoundManager.GetInstance().CompletedRounds;

            maxHealth = 30f + (roundsCompleted * healthBonusPerRound);
            currentHealth = maxHealth;
            damage = 10f + (roundsCompleted * damageBonusPerRound);
        }

        currentState = EnemyState.Normal;
        isEnraged = false;
        moveSpeed = baseMoveSpeed;
        if (agent != null) agent.speed = baseMoveSpeed;

        // Offset aleatorio para que no todos hagan dash al mismo segundo
        dashCooldownTimer = Random.Range(3f, dashCooldown);
        dashStateTimer = 0f;
    }

    protected override void Update()
    {
        if (isGamePaused || isDead || target == null || agent == null)
            return;

        // El cooldown corre siempre que no haya pausa (igual que antes con Time.time)
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        // Mientras carga o hace dash, se avanza el dash y no se ejecuta la IA base
        if (currentState != EnemyState.Normal)
        {
            UpdateDash();
            return;
        }

        int currentRound = RoundManager.GetInstance() != null ? RoundManager.GetInstance().CompletedRounds + 1 : 1;

        CheckEnrage(currentRound);

        float distance = Vector3.Distance(transform.position, target.position);

        if (currentRound >= dashUnlockRound && dashCooldownTimer <= 0f && distance <= detectionRange && distance > attackRange)
        {
            StartDash();
            return;
        }

        base.Update();
    }

    // ---------------- DASH ----------------

    private void StartDash()
    {
        Debug.Log($"<color=yellow>{name} inicia CARGA de dash!</color>");
        currentState = EnemyState.Charging;
        dashCooldownTimer = dashCooldown;
        dashStateTimer = dashChargeTime;

        agent.ResetPath();
        dashDirection = (target.position - transform.position).normalized;
        dashDirection.y = 0;
        transform.forward = dashDirection;

        // Aquí va el feedback visual/sonoro de la carga (color, partículas, etc.)
    }

    private void UpdateDash()
    {
        dashStateTimer -= Time.deltaTime;
        if (dashStateTimer > 0f) return;

        switch (currentState)
        {
            case EnemyState.Charging:
                BeginDashMovement();
                break;

            case EnemyState.Dashing:
                EndDash();
                break;
        }
    }

    private void BeginDashMovement()
    {
        Debug.Log($"<color=yellow>{name} inicia DASH!</color>");
        currentState = EnemyState.Dashing;
        dashStateTimer = dashDuration;

        dashOriginalSpeed = agent.speed;
        dashOriginalAcceleration = agent.acceleration;
        agent.speed = baseMoveSpeed * dashSpeedMultiplier;
        agent.acceleration = dashOriginalAcceleration * 2f;

        // Destino pasado el jugador para que atraviese en línea recta
        Vector3 targetDestination = transform.position + dashDirection * (attackRange * 3f);
        agent.SetDestination(targetDestination);
    }

    private void EndDash()
    {
        Debug.Log($"<color=yellow>{name} termina DASH!</color>");
        agent.speed = dashOriginalSpeed;
        agent.acceleration = dashOriginalAcceleration;
        agent.ResetPath();
        currentState = EnemyState.Normal;
    }

    // ---------------- RESTO ----------------

    private void CheckEnrage(int currentRound)
    {
        if (currentRound < enrageUnlockRound || isEnraged) return;

        if (currentHealth <= maxHealth * 0.4f)
        {
            isEnraged = true;
            agent.speed = baseMoveSpeed * 1.5f;
            damage *= 1.3f;
            Debug.Log($"<color=red>{name} entró en MODO FURIA!</color>");
        }
    }

    protected override void OnDeath()
    {
        base.OnDeath();
    }
}