using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ChestInventoryUI : MonoBehaviour
{
    [SerializeField]
    private ChestInventory chestInventory;
    public Transform slotPanel;
    public Image trashCanImage;
    public GameObject draggedItemPrefab;

    private List<GameObject> slots = new List<GameObject>();
    private GameObject draggedItem;
    private int draggedItemIndex = -1;
    private int targetSlotIndex = -1;

    void Start()
    {
        if (chestInventory == null)
        {
            Debug.LogError("ChestInventory is not assigned!");
            return;
        }

        chestInventory.OnInventoryChanged += UpdateInventoryUI;
        InitializeSlots();
        UpdateInventoryUI();
    }

    void InitializeSlots()
    {
        for (int i = 0; i < slotPanel.childCount; i++)
        {
            GameObject slot = slotPanel.GetChild(i).gameObject;
            slots.Add(slot);

            if (slot.GetComponent<CanvasGroup>() == null)
            {
                slot.AddComponent<CanvasGroup>();
            }

            EventTrigger trigger = slot.AddComponent<EventTrigger>();
            AddEventTrigger(trigger, EventTriggerType.PointerClick, OnSlotClick);
            AddEventTrigger(trigger, EventTriggerType.BeginDrag, BeginDrag);
            AddEventTrigger(trigger, EventTriggerType.Drag, Drag);
            AddEventTrigger(trigger, EventTriggerType.EndDrag, EndDrag);
        }
    }

    void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

    void OnSlotClick(BaseEventData data)
    {
        // Handle slot click if needed
    }

    public void BeginDrag(BaseEventData data)
    {
        PointerEventData pointerData = (PointerEventData)data;
        draggedItemIndex = GetSlotIndex(pointerData.pointerPress);
        if (draggedItemIndex >= 0 && draggedItemIndex < chestInventory.items.Count)
        {
            draggedItem = CreateDraggedItem(draggedItemIndex);
            slots[draggedItemIndex].GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }

    GameObject CreateDraggedItem(int index)
    {
        GameObject itemObject = Instantiate(draggedItemPrefab);
        Image itemImage = itemObject.GetComponent<Image>();
        itemImage.sprite = chestInventory.items[index].item.itemIcon;

        RectTransform rectTransform = itemObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(50, 50);
        rectTransform.anchoredPosition = Vector2.zero;

        Canvas canvas = FindObjectOfType<Canvas>();
        itemObject.transform.SetParent(canvas.transform, false);

        return itemObject;
    }

    public void Drag(BaseEventData data)
    {
        if (draggedItem != null)
        {
            PointerEventData pointerData = (PointerEventData)data;
            Canvas canvas = FindObjectOfType<Canvas>();
            RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, pointerData.position, canvas.worldCamera, out localPoint);

            RectTransform draggedItemRectTransform = draggedItem.GetComponent<RectTransform>();
            draggedItemRectTransform.anchoredPosition = localPoint;
        }
    }

    public void EndDrag(BaseEventData data)
    {
        if (draggedItem != null)
        {
            PointerEventData pointerData = (PointerEventData)data;

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(slotPanel as RectTransform, pointerData.position, null, out localPoint);

            targetSlotIndex = -1;
            for (int i = 0; i < slots.Count; i++)
            {
                RectTransform slotRectTransform = slots[i].GetComponent<RectTransform>();
                if (RectTransformUtility.RectangleContainsScreenPoint(slotRectTransform, pointerData.position))
                {
                    targetSlotIndex = i;
                    break;
                }
            }

            if (RectTransformUtility.RectangleContainsScreenPoint(trashCanImage.rectTransform, pointerData.position))
            {
                if (draggedItemIndex >= 0 && draggedItemIndex < chestInventory.items.Count)
                {
                    var itemToRemove = chestInventory.items[draggedItemIndex];
                    if (itemToRemove != null)
                    {
                        chestInventory.RemoveItem(itemToRemove.item, itemToRemove.stackSize);
                    }
                }
            }
            else if (targetSlotIndex >= 0 && targetSlotIndex < slots.Count)
            {
                while (chestInventory.items.Count <= targetSlotIndex)
                {
                    chestInventory.items.Add(new Inventory.ItemStack { item = null, stackSize = 0 });
                }

                if (draggedItemIndex >= 0 && draggedItemIndex < chestInventory.items.Count)
                {
                    Inventory.ItemStack draggedItemStack = chestInventory.items[draggedItemIndex];
                    Inventory.ItemStack targetItemStack = chestInventory.items[targetSlotIndex];
                    chestInventory.items[draggedItemIndex] = targetItemStack;
                    chestInventory.items[targetSlotIndex] = draggedItemStack;

                    UpdateInventoryUI();
                }
            }
            else
            {
                Debug.Log("Returning item to original slot: " + draggedItemIndex);
                if (draggedItemIndex >= 0 && draggedItemIndex < slots.Count)
                {
                    // Return item to the original slot
                    if (chestInventory.items.Count <= draggedItemIndex)
                    {
                        chestInventory.items.Add(new Inventory.ItemStack { item = null, stackSize = 0 });
                    }

                    Inventory.ItemStack draggedItemStack = chestInventory.items[draggedItemIndex];
                    if (draggedItemStack.item == null)
                    {
                        // Restore the original item stack to the slot
                        draggedItemStack.item = chestInventory.items[draggedItemIndex].item;
                        draggedItemStack.stackSize = chestInventory.items[draggedItemIndex].stackSize;
                    }
                    else
                    {
                        // Slot was not empty, return the dragged item
                        Inventory.ItemStack originalItemStack = chestInventory.items[draggedItemIndex];
                        chestInventory.items[draggedItemIndex] = new Inventory.ItemStack { item = draggedItemStack.item, stackSize = draggedItemStack.stackSize };
                        draggedItemStack.item = originalItemStack.item;
                        draggedItemStack.stackSize = originalItemStack.stackSize;
                    }

                    UpdateInventoryUI();
                }
            }

            if (draggedItemIndex >= 0 && draggedItemIndex < slots.Count)
            {
                slots[draggedItemIndex].GetComponent<CanvasGroup>().blocksRaycasts = true;
            }

            Destroy(draggedItem);
            CleanUpDrag();
        }
    }


    private void CleanUpDrag()
    {
        draggedItem = null;
        draggedItemIndex = -1;
        targetSlotIndex = -1;
    }

    int GetSlotIndex(GameObject slot)
    {
        return slots.IndexOf(slot);
    }

    void UpdateInventoryUI()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            Transform iconTransform = slots[i].transform.Find("Icon");
            Transform countTransform = slots[i].transform.Find("Count");

            Image iconImage = iconTransform?.GetComponent<Image>();
            TMP_Text countText = countTransform?.GetComponent<TMP_Text>();

            if (i < chestInventory.items.Count)
            {
                if (chestInventory.items[i] != null && chestInventory.items[i].item != null)
                {
                    if (iconImage != null)
                    {
                        iconImage.sprite = chestInventory.items[i].item.itemIcon;
                        iconImage.color = Color.white;
                    }
                    if (countText != null)
                    {
                        countText.text = chestInventory.items[i].stackSize.ToString();
                    }
                }
                else
                {
                    if (iconImage != null) iconImage.sprite = null;
                    if (countText != null) countText.text = "";
                }
            }
            else
            {
                if (iconImage != null) iconImage.sprite = null;
                if (countText != null) countText.text = "";
            }
        }
    }

    private void OnDestroy()
    {
        if (chestInventory != null)
        {
            chestInventory.OnInventoryChanged -= UpdateInventoryUI;
        }
    }
}
