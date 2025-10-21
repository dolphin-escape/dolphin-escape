using UnityEngine;
using System.Collections.Generic;

// public class DetectCatcher : MonoBehaviour
// {
//     SpriteRenderer spriteRenderer;
//     public bool isHiding = false;

//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
//         spriteRenderer = GetComponent<SpriteRenderer>();
//     }

//     // Update is called once per frame
//     void Update()
//     {
//     }

//     void OnTriggerStay2D(Collider2D collision)
//     {
//         if (collision.CompareTag("Catcher"))
//         {
//             if (isHiding)
//             {
//                 Debug.Log("Uncaught");
//                 spriteRenderer.color = new Color(255 / 255f, 255 / 255f, 255 / 255f);
//             }
//             else
//             {
//                 Debug.Log("Caught");
//                 spriteRenderer.color = new Color(155 / 255f, 0 / 255f, 0 / 255f);
//             }   
//         }
//     }

//     void OnTriggerExit2D(Collider2D collision)
//     {
//         if (collision.CompareTag("Catcher"))
//         {
//             Debug.Log("Uncaught");
//             spriteRenderer.color = new Color(255 / 255f, 255 / 255f, 255 / 255f);
//         }
//     }
// }

// Di Player script (DetectCatcher)
using UnityEngine;
using System.Collections.Generic;

public class DetectCatcher : MonoBehaviour
{
    private HashSet<HidingSpot> currentHidingSpots = new HashSet<HidingSpot>();
    private SpriteRenderer spriteRenderer;
    private PolygonCollider2D playerCollider;
    public bool isHiding;
    
    void Start()
    {
        playerCollider = GetComponent<PolygonCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    public void RegisterHidingSpot(HidingSpot spot)
    {
        currentHidingSpots.Add(spot);
        CheckIfFullyHidden();
    }
    
    public void UnregisterHidingSpot(HidingSpot spot)
    {
        currentHidingSpots.Remove(spot);
        CheckIfFullyHidden();
    }
    
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Catcher"))
        {
            Collider2D catcherCollider = collision;

            if (IsCaughtBy(catcherCollider))
            {
                Debug.Log("Caught");
                spriteRenderer.color = new Color(155 / 255f, 0 / 255f, 0 / 255f);
                RespawnManager.Instance.Death();
            }
            else
            {
                Debug.Log("Uncaught - Hiding successfully");
                spriteRenderer.color = new Color(255 / 255f, 255 / 255f, 255 / 255f);
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Catcher"))
        {
            Debug.Log("Uncaught - Left catcher area");
            spriteRenderer.color = new Color(255 / 255f, 255 / 255f, 255 / 255f);
        }
    }
    
    bool IsCaughtBy(Collider2D catcherCollider)
    {
        Vector2[] points = playerCollider.points;
        Vector2 offset = playerCollider.offset;
        
        foreach (Vector2 point in points)
        {
            Vector2 localPointWithOffset = point + offset;
            Vector2 worldPoint = transform.TransformPoint(localPointWithOffset);
            
            // Cek apakah point ini di dalam hiding spot
            bool pointIsHidden = false;
            foreach (HidingSpot spot in currentHidingSpots)
            {
                if (spot.ContainsPoint(worldPoint))
                {
                    pointIsHidden = true;
                    break;
                }
            }
            
            // Jika point TIDAK hidden, cek apakah di dalam catcher
            if (!pointIsHidden)
            {
                if (catcherCollider.OverlapPoint(worldPoint))
                {
                    Debug.Log("Found exposed vertex in catcher area!");
                    return true; // TERTANGKAP! Ada vertex yang exposed dan di dalam catcher
                }
            }
        }
        
        return false; // Semua vertex yang di dalam catcher area sudah hidden
    }
    
    void CheckIfFullyHidden()
    {
        if (currentHidingSpots.Count == 0)
        {
            isHiding = false;
            return;
        }
        
        // Cek apakah SEMUA vertex player ada di dalam SALAH SATU hiding spot
        Vector2[] points = playerCollider.points;
        Vector2 offset = playerCollider.offset;
        
        foreach (Vector2 point in points)
        {
            Vector2 localPointWithOffset = point + offset;
            Vector2 worldPoint = transform.TransformPoint(localPointWithOffset);
            
            bool pointIsInAnySpot = false;
            
            // Cek apakah point ini ada di SALAH SATU hiding spot
            foreach (HidingSpot spot in currentHidingSpots)
            {
                if (spot.ContainsPoint(worldPoint))
                {
                    pointIsInAnySpot = true;
                    break;
                }
            }
            
            // Kalau ada 1 point yang tidak di dalam semua hiding spot, berarti tidak bersembunyi
            if (!pointIsInAnySpot)
            {
                isHiding = false;
                return;
            }
        }
        
        // Semua point ada di dalam (bisa di hiding spot berbeda)
        isHiding = true;
        Debug.Log("Player is FULLY HIDDEN across " + currentHidingSpots.Count + " hiding spots");
    }
}