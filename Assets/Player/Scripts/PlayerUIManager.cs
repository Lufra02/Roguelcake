using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    private GameManager gameManager;

    [Header("Pause System")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton_Pause;
    [SerializeField] private Button optionsButton_Pause;
    [SerializeField] private Button exitButton_Pause;

    // Se conserva por si quieres utilizarlo en el futuro
    [SerializeField] private GameObject selectedButtonHover;

    [Header("Death System")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private Button restartButton_Death;
    [SerializeField] private Button exitButton_Death;

    // Se conserva por si quieres utilizarlo en el futuro
    [SerializeField] private GameObject selectedButtonHover_Death;


    private void Start()
    {
        gameManager = GameManager.Instance;

        // Configurar navegación
        // ConfigureDeathNavigation();
        // ConfigurePauseNavigation();

        // =========================
        // BOTONES DE PAUSA
        // =========================

        resumeButton_Pause.onClick.AddListener(Resume);
        optionsButton_Pause.onClick.AddListener(Options);
        exitButton_Pause.onClick.AddListener(ExitScene);

        // =========================
        // BOTONES DE MUERTE
        // =========================

        restartButton_Death.onClick.AddListener(Restart);
        exitButton_Death.onClick.AddListener(ExitScene);

        // =========================
        // OCULTAR PANELES
        // =========================

        pausePanel.SetActive(false);
        deathPanel.SetActive(false);

        // =========================
        // SELECTORES
        // =========================

        // Los conservamos para futuro uso,
        // pero actualmente no se utilizan.
        if (selectedButtonHover != null)
            selectedButtonHover.SetActive(false);

        if (selectedButtonHover_Death != null)
            selectedButtonHover_Death.SetActive(false);

        // =========================
        // GAME MANAGER
        // =========================

        if (gameManager != null)
        {
            gameManager.onChangeGameState += OnChangeGameStateCallback;
        }
    }


    // =========================================================
    // DEATH UI
    // =========================================================

    private void ConfigureDeathNavigation()
    {
        Navigation restartNavigation = restartButton_Death.navigation;
        restartNavigation.mode = Navigation.Mode.Automatic;

        restartButton_Death.navigation = restartNavigation;


        Navigation exitNavigation = exitButton_Death.navigation;
        exitNavigation.mode = Navigation.Mode.Automatic;

        exitButton_Death.navigation = exitNavigation;
    }


    public void ShowDeathUI()
    {
        deathPanel.SetActive(true);

        if (selectedButtonHover != null)
            selectedButtonHover.SetActive(false);

        if (selectedButtonHover_Death != null)
            selectedButtonHover_Death.SetActive(false);

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(
                restartButton_Death.gameObject
            );
        }
    }


    // =========================================================
    // PAUSE UI
    // =========================================================

    private void ConfigurePauseNavigation()
    {

        Navigation resumeNavigation = resumeButton_Pause.navigation;
        resumeNavigation.mode = Navigation.Mode.Automatic;

        resumeButton_Pause.navigation = resumeNavigation;


        Navigation optionsNavigation = optionsButton_Pause.navigation;
        optionsNavigation.mode = Navigation.Mode.Automatic;

        optionsButton_Pause.navigation = optionsNavigation;


        Navigation exitNavigation = exitButton_Pause.navigation;
        exitNavigation.mode = Navigation.Mode.Automatic;

        exitButton_Pause.navigation = exitNavigation;
    }


    private void OnChangeGameStateCallback(GameState newState)
    {
        bool isPaused = newState == GameState.Pause;

        if (isPaused)
        {
            pausePanel.SetActive(true);

            // No utilizamos los selectores
            if (selectedButtonHover != null)
                selectedButtonHover.SetActive(false);

            if (selectedButtonHover_Death != null)
                selectedButtonHover_Death.SetActive(false);

            SelectFirstButton();
        }
        else
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            pausePanel.SetActive(false);

            if (selectedButtonHover != null)
                selectedButtonHover.SetActive(false);

            if (selectedButtonHover_Death != null)
                selectedButtonHover_Death.SetActive(false);
        }
    }


    private void SelectFirstButton()
    {
        if (EventSystem.current == null)
            return;

        if (resumeButton_Pause == null)
            return;

        EventSystem.current.SetSelectedGameObject(
            resumeButton_Pause.gameObject
        );
    }


    // =========================================================
    // BUTTONS
    // =========================================================

    private void Resume()
    {
        if (gameManager != null)
        {
            gameManager.PauseGame();
        }
    }


    private void Restart()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
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


    // =========================================================
    // CLEANUP
    // =========================================================

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.onChangeGameState -=
                OnChangeGameStateCallback;
        }
    }
}