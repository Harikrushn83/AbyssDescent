using UnityEngine;

public class DropPickup : MonoBehaviour
{
    public enum DropType { Food, SilverChest, Bomb, GoldChest }
    public DropType dropType;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();

        switch (dropType)
        {
            case DropType.Food:
                if (playerHealth != null)
                {
                    playerHealth.Heal(70); // Heals player by 10
                    Debug.Log("heal");
                }
                break;

            case DropType.SilverChest:
                UpgradeManager upgradeManager = FindObjectOfType<UpgradeManager>();
                if (upgradeManager != null)
                {
                    upgradeManager.ShowUpgradeOptions(); // Triggers upgrade selection
                    Debug.Log("silver");
                }
                break;

            case DropType.Bomb:
                EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
                foreach (var enemy in enemies)
                {
                    enemy.TakeDamage(999); // Nukes all enemies
                    Debug.Log("bomb");
                }
                break;
            case DropType.GoldChest:
                WeaponUnlocks unlocks = FindObjectOfType<WeaponUnlocks>();
                if (unlocks != null)
                {
                    var locked = unlocks.GetLockedWeapons();
                    if (locked.Count > 0)
                    {
                        int index = Random.Range(0, locked.Count);
                        var randomWeapon = locked[index];
                        unlocks.UnlockWeapon(randomWeapon);
                        Debug.Log("gold");
                    }
                }
                break;

        }

        Destroy(gameObject); // Remove drop after use
    }
}
