using UnityEngine;
using UnityEngine.InputSystem;

// Controla el movimiento con WASD o joystick izquierdo.
// Controla el apuntado con mouse o joystick derecho.
[RequireComponent(typeof(Rigidbody), typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    [Header("Apuntado")]
    public Camera mainCamera;

    [Header("Corrección de modelo")]
    [Tooltip("Corrección visual de rotación del modelo.")]
    public float modelRotationOffset = 0f;

    [Header("Gamepad")]
    [Range(0f, 0.9f)]
    public float gamepadDeadZone = 0.15f;

    private Rigidbody rb;
    private PlayerStats stats;

    private Vector3 moveInput;
    private Vector3 mouseWorldPosition;

    private Vector3 aimDirection = Vector3.forward;

    private PlayerAnimationManager animationManager;

    // Control interno del movimiento.
    private bool movementEnabled = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        stats = GetComponent<PlayerStats>();

        animationManager = GetComponent<PlayerAnimationManager>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        if (!movementEnabled)
        {
            moveInput = Vector3.zero;
            UpdateAnimator();
            return;
        }

        ReadMovementInput();
        UpdateAiming();
        UpdateAnimator();
    }

    // Habilita o deshabilita el movimiento y el apuntado.
    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;

        moveInput = Vector3.zero;

        UpdateAnimator();

        if (!enabled)
        {
            rb.linearVelocity = new Vector3(
                0f,
                rb.linearVelocity.y,
                0f
            );
        }
    }

    private void UpdateAnimator()
    {
        animationManager?.SetMovement(moveInput.magnitude);
    }

    private void ReadMovementInput()
    {
        Vector2 input = Vector2.zero;

        // Teclado
        if (Keyboard.current != null)
        {
            float h = 0f;
            float v = 0f;

            if (Keyboard.current.aKey.isPressed)
                h -= 1f;

            if (Keyboard.current.dKey.isPressed)
                h += 1f;

            if (Keyboard.current.sKey.isPressed)
                v -= 1f;

            if (Keyboard.current.wKey.isPressed)
                v += 1f;

            input += new Vector2(h, v);
        }

        // Gamepad
        if (Gamepad.current != null)
        {
            Vector2 stickInput =
                Gamepad.current.leftStick.ReadValue();

            if (stickInput.sqrMagnitude >
                gamepadDeadZone * gamepadDeadZone)
            {
                input += stickInput;
            }
        }

        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        moveInput = new Vector3(
            input.x,
            0f,
            input.y
        );
    }

    private void FixedUpdate()
    {
        if (!movementEnabled)
            return;

        Vector3 targetVelocity =
            moveInput * stats.movementSpeed;

        targetVelocity.y = rb.linearVelocity.y;

        rb.linearVelocity = targetVelocity;
    }

    private void UpdateAiming()
    {
        if (!movementEnabled)
            return;

        if (Gamepad.current != null)
        {
            Vector2 rightStick =
                Gamepad.current.rightStick.ReadValue();

            if (rightStick.sqrMagnitude >
                gamepadDeadZone * gamepadDeadZone)
            {
                Vector3 stickDirection =
                    new Vector3(
                        rightStick.x,
                        0f,
                        rightStick.y
                    );

                ApplyAimDirection(stickDirection);
            }

            return;
        }

        AimWithMouse();
    }

    private void AimWithMouse()
    {
        if (Mouse.current == null)
            return;

        if (mainCamera == null)
            return;

        Vector2 mouseScreenPos =
            Mouse.current.position.ReadValue();

        Ray ray =
            mainCamera.ScreenPointToRay(mouseScreenPos);

        Plane groundPlane = new Plane(
            Vector3.up,
            new Vector3(
                0f,
                transform.position.y,
                0f
            )
        );

        if (groundPlane.Raycast(ray, out float distance))
        {
            mouseWorldPosition =
                ray.GetPoint(distance);

            Vector3 direction =
                mouseWorldPosition - transform.position;

            direction.y = 0f;

            ApplyAimDirection(direction);
        }
    }

    private void ApplyAimDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
            return;

        aimDirection = direction.normalized;

        Quaternion lookRotation =
            Quaternion.LookRotation(
                aimDirection,
                Vector3.up
            );

        transform.rotation =
            lookRotation *
            Quaternion.Euler(
                0f,
                modelRotationOffset,
                0f
            );
    }

    public Vector3 GetAimDirection()
    {
        return aimDirection;
    }

    public Vector3 GetMouseWorldPosition()
    {
        return mouseWorldPosition;
    }
}