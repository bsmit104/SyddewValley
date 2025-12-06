using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    [SerializeField] private bool debugMode = false;
    [SerializeField] private ShopData shopData;
    
    // Visual indicator for debugging
    [SerializeField] private Color gizmoColor = Color.green;
    [SerializeField] private float gizmoRadius = 0.5f;
    
    private bool playerInRange = false;
    private ShopManager shopManager;

    void Start()
    {
        // Always find the ShopManager instance (it persists across scenes)
        shopManager = ShopManager.Instance;
        
        if (shopManager == null)
        {
            Debug.LogError("ShopTrigger: ShopManager.Instance not found! Make sure ShopManager exists in the first scene.");
        }

        if (shopData == null)
        {
            Debug.LogError("ShopTrigger: No ShopData assigned!");
        }
        
        if (debugMode)
        {
            Debug.Log($"ShopTrigger initialized for shop: {shopData?.shopName}");
        }
    }

    void Update()
    {
        // Allow player to press E to open shop when in range
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (shopManager != null && shopData != null)
            {
                shopManager.ToggleShop(shopData);
                
                if (debugMode)
                {
                    Debug.Log($"Toggling shop: {shopData.shopName}");
                }
            }
            else
            {
                Debug.LogWarning("Cannot open shop - missing ShopManager or ShopData");
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
                Debug.Log($"Player entered shop area: {shopData?.shopName}");
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
                Debug.Log($"Player exited shop area: {shopData?.shopName}");
            }
            
            // Close shop if open
            if (shopManager != null && shopManager.IsShopOpen())
            {
                shopManager.CloseShop();
            }
        }
    }
    
    void OnDrawGizmos()
    {
        // Visual indicator in Scene view
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
        
        // Show shop name in scene view
        #if UNITY_EDITOR
        if (shopData != null)
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, shopData.shopName);
        }
        #endif
    }
}

// using UnityEngine;

// public class ShopTrigger : MonoBehaviour
// {
//     [SerializeField] private bool debugMode = false;
//     [Tooltip("Optional direct reference to shop manager")]
//     [SerializeField] private ShopManager shopManagerReference;
//     [SerializeField] private ShopData shopData;
    
//     // Visual indicator for debugging
//     [SerializeField] private Color gizmoColor = Color.green;
//     [SerializeField] private float gizmoRadius = 0.5f;
    
//     private bool playerInRange = false;

//     void Start()
//     {
//         if (shopManagerReference == null)
//         {
//             shopManagerReference = FindObjectOfType<ShopManager>();
//         }

//         if (shopData == null)
//         {
//             Debug.LogError("ShopTrigger: No ShopData assigned!");
//         }
//     }

//     void Update()
//     {
//         // Allow player to press E to open shop when in range
//         if (playerInRange && Input.GetKeyDown(KeyCode.E))
//         {
//             if (ShopManager.Instance != null)
//             {
//                 ShopManager.Instance.ToggleShop(shopData);
//             }
//             else if (shopManagerReference != null)
//             {
//                 shopManagerReference.ToggleShop(shopData);
//             }
//         }
//     }

//     void OnTriggerEnter2D(Collider2D other)
//     {
//         if (other.CompareTag("Player"))
//         {
//             playerInRange = true;
//             if (debugMode)
//             {
//                 Debug.Log("Player entered shop area");
//             }
//         }
//     }

//     void OnTriggerExit2D(Collider2D other)
//     {
//         if (other.CompareTag("Player"))
//         {
//             playerInRange = false;
//             if (debugMode)
//             {
//                 Debug.Log("Player exited shop area");
//             }
            
//             // Close shop if open
//             if (ShopManager.Instance != null && ShopManager.Instance.IsShopOpen())
//             {
//                 ShopManager.Instance.CloseShop();
//             }
//             else if (shopManagerReference != null && shopManagerReference.IsShopOpen())
//             {
//                 shopManagerReference.CloseShop();
//             }
//         }
//     }
    
//     void OnDrawGizmos()
//     {
//         // Visual indicator in Scene view
//         Gizmos.color = gizmoColor;
//         Gizmos.DrawWireSphere(transform.position, gizmoRadius);
//     }
// } 