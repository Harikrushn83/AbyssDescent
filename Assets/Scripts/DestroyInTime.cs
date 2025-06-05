using UnityEngine;

public class DestroyInTime : MonoBehaviour
{
    public float timeToDestroy = 2f; // You can set this value from the Inspector

    void Start()
    {
        Destroy(gameObject, timeToDestroy);
    }
}
