// ForageSpawner.cs - Spawns collectible items on tilemap
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class ForageSpawner : MonoBehaviour
{
    [Header("Scene Control")]
    [Tooltip("Scenes where forage items should NOT spawn")]
    public List<string> excludedScenes = new List<string> { "MainMenu", "SaveSlotMenu" };

    [Header("GLOBAL FALLBACK (only if no SceneForageConfig in current scene)")]
    public List<ForageItemData> defaultForageItems = new List<ForageItemData>();
    public int defaultItemCount = 15;
    public float defaultMinDistanceBetweenItems = 3f;
    public LayerMask defaultObstacleLayers = ~0;

    [System.Serializable]
    public class ForageItemData
    {
        public Item item;                    // The item to give when collected
        public GameObject worldPrefab;       // Visual prefab in world (has ItemPickup component)
        [Range(0f, 1f)]
        public float spawnWeight = 1f;       // Probability weight for spawning this item
    }

    private static ForageSpawner _instance;
    private Transform forageItemsParent;
    private Coroutine currentSpawnRoutine;
    private string currentSpawnScene;

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
        Debug.Log($"[ForageSpawner] Scene loaded: {scene.name}");
        
        // Stop any ongoing spawn routine IMMEDIATELY
        if (currentSpawnRoutine != null)
        {
            StopCoroutine(currentSpawnRoutine);
            currentSpawnRoutine = null;
            currentSpawnScene = null;
        }
        
        // Clear any existing forage items
        ClearForageItems();
        
        // Check if this scene should have forage items
        if (excludedScenes.Contains(scene.name))
        {
            Debug.Log($"[ForageSpawner] Scene '{scene.name}' is excluded from forage spawning");
            return;
        }
        
        // Start new spawn routine for this scene
        currentSpawnScene = scene.name;
        currentSpawnRoutine = StartCoroutine(SpawnForCurrentScene());
    }

    private void Start()
    {
        // Check if current scene should have forage items
        string currentScene = SceneManager.GetActiveScene().name;
        
        if (excludedScenes.Contains(currentScene))
        {
            Debug.Log($"[ForageSpawner] Starting in excluded scene '{currentScene}', skipping spawn");
            return;
        }
        
        // First scene
        Debug.Log("[ForageSpawner] Starting initial spawn");
        currentSpawnScene = currentScene;
        currentSpawnRoutine = StartCoroutine(SpawnForCurrentScene());
    }

    private IEnumerator SpawnForCurrentScene()
    {
        // Store the scene we're spawning for
        string spawnSceneName = SceneManager.GetActiveScene().name;
        
        // Wait for scene to fully load
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // SAFETY CHECK: Verify we're still in the same scene
        if (SceneManager.GetActiveScene().name != spawnSceneName)
        {
            Debug.Log($"[ForageSpawner] Scene changed during initialization. Aborting spawn for '{spawnSceneName}'");
            yield break;
        }

        Debug.Log($"[ForageSpawner] Beginning spawn for scene: {spawnSceneName}");

        // Get config or use defaults
        SceneForageConfig config = FindObjectOfType<SceneForageConfig>();

        List<ForageItemData> items = config ? config.forageItems : defaultForageItems;
        int targetCount = config ? config.targetItemCount : defaultItemCount;
        float minDist = config ? config.minDistanceBetweenItems : defaultMinDistanceBetweenItems;
        float stagger = config ? config.spawnStaggerDelay : 0.1f;
        LayerMask obstacles = config ? config.obstacleLayers : defaultObstacleLayers;

        // Find Tilemap in current scene
        Tilemap tilemap = config ? config.groundTilemap : FindObjectOfType<Tilemap>();

        if (tilemap == null)
        {
            Debug.LogWarning($"[ForageSpawner] No Tilemap found in scene: {spawnSceneName} - skipping forage spawn");
            yield break;
        }

        if (items == null || items.Count == 0)
        {
            Debug.LogWarning($"[ForageSpawner] No forage items defined for scene: {spawnSceneName}");
            yield break;
        }

        Debug.Log($"[ForageSpawner] Config found: {(config != null ? "SceneForageConfig" : "Default")}. Target: {targetCount} items");

        // Fresh parent object for this scene only
        GameObject parentObj = new GameObject($"ForageItems_{spawnSceneName}");
        forageItemsParent = parentObj.transform;

        List<Vector3> usedPositions = new List<Vector3>();
        Bounds bounds = tilemap.localBounds;

        int spawned = 0;
        int maxAttempts = 3000;

        while (spawned < targetCount && maxAttempts-- > 0)
        {
            // CRITICAL SAFETY CHECK: Verify scene hasn't changed
            if (SceneManager.GetActiveScene().name != spawnSceneName || tilemap == null)
            {
                Debug.Log($"[ForageSpawner] Scene changed or tilemap destroyed during spawn. Aborting.");
                if (forageItemsParent != null)
                {
                    Destroy(forageItemsParent.gameObject);
                }
                yield break;
            }

            Vector3 randomWorld = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                0
            );

            Vector3Int cell = tilemap.WorldToCell(randomWorld);
            if (!tilemap.HasTile(cell)) continue;

            Vector3 spawnPos = tilemap.GetCellCenterWorld(cell);

            // Safety checks
            if (Physics2D.OverlapCircle(spawnPos, 0.4f, obstacles) != null) continue;
            if (usedPositions.Exists(p => Vector2.Distance(p, spawnPos) < minDist)) continue;

            // Select random item based on weights
            ForageItemData selectedItem = SelectWeightedRandomItem(items);
            if (selectedItem == null || selectedItem.worldPrefab == null) continue;

            // SPAWN
            GameObject forageObj = Instantiate(
                selectedItem.worldPrefab,
                spawnPos,
                Quaternion.identity,
                forageItemsParent
            );

            // Ensure ItemPickup component has correct item reference
            ItemPickup pickup = forageObj.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                pickup.item = selectedItem.item;
            }
            else
            {
                Debug.LogWarning($"[ForageSpawner] Prefab '{selectedItem.worldPrefab.name}' missing ItemPickup component!");
            }

            usedPositions.Add(spawnPos);
            spawned++;

            yield return new WaitForSeconds(stagger);
        }

        Debug.Log($"[ForageSpawner] Successfully spawned {spawned}/{targetCount} forage items in '{spawnSceneName}'");
        currentSpawnRoutine = null;
        currentSpawnScene = null;
    }

    private ForageItemData SelectWeightedRandomItem(List<ForageItemData> items)
    {
        float totalWeight = 0f;
        foreach (var item in items)
        {
            totalWeight += item.spawnWeight;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var item in items)
        {
            currentWeight += item.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return item;
            }
        }

        return items[items.Count - 1]; // Fallback
    }

    private void ClearForageItems()
    {
        if (forageItemsParent != null)
        {
            Debug.Log($"[ForageSpawner] Clearing {forageItemsParent.childCount} forage items");
            Destroy(forageItemsParent.gameObject);
            forageItemsParent = null;
        }
    }

    [ContextMenu("Respawn Forage Items in Current Scene")]
    public void ManualRespawn()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        
        if (excludedScenes.Contains(currentScene))
        {
            Debug.LogWarning($"[ForageSpawner] Cannot spawn in excluded scene '{currentScene}'");
            return;
        }
        
        if (currentSpawnRoutine != null)
        {
            StopCoroutine(currentSpawnRoutine);
            currentSpawnRoutine = null;
            currentSpawnScene = null;
        }
        
        ClearForageItems();
        currentSpawnScene = currentScene;
        currentSpawnRoutine = StartCoroutine(SpawnForCurrentScene());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}