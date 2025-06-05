using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public WeaponData weaponData;
    protected float lastFireTime;
    protected PlayerStats playerStats; // SAVE it once here

    protected virtual void Awake()
    {
        playerStats = GetComponentInParent<PlayerStats>(); // safer
    }

    public virtual void TryFire(Vector2 direction)
    {
        if (!CanFire()) return;
        Fire(direction.normalized);
        ResetCooldown();
    }

    public bool CanFire()
    {
        float effectiveFireRate = weaponData.fireRate;
        if (playerStats != null)
        {
            effectiveFireRate += playerStats.GetStat(GetWeaponType(), UpgradeData.UpgradeType.FireRate);
        }
        return Time.time - lastFireTime >= 1f / effectiveFireRate;
    }

    public void ResetCooldown()
    {
        lastFireTime = Time.time;
    }

    protected abstract void Fire(Vector2 direction);

    protected abstract UpgradeData.WeaponType GetWeaponType(); // here is okay because WeaponBase is abstract
}
