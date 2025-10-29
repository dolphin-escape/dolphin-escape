using UnityEngine;

public class FollowPlayerXY : MonoBehaviour
{
    [Header("Target")]
    public Transform playerTransform;

    [Header("Follow Settings")]
    public float moveSpeed = 10f; // seberapa cepat kamera ngejar target

    [Header("X Clamp")]
    public float minX = 0f;       // batas paling kiri kamera
    public float maxX = 100f;     // batas paling kanan kamera

    [Header("Y Clamp")]
    public float minY = 0f;       // batas paling bawah kamera
    public float maxY = 50f;      // batas paling atas kamera

    [Header("Z / Depth Lock")]
    public float cameraZPosition = -10f;

    private void Start()
    {
        // Hitung posisi awal kamera yang valid (sudah di-clamp)
        float clampedX = Mathf.Clamp(playerTransform.position.x, minX, maxX);
        float clampedY = Mathf.Clamp(playerTransform.position.y, minY, maxY);

        transform.position = new Vector3(
            clampedX,
            clampedY,
            cameraZPosition
        );
    }

    private void LateUpdate()
    {
        // Cari target posisi berdasarkan posisi player,
        // tapi dibatasi oleh min/max
        float clampedX = Mathf.Clamp(playerTransform.position.x, minX, maxX);
        float clampedY = Mathf.Clamp(playerTransform.position.y, minY, maxY);

        Vector3 targetPosition = new Vector3(
            clampedX,
            clampedY,
            cameraZPosition
        );

        // Gerakkan kamera mendekati target
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }
}
