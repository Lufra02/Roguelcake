// Cualquier objeto que pueda recibir daño (enemigos, cajas destructibles, etc.)
// debe implementar esta interfaz.
public interface IDamageable
{
    void TakeDamage(int amount);
}