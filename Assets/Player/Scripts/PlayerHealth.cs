using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(PlayerStats))]
public class PlayerHealth : Health
{
    private PlayerStats stats;

    public bool canBeDamaged;
    [SerializeField] private float eFrameSeconds;
    private float timer;

    [Header("Life UI System")]
    [SerializeField] public LifeSystemUIManager lifeSystemUIManager;

    [Header("Integracion Visual")]
    [SerializeField] List<GameObject> Gomita = new List<GameObject>();
    
    [Header("Explosion de gomita")]
    [SerializeField] private GameObject gummyPrefab;
    
    [SerializeField] [UnityEngine.Range(0.0f, 10.0f)] private float horizontalForceMultiplier = 1f;
    [SerializeField] [UnityEngine.Range(0.0f, 10.0f)] private float verticalForceMultiplier = 1f;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        if (stats != null)
            SetMaxHealth(stats.maxHealth);

        SetHealth(MaxHealth);
        
        canBeDamaged = true;
        timer = 0f;

        foreach (var item in Gomita)
        {
            item.SetActive(true);
        }
        lifeSystemUIManager.UpdateHearths(CurrentHealth, MaxHealth);
    }
    
    public void GetHealthBack() 
    {
        Heal(stats.gummyHealAmount);
        UpdateDamageVisuals();
        lifeSystemUIManager.UpdateHearths(CurrentHealth, MaxHealth);
    }

    public void RestoreHealth(int amount)
    {
        Heal(amount);
        UpdateDamageVisuals();
        lifeSystemUIManager.UpdateHearths(CurrentHealth, MaxHealth);
    }

    public void SetMaximumHealth(int amount)
    {
        SetMaxHealth(amount);
        UpdateDamageVisuals();

        lifeSystemUIManager.UpdateHearths(CurrentHealth, MaxHealth);
        print($"Current: {CurrentHealth} / Max: {MaxHealth}");
    }

    public override void TakeDamage(float dmg)
    {
        base.TakeDamage(dmg);
        UpdateDamageVisuals();
        if (CurrentHealth <= 0) 
        {
            Die();
        }
        else
        {
            PlayerManager.Instance.playerAnimationManager.PlayOnTakeAnimation("Take_Damage_01");
            lifeSystemUIManager.UpdateHearths(CurrentHealth, MaxHealth);
        }
    }
    
    private void Update()
    {
        // E FRAMES UPDATE
        if (!canBeDamaged)
        {
            timer += Time.deltaTime;
            // AGREGAR EFECTO DE QUE ESTA INVULNERABLE
            
            if (timer >= eFrameSeconds)
            {
                canBeDamaged = true;
                timer = 0f;
            }
        }
    }

    // DESGASTE VISUAL PROGRESIVO
    private void UpdateDamageVisuals()
    {
        if (Gomita == null || Gomita.Count == 0 || stats == null) return;

        // Evita division por cero si maxHealth es 1
        float maxHealth = Mathf.Max(stats.maxHealth, 1f);
        float usableRange = Mathf.Max(maxHealth - 1f, 1f); // El rango real de "desgaste" va de 1 a maxHealth

        // 1 de vida = 0%, vida maxima = 100%
        float healthPercent = Mathf.Clamp01((CurrentHealth - 1f) / usableRange);

        int activeCount = Mathf.CeilToInt(healthPercent * Gomita.Count);

        for (int i = 0; i < Gomita.Count; i++)
        {
            Gomita[i].SetActive(i < activeCount);
        }
    }

    public void RecieveExplosion()
    {
        canBeDamaged = false;
        // Si no esta en calaca
        if (CurrentHealth > 1)
        {
            int gummiesToThrow = Mathf.RoundToInt(CurrentHealth - 1);
            
            for (int i = 0; i < gummiesToThrow; i++)
            {
                GameObject gummy = Instantiate(gummyPrefab, transform.position, Quaternion.identity);

                FakeProjectilePhysics fakePhysics = gummy.GetComponent<FakeProjectilePhysics>();
                if (fakePhysics != null)
                {
                    Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
                    float horizontalForce = Random.Range(2f, 4f) * horizontalForceMultiplier;
                    float upwardForce = Random.Range(4f, 6f) * verticalForceMultiplier;

                    fakePhysics.Launch(randomDirection * horizontalForce + Vector3.up * upwardForce);
                }    
            }

            SetHealth(1);
            UpdateDamageVisuals();
            lifeSystemUIManager.UpdateHearths(CurrentHealth, MaxHealth);
        }
        else
        {
            Die();
        }
    }

    public override void Die()
    {
        base.Die();
        PlayerManager manager = PlayerManager.Instance;

        if (manager.isDead)
            return;

        manager.isDead = true;
        
        manager.playerCombat.enabled = false;
        manager.playerController.enabled = false;

        manager.playerAnimationManager.PlayOnTakeAnimation("Death_01"); 
       
    }
}
