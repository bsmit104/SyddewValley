using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemPlacement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private Tilemap groundTilemap; // Reference to your ground tilemap
    [SerializeField] private Camera mainCamera;
    
    [Header("Placement Settings")]
    [SerializeField] private GameObject placedItemPrefab; // Prefab for placed items
    [SerializeField] private LayerMask blockingLayers; // Layers that block placement
    [SerializeField] private float placementRange = 3f; // How far player can place items
    [SerializeField] private KeyCode placeKey = KeyCode.Q; // Key to place item
    
    [Header("Visual Feedback")]
    [SerializeField] private Color validPlacementColor = new Color(0, 1, 0, 0.5f);
    [SerializeField] private Color invalidPlacementColor = new Color(1, 0, 0, 0.5f);
    
    private GameObject placementPreview;
    private SpriteRenderer previewRenderer;
    private bool isPlacementMode = false;
    private Vector3Int currentGridPos;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        CreatePlacementPreview();
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
        previewRenderer.sortingOrder = 100; // Render on top
        placementPreview.SetActive(false);
    }

    private void HandlePlacementInput()
    {
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
        placementPreview.SetActive(true);
        previewRenderer.sprite = item.itemIcon;
    }

    private void ExitPlacementMode()
    {
        isPlacementMode = false;
        placementPreview.SetActive(false);
    }

    private void UpdatePlacementPreview()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        
        // Snap to grid if using tilemap
        if (groundTilemap != null)
        {
            currentGridPos = groundTilemap.WorldToCell(mouseWorldPos);
            Vector3 cellCenterWorld = groundTilemap.GetCellCenterWorld(currentGridPos);
            placementPreview.transform.position = cellCenterWorld;
        }
        else
        {
            // Snap to integer grid
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
}