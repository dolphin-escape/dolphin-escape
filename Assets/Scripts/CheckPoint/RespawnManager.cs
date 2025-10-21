using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;
    public Vector3 respawnPosition = new Vector3(-5, 1, 0);

    private void Awake()
    {
        Instance = this;
    }

    public void Death()
    {
        transform.position = respawnPosition;
    }

    public void SetRespawnPosition(Vector3 newPosition)
    {
        respawnPosition = newPosition;
    }
}

