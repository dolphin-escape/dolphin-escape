using UnityEngine;

public class FollowPlayerXY : MonoBehaviour
{
    [Header("Target")]
    public Transform playerTransform;

    [Header("Follow Settings")]
    public float moveSpeed = 10f; // Seberapa cepat kamera mengejar target

    [Header("X Clamp")]
    public float minX = 0f;       // Batas paling kiri kamera
    public float maxX = 100f;     // Batas paling kanan kamera

    [Header("Y Clamp")]
    public float minY = 0f;       // Batas paling bawah kamera
    public float maxY = 50f;      // Batas paling atas kamera

    [Header("Z / Depth Lock")]
    public float cameraZPosition = -10f;

    private FollowPlayerXYLogic logic;

    private void Awake()
    {
        // Inisialisasi logic helper untuk clamping
        logic = new FollowPlayerXYLogic();
    }

    private void Start()
    {
        // Set posisi awal kamera berdasarkan posisi player yang sudah di-clamp
        transform.position = logic.CalculateClampedPosition(
            playerTransform.position,
            minX, maxX,
            minY, maxY,
            cameraZPosition
        );
    }

    private void LateUpdate()
    {
        // Hitung posisi target kamera (dengan clamp)
        Vector3 targetPosition = logic.CalculateClampedPosition(
            playerTransform.position,
            minX, maxX,
            minY, maxY,
            cameraZPosition
        );

        // Gerakkan kamera mendekati target posisi
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }
}
