using UnityEngine;

public class InfiniteLightMovement : MonoBehaviour
{
    public Transform[] waypoints;
    public float[] segmentSpeeds;
    public float defaultSpeed = 2f;
    public float arriveThreshold = 0.01f;

    public enum EasingType { Linear, EaseInOut, EaseIn, EaseOut, Smooth }
    public EasingType easingType = EasingType.Linear;

    Rigidbody2D rb;
    int targetIndex = 1;
    int dir = 1;

    Vector2 segmentStart;
    float segmentLength;
    float segmentProgress;
    float currentSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            enabled = false;
            return;
        }

        // Always snap to first waypoint
        Vector2 startPos = waypoints[0].position;
        if (rb) rb.position = startPos;
        else transform.position = startPos;

        InitSegment(startPos);
    }

    void InitSegment(Vector2 pos)
    {
        segmentStart = pos;
        segmentLength = Vector2.Distance(pos, waypoints[targetIndex].position);
        segmentProgress = 0f;

        int segIdx = dir > 0 ? targetIndex - 1 : targetIndex;
        segIdx = Mathf.Clamp(segIdx, 0, waypoints.Length - 2);
        currentSpeed = (segmentSpeeds != null && segIdx < segmentSpeeds.Length) ? segmentSpeeds[segIdx] : defaultSpeed;
    }

    void Update()
    {
        if (rb) return;
        transform.position = Step(transform.position, Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (!rb) return;
        rb.MovePosition(Step(rb.position, Time.fixedDeltaTime));
    }

    Vector2 Step(Vector2 pos, float dt)
    {
        if (currentSpeed <= 0f || segmentLength <= arriveThreshold) return pos;

        segmentProgress += (currentSpeed * dt) / segmentLength;

        if (segmentProgress >= 1f)
        {
            Vector2 target = waypoints[targetIndex].position;
            Advance();
            InitSegment(target);
            return target;
        }

        float t = Ease(segmentProgress);
        return Vector2.Lerp(segmentStart, waypoints[targetIndex].position, t);
    }

    void Advance()
    {
        if (targetIndex == waypoints.Length - 1) dir = -1;
        else if (targetIndex == 0) dir = 1;
        targetIndex = Mathf.Clamp(targetIndex + dir, 0, waypoints.Length - 1);
    }

    float Ease(float t)
    {
        t = Mathf.Clamp01(t);
        switch (easingType)
        {
            case EasingType.EaseInOut: return t < 0.5f ? 2f * t * t : 1f - 2f * (1f - t) * (1f - t);
            case EasingType.EaseIn: return t * t;
            case EasingType.EaseOut: return 1f - (1f - t) * (1f - t);
            case EasingType.Smooth: return t * t * (3f - 2f * t);
            default: return t;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (!waypoints[i]) continue;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(waypoints[i].position, 0.1f);

            if (i < waypoints.Length - 1 && waypoints[i + 1])
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);

                Vector3 mid = (waypoints[i].position + waypoints[i + 1].position) * 0.5f;
                float spd = (segmentSpeeds != null && i < segmentSpeeds.Length) ? segmentSpeeds[i] : defaultSpeed;
                UnityEditor.Handles.Label(mid, $"v:{spd}");
            }
        }
    }
#endif
}