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

    [SerializeField] private GameObject selectedButtonHover;

    [Header("Death System")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private Button restartButton_Death;
    [SerializeField] private Button exitButton_Death;

    [SerializeField] private GameObject selectedButtonHover_Death;

    private void Start()
    {
        gameManager = GameManager.Instance;

        // Asignacion de navegacion
        ConfigureDeathNavigation();
        ConfigurePauseNavigation();

        // Botones de pausa
        resumeButton_Pause.onClick.AddListener(Resume);
        optionsButton_Pause.onClick.AddListener(Options);
        exitButton_Pause.onClick.AddListener(ExitScene);

        // Botones de muerte
        restartButton_Death.onClick.AddListener(Restart);
        exitButton_Death.onClick.AddListener(ExitScene);

        // Ocultar paneles al comenzar
        pausePanel.SetActive(false);
        deathPanel.SetActive(false);

        // Ocultar selectores
        if (selectedButtonHover != null)
            selectedButtonHover.SetActive(false);

        if (selectedButtonHover_Death != null)
            selectedButtonHover_Death.SetActive(false);

        if (gameManager != null)
        {
            gameManager.onChangeGameState += OnChangeGameStateCallback;
        }
    }

    private void Update()
    {
        if (EventSystem.current == null)
            return;

        // Actualizar selector del menú de pausa
        if (pausePanel != null && pausePanel.activeSelf)
        {
            UpdatePauseHoverPosition();
        }

        // Actualizar selector del menú de muerte
        if (deathPanel != null && deathPanel.activeSelf)
        {
            UpdateDeathHoverPosition();
        }
    }

    // =========================================================
    // DEATH UI
    // =========================================================

    private void ConfigureDeathNavigation()
    {
        Navigation restartNavigation = restartButton_Death.navigation;
        restartNavigation.mode = Navigation.Mode.Explicit;

        restartNavigation.selectOnDown = exitButton_Death;
        restartNavigation.selectOnUp = exitButton_Death;

        restartButton_Death.navigation = restartNavigation;


        Navigation exitNavigation = exitButton_Death.navigation;
        exitNavigation.mode = Navigation.Mode.Explicit;

        exitNavigation.selectOnDown = restartButton_Death;
        exitNavigation.selectOnUp = restartButton_Death;

        exitButton_Death.navigation = exitNavigation;
    }

    public void ShowDeathUI()
    {
        deathPanel.SetActive(true);

        // Ocultar selector de pausa
        if (selectedButtonHover != null)
            selectedButtonHover.SetActive(false);

        // Mostrar selector de muerte
        if (selectedButtonHover_Death != null)
            selectedButtonHover_Death.SetActive(true);

        // Seleccionar automáticamente Restart
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(
            restartButton_Death.gameObject
        );

        UpdateDeathHoverPosition();
    }

    private void UpdateDeathHoverPosition()
    {
        if (selectedButtonHover_Death == null)
            return;

        if (EventSystem.current == null)
            return;

        GameObject selected =
            EventSystem.current.currentSelectedGameObject;

        if (selected == null)
            return;

        selectedButtonHover_Death.transform.position =
            selected.transform.position;
    }

    // =========================================================
    // PAUSE UI
    // =========================================================

    private void ConfigurePauseNavigation()
    {
        Navigation resumeNavigation = resumeButton_Pause.navigation;
        resumeNavigation.mode = Navigation.Mode.Explicit;

        resumeNavigation.selectOnDown = optionsButton_Pause;
        resumeNavigation.selectOnUp = exitButton_Pause;

        resumeButton_Pause.navigation = resumeNavigation;


        Navigation optionsNavigation = optionsButton_Pause.navigation;
        optionsNavigation.mode = Navigation.Mode.Explicit;

        optionsNavigation.selectOnDown = exitButton_Pause;
        optionsNavigation.selectOnUp = resumeButton_Pause;

        optionsButton_Pause.navigation = optionsNavigation;


        Navigation exitNavigation = exitButton_Pause.navigation;
        exitNavigation.mode = Navigation.Mode.Explicit;

        exitNavigation.selectOnDown = resumeButton_Pause;
        exitNavigation.selectOnUp = optionsButton_Pause;

        exitButton_Pause.navigation = exitNavigation;
    }

    private void OnChangeGameStateCallback(GameState newState)
    {
        bool isPaused = newState == GameState.Pause;

        pausePanel.SetActive(isPaused);

        if (isPaused)
        {
            // Ocultar selector de muerte
            if (selectedButtonHover_Death != null)
                selectedButtonHover_Death.SetActive(false);

            // Mostrar selector de pausa
            if (selectedButtonHover != null)
                selectedButtonHover.SetActive(true);

            SelectFirstButton();
        }
        else
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            if (selectedButtonHover != null)
                selectedButtonHover.SetActive(false);
        }
    }

    private void SelectFirstButton()
    {
        if (EventSystem.current == null)
            return;

        if (resumeButton_Pause == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);

        EventSystem.current.SetSelectedGameObject(
            resumeButton_Pause.gameObject
        );

        UpdatePauseHoverPosition();
    }

    private void UpdatePauseHoverPosition()
    {
        if (selectedButtonHover == null)
            return;

        if (EventSystem.current == null)
            return;

        GameObject selected =
            EventSystem.current.currentSelectedGameObject;

        if (selected == null)
            return;

        selectedButtonHover.transform.position =
            selected.transform.position;
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