// EnemySpawner.cs – FIXED VERSION (Dec 2025)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class EnemySpawner : MonoBehaviour
{
    [Header("GLOBAL FALLBACK (only if no SceneEnemyConfig in current scene)")]
    public List<GameObject> defaultEnemyPrefabs = new List<GameObject>();
    public int defaultEnemyCount = 12;
    public float defaultPlayerSafeRadius = 10f;
    public float defaultMinDistanceBetweenEnemies = 5f;
    public LayerMask defaultObstacleLayers = ~0;

    private static EnemySpawner _instance;
    private Transform enemiesParent;
    private Coroutine currentSpawnRoutine;

    private void Awake()
    {
        // Singleton + survive scene changes
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[EnemySpawner] Scene loaded: {scene.name}");
        
        // Stop any ongoing spawn routine
        if (currentSpawnRoutine != null)
        {
            StopCoroutine(currentSpawnRoutine);
            currentSpawnRoutine = null;
        }
        
        // Clear old enemies
        ClearEnemies();
        
        // Start new spawn routine
        currentSpawnRoutine = StartCoroutine(SpawnForCurrentScene());
    }

    private void Start()
    {
        // First scene
        Debug.Log("[EnemySpawner] Starting initial spawn");
        currentSpawnRoutine = StartCoroutine(SpawnForCurrentScene());
    }

    private IEnumerator SpawnForCurrentScene()
    {
        // Wait for scene to fully load
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Debug.Log($"[EnemySpawner] Beginning spawn for scene: {SceneManager.GetActiveScene().name}");

        // === ALWAYS GET FRESH REFERENCES FROM THE CURRENT SCENE ===
        SceneEnemyConfig config = FindObjectOfType<SceneEnemyConfig>();

        List<GameObject> prefabs = config ? config.enemyPrefabs : defaultEnemyPrefabs;
        int targetCount          = config ? config.targetEnemyCount : defaultEnemyCount;
        float safeRadius         = config ? config.playerSafeRadius : defaultPlayerSafeRadius;
        float minDist            = config ? config.minDistanceBetweenEnemies : defaultMinDistanceBetweenEnemies;
        float stagger            = config ? config.spawnStaggerDelay : 0.15f;
        LayerMask obstacles      = config ? config.obstacleLayers : defaultObstacleLayers;

        // Find Tilemap in current scene (never cache!)
        Tilemap tilemap = config ? config.groundTilemap : FindObjectOfType<Tilemap>();

        if (tilemap == null)
        {
            Debug.LogError($"[EnemySpawner] No Tilemap found in scene: {SceneManager.GetActiveScene().name}");
            yield break;
        }

        if (prefabs == null || prefabs.Count == 0)
        {
            Debug.LogWarning($"[EnemySpawner] No enemy prefabs defined for scene: {SceneManager.GetActiveScene().name}");
            yield break;
        }

        Debug.Log($"[EnemySpawner] Config found: {(config != null ? "SceneEnemyConfig" : "Default")}. Target: {targetCount} enemies");

        // Fresh parent object for this scene only
        GameObject parentObj = new GameObject($"Enemies_{SceneManager.GetActiveScene().name}");
        enemiesParent = parentObj.transform;

        // Player position (fresh every time)
        Vector3 playerPos = GetPlayerPosition();

        List<Vector3> usedPositions = new List<Vector3>();
        Bounds bounds = tilemap.localBounds;

        int spawned = 0;
        int maxAttempts = 3000;

        while (spawned < targetCount && maxAttempts-- > 0)
        {
            Vector3 randomWorld = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                0
            );

            Vector3Int cell = tilemap.WorldToCell(randomWorld);
            if (!tilemap.HasTile(cell)) continue;

            Vector3 spawnPos = tilemap.GetCellCenterWorld(cell);

            // Safety checks
            if (Vector2.Distance(spawnPos, playerPos) < safeRadius) continue;
            if (Physics2D.OverlapCircle(spawnPos, 0.4f, obstacles) != null) continue;
            if (usedPositions.Exists(p => Vector2.Distance(p, spawnPos) < minDist)) continue;

            // SPAWN
            GameObject enemy = Instantiate(
                prefabs[Random.Range(0, prefabs.Count)],
                spawnPos,
                Quaternion.identity,
                enemiesParent
            );

            usedPositions.Add(spawnPos);
            spawned++;

            yield return new WaitForSeconds(stagger);
        }

        Debug.Log($"[EnemySpawner] Successfully spawned {spawned}/{targetCount} enemies in '{SceneManager.GetActiveScene().name}'");
        currentSpawnRoutine = null;
    }

    private Vector3 GetPlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[EnemySpawner] Player not found! Using zero position.");
            return Vector3.zero;
        }
        return player.transform.position;
    }

    private void ClearEnemies()
    {
        if (enemiesParent != null)
        {
            Debug.Log($"[EnemySpawner] Clearing {enemiesParent.childCount} enemies");
            Destroy(enemiesParent.gameObject);
            enemiesParent = null;
        }
    }

    // Optional: manual respawn from inspector or button
    [ContextMenu("Respawn Enemies in Current Scene")]
    public void ManualRespawn()
    {
        if (currentSpawnRoutine != null)
            StopCoroutine(currentSpawnRoutine);
        
        ClearEnemies();
        currentSpawnRoutine = StartCoroutine(SpawnForCurrentScene());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Tilemaps;

// public class EnemySpawner : MonoBehaviour
// {
//     [Header("Enemy Prefabs")]
//     public List<GameObject> enemyPrefabs = new List<GameObject>();

//     [Header("Spawn Amount")]
//     [Range(5, 30)] public int targetEnemyCount = 15;

//     [Header("Spawn Area")]
//     public Tilemap groundTilemap;
    
//     [Header("Player Safety Zone")]
//     [Range(3f, 15f)]
//     public float playerSafeRadius = 8f;   

//     [Header("Spread Control")]
//     [Range(0f, 20f)]
//     public float minDistanceBetweenEnemies = 4f;  

//     [Header("Obstacle Detection")]
//     [Tooltip("Layers to avoid spawning on (walls, obstacles, etc.)")]
//     public LayerMask obstacleLayers = -1;  // Everything by default

//     [Header("Performance & Visuals")]
//     public float spawnStaggerDelay = 0.15f;
//     public bool showGizmos = true;

//     private List<Vector3> usedPositions = new List<Vector3>();
//     private Transform enemiesParent;

//     void Start()
//     {
//         if (enemyPrefabs.Count == 0 || groundTilemap == null)
//         {
//             Debug.LogError("EnemySpawner: Missing prefabs or tilemap!");
//             return;
//         }

//         // Create parent folder under this spawner
//         GameObject parentObj = new GameObject("Spawned Enemies");
//         enemiesParent = parentObj.transform;
//         enemiesParent.SetParent(transform);

//         StartCoroutine(SpawnEnemiesEvenly());
//     }

//     IEnumerator SpawnEnemiesEvenly()
//     {
//         usedPositions.Clear();

//         // Cache player position once (fast!)
//         Vector3 playerPos = GetPlayerPosition();

//         Bounds tilemapBounds = groundTilemap.localBounds;
//         Vector3 min = tilemapBounds.min;
//         Vector3 max = tilemapBounds.max;

//         int attempts = 0;
//         int spawned = 0;

//         while (spawned < targetEnemyCount && attempts < 2000)  // Increased limit for safety
//         {
//             // Random position inside tilemap
//             float x = Random.Range(min.x, max.x);
//             float y = Random.Range(min.y, max.y);
//             Vector3 candidatePos = new Vector3(x, y, 0);

//             // Snap to nearest tile center
//             Vector3Int cell = groundTilemap.WorldToCell(candidatePos);
//             if (!groundTilemap.HasTile(cell))
//             {
//                 attempts++;
//                 continue;
//             }

//             Vector3 worldPos = groundTilemap.GetCellCenterWorld(cell);

//             // 1. Outside player safe zone?
//             if (Vector2.Distance(worldPos, playerPos) < playerSafeRadius)
//             {
//                 attempts++;
//                 continue;
//             }

//             // 2. No obstacles/colliders?
//             if (Physics2D.OverlapCircle(worldPos, 0.4f, obstacleLayers) != null)
//             {
//                 attempts++;
//                 continue;
//             }

//             // 3. Far enough from other enemies?
//             bool tooClose = false;
//             foreach (Vector3 used in usedPositions)
//             {
//                 if (Vector2.Distance(worldPos, used) < minDistanceBetweenEnemies)
//                 {
//                     tooClose = true;
//                     break;
//                 }
//             }
//             if (tooClose)
//             {
//                 attempts++;
//                 continue;
//             }

//             // PERFECT SPOT! Spawn here
//             GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
//             GameObject enemy = Instantiate(prefab, worldPos, Quaternion.identity, enemiesParent);
//             enemy.name = prefab.name + " #" + (spawned + 1);

//             usedPositions.Add(worldPos);
//             spawned++;

//             yield return new WaitForSeconds(spawnStaggerDelay);
//             attempts = 0;  // Reset on success
//         }

//         Debug.Log($"EnemySpawner: Spawned {spawned}/{targetEnemyCount} enemies perfectly spread across valid tiles!");
//     }

//     Vector3 GetPlayerPosition()
//     {
//         GameObject player = GameObject.FindGameObjectWithTag("Player");
//         return player != null ? player.transform.position : Vector3.zero;
//     }

//     public void ClearAndRespawn()
//     {
//         if (enemiesParent != null)
//         {
//             foreach (Transform child in enemiesParent)
//                 if (child != null) Destroy(child.gameObject);
//             Destroy(enemiesParent.gameObject);
//         }
//         Start();
//     }

//     void OnDrawGizmosSelected()
//     {
//         if (!showGizmos || groundTilemap == null) return;

//         // Tilemap spawn area
//         Gizmos.color = new Color(0, 1, 1, 0.2f);  // Cyan
//         Gizmos.DrawCube(groundTilemap.localBounds.center, groundTilemap.localBounds.size);

//         // Player safe zone (red)
//         Vector3 playerPos = GetPlayerPosition();
//         Gizmos.color = new Color(1, 0, 0, 0.3f);
//         Gizmos.DrawWireSphere(playerPos, playerSafeRadius);

//         // Used spawn positions (during runtime)
//         Gizmos.color = Color.magenta;
//         foreach (Vector3 pos in usedPositions)
//             Gizmos.DrawSphere(pos, 0.4f);
//     }
// }


// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Tilemaps;

// public class EnemySpawner : MonoBehaviour
// {
//     [Header("Enemy Prefabs")]
//     [Tooltip("Assign your 4 enemy prefabs here")]
//     public List<GameObject> enemyPrefabs = new List<GameObject>();

//     [Header("Spawn Settings")]
//     [Range(10, 20)]
//     public int minEnemies = 10;
    
//     [Range(10, 20)]
//     public int maxEnemies = 15;
    
//     public Tilemap spawnTilemap;  // Assign the GROUND/WALKABLE tilemap layer
    
//     [Tooltip("Inclusive bounds for spawn area (tile coordinates)")]
//     public Vector2Int spawnBoundsMin = new Vector2Int(-20, -10);
    
//     [Tooltip("Inclusive bounds for spawn area (tile coordinates)")]
//     public Vector2Int spawnBoundsMax = new Vector2Int(20, 10);
    
//     [Tooltip("Minimum distance from player spawn")]
//     public float minDistanceFromPlayer = 5f;
    
//     [Header("Obstacle Detection")]
//     [Tooltip("Layers to treat as obstacles (walls, etc.)")]
//     public LayerMask obstacleLayers = -1;
    
//     [Header("Spawn Behavior")]
//     public float spawnDelay = 0.5f;  // Stagger spawns slightly for effect
    
//     [Header("References")]
//     public Transform playerTransform;
    
//     [Header("Debug")]
//     public bool showValidSpawnsInGizmos = true;
//     public bool logSpawnInfo = true;

//     private List<Vector3> validSpawnPositions;
//     private Transform enemyFolder;

//     void Start()
//     {
//         if (enemyPrefabs.Count == 0)
//         {
//             Debug.LogWarning($"{name}: No enemy prefabs assigned! Add some to the list.");
//             return;
//         }
        
//         if (spawnTilemap == null)
//         {
//             Debug.LogError($"{name}: Ground tilemap not assigned!");
//             return;
//         }
        
//         // Auto-find player if not assigned
//         if (playerTransform == null)
//         {
//             GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
//             if (playerObj != null)
//                 playerTransform = playerObj.transform;
//             else
//                 Debug.LogWarning($"{name}: Player not found! Spawns may overlap player.");
//         }
        
//         // Create enemies folder
//         enemyFolder = new GameObject("Enemies").transform;
//         enemyFolder.SetParent(transform);
        
//         // Find valid positions and spawn
//         StartCoroutine(SpawnEnemiesCoroutine());
//     }

//     IEnumerator SpawnEnemiesCoroutine()
//     {
//         // Precompute valid positions
//         validSpawnPositions = FindValidSpawnPositions();
        
//         if (validSpawnPositions.Count == 0)
//         {
//             Debug.LogError($"{name}: No valid spawn positions found! Check bounds, tilemap, and obstacles.");
//             yield break;
//         }
        
//         if (logSpawnInfo)
//             Debug.Log($"{name}: Found {validSpawnPositions.Count} valid positions. Spawning {minEnemies}-{maxEnemies} enemies.");
        
//         // Shuffle positions to randomize
//         ShuffleList(validSpawnPositions);
        
//         // Determine spawn count
//         int spawnCount = Random.Range(minEnemies, maxEnemies + 1);
//         spawnCount = Mathf.Min(spawnCount, validSpawnPositions.Count);
        
//         // Spawn with stagger
//         for (int i = 0; i < spawnCount; i++)
//         {
//             Vector3 spawnPos = validSpawnPositions[i];
//             GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            
//             GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity, enemyFolder);
//             enemy.name = prefab.name + " #" + (i + 1);
            
//             if (logSpawnInfo)
//                 Debug.Log($"{name}: Spawned {enemy.name} at {spawnPos}");
            
//             yield return new WaitForSeconds(spawnDelay);
//         }
        
//         if (logSpawnInfo)
//             Debug.Log($"{name}: Wave complete! {spawnCount} enemies spawned.");
//     }

//     List<Vector3> FindValidSpawnPositions()
//     {
//         List<Vector3> positions = new List<Vector3>();
//         Vector3 playerPos = playerTransform != null ? playerTransform.position : Vector3.zero;
        
//         for (int x = spawnBoundsMin.x; x <= spawnBoundsMax.x; x++)
//         {
//             for (int y = spawnBoundsMin.y; y <= spawnBoundsMax.y; y++)
//             {
//                 Vector3 pos = new Vector3(x + 0.5f, y + 0.5f, 0f);  // Tile center
                
//                 // Check 1: Has ground tile?
//                 Vector3Int cellPos = spawnTilemap.WorldToCell(pos);
//                 if (!spawnTilemap.HasTile(cellPos))
//                     continue;
                
//                 // Check 2: Far enough from player?
//                 if (Vector3.Distance(pos, playerPos) < minDistanceFromPlayer)
//                     continue;
                
//                 // Check 3: No obstacles/colliders?
//                 if (Physics2D.OverlapCircle(pos, 0.3f, obstacleLayers) != null)
//                     continue;
                
//                 positions.Add(pos);
//             }
//         }
        
//         return positions;
//     }

//     void ShuffleList<T>(List<T> list)
//     {
//         for (int i = list.Count - 1; i > 0; i--)
//         {
//             int j = Random.Range(0, i + 1);
//             T temp = list[i];
//             list[i] = list[j];
//             list[j] = temp;
//         }
//     }

//     // Public method to respawn (call from UI, etc.)
//     public void RespawnEnemies()
//     {
//         // Destroy old enemies
//         if (enemyFolder != null)
//         {
//             foreach (Transform child in enemyFolder)
//                 Destroy(child.gameObject);
//         }
        
//         StartCoroutine(SpawnEnemiesCoroutine());
//     }

//     void OnDrawGizmosSelected()
//     {
//         if (!showValidSpawnsInGizmos || spawnTilemap == null) return;
        
//         // Draw spawn bounds
//         Gizmos.color = Color.yellow;
//         Vector3 min = new Vector3(spawnBoundsMin.x, spawnBoundsMin.y, 0);
//         Vector3 max = new Vector3(spawnBoundsMax.x + 1, spawnBoundsMax.y + 1, 0);
//         Gizmos.DrawWireCube((min + max) * 0.5f, max - min);
        
//         // Draw valid positions (green) and invalid (red) - but compute on fly for gizmos
//         Vector3 playerPos = playerTransform != null ? playerTransform.position : transform.position;
//         Gizmos.color = Color.green;
        
//         for (int x = spawnBoundsMin.x; x <= spawnBoundsMax.x; x++)
//         {
//             for (int y = spawnBoundsMin.y; y <= spawnBoundsMax.y; y++)
//             {
//                 Vector3 pos = new Vector3(x + 0.5f, y + 0.5f, 0f);
//                 Vector3Int cellPos = spawnTilemap.WorldToCell(pos);
                
//                 if (spawnTilemap.HasTile(cellPos) &&
//                     Vector3.Distance(pos, playerPos) >= minDistanceFromPlayer &&
//                     Physics2D.OverlapCircle(pos, 0.3f, obstacleLayers) == null)
//                 {
//                     Gizmos.DrawWireSphere(pos, 0.2f);
//                 }
//             }
//         }
//     }
// }