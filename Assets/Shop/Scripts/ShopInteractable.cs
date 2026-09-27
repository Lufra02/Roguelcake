using System.Collections.Generic;
using UnityEngine;

// Colócalo en el GameObject de la tienda, junto con un Collider en la capa Interactable.
public class ShopInteractable : MonoBehaviour, IInteractable
{

    [Header("UI")]
    [SerializeField] private GameObject shopCanvas;
    [Tooltip("Lista de los objetos de las tarjetas")]
    [SerializeField] private ShopCardButton[] shopCardButtons;
    
    [Header("Tarjetas disponibles")]
    [SerializeField] private ShopCard[] cardDefinitions;

    [Header("Referencias del jugador")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerCombat playerCombat;
    
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

        AssignCardsToButtons(interactor.GetComponent<PlayerStats>());
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
    
    // Solo reparte datos entre los botones que ya existen en la escena; no instancia nada.
    private void AssignCardsToButtons(PlayerStats playerStats)
    {
        if (shopCardButtons == null || shopCardButtons.Length == 0) return;
 
        List<ShopCard> selectedCards = GetRandomCards(shopCardButtons.Length);
 
        for (int i = 0; i < shopCardButtons.Length; i++)
        {
            ShopCardButton button = shopCardButtons[i];
            if (button == null) continue;
 
            if (i < selectedCards.Count)
            {
                button.gameObject.SetActive(true);
                button.SetCard(selectedCards[i]);
                button.SetPlayer(playerStats);
            }
            else
            {
                // Hay más botones que cartas disponibles: se ocultan los sobrantes
                button.gameObject.SetActive(false);
            }
        }
    }

    private List<ShopCard> GetRandomCards(int count)
    {
        List<ShopCard> availableCards = new List<ShopCard>();
        foreach (ShopCard card in cardDefinitions)
        {
            if (card != null)
                availableCards.Add(card);
        }
 
        int cardCount = Mathf.Min(count, availableCards.Count);
        List<ShopCard> selectedCards = new List<ShopCard>(cardCount);
 
        for (int index = 0; index < cardCount; index++)
        {
            int randomIndex = Random.Range(0, availableCards.Count);
            selectedCards.Add(availableCards[randomIndex]);
            availableCards.RemoveAt(randomIndex);
        }
 
        print(selectedCards);
        return selectedCards;
    }

    
}
