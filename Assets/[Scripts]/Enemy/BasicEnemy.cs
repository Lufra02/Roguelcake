using UnityEngine;

public class BasicEnemy : Enemy
{
   protected override void Start()
    {
        base.Start(); // Ejecuta la búsqueda del jugador por tag
        // Tu código adicional aquí...
    }

    protected override void Update()
    {
        base.Update(); // Ejecuta la máquina de estados (Move, Attack, Idle)
        // Tu código adicional aquí...
    }
}
