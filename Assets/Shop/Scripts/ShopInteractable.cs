using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Colócalo en el GameObject de la "Tienda", junto con un Collider (puede ser trigger o no,
// solo se usa para que PlayerInteraction lo detecte por OverlapSphere) en la capa "Interactable".
public class ShopInteractable : MonoBehaviour, IInteractable
{

    [Header("UI")]
    public GameObject shopCanvas; // Canvas de la tienda a mostrar/ocultar

    [Header("Referencias del jugador (se autocompletan si se dejan vacías)")]
    public PlayerController playerController;
    public PlayerCombat playerCombat;
    [SerializeField] private ShopCardButton[] cardButtons;
    [Tooltip("Las tarjetas se muestran de izquierda a derecha. Solo se renderizan las primeras tres.")]
    [SerializeField] private ShopCard[] cardDefinitions;
    [Tooltip("Prefab opcional que define el diseño de todas las tarjetas. Si se deja vacío, se usa el diseño generado por código.")]
    [SerializeField] private ShopCardButton cardViewPrefab;

    [Header("Aspecto de tarjetas")]
    [SerializeField] private Color cardColor = new Color(0.26f, 0.12f, 0.33f, 1f);
    [SerializeField] private Color disabledCardColor = new Color(0.16f, 0.16f, 0.16f, 1f);
    [SerializeField, Min(80f)] private float cardHeight = 170f;
    [SerializeField, Min(0f)] private float cardSpacing = 16f;
    [SerializeField, Min(0f)] private float panelPadding = 24f;

    private bool isOpen = false;
    private Transform cardContainer;
    private PlayerManager playerManager;

    public void Interact(GameObject interactor)
    {
        if (isOpen)
        {
            CloseShop();
        }
        else
        {
            OpenShop(interactor);
        }
    }

    void OpenShop(GameObject interactor)
    {
        isOpen = true;

        if (playerController == null) playerController = interactor.GetComponent<PlayerController>();
        if (playerCombat == null) playerCombat = interactor.GetComponent<PlayerCombat>();
        playerManager = interactor.GetComponent<PlayerManager>();
        playerManager?.SetOpenShop(this);

        PlayerStats playerStats = interactor.GetComponent<PlayerStats>();
        CreateCardViews();

        foreach (ShopCardButton cardButton in cardButtons)
            cardButton?.SetPlayer(playerStats);

        playerController?.SetMovementEnabled(false);
        playerCombat?.SetCombatEnabled(false);

        if (shopCanvas != null) shopCanvas.SetActive(true);
    }

    // Llama a este método desde el botón "Cerrar" del Canvas de la tienda (OnClick en el Inspector).
    public void CloseShop()
    {
        isOpen = false;
        playerManager?.ClearOpenShop(this);

        if (shopCanvas != null) shopCanvas.SetActive(false);

        playerController?.SetMovementEnabled(true);
        playerCombat?.SetCombatEnabled(true);
    }

    // Genera una presentación básica para las tarjetas asignadas. Más adelante puedes
    // sustituirla por prefabs sin cambiar los assets ShopCard ni sus efectos.
    private void CreateCardViews()
    {
        ClearCardViews();
        if (shopCanvas == null || cardDefinitions == null || cardDefinitions.Length == 0) return;

        List<ShopCard> selectedCards = GetRandomCards();
        int visibleCardCount = selectedCards.Count;
        cardButtons = new ShopCardButton[visibleCardCount];

        GameObject containerObject = new GameObject("Card Container", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        containerObject.transform.SetParent(shopCanvas.transform, false);
        cardContainer = containerObject.transform;

        RectTransform containerTransform = containerObject.GetComponent<RectTransform>();
        containerTransform.anchorMin = Vector2.zero;
        containerTransform.anchorMax = Vector2.one;
        containerTransform.offsetMin = new Vector2(panelPadding, panelPadding);
        containerTransform.offsetMax = new Vector2(-panelPadding, -85f);

        HorizontalLayoutGroup layout = containerObject.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = cardSpacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        for (int index = 0; index < visibleCardCount; index++)
        {
            ShopCard card = selectedCards[index];

            ShopCardButton cardButton;
            if (cardViewPrefab != null)
            {
                cardButton = Instantiate(cardViewPrefab, containerObject.transform);
                cardButton.name = $"Card - {card.Title}";
                cardButton.SetCard(card);
            }
            else
            {
                GameObject cardObject = new GameObject($"Card - {card.Title}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(ShopCardButton));
                cardObject.transform.SetParent(containerObject.transform, false);

                Button button = cardObject.GetComponent<Button>();
                cardButton = cardObject.GetComponent<ShopCardButton>();
                TMP_Text title = CreateText("Title", cardObject.transform, new Vector2(0f, 55f), new Vector2(270f, 35f), 24, TextAlignmentOptions.Center);
                Image icon = CreateImage("Image", cardObject.transform, new Vector2(-108f, 2f), new Vector2(52f, 52f));
                TMP_Text description = CreateText("Description", cardObject.transform, new Vector2(18f, 5f), new Vector2(220f, 55f), 16, TextAlignmentOptions.Center);
                TMP_Text cost = CreateText("Cost", cardObject.transform, new Vector2(0f, -60f), new Vector2(270f, 30f), 20, TextAlignmentOptions.Center);

                cardButton.Configure(card, title, icon, cost, description, button);
            }

            LayoutElement layoutElement = cardButton.GetComponent<LayoutElement>();
            if (layoutElement == null)
                layoutElement = cardButton.gameObject.AddComponent<LayoutElement>();

            layoutElement.preferredHeight = cardHeight;
            layoutElement.flexibleWidth = 1f;
            cardButton.ApplyColors(cardColor, disabledCardColor);
            Button purchaseButton = cardButton.GetComponent<Button>();
            if (purchaseButton != null)
                purchaseButton.onClick.AddListener(cardButton.Purchase);
            cardButtons[index] = cardButton;
        }
    }

    private List<ShopCard> GetRandomCards()
    {
        List<ShopCard> availableCards = new List<ShopCard>();
        foreach (ShopCard card in cardDefinitions)
        {
            if (card != null)
                availableCards.Add(card);
        }

        int cardCount = Mathf.Min(3, availableCards.Count);
        List<ShopCard> selectedCards = new List<ShopCard>(cardCount);

        // Extrae una tarjeta aleatoria y la elimina de la lista para no repetirla.
        for (int index = 0; index < cardCount; index++)
        {
            int randomIndex = Random.Range(0, availableCards.Count);
            selectedCards.Add(availableCards[randomIndex]);
            availableCards.RemoveAt(randomIndex);
        }

        return selectedCards;
    }

    private void ClearCardViews()
    {
        if (cardContainer != null)
            Destroy(cardContainer.gameObject);

        cardContainer = null;
        cardButtons = System.Array.Empty<ShopCardButton>();
    }

    private static TMP_Text CreateText(string objectName, Transform parent, Vector2 position, Vector2 size, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform textTransform = textObject.GetComponent<RectTransform>();
        textTransform.anchorMin = new Vector2(0.5f, 0.5f);
        textTransform.anchorMax = new Vector2(0.5f, 0.5f);
        textTransform.anchoredPosition = position;
        textTransform.sizeDelta = size;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        text.enableWordWrapping = true;
        return text;
    }

    private static Image CreateImage(string objectName, Transform parent, Vector2 position, Vector2 size)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform imageTransform = imageObject.GetComponent<RectTransform>();
        imageTransform.anchorMin = new Vector2(0.5f, 0.5f);
        imageTransform.anchorMax = new Vector2(0.5f, 0.5f);
        imageTransform.anchoredPosition = position;
        imageTransform.sizeDelta = size;

        Image image = imageObject.GetComponent<Image>();
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

}
