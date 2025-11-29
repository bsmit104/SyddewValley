using UnityEngine;

public class PlacedItem : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private Item itemDataInspector; // For manually placed items in scene
    
    public Item itemData { get; private set; }
    public Vector3Int gridPosition { get; private set; }
    
    [Header("Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool canPickup = true;
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private KeyCode pickupKey = KeyCode.E;
    
    private GameObject player;
    private Inventory playerInventory;
    private bool playerInRange = false;

    public void Initialize(Item item, Vector3Int gridPos)
    {
        if (item == null)
        {
            Debug.LogError("PlacedItem.Initialize: item is null!");
            return;
        }
        
        itemData = item;
        gridPosition = gridPos;
        
        SetupVisuals();
        
        Debug.Log($"PlacedItem initialized with: {item.itemName}");
    }

    void Awake()
    {
        // If item was set in inspector (manually placed in scene), use that
        if (itemDataInspector != null && itemData == null)
        {
            itemData = itemDataInspector;
            SetupVisuals();
            Debug.Log($"PlacedItem loaded from inspector: {itemData.itemName}");
        }
    }

    private void SetupVisuals()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Add sprite renderer if it doesn't exist
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        if (spriteRenderer != null && itemData != null && itemData.itemIcon != null)
        {
            spriteRenderer.sprite = itemData.itemIcon;
        }
        
        // Set up the game object name
        if (itemData != null)
        {
            gameObject.name = $"Placed_{itemData.itemName}";
        }
        
        // Ensure there's a collider for interaction
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
        }
    }

    void Start()
    {
        // Find player
        player = GameObject.FindGameObjectWithTag("Player");
        
        // Find inventory (it's on a separate GameObject with "Inventory" tag)
        GameObject inventoryObject = GameObject.FindGameObjectWithTag("Inventory");
        if (inventoryObject != null)
        {
            playerInventory = inventoryObject.GetComponent<Inventory>();
        }
        else
        {
            Debug.LogWarning("Inventory GameObject not found! Make sure there's a GameObject tagged 'Inventory'");
        }
    }

    void Update()
    {
        if (!canPickup) return;
        
        CheckPlayerProximity();
        
        if (playerInRange && Input.GetKeyDown(pickupKey))
        {
            PickupItem();
        }
    }

    private void CheckPlayerProximity()
    {
        if (player == null) return;
        
        float distance = Vector3.Distance(transform.position, player.transform.position);
        playerInRange = distance <= pickupRange;
    }

    private void PickupItem()
    {
        if (playerInventory == null)
        {
            Debug.LogError("Player inventory not found!");
            return;
        }
        
        if (itemData == null)
        {
            Debug.LogError("PlacedItem has no itemData! Was Initialize() called?");
            return;
        }
        
        // Try to add item back to inventory
        bool added = playerInventory.AddItem(itemData, 1);
        
        if (added)
        {
            Debug.Log($"Picked up {itemData.itemName}");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Inventory is full!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }

    // Optional: Visual feedback when player is in range
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // You could add a highlight effect here
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.8f);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
            }
        }
    }
}