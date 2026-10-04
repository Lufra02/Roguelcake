using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Colócalo en el botón de una tarjeta de la tienda y conecta sus referencias de UI.
public class ShopCardButton : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private ShopCard card;

    [Header("UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image imageDisplay;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button purchaseButton;

    [SerializeField] private GameObject selectionHighlight;

    private PlayerStats playerStats;
    private bool purchased;

    // Se dispara cuando ESTA carta se compra con éxito.
    // La tienda lo usa para, por ejemplo, cerrarse sola tras una recompensa de nivel.
    public event Action<ShopCardButton> OnPurchased;

    // Se dispara cuando el mouse entra en esta carta. La tienda lo usa para mover el
    // resaltado al mismo lugar que usa la navegación con gamepad, sin duplicar lógica.
    public event Action<ShopCardButton> OnHoverEnter;

    private void Awake()
    {
        if (purchaseButton == null)
            purchaseButton = GetComponent<Button>();

        if (purchaseButton != null)
            purchaseButton.onClick.AddListener(Purchase);

        RefreshView();
    }

    public void SetPlayer(PlayerStats stats)
    {
        playerStats = stats;
    }

    public void SetCard(ShopCard assignedCard)
    {
        card = assignedCard;
        purchased = false;
        RefreshView();
    }

    public void SetSelected(bool selected)
    {
        if (selectionHighlight != null)
            selectionHighlight.SetActive(selected);
    }

    // Requerido por IPointerEnterHandler: Unity lo llama automáticamente cuando el cursor
    // entra en el área de este botón (necesita un Canvas con GraphicRaycaster y un EventSystem).
    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHoverEnter?.Invoke(this);
    }

    public void Purchase()
    {
        if (purchased || card == null || playerStats == null) return;

        purchased = card.ApplyTo(playerStats);
        RefreshView();

        if (purchased)
        {
            OnPurchased?.Invoke(this);
        }
    }

    private void RefreshView()
    {
        if (card == null) return;

        if (titleText != null) titleText.text = card.Title;
        if (costText != null) costText.text = card.Cost.ToString();
        if (descriptionText != null) descriptionText.text = card.Description;

        if (imageDisplay != null)
        {
            imageDisplay.sprite = card.Image;
            imageDisplay.enabled = card.Image != null;
        }

        if (purchaseButton != null)
            purchaseButton.interactable = !purchased;
    }
}