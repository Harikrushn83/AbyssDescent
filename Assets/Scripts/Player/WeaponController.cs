using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [System.Serializable]
    public class WeaponSlot
    {
        public UpgradeData.WeaponType weaponType;
        public WeaponBase directionalWeapon;
        public AutoWeaponBase autoWeapon;
    }

    public List<WeaponSlot> weapons = new List<WeaponSlot>();
    private WeaponUnlocks weaponUnlocks;

    void Awake()
    {
        weaponUnlocks = GetComponent<WeaponUnlocks>();
        if (weaponUnlocks == null)
        {
            Debug.LogError("Missing WeaponUnlocks on Player!");
        }
    }

    void Update()
    {
        Vector2 aimDirection = GetAimDirection();

        foreach (var slot in weapons)
        {
            if (!weaponUnlocks.IsWeaponUnlocked(slot.weaponType)) continue;

            if (slot.directionalWeapon != null)
            {
                slot.directionalWeapon.TryFire(aimDirection);
            }

            if (slot.autoWeapon != null)
            {
                slot.autoWeapon.AutoFire();
            }
        }
    }

    Vector2 GetAimDirection()
    {
        Vector2 dir = Vector2.right;
#if UNITY_EDITOR || UNITY_STANDALONE
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        dir = (mousePos - transform.position).normalized;
#elif UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            Vector3 touchPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            dir = (touchPos - transform.position).normalized;
        }
#endif
        return dir;
    }
}
