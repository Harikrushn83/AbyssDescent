//using UnityEngine;

//public class ShotgunFire : MonoBehaviour, IFireType
//{
//    public int pelletCount = 3;
//    public float spreadAngle = 15f;

//    public void Fire(Vector2 direction, WeaponData data, Transform firePoint)
//    {
//        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
//        int mid = pelletCount / 2;

//        for (int i = 0; i < pelletCount; i++)
//        {
//            float angleOffset = (i - mid) * spreadAngle;
//            float finalAngle = baseAngle + angleOffset;

//            Vector2 shootDir = new Vector2(
//                Mathf.Cos(finalAngle * Mathf.Deg2Rad),
//                Mathf.Sin(finalAngle * Mathf.Deg2Rad)
//            ).normalized;

//            GameObject proj = Instantiate(data.projectilePrefab, firePoint.position, Quaternion.identity);
//            var baseProj = proj.GetComponent<ProjectileBase>();
//            baseProj.Init(shootDir, data);
//        }
//    }
//}
