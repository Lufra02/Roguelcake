using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    GameManager gameManager;
    
    [Header("Pause System")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button exitButton;
    
    [SerializeField] private GameObject selectedButtonHover;

    private void Start()
    {
        gameManager = GameManager.Instance;
        
        resumeButton.onClick.AddListener(Resume);
        optionsButton.onClick.AddListener(Options);
        exitButton.onClick.AddListener(ExitScene);
        
        pausePanel.SetActive(false);
        
        if (gameManager != null)
        {
            gameManager.onChangeGameState += OnChangeGameStateCallback;
        }
    }

    private void Update()
    {
        // Mientras el panel esté visible, el highlight sigue al botón que el EventSystem
        // considere "seleccionado" en este momento, venga ese estado de mouse, teclado o gamepad.
        if (!pausePanel.activeSelf) return;
 
        UpdateHoverPosition();
    }
    
    private void UpdateHoverPosition()
    {
        if (selectedButtonHover == null || EventSystem.current == null) return;
 
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null) return;
 
        selectedButtonHover.transform.position = selected.transform.position;
    }
    
    private void OnChangeGameStateCallback(GameState newState)
    {
        bool isPaused = newState == GameState.Pause;
        pausePanel.SetActive(isPaused);
 
        if (isPaused)
        {
            SelectFirstButton();
        }
        else if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
    
    private void SelectFirstButton()
    {
        if (EventSystem.current == null || resumeButton == null) return;
 
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
    }

    private void Resume()
    {
        gameManager.PauseGame();
    }

    private void Options()
    {
        
    }

    private void ExitScene()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    private void OnDestroy()
    {
        if (gameManager != null)
            gameManager.onChangeGameState -= OnChangeGameStateCallback;
    }
    
}
