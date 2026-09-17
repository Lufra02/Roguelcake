using UnityEngine;

// Colócalo en el GameObject de la "Tienda", junto con un Collider (puede ser trigger o no,
// solo se usa para que PlayerInteraction lo detecte por OverlapSphere) en la capa "Interactable".
public class ShopInteractable : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "Presiona E para abrir la tienda";

    [Header("UI")]
    public GameObject shopCanvas; // Canvas de la tienda a mostrar/ocultar

    [Header("Referencias del jugador (se autocompletan si se dejan vacías)")]
    public PlayerController playerController;
    public PlayerCombat playerCombat;

    private bool isOpen = false;
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

}
