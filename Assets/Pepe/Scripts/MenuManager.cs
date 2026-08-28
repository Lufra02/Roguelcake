using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Canvas")]
    public GameObject MenuPrin;
    public GameObject MenuAjus;

    [Header("Nombre de la escena")]
    public string EscenaJuego;

    public void Jugar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(EscenaJuego);
    }

    public void AbrirAjusteSonido()
    {
        MenuPrin.SetActive(false);
        MenuAjus.SetActive(true);
    }

    public void RegresarMenuPrin()
    {
        MenuAjus.SetActive(false);
        MenuPrin.SetActive(true);
    }

    public void Salir()
    {
        Application.Quit();
        Debug.Log("Salir juego");
    }
}