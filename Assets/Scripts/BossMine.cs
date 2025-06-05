using UnityEngine;

public class BossMine : MonoBehaviour
{
    public GameObject explosionPrefab;

    [Header("Damage")]
    public int damage = 15;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Explode();
            Destroy(gameObject);
        }
    }

    void Explode()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        // Add damage logic here (same as projectile)

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 2f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                hit.GetComponent<PlayerHealth>().TakeDamage(damage);
            }
        }
    }
}