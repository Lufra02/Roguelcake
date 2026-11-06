using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Colócalo en el botón de una tarjeta de la tienda y conecta sus referencias de UI.
public class ShopCardButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private ShopCard card;
    private Button purchaseButton;

    [Header("UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image imageDisplay;
    [SerializeField] private TMP_Text descriptionText;


    [Header("Settings")]
    [SerializeField] public Sprite offHover;
    [SerializeField] public Sprite onHover;

    [SerializeField] private Image backGround;


    [SerializeField] public GameObject hoverIconGO;

    [Header("Game feel")]
    [SerializeField, Min(1f)] private float highlightedWidthScale = 1.06f;
    [SerializeField, Min(1f)] private float highlightedHeightScale = 1.025f;
    [SerializeField, Min(1f)] private float scaleSpringStrength = 280f;
    [SerializeField, Min(0f)] private float scaleSpringDamping = 26f;
    [SerializeField, Min(0f)] private float entryBounce = 6f;

    private bool isPointerOver;
    private bool isSelected;
    private bool wasHighlighted;
    private RectTransform cardTransform;
    private Vector3 baseScale;
    private Vector3 scaleVelocity;

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
        cardTransform = transform as RectTransform;
        if (cardTransform != null)
            baseScale = cardTransform.localScale;

        if (purchaseButton == null)
            purchaseButton = GetComponent<Button>();

        if (backGround == null && purchaseButton != null)
            backGround = purchaseButton.targetGraphic as Image;

        if (purchaseButton != null)
            purchaseButton.onClick.AddListener(Purchase);

        isPointerOver = false;
        isSelected = false;

        RefreshView();
        RefreshSelectionVisual();
    }

    private void Update()
    {
        if (cardTransform == null)
            return;

        bool highlighted = isSelected || isPointerOver;
        Vector3 targetScale = highlighted
            ? new Vector3(baseScale.x * highlightedWidthScale, baseScale.y * highlightedHeightScale, baseScale.z)
            : baseScale;

        if (highlighted && !wasHighlighted)
        {
            Vector3 scaleDifference = targetScale - baseScale;
            scaleVelocity += scaleDifference * entryBounce;
        }

        wasHighlighted = highlighted;

        float deltaTime = Time.unscaledDeltaTime;
        Vector3 springForce = (targetScale - cardTransform.localScale) * scaleSpringStrength;
        Vector3 dampingForce = scaleVelocity * scaleSpringDamping;
        scaleVelocity += (springForce - dampingForce) * deltaTime;
        cardTransform.localScale += scaleVelocity * deltaTime;
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
        isSelected = selected;
        RefreshSelectionVisual();
    }

    // Requerido por IPointerEnterHandler: Unity lo llama automáticamente cuando el cursor
    // entra en el área de este botón (necesita un Canvas con GraphicRaycaster y un EventSystem).
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        RefreshSelectionVisual();
        OnHoverEnter?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        RefreshSelectionVisual();
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
        if (descriptionText != null) descriptionText.text = card.Description;

        if (imageDisplay != null)
        {
            imageDisplay.sprite = card.Image;
            imageDisplay.enabled = card.Image != null;
        }

        if (purchaseButton != null)
            purchaseButton.interactable = !purchased;
    }

    private void RefreshSelectionVisual()
    {
        bool highlighted = isSelected || isPointerOver;

        if (backGround != null)
        {
            Sprite targetSprite = highlighted ? onHover : offHover;
            if (targetSprite != null)
                backGround.sprite = targetSprite;
        }

        if (hoverIconGO != null)
            hoverIconGO.SetActive(highlighted);
    }
}
