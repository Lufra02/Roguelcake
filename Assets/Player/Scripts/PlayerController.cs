using UnityEngine;
using UnityEngine.InputSystem;

// Controla el movimiento con WASD o el joystick izquierdo de un gamepad (plano XZ),
// y la rotación del jugador hacia el mouse en 3D.
// Usa el paquete nuevo "Input System" (Keyboard.current / Mouse.current / Gamepad.current).
// Requiere Rigidbody con Use Gravity = true (o false si tu juego no tiene caída).
[RequireComponent(typeof(Rigidbody), typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    [Header("Apuntado")]
    public Camera mainCamera;

    [Header("Corrección de modelo")]
    [Tooltip("Si tu modelo 3D no fue exportado con el frente hacia +Z, usa este valor para corregir el giro visual. Prueba con 90, -90 o 180 hasta que el frente del modelo coincida con la dirección real de apuntado.")]
    public float modelRotationOffset = 0f;

    [Header("Gamepad")]
    [Tooltip("Qué tanto hay que mover el joystick izquierdo antes de que cuente como input. Evita que un stick con drift mueva al jugador solo.")]
    [Range(0f, 0.9f)] public float gamepadDeadZone = 0.15f;

    private Rigidbody rb;
    private PlayerStats stats;
    private Vector3 moveInput;
    private Vector3 mouseWorldPosition;

    // Dirección de apuntado actual, sin importar si vino del mouse o del stick derecho.
    private Vector3 aimDirection = Vector3.forward;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        stats = GetComponent<PlayerStats>();
        if (mainCamera == null) mainCamera = Camera.main;

        // El jugador rota manualmente en Y hacia el mouse; evitamos que la física lo vuelque
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
         
        if (!PlayerManager.Instance.canMove)
        {
            moveInput = Vector3.zero;
            return;
        }

        ReadMovementInput();
        UpdateAiming();
    }

    // Habilita o deshabilita el movimiento y el apuntado del jugador.
    // Al deshabilitar, detiene inmediatamente la velocidad horizontal (conserva la vertical, por gravedad).
    public void SetMovementEnabled(bool enabled)
    {
        PlayerManager.Instance.canMove = enabled;
        moveInput = Vector3.zero;

        if (!enabled)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }

    void ReadMovementInput()
    {
        Vector2 input = Vector2.zero;

        // Teclado (A/D/S/W). Protegido por si no hay teclado detectado.
        if (Keyboard.current != null)
        {
            float h = 0f;
            float v = 0f;

            if (Keyboard.current.aKey.isPressed) h -= 1f;
            if (Keyboard.current.dKey.isPressed) h += 1f;
            if (Keyboard.current.sKey.isPressed) v -= 1f;
            if (Keyboard.current.wKey.isPressed) v += 1f;

            input += new Vector2(h, v);
        }

        // Gamepad: joystick izquierdo. Se suma al input de teclado en vez de reemplazarlo,
        // así funcionan ambos sin tener que detectar "qué dispositivo está usando el jugador".
        if (Gamepad.current != null)
        {
            Vector2 stickInput = Gamepad.current.leftStick.ReadValue();

            // Deadzone manual: algunos sticks reportan un valor pequeño incluso en reposo.
            if (stickInput.sqrMagnitude > gamepadDeadZone * gamepadDeadZone)
            {
                input += stickInput;
            }
        }

        // Si por alguna razón se presionan teclado y gamepad a la vez, evita que la suma
        // supere magnitud 1 (que haría al jugador moverse más rápido de lo normal).
        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        moveInput = new Vector3(input.x, 0f, input.y);
    }

    void FixedUpdate()
    {
        // Nota: en Unity 6 el Rigidbody usa "linearVelocity".
        // Si tu proyecto usa una versión anterior, cambia esta línea por: rb.velocity = ...
        Vector3 targetVelocity = moveInput * stats.movementSpeed;
        targetVelocity.y = rb.linearVelocity.y; // conserva la velocidad vertical (gravedad, saltos, etc.)
        rb.linearVelocity = targetVelocity;
    }

    // Decide la fuente de apuntado: si el stick derecho del gamepad tiene input
    // significativo, tiene prioridad sobre el mouse (si lo estás moviendo, es porque
    // quieres apuntar con él). Si no, se usa el mouse como siempre.
    void UpdateAiming()
    {
        // Con un gamepad conectado, el mouse se ignora por completo (no solo cuando
        // el stick está activo). Así, al soltar el stick, el jugador se queda mirando
        // hacia la última dirección apuntada en vez de "saltar" hacia donde esté el mouse.
        if (Gamepad.current != null)
        {
            Vector2 rightStick = Gamepad.current.rightStick.ReadValue();

            if (rightStick.sqrMagnitude > gamepadDeadZone * gamepadDeadZone)
            {
                Vector3 stickDirection = new Vector3(rightStick.x, 0f, rightStick.y);
                ApplyAimDirection(stickDirection);
            }
            // Stick en reposo: no se actualiza aimDirection ni la rotación, se mantiene la última.

            return;
        }

        AimWithMouse();
    }

    void AimWithMouse()
    {
        if (Mouse.current == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPos);

        // Plano matemático horizontal a la altura del jugador.
        // No requiere colliders de "suelo" reales, funciona con cámara top-down o isométrica.
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

        if (groundPlane.Raycast(ray, out float distance))
        {
            mouseWorldPosition = ray.GetPoint(distance);

            Vector3 direction = mouseWorldPosition - transform.position;
            direction.y = 0f;

            ApplyAimDirection(direction);
        }
    }

    // Punto único donde se aplica cualquier dirección de apuntado (venga de donde venga):
    // actualiza aimDirection y rota el modelo hacia ella, con el offset de corrección ya aplicado.
    void ApplyAimDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.0001f) return;

        aimDirection = direction.normalized;

        // El offset solo corrige la rotación visual; GetAimDirection() sigue devolviendo
        // la dirección real de apuntado, sin distorsión, para que el disparo apunte bien.
        Quaternion lookRotation = Quaternion.LookRotation(aimDirection, Vector3.up);
        transform.rotation = lookRotation * Quaternion.Euler(0f, modelRotationOffset, 0f);
    }

    // Dirección normalizada (en el plano XZ) hacia donde apunta el jugador, usada por PlayerCombat
    public Vector3 GetAimDirection()
    {
        return aimDirection;
    }

    public Vector3 GetMouseWorldPosition()
    {
        return mouseWorldPosition;
    }
}