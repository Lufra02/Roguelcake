using System.Collections.Generic;
using UnityEngine;

// Colócalo en el GameObject de la tienda, junto con un Collider en la capa Interactable.
public class ShopInteractable : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "Presiona E para abrir la tienda";

    [Header("UI")]
    [SerializeField] private GameObject shopCanvas;
    [Tooltip("Objeto padre donde se instancian las tarjetas. Su layout se configura desde el editor.")]
    [SerializeField] private Transform cardContainer;
    [SerializeField] private ShopCardButton cardViewPrefab;

    [Header("Tarjetas disponibles")]
    [SerializeField] private ShopCard[] cardDefinitions;

    [Header("Referencias del jugador")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerCombat playerCombat;

    private readonly List<ShopCardButton> activeCardViews = new List<ShopCardButton>();
    private bool isOpen;
    private PlayerManager playerManager;

    public void Interact(GameObject interactor)
    {
        if (isOpen)
            CloseShop();
        else
            OpenShop(interactor);
    }

    private void OpenShop(GameObject interactor)
    {
        isOpen = true;
        playerController ??= interactor.GetComponent<PlayerController>();
        playerCombat ??= interactor.GetComponent<PlayerCombat>();
        playerManager = interactor.GetComponent<PlayerManager>();
        playerManager?.SetOpenShop(this);

        CreateCardViews(interactor.GetComponent<PlayerStats>());
        playerController?.SetMovementEnabled(false);
        playerCombat?.SetCombatEnabled(false);

        if (shopCanvas != null)
            shopCanvas.SetActive(true);
    }

    // Llama a este método desde el botón de cerrar de la interfaz.
    public void CloseShop()
    {
        isOpen = false;
        playerManager?.ClearOpenShop(this);

        if (shopCanvas != null)
            shopCanvas.SetActive(false);

        playerController?.SetMovementEnabled(true);
        playerCombat?.SetCombatEnabled(true);
    }

    // Solo selecciona datos e instancia el prefab: el diseño pertenece al prefab y al contenedor.
    private void CreateCardViews(PlayerStats playerStats)
    {
        ClearCardViews();

        if (cardContainer == null || cardViewPrefab == null || cardDefinitions == null)
        {
            Debug.LogWarning("La tienda necesita Card Container, Card View Prefab y Card Definitions.", this);
            return;
        }

        foreach (ShopCard card in GetRandomCards())
        {
            ShopCardButton cardView = Instantiate(cardViewPrefab, cardContainer);
            cardView.name = $"Card - {card.Title}";
            cardView.SetCard(card);
            cardView.SetPlayer(playerStats);
            activeCardViews.Add(cardView);
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
        foreach (ShopCardButton cardView in activeCardViews)
        {
            if (cardView != null)
                Destroy(cardView.gameObject);
        }

        activeCardViews.Clear();
    }
}
