using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform playerTransform;
    public float moveSpeed = 10f;

    private FollowPlayerLogic logic;

    void Awake()
    {
        logic = new FollowPlayerLogic();
    }

    void Start()
    {
        transform.position = logic.CalculateTargetPosition(playerTransform.position);
    }

    void LateUpdate()
    {
        Vector3 target = logic.CalculateTargetPosition(playerTransform.position);
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }
}
