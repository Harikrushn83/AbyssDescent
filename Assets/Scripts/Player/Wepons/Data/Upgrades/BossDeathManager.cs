using UnityEngine;

public class BossDeathManager : MonoBehaviour
{
    [SerializeField] private WeaponUnlocks weaponUnlocks;

    // Call this method when the boss dies
    public void OnBossDeath()
    {
        // Unlock weapon of choice (Laser, for example)
        weaponUnlocks.UnlockWeapon(UpgradeData.WeaponType.Laser);

        // Optionally, you can also trigger some event here for UI, chest opening, etc.
        Debug.Log("Boss defeated! Laser unlocked!");
    }
}
