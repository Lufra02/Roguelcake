using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [Header("Configuración del Pool")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private bool canExpand = true;

    private readonly Queue<GameObject> poolQueue = new Queue<GameObject>();

    private void Awake()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        if (prefab == null)
        {
            Debug.LogError(
                $"[ObjectPool] No se ha asignado ningún prefab en '{gameObject.name}'.",
                this
            );

            return;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = CreateNewInstance();
            poolQueue.Enqueue(obj);
        }
    }

    private GameObject CreateNewInstance()
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);

        return obj;
    }

    public GameObject Get(
      Vector3 position,
      Quaternion rotation,
      bool isWaveEnemy)
    {
        GameObject obj;

        if (poolQueue.Count > 0)
        {
            obj = poolQueue.Dequeue();
        }
        else if (canExpand)
        {
            obj = CreateNewInstance();
        }
        else
        {
            Debug.LogWarning(
                "[ObjectPool] No hay objetos disponibles."
            );

            return null;
        }

        obj.transform.SetPositionAndRotation(
            position,
            rotation
        );

        PooledEnemy pooledEnemy = obj.GetComponent<PooledEnemy>();

        if (pooledEnemy != null)
        {
            pooledEnemy.Setup(this, isWaveEnemy);
        }

        obj.SetActive(true);

        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        if (obj == null)
            return;

        obj.SetActive(false);
        obj.transform.SetParent(transform);

        poolQueue.Enqueue(obj);
    }
}