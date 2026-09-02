using UnityEngine;

// Cualquier objeto con el que el jugador pueda interactuar (tienda, NPC, cofre, palanca, etc.)
// debe implementar esta interfaz.
public interface IInteractable
{
    // Texto opcional para mostrar en la UI ("Presiona E para abrir la tienda")
    string InteractionPrompt { get; }

    // Se llama cuando el jugador interactúa. "interactor" es el GameObject del jugador.
    void Interact(GameObject interactor);
}