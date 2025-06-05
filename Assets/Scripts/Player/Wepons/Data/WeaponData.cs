using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public float damage;
    public float fireRate;
    public float range;
    public float projectileSpeed;
    public GameObject projectilePrefab;
    public bool destroyOnEnemyHit = true;
    public bool destroyOnWallHit = true;
    public float aliveTime = 5f;
}
