using UnityEngine;
using UnityEngine.UI;

public class Prueba : MonoBehaviour
{
    public AudioSource musica;
    public AudioSource efecto;

    public Slider sliderMusica;
    public Slider sliderEfecto;

    void Start()
    {
        sliderMusica.value = musica.volume;
        sliderEfecto.value = efecto.volume;

        sliderMusica.onValueChanged.AddListener(CambiarMusica);
        sliderEfecto.onValueChanged.AddListener(CambiarEfectos);
    }

    public void CambiarMusica(float valor)
    {
        musica.volume = valor;
    }

    public void CambiarEfectos(float valor)
    {
        efecto.volume = valor;
    }
}