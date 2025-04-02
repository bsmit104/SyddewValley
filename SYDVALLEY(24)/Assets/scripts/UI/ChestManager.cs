using UnityEngine;

public class ChestManager : MonoBehaviour
{
    public static ChestManager Instance { get; private set; }
    
    [SerializeField] private GameObject chestCanvas;
    [SerializeField] private ChestInventoryUI chestInventoryUI;
    private Chest currentChest;

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
        if (chestCanvas == null || chestInventoryUI == null)
        {
            Debug.LogError("ChestManager: Required components not assigned!");
            return;
        }
        chestCanvas.SetActive(false);
    }

    public void OpenChest(Chest chest)
    {
        // If we're already looking at a chest, close it first
        if (currentChest != null && currentChest != chest)
        {
            CloseChest();
        }

        currentChest = chest;
        chestInventoryUI.chestInventory = chest.ChestInventory;
        chestCanvas.SetActive(true);
        
        // Force UI update to show the correct inventory
        chestInventoryUI.UpdateChestInventoryUI();
    }

    public void CloseChest()
    {
        if (currentChest != null)
        {
            currentChest = null;
            chestInventoryUI.chestInventory = null;
            chestCanvas.SetActive(false);
            
            // Clear the UI
            chestInventoryUI.UpdateChestInventoryUI();
        }
    }

    public bool IsChestOpen() => chestCanvas.activeSelf;
} 