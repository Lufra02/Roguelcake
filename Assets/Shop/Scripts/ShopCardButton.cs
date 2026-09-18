using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Colócalo en el botón de una tarjeta de la tienda y conecta sus referencias de UI.
public class ShopCardButton : MonoBehaviour
{
    [SerializeField] private ShopCard card;

    [Header("UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image imageDisplay;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button purchaseButton;

    private PlayerStats playerStats;
    private bool purchased;

    private void Awake()
    {
        if (purchaseButton == null)
            purchaseButton = GetComponent<Button>();

        RefreshView();
    }

    public void SetPlayer(PlayerStats stats)
    {
        playerStats = stats;
    }

    public void Configure(ShopCard assignedCard, TMP_Text title, Image image, TMP_Text cost, TMP_Text description, Button button)
    {
        card = assignedCard;
        titleText = title;
        imageDisplay = image;
        costText = cost;
        descriptionText = description;
        purchaseButton = button;
        RefreshView();
    }

    public void SetCard(ShopCard assignedCard)
    {
        card = assignedCard;
        purchased = false;
        RefreshView();
    }

    public void ApplyColors(Color normalColor, Color disabledColor)
    {
        if (purchaseButton == null)
            purchaseButton = GetComponent<Button>();

        Image background = GetComponent<Image>();
        if (background != null)
            background.color = normalColor;

        if (purchaseButton == null) return;

        ColorBlock buttonColors = purchaseButton.colors;
        buttonColors.normalColor = normalColor;
        buttonColors.highlightedColor = Color.Lerp(normalColor, Color.white, 0.18f);
        buttonColors.pressedColor = Color.Lerp(normalColor, Color.black, 0.2f);
        buttonColors.selectedColor = buttonColors.highlightedColor;
        buttonColors.disabledColor = disabledColor;
        buttonColors.colorMultiplier = 1f;
        purchaseButton.colors = buttonColors;
    }

    // Asígnalo al evento OnClick del botón de compra.
    public void Purchase()
    {
        if (purchased || card == null || playerStats == null) return;

        purchased = card.ApplyTo(playerStats);
        RefreshView();
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
