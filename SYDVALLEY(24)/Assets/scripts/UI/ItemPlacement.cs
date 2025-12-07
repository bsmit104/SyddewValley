using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class ItemPlacement : MonoBehaviour
{
    [Header("Placement Settings")]
    [SerializeField] private GameObject placedItemPrefab;
    [SerializeField] private LayerMask blockingLayers;
    [SerializeField] private float placementRange = 3f;
    [SerializeField] private KeyCode placeKey = KeyCode.Q;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color validPlacementColor = new Color(0, 1, 0, 0.5f);
    [SerializeField] private Color invalidPlacementColor = new Color(1, 0, 0, 0.5f);
    
    // References that auto-reconnect
    private Inventory inventory;
    private Tilemap groundTilemap;
    private Camera mainCamera;
    
    private GameObject placementPreview;
    private SpriteRenderer previewRenderer;
    private bool isPlacementMode = false;
    private Vector3Int currentGridPos;

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Find or use existing preview immediately
        FindOrCreatePreview();
    }

    void Start()
    {
        FindSceneReferences();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindSceneReferences();
        FindOrCreatePreview();
    }
    
    private void FindOrCreatePreview()
    {
        // Look for existing preview in DontDestroyOnLoad
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
        GameObject foundPreview = null;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "PlacementPreview")
            {
                if (foundPreview == null)
                {
                    // This is the first one we found, keep it
                    foundPreview = obj;
                }
                else
                {
                    // This is a duplicate, destroy it
                    Debug.Log($"Destroying duplicate PlacementPreview: {obj.GetInstanceID()}");
                    Destroy(obj);
                }
            }
        }
        
        if (foundPreview != null)
        {
            // Use the existing preview
            placementPreview = foundPreview;
            previewRenderer = foundPreview.GetComponent<SpriteRenderer>();
            Debug.Log($"Using existing PlacementPreview: {foundPreview.GetInstanceID()}");
        }
        else
        {
            // No preview exists, create one
            CreatePlacementPreview();
        }
    }

    private void FindSceneReferences()
    {
        // Find Inventory (persistent singleton)
        if (inventory == null)
        {
            inventory = Inventory.Instance;
            if (inventory == null)
            {
                Debug.LogWarning("ItemPlacement: Inventory instance not found!");
            }
        }
        
        // Find Main Camera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("ItemPlacement: Main Camera not found!");
            }
        }
        
        // Find Ground Tilemap by tag
        if (groundTilemap == null)
        {
            GameObject groundObject = GameObject.FindGameObjectWithTag("Ground");
            if (groundObject != null)
            {
                groundTilemap = groundObject.GetComponent<Tilemap>();
                if (groundTilemap != null)
                {
                    Debug.Log("ItemPlacement: Found Ground tilemap");
                }
            }
            else
            {
                // No ground tilemap found - will use integer grid snapping instead
                groundTilemap = null;
            }
        }
        
        // Check if preview exists and is valid
        if (placementPreview == null)
        {
            // Look for existing preview first (in case of duplicates)
            GameObject existingPreview = GameObject.Find("PlacementPreview");
            if (existingPreview != null)
            {
                placementPreview = existingPreview;
                previewRenderer = placementPreview.GetComponent<SpriteRenderer>();
            }
            else
            {
                CreatePlacementPreview();
            }
        }
    }

    void Update()
    {
        HandlePlacementInput();
        
        if (isPlacementMode)
        {
            UpdatePlacementPreview();
        }
    }

    private void CreatePlacementPreview()
    {
        placementPreview = new GameObject("PlacementPreview");
        previewRenderer = placementPreview.AddComponent<SpriteRenderer>();
        previewRenderer.sortingOrder = 100;
        placementPreview.SetActive(false);
        
        // Make the preview persist across scenes
        DontDestroyOnLoad(placementPreview);
        
        Debug.Log("ItemPlacement: Created placement preview");
    }

    private void HandlePlacementInput()
    {
        if (inventory == null) return;
        
        Item selectedItem = inventory.GetSelectedItem();
        
        // Check if selected item is placeable
        if (selectedItem != null && selectedItem.isPlaceable)
        {
            if (!isPlacementMode)
            {
                EnterPlacementMode(selectedItem);
            }
            
            // Place item on key press
            if (Input.GetKeyDown(placeKey) || Input.GetMouseButtonDown(0))
            {
                TryPlaceItem();
            }
        }
        else if (isPlacementMode)
        {
            ExitPlacementMode();
        }
        
        // Cancel placement with right click or ESC
        if (isPlacementMode && (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)))
        {
            ExitPlacementMode();
        }
    }

    private void EnterPlacementMode(Item item)
    {
        isPlacementMode = true;
        if (placementPreview != null)
        {
            placementPreview.SetActive(true);
            previewRenderer.sprite = item.itemIcon;
        }
    }

    private void ExitPlacementMode()
    {
        isPlacementMode = false;
        if (placementPreview != null)
        {
            placementPreview.SetActive(false);
        }
    }

    private void UpdatePlacementPreview()
    {
        if (mainCamera == null || placementPreview == null) return;
        
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        
        // Snap to grid
        if (groundTilemap != null)
        {
            // Use tilemap grid if available
            currentGridPos = groundTilemap.WorldToCell(mouseWorldPos);
            Vector3 cellCenterWorld = groundTilemap.GetCellCenterWorld(currentGridPos);
            placementPreview.transform.position = cellCenterWorld;
        }
        else
        {
            // Snap to integer grid if no tilemap
            currentGridPos = new Vector3Int(
                Mathf.RoundToInt(mouseWorldPos.x),
                Mathf.RoundToInt(mouseWorldPos.y),
                0
            );
            placementPreview.transform.position = new Vector3(currentGridPos.x, currentGridPos.y, 0);
        }
        
        // Update preview color based on validity
        bool canPlace = CanPlaceAtPosition(placementPreview.transform.position);
        previewRenderer.color = canPlace ? validPlacementColor : invalidPlacementColor;
    }

    private bool CanPlaceAtPosition(Vector3 position)
    {
        // Check if within range of player
        float distanceToPlayer = Vector3.Distance(transform.position, position);
        if (distanceToPlayer > placementRange)
            return false;
        
        // Check if position is blocked by colliders
        Collider2D hit = Physics2D.OverlapCircle(position, 0.1f, blockingLayers);
        if (hit != null)
            return false;
        
        // Check if there's already a placed item here
        PlacedItem existingItem = FindPlacedItemAtPosition(position);
        if (existingItem != null)
            return false;
        
        return true;
    }

    private void TryPlaceItem()
    {
        if (inventory == null || placementPreview == null)
        {
            Debug.LogWarning("Cannot place item - missing references!");
            return;
        }
        
        Vector3 placePosition = placementPreview.transform.position;
        
        if (!CanPlaceAtPosition(placePosition))
        {
            Debug.Log("Cannot place item here!");
            return;
        }
        
        Item selectedItem = inventory.GetSelectedItem();
        if (selectedItem == null)
        {
            Debug.LogError("No selected item to place!");
            return;
        }
        
        if (placedItemPrefab == null)
        {
            Debug.LogError("Placed Item Prefab is not assigned in ItemPlacement!");
            return;
        }
        
        // Create the placed item
        GameObject placedObject = Instantiate(placedItemPrefab, placePosition, Quaternion.identity);
        PlacedItem placedItemComponent = placedObject.GetComponent<PlacedItem>();
        
        if (placedItemComponent == null)
        {
            Debug.LogError("PlacedItem component not found on prefab! Adding it now...");
            placedItemComponent = placedObject.AddComponent<PlacedItem>();
        }
        
        // Initialize the placed item with the item data
        placedItemComponent.Initialize(selectedItem, currentGridPos);
        
        // Make sure the placed item has proper rendering
        SpriteRenderer itemRenderer = placedObject.GetComponent<SpriteRenderer>();
        if (itemRenderer != null)
        {
            itemRenderer.sprite = selectedItem.itemIcon;
            itemRenderer.sortingOrder = 5; // Make sure it's visible
            itemRenderer.color = Color.white; // Full opacity
        }
        else
        {
            Debug.LogWarning("Placed item prefab doesn't have a SpriteRenderer!");
        }
        
        // Remove item from inventory
        inventory.RemoveItem(selectedItem, 1);
        
        Debug.Log($"Placed {selectedItem.itemName} at {placePosition}");
    }

    private PlacedItem FindPlacedItemAtPosition(Vector3 position)
    {
        PlacedItem[] allPlacedItems = FindObjectsOfType<PlacedItem>();
        foreach (PlacedItem item in allPlacedItems)
        {
            if (Vector3.Distance(item.transform.position, position) < 0.1f)
                return item;
        }
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw placement range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, placementRange);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}



// using UnityEngine;
// using UnityEngine.Tilemaps;

// public class ItemPlacement : MonoBehaviour
// {
//     [Header("References")]
//     [SerializeField] private Inventory inventory;
//     [SerializeField] private Tilemap groundTilemap; // Reference to your ground tilemap
//     [SerializeField] private Camera mainCamera;
    
//     [Header("Placement Settings")]
//     [SerializeField] private GameObject placedItemPrefab; // Prefab for placed items
//     [SerializeField] private LayerMask blockingLayers; // Layers that block placement
//     [SerializeField] private float placementRange = 3f; // How far player can place items
//     [SerializeField] private KeyCode placeKey = KeyCode.Q; // Key to place item
    
//     [Header("Visual Feedback")]
//     [SerializeField] private Color validPlacementColor = new Color(0, 1, 0, 0.5f);
//     [SerializeField] private Color invalidPlacementColor = new Color(1, 0, 0, 0.5f);
    
//     private GameObject placementPreview;
//     private SpriteRenderer previewRenderer;
//     private bool isPlacementMode = false;
//     private Vector3Int currentGridPos;

//     void Start()
//     {
//         if (mainCamera == null)
//             mainCamera = Camera.main;
        
//         CreatePlacementPreview();
//     }

//     void Update()
//     {
//         HandlePlacementInput();
        
//         if (isPlacementMode)
//         {
//             UpdatePlacementPreview();
//         }
//     }

//     private void CreatePlacementPreview()
//     {
//         placementPreview = new GameObject("PlacementPreview");
//         previewRenderer = placementPreview.AddComponent<SpriteRenderer>();
//         previewRenderer.sortingOrder = 100; // Render on top
//         placementPreview.SetActive(false);
//     }

//     private void HandlePlacementInput()
//     {
//         Item selectedItem = inventory.GetSelectedItem();
        
//         // Check if selected item is placeable
//         if (selectedItem != null && selectedItem.isPlaceable)
//         {
//             if (!isPlacementMode)
//             {
//                 EnterPlacementMode(selectedItem);
//             }
            
//             // Place item on key press
//             if (Input.GetKeyDown(placeKey) || Input.GetMouseButtonDown(0))
//             {
//                 TryPlaceItem();
//             }
//         }
//         else if (isPlacementMode)
//         {
//             ExitPlacementMode();
//         }
        
//         // Cancel placement with right click or ESC
//         if (isPlacementMode && (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)))
//         {
//             ExitPlacementMode();
//         }
//     }

//     private void EnterPlacementMode(Item item)
//     {
//         isPlacementMode = true;
//         placementPreview.SetActive(true);
//         previewRenderer.sprite = item.itemIcon;
//     }

//     private void ExitPlacementMode()
//     {
//         isPlacementMode = false;
//         placementPreview.SetActive(false);
//     }

//     private void UpdatePlacementPreview()
//     {
//         Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
//         mouseWorldPos.z = 0;
        
//         // Snap to grid if using tilemap
//         if (groundTilemap != null)
//         {
//             currentGridPos = groundTilemap.WorldToCell(mouseWorldPos);
//             Vector3 cellCenterWorld = groundTilemap.GetCellCenterWorld(currentGridPos);
//             placementPreview.transform.position = cellCenterWorld;
//         }
//         else
//         {
//             // Snap to integer grid
//             currentGridPos = new Vector3Int(
//                 Mathf.RoundToInt(mouseWorldPos.x),
//                 Mathf.RoundToInt(mouseWorldPos.y),
//                 0
//             );
//             placementPreview.transform.position = new Vector3(currentGridPos.x, currentGridPos.y, 0);
//         }
        
//         // Update preview color based on validity
//         bool canPlace = CanPlaceAtPosition(placementPreview.transform.position);
//         previewRenderer.color = canPlace ? validPlacementColor : invalidPlacementColor;
//     }

//     private bool CanPlaceAtPosition(Vector3 position)
//     {
//         // Check if within range of player
//         float distanceToPlayer = Vector3.Distance(transform.position, position);
//         if (distanceToPlayer > placementRange)
//             return false;
        
//         // Check if position is blocked by colliders
//         Collider2D hit = Physics2D.OverlapCircle(position, 0.1f, blockingLayers);
//         if (hit != null)
//             return false;
        
//         // Check if there's already a placed item here
//         PlacedItem existingItem = FindPlacedItemAtPosition(position);
//         if (existingItem != null)
//             return false;
        
//         return true;
//     }

//     private void TryPlaceItem()
//     {
//         Vector3 placePosition = placementPreview.transform.position;
        
//         if (!CanPlaceAtPosition(placePosition))
//         {
//             Debug.Log("Cannot place item here!");
//             return;
//         }
        
//         Item selectedItem = inventory.GetSelectedItem();
//         if (selectedItem == null)
//         {
//             Debug.LogError("No selected item to place!");
//             return;
//         }
        
//         if (placedItemPrefab == null)
//         {
//             Debug.LogError("Placed Item Prefab is not assigned in ItemPlacement!");
//             return;
//         }
        
//         // Create the placed item
//         GameObject placedObject = Instantiate(placedItemPrefab, placePosition, Quaternion.identity);
//         PlacedItem placedItemComponent = placedObject.GetComponent<PlacedItem>();
        
//         if (placedItemComponent == null)
//         {
//             Debug.LogError("PlacedItem component not found on prefab! Adding it now...");
//             placedItemComponent = placedObject.AddComponent<PlacedItem>();
//         }
        
//         // Initialize the placed item with the item data
//         placedItemComponent.Initialize(selectedItem, currentGridPos);
        
//         // Remove item from inventory
//         inventory.RemoveItem(selectedItem, 1);
        
//         Debug.Log($"Placed {selectedItem.itemName} at {placePosition}");
//     }

//     private PlacedItem FindPlacedItemAtPosition(Vector3 position)
//     {
//         PlacedItem[] allPlacedItems = FindObjectsOfType<PlacedItem>();
//         foreach (PlacedItem item in allPlacedItems)
//         {
//             if (Vector3.Distance(item.transform.position, position) < 0.1f)
//                 return item;
//         }
//         return null;
//     }

//     private void OnDrawGizmosSelected()
//     {
//         // Draw placement range
//         Gizmos.color = Color.yellow;
//         Gizmos.DrawWireSphere(transform.position, placementRange);
//     }
// }