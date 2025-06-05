using UnityEngine;

public class RicoWeapon : AutoWeaponBase
{
    //protected override void Fire(Vector2 direction)
    //{
    //    GameObject proj = Instantiate(weaponData.projectilePrefab, transform.position, Quaternion.identity);
    //    proj.GetComponent<ProjectileBase>().Init(direction, weaponData);
    //}
    protected override void Fire(Vector2 direction)
    {
        if (playerStats == null) return;

        // Generate a random angle (between -180 and 180 degrees)
        float randomAngle = Random.Range(-180f, 180f);
        direction = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));

        // Make a temp copy of weapon data to apply upgrades
        WeaponData tempData = Instantiate(weaponData);
        tempData.damage += playerStats.GetStat(GetWeaponType(), UpgradeData.UpgradeType.Damage);
        tempData.aliveTime += playerStats.GetStat(GetWeaponType(), UpgradeData.UpgradeType.AliveTime);

        GameObject proj = Instantiate(tempData.projectilePrefab, transform.position, Quaternion.identity);
        proj.GetComponent<ProjectileBase>().Init(direction, tempData);
    }

    protected override UpgradeData.WeaponType GetWeaponType()
    {
        return UpgradeData.WeaponType.Ricochet;
    }
}
