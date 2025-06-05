using UnityEngine;

public class LaserHit : MonoBehaviour
{
    private ProjectileBase proj;

    void Awake()
    {
        proj = GetComponent<ProjectileBase>();
    }

    //void OnTriggerEnter2D(Collider2D col)
    //{
    //    //Debug.Log("triggered");
    //    if (col.CompareTag("Enemy"))
    //    {
    //        var enemy = col.GetComponent<EnemyAI>();
    //        if (enemy != null)
    //        {
    //            enemy.TakeDamage(proj.data.damage);
    //        }
    //        Destroy(gameObject);
    //    }
    //    if (col.CompareTag("Wall"))
    //    {
    //        //Debug.Log("wtrigger");
    //        if (proj.data.destroyOnWallHit)
    //            Destroy(gameObject);
    //    }
    //}
}
