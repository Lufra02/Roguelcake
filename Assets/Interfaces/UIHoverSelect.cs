using UnityEngine;
using UnityEngine.EventSystems;

// Colócalo en cualquier botón de UI (o en su mismo GameObject junto al Button) para que,
// al pasar el mouse por encima, también quede "seleccionado" en el EventSystem — exactamente
// el mismo estado que deja la navegación con teclado o gamepad. Así los tres métodos de
// input comparten un solo concepto de "botón actualmente seleccionado".
public class UIHoverSelect : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem.current?.SetSelectedGameObject(gameObject);
    }
}