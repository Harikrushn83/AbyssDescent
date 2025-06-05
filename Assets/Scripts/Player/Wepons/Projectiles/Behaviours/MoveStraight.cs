using UnityEngine;

public class MoveStraight : MonoBehaviour
{
    private ProjectileBase proj;

    void Start()
    {
        proj = GetComponent<ProjectileBase>();
    }

    void Update()
    {
        transform.Translate(proj.direction * proj.data.projectileSpeed * Time.deltaTime);

        if (Vector2.Distance(proj.startPos, transform.position) > proj.data.range)
            Destroy(gameObject);
    }
}
