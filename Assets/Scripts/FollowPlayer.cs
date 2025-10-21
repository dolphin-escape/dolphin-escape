using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform playerTransform;
    public float moveSpeed = 5f; 

    private float leftXBound = 0f;
    private float cameraZPosition = -10f;
    private float cameraYPosition = 0f;

    // FUNGSI BARU DITAMBAHKAN
    void Start()
    {
        // Tentukan di mana kamera HARUSNYA berada saat game mulai
        Vector3 initialPosition;

        if (playerTransform.position.x > leftXBound)
        {
            initialPosition = new Vector3(playerTransform.position.x, cameraYPosition, cameraZPosition);
        }
        else
        {
            // Karena player mulai di X = -5, posisi awal kamera harusnya di leftXBound (X = 0)
            initialPosition = new Vector3(leftXBound, cameraYPosition, cameraZPosition);
        }
        
        // Langsung "Snap" kamera ke posisi awal itu.
        // Ini akan menghentikan gerakan "ngikutin" di awal game.
        transform.position = initialPosition;
    }

    // Fungsi LateUpdate tetap sama seperti solusi saya sebelumnya
    void LateUpdate()
    {
        // 1. Tentukan dulu target posisi kamera
        Vector3 targetPosition;

        if (playerTransform.position.x > leftXBound)
        {
            // Jika player di sebelah kanan batas, targetnya adalah posisi X player
            targetPosition = new Vector3(playerTransform.position.x, cameraYPosition, cameraZPosition);
        }
        else
        {
            // Jika player di sebelah kiri batas, targetnya adalah batas itu sendiri (X=0)
            targetPosition = new Vector3(leftXBound, cameraYPosition, cameraZPosition);
        }

        // 2. SELALU bergerak perlahan ke target
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }
}