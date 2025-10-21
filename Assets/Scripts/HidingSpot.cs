using UnityEngine;


// public class HidingSpot : MonoBehaviour
// {
//     private PolygonCollider2D polyCollider;
//     private float checkInterval = 0.1f;
//     private float lastCheckTime;

//     void Start()
//     {
//         polyCollider = GetComponent<PolygonCollider2D>();
//     }

//     void OnTriggerStay2D(Collider2D other)
//     {
//         Debug.Log(other + " IDK is touching");
//         if (!other.CompareTag("Player"))
//             return;

//         if(Time.time - lastCheckTime < checkInterval)
//             return;

//         Debug.Log(other + " Player is touching");

//         lastCheckTime = Time.time;

//         DetectCatcher player = other.GetComponent<DetectCatcher>();
//         if(player == null)
//             return;

//         PolygonCollider2D playerCollider = other.GetComponent<PolygonCollider2D>();

//         if (IsFullyInside(playerCollider, polyCollider)) {
//             print("Player fully inside" + gameObject.name);
//             player.isHiding = true;
//         } 
//         else
//             player.isHiding = false;
//     }

//     void OnTriggerExit2D(Collider2D other)
//     {
//         if(!other.CompareTag("Player"))
//             return;

//         DetectCatcher player = other.GetComponent<DetectCatcher>();
//         if(player != null)
//             player.isHiding = false;
//     }

//     bool IsFullyInside(PolygonCollider2D inner, PolygonCollider2D outer)
//     {
//         Vector2[] points = inner.points;
//         Transform innerTransform = inner.transform;
//         Vector2 innerOffset = inner.offset;

//         for (int i = 0; i < points.Length; i++)
//         {
//             Vector2 localPointWithOffset = points[i] + innerOffset;
//             Vector2 worldPoint = innerTransform.TransformPoint(localPointWithOffset);

//             if(!outer.OverlapPoint(worldPoint))
//                 return false;
//         }

//         return true;
//     }
// }


// Di HidingSpot script
public class HidingSpot : MonoBehaviour
{
    private PolygonCollider2D polyCollider;

    void Start()
    {
        polyCollider = GetComponent<PolygonCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        DetectCatcher player = other.GetComponent<DetectCatcher>();
        if (player != null)
        {
            player.RegisterHidingSpot(this);
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        DetectCatcher player = other.GetComponent<DetectCatcher>();
        if (player != null)
        {
            player.UnregisterHidingSpot(this);
        }
    }

    public bool ContainsPoint(Vector2 worldPoint)
    {
        return polyCollider.OverlapPoint(worldPoint);
    }
}
