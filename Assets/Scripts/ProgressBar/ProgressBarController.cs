using UnityEngine;
using UnityEngine.UI;

public class ProgressBarController : MonoBehaviour
{
    [Header("References")]
    public Transform player;               // your player in the scene
    public Transform levelStart;           // empty object at level start
    public Transform levelEnd;             // empty object at level end
    public Slider progressBar;             // the UI slider
    public Transform[] boxPositions;       // your box GameObjects
    public RectTransform[] boxMarkers;     // UI dots representing boxes
    public RectTransform playerMarker;     // the handle or player icon in the UI

    private float barWidth;

    void Start()
    {
        // Cache width of the slider bar in UI space
        barWidth = progressBar.GetComponent<RectTransform>().rect.width;
    }

    void Update()
    {
        if (!player || !levelStart || !levelEnd) return;

        float totalDistance = levelEnd.position.x - levelStart.position.x;
        float playerDistance = player.position.x - levelStart.position.x;
        float progress = Mathf.Clamp01(playerDistance / totalDistance);
        progressBar.value = progress;

        // Recalculate in case the UI layout changes
        barWidth = progressBar.GetComponent<RectTransform>().rect.width;

        // --- Move Player Marker (handle) ---
        if (playerMarker != null)
        {
            Vector2 pos = playerMarker.anchoredPosition;
            pos.x = progress * barWidth - (barWidth / 2f);
            playerMarker.anchoredPosition = pos;
        }

        // --- Update Box Markers ---
        for (int i = 0; i < boxPositions.Length; i++)
        {
            if (!boxMarkers[i]) continue;

            float boxProgress = Mathf.Clamp01(
                (boxPositions[i].position.x - levelStart.position.x) / totalDistance
            );

            Vector2 markerPos = boxMarkers[i].anchoredPosition;
            markerPos.x = boxProgress * barWidth - (barWidth / 2f);
            boxMarkers[i].anchoredPosition = markerPos;
        }
    }
}