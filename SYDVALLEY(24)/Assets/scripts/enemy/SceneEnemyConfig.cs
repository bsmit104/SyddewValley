using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SceneEnemyConfig : MonoBehaviour
{
    [Header("Scene-Specific Enemies")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();
    
    [Header("Spawn Overrides")]
    public int targetEnemyCount = 15;
    public float playerSafeRadius = 10f;
    public float minDistanceBetweenEnemies = 5f;
    public float spawnStaggerDelay = 0.15f;
    
    [Header("Required")]
    [Tooltip("Assign your GROUND/WALKABLE tilemap for this scene")]
    public Tilemap groundTilemap;
    
    [Header("Obstacles")]
    public LayerMask obstacleLayers = ~0;  // Block everything by default
    
    [Header("Debug")]
    public bool showGizmos = true;
    
    // Optional: Hide this empty GameObject in Game view
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }
}