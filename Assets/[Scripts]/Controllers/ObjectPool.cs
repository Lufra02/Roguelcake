using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [Header("Configuración del Pool")]
    [Tooltip("Prefab base que se va a clonar.")]
    [SerializeField] private GameObject prefab;

    [Tooltip("Cantidad inicial de instancias en el pool.")]
    [SerializeField] private int initialPoolSize = 20;

    [Tooltip("Si se agotan los objetos disponibles, ¿se crean nuevos bajo demanda?")]
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
            Debug.LogError($"[ObjectPool] No se ha asignado ningún prefab en el GameObject '{gameObject.name}'.", this);
            return;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewInstance();
        }
    }

    private GameObject CreateNewInstance()
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        poolQueue.Enqueue(obj);
        return obj;
    }

    /// <summary>
    /// Obtiene un objeto disponible del pool, lo posiciona y lo activa.
    /// </summary>
    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject obj;

        if (poolQueue.Count > 0)
        {
            obj = poolQueue.Dequeue();
        }
        else if (canExpand)
        {
            // Creamos uno nuevo si la cola está vacía y permitimos expansión
            obj = CreateNewInstance();
            poolQueue.Dequeue(); // Se extrae el recién encolado
        }
        else
        {
            Debug.LogWarning("[ObjectPool] No hay objetos disponibles en el pool y la expansión está deshabilitada.");
            return null;
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        return obj;
    }

    /// <summary>
    /// Desactiva el objeto y lo regresa al pool para volver a usarse.
    /// </summary>
    public void ReturnToPool(GameObject obj)
    {
        if (obj == null) return;

        obj.SetActive(false);
        obj.transform.SetParent(transform);
        poolQueue.Enqueue(obj);
    }
}