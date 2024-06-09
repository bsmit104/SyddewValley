using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    public List<ItemStack> items = new List<ItemStack>();

    [System.Serializable]
    public class ItemStack
    {
        public Item item;
        public int stackSize;
    }

    public Item someItemReference;

    // Event to notify when the inventory changes
    public event Action OnInventoryChanged;

    void Start()
    {
        if (someItemReference != null)
        {
            items.Add(new ItemStack { item = someItemReference, stackSize = 1 });
            Debug.Log("Added test item to inventory: " + someItemReference.itemName);
            OnInventoryChanged?.Invoke();
        }
        else
        {
            Debug.LogWarning("someItemReference is not assigned.");
        }
    }

    public bool IsFull()
    {
        return items.Count >= 9; // Assuming 9 is the maximum number of different items the inventory can hold
    }

    // public void AddItem(Item itemToAdd, int quantity = 1)
    // {
    //     foreach (var itemStack in items)
    //     {
    //         if (itemStack.item.itemName == itemToAdd.itemName && itemStack.stackSize < itemStack.item.maxStack)
    //         {
    //             int availableSpace = itemStack.item.maxStack - itemStack.stackSize;
    //             int toAdd = Mathf.Min(quantity, availableSpace);
    //             itemStack.stackSize += toAdd;
    //             quantity -= toAdd;
    //             if (quantity == 0)
    //             {
    //                 OnInventoryChanged?.Invoke();
    //                 return;
    //             }
    //         }
    //     }
    //     if (quantity > 0)
    //     {
    //         items.Add(new ItemStack { item = itemToAdd, stackSize = quantity });
    //         OnInventoryChanged?.Invoke();
    //     }
    // }

    public bool AddItem(Item itemToAdd, int quantity = 1)
    {
        foreach (var itemStack in items)
        {
            if (itemStack.item.itemName == itemToAdd.itemName && itemStack.stackSize < itemStack.item.maxStack)
            {
                int availableSpace = itemStack.item.maxStack - itemStack.stackSize;
                int toAdd = Mathf.Min(quantity, availableSpace);
                itemStack.stackSize += toAdd;
                quantity -= toAdd;
                if (quantity == 0)
                {
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
        }
        if (quantity > 0 && !IsFull())
        {
            items.Add(new ItemStack { item = itemToAdd, stackSize = quantity });
            OnInventoryChanged?.Invoke();
            return true;
        }
        return false;
    }

    public bool RemoveItem(Item itemToRemove, int quantity = 1)
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            ItemStack stack = items[i];
            if (stack.item.itemName == itemToRemove.itemName)
            {
                if (quantity >= stack.stackSize)
                {
                    quantity -= stack.stackSize;
                    items.RemoveAt(i);
                }
                else
                {
                    stack.stackSize -= quantity;
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
        }
        OnInventoryChanged?.Invoke();
        return quantity == 0;
    }
}

// using System.Collections.Generic;
// using UnityEngine;

// public class Inventory : MonoBehaviour
// {
//     [SerializeField]
//     public List<ItemStack> items = new List<ItemStack>();

//     [System.Serializable]
//     public class ItemStack
//     {
//         public Item item;
//         public int stackSize;
//     }

//     // Reference to an item for testing purposes
//     public Item someItemReference;

//     void Start()
//     {
//         // Initial setup if necessary
//         if (someItemReference != null)
//         {
//             items.Add(new ItemStack { item = someItemReference, stackSize = 1 });
//             Debug.Log("Added test item to inventory: " + someItemReference.itemName);
//         }
//         else
//         {
//             Debug.LogWarning("someItemReference is not assigned.");
//         }
//     }

//     // Additional methods will be added here

//     public void AddItem(Item itemToAdd, int quantity = 1)
//     {
//         foreach (var itemStack in items)
//         {
//             if (itemStack.item.itemName == itemToAdd.itemName && itemStack.stackSize < itemStack.item.maxStack)
//             {
//                 int availableSpace = itemStack.item.maxStack - itemStack.stackSize;
//                 int toAdd = Mathf.Min(quantity, availableSpace);
//                 itemStack.stackSize += toAdd;
//                 quantity -= toAdd;
//                 if (quantity == 0) return;
//             }
//         }
//         if (quantity > 0)
//         {
//             items.Add(new ItemStack { item = itemToAdd, stackSize = quantity });
//         }
//     }

//     public bool RemoveItem(Item itemToRemove, int quantity = 1)
//     {
//         for (int i = items.Count - 1; i >= 0; i--)
//         {
//             ItemStack stack = items[i];
//             if (stack.item.itemName == itemToRemove.itemName)
//             {
//                 if (quantity >= stack.stackSize)
//                 {
//                     quantity -= stack.stackSize;
//                     items.RemoveAt(i);
//                 }
//                 else
//                 {
//                     stack.stackSize -= quantity;
//                     return true;
//                 }
//             }
//         }
//         return quantity == 0;
//     }
// }