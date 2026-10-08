using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button exitButton;

    private void Start()
    {
        // =========================
        // LISTENERS DE BOTONES
        // =========================
        if (playButton != null)
            playButton.onClick.AddListener(StartGame);

        if (optionsButton != null)
            optionsButton.onClick.AddListener(OpenOptions);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitScene);

        // =========================
        // NAVEGACIÓN Y FOCO INICIAL
        // =========================
        ConfigureNavigation();

        // Esperamos al final del frame para garantizar que el EventSystem esté listo
        StartCoroutine(SelectFirstButtonRoutine());
    }

    private void ConfigureNavigation()
    {
        SetAutoNavigation(playButton);
        SetAutoNavigation(optionsButton);
        SetAutoNavigation(exitButton);
    }

    private void SetAutoNavigation(Button button)
    {
        if (button == null) return;

        Navigation nav = button.navigation;
        nav.mode = Navigation.Mode.Automatic;
        button.navigation = nav;
    }

    private IEnumerator SelectFirstButtonRoutine()
    {
        // Espera un frame para que la jerarquía de UI y el EventSystem procesen el setup
        yield return new WaitForEndOfFrame();

        SelectFirstButton();
    }

    public void SelectFirstButton()
    {
        if (EventSystem.current == null || playButton == null)
            return;

        // Limpiar selección previa para forzar el resaltado visual
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(playButton.gameObject);
    }

    // =========================================================
    // ACCIONES DE BOTONES
    // =========================================================

    public void StartGame()
    {
        // Si usas el GameSceneManager que armamos antes, puedes llamarlo aquí:
        // GameSceneManager.Instance.LoadScene("Level_01");
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OpenOptions()
    {
        // Lógica o apertura de panel de opciones
    }

    public void ExitScene()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}