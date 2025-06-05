using System.Collections.Generic;
using UnityEngine;

public class BladeSpawner : AutoWeaponBase
{
    public GameObject bladePrefab;
    public int initialBladeCount = 2;
    public float baseOrbitRadius = 2f;
    public float baseRotationSpeed = 180f;
    public int baseDamage = 10;

    private List<SpinningBlade> blades = new List<SpinningBlade>();
    private float currentAngle = 0f;

    protected override void Awake()
    {
        base.Awake(); // Inherited from AutoWeaponBase
    }


    public override void AutoFire()
    {
        // Orbiting blades don't need active firing,
        // but WeaponController expects this method.
    }

    public void Start()
    {
        WeaponUnlocks unlocks = FindObjectOfType<WeaponUnlocks>();
        if (unlocks == null || !unlocks.IsWeaponUnlocked(UpgradeData.WeaponType.Blade))
        {
            gameObject.SetActive(false); // disable if not unlocked
            return;
        }

        CreateBlades();
    }

    void Update()
    {
        float speedMultiplier = 1f + (playerStats?.GetStat(UpgradeData.WeaponType.Blade, UpgradeData.UpgradeType.SpinSpeed) ?? 0f);
        currentAngle += (baseRotationSpeed * speedMultiplier) * Time.deltaTime;

        foreach (var blade in blades)
        {
            blade.UpdateBladePosition(currentAngle);
        }
    }

    public void CreateBlades()
    {
        // Clear existing blades
        foreach (var blade in blades)
        {
            if (blade != null && blade.gameObject != null)
                Destroy(blade.gameObject);
        }
        blades.Clear();

        // Calculate blade count with upgrades (clamped between 2 and 6)
        int bladeUpgradeCount = Mathf.RoundToInt(playerStats?.GetStat(UpgradeData.WeaponType.Blade, UpgradeData.UpgradeType.BladeCount) ?? 0);
        int totalBlades = Mathf.Clamp(initialBladeCount + bladeUpgradeCount, 2, 6);
        float angleStep = 360f / totalBlades;

        // Apply damage upgrades
        float damageBonus = playerStats?.GetStat(UpgradeData.WeaponType.Blade, UpgradeData.UpgradeType.Damage) ?? 0f;
        int currentDamage = baseDamage + Mathf.RoundToInt(damageBonus);

        for (int i = 0; i < totalBlades; i++)
        {
            GameObject blade = Instantiate(bladePrefab, transform.position, Quaternion.identity);
            var spin = blade.GetComponent<SpinningBlade>();

            spin.player = transform;
            spin.orbitRadius = baseOrbitRadius;
            spin.angleOffset = i * angleStep;
            spin.SetDamage(currentDamage);

            blades.Add(spin);
        }
    }

    protected override void Fire(Vector2 direction)
    {
        // Blades don't "fire", they spin passively.
    }

    protected override UpgradeData.WeaponType GetWeaponType()
    {
        return UpgradeData.WeaponType.Blade;
    }
}
