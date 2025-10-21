using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class AmphibiousAddForceController2D : MonoBehaviour
{
    [Header("Virtual Controls")]
    [SerializeField] VirtualJoystick joystick;
    [SerializeField] VirtualJumpButton jumpButton;

    [Header("Sprites")]
    [SerializeField] Sprite dolphinRightSprite;
    [SerializeField] Sprite dolphinLeftSprite;

    [Header("Layers")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] LayerMask waterMask;

    [Header("Land/Air (Platformer)")]
    [SerializeField] float maxLandSpeed = 8f;
    [SerializeField] float landAccel = 60f;
    [SerializeField] float landBrake = 90f;
    [SerializeField] float landGravityScale = 3f;
    [SerializeField] float airControl = 0.7f;
    [SerializeField] float jumpForce = 12f;
    [SerializeField] float coyoteTime = 0.15f;
    [SerializeField] float jumpBufferTime = 0.1f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.15f;

    [Header("Water (Swim)")]
    [SerializeField] float maxSwimSpeed = 15f;
    [SerializeField] float swimAccel = 50f;
    [SerializeField] float waterDrag = 4f;
    [SerializeField] float waterAngularDrag = 8f;
    [SerializeField] float waterEntryVelocityDamping = 0.3f;
    [SerializeField] float waterJumpForce = 18f;
    [SerializeField] float waterSurfaceOffset = 0.5f;
    [SerializeField] float swimDeadzone = 0.1f;
    [SerializeField] float waterGravityScale = 0.2f;
    [SerializeField] float surfacePushDownForce = 5f;

    [Header("Jump Settings")]
    [SerializeField] int maxJumps = 2;
    [SerializeField] float variableJumpMultiplier = 0.5f;
    [SerializeField] float firstJumpForce = 10f;
    [SerializeField] float secondJumpForce = 16f;
    [SerializeField] float jumpForwardBoost = 3f;

    [Header("Rotation Settings")]
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float landRotationSpeed = 15f;
    [SerializeField] float fallRotationSpeed = 8f;
    [SerializeField] bool enableRotation = true;

    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Vector2 input;
    bool inWater;
    float defaultDrag;
    int currentJumps = 0;
    bool wasGrounded = false;
    bool jumpPressed = false;
    bool jumpHeld = false;
    bool facingRight = true;
    float coyoteTimeCounter;
    float jumpBufferCounter;
    float waterSurfaceY;
    bool hasWaterSurface;
    bool isAirborne = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        defaultDrag = rb.linearDamping;

        if (joystick == null) joystick = FindObjectOfType<VirtualJoystick>();
        if (jumpButton == null) jumpButton = FindObjectOfType<VirtualJumpButton>();

        // Set sprite awal ke kanan
        if (spriteRenderer != null && dolphinRightSprite != null)
            spriteRenderer.sprite = dolphinRightSprite;
    }

    void Update()
    {
        // CHANGED
        inWater = true;


        // Input
        if (joystick != null)
            input = joystick.GetInputDirection();
        else
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Sprite switching
        if (input.x < -0.01f)
        {
            facingRight = false;
            if (spriteRenderer && dolphinLeftSprite) spriteRenderer.sprite = dolphinLeftSprite;
        }
        else if (input.x > 0.01f)
        {
            facingRight = true;
            if (spriteRenderer && dolphinRightSprite) spriteRenderer.sprite = dolphinRightSprite;
        }

        // Ground check
        bool isGrounded = IsGrounded();
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            isAirborne = false;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            if (!inWater) isAirborne = true;
        }

        if (isGrounded && !wasGrounded) currentJumps = 0;
        wasGrounded = isGrounded;

        if (inWater && currentJumps > 0)
        {
            currentJumps = 0;
            isAirborne = false;
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
                if (IsNearWaterSurface())
                {
                    jumpBufferCounter = 0f;
                    float forwardDir = facingRight ? 1f : -1f;
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x + (jumpForwardBoost * forwardDir), 0f);
                    rb.AddForce(Vector2.up * waterJumpForce, ForceMode2D.Impulse);
                    isAirborne = true;
                }
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
                    rb.AddForce(Vector2.up * jumpForceToUse, ForceMode2D.Impulse);
                    isAirborne = true;
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
            rb.linearDamping = waterDrag;
            rb.angularDamping = waterAngularDrag;
            SwimForces();
        }
        else
        {
            rb.gravityScale = landGravityScale;
            rb.linearDamping = defaultDrag;
            rb.angularDamping = 0.05f;
            LandAirForces();
        }

        // Rotation logic
        if (enableRotation)
        {
            if (inWater)
                RotateTowardsInput();
            else if (isAirborne && rb.linearVelocity.y < 0f)
                RotateHeadDown();
            else
            {
                // Reset rotation to horizontal when grounded
                Quaternion targetRot = Quaternion.identity;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, landRotationSpeed * Time.fixedDeltaTime);
            }
        }
    }

    void LandAirForces()
    {
        float targetVx = input.x * maxLandSpeed;
        float vxError = targetVx - rb.linearVelocity.x;
        float controlMultiplier = IsGrounded() ? 1f : airControl;
        float maxAx = (Mathf.Abs(targetVx) > 0.01f) ? landAccel : landBrake;
        maxAx *= controlMultiplier;
        float desiredAx = Mathf.Clamp(vxError / Time.fixedDeltaTime, -maxAx, maxAx);
        rb.AddForce(new Vector2(desiredAx * rb.mass, 0f), ForceMode2D.Force);

        float clampedVx = Mathf.Clamp(rb.linearVelocity.x, -maxLandSpeed, maxLandSpeed);
        rb.linearVelocity = new Vector2(clampedVx, rb.linearVelocity.y);
    }

    void SwimForces()
    {
        Vector2 processedInput = input.magnitude > swimDeadzone ? input : Vector2.zero;
        Vector2 targetVel = processedInput.normalized * maxSwimSpeed;
        Vector2 velError = targetVel - rb.linearVelocity;
        float accelMult = Mathf.Lerp(0.5f, 1f, processedInput.magnitude);
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
    else
    {
        Quaternion targetRotation = Quaternion.identity;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, landRotationSpeed * Time.fixedDeltaTime);

        if (spriteRenderer != null)
        {
            if (IsGrounded() && Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            {
                if (rb.linearVelocity.x < 0)
                {
                    facingRight = false;
                    spriteRenderer.sprite = dolphinLeftSprite;
                }
                else if (rb.linearVelocity.x > 0)
                {
                    facingRight = true;
                    spriteRenderer.sprite = dolphinRightSprite;
                }
            }
        }
    }
}

    void UpdateSpriteDirection()
    {
        if (spriteRenderer == null) return;

        if (input.x < -0.01f)
        {
            facingRight = false;
            spriteRenderer.sprite = dolphinLeftSprite;
        }
        else if (input.x > 0.01f)
        {
            facingRight = true;
            spriteRenderer.sprite = dolphinRightSprite;
        }
    }

    void RotateHeadDown()
    {
        float targetAngle = facingRight ? -90f : 90f;
        float currentAngle = transform.rotation.eulerAngles.z;
        if (currentAngle > 180f) currentAngle -= 360f;
        float smoothAngle = Mathf.LerpAngle(currentAngle, targetAngle, fallRotationSpeed * Time.fixedDeltaTime);
        transform.rotation = Quaternion.Euler(0, 0, smoothAngle);
    }

    bool IsGrounded()
    {
        if (!groundCheck) return rb.IsTouchingLayers(groundMask);
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
    }

    bool IsNearWaterSurface()
    {
        if (!hasWaterSurface) return false;
        return transform.position.y >= (waterSurfaceY - waterSurfaceOffset);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & waterMask) != 0)
        {
            inWater = true;
            Bounds waterBounds = other.bounds;
            waterSurfaceY = waterBounds.max.y;
            hasWaterSurface = true;
            rb.linearVelocity *= waterEntryVelocityDamping;

            if (spriteRenderer)
                spriteRenderer.sprite = facingRight ? dolphinRightSprite : dolphinLeftSprite;

            if (rb.linearVelocity.y < 0f || !isAirborne)
                transform.rotation = Quaternion.identity;
            isAirborne = false;
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