using UnityEngine;
using UnityEngine.InputSystem;

// Detecta el IInteractable más cercano dentro de un radio y permite interactuar con la tecla E.
// Colócalo en el mismo GameObject que PlayerController.
public class PlayerInteraction : MonoBehaviour
{
    [Header("Detección")]
    public float interactionRadius = 2.5f;
    public LayerMask interactableLayer;

    [Header("UI (opcional)")]
    [Tooltip("GameObject de UI que se activa/desactiva automáticamente cuando hay algo interactuable cerca (ej. un ícono o texto 'Presiona E').")]
    public GameObject interactionPromptUI;

    private IInteractable currentInteractable;

    void Update()
    {
        FindClosestInteractable();

        if (currentInteractable != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            currentInteractable.Interact(gameObject);
        }
    }

    void FindClosestInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius, interactableLayer);

        IInteractable closest = null;
        float closestDist = float.MaxValue;

        foreach (Collider hit in hits)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable == null) continue;

            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = interactable;
            }
        }

        currentInteractable = closest;

        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(currentInteractable != null);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}