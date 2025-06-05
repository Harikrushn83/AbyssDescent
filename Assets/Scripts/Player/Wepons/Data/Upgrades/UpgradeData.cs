using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Upgrade")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    public string description;
    public Sprite icon;

    public enum WeaponType { Shotgun, Laser, Ricochet, Blade }
    public WeaponType weapon;

    public enum UpgradeType { Damage, FireRate, Range, Spread, AliveTime, BladeCount, SpinSpeed, ProjectileSpeed }
    public UpgradeType stat;

    public float value; // amount to increase
}
