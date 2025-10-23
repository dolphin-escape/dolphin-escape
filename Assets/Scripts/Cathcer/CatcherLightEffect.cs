using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class CatcherLightEffect : MonoBehaviour
{
    [Header("Blink Settings")]
    [SerializeField] private Color caughtColor = Color.red; // Warna saat menangkap
    [SerializeField] private float blinkIntensity = 5f;     // Intensitas saat berkedip
    [SerializeField] private int blinkCount = 3;            // Jumlah kedipan
    [SerializeField] private float blinkDuration = 0.1f;    // Durasi satu kedipan (on/off)

    private Light2D spotLight;
    private Color originalColor;
    private float originalIntensity;
    private bool isBlinking = false; // Flag agar coroutine tidak tumpang tindih

    void Awake()
    {
        spotLight = GetComponent<Light2D>();
        
        // Simpan pengaturan awal lampu
        originalColor = spotLight.color;
        originalIntensity = spotLight.intensity;
    }

    // Ini adalah fungsi yang akan dipanggil oleh Player
    public void TriggerCaughtEffect()
    {
        // Hanya jalankan jika tidak sedang berkedip
        if (!isBlinking)
        {
            StartCoroutine(BlinkEffectCoroutine());
        }
    }

    private IEnumerator BlinkEffectCoroutine()
    {
        isBlinking = true;

        for (int i = 0; i < blinkCount; i++)
        {
            // --- FASE "ON" (Merah & Terang) ---
            spotLight.color = caughtColor;
            spotLight.intensity = blinkIntensity;
            yield return new WaitForSeconds(blinkDuration);

            // --- FASE "OFF" (Kembali normal) ---
            spotLight.color = originalColor;
            spotLight.intensity = originalIntensity;
            yield return new WaitForSeconds(blinkDuration);
        }

        // Pastikan lampu kembali ke kondisi normal setelah selesai
        spotLight.color = originalColor;
        spotLight.intensity = originalIntensity;
        
        isBlinking = false;
    }
}