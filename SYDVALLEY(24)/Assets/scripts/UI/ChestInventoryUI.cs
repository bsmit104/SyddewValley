using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ChestInventoryUI : MonoBehaviour
{
    [SerializeField] public ChestInventory chestInventory;
    [SerializeField] private GridLayoutGroup slotGrid;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject draggedItemPrefab;
    [SerializeField] private Color selectedSlotColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    [SerializeField] private Color hoverSlotColor = new Color(0.9f, 0.9f, 0.9f, 1f);

    private readonly List<GameObject> slots = new List<GameObject>();
    private GameObject draggedItem;
    private int draggedItemIndex = -1;
    private int selectedSlotIndex = -1;
    private int hoveredSlotIndex = -1;

    void Start()
    {
        if (slotGrid == null)
        {
            Debug.LogError("Slot Grid is not assigned!");
            return;
        }

        InitializeSlots();
        UpdateChestInventoryUI();
    }

    void OnEnable()
    {
        if (chestInventory != null)
        {
            chestInventory.OnChestInventoryChanged += UpdateChestInventoryUI;
        }
    }

    void OnDisable()
    {
        if (chestInventory != null)
        {
            chestInventory.OnChestInventoryChanged -= UpdateChestInventoryUI;
        }
    }

    void InitializeSlots()
    {
        // Clear existing slots
        foreach (Transform child in slotGrid.transform)
        {
            Destroy(child.gameObject);
        }
        slots.Clear();

        // Create new slots (6x6 grid for chest)
        for (int i = 0; i < 44; i++)
        {
            GameObject slot = Instantiate(slotPrefab, slotGrid.transform);
            slots.Add(slot);

            var canvasGroup = slot.GetComponent<CanvasGroup>();
            var trigger = slot.AddComponent<EventTrigger>();
            
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

    public void BeginDrag(BaseEventData data)
    {
        var pointerData = (PointerEventData)data;
        draggedItemIndex = GetSlotIndex(pointerData.pointerPress);
        
        if (IsValidDragIndex())
        {
            draggedItem = CreateDraggedItem(draggedItemIndex);
            if (draggedItem != null)
            {
                draggedItem.transform.SetParent(transform.root, false);
                draggedItem.transform.SetAsLastSibling();
                slots[draggedItemIndex].GetComponent<CanvasGroup>().blocksRaycasts = false;
            }
        }
    }

    private bool IsValidDragIndex() => draggedItemIndex >= 0 && draggedItemIndex < chestInventory.items.Count;

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

        itemImage.sprite = chestInventory.items[index].item.itemIcon;
        itemImage.color = Color.white;

        var rectTransform = itemObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(50, 50);

        return itemObject;
    }

    public void Drag(BaseEventData data)
    {
        if (draggedItem == null) return;

        var pointerData = (PointerEventData)data;
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

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

        HandleDropIntoPlayerInventory(pointerData.position);

        if (targetSlotIndex >= 0)
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

    private void HandleDropIntoPlayerInventory(Vector2 position)
    {
        var playerInventoryUI = FindObjectOfType<InventoryUI>();
        if (playerInventoryUI == null) return;

        int playerSlotIndex = playerInventoryUI.GetTargetSlotIndex(position);
        if (playerSlotIndex >= 0)
        {
            var draggedItemStack = chestInventory.items[draggedItemIndex];
            chestInventory.RemoveItem(draggedItemStack.item, draggedItemStack.stackSize);
            playerInventoryUI.inventory.AddItem(draggedItemStack.item, draggedItemStack.stackSize);
            playerInventoryUI.UpdateInventoryUI();
        }
    }

    private void SwapItems(int targetSlotIndex)
    {
        // Ensure the chest inventory list has enough slots
        while (chestInventory.items.Count <= targetSlotIndex)
        {
            chestInventory.items.Add(new Inventory.ItemStack { item = null, stackSize = 0 });
        }

        if (!IsValidDragIndex()) return;

        var draggedItemStack = chestInventory.items[draggedItemIndex];
        var targetItemStack = chestInventory.items[targetSlotIndex];

        // If target slot is empty, just move the item
        if (targetItemStack.item == null)
        {
            chestInventory.items[targetSlotIndex] = draggedItemStack;
            chestInventory.items[draggedItemIndex] = new Inventory.ItemStack { item = null, stackSize = 0 };
        }
        // If target slot has the same item type and isn't full, try to stack
        else if (targetItemStack.item.itemName == draggedItemStack.item.itemName && 
                 targetItemStack.stackSize < targetItemStack.item.maxStack)
        {
            int availableSpace = targetItemStack.item.maxStack - targetItemStack.stackSize;
            int toAdd = Mathf.Min(draggedItemStack.stackSize, availableSpace);
            
            targetItemStack.stackSize += toAdd;
            draggedItemStack.stackSize -= toAdd;

            chestInventory.items[draggedItemIndex] = draggedItemStack.stackSize <= 0 ? 
                new Inventory.ItemStack { item = null, stackSize = 0 } : draggedItemStack;
        }
        // Otherwise, swap the items
        else
        {
            chestInventory.items[draggedItemIndex] = targetItemStack;
            chestInventory.items[targetSlotIndex] = draggedItemStack;
        }

        UpdateChestInventoryUI();
        Debug.Log($"Item moved to chest slot: {targetSlotIndex}");
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

    public void UpdateChestInventoryUI()
    {
        // Clear all slots if there's no chest inventory
        if (chestInventory == null)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                var iconTransform = slots[i].transform.Find("Icon");
                var countTransform = slots[i].transform.Find("Count");
                var slotImage = slots[i].GetComponent<Image>();

                var iconImage = iconTransform?.GetComponent<Image>();
                var countText = countTransform?.GetComponent<TMP_Text>();

                ClearSlot(iconImage, countText);
                slotImage.color = Color.white;
            }
            return;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            var iconTransform = slots[i].transform.Find("Icon");
            var countTransform = slots[i].transform.Find("Count");
            var slotImage = slots[i].GetComponent<Image>();

            var iconImage = iconTransform?.GetComponent<Image>();
            var countText = countTransform?.GetComponent<TMP_Text>();

            if (i < chestInventory.items.Count)
            {
                var itemStack = chestInventory.items[i];
                if (itemStack != null && itemStack.item != null)
                {
                    iconImage.sprite = itemStack.item.itemIcon;
                    iconImage.color = Color.white;
                    countText.text = itemStack.stackSize.ToString();
                }
                else
                {
                    ClearSlot(iconImage, countText);
                }
            }
            else
            {
                ClearSlot(iconImage, countText);
            }

            // Update slot color based on selection and hover states
            if (i == selectedSlotIndex)
            {
                slotImage.color = selectedSlotColor;
            }
            else if (i == hoveredSlotIndex)
            {
                slotImage.color = hoverSlotColor;
            }
            else
            {
                slotImage.color = Color.white;
            }
        }
    }

    private void ClearSlot(Image iconImage, TMP_Text countText)
    {
        iconImage.sprite = null;
        countText.text = "";
    }

    public void OnSlotEnter(BaseEventData data)
    {
        var pointerData = (PointerEventData)data;
        int slotIndex = GetSlotIndex(pointerData.pointerCurrentRaycast.gameObject);
        if (slotIndex != hoveredSlotIndex)
        {
            if (hoveredSlotIndex >= 0 && hoveredSlotIndex < slots.Count)
            {
                var oldSlotImage = slots[hoveredSlotIndex].GetComponent<Image>();
                oldSlotImage.color = hoveredSlotIndex == selectedSlotIndex ? selectedSlotColor : Color.white;
            }

            hoveredSlotIndex = slotIndex;
            if (hoveredSlotIndex >= 0 && hoveredSlotIndex < slots.Count)
            {
                var newSlotImage = slots[hoveredSlotIndex].GetComponent<Image>();
                newSlotImage.color = hoverSlotColor;
            }
        }
    }

    public void OnSlotExit(BaseEventData data)
    {
        if (hoveredSlotIndex >= 0 && hoveredSlotIndex < slots.Count)
        {
            var slotImage = slots[hoveredSlotIndex].GetComponent<Image>();
            slotImage.color = hoveredSlotIndex == selectedSlotIndex ? selectedSlotColor : Color.white;
        }
        hoveredSlotIndex = -1;
    }

    private void OnDestroy()
    {
        if (chestInventory != null)
        {
            chestInventory.OnChestInventoryChanged -= UpdateChestInventoryUI;
        }
    }
}
