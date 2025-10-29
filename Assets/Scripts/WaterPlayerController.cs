using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class WaterPlayerController : MonoBehaviour
{
    [Header("Virtual Controls")]
    [SerializeField] VirtualJoystick joystick;
    [SerializeField] VirtualJumpButton jumpButton;

    [Header("Layers")]
    [SerializeField] LayerMask waterMask;

    [Header("Non-Water (Fly)")]
    [SerializeField] float landGravityScale = 5f;

    [Header("Water (Swim)")]
    [SerializeField] float maxSwimSpeed = 15f;
    [SerializeField] float swimAccel = 50f;

    [Header("Jump Settings")]
    [SerializeField] float jumpForce = 200;
    [SerializeField] float jumpCoolDown = 1f;
    [SerializeField] float jumpForwardBoost = 5f;
    float nextJumpTime = 0f;

    [Header("Rotation Settings")]
    [SerializeField] float rotationSpeed = 5f;

    Rigidbody2D rb;
    public Vector2 input;
    public bool inWater;
    bool facingRight = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (joystick == null) joystick = FindFirstObjectByType<VirtualJoystick>();
        if (jumpButton == null) jumpButton = FindFirstObjectByType<VirtualJumpButton>();
    }

    void Update()
    {
        // Input
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (joystick != null)
            input = joystick.GetInputDirection();

        // Jump input
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space);
        if (jumpButton)
            jumpPressed = jumpPressed || jumpButton.GetJumpPressed();

        if (jumpPressed && Time.time >= nextJumpTime)
        {
            rb.linearVelocity += (Vector2)transform.right * jumpForwardBoost;
            rb.AddForce((Vector2)transform.right * jumpForce, ForceMode2D.Impulse);
            nextJumpTime = Time.time + jumpCoolDown;
        }

        // Sprite switching
        UpdateSpriteDirection();
    }

    void FixedUpdate()
    {
        if (inWater)
        {
            rb.gravityScale = 0;
            SwimForces();
            RotateTowardsInput();
        }
        else
        {
            rb.gravityScale = landGravityScale;
            FaceVelocitySmooth();
            AirForces();
            RotateTowardsInput();
        }
    }

    void SwimForces()
    {
        Vector2 targetVel = input.normalized * maxSwimSpeed;
        Vector2 velError = targetVel - rb.linearVelocity;
        float accelMult = Mathf.Lerp(0.5f, 1f, input.magnitude);
        Vector2 desiredA = Vector2.ClampMagnitude(velError / Time.fixedDeltaTime, swimAccel * accelMult);
        rb.AddForce(desiredA * rb.mass, ForceMode2D.Force);

        if (rb.linearVelocity.magnitude > maxSwimSpeed)
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, rb.linearVelocity.normalized * maxSwimSpeed, 0.2f);
    }

    void AirForces()
    {
        Vector2 targetVel = input.normalized * maxSwimSpeed * 2;
        Vector2 velError = targetVel - rb.linearVelocity;
        float accelMult = Mathf.Lerp(0.5f, 1f, input.magnitude);
        Vector2 desiredA = Vector2.ClampMagnitude(velError / Time.fixedDeltaTime, swimAccel * accelMult);
        rb.AddForce(desiredA * rb.mass, ForceMode2D.Force);
    }

    void RotateTowardsInput()
    {
        float rotationLerp = 0.5f;
        float magnitude = input.magnitude;
        if (magnitude < 0.15f)
            return;

        float targetAngle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        float currentAngle = transform.rotation.eulerAngles.z;
        float smoothAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.fixedDeltaTime * rotationLerp);
        transform.rotation = Quaternion.Euler(0, 0, smoothAngle);

        UpdateSpriteDirection();
    }

    void FaceVelocitySmooth()
    {
        Vector2 v = rb.linearVelocity;

        float target = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        var targetQ = Quaternion.Euler(0, 0, target);

        // Kecepatan rotasi konstan (linear), enak untuk timing
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetQ,
            360 * Time.fixedDeltaTime
        );
    }

    void UpdateSpriteDirection()
    {
        // Right jika cos(z) >= 0  → kuadran I & IV
        bool right = Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad) >= 0f;
        if (right != facingRight)
        {
            facingRight = right;
            transform.localScale = new Vector3(transform.localScale.x, facingRight ? Mathf.Abs(transform.localScale.y) : -Mathf.Abs(transform.localScale.y), transform.localScale.z);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Enter water layer
        if (((1 << other.gameObject.layer) & waterMask) != 0)
        {
            inWater = true;
            transform.rotation = facingRight
                ? Quaternion.identity
                : Quaternion.Euler(0f, 0f, -180f);
            rb.linearDamping = 0.3f;
            nextJumpTime -= jumpCoolDown;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Exit water layer
        if (((1 << other.gameObject.layer) & waterMask) != 0)
        {
            inWater = false;
            UpdateSpriteDirection();
        }
    }
}