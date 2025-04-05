using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopDebugger : MonoBehaviour
{
    [SerializeField] private bool enableDebug = true;
    [SerializeField] private GameObject debugPanel;
    [SerializeField] private TextMeshProUGUI debugText;
    
    private ShopManager shopManager;
    private MoneyManager moneyManager;
    private InventoryUI inventoryUI;
    private ShopUI shopUI;
    
    void Start()
    {
        // Find all shop-related components
        shopManager = FindObjectOfType<ShopManager>();
        moneyManager = FindObjectOfType<MoneyManager>();
        inventoryUI = FindObjectOfType<InventoryUI>();
        shopUI = FindObjectOfType<ShopUI>();
        
        if (debugPanel) debugPanel.SetActive(enableDebug);
        
        // Check components
        CheckComponents();
    }
    
    void Update()
    {
        if (!enableDebug) return;
        
        // Update debug info every frame
        UpdateDebugInfo();
    }
    
    private void CheckComponents()
    {
        string status = "Shop System Status:\n";
        
        // Check ShopManager
        status += "ShopManager: " + (shopManager != null ? "Found ✓" : "MISSING ✗") + "\n";
        
        // Check MoneyManager
        status += "MoneyManager: " + (moneyManager != null ? "Found ✓" : "MISSING ✗") + "\n";
        
        // Check InventoryUI
        status += "InventoryUI: " + (inventoryUI != null ? "Found ✓" : "MISSING ✗") + "\n";
        
        // Check ShopUI
        status += "ShopUI: " + (shopUI != null ? "Found ✓" : "MISSING ✗") + "\n";
        
        // Check ShopCanvas
        if (shopManager != null)
        {
            var shopCanvas = shopManager.GetComponent<Canvas>();
            status += "ShopCanvas: " + (shopCanvas != null ? "Found ✓" : "MISSING ✗") + "\n";
        }
        else
        {
            status += "ShopCanvas: UNKNOWN (ShopManager missing)\n";
        }
        
        // Check Player Tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        status += "Player with 'Player' tag: " + (player != null ? "Found ✓" : "MISSING ✗") + "\n";
        
        Debug.Log(status);
        if (debugText) debugText.text = status;
    }
    
    private void UpdateDebugInfo()
    {
        if (debugText == null) return;
        
        string info = "Shop System Status:\n";
        
        // ShopManager status
        if (shopManager != null)
        {
            info += $"Shop Open: {shopManager.IsShopOpen()}\n";
        }
        
        // Money status
        if (moneyManager != null)
        {
            info += $"Money: ${moneyManager.GetCurrentMoney()}\n";
        }
        
        // Player in shop range
        if (inventoryUI != null)
        {
            info += $"In Shop Range: {inventoryUI.GetComponent<InventoryUI>() != null}\n";
        }
        
        debugText.text = info;
    }
    
    [ContextMenu("Force Debug Output")]
    public void ForceDebugOutput()
    {
        CheckComponents();
    }
} 