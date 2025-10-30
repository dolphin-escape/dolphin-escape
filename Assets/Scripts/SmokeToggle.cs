using UnityEngine;

public class SmokePattern : MonoBehaviour
{
    private ParticleSystem.EmissionModule emission;

    void Start()
    {
        var smoke = GetComponent<ParticleSystem>();
        emission = smoke.emission;
        StartCoroutine(SmokeCycle());
    }

    System.Collections.IEnumerator SmokeCycle()
    {
        while (true)
        {
            // 🔹 Big puff
            emission.rateOverTime = 40f;
            yield return new WaitForSeconds(5f);

            // 🔹 Stop
            emission.rateOverTime = 0f;
            yield return new WaitForSeconds(1f);

            // 🔹 Small puff
            emission.rateOverTime = 10f;
            yield return new WaitForSeconds(0.5f);

            // 🔹 Stop again (longer pause)
            emission.rateOverTime = 0f;
            yield return new WaitForSeconds(5f);
        }
    }
}