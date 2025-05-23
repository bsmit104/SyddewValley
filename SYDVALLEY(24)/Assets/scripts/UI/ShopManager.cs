using UnityEngine;
using System.Collections;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [SerializeField] private GameObject shopCanvas;
    [SerializeField] private ShopUI shopUI;
    
    // Simple debug toggle
    [SerializeField] private bool debugMode = false;

    private bool isShopOpen = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Try to find shopCanvas if not assigned
        if (shopCanvas == null)
        {
            Debug.LogWarning("ShopManager: Shop Canvas not assigned!");
            return;
        }

        // Try to find shopUI if not assigned
        if (shopUI == null)
        {
            shopUI = shopCanvas.GetComponentInChildren<ShopUI>(true);
            if (shopUI == null)
            {
                Debug.LogError("ShopManager: ShopUI component not found!");
                return;
            }
        }

        // Start with shop closed
        shopCanvas.SetActive(false);
        isShopOpen = false;
    }

    public void OpenShop(ShopData shopData)
    {
        if (isShopOpen) return;
        
        // Set the shop data
        if (shopUI != null)
        {
            shopUI.SetShopData(shopData);
        }
        
        shopCanvas.SetActive(true);
        isShopOpen = true;
        
        // Notify inventory about shop state
        var inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.SetInShopRange(true);
        }
        
        // Refresh shop UI - do this safely
        if (shopUI != null)
        {
            try
            {
                // Delay the refresh to ensure canvas is active
                StartCoroutine(RefreshShopAfterDelay());
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error refreshing shop: {e.Message}");
            }
        }
    }

    private System.Collections.IEnumerator RefreshShopAfterDelay()
    {
        // Wait for 2 frames to ensure everything is initialized
        yield return null;
        yield return null;
        
        if (shopUI != null && shopUI.isActiveAndEnabled)
        {
            shopUI.RefreshShopItems();
        }
    }

    public void CloseShop()
    {
        if (!isShopOpen) return;
        
        // Notify inventory about shop state before closing
        var inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null && inventoryUI.isActiveAndEnabled)
        {
            inventoryUI.SetInShopRange(false);
        }
        
        // Close the shop after updating inventory
        shopCanvas.SetActive(false);
        isShopOpen = false;
    }

    public void ToggleShop(ShopData shopData)
    {
        if (isShopOpen)
        {
            CloseShop();
        }
        else
        {
            OpenShop(shopData);
        }
    }

    public bool IsShopOpen()
    {
        return isShopOpen;
    }
} 