using System.Collections.Generic;
using UnityEngine;

public class WeaponUnlocks : MonoBehaviour
{
    private HashSet<UpgradeData.WeaponType> unlockedWeapons = new HashSet<UpgradeData.WeaponType>();

    void Awake()
    {
        // Start with Shotgun only
        unlockedWeapons.Add(UpgradeData.WeaponType.Shotgun);
    }

    public bool IsWeaponUnlocked(UpgradeData.WeaponType weapon)
    {
        return unlockedWeapons.Contains(weapon);
    }

    public void UnlockWeapon(UpgradeData.WeaponType weapon)
    {
        if (!unlockedWeapons.Contains(weapon))
        {
            unlockedWeapons.Add(weapon);
            Debug.Log($"Unlocked {weapon}");
        }

        // Activate weapon if it's the Blade
        if (weapon == UpgradeData.WeaponType.Blade)
        {
            ActivateBladeWeapon();
        }
    }

    private void ActivateBladeWeapon()
    {
        BladeSpawner bladeSpawner = FindObjectOfType<BladeSpawner>(true); // true to include inactive
        if (bladeSpawner != null)
        {
            bladeSpawner.gameObject.SetActive(true);
            bladeSpawner.Start(); // Manually call Start to initialize blades
        }
    }

    public List<UpgradeData.WeaponType> GetUnlockedWeapons()
    {
        return new List<UpgradeData.WeaponType>(unlockedWeapons);
    }

    public List<UpgradeData.WeaponType> GetLockedWeapons()
    {
        var allWeapons = new List<UpgradeData.WeaponType>
    {
        UpgradeData.WeaponType.Laser,
        UpgradeData.WeaponType.Ricochet,
        UpgradeData.WeaponType.Blade
    };

        allWeapons.RemoveAll(w => unlockedWeapons.Contains(w));
        return allWeapons;
    }
}
