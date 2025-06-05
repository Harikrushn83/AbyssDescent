using UnityEngine;

public class SpinningBlade : MonoBehaviour
{
    public Transform player;
    public float orbitRadius = 2f;
    public float angleOffset;
    public int damage = 10;

    public void SetDamage(int value)
    {
        damage = value;
    }

    public void UpdateBladePosition(float sharedAngle)
    {
        if (player == null) return;

        float totalAngle = sharedAngle + angleOffset;
        float rad = totalAngle * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * orbitRadius;

        transform.position = (Vector2)player.position + offset;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyAI>().TakeDamage(damage);
        }
        if (other.CompareTag("Boss"))
        {
            other.GetComponent<BossAI>().TakeDamage(damage);
        }
    }
}
