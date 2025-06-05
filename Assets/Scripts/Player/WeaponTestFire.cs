using UnityEngine;

public class WeaponTestFire : MonoBehaviour
{
    public WeaponBase weapon;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mouseWorld - transform.position);
            Debug.Log($"Fire input detected. Direction: {direction}");
            weapon.TryFire(direction);
        }
    }
}
