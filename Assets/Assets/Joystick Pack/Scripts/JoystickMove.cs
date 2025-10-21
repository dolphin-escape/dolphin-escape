using UnityEngine;

public class JoystickMove : MonoBehaviour
{
    [Header("Joystick & Movement")]
    public Joystick movementJoystick;
    public float playerSpeed = 5f;
    public float rotationSpeed = 10f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool canMove = true; // 🔹 will be false when hitting the box

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = movementJoystick.Direction;

        if (direction.magnitude > 0.1f)
        {
            // Move dolphin
            rb.linearVelocity = direction * playerSpeed;

            // Get rotation angle (same as your original code)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Rotate dolphin to face the movement direction
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

            // Keep your original flip logic
            if (spriteRenderer != null)
            {
                if (direction.x < 0)
                    spriteRenderer.flipY = true;   // flip vertically when moving left
                else
                    spriteRenderer.flipY = false;  // normal when moving right
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // 🔹 When dolphin touches a box (trigger)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Box"))
        {
            canMove = false;
            rb.linearVelocity = Vector2.zero;
        }
    }

    // 🔹 When dolphin leaves the box
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Box"))
        {
            canMove = true;
        }
    }
}