using System;
using UnityEngine;

public class PooledEnemy : MonoBehaviour
{
    private ObjectPool poolOwner;

    public bool IsWaveEnemy { get; private set; }

    public event Action<PooledEnemy> OnEnemyDeath;

    public void Setup(ObjectPool pool, bool isWaveEnemy)
    {
        poolOwner = pool;
        IsWaveEnemy = isWaveEnemy;
    }

    public void Die()
    {
        OnEnemyDeath?.Invoke(this);

        if (poolOwner != null)
        {
            poolOwner.ReturnToPool(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}