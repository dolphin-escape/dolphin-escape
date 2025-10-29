using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform playerTransform;
    public float moveSpeed = 10f; 

    private float leftXBound = 3.5f;
    private float cameraZPosition = -10f;
    private float cameraYPosition = 0f;

    void Start()
    {
        Vector3 initialPosition;

        if (playerTransform.position.x > leftXBound)
        {
            initialPosition = new Vector3(playerTransform.position.x, cameraYPosition, cameraZPosition);
        }
        else
        {
            initialPosition = new Vector3(leftXBound, cameraYPosition, cameraZPosition);
        }
        
        transform.position = initialPosition;
    }

    void LateUpdate()
    {
        Vector3 targetPosition;

        if (playerTransform.position.x > leftXBound)
        {
            targetPosition = new Vector3(playerTransform.position.x, cameraYPosition, cameraZPosition);
        }
        else
        {
            targetPosition = new Vector3(leftXBound, cameraYPosition, cameraZPosition);
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }
}