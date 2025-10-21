using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class WaterPlayerController : MonoBehaviour
{
    [SerializeField] Vector2 rbVelo;

    [Header("Virtual Controls")]
    [SerializeField] VirtualJoystick joystick;
    [SerializeField] VirtualJumpButton jumpButton;

    [Header("Sprites")]
    [SerializeField] Sprite dolphinRightSprite;
    [SerializeField] Sprite dolphinLeftSprite;

    [Header("Layers")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] LayerMask waterMask;

    [Header("Non-Water (Fly)")]
    [SerializeField] float landGravityScale = 3f;
    [SerializeField] float jumpForce = 1000f;
    [SerializeField] float coyoteTime = 0.15f;
    [SerializeField] float jumpBufferTime = 0.1f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.15f;

    [Header("Water (Swim)")]
    [SerializeField] float maxSwimSpeed = 15f;
    [SerializeField] float swimAccel = 50f;
    [SerializeField] float waterSurfaceOffset = 0.5f;

    [Header("Jump Settings")]
    [SerializeField] int maxJumps = 2;
    [SerializeField] float variableJumpMultiplier = 0.5f;
    [SerializeField] float firstJumpForce = 10f;
    [SerializeField] float secondJumpForce = 16f;
    [SerializeField] float jumpForwardBoost = 3f;

    [Header("Rotation Settings")]
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] bool enableRotation = true;

    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    public Vector2 input;
    bool inWater;
    int currentJumps = 0;
    bool wasGrounded = false;
    bool jumpPressed = false;
    bool jumpHeld = false;
    bool facingRight = true;
    float coyoteTimeCounter;
    float jumpBufferCounter;
    float waterSurfaceY;
    bool hasWaterSurface;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (joystick == null) joystick = FindFirstObjectByType<VirtualJoystick>();
        if (jumpButton == null) jumpButton = FindFirstObjectByType<VirtualJumpButton>();

        // Set sprite awal ke kanan
        if (spriteRenderer != null && dolphinRightSprite != null)
            spriteRenderer.sprite = dolphinRightSprite;
    }

    void Update()
    {
        // CHANGED
        // inWater = true;

        rbVelo = rb.linearVelocity;

        // Input
        if (joystick != null)
            input = joystick.GetInputDirection();
        else
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Sprite switching
        UpdateSpriteDirection();

        // Ground check
        bool isGrounded = IsGrounded();
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (isGrounded && !wasGrounded) currentJumps = 0;
        wasGrounded = isGrounded;

        if (inWater && currentJumps > 0)
        {
            currentJumps = 0;
        }

        // Jump input
        jumpPressed = Input.GetKeyDown(KeyCode.Space);
        jumpHeld = Input.GetKey(KeyCode.Space);
        if (jumpButton)
        {
            jumpPressed = jumpPressed || jumpButton.GetJumpPressed();
            jumpHeld = jumpHeld || jumpButton.IsPressed();
        }

        if (jumpPressed) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        // Jump logic
        if (jumpBufferCounter > 0f)
        {
            if (inWater)
            {
                // if (IsNearWaterSurface())
                // {
                    jumpBufferCounter = 0f;
                    float forwardDir = facingRight ? 1f : -1f;
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x + (jumpForwardBoost * forwardDir), 0f);
                    rb.AddForce(transform.right * jumpForce, ForceMode2D.Impulse);
                // }
            }
            else
            {
                if (coyoteTimeCounter > 0f || currentJumps < maxJumps)
                {
                    currentJumps++;
                    jumpBufferCounter = 0f;
                    float forwardDir = facingRight ? 1f : -1f;
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x + (jumpForwardBoost * forwardDir), 0f);
                    float jumpForceToUse = (currentJumps == 1) ? firstJumpForce : secondJumpForce;
                    rb.AddForce(transform.right * jumpForceToUse, ForceMode2D.Impulse);
                }
            }
        }

        // CHANGED
        if (!inWater && !jumpHeld && rb.linearVelocity.y > 0)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * variableJumpMultiplier);
    }

    void FixedUpdate()
    {
        if (inWater)
        {
            rb.gravityScale = 0;
            SwimForces();

        }
        else
        {
            rb.gravityScale = landGravityScale;
            FaceVelocitySmooth();
        }

        // Rotation logic
        if (enableRotation)
        {
            if (inWater)
                RotateTowardsInput();
            // else
            // {
            //     // Reset rotation to horizontal when grounded
            //     Quaternion targetRot = Quaternion.identity;
            //     transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, landRotationSpeed * Time.fixedDeltaTime);
            // }
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
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, rb.linearVelocity.normalized * maxSwimSpeed, 0.1f);
    }

    // 🐬 Fixed RotateTowardsInput (tanpa +180f, pakai shortest path)
    void RotateTowardsInput()
    {
        if (inWater)
        {
            bool nearSurface = hasWaterSurface &&
                            transform.position.y >= (waterSurfaceY - (waterSurfaceOffset * 0.3f));

            float rotationLerp = nearSurface ? 0.25f : 1f;
            float magnitude = input.magnitude;
            if (magnitude < 0.15f)
                return;

            // --- PERUBAHAN UTAMA ---
            // Hapus +180f karena sekarang sprite kiri kamu sudah manual dibalik di asset
            float targetAngle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;

            float currentAngle = transform.rotation.eulerAngles.z;
            float smoothAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.fixedDeltaTime * rotationLerp);

            transform.rotation = Quaternion.Euler(0, 0, smoothAngle);

            UpdateSpriteDirection();
        }
        // else
        // {
        //     Quaternion targetRotation = Quaternion.identity;
        //     transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, landRotationSpeed * Time.fixedDeltaTime);

        //     if (spriteRenderer != null)
        //     {
        //         if (IsGrounded() && Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        //         {
        //             if (rb.linearVelocity.x < 0)
        //             {
        //                 facingRight = false;
        //                 spriteRenderer.sprite = dolphinLeftSprite;
        //             }
        //             else if (rb.linearVelocity.x > 0)
        //             {
        //                 facingRight = true;
        //                 spriteRenderer.sprite = dolphinRightSprite;
        //             }
        //         }
        //     }
        // }
    }

    void FaceVelocitySmooth()
    {
        Vector2 v = rb.linearVelocity;

        float target = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        var targetQ  = Quaternion.Euler(0, 0, target);

        // Kecepatan rotasi konstan (linear), enak untuk timing
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetQ,
            360 * Time.fixedDeltaTime
        );
    }

    void UpdateSpriteDirection()
    {
        if (spriteRenderer == null) return;

        // Right jika cos(z) >= 0  → kuadran I & IV
        bool right = Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad) >= 0f;
        if (right != facingRight) {
            facingRight = right;
            if (spriteRenderer) spriteRenderer.sprite = right ? dolphinRightSprite : dolphinLeftSprite;
        }
    }

    bool IsGrounded()
    {
        if (!groundCheck) return rb.IsTouchingLayers(groundMask);
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & waterMask) != 0)
        {
            inWater = true;
            Bounds waterBounds = other.bounds;
            waterSurfaceY = waterBounds.max.y;
            hasWaterSurface = true;

            transform.rotation = facingRight 
                ? Quaternion.identity 
                : Quaternion.Euler(0f, 0f, -180f);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & waterMask) != 0)
        {
            inWater = false;
            hasWaterSurface = false;

            if (spriteRenderer)
                spriteRenderer.sprite = facingRight ? dolphinRightSprite : dolphinLeftSprite;
        }
    }
}