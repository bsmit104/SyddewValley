using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    private bool isPlayerInRange;
    private ChestInventory chestInventory;
    public Inventory playerInventory;

    public ChestInventory ChestInventory => chestInventory;

    void Start()
    {
        // Create a unique inventory instance for this chest
        chestInventory = gameObject.AddComponent<ChestInventory>();
        
        // Ensure ChestManager exists
        if (ChestManager.Instance == null)
        {
            Debug.LogError("ChestManager not found in scene! Please add a ChestManager GameObject with the ChestManager script.");
            enabled = false;
            return;
        }
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
    }

    // public void TransferToChest(Inventory.ItemStack itemStack)
    // {
    //     if (chestInventory.AddItem(itemStack))
    //     {
    //         playerInventory.RemoveItem(itemStack.item, itemStack.stackSize);
    //     }
    // }

    // public void TransferToPlayer(Inventory.ItemStack itemStack)
    // {
    //     if (playerInventory.AddItem(itemStack.item, itemStack.stackSize))
    //     {
    //         chestInventory.RemoveItem(itemStack.item, itemStack.stackSize);
    //     }
    // }
}


// public class Chest : MonoBehaviour
// {
//     public GameObject chestCanvas; // Assign this in the Inspector
//     private bool isPlayerInRange;
//     public ChestInventory chestInventory;
//     public Inventory playerInventory;

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
//         if (chestCanvas.activeSelf)
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
//         chestCanvas.SetActive(true);
//     }

//     void CloseChest()
//     {
//         chestCanvas.SetActive(false);
//     }

//     public void TransferItemToPlayerInventory(int itemIndex)
//     {
//         if (chestInventory != null && playerInventory != null)
//         {
//             if (itemIndex >= 0 && itemIndex < chestInventory.items.Count)
//             {
//                 var itemStack = chestInventory.items[itemIndex];
//                 if (itemStack != null && itemStack.item != null)
//                 {
//                     if (playerInventory.AddItem(itemStack.item, itemStack.stackSize))
//                     {
//                         chestInventory.RemoveItem(itemStack.item, itemStack.stackSize);
//                     }
//                 }
//             }
//         }
//     }

//     public void TransferItemToChestInventory(int itemIndex)
//     {
//         if (chestInventory != null && playerInventory != null)
//         {
//             if (itemIndex >= 0 && itemIndex < playerInventory.items.Count)
//             {
//                 var itemStack = playerInventory.items[itemIndex];
//                 if (itemStack != null && itemStack.item != null)
//                 {
//                     if (chestInventory.AddItem(itemStack.item, itemStack.stackSize))
//                     {
//                         playerInventory.RemoveItem(itemStack.item, itemStack.stackSize);
//                     }
//                 }
//             }
//         }
//     }
// }





// using UnityEngine;

// public class Chest : MonoBehaviour
// {
//     public GameObject chestCanvas; // Assign this in the Inspector
//     private bool isPlayerInRange;

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
//         if (chestCanvas.activeSelf)
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
//         chestCanvas.SetActive(true);
//     }

//     void CloseChest()
//     {
//         chestCanvas.SetActive(false);
//     }
// }
