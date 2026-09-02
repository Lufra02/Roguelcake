using UnityEngine;
using UnityEngine.InputSystem;

// Controla el movimiento con WASD (plano XZ) y la rotación del jugador hacia el mouse en 3D.
// Usa el paquete nuevo "Input System" (Keyboard.current / Mouse.current).
// Requiere Rigidbody con Use Gravity = true (o false si tu juego no tiene caída).
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 6f;

    [Header("Apuntado")]
    public Camera mainCamera;

    [Header("Corrección de modelo")]
    [Tooltip("Si tu modelo 3D no fue exportado con el frente hacia +Z, usa este valor para corregir el giro visual. Prueba con 90, -90 o 180 hasta que el frente del modelo coincida con la dirección real de apuntado.")]
    public float modelRotationOffset = 0f;

    private Rigidbody rb;
    private Vector3 moveInput;
    private Vector3 mouseWorldPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (mainCamera == null) mainCamera = Camera.main;

        // El jugador rota manualmente en Y hacia el mouse; evitamos que la física lo vuelque
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        ReadMovementInput();
        AimTowardsMouse();
    }

    void ReadMovementInput()
    {
        // Keyboard.current puede ser null si no hay teclado detectado; nos protegemos
        if (Keyboard.current == null)
        {
            moveInput = Vector3.zero;
            return;
        }

        float h = 0f;
        float v = 0f;

        if (Keyboard.current.aKey.isPressed) h -= 1f;
        if (Keyboard.current.dKey.isPressed) h += 1f;
        if (Keyboard.current.sKey.isPressed) v -= 1f;
        if (Keyboard.current.wKey.isPressed) v += 1f;

        moveInput = new Vector3(h, 0f, v).normalized;
    }

    void FixedUpdate()
    {
        // Nota: en Unity 6 el Rigidbody usa "linearVelocity".
        // Si tu proyecto usa una versión anterior, cambia esta línea por: rb.velocity = ...
        Vector3 targetVelocity = moveInput * moveSpeed;
        targetVelocity.y = rb.linearVelocity.y; // conserva la velocidad vertical (gravedad, saltos, etc.)
        rb.linearVelocity = targetVelocity;
    }

    void AimTowardsMouse()
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

            if (direction.sqrMagnitude > 0.0001f)
            {
                // El offset solo corrige la rotación visual; GetAimDirection() sigue devolviendo
                // la dirección real hacia el mouse, sin distorsión, para que el disparo apunte bien.
                Quaternion lookRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                transform.rotation = lookRotation * Quaternion.Euler(0f, modelRotationOffset, 0f);
            }
        }
    }

    // Dirección normalizada (en el plano XZ) hacia el mouse, usada por PlayerCombat
    public Vector3 GetAimDirection()
    {
        Vector3 dir = mouseWorldPosition - transform.position;
        dir.y = 0f;
        return dir.normalized;
    }

    public Vector3 GetMouseWorldPosition()
    {
        return mouseWorldPosition;
    }
}