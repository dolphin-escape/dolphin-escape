using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public Image joystickBackground;
    public Image joystickHandle;
    
    private Vector2 inputDirection = Vector2.zero;
    private Vector2 joystickStartPosition;
    
    [SerializeField]
    private float handleRange = 50f; // Maximum distance the handle can move from center

    void Start()
    {
        joystickStartPosition = joystickHandle.rectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 direction = eventData.position - (Vector2)joystickBackground.rectTransform.position;
        
        // Calculate input direction normalized
        inputDirection = (direction.magnitude > joystickBackground.rectTransform.sizeDelta.x / 2f) 
            ? direction.normalized 
            : direction / (joystickBackground.rectTransform.sizeDelta.x / 2f);
        
        // Move the handle
        joystickHandle.rectTransform.anchoredPosition = 
            joystickStartPosition + inputDirection * handleRange;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Reset joystick
        inputDirection = Vector2.zero;
        joystickHandle.rectTransform.anchoredPosition = joystickStartPosition;
    }

    public Vector2 GetInputDirection()
    {
        return inputDirection;
    }
}