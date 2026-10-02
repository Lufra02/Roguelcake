using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }

    // GM 
    GameManager gameManager;

    // REFERENCIAS
    [HideInInspector] public PlayerHealth playerHealth;
    [HideInInspector] public PlayerCombat playerCombat;
    [HideInInspector] public PlayerController playerController;
    [HideInInspector] public PlayerInteraction playerInteraction;
    [HideInInspector] public PlayerStats playerStats;

    // FLAGS
    public bool canMove = true;
    public bool canAttack = true;
    public bool isDead = false;
    public bool isPaused = false;
    public bool isInShop = false;

    // La interfaz abierta tiene prioridad sobre los controles generales del juego.
    private ShopInteractable openShop;
    
    private void Start()
    {
        // ASIGNACIONES
        playerInteraction = GetComponent<PlayerInteraction>();
        playerHealth = GetComponent<PlayerHealth>();
        playerCombat = GetComponent<PlayerCombat>();
        playerController = GetComponent<PlayerController>();
        playerStats = GetComponent<PlayerStats>();

        gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.onChangeGameState += OnChangeGameStateCallback;
        }

    }
    
    private void Update()
    {
        
        if(openShop != null) return;
        
        // wasPressedThisFrame solo es verdadero en el frame donde comienza la pulsación.
        if (gameManager != null && Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Gamepad.current.startButton.wasPressedThisFrame))
        {
            gameManager.PauseGame();
        }
    }

    public void SetOpenShop(ShopInteractable shop)
    {
        openShop = shop;
    }

    public void ClearOpenShop(ShopInteractable shop)
    {
        if (openShop == shop)
            openShop = null;
    }

    //Esta es la funcion que quiero que se haga cada vez que pauso o despauso el juego
    public void OnChangeGameStateCallback(GameState newState) 
    {
        isPaused = newState == GameState.Pause;
        if (isPaused)
        {
            canMove = false;
            canAttack = false;
        }
        else { 
            canMove = !isInShop;
            canAttack = !isInShop;
        }
        
        playerController?.SetMovementEnabled(canMove);
        playerCombat?.SetCombatEnabled(canAttack);
    }
    
    private void OnDestroy()
    {
        if (gameManager != null)
            gameManager.onChangeGameState -= OnChangeGameStateCallback;
    }

}
