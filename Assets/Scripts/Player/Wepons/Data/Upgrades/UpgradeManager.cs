using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class UpgradeManager : MonoBehaviour
{
    public List<UpgradeData> allUpgrades;
    public GameObject upgradeUI; // Hook to your UI panel
    public UpgradeOptionUI optionPrefab; // UI button prefab
    public Transform optionParent; // Parent to spawn buttons under

    private PlayerStats player;
    private WeaponUnlocks weaponUnlocks;

    void Awake()
    {
        player = FindObjectOfType<PlayerStats>();
        weaponUnlocks = FindObjectOfType<WeaponUnlocks>();
    }

    public void ShowUpgradeOptions()
    {
        Time.timeScale = 0f;
        upgradeUI.SetActive(true);
        ClearOldOptions();

        List<UpgradeData> options = GetRandomUpgrades(3);

        foreach (var upgrade in options)
        {
            var ui = Instantiate(optionPrefab, optionParent);
            ui.Setup(upgrade, () =>
            {
                player.ApplyUpgrade(upgrade);
                if (upgrade.weapon == UpgradeData.WeaponType.Blade)
                {
                    BladeSpawner bladeSpawner = FindObjectOfType<BladeSpawner>();
                    if (bladeSpawner != null)
                    {
                        bladeSpawner.CreateBlades();
                    }
                }
                Time.timeScale = 1f;
                upgradeUI.SetActive(false);
                ClearOldOptions();
            });
        }
    }

    List<UpgradeData> GetRandomUpgrades(int count)
    {
        // ✅ Only use upgrades for weapons the player has unlocked
        List<UpgradeData> pool = allUpgrades.FindAll(upgrade =>
            weaponUnlocks.IsWeaponUnlocked(upgrade.weapon)
        );

        List<UpgradeData> options = new List<UpgradeData>();

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            options.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return options;
    }

    void ClearOldOptions()
    {
        foreach (Transform child in optionParent)
            Destroy(child.gameObject);
    }
}
