using UnityEngine;

public class ProjectileBase : MonoBehaviour
{
    public WeaponData data;
    public Vector2 direction;
    public Vector2 startPos;

    private float currentAliveTime;

    public void Init(Vector2 dir, WeaponData weaponData)
    {
        direction = dir.normalized;
        data = weaponData;
        startPos = transform.position;

        currentAliveTime = data.aliveTime;

        // Rotate the projectile to face the direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Update()
    {
        bool hasRigidbody = TryGetComponent<Rigidbody2D>(out var rb) && rb.bodyType != RigidbodyType2D.Static;

        if (!hasRigidbody)
        {
            // Manual movement for non-physics projectiles
            transform.Translate((Vector3)direction * data.projectileSpeed * Time.deltaTime, Space.World);

            if (Vector2.Distance(startPos, transform.position) > data.range)
                Destroy(gameObject);
        }

        currentAliveTime -= Time.deltaTime;
        if (currentAliveTime <= 0f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("tri");
        if (other.CompareTag("Enemy"))
        {
            //Debug.Log("etri");
            other.GetComponent<EnemyAI>().TakeDamage((int)data.damage);

            if (data.destroyOnEnemyHit)
                Destroy(gameObject);
        }

        if (other.CompareTag("Boss"))
        {
            //Debug.Log("btri");
            other.GetComponent<BossAI>().TakeDamage((int)data.damage);

            if (data.destroyOnEnemyHit)
                Destroy(gameObject);
        }

        if (other.CompareTag("Wall"))
        {
            //Debug.Log("wtri");
            if (data.destroyOnWallHit)
                Destroy(gameObject);
        }
    }
}