using System;
using System.Collections.Generic;
using UnityEngine;

public class ChestInventory : MonoBehaviour
{
    public List<Inventory.ItemStack> items = new List<Inventory.ItemStack>();

    public event System.Action OnChestInventoryChanged;

    public bool AddItem(Item itemToAdd, int quantity = 1, int targetSlotIndex = -1)
    {
        if (itemToAdd == null)
        {
            Debug.LogError("itemToAdd is null");
            return false;
        }

        // If target slot is specified, try to add to that slot first
        if (targetSlotIndex >= 0)
        {
            // Ensure the list has enough slots
            while (items.Count <= targetSlotIndex)
            {
                items.Add(new Inventory.ItemStack { item = null, stackSize = 0 });
            }

            var targetStack = items[targetSlotIndex];
            if (targetStack.item == null)
            {
                items[targetSlotIndex] = new Inventory.ItemStack { item = itemToAdd, stackSize = quantity };
                OnChestInventoryChanged?.Invoke();
                return true;
            }
            else if (targetStack.item.itemName == itemToAdd.itemName && 
                     targetStack.stackSize < targetStack.item.maxStack)
            {
                int availableSpace = targetStack.item.maxStack - targetStack.stackSize;
                int toAdd = Mathf.Min(quantity, availableSpace);
                targetStack.stackSize += toAdd;
                quantity -= toAdd;
                
                if (quantity == 0)
                {
                    OnChestInventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        // Try to stack with existing items
        foreach (var itemStack in items)
        {
            if (itemStack.item != null && 
                itemStack.item.itemName == itemToAdd.itemName && 
                itemStack.stackSize < itemStack.item.maxStack)
            {
                int availableSpace = itemStack.item.maxStack - itemStack.stackSize;
                int toAdd = Mathf.Min(quantity, availableSpace);
                itemStack.stackSize += toAdd;
                quantity -= toAdd;
                
                if (quantity == 0)
                {
                    OnChestInventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        // Add remaining quantity as new stack
        if (quantity > 0)
        {
            items.Add(new Inventory.ItemStack { item = itemToAdd, stackSize = quantity });
            OnChestInventoryChanged?.Invoke();
            return true;
        }
        return false;
    }

    public void RemoveItem(Item itemToRemove, int quantity = 1)
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            var stack = items[i];
            if (stack.item != null && stack.item.itemName == itemToRemove.itemName)
            {
                if (quantity >= stack.stackSize)
                {
                    quantity -= stack.stackSize;
                    items.RemoveAt(i);
                }
                else
                {
                    stack.stackSize -= quantity;
                    quantity = 0;
                }

                if (quantity == 0)
                {
                    OnChestInventoryChanged?.Invoke();
                    return;
                }
            }
        }
        OnChestInventoryChanged?.Invoke();
    }
}




// using System.Collections.Generic;
// using System;
// using UnityEngine;

// public class ChestInventory : MonoBehaviour
// {
//     [SerializeField]
//     public List<Inventory.ItemStack> items = new List<Inventory.ItemStack>();

//     public event Action OnInventoryChanged; // Event declaration

//     public bool IsFull()
//     {
//         foreach (var itemStack in items)
//         {
//             if (itemStack == null || itemStack.item == null)
//                 return false;
//         }
//         return items.Count >= 9;
//     }

//     public bool AddItem(Item itemToAdd, int quantity = 1)
//     {
//         if (itemToAdd == null)
//         {
//             Debug.LogError("itemToAdd is null");
//             return false;
//         }

//         foreach (var itemStack in items)
//         {
//             if (itemStack != null && itemStack.item != null && itemStack.item.itemName == itemToAdd.itemName && itemStack.stackSize < itemStack.item.maxStack)
//             {
//                 int availableSpace = itemStack.item.maxStack - itemStack.stackSize;
//                 int toAdd = Mathf.Min(quantity, availableSpace);
//                 itemStack.stackSize += toAdd;
//                 quantity -= toAdd;
//                 if (quantity == 0)
//                 {
//                     OnInventoryChanged?.Invoke(); // Raise the event
//                     return true;
//                 }
//             }
//         }

//         if (quantity > 0 && !IsFull())
//         {
//             int availableSlotIndex = items.FindIndex(stack => stack == null || stack.item == null);
//             if (availableSlotIndex == -1)
//             {
//                 items.Add(new Inventory.ItemStack { item = itemToAdd, stackSize = quantity });
//             }
//             else
//             {
//                 items[availableSlotIndex] = new Inventory.ItemStack { item = itemToAdd, stackSize = quantity };
//             }
//             OnInventoryChanged?.Invoke(); // Raise the event
//             return true;
//         }
//         return false;
//     }

//     public void RemoveItem(Item itemToRemove, int quantity = 1)
//     {
//         for (int i = items.Count - 1; i >= 0; i--)
//         {
//             Inventory.ItemStack stack = items[i];
//             if (stack != null && stack.item != null && stack.item.itemName == itemToRemove.itemName)
//             {
//                 if (quantity >= stack.stackSize)
//                 {
//                     quantity -= stack.stackSize;
//                     items.RemoveAt(i);
//                 }
//                 else
//                 {
//                     stack.stackSize -= quantity;
//                     quantity = 0;
//                 }

//                 if (quantity == 0)
//                 {
//                     OnInventoryChanged?.Invoke(); // Raise the event
//                     return;
//                 }
//             }
//         }
//         OnInventoryChanged?.Invoke(); // Raise the event
//     }
// }
















// using System.Collections;
// using System.Collections.Generic;
// using System;
// using UnityEngine;

// public class ChestInventory : MonoBehaviour
// {
//     [SerializeField]
//     public List<Inventory.ItemStack> items = new List<Inventory.ItemStack>();

//     public event Action OnInventoryChanged;

//     public bool IsFull()
//     {
//         foreach (var itemStack in items)
//         {
//             if (itemStack == null || itemStack.item == null)
//                 return false;
//         }
//         return items.Count >= 9;
//     }

//     public bool AddItem(Item itemToAdd, int quantity = 1)
//     {
//         if (itemToAdd == null)
//         {
//             Debug.LogError("itemToAdd is null");
//             return false;
//         }

//         foreach (var itemStack in items)
//         {
//             if (itemStack != null && itemStack.item != null && itemStack.item.itemName == itemToAdd.itemName && itemStack.stackSize < itemStack.item.maxStack)
//             {
//                 int availableSpace = itemStack.item.maxStack - itemStack.stackSize;
//                 int toAdd = Mathf.Min(quantity, availableSpace);
//                 itemStack.stackSize += toAdd;
//                 quantity -= toAdd;
//                 if (quantity == 0)
//                 {
//                     OnInventoryChanged?.Invoke();
//                     return true;
//                 }
//             }
//         }

//         if (quantity > 0 && !IsFull())
//         {
//             int availableSlotIndex = items.FindIndex(stack => stack == null || stack.item == null);
//             if (availableSlotIndex == -1)
//             {
//                 items.Add(new Inventory.ItemStack { item = itemToAdd, stackSize = quantity });
//             }
//             else
//             {
//                 items[availableSlotIndex] = new Inventory.ItemStack { item = itemToAdd, stackSize = quantity };
//             }
//             OnInventoryChanged?.Invoke();
//             return true;
//         }
//         return false;
//     }

//     public void RemoveItem(Item itemToRemove, int quantity = 1)
//     {
//         for (int i = items.Count - 1; i >= 0; i--)
//         {
//             Inventory.ItemStack stack = items[i];
//             if (stack != null && stack.item != null && stack.item.itemName == itemToRemove.itemName)
//             {
//                 if (quantity >= stack.stackSize)
//                 {
//                     quantity -= stack.stackSize;
//                     items.RemoveAt(i);
//                 }
//                 else
//                 {
//                     stack.stackSize -= quantity;
//                     quantity = 0;
//                 }

//                 if (quantity == 0)
//                 {
//                     OnInventoryChanged?.Invoke();
//                     return;
//                 }
//             }
//         }
//         OnInventoryChanged?.Invoke();
//     }
// }
