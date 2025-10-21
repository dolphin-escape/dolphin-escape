using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JumpButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Jump Settings")]
    public float jumpForce = 10f;
    public Rigidbody2D dolphinRigidbody;

    [Header("Optional: Cooldown")]
    public float jumpCooldown = 0.5f;
    private float lastJumpTime = -999f;

    [Header("Optional: Visual Feedback")]
    public Button buttonComponent;
    private ColorBlock originalColors;

    void Start()
    {
        // Auto-find dolphin if not assigned
        if (dolphinRigidbody == null)
        {
            GameObject dolphin = GameObject.FindGameObjectWithTag("Player");
            if (dolphin != null)
            {
                dolphinRigidbody = dolphin.GetComponent<Rigidbody2D>();
            }
        }

        // Store original button colors
        if (buttonComponent != null)
        {
            originalColors = buttonComponent.colors;
        }
    }

    // When button is pressed down
    public void OnPointerDown(PointerEventData eventData)
    {
        Jump();
    }

    // When button is released (optional, if you want to do something)
    public void OnPointerUp(PointerEventData eventData)
    {
        // Optional: Add any release behavior here
    }

    void Jump()
    {
        Debug.Log("Jump button clicked!"); // 🔍 Debug message
        
        if (dolphinRigidbody == null)
        {
            Debug.LogError("Dolphin Rigidbody is not assigned!"); // 🔍 Error check
            return;
        }

        // Check cooldown
        if (Time.time - lastJumpTime < jumpCooldown)
        {
            Debug.Log("Jump on cooldown!"); // 🔍 Cooldown check
            return;
        }

        // 🔥 KEY: Reset vertical velocity to 0 first (like your friend's code)
        // This makes jump consistent and powerful!
        dolphinRigidbody.linearVelocity = new Vector2(dolphinRigidbody.linearVelocity.x, 0f);
        
        // Add upward force
        dolphinRigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        Debug.Log("Jump force applied: " + jumpForce); // 🔍 Confirm jump
        
        lastJumpTime = Time.time;

        // Optional: Visual feedback
        StartCoroutine(ButtonPressEffect());
    }

    // Optional: Visual feedback when button pressed
    System.Collections.IEnumerator ButtonPressEffect()
    {
        if (buttonComponent != null)
        {
            ColorBlock colors = buttonComponent.colors;
            colors.normalColor = colors.pressedColor;
            buttonComponent.colors = colors;

            yield return new WaitForSeconds(0.1f);

            buttonComponent.colors = originalColors;
        }
    }
}