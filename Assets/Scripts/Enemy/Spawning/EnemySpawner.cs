using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnRate
    {
        public EnemyData enemyType;
        public Vector2 spawnsPerSecond; // x = min, y = max
    }

    [System.Serializable]
    public class WaveSettings
    {
        public float duration = 120f;
        public List<EnemySpawnRate> spawnRates;
        public float delayAfterWave = 5f;
        public bool isBossWave = false; // Add this field
    }

    [Header("Configurations")]
    [SerializeField] List<WaveSettings> waveSettings;
    [SerializeField] float poolBufferTime = 25f; // Configurable buffer time


    [Header("Spawn Area")]
    [SerializeField] float minSpawnRange = 5f;
    [SerializeField] float maxSpawnRange = 15f;
    [SerializeField] LayerMask walkableLayer;
    [SerializeField] LayerMask wallLayer;

    private Dictionary<GameObject, Queue<GameObject>> enemyPools = new Dictionary<GameObject, Queue<GameObject>>();
    private Transform player;
    private int currentWaveIndex;
    private bool waveInProgress;
    private float waveTimer;
    private float[] spawnAccumulators;

    [SerializeField] private GameObject bossIcon;

    [SerializeField] public bool isEndlessMode = false;
    [SerializeField] private float endlessSpawnInterval = 2f;
    [SerializeField] private float endlessDifficultyInterval = 1f;
    [SerializeField] private List<EnemyData> endlessEnemies; // populate this from your last wave or manually

    private float endlessSpawnTimer;
    private float endlessDifficultyTimer;
    private float endlessHealthMultiplier = 1f;
    private float endlessMoveSpeed = 3f;

    //[SerializeField] private bool startEndlessOnLaunch = true;

    void Start()
    {
        //if (startEndlessOnLaunch)
        //{
        //    StartEndlessMode();
        //}
        if (waveSettings.Count > 0)
        {
            ResizePoolsForWave(waveSettings[0]);
            StartNextWave();
        }
    }

    void Update()
    {
        if (player == null) player = GameObject.FindWithTag("Player").transform;
        if (!waveInProgress) return;
        if (isEndlessMode)
        {
            endlessSpawnTimer += Time.deltaTime;
            endlessDifficultyTimer += Time.deltaTime;

            if (endlessSpawnTimer >= endlessSpawnInterval)
            {
                endlessSpawnTimer = 0f;
                if (endlessEnemies.Count > 0)
                {
                    var randomEnemy = endlessEnemies[Random.Range(0, endlessEnemies.Count)];
                    SpawnEnemy(randomEnemy.prefab, isEndless: true);
                }
            }

            if (endlessDifficultyTimer >= endlessDifficultyInterval)
            {
                endlessDifficultyTimer = 0f;
                endlessHealthMultiplier += 0.25f;
                endlessMoveSpeed = Mathf.Min(endlessMoveSpeed + 0.25f, 10f);
                Debug.Log($"Endless difficulty increased! Health x{endlessHealthMultiplier}, Speed: {endlessMoveSpeed}");

                // 🔁 Apply speed to ALL enemies in scene
                EnemyAI[] allEnemies = FindObjectsOfType<EnemyAI>();
                foreach (var ai in allEnemies)
                {
                    ai.moveSpeed = endlessMoveSpeed;
                }
            }
            return;
        }

        // Only increment timer for non-boss waves
        if (!waveSettings[currentWaveIndex].isBossWave)
        {
            waveTimer += Time.deltaTime;
            if (waveTimer >= waveSettings[currentWaveIndex].duration)
            {
                EndCurrentWave();
                return;
            }
            UpdateSpawning();
        }
    }

    public void StartEndlessMode()
    {
        isEndlessMode = true;
        waveInProgress = true;
        endlessSpawnTimer = 0f;
        endlessDifficultyTimer = 0f;
        endlessHealthMultiplier = 1f;
        endlessMoveSpeed = 3f;

        Debug.Log("Endless Mode Started!");

        // Pool all possible enemies once
        foreach (var enemy in endlessEnemies)
        {
            if (!enemyPools.ContainsKey(enemy.prefab))
            {
                enemyPools[enemy.prefab] = new Queue<GameObject>();
            }

            for (int i = 0; i < 30; i++) // pre-pool some
            {
                GameObject e = Instantiate(enemy.prefab, transform);
                e.SetActive(false);
                enemyPools[enemy.prefab].Enqueue(e);
            }
        }
    }


    // Replace InitializePools with this:
    void ResizePoolsForWave(WaveSettings wave)
    {
        foreach (var rate in wave.spawnRates)
        {
            int required = Mathf.CeilToInt(rate.spawnsPerSecond.y * poolBufferTime);

            if (!enemyPools.ContainsKey(rate.enemyType.prefab))
            {
                enemyPools[rate.enemyType.prefab] = new Queue<GameObject>();
            }

            while (enemyPools[rate.enemyType.prefab].Count < required)
            {
                var enemy = Instantiate(rate.enemyType.prefab, transform);
                enemy.SetActive(false);
                enemyPools[rate.enemyType.prefab].Enqueue(enemy);
            }
        }
    }

    void UpdateSpawning()
    {
        if (waveSettings[currentWaveIndex].isBossWave) return;

        for (int i = 0; i < waveSettings[currentWaveIndex].spawnRates.Count; i++)
        {
            var rate = waveSettings[currentWaveIndex].spawnRates[i];
            float spawnsThisFrame = Random.Range(rate.spawnsPerSecond.x, rate.spawnsPerSecond.y) * Time.deltaTime;
            spawnAccumulators[i] += spawnsThisFrame;

            while (spawnAccumulators[i] >= 1f)
            {
                SpawnEnemy(rate.enemyType.prefab);
                spawnAccumulators[i] -= 1f;
            }
        }
    }

    void SpawnEnemy(GameObject prefab, bool isEndless = false)
    {
        Vector2 spawnPos = GetValidSpawnPosition();
        if (spawnPos == Vector2.zero) return;

        if (!enemyPools.ContainsKey(prefab))
        {
            enemyPools[prefab] = new Queue<GameObject>();
        }

        GameObject enemy = null;

        // Search for a valid inactive enemy in the pool
        int tries = enemyPools[prefab].Count;
        for (int i = 0; i < tries; i++)
        {
            var candidate = enemyPools[prefab].Dequeue();
            if (!candidate.activeInHierarchy)
            {
                enemy = candidate;
                break;
            }
            else
            {
                // Still alive, put it back
                enemyPools[prefab].Enqueue(candidate);
            }
        }

        if (enemy == null)
        {
            // No inactive enemies found, instantiate new one
            enemy = Instantiate(prefab, transform);
            enemy.SetActive(false);
        }

        enemy.transform.position = spawnPos;
        var enemyAI = enemy.GetComponent<EnemyAI>();
        enemyAI.spawner = this;
        enemyAI.ResetEnemy(); // super important
        if (isEndless && enemyAI != null)
        {
            enemyAI._currentHP = Mathf.RoundToInt(enemyAI.data.maxHP * endlessHealthMultiplier);
            enemyAI.moveSpeed = endlessMoveSpeed;
        }

        enemy.SetActive(true);
        enemy.transform.SetParent(null);
    }

    Vector2 GetValidSpawnPosition()
    {
        for (int i = 0; i < 30; i++)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            Vector2 testPos = (Vector2)player.position + randomDir * Random.Range(minSpawnRange, maxSpawnRange);
            if (Physics2D.OverlapPoint(testPos, walkableLayer) && !Physics2D.OverlapCircle(testPos, 1f, wallLayer))
                return testPos;
        }
        return Vector2.zero;
    }

    void StartNextWave()
    {
        Debug.Log($"=== Starting Wave {currentWaveIndex + 1} / {waveSettings.Count} ===");
        Debug.Log($"Wave Index: {currentWaveIndex} | Is Boss Wave: {waveSettings[currentWaveIndex].isBossWave}");

        waveInProgress = true;
        waveTimer = 0f;
        spawnAccumulators = new float[waveSettings[currentWaveIndex].spawnRates.Count];

        // Toggle boss icon visibility
        if (bossIcon != null)
        {
            bossIcon.SetActive(waveSettings[currentWaveIndex].isBossWave);
        }

        if (waveSettings[currentWaveIndex].isBossWave)
        {
            int bossIndex = waveSettings.Take(currentWaveIndex + 1).Count(w => w.isBossWave) - 1;
            Debug.Log($"Boss Wave Detected! bossIndex = {bossIndex}");

            RoomGenerator roomGenerator = FindObjectOfType<RoomGenerator>();
            if (roomGenerator != null)
            {
                roomGenerator.StartBossWave(bossIndex);
            }
        }

    }

    void EndCurrentWave()
    {
        waveInProgress = false;

        if (currentWaveIndex < waveSettings.Count - 1)
        {
            ResizePoolsForWave(waveSettings[currentWaveIndex + 1]);
        }

        currentWaveIndex++;
        Debug.Log(currentWaveIndex);

        // Hide the boss icon when any wave ends
        if (bossIcon != null)
            bossIcon.SetActive(false);

        if (currentWaveIndex < waveSettings.Count)
        {
            Invoke(nameof(StartNextWave),
                  waveSettings[currentWaveIndex - 1].delayAfterWave);
        }
        else
        {
            Debug.Log("All waves complete!");
            GameManager.Instance.WinGame();
            // Optional: Trigger game over, victory, or endless loop
        }

    }

    public void OnBossDeath()
    {
        if (waveSettings[currentWaveIndex].isBossWave)
        {
            EndCurrentWave();
        }
    }

    public void ReturnEnemyToPool(GameObject enemy)
    {
        // Find which prefab type this enemy is
        foreach (var pool in enemyPools)
        {
            if (enemy.name.StartsWith(pool.Key.name))
            {
                //Debug.Log($"{enemy.name} died with HP: {enemy.GetComponent<EnemyAI>().DebugCurrentHP}");
                enemy.SetActive(false);
                enemy.transform.SetParent(transform);
                pool.Value.Enqueue(enemy);
                return;
            }
        }

        // If unknown type, destroy it
        Destroy(enemy);
        //Debug.Log("");
    }

    void OnDrawGizmosSelected()
    {
        if (player == null) player = GameObject.FindWithTag("Player")?.transform;
        if (player == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.position, minSpawnRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, maxSpawnRange);
    }
}