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

    public event Action OnInventoryChanged;
    public event Action<Item> OnSelectedItemChanged;

    private Item selectedItem;

    void Start()
    {
        // Initialize inventory if needed
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
                OnInventoryChanged?.Invoke(); // Force UI refresh when selecting via number keys
            }
        }
    }

    public bool IsFull()
    {
        return items.Count >= 9 && !items.Exists(stack => stack == null || stack.item == null);
    }

    public bool AddItem(Item itemToAdd, int quantity = 1)
    {
        if (itemToAdd == null)
        {
            Debug.LogError("itemToAdd is null");
            return false;
        }

        // Try to stack with existing items first
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
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        // Add remaining quantity as new stack
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
            Debug.Log(selectedItem != null ? $"Selected item: {selectedItem.itemName}" : "Selected slot is empty.");
        }
    }

    public Item GetSelectedItem() => selectedItem;

    public void GiveSelectedItemToNPC(NPCInteraction npcInteraction)
    {
        if (selectedItem != null && npcInteraction != null)
        {
            npcInteraction.GiveGift(selectedItem);
            RemoveItem(selectedItem, 1);
        }
    }
}


