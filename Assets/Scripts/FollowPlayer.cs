using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform playerTransform;
    private float leftXBound = 2f;
    private float rightXBound = 200f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (playerTransform.position.x < leftXBound)
        {
            transform.position = new Vector3(leftXBound, 0, -10);
        }
        else if (playerTransform.position.x <= rightXBound)
        {
            transform.position = new Vector3(playerTransform.position.x, 0, -10);
        }
        else 
        {
            transform.position = new Vector3(rightXBound, 0, -10);
        }
    }
}
