using UnityEngine;

public class ShotgunWeapon : AutoWeaponBase
{
    private PlayerMovement playerMove;

    protected override void Awake()
    {
        base.Awake(); // important to call WeaponBase.Awake()
        playerMove = GetComponentInParent<PlayerMovement>();
    }

    //public override void AutoFire()
    //{
    //    if (playerMove == null) return;

    //    Vector2 moveDir = playerMove.MoveDirection;

    //    if (moveDir.magnitude > 0.1f && CanFire())
    //    {
    //        Fire(moveDir.normalized);
    //        ResetCooldown();
    //    }
    //}
    public override void AutoFire()
    {
        GameObject nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null || !CanFire()) return;

        Vector2 direction = (nearestEnemy.transform.position - transform.position).normalized;

        Fire(direction);
        ResetCooldown();
    }


    protected override void Fire(Vector2 direction)
    {
        if (playerStats == null) return;

        float extraDamage = playerStats.GetStat(UpgradeData.WeaponType.Shotgun, UpgradeData.UpgradeType.Damage);
        float extraRange = playerStats.GetStat(UpgradeData.WeaponType.Shotgun, UpgradeData.UpgradeType.Range);
        float extraSpread = playerStats.GetStat(UpgradeData.WeaponType.Shotgun, UpgradeData.UpgradeType.Spread);

        int basePelletCount = 5;
        int pelletCount = basePelletCount + Mathf.RoundToInt(extraSpread);

        float spreadAngle = 30f + (extraSpread * 5f);

        for (int i = 0; i < pelletCount; i++)
        {
            float angle = -spreadAngle / 2f + spreadAngle * (i / (float)(pelletCount - 1));
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
            Vector2 dir = rotation * direction;

            GameObject proj = Instantiate(weaponData.projectilePrefab, transform.position, Quaternion.identity);

            var projectile = proj.GetComponent<ProjectileBase>();

            // Make a temp copy of weapon data so upgrades only apply to this projectile
            WeaponData tempData = Instantiate(weaponData);
            tempData.damage += extraDamage;
            tempData.range += extraRange;

            projectile.Init(dir.normalized, tempData);
        }
    }

    protected override UpgradeData.WeaponType GetWeaponType()
    {
        return UpgradeData.WeaponType.Shotgun;
    }
}
