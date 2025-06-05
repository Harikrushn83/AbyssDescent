//using UnityEngine;

//public class BounceOnWall : MonoBehaviour
//{
//    private Rigidbody2D rb;

//    void Start() => rb = GetComponent<Rigidbody2D>();

//    void OnCollisionEnter2D(Collision2D col)
//    {
//        if (col.collider.CompareTag("Wall"))
//        {
//            Vector2 normal = col.contacts[0].normal;
//            rb.linearVelocity = Vector2.Reflect(rb.linearVelocity, normal);
//        }
//    }
//}
