using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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

    [Header("Control")]
    [SerializeField] private float navigationCooldown = 0.2f;

    private int selectedCardIndex = 0;
    private float navigationTimer = 0f;

    public void Interact(GameObject interactor)
    {
        if (isOpen)
            CloseShop();
        else
            OpenShop(interactor, closeShopOnPurchase: false);
    }

    private void Update()
    {
        if (!isOpen)
            return;

        HandleNavigationInput();
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

        playerController = interactor.GetComponent<PlayerController>();
        playerCombat = interactor.GetComponent<PlayerCombat>();
        playerManager = interactor.GetComponent<PlayerManager>();
        playerManager?.SetOpenShop(this);
        
        // Debajo de esto
        AssignCardsToButtons(interactor.GetComponent<PlayerStats>());
        playerController?.SetMovementEnabled(false);
        playerCombat?.SetCombatEnabled(false);

        if (shopCanvas != null)
            shopCanvas.SetActive(true);

        UpdateCardSelection();
    }

    // Llama a este método desde el botón de cerrar de la interfaz.
    public void CloseShop()
    {
        isOpen = false;
        playerManager?.ClearOpenShop(this);

        if (shopCanvas != null)
            shopCanvas.SetActive(false);

        ClearCardSelection();

        playerController?.SetMovementEnabled(true);
        playerCombat?.SetCombatEnabled(true);
    }

    // Administra el movimiento del control
    private void HandleNavigationInput()
    {
        Gamepad gamepad = Gamepad.current;
        Keyboard keyboard = Keyboard.current;

        // Comprar carta seleccionada con A/Cross, Enter o Espacio.
        bool submitPressed = gamepad != null && gamepad.buttonSouth.wasPressedThisFrame;
        if (keyboard != null)
            submitPressed |= keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame;

        if (submitPressed)
        {
            PurchaseSelectedCard();
            return;
        }

        if (navigationTimer > 0f)
        {
            navigationTimer -= Time.unscaledDeltaTime;
            return;
        }

        bool moveLeft = false;
        bool moveRight = false;

        if (gamepad != null)
        {
            Vector2 navigation = gamepad.leftStick.ReadValue();
            moveLeft |= navigation.x < -0.5f || gamepad.dpad.left.isPressed;
            moveRight |= navigation.x > 0.5f || gamepad.dpad.right.isPressed;
        }

        // Navegación horizontal con flechas o A/D.
        if (keyboard != null)
        {
            moveLeft |= keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame;
            moveRight |= keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame;
        }

        if (moveRight)
        {
            SelectNextCard();
            navigationTimer = navigationCooldown;
        }
        else if (moveLeft)
        {
            SelectPreviousCard();
            navigationTimer = navigationCooldown;
        }
    }

    private void SelectNextCard()
    {
        int activeCards = GetActiveCardCount();

        if (activeCards == 0)
            return;

        selectedCardIndex++;

        if (selectedCardIndex >= activeCards)
            selectedCardIndex = 0;

        UpdateCardSelection();
    }

    private void SelectPreviousCard()
    {
        int activeCards = GetActiveCardCount();

        if (activeCards == 0)
            return;

        selectedCardIndex--;

        if (selectedCardIndex < 0)
            selectedCardIndex = activeCards - 1;

        UpdateCardSelection();
    }

    private int GetActiveCardCount()
    {
        int count = 0;

        foreach (ShopCardButton cardButton in shopCardButtons)
        {
            if (cardButton != null && cardButton.gameObject.activeSelf)
                count++;
        }

        return count;
    }

    private void UpdateCardSelection()
    {
        int activeIndex = 0;
        GameObject selectedGameObject = null;

        for (int i = 0; i < shopCardButtons.Length; i++)
        {
            ShopCardButton button = shopCardButtons[i];

            if (button == null || !button.gameObject.activeSelf)
                continue;

            bool isSelected = activeIndex == selectedCardIndex;
            button.SetSelected(isSelected);
            if (isSelected)
                selectedGameObject = button.gameObject;

            activeIndex++;
        }

        if (selectedGameObject != null && shopCanvas != null && shopCanvas.activeInHierarchy && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(selectedGameObject);
    }

    private void ClearCardSelection()
    {
        if (shopCardButtons != null)
        {
            foreach (ShopCardButton button in shopCardButtons)
            {
                if (button != null)
                    button.SetSelected(false);
            }
        }

        if (EventSystem.current != null && shopCanvas != null &&
            EventSystem.current.currentSelectedGameObject != null &&
            EventSystem.current.currentSelectedGameObject.transform.IsChildOf(shopCanvas.transform))
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    // Para cerrar la tienda despues de la toma de una carta
    private void HandleCardPurchased(ShopCardButton button)
    {
        if (closeOnPurchase)
        {
            CloseShop();
        }
    }

    // El mouse entró en una carta: la trata como la selección actual, igual que haría
    // el gamepad al navegar hasta ella. Así ambos métodos de input comparten un solo estado.
    private void HandleCardHovered(ShopCardButton hoveredButton)
    {
        int activeIndex = 0;

        for (int i = 0; i < shopCardButtons.Length; i++)
        {
            ShopCardButton button = shopCardButtons[i];

            if (button == null || !button.gameObject.activeSelf)
                continue;

            if (button == hoveredButton)
            {
                selectedCardIndex = activeIndex;
                UpdateCardSelection();
                return;
            }

            activeIndex++;
        }
    }

    private void PurchaseSelectedCard()
    {
        int activeIndex = 0;

        for (int i = 0; i < shopCardButtons.Length; i++)
        {
            ShopCardButton button = shopCardButtons[i];

            if (button == null || !button.gameObject.activeSelf)
                continue;

            if (activeIndex == selectedCardIndex)
            {
                button.Purchase();
                return;
            }

            activeIndex++;
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
            button.OnHoverEnter -= HandleCardHovered;

            if (i < selectedCards.Count)
            {
                button.gameObject.SetActive(true);
                Button uiButton = button.GetComponent<Button>();
                if (uiButton != null)
                {
                    Navigation navigation = uiButton.navigation;
                    navigation.mode = Navigation.Mode.None;
                    uiButton.navigation = navigation;
                }

                button.SetCard(selectedCards[i]);
                button.SetPlayer(playerStats);
                button.OnPurchased += HandleCardPurchased;
                button.OnHoverEnter += HandleCardHovered;
            }
            else
            {
                // Hay más botones que cartas disponibles: se ocultan los sobrantes
                button.gameObject.SetActive(false);
            }
        }

        selectedCardIndex = 0;
        UpdateCardSelection();

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
       
        if (playerStats == null)
        return true;

        // 
        if(playerStats.maxHealth >= playerStats.MaxHealthAllowed && card.effect == ShopCardEffect.MaxHealth)
        {
            return false;
        }

        // =========================================================
        // BLOQUEO ENTRE CATEGORÍAS
        // =========================================================

        // Si tienes REBOUND, no aparecen EXPLOSIVE ni HUGE
        if (card.ExplosiveUpgrade && playerStats.bouncyGun)
            return false;

        if (card.HugeBulletUpgrade && playerStats.bouncyGun)
            return false;


        // Si tienes EXPLOSIVE, no aparecen REBOUND ni HUGE
        if (card.BounceUpgrade && playerStats.explosiveGun)
            return false;

        if (card.HugeBulletUpgrade && playerStats.explosiveGun)
            return false;


        // Si tienes HUGE BULLET, no aparecen REBOUND ni EXPLOSIVE
        if (card.BounceUpgrade && playerStats.hugeGun)
            return false;

        if (card.ExplosiveUpgrade && playerStats.hugeGun)
            return false;


        // =========================================================
        // LAS MEJORAS NORMALES REQUIEREN TENER DESBLOQUEADA
        // SU CATEGORÍA
        // =========================================================

        // Mejoras EXPLOSIVE
        if (card.ExplosiveUpgrade &&
            !card.IsUnlockCard &&
            !playerStats.explosiveGun)
            return false;


        // Mejoras REBOUND
        if (card.BounceUpgrade &&
            !card.IsUnlockCard &&
            !playerStats.bouncyGun)
            return false;


        // Mejoras HUGE BULLET
        if (card.HugeBulletUpgrade &&
            !card.IsUnlockCard &&
            !playerStats.hugeGun)
            return false;


        // =========================================================
        // LAS CARTAS DE DESBLOQUEO DESAPARECEN UNA VEZ ACTIVADAS
        // =========================================================

        if (card.IsUnlockCard &&
            card.ExplosiveUpgrade &&
            playerStats.explosiveGun)
            return false;


        if (card.IsUnlockCard &&
            card.BounceUpgrade &&
            playerStats.bouncyGun)
            return false;


        if (card.IsUnlockCard &&
            card.HugeBulletUpgrade &&
            playerStats.hugeGun)
            return false;

        return true;
    }
}
