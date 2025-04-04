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

    // public Item someItemReference;

    public event Action OnInventoryChanged;
    public event Action<Item> OnSelectedItemChanged;

    private Item selectedItem;

    void Start()
    {
        // if (someItemReference != null)
        // {
        //     items.Add(new ItemStack { item = someItemReference, stackSize = 1 });
        //     Debug.Log("Added test item to inventory: " + someItemReference.itemName);
        //     OnInventoryChanged?.Invoke();
        // }
        // else
        // {
        //     Debug.LogWarning("someItemReference is not assigned.");
        // }
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
        foreach (var itemStack in items)
        {
            if (itemStack == null || itemStack.item == null)
                return false;
        }
        return items.Count >= 9;
    }

    public bool AddItem(Item itemToAdd, int quantity = 1)
    {
        if (itemToAdd == null)
        {
            Debug.LogError("itemToAdd is null");
            return false;
        }

        Debug.Log("Adding item: " + itemToAdd.itemName + ", Quantity: " + quantity);

        // Try to stack the item in existing slots first
        foreach (var itemStack in items)
        {
            if (itemStack != null && itemStack.item != null && itemStack.item.itemName == itemToAdd.itemName && itemStack.stackSize < itemStack.item.maxStack)
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

        // If there's remaining quantity to add, find an empty slot
        if (quantity > 0 && !IsFull())
        {
            int availableSlotIndex = items.FindIndex(stack => stack == null || stack.item == null);
            if (availableSlotIndex == -1)
            {
                items.Add(new ItemStack { item = itemToAdd, stackSize = quantity });
            }
            else
            {
                items[availableSlotIndex] = new ItemStack { item = itemToAdd, stackSize = quantity };
            }
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
            if (stack != null && stack.item != null && stack.item.itemName == itemToRemove.itemName)
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
            selectedItem = items[index]?.item;
            OnSelectedItemChanged?.Invoke(selectedItem);
            if (selectedItem != null)
            {
                Debug.Log("Selected item: " + selectedItem.itemName);
            }
            else
            {
                Debug.Log("Selected slot is empty.");
            }
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


