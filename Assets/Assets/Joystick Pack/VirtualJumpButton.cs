using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJumpButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool isPressed = false;
    private bool jumpTriggered = false;

    [Header("Visual Feedback")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);

    void Awake()
    {
        if (buttonImage == null)
        {
            buttonImage = GetComponent<Image>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        jumpTriggered = true;
        
        if (buttonImage != null)
        {
            buttonImage.color = pressedColor;
        }
        
        Debug.Log("Jump button pressed");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        
        if (buttonImage != null)
        {
            buttonImage.color = normalColor;
        }
        
        Debug.Log("Jump button released");
    }

    /// <summary>
    /// Returns true only once per press (like GetKeyDown)
    /// </summary>
    public bool GetJumpPressed()
    {
        if (jumpTriggered)
        {
            jumpTriggered = false;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true while button is held down (like GetKey)
    /// </summary>
    public bool IsPressed()
    {
        return isPressed;
    }

    void OnDisable()
    {
        isPressed = false;
        jumpTriggered = false;
    }
}