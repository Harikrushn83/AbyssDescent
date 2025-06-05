using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private Dictionary<UpgradeData.WeaponType, Dictionary<UpgradeData.UpgradeType, float>> upgrades =
        new Dictionary<UpgradeData.WeaponType, Dictionary<UpgradeData.UpgradeType, float>>();

    public void ApplyUpgrade(UpgradeData upgrade)
    {
        if (!upgrades.ContainsKey(upgrade.weapon))
            upgrades[upgrade.weapon] = new Dictionary<UpgradeData.UpgradeType, float>();

        if (!upgrades[upgrade.weapon].ContainsKey(upgrade.stat))
            upgrades[upgrade.weapon][upgrade.stat] = 0f;

        upgrades[upgrade.weapon][upgrade.stat] += upgrade.value;

        Debug.Log($"Applied {upgrade.upgradeName}: {upgrade.stat} +{upgrade.value} on {upgrade.weapon}");
    }

    public float GetStat(UpgradeData.WeaponType weapon, UpgradeData.UpgradeType stat)
    {
        if (upgrades.ContainsKey(weapon) && upgrades[weapon].ContainsKey(stat))
            return upgrades[weapon][stat];
        return 0f;
    }
}
