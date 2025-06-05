using UnityEngine;

public class BounceOnHit : MonoBehaviour
{
    private Rigidbody2D rb;
    private ProjectileBase proj;

    public float randomAngleVariance = 10f; // degrees

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        proj = GetComponent<ProjectileBase>();
        //Debug.Log("start");
        rb.linearVelocity = proj.direction * proj.data.projectileSpeed;

    }

    void Update()
    {
        // Always maintain velocity
        if (proj != null)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * proj.data.projectileSpeed;
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Wall") || col.collider.CompareTag("Enemy") || col.collider.CompareTag("Boss"))
        {
            Vector2 normal = col.contacts[0].normal;
            Vector2 newDir = Vector2.Reflect(rb.linearVelocity.normalized, normal);

            // Apply random angle variation
            float angleOffset = Random.Range(-randomAngleVariance, randomAngleVariance);
            newDir = Quaternion.Euler(0f, 0f, angleOffset) * newDir;

            rb.linearVelocity = newDir.normalized * proj.data.projectileSpeed;
            //Debug.Log("coll");
            // Apply damage if it's an enemy
            if (col.collider.CompareTag("Enemy"))
            {
                //Debug.Log("ene");
                col.collider.GetComponent<EnemyAI>().TakeDamage((int)proj.data.damage);
            }
            if (col.collider.CompareTag("Boss"))
            {
                //Debug.Log("bos");
                col.collider.GetComponent<BossAI>().TakeDamage((int)proj.data.damage);
            }
        }
    }
}
