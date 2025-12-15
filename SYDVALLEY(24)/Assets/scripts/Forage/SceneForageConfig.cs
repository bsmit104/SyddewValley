// ===================================================================
// SceneForageConfig.cs - Per-scene configuration for forage items
// ===================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SceneForageConfig : MonoBehaviour
{
    [Header("Forage Items to Spawn")]
    public List<ForageSpawner.ForageItemData> forageItems = new List<ForageSpawner.ForageItemData>();
    
    [Header("Spawn Settings")]
    public int targetItemCount = 15;
    public float minDistanceBetweenItems = 3f;
    public float spawnStaggerDelay = 0.1f;
    
    [Header("Tilemap Reference")]
    public Tilemap groundTilemap;
    
    [Header("Obstacle Detection")]
    public LayerMask obstacleLayers = ~0;
    
    private void OnValidate()
    {
        // Auto-find tilemap if not assigned
        if (groundTilemap == null)
        {
            groundTilemap = FindObjectOfType<Tilemap>();
        }
    }
}
