using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private FloatingJoystick joystick;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    public Vector2 MoveDirection => moveInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void HandleInput()
    {
        //float moveX = Input.GetAxisRaw("Horizontal");
        //float moveY = Input.GetAxisRaw("Vertical");

        //// If keyboard is not being used, use joystick input
        //if (moveX == 0 && moveY == 0 && joystick != null)
        //{
        //    moveX = joystick.Horizontal;
        //    moveY = joystick.Vertical;
        //}

        float moveX = joystick.Horizontal;
        float moveY = joystick.Vertical;

        moveInput = new Vector2(moveX, moveY).normalized;
    }

    private void MovePlayer()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }
}
