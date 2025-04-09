using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    [SerializeField] private bool debugMode = false;
    [Tooltip("Optional direct reference to shop manager")]
    [SerializeField] private ShopManager shopManagerReference;
    [SerializeField] private ShopData shopData;
    
    // Visual indicator for debugging
    [SerializeField] private Color gizmoColor = Color.green;
    [SerializeField] private float gizmoRadius = 0.5f;
    
    private bool playerInRange = false;

    void Start()
    {
        if (shopManagerReference == null)
        {
            shopManagerReference = FindObjectOfType<ShopManager>();
        }

        if (shopData == null)
        {
            Debug.LogError("ShopTrigger: No ShopData assigned!");
        }
    }

    void Update()
    {
        // Allow player to press E to open shop when in range
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.ToggleShop(shopData);
            }
            else if (shopManagerReference != null)
            {
                shopManagerReference.ToggleShop(shopData);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (debugMode)
            {
                Debug.Log("Player entered shop area");
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (debugMode)
            {
                Debug.Log("Player exited shop area");
            }
            
            // Close shop if open
            if (ShopManager.Instance != null && ShopManager.Instance.IsShopOpen())
            {
                ShopManager.Instance.CloseShop();
            }
            else if (shopManagerReference != null && shopManagerReference.IsShopOpen())
            {
                shopManagerReference.CloseShop();
            }
        }
    }
    
    void OnDrawGizmos()
    {
        // Visual indicator in Scene view
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
    }
} 