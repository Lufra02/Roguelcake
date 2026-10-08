using System.Collections.Generic;
using UnityEngine;

public class LifeSystemUIManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] GameObject lastHearth;
    [SerializeField] GameObject fullHearth;
    [SerializeField] GameObject halfHearth;
    [SerializeField] GameObject emptyHearth; // Fondo que indica la vida maxima

    [Header("Positions")]
    [SerializeField] Transform lastHearthPosition;
    [SerializeField] Transform heartsContainer; // Objeto con el Horizontal Layout Group

    [Header("Referencias")]
    [SerializeField] PlayerStats playerStats;
 
    private List<GameObject> spawnedEmptyHearts = new List<GameObject>(); // Los slots de fondo (uno por vida maxima)
    private List<GameObject> spawnedFilledHearts = new List<GameObject>(); // Lo que se instancia encima (lleno/mitad)
    private GameObject spawnedLastHearth;

    private int cachedHeartSlots;

    [Header("Limite de corazones")]
    // Calculado a partir de PlayerStats.MaxHealthAllowed, ya no es un valor fijo separado
    private int MaxHeartSlots => playerStats != null ? Mathf.Max((playerStats.MaxHealthAllowed - 1) / 2, 0) : 6;

    public void UpdateHearths(float currentHealth, int maxHealth)
    {
        int remainingHealth = Mathf.Max(maxHealth - 1, 0); // El ultimo punto lo cubre lastHearth
        int requiredSlots = Mathf.Min(Mathf.CeilToInt(remainingHealth / 2f), MaxHeartSlots);

        if (requiredSlots != cachedHeartSlots || spawnedEmptyHearts.Count != requiredSlots)
        {
            cachedHeartSlots = requiredSlots;
            RebuildEmptySlots(cachedHeartSlots);
        }

        ClearFilledHearts();

        if (currentHealth >= 1)
        {
            spawnedLastHearth = Instantiate(lastHearth, lastHearthPosition);
        }

        float healthForHearts = Mathf.Max(currentHealth - 1, 0);

        for (int i = 0; i < cachedHeartSlots; i++)
        {
            float healthForThisHeart = Mathf.Clamp(healthForHearts - (i * 2), 0, 2);

            Transform slotParent = spawnedEmptyHearts[i].transform;

            if (healthForThisHeart >= 2)
            {
                GameObject heart = Instantiate(fullHearth, slotParent);
                StretchToFullSlot(heart);
                spawnedFilledHearts.Add(heart);
            }
            else if (healthForThisHeart >= 1)
            {
                GameObject heart = Instantiate(halfHearth, slotParent);
                StretchToHalfSlot(heart);
                spawnedFilledHearts.Add(heart);
            }
        }
    }

    private void RebuildEmptySlots(int slotCount)
    {
        // Destruimos el fondo anterior (esto tambien destruye los hijos lleno/mitad)
        foreach (var slot in spawnedEmptyHearts)
        {
            if (slot != null) Destroy(slot);
        }
        spawnedEmptyHearts.Clear();
        spawnedFilledHearts.Clear();

        for (int i = 0; i < slotCount; i++)
        {
            GameObject empty = Instantiate(emptyHearth, heartsContainer);
            spawnedEmptyHearts.Add(empty);
        }
    }

    private void ClearFilledHearts()
    {
        foreach (var heart in spawnedFilledHearts)
        {
            if (heart != null) Destroy(heart);
        }
        spawnedFilledHearts.Clear();

        if (spawnedLastHearth != null)
        {
            Destroy(spawnedLastHearth);
            spawnedLastHearth = null;
        }
    }

    private void ResetLocalTransform(GameObject obj)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }

    private void StretchToFullSlot(GameObject obj)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }

    private void StretchToHalfSlot(GameObject obj)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            // Ocupa solo la mitad izquierda del slot; la otra mitad queda visible el fondo vacio
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}