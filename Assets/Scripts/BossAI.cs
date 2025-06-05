using UnityEngine;

public class BossAI : MonoBehaviour
{
    [Header("Combat Settings")]
    public int[] maxHealthPerWave = { 200, 300, 400, 500 }; // Example values for waves 0–3
    private int maxHealth;
    public float attackInterval = 2f;
    private int currentHealth;
    private float nextAttackTime;

    private Transform player;
    private Rigidbody2D rb;
    private Vector2 fixedPosition;

    public BossAltar connectedAltar;

    [Header("Projectile Attack")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileForce = 10f;

    [Header("Wave Scaling")]
    public int currentWaveIndex; // Set by RoomGenerator when spawned
    public float[] speedMultipliersPerWave = { 1f, 1.3f, 1.6f, 2f };
    public float[] healRatesPerWave = { 0f, 0f, 100f, 150f }; // Heal per second
    public GameObject[] minionPrefabsPerWave; // Assign in inspector

    private float nextHealTime;
    private float nextMinionSpawnTime;

    public GameObject goldChestPrefab;

    private bool isDead = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        fixedPosition = transform.position;
        maxHealth = maxHealthPerWave[currentWaveIndex];
        currentHealth = maxHealth;
        nextAttackTime = Time.time + attackInterval; // Initialize first attack

        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        ApplyWaveScaling();

        BossHealthBarUI bossBar = FindObjectOfType<BossHealthBarUI>();
        if (bossBar != null)
        {
            bossBar.SetBoss(this);
        }
    }

    void Update()
    {
        if (Time.time >= nextAttackTime)
        {
            StartProjectileAttack();
            nextAttackTime = Time.time + attackInterval; // Reset timer
        }

        if (player == null) return;

        // Left/Right flip only
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (player.position.x > transform.position.x ? 1 : -1);
        transform.localScale = scale;

        HandleWaveSpecificAbilities();
    }

    void LateUpdate()
    {
        transform.position = fixedPosition;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        //Debug.Log(currentHealth);
        if (currentHealth <= 0)
        {
            isDead = true;
            Die();
        }
    }

    void Die()
    {
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.OnBossDeath();
        }

        RoomGenerator roomGenerator = FindObjectOfType<RoomGenerator>();
            if (roomGenerator != null )
            {
                roomGenerator.DeactivateBarriers(connectedAltar);
            }

        if (goldChestPrefab != null)
        {
            Instantiate(goldChestPrefab, transform.position, Quaternion.identity);
        }

        // Add death effects here when ready
        BossHealthBarUI bossBar = FindObjectOfType<BossHealthBarUI>();
        if (bossBar != null)
        {
            bossBar.ClearBoss();
        }

        Destroy(gameObject);
    }

    void StartProjectileAttack()
    {
        GameObject projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        // Set the projectile's target to the player's position at time of firing
        BossProjectile bossProjectile = projectile.GetComponent<BossProjectile>();
        if (bossProjectile != null)
        {
            bossProjectile.SetTarget(player.position);
        }
    }

    void ApplyWaveScaling()
    {
        // Speed boost (reduces attack interval)
        attackInterval /= speedMultipliersPerWave[currentWaveIndex];
    }

    void HandleWaveSpecificAbilities()
    {
        // Wave 3+ healing
        if (healRatesPerWave.Length > currentWaveIndex &&
            healRatesPerWave[currentWaveIndex] > 0 &&
            Time.time >= nextHealTime)
        {
            Heal(300);
            nextHealTime = Time.time + (10f / healRatesPerWave[currentWaveIndex]);
        }

        // Wave 4+ minion spawning
        if (minionPrefabsPerWave.Length > currentWaveIndex &&
            minionPrefabsPerWave[currentWaveIndex] != null &&
            Time.time >= nextMinionSpawnTime)
        {
            SpawnMinion();
            nextMinionSpawnTime = Time.time + Random.Range(0.5f, 0.3f);
        }
    }

    void SpawnMinion()
    {
        Instantiate(
            minionPrefabsPerWave[currentWaveIndex],
            transform.position + (Vector3)Random.insideUnitCircle * 2f,
            Quaternion.identity
        );
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;
}