using UnityEngine;

public abstract class AutoWeaponBase : WeaponBase
{
    public virtual void AutoFire()
    {
        // Default AutoFire logic for projectile weapons
        GameObject nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null) return;

        Vector2 direction = (nearestEnemy.transform.position - transform.position).normalized;

        TryFire(direction);
    }

    protected GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");

        GameObject closest = null;
        float shortestDist = Mathf.Infinity;

        foreach (var target in enemies)
        {
            float dist = Vector2.Distance(transform.position, target.transform.position);
            if (dist < shortestDist)
            {
                shortestDist = dist;
                closest = target;
            }
        }

        foreach (var target in bosses)
        {
            float dist = Vector2.Distance(transform.position, target.transform.position);
            if (dist < shortestDist)
            {
                shortestDist = dist;
                closest = target;
            }
        }

        return closest;
    }
}
