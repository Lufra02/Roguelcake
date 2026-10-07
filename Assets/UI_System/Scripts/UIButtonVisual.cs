using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonVisual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Sprites")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;

    private Image buttonImage;

    private void Awake()
    {
        buttonImage = GetComponent<Image>();

        if (buttonImage != null)
        {
            buttonImage.sprite = normalSprite;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Si el botón sigue seleccionado por mando/teclado,
        // mantenemos el sprite de hover.
        if (EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject == gameObject)
        {
            SetHover();
        }
        else
        {
            SetNormal();
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        SetHover();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        SetNormal();
    }

    private void SetHover()
    {
        if (buttonImage != null && hoverSprite != null)
        {
            buttonImage.sprite = hoverSprite;
        }
    }

    private void SetNormal()
    {
        if (buttonImage != null && normalSprite != null)
        {
            buttonImage.sprite = normalSprite;
        }
    }

    private void OnEnable()
    {
        SetNormal();
    }
}