using UnityEngine;

[System.Serializable]
public class DropChanceSettings
{
    [Range(0, 100)] public float foodDropChance = 20f;
    [Range(0, 100)] public float silverChestDropChance = 8f;
    [Range(0, 100)] public float bombDropChance = 2f;
}

public class EnemyDropSystem : MonoBehaviour
{
    [SerializeField] private DropChanceSettings dropSettings;
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private GameObject silverChestPrefab;
    [SerializeField] private GameObject bombPrefab;

    public static EnemyDropSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void HandleEnemyDeath(Vector2 deathPosition)
    {
        Debug.Log("111111111111111");
        float randomRoll = Random.Range(0f, 100f);
        //Debug.Log("Drop : " + randomRoll);
        float cumulativeChance = 0f;

        // Check bomb drop first (rarest)
        cumulativeChance += dropSettings.bombDropChance;
        if (randomRoll <= cumulativeChance)
        {
            SpawnDrop(bombPrefab, deathPosition);
            return;
        }

        // Check silver chest
        cumulativeChance += dropSettings.silverChestDropChance;
        if (randomRoll <= cumulativeChance)
        {
            SpawnDrop(silverChestPrefab, deathPosition);
            return;
        }

        // Check food
        cumulativeChance += dropSettings.foodDropChance;
        if (randomRoll <= cumulativeChance)
        {
            SpawnDrop(foodPrefab, deathPosition);
        }
    }

    private void SpawnDrop(GameObject prefab, Vector2 position)
    {
        if (prefab != null)
        {
            Instantiate(prefab, position, Quaternion.identity);
        }
    }
}