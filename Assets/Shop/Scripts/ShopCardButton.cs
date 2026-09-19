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
