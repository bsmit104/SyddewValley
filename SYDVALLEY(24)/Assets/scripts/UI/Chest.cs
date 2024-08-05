using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public GameObject chestCanvas; // Assign this in the Inspector
    private bool isPlayerInRange;
    public ChestInventory chestInventory;
    public Inventory playerInventory;

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
        if (chestCanvas.activeSelf)
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
        chestCanvas.SetActive(true);
    }

    void CloseChest()
    {
        chestCanvas.SetActive(false);
    }

    public void TransferItemToPlayerInventory(int itemIndex)
    {
        if (chestInventory != null && playerInventory != null)
        {
            if (itemIndex >= 0 && itemIndex < chestInventory.items.Count)
            {
                var itemStack = chestInventory.items[itemIndex];
                if (itemStack != null && itemStack.item != null)
                {
                    if (playerInventory.AddItem(itemStack.item, itemStack.stackSize))
                    {
                        chestInventory.RemoveItem(itemStack.item, itemStack.stackSize);
                    }
                }
            }
        }
    }

    public void TransferItemToChestInventory(int itemIndex)
    {
        if (chestInventory != null && playerInventory != null)
        {
            if (itemIndex >= 0 && itemIndex < playerInventory.items.Count)
            {
                var itemStack = playerInventory.items[itemIndex];
                if (itemStack != null && itemStack.item != null)
                {
                    if (chestInventory.AddItem(itemStack.item, itemStack.stackSize))
                    {
                        playerInventory.RemoveItem(itemStack.item, itemStack.stackSize);
                    }
                }
            }
        }
    }
}

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
