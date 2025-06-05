using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 3f;
    public float mineLifetime = 5f; // Time before mine disappears
    public GameObject explosionPrefab;
    public GameObject minePrefab;

    private Vector2 targetPosition;
    private bool isMoving = true;

    [Header("Damage")]
    public int damage = 10;

    public void SetTarget(Vector2 target)
    {
        targetPosition = target;
    }

    void Update()
    {
        if (!isMoving) return;

        // Move toward the target position
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        // Calculate direction and rotate projectile
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        // If reached target position without hitting player, spawn mine
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            SpawnMine();
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Explode();
            Destroy(gameObject);
        }
    }

    void SpawnMine()
    {
        GameObject mine = Instantiate(minePrefab, targetPosition, Quaternion.identity);
        Destroy(mine, mineLifetime); // Mine disappears after delay if not triggered
    }

    void Explode()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        // Add damage logic here (e.g., PlayerHealth.TakeDamage())

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                hit.GetComponent<PlayerHealth>().TakeDamage(damage);
            }
        }
    }
}