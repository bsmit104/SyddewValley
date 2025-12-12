// EnemySpawner.cs – FIXED VERSION (Dec 2025)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class EnemySpawner : MonoBehaviour
{
    [Header("Scene Control")]
    [Tooltip("Scenes where enemies should NOT spawn (MainMenu, etc.)")]
    public List<string> excludedScenes = new List<string> { "MainMenu", "SaveSlotMenu" };

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
        
        // Check if this scene should have enemies
        if (excludedScenes.Contains(scene.name))
        {
            Debug.Log($"[EnemySpawner] Scene '{scene.name}' is excluded from enemy spawning");
            
            // Stop any ongoing spawn routine
            if (currentSpawnRoutine != null)
            {
                StopCoroutine(currentSpawnRoutine);
                currentSpawnRoutine = null;
            }
            
            // Clear any existing enemies
            ClearEnemies();
            return;
        }
        
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
        // Check if current scene should have enemies
        string currentScene = SceneManager.GetActiveScene().name;
        
        if (excludedScenes.Contains(currentScene))
        {
            Debug.Log($"[EnemySpawner] Starting in excluded scene '{currentScene}', skipping spawn");
            return;
        }
        
        // First scene
        Debug.Log("[EnemySpawner] Starting initial spawn");
        currentSpawnRoutine = StartCoroutine(SpawnForCurrentScene());
    }

    private IEnumerator SpawnForCurrentScene()
    {
        // Wait for scene to fully load
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"[EnemySpawner] Beginning spawn for scene: {sceneName}");

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
            Debug.LogWarning($"[EnemySpawner] No Tilemap found in scene: {sceneName} - skipping enemy spawn");
            yield break;
        }

        if (prefabs == null || prefabs.Count == 0)
        {
            Debug.LogWarning($"[EnemySpawner] No enemy prefabs defined for scene: {sceneName}");
            yield break;
        }

        Debug.Log($"[EnemySpawner] Config found: {(config != null ? "SceneEnemyConfig" : "Default")}. Target: {targetCount} enemies");

        // Fresh parent object for this scene only
        GameObject parentObj = new GameObject($"Enemies_{sceneName}");
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

        Debug.Log($"[EnemySpawner] Successfully spawned {spawned}/{targetCount} enemies in '{sceneName}'");
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
        string currentScene = SceneManager.GetActiveScene().name;
        
        if (excludedScenes.Contains(currentScene))
        {
            Debug.LogWarning($"[EnemySpawner] Cannot spawn in excluded scene '{currentScene}'");
            return;
        }
        
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

// // EnemySpawner.cs – FIXED VERSION (Dec 2025)
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Tilemaps;
// using UnityEngine.SceneManagement;

// public class EnemySpawner : MonoBehaviour
// {
//     [Header("GLOBAL FALLBACK (only if no SceneEnemyConfig in current scene)")]
//     public List<GameObject> defaultEnemyPrefabs = new List<GameObject>();
//     public int defaultEnemyCount = 12;
//     public float defaultPlayerSafeRadius = 10f;
//     public float defaultMinDistanceBetweenEnemies = 5f;
//     public LayerMask defaultObstacleLayers = ~0;

//     private static EnemySpawner _instance;
//     private Transform enemiesParent;
//     private Coroutine currentSpawnRoutine;

//     private void Awake()
//     {
//         // Singleton + survive scene changes
//         if (_instance != null && _instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }

//         _instance = this;
//         DontDestroyOnLoad(gameObject);

//         SceneManager.sceneLoaded += OnSceneLoaded;
//     }

//     private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//     {
//         Debug.Log($"[EnemySpawner] Scene loaded: {scene.name}");
        
//         // Stop any ongoing spawn routine
//         if (currentSpawnRoutine != null)
//         {
//             StopCoroutine(currentSpawnRoutine);
//             currentSpawnRoutine = null;
//         }
        
//         // Clear old enemies
//         ClearEnemies();
        
//         // Start new spawn routine
//         currentSpawnRoutine = StartCoroutine(SpawnForCurrentScene());
//     }

//     private void Start()
//     {
//         // First scene
//         Debug.Log("[EnemySpawner] Starting initial spawn");
//         currentSpawnRoutine = StartCoroutine(SpawnForCurrentScene());
//     }

//     private IEnumerator SpawnForCurrentScene()
//     {
//         // Wait for scene to fully load
//         yield return new WaitForEndOfFrame();
//         yield return new WaitForEndOfFrame();

//         Debug.Log($"[EnemySpawner] Beginning spawn for scene: {SceneManager.GetActiveScene().name}");

//         // === ALWAYS GET FRESH REFERENCES FROM THE CURRENT SCENE ===
//         SceneEnemyConfig config = FindObjectOfType<SceneEnemyConfig>();

//         List<GameObject> prefabs = config ? config.enemyPrefabs : defaultEnemyPrefabs;
//         int targetCount          = config ? config.targetEnemyCount : defaultEnemyCount;
//         float safeRadius         = config ? config.playerSafeRadius : defaultPlayerSafeRadius;
//         float minDist            = config ? config.minDistanceBetweenEnemies : defaultMinDistanceBetweenEnemies;
//         float stagger            = config ? config.spawnStaggerDelay : 0.15f;
//         LayerMask obstacles      = config ? config.obstacleLayers : defaultObstacleLayers;

//         // Find Tilemap in current scene (never cache!)
//         Tilemap tilemap = config ? config.groundTilemap : FindObjectOfType<Tilemap>();

//         if (tilemap == null)
//         {
//             Debug.LogError($"[EnemySpawner] No Tilemap found in scene: {SceneManager.GetActiveScene().name}");
//             yield break;
//         }

//         if (prefabs == null || prefabs.Count == 0)
//         {
//             Debug.LogWarning($"[EnemySpawner] No enemy prefabs defined for scene: {SceneManager.GetActiveScene().name}");
//             yield break;
//         }

//         Debug.Log($"[EnemySpawner] Config found: {(config != null ? "SceneEnemyConfig" : "Default")}. Target: {targetCount} enemies");

//         // Fresh parent object for this scene only
//         GameObject parentObj = new GameObject($"Enemies_{SceneManager.GetActiveScene().name}");
//         enemiesParent = parentObj.transform;

//         // Player position (fresh every time)
//         Vector3 playerPos = GetPlayerPosition();

//         List<Vector3> usedPositions = new List<Vector3>();
//         Bounds bounds = tilemap.localBounds;

//         int spawned = 0;
//         int maxAttempts = 3000;

//         while (spawned < targetCount && maxAttempts-- > 0)
//         {
//             Vector3 randomWorld = new Vector3(
//                 Random.Range(bounds.min.x, bounds.max.x),
//                 Random.Range(bounds.min.y, bounds.max.y),
//                 0
//             );

//             Vector3Int cell = tilemap.WorldToCell(randomWorld);
//             if (!tilemap.HasTile(cell)) continue;

//             Vector3 spawnPos = tilemap.GetCellCenterWorld(cell);

//             // Safety checks
//             if (Vector2.Distance(spawnPos, playerPos) < safeRadius) continue;
//             if (Physics2D.OverlapCircle(spawnPos, 0.4f, obstacles) != null) continue;
//             if (usedPositions.Exists(p => Vector2.Distance(p, spawnPos) < minDist)) continue;

//             // SPAWN
//             GameObject enemy = Instantiate(
//                 prefabs[Random.Range(0, prefabs.Count)],
//                 spawnPos,
//                 Quaternion.identity,
//                 enemiesParent
//             );

//             usedPositions.Add(spawnPos);
//             spawned++;

//             yield return new WaitForSeconds(stagger);
//         }

//         Debug.Log($"[EnemySpawner] Successfully spawned {spawned}/{targetCount} enemies in '{SceneManager.GetActiveScene().name}'");
//         currentSpawnRoutine = null;
//     }

//     private Vector3 GetPlayerPosition()
//     {
//         GameObject player = GameObject.FindGameObjectWithTag("Player");
//         if (player == null)
//         {
//             Debug.LogWarning("[EnemySpawner] Player not found! Using zero position.");
//             return Vector3.zero;
//         }
//         return player.transform.position;
//     }

//     private void ClearEnemies()
//     {
//         if (enemiesParent != null)
//         {
//             Debug.Log($"[EnemySpawner] Clearing {enemiesParent.childCount} enemies");
//             Destroy(enemiesParent.gameObject);
//             enemiesParent = null;
//         }
//     }

//     // Optional: manual respawn from inspector or button
//     [ContextMenu("Respawn Enemies in Current Scene")]
//     public void ManualRespawn()
//     {
//         if (currentSpawnRoutine != null)
//             StopCoroutine(currentSpawnRoutine);
        
//         ClearEnemies();
//         currentSpawnRoutine = StartCoroutine(SpawnForCurrentScene());
//     }

//     private void OnDestroy()
//     {
//         SceneManager.sceneLoaded -= OnSceneLoaded;
//     }
// }
