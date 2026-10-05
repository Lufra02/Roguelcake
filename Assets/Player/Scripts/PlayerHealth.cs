using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(PlayerStats))]
public class PlayerHealth : Health
{
    private PlayerStats stats;

    public bool canBeDamaged;
    [SerializeField] private float eFrameSeconds;
    private float timer;
    
    [Header("Explosion de gomita")]
    [SerializeField] private GameObject gummyPrefab;
    
    [SerializeField] [Range(0.0f, 10.0f)] private float horizontalForceMultiplier = 1f;
    [SerializeField] [Range(0.0f, 10.0f)] private float verticalForceMultiplier = 1f;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        if (stats != null)
            SetMaxHealth(stats.maxHealth);
        
        canBeDamaged = true;
        timer = 0f;
    }
    
    public void GetHealthBack() 
    {
        Heal(stats.gummyHealAmount);
    }

    public void RestoreHealth(int amount) => Heal(amount);

    public void SetMaximumHealth(int amount) => SetMaxHealth(amount);

    public override void TakeDamage(float dmg)
    {
        base.TakeDamage(dmg);
        if (CurrentHealth <= 0) 
        {
            Die();
        }
        else
        {
            PlayerManager.Instance.playerAnimationManager.PlayOnTakeAnimation("Take_Damage_01");
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
        manager.playerHealth.enabled = false;

        manager.playerAnimationManager.PlayOnTakeAnimation("Death_01");
        
        
    }
}
