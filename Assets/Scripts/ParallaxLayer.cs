using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Tooltip("Camera to follow (usually your main camera).")]
    [SerializeField] private Transform cam;

    [Tooltip("How strong the parallax is. Bigger = moves more. Far background = small number, like 0.1")]
    [SerializeField] private float parallaxStrength = 0.2f;

    // internal tracking
    private Vector3 startPos;        // where this layer started
    private Vector3 camStartPos;     // where the camera started

    void Start()
    {
        if (cam == null)
        {
            cam = Camera.main.transform;
        }

        startPos = transform.position;
        camStartPos = cam.position;
    }

    void LateUpdate()
    {
        // how much camera moved from the start
        Vector3 camDelta = cam.position - camStartPos;

        // apply a fraction of that movement to this layer
        Vector3 targetPos = startPos + camDelta * parallaxStrength;

        // keep original Z (so it doesn't jump forward/back in depth)
        targetPos.y = startPos.y;
        targetPos.z = startPos.z;

        transform.position = targetPos;
    }
}
