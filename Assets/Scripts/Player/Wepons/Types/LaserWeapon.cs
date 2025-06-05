using UnityEngine;

public class LaserWeapon : AutoWeaponBase
{
    protected override void Fire(Vector2 direction)
    {
        Debug.Log("fire");
        GameObject proj = Instantiate(weaponData.projectilePrefab, transform.position, Quaternion.identity);

        // Modify weaponData based on upgrades
        WeaponData upgradedWeaponData = new WeaponData();
        upgradedWeaponData = Instantiate(weaponData); // Create a copy so upgrades don't affect the original data

        // Apply relevant upgrades from PlayerStats
        upgradedWeaponData.damage += playerStats.GetStat(GetWeaponType(), UpgradeData.UpgradeType.Damage);
        upgradedWeaponData.range += playerStats.GetStat(GetWeaponType(), UpgradeData.UpgradeType.Range);
        upgradedWeaponData.projectileSpeed += playerStats.GetStat(GetWeaponType(), UpgradeData.UpgradeType.ProjectileSpeed);

        // Pass upgraded weapon data to the projectile
        proj.GetComponent<ProjectileBase>().Init(direction, upgradedWeaponData);
    }

    protected override UpgradeData.WeaponType GetWeaponType()
    {
        return UpgradeData.WeaponType.Laser;
    }
}
