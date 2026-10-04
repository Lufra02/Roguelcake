using UnityEngine;

public class Gummie : MonoBehaviour
{
    private FakeProjectilePhysics fakePhysics;

    // Empieza en false: recién instanciada (dentro o cerca del jugador), no debe poder
    // ser recogida todavía. Se activa cuando FakeProjectilePhysics avisa que aterrizó.
    private bool canBeCollected;

    private void Awake()
    {
        fakePhysics = GetComponent<FakeProjectilePhysics>();

        if (fakePhysics != null)
            fakePhysics.OnLanded += HandleLanded;
    }

    private void HandleLanded()
    {
        canBeCollected = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // El suelo se detecta siempre, incluso mientras todavía está "en el aire":
        // esto es justamente lo que dispara el aterrizaje.
        if (other.gameObject.layer == 3)
        {
            fakePhysics?.Land();
            return;
        }

        // Mientras no haya aterrizado, se ignora cualquier otra colisión (incluido el jugador).
        if (!canBeCollected) return;

        var player = other.GetComponent<PlayerManager>();
        if (player != null)
        {
            player.playerHealth.GetHealthBack();
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (fakePhysics != null)
            fakePhysics.OnLanded -= HandleLanded;
    }
}