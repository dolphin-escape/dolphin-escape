using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    private BoxCollider2D checkCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        checkCollider = GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            // update the respawn point on the respawnController to the current checkpoint
            RespawnManager.Instance.SetRespawnPosition(transform.position);

            // stop the player using the old checkpoint
            checkCollider.enabled = false;
        }
    }
}
