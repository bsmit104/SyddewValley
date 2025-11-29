using UnityEngine;
using WorldTime;

public class Bed : MonoBehaviour
{
    [Header("Debug (optional)")]
    public bool showDebugLogs = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (showDebugLogs)
            Debug.Log($"[Bed] Trigger entered by: {other.name} | Tag: {other.tag} | Layer: {LayerMask.LayerToName(other.gameObject.layer)}");

        if (other.CompareTag("Player"))
        {
            var clock = FindFirstObjectByType<WorldClock>();
            if (clock != null)
            {
                clock.PlayerEnteredBed();
                if (showDebugLogs) Debug.Log("[Bed] Player entered bed → New Day screen triggered!");
            }
            else
            {
                Debug.LogError("[Bed] WorldClock not found in scene! Add the WorldClock script to a GameObject.");
            }
        }
    }
}



// using UnityEngine;
// using WorldTime;

// public class Bed : MonoBehaviour
// {
//     private void OnTriggerEnter2D(Collider2D other)
//     {
//         if (other.CompareTag("Player"))
//         {
//             // Fixed: Use FindFirstObjectByType instead of deprecated FindObjectOfType
//             FindFirstObjectByType<WorldClock>()?.PlayerEnteredBed();
//         }
//     }
// }