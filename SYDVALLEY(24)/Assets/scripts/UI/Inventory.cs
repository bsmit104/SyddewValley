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

    public event Action OnInventoryChanged;
    public event Action<Item> OnSelectedItemChanged;

    private Item selectedItem;

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

    void Update()
    {
        HandleNumberKeyInput();
    }

    private void HandleNumberKeyInput()
    {
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectItem(i);
            }
        }
    }

    public bool IsFull()
    {
        return items.Count >= 9; 
    }

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

    public void RemoveItem(Item itemToRemove, int quantity = 1)
    {
        Debug.Log($"Trying to remove {quantity} of {itemToRemove.itemName}");
        for (int i = items.Count - 1; i >= 0; i--)
        {
            ItemStack stack = items[i];
            if (stack.item.itemName == itemToRemove.itemName)
            {
                if (quantity >= stack.stackSize)
                {
                    quantity -= stack.stackSize;
                    Debug.Log($"Removing stack {stack.stackSize} of {stack.item.itemName}");
                    items.RemoveAt(i);
                }
                else
                {
                    stack.stackSize -= quantity;
                    Debug.Log($"Reducing stack by {quantity}, new count {stack.stackSize}");
                    quantity = 0;
                }

                if (quantity == 0)
                {
                    OnInventoryChanged?.Invoke();
                    // Deselect the item if it's the selected item being removed
                    if (selectedItem == stack.item)
                    {
                        selectedItem = null;
                        OnSelectedItemChanged?.Invoke(null);
                    }
                    return;
                }
            }
        }
        OnInventoryChanged?.Invoke();
    }

    public void SelectItem(int index)
    {
        if (index >= 0 && index < items.Count)
        {
            selectedItem = items[index].item;
            OnSelectedItemChanged?.Invoke(selectedItem);
            Debug.Log("Selected item: " + selectedItem.itemName);
        }
    }

    public Item GetSelectedItem()
    {
        return selectedItem;
    }

    public void GiveSelectedItemToNPC(NPCInteraction npcInteraction)
    {
        if (selectedItem != null && npcInteraction != null)
        {
            npcInteraction.GiveGift(selectedItem);
            RemoveItem(selectedItem, 1); // Remove one instance of the item from inventory
        }
    }
}

// using System;
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

//     public Item someItemReference;

//     public event Action OnInventoryChanged;
//     public event Action<Item> OnSelectedItemChanged;

//     private Item selectedItem;

//     void Start()
//     {
//         if (someItemReference != null)
//         {
//             items.Add(new ItemStack { item = someItemReference, stackSize = 1 });
//             Debug.Log("Added test item to inventory: " + someItemReference.itemName);
//             OnInventoryChanged?.Invoke();
//         }
//         else
//         {
//             Debug.LogWarning("someItemReference is not assigned.");
//         }
//     }

//     void Update()
//     {
//         HandleNumberKeyInput();
//     }

//     private void HandleNumberKeyInput()
//     {
//         for (int i = 0; i < 9; i++)
//         {
//             if (Input.GetKeyDown(KeyCode.Alpha1 + i))
//             {
//                 SelectItem(i);
//             }
//         }
//     }

//     public bool IsFull()
//     {
//         return items.Count >= 9; 
//     }

//     public bool AddItem(Item itemToAdd, int quantity = 1)
//     {
//         foreach (var itemStack in items)
//         {
//             if (itemStack.item.itemName == itemToAdd.itemName && itemStack.stackSize < itemStack.item.maxStack)
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
//             items.Add(new ItemStack { item = itemToAdd, stackSize = quantity });
//             OnInventoryChanged?.Invoke();
//             return true;
//         }
//         return false;
//     }

//     [Obsolete]
//     public bool RemoveItem(Item itemToRemove, int quantity = 1)
//     {
//         Debug.Log($"Trying to remove {quantity} of {itemToRemove.itemName}");
//         for (int i = items.Count - 1; i >= 0; i--)
//         {
//             ItemStack stack = items[i];
//             if (stack.item.itemName == itemToRemove.itemName)
//             {
//                 if (quantity >= stack.stackSize)
//                 {
//                     quantity -= stack.stackSize;
//                     Debug.Log($"Removing stack {stack.stackSize} of {stack.item.itemName}");
//                     items.RemoveAt(i);
//                 }
//                 else
//                 {
//                     stack.stackSize -= quantity;
//                     Debug.Log($"Reducing stack by {quantity}, new count {stack.stackSize}");
//                     quantity = 0;
//                 }

//                 if (quantity == 0)
//                 {
//                     OnInventoryChanged?.Invoke();
//                     return true;
//                 }
//             }
//         }
//         OnInventoryChanged?.Invoke();
//         return quantity == 0;
//     }

//     public void SelectItem(int index)
//     {
//         if (index >= 0 && index < items.Count)
//         {
//             selectedItem = items[index].item;
//             OnSelectedItemChanged?.Invoke(selectedItem);
//             Debug.Log("Selected item: " + selectedItem.itemName);
//         }
//     }

//     public Item GetSelectedItem()
//     {
//         return selectedItem;
//     }

//     [Obsolete]
//     public void GiveSelectedItemToNPC(NPCInteraction npcInteraction)
//     {
//         if (selectedItem != null && npcInteraction != null)
//         {
//             npcInteraction.GiveGift(selectedItem);
//             RemoveItem(selectedItem, 1); // Remove one instance of the item from inventory
//         }
//     }
// }



////////////////good with updated trash and selected item////////////////////
// using System;
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

//     public Item someItemReference;

//     // Event to notify when the inventory changes
//     public event Action OnInventoryChanged;

//     // Event to notify when the selected item changes
//     public event Action<Item> OnSelectedItemChanged;

//     private Item selectedItem;

//     void Start()
//     {
//         if (someItemReference != null)
//         {
//             items.Add(new ItemStack { item = someItemReference, stackSize = 1 });
//             Debug.Log("Added test item to inventory: " + someItemReference.itemName);
//             OnInventoryChanged?.Invoke();
//         }
//         else
//         {
//             Debug.LogWarning("someItemReference is not assigned.");
//         }
//     }

//     void Update()
//     {
//         HandleNumberKeyInput();
//     }

//     private void HandleNumberKeyInput()
//     {
//         for (int i = 0; i < 9; i++)
//         {
//             if (Input.GetKeyDown(KeyCode.Alpha1 + i))
//             {
//                 SelectItem(i);
//             }
//         }
//     }

//     public bool IsFull()
//     {
//         return items.Count >= 9; // Assuming 9 is the maximum number of different items the inventory can hold
//     }

//     public bool AddItem(Item itemToAdd, int quantity = 1)
//     {
//         foreach (var itemStack in items)
//         {
//             if (itemStack.item.itemName == itemToAdd.itemName && itemStack.stackSize < itemStack.item.maxStack)
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
//             items.Add(new ItemStack { item = itemToAdd, stackSize = quantity });
//             OnInventoryChanged?.Invoke();
//             return true;
//         }
//         return false;
//     }

//     // public bool RemoveItem(Item itemToRemove, int quantity = 1)
//     // {
//     //     Debug.Log($"Trying to remove {quantity} of {itemToRemove.itemName}");
//     //     for (int i = items.Count - 1; i >= 0; i--)
//     //     {
//     //         ItemStack stack = items[i];
//     //         if (stack.item.itemName == itemToRemove.itemName)
//     //         {
//     //             if (quantity >= stack.stackSize)
//     //             {
//     //                 quantity -= stack.stackSize;
//     //                 Debug.Log($"Removing stack {stack.stackSize} of {stack.item.itemName}");
//     //                 items.RemoveAt(i);
//     //             }
//     //             else
//     //             {
//     //                 stack.stackSize -= quantity;
//     //                 Debug.Log($"Reducing stack by {quantity}, new count {stack.stackSize}");
//     //                 quantity = 0;
//     //             }

//     //             if (quantity == 0)
//     //             {
//     //                 OnInventoryChanged?.Invoke();
//     //                 return true;
//     //             }
//     //         }
//     //     }
//     //     OnInventoryChanged?.Invoke();
//     //     return quantity == 0;
//     // }

//     [Obsolete]
//     public bool RemoveItem(Item itemToRemove, int quantity = 1)
// {
//     Debug.Log($"Trying to remove {quantity} of {itemToRemove.itemName}");
//     for (int i = items.Count - 1; i >= 0; i--)
//     {
//         ItemStack stack = items[i];
//         if (stack.item.itemName == itemToRemove.itemName)
//         {
//             if (quantity >= stack.stackSize)
//             {
//                 quantity -= stack.stackSize;
//                 Debug.Log($"Removing stack {stack.stackSize} of {stack.item.itemName}");
//                 items.RemoveAt(i);
//             }
//             else
//             {
//                 stack.stackSize -= quantity;
//                 Debug.Log($"Reducing stack by {quantity}, new count {stack.stackSize}");
//                 quantity = 0;
//             }

//             if (quantity == 0)
//             {
//                 OnInventoryChanged?.Invoke();

//                 // Get reference to the InventoryUI script
//                 InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
//                 if (inventoryUI != null)
//                 {
//                     // Update the selected item display if the removed item was the selected item
//                     if (selectedItem == itemToRemove)
//                     {
//                         selectedItem = null; // Deselect the item
//                         OnSelectedItemChanged?.Invoke(null); // Notify that no item is selected
//                         inventoryUI.UpdateSelectedItemDisplay(null); // Update the display
//                     }
//                 }
//                 return true;
//             }
//         }
//     }
//     OnInventoryChanged?.Invoke();
//     return quantity == 0;
// }

//     public void SelectItem(int index)
//     {
//         if (index >= 0 && index < items.Count)
//         {
//             selectedItem = items[index].item;
//             OnSelectedItemChanged?.Invoke(selectedItem);
//             Debug.Log("Selected item: " + selectedItem.itemName);
//         }
//     }

//     public Item GetSelectedItem()
//     {
//         return selectedItem;
//     }
// }

///////////////////////gooood////////////////////////
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

//     public Item someItemReference;

//     // Event to notify when the inventory changes
//     public event Action OnInventoryChanged;

//     void Start()
//     {
//         if (someItemReference != null)
//         {
//             items.Add(new ItemStack { item = someItemReference, stackSize = 1 });
//             Debug.Log("Added test item to inventory: " + someItemReference.itemName);
//             OnInventoryChanged?.Invoke();
//         }
//         else
//         {
//             Debug.LogWarning("someItemReference is not assigned.");
//         }
//     }

//     public bool IsFull()
//     {
//         return items.Count >= 9; // Assuming 9 is the maximum number of different items the inventory can hold
//     }
//     public bool AddItem(Item itemToAdd, int quantity = 1)
//     {
//         foreach (var itemStack in items)
//         {
//             if (itemStack.item.itemName == itemToAdd.itemName && itemStack.stackSize < itemStack.item.maxStack)
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
//             items.Add(new ItemStack { item = itemToAdd, stackSize = quantity });
//             OnInventoryChanged?.Invoke();
//             return true;
//         }
//         return false;
//     }

//     public bool RemoveItem(Item itemToRemove, int quantity = 1)
//     {
//         Debug.Log($"Trying to remove {quantity} of {itemToRemove.itemName}");
//         for (int i = items.Count - 1; i >= 0; i--)
//         {
//             ItemStack stack = items[i];
//             if (stack.item.itemName == itemToRemove.itemName)
//             {
//                 if (quantity >= stack.stackSize)
//                 {
//                     quantity -= stack.stackSize;
//                     Debug.Log($"Removing stack {stack.stackSize} of {stack.item.itemName}");
//                     items.RemoveAt(i);
//                 }
//                 else
//                 {
//                     stack.stackSize -= quantity;
//                     Debug.Log($"Reducing stack by {quantity}, new count {stack.stackSize}");
//                     quantity = 0;
//                 }

//                 if (quantity == 0)
//                 {
//                     OnInventoryChanged?.Invoke();
//                     return true;
//                 }
//             }
//         }
//         OnInventoryChanged?.Invoke();
//         return quantity == 0;
//     }
// }

////////////////////////////////////////////////////////





    // public bool RemoveItem(Item itemToRemove, int quantity = 1)
    // {
    //     for (int i = items.Count - 1; i >= 0; i--)
    //     {
    //         ItemStack stack = items[i];
    //         if (stack.item.itemName == itemToRemove.itemName)
    //         {
    //             if (quantity >= stack.stackSize)
    //             {
    //                 quantity -= stack.stackSize;
    //                 items.RemoveAt(i);
    //             }
    //             else
    //             {
    //                 stack.stackSize -= quantity;
    //                 OnInventoryChanged?.Invoke();
    //                 return true;
    //             }
    //         }
    //     }
    //     OnInventoryChanged?.Invoke();
    //     return quantity == 0;
    // }


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