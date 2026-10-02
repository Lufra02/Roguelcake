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
    private bool closeOnPurchase;
    private PlayerManager playerManager;

    public void Interact(GameObject interactor)
    {
        if (isOpen)
            CloseShop();
        else
            OpenShop(interactor, closeShopOnPurchase: false);
    }

    // Apertura de tienda desde subida de nivel
    public void OpenForLevelUp(GameObject interactor)
    {
        if (isOpen) return;
        OpenShop(interactor, closeShopOnPurchase: true);
    }

    private void OpenShop(GameObject interactor, bool closeShopOnPurchase)
    {
        isOpen = true;
        closeOnPurchase = closeShopOnPurchase;

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

    // Para cerrar la tienda despues de la toma de una carta
    private void HandleCardPurchased(ShopCardButton button)
    {
        if (closeOnPurchase)
        {
            CloseShop();
        }
    }

    // Solo reparte datos entre los botones que ya existen en la escena; no instancia nada.
    private void AssignCardsToButtons(PlayerStats playerStats)
    {
        if (shopCardButtons == null || shopCardButtons.Length == 0) return;

        List<ShopCard> selectedCards = GetRandomCards(shopCardButtons.Length, playerStats);

        for (int i = 0; i < shopCardButtons.Length; i++)
        {
            ShopCardButton button = shopCardButtons[i];
            if (button == null) continue;

            button.OnPurchased -= HandleCardPurchased;

            if (i < selectedCards.Count)
            {
                button.gameObject.SetActive(true);
                button.SetCard(selectedCards[i]);
                button.SetPlayer(playerStats);
                button.OnPurchased += HandleCardPurchased;
            }
            else
            {
                // Hay más botones que cartas disponibles: se ocultan los sobrantes
                button.gameObject.SetActive(false);
            }
        }
    }

    private List<ShopCard> GetRandomCards(int count, PlayerStats playerStats)
    {
        List<ShopCard> availableCards = new List<ShopCard>();
        foreach (ShopCard card in cardDefinitions)
        {
            if (card == null) continue;
            if (!IsCardAvailable(card, playerStats)) continue;

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
        return selectedCards;
    }

    // Una carta de rebote no debe aparecer si el jugador ya tiene la mejora explosiva activa, y viceversa.
    // Las cartas que no pertenecen a ninguna de las dos categorías (ambos bools en false) nunca se filtran.
    private bool IsCardAvailable(ShopCard card, PlayerStats playerStats)
    {
        if (playerStats == null) return true; // sin referencia al jugador no se puede filtrar, se muestra igual

        // Bloquea la categoría contraria por completo (incluida su carta de desbloqueo).
        if (card.ExplosiveUpgrade && playerStats.bouncyGun) return false;
        if (card.BounceUpgrade && playerStats.explosiveGun) return false;

        // Las mejoras dentro de una categoría (no la carta de desbloqueo en sí) solo aparecen
        // si esa categoría ya está activa. Evita, por ejemplo, ofrecer "+ daño de explosión"
        // antes de haber comprado "Explosive Bullet".
        if (card.ExplosiveUpgrade && !card.IsUnlockCard && !playerStats.explosiveGun) return false;
        if (card.BounceUpgrade && !card.IsUnlockCard && !playerStats.bouncyGun) return false;

        // La carta de desbloqueo ya no debe ofrecerse una vez que esa categoría quedó activa.
        if (card.IsUnlockCard && card.ExplosiveUpgrade && playerStats.explosiveGun) return false;
        if (card.IsUnlockCard && card.BounceUpgrade && playerStats.bouncyGun) return false;

        return true;
    }
}