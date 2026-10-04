using System;
using UnityEngine;
using Random = UnityEngine.Random;

// Simula un arco físico simple (como el de un proyectil o un objeto que sale despedido)
// SIN usar Rigidbody. Útil para objetos pequeños y desechables (ej. gomitas de una
// explosión) donde instanciar muchos Rigidbody reales sería más caro de lo necesario.
public class FakeProjectilePhysics : MonoBehaviour
{
    [Header("Física simulada")]
    [Tooltip("Qué tan fuerte 'cae' el objeto. Equivalente conceptual a la gravedad de un Rigidbody.")]
    [SerializeField] private float gravity = 20f;
    [Tooltip("Altura Y del suelo. Cuando el objeto cae por debajo de este valor, se detiene ahí.")]
    [SerializeField] private float groundLevel = 0f;

    [Header("Efecto visual (opcional)")]
    [SerializeField] private bool randomSpin = true;
    [SerializeField] private float spinSpeed = 180f;

    private Vector3 velocity;
    private bool isGrounded;
    private Vector3 spinAxis;

    // Se dispara UNA vez, justo cuando el objeto aterriza (ya sea por alcanzar groundLevel
    // o porque algo externo llamó a Land() manualmente, como un collider real de suelo).
    public event System.Action OnLanded;

    private GameManager gameManager;
    private bool isGamePaused;
    private void Awake()
    {
        spinAxis = Random.onUnitSphere;
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.onChangeGameState += OnChangeGameStateCallback;
            isGamePaused = gameManager.gameState == GameState.Pause;
        }
    }

    // Esta es la funcion que se hace cada vez que pauso o despauso el juego
    private void OnChangeGameStateCallback(GameState newState)
    {
        isGamePaused = newState == GameState.Pause;
    }

    // Llama a esto justo después de instanciar el objeto, con la velocidad inicial completa
    // (dirección + fuerza ya combinadas). Por ejemplo: una dirección horizontal aleatoria
    // más un impulso hacia arriba, para simular que "salta" desde el centro de la explosión.
    public void Launch(Vector3 initialVelocity)
    {
        velocity = initialVelocity;
        isGrounded = false;
        enabled = true; // por si se había desactivado al aterrizar una vez anterior (pooling)
    }

    private void Update()
    {
        if (isGamePaused) return;
        if (isGrounded) return;

        // Integración simple (Euler explícito): la gravedad reduce la velocidad vertical
        // cada frame, y la posición avanza según la velocidad actual. No es tan preciso
        // como la física real de Unity, pero para un efecto visual como este es más que suficiente.
        velocity.y -= gravity * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;

        if (randomSpin)
        {
            transform.Rotate(spinAxis, spinSpeed * Time.deltaTime, Space.World);
        }

        if (transform.position.y <= groundLevel)
        {
            Vector3 pos = transform.position;
            pos.y = groundLevel;
            transform.position = pos;

            Land();
        }
    }

    // Marca el objeto como aterrizado y dispara OnLanded. Público para que, por ejemplo,
    // un collider real de suelo (detectado en OnTriggerEnter del propio objeto) pueda
    // forzar el aterrizaje antes de que se cumpla la condición de groundLevel.
    public void Land()
    {
        if (isGrounded) return; // evita invocar el evento más de una vez

        velocity = Vector3.zero;
        isGrounded = true;
        OnLanded?.Invoke();
    }
}