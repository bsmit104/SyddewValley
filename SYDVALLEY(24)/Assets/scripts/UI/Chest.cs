using UnityEngine;
using UnityEngine.SceneManagement;

public class Chest : MonoBehaviour
{
    [Header("Chest Identification")]
    [Tooltip("Unique ID for this chest (e.g., 'Town_ChestInHouse', 'City_ShopChest')")]
    public string chestID;

    private bool isPlayerInRange;
    private ChestInventory chestInventory;
    public Inventory playerInventory;

    public ChestInventory ChestInventory => chestInventory;
    public string ChestID => chestID;

    void Awake()
    {
        // Auto-generate chest ID if not set
        if (string.IsNullOrEmpty(chestID))
        {
            chestID = $"{SceneManager.GetActiveScene().name}_{gameObject.name}_{transform.position.x}_{transform.position.y}";
        }
    }

    void Start()
    {
        // Create a unique inventory instance for this chest
        chestInventory = gameObject.AddComponent<ChestInventory>();
        
        // Ensure ChestManager exists
        if (ChestManager.Instance == null)
        {
            Debug.LogError("ChestManager not found in scene!");
            enabled = false;
            return;
        }

        // Load chest contents from save
        LoadChestContents();
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ToggleChest();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            CloseChest();
        }
    }

    void ToggleChest()
    {
        if (ChestManager.Instance == null) return;
        
        if (ChestManager.Instance.IsChestOpen())
        {
            CloseChest();
        }
        else
        {
            OpenChest();
        }
    }

    void OpenChest()
    {
        if (ChestManager.Instance == null) return;
        ChestManager.Instance.OpenChest(this);
    }

    void CloseChest()
    {
        if (ChestManager.Instance == null) return;
        ChestManager.Instance.CloseChest();
        
        // Save chest contents when closing
        SaveChestContents();
    }

    private void LoadChestContents()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.LoadChestInventory(chestID, chestInventory);
        }
    }

    private void SaveChestContents()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveChestInventory(chestID, chestInventory);
        }
    }

    private void OnDestroy()
    {
        // Save chest contents when the scene unloads
        SaveChestContents();
    }
}


// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class Chest : MonoBehaviour
// {
//     private bool isPlayerInRange;
//     private ChestInventory chestInventory;
//     public Inventory playerInventory;

//     public ChestInventory ChestInventory => chestInventory;

//     void Start()
//     {
//         // Create a unique inventory instance for this chest
//         chestInventory = gameObject.AddComponent<ChestInventory>();
        
//         // Ensure ChestManager exists
//         if (ChestManager.Instance == null)
//         {
//             Debug.LogError("ChestManager not found in scene! Please add a ChestManager GameObject with the ChestManager script.");
//             enabled = false;
//             return;
//         }
//     }

//     void Update()
//     {
//         if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
//         {
//             ToggleChest();
//         }
//     }

//     void OnTriggerEnter2D(Collider2D other)
//     {
//         if (other.CompareTag("Player"))
//         {
//             isPlayerInRange = true;
//         }
//     }

//     void OnTriggerExit2D(Collider2D other)
//     {
//         if (other.CompareTag("Player"))
//         {
//             isPlayerInRange = false;
//             CloseChest();
//         }
//     }

//     void ToggleChest()
//     {
//         if (ChestManager.Instance == null) return;
        
//         if (ChestManager.Instance.IsChestOpen())
//         {
//             CloseChest();
//         }
//         else
//         {
//             OpenChest();
//         }
//     }

//     void OpenChest()
//     {
//         if (ChestManager.Instance == null) return;
//         ChestManager.Instance.OpenChest(this);
//     }

//     void CloseChest()
//     {
//         if (ChestManager.Instance == null) return;
//         ChestManager.Instance.CloseChest();
//     }

//     // public void TransferToChest(Inventory.ItemStack itemStack)
//     // {
//     //     if (chestInventory.AddItem(itemStack))
//     //     {
//     //         playerInventory.RemoveItem(itemStack.item, itemStack.stackSize);
//     //     }
//     // }

//     // public void TransferToPlayer(Inventory.ItemStack itemStack)
//     // {
//     //     if (playerInventory.AddItem(itemStack.item, itemStack.stackSize))
//     //     {
//     //         chestInventory.RemoveItem(itemStack.item, itemStack.stackSize);
//     //     }
//     // }
// }

