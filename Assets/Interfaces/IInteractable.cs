using UnityEngine;

// Cualquier objeto con el que el jugador pueda interactuar (tienda, NPC, cofre, palanca, etc.)
// debe implementar esta interfaz.
public interface IInteractable
{
    // Se llama cuando el jugador interactúa. "interactor" es el GameObject del jugador.
    void Interact(GameObject interactor);
}