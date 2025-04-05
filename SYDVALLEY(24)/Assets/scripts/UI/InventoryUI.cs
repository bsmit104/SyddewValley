using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] public Inventory inventory;
    [SerializeField] private GridLayoutGroup slotGrid;
    [SerializeField] private Image trashCanImage;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject draggedItemPrefab;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    [SerializeField] private Color hoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    [SerializeField] private Color sellableColor = new Color(0.7f, 1f, 0.7f, 1f);
    [SerializeField] private Color sellableSelectedColor = new Color(0.5f, 0.9f, 0.5f, 1f); // Bright green for selected sellable
    [SerializeField] private Color sellableHoverColor = new Color(0.6f, 0.9f, 0.6f, 1f); // Medium green for hover

    private readonly List<GameObject> slots = new List<GameObject>();
    private GameObject draggedItem;
    private int draggedItemIndex = -1;
    private int selectedSlotIndex = -1;
    private int hoveredSlotIndex = -1;
    private bool isInShopRange = false;

    void Start()
    {
        if (inventory == null)
        {
            Debug.LogError("Inventory is not assigned!");
            return;
        }

        if (slotGrid == null)
        {
            Debug.LogError("Slot Grid is not assigned!");
            return;
        }

        inventory.OnInventoryChanged += UpdateInventoryUI;
        inventory.OnSelectedItemChanged += UpdateSelectedSlot;

        InitializeSlots();
        UpdateInventoryUI();
    }

    void InitializeSlots()
    {
        // Clear existing slots
        foreach (Transform child in slotGrid.transform)
        {
            Destroy(child.gameObject);
        }
        slots.Clear();

        // Create new slots
        for (int i = 0; i < 9; i++) // 9 slots for hotbar
        {
            GameObject slot = Instantiate(slotPrefab, slotGrid.transform);
            slots.Add(slot);

            var canvasGroup = slot.GetComponent<CanvasGroup>();
            var trigger = slot.AddComponent<EventTrigger>();
            
            AddEventTrigger(trigger, EventTriggerType.PointerClick, OnSlotClick);
            AddEventTrigger(trigger, EventTriggerType.BeginDrag, BeginDrag);
            AddEventTrigger(trigger, EventTriggerType.Drag, Drag);
            AddEventTrigger(trigger, EventTriggerType.EndDrag, EndDrag);
            AddEventTrigger(trigger, EventTriggerType.PointerEnter, OnSlotEnter);
            AddEventTrigger(trigger, EventTriggerType.PointerExit, OnSlotExit);
        }
    }

    void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        var entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

    void OnSlotClick(BaseEventData data)
    {
        var pointerData = (PointerEventData)data;
        int slotIndex = GetSlotIndex(pointerData.pointerPress);
        SelectSlot(slotIndex);
    }

    private void SelectSlot(int index)
    {
        // Just update the index and refresh
        selectedSlotIndex = index;
        inventory.SelectItem(index);
        
        // Refresh the affected slots
        if (selectedSlotIndex >= 0) UpdateSlot(selectedSlotIndex);
        UpdateInventoryUI();
    }

    public void OnSlotEnter(BaseEventData data)
    {
        var pointerData = (PointerEventData)data;
        int slotIndex = GetSlotIndex(pointerData.pointerCurrentRaycast.gameObject);
        
        int oldHoveredIndex = hoveredSlotIndex;
        hoveredSlotIndex = slotIndex;
        
        // Update affected slots
        if (oldHoveredIndex >= 0) UpdateSlot(oldHoveredIndex);
        if (hoveredSlotIndex >= 0) UpdateSlot(hoveredSlotIndex);
    }

    public void OnSlotExit(BaseEventData data)
    {
        int oldHoveredIndex = hoveredSlotIndex;
        hoveredSlotIndex = -1;
        
        // Update the slot that was previously hovered
        if (oldHoveredIndex >= 0) UpdateSlot(oldHoveredIndex);
    }

    private void UpdateSlot(int index)
    {
        if (index < 0 || index >= slots.Count) return;
        
        var slot = slots[index];
        var iconTransform = slot.transform.Find("Icon");
        var countTransform = slot.transform.Find("Count");
        var slotImage = slot.GetComponent<Image>();
        var iconImage = iconTransform?.GetComponent<Image>();
        var countText = countTransform?.GetComponent<TMP_Text>();
        
        bool hasItem = false;
        bool isSellable = false;
        
        // Update item display
        if (index < inventory.items.Count && inventory.items[index]?.item != null)
        {
            var itemStack = inventory.items[index];
            hasItem = true;
            
            // Check if this item is sellable
            isSellable = isInShopRange && itemStack.item.isSellable;
            
            // Update icon and count
            iconImage.sprite = itemStack.item.itemIcon;
            iconImage.color = Color.white;
            countText.text = itemStack.stackSize.ToString();
        }
        else
        {
            ClearSlot(iconImage, countText);
        }
        
        // Determine slot color based on state
        Color slotColor = normalColor;
        
        if (hasItem)
        {
            bool isSelected = (index == selectedSlotIndex);
            bool isHovered = (index == hoveredSlotIndex);
            
            if (isSellable)
            {
                // Sellable item states
                if (isSelected && isHovered) {
                    slotColor = sellableSelectedColor; // Selected + hovered sellable
                }
                else if (isSelected) {
                    slotColor = sellableSelectedColor; // Selected sellable
                }
                else if (isHovered) {
                    slotColor = sellableHoverColor; // Hovered sellable
                }
                else {
                    slotColor = sellableColor; // Normal sellable
                }
            }
            else
            {
                // Regular item states
                if (isSelected && isHovered) {
                    slotColor = selectedColor; // Selected + hovered
                }
                else if (isSelected) {
                    slotColor = selectedColor; // Selected
                }
                else if (isHovered) {
                    slotColor = hoverColor; // Hovered
                }
                // else: normal color
            }
        }
        else if (index == hoveredSlotIndex)
        {
            slotColor = hoverColor; // Empty slot, but hovered
        }
        
        // Apply the color
        slotImage.color = slotColor;
    }

    public void BeginDrag(BaseEventData data)
    {
        var pointerData = (PointerEventData)data;
        draggedItemIndex = GetSlotIndex(pointerData.pointerPress);
        Debug.Log($"BeginDrag: Index = {draggedItemIndex}");
        
        if (IsValidDragIndex())
        {
            // Clear selection when starting drag
            if (selectedSlotIndex >= 0 && selectedSlotIndex < slots.Count)
            {
                var selectedSlotImage = slots[selectedSlotIndex].GetComponent<Image>();
                selectedSlotImage.color = Color.white;
            }
            selectedSlotIndex = -1;
            inventory.SelectItem(-1);

            draggedItem = CreateDraggedItem(draggedItemIndex);
            if (draggedItem != null)
            {
                var canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    draggedItem.transform.SetParent(canvas.transform, false);
                    draggedItem.transform.SetAsLastSibling();
                    Debug.Log("Dragged item created and parented to canvas");
                }
                else
                {
                    Debug.LogError("Canvas not found!");
                }
            slots[draggedItemIndex].GetComponent<CanvasGroup>().blocksRaycasts = false;
            }
            else
            {
                Debug.LogError("Failed to create dragged item!");
            }
        }
        else
        {
            Debug.LogError($"Invalid drag index: {draggedItemIndex}");
        }
    }

    private bool IsValidDragIndex() => draggedItemIndex >= 0 && draggedItemIndex < inventory.items.Count;

    GameObject CreateDraggedItem(int index)
    {
        if (draggedItemPrefab == null)
        {
            Debug.LogError("DraggedItemPrefab is not assigned!");
            return null;
        }

        var itemObject = Instantiate(draggedItemPrefab);
        var itemImage = itemObject.GetComponent<Image>();
        if (itemImage == null)
        {
            Debug.LogError("DraggedItemPrefab does not have an Image component!");
            Destroy(itemObject);
            return null;
        }

        if (inventory.items[index]?.item?.itemIcon != null)
        {
        itemImage.sprite = inventory.items[index].item.itemIcon;
            itemImage.color = Color.white;
            Debug.Log("Dragged item sprite set successfully");
        }
        else
        {
            Debug.LogError($"No item icon found at index {index}");
        }

        var rectTransform = itemObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(50, 50);
        rectTransform.anchoredPosition = Vector2.zero;

        return itemObject;
    }

    public void Drag(BaseEventData data)
    {
        if (draggedItem == null)
        {
            Debug.LogError("Dragged item is null during drag!");
            return;
        }

        var pointerData = (PointerEventData)data;
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found during drag!");
            return;
        }

        var canvasRectTransform = canvas.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, pointerData.position, canvas.worldCamera, out Vector2 localPoint);

        var draggedItemRectTransform = draggedItem.GetComponent<RectTransform>();
        draggedItemRectTransform.anchoredPosition = localPoint;
    }

    public void EndDrag(BaseEventData data)
    {
        if (draggedItem == null) return;

        var pointerData = (PointerEventData)data;
        int targetSlotIndex = GetTargetSlotIndex(pointerData.position);

        HandleDropIntoChest(pointerData.position);

        if (RectTransformUtility.RectangleContainsScreenPoint(trashCanImage.rectTransform, pointerData.position))
        {
            RemoveDraggedItem();
        }
        else if (targetSlotIndex >= 0)
        {
            SwapItems(targetSlotIndex);
        }

        if (draggedItemIndex >= 0 && draggedItemIndex < slots.Count)
        {
        slots[draggedItemIndex].GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
        
        Destroy(draggedItem);
        CleanUpDrag();
    }

private void HandleDropIntoChest(Vector2 position)
{
        var chestInventoryUI = FindObjectOfType<ChestInventoryUI>();
        if (chestInventoryUI == null) return;

        int chestSlotIndex = chestInventoryUI.GetTargetSlotIndex(position);
        if (chestSlotIndex >= 0)
        {
            var draggedItemStack = inventory.items[draggedItemIndex];
            inventory.RemoveItem(draggedItemStack.item, draggedItemStack.stackSize);
            chestInventoryUI.chestInventory.AddItem(draggedItemStack.item, draggedItemStack.stackSize, chestSlotIndex);
            chestInventoryUI.UpdateChestInventoryUI();
    }
}

    private void RemoveDraggedItem()
    {
        if (!IsValidDragIndex()) return;

            var itemToRemove = inventory.items[draggedItemIndex];
            if (itemToRemove != null)
            {
                inventory.RemoveItem(itemToRemove.item, itemToRemove.stackSize);
                Debug.Log($"Item dropped in trash can and removed: {itemToRemove.item.itemName}");
        }
    }

    private void SwapItems(int targetSlotIndex)
    {
        // Ensure the inventory list has enough slots
        while (inventory.items.Count <= targetSlotIndex)
        {
            inventory.items.Add(new Inventory.ItemStack { item = null, stackSize = 0 });
        }

        if (!IsValidDragIndex()) return;

        var draggedItemStack = inventory.items[draggedItemIndex];
        var targetItemStack = inventory.items[targetSlotIndex];

        // If target slot is empty, just move the item
        if (targetItemStack.item == null)
        {
            inventory.items[targetSlotIndex] = draggedItemStack;
            inventory.items[draggedItemIndex] = new Inventory.ItemStack { item = null, stackSize = 0 };
        }
        // If target slot has the same item type and isn't full, try to stack
        else if (targetItemStack.item.itemName == draggedItemStack.item.itemName && 
                 targetItemStack.stackSize < targetItemStack.item.maxStack)
        {
            int availableSpace = targetItemStack.item.maxStack - targetItemStack.stackSize;
            int toAdd = Mathf.Min(draggedItemStack.stackSize, availableSpace);
            
            targetItemStack.stackSize += toAdd;
            draggedItemStack.stackSize -= toAdd;

            inventory.items[draggedItemIndex] = draggedItemStack.stackSize <= 0 ? 
                new Inventory.ItemStack { item = null, stackSize = 0 } : draggedItemStack;
        }
        // Otherwise, swap the items
        else
        {
            inventory.items[draggedItemIndex] = targetItemStack;
            inventory.items[targetSlotIndex] = draggedItemStack;
        }

        UpdateInventoryUI();
        Debug.Log($"Item moved to slot: {targetSlotIndex}");
    }

    private void CleanUpDrag()
    {
        draggedItem = null;
        draggedItemIndex = -1;
    }

    int GetSlotIndex(GameObject slot) => slots.IndexOf(slot);

    public int GetTargetSlotIndex(Vector2 position)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            var slotRectTransform = slots[i].GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(slotRectTransform, position))
            {
                return i;
            }
        }
        return -1;
    }

    public void UpdateInventoryUI()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            UpdateSlot(i);
        }
    }

    private void ClearSlot(Image iconImage, TMP_Text countText)
    {
        iconImage.sprite = null;
        iconImage.color = new Color(1, 1, 1, 0); // Set alpha to 0 to make it fully transparent
        countText.text = "";
    }

    private void UpdateSelectedSlot(Item selectedItem)
    {
        // Find the slot containing the selected item
        int newSelectedIndex = -1;
        for (int i = 0; i < inventory.items.Count; i++)
        {
            if (inventory.items[i]?.item == selectedItem)
            {
                newSelectedIndex = i;
                break;
            }
        }

        // Update the selected slot index
        selectedSlotIndex = newSelectedIndex;
        
        // Refresh the entire UI to ensure proper coloring
        UpdateInventoryUI();
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= UpdateInventoryUI;
            inventory.OnSelectedItemChanged -= UpdateSelectedSlot;
        }
    }

    void Update()
    {
        // Handle right-click selling
        if (isInShopRange && Input.GetMouseButtonDown(1)) // Right click
        {
            var mousePos = Input.mousePosition;
            int slotIndex = GetTargetSlotIndex(mousePos);
            
            if (slotIndex >= 0 && slotIndex < inventory.items.Count)
            {
                var itemStack = inventory.items[slotIndex];
                if (itemStack?.item != null && itemStack.item.isSellable)
                {
                    var shopUI = FindObjectOfType<ShopUI>();
                    if (shopUI != null)
                    {
                        shopUI.ShowSellConfirmation(itemStack.item);
                    }
                }
            }
        }
    }

    public void SetInShopRange(bool inRange)
    {
        if (isInShopRange != inRange)
        {
            isInShopRange = inRange;
            UpdateInventoryUI(); // Refresh all slots when shop status changes
            Debug.Log("Shop range set to: " + inRange);
        }
    }
}

