using UnityEngine;

public class PierceEnemy : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // Deal damage but DON'T destroy this projectile
            //collision.GetComponent<EnemyHealth>()?.TakeDamage(10);
        }
    }
}
