using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [SerializeField]
    private Inventory inventory;
    public Transform slotPanel;
    public Image trashCanImage; // Assign your trash can image in the editor
    public Transform playerHeadTransform; // Assign the player's head transform in the editor
    public GameObject selectedItemDisplayPrefab; // Prefab for displaying selected item above the player's head
    public GameObject draggedItemPrefab; // Prefab for displaying dragged item

    private List<GameObject> slots = new List<GameObject>();
    private GameObject selectedItemDisplay;
    private GameObject draggedItem;
    private int draggedItemIndex = -1;
    private int targetSlotIndex = -1;

    void Start()
    {
        Debug.Log("Initializing inventory UI...");

        if (inventory == null)
        {
            Debug.LogError("Inventory is not assigned!");
            return;
        }

        inventory.OnInventoryChanged += UpdateInventoryUI;
        inventory.OnSelectedItemChanged += UpdateSelectedItemDisplay;

        InitializeSlots();
        UpdateInventoryUI();
    }

    void InitializeSlots()
    {
        Debug.Log("Initializing slots...");
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
        Debug.Log("Slots initialized. Total slots: " + slots.Count);
    }

    void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

    void OnSlotClick(BaseEventData data)
    {
        PointerEventData pointerData = (PointerEventData)data;
        int slotIndex = GetSlotIndex(pointerData.pointerPress);
        inventory.SelectItem(slotIndex);
    }
    
    public void BeginDrag(BaseEventData data)
    {
        PointerEventData pointerData = (PointerEventData)data;
        draggedItemIndex = GetSlotIndex(pointerData.pointerPress);
        if (draggedItemIndex >= 0 && draggedItemIndex < inventory.items.Count)
        {
            Debug.Log("BeginDrag: " + draggedItemIndex);
            draggedItem = CreateDraggedItem(draggedItemIndex);
            slots[draggedItemIndex].GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }

    GameObject CreateDraggedItem(int index)
    {
        Debug.Log("Creating dragged item...");
        GameObject itemObject = Instantiate(draggedItemPrefab);
        Image itemImage = itemObject.GetComponent<Image>();
        itemImage.sprite = inventory.items[index].item.itemIcon;

        // Set size using RectTransform
        RectTransform rectTransform = itemObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(50, 50); // Set desired size here
        rectTransform.anchoredPosition = Vector2.zero; // Center the item initially

        // Set parent to Canvas
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
            if (canvas == null) return;

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

        // Check if item is dropped over the trash can
        if (RectTransformUtility.RectangleContainsScreenPoint(trashCanImage.rectTransform, pointerData.position))
        {
            if (draggedItemIndex >= 0 && draggedItemIndex < inventory.items.Count)
            {
                var itemToRemove = inventory.items[draggedItemIndex];
                if (itemToRemove != null)
                {
                    inventory.RemoveItem(itemToRemove.item, itemToRemove.stackSize);
                    Debug.Log("Item dropped in trash can and removed: " + itemToRemove.item.itemName);
                }
                else
                {
                    Debug.LogError("Item to remove is null.");
                }
            }
            else
            {
                Debug.LogError("Invalid draggedItemIndex: " + draggedItemIndex);
            }
        }
        else if (targetSlotIndex >= 0 && targetSlotIndex < slots.Count)
        {
            while (inventory.items.Count <= targetSlotIndex)
            {
                inventory.items.Add(new Inventory.ItemStack { item = null, stackSize = 0 });
            }

            if (draggedItemIndex >= 0 && draggedItemIndex < inventory.items.Count)
            {
                Debug.Log("Item dropped in slot: " + targetSlotIndex);

                Inventory.ItemStack draggedItemStack = inventory.items[draggedItemIndex];
                Inventory.ItemStack targetItemStack = inventory.items[targetSlotIndex];
                inventory.items[draggedItemIndex] = targetItemStack;
                inventory.items[targetSlotIndex] = draggedItemStack;

                UpdateInventoryUI();
            }
            else
            {
                Debug.LogError("Invalid draggedItemIndex: " + draggedItemIndex + " (out of bounds for inventory.items)");
            }
        }
        else
        {
            // Return item to original slot if not dropped in a valid slot or trash can
            Debug.Log("Returning item to original slot: " + draggedItemIndex);
        }

        if (draggedItemIndex >= 0 && draggedItemIndex < slots.Count)
        {
            slots[draggedItemIndex].GetComponent<CanvasGroup>().blocksRaycasts = true;
        }

        Destroy(draggedItem);
        CleanUpDrag();
    }
    else
    {
        Debug.LogError("Dragged item is null.");
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

    bool IsPointerOverUIObject(GameObject uiObject)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);
        return results.Exists(result => result.gameObject == uiObject);
    }

    void UpdateInventoryUI()
    {
        Debug.Log("Updating inventory UI...");
        for (int i = 0; i < slots.Count; i++)
        {
            Transform iconTransform = slots[i].transform.Find("Icon");
            Transform countTransform = slots[i].transform.Find("Count");

            Image iconImage = iconTransform?.GetComponent<Image>();
            TMP_Text countText = countTransform?.GetComponent<TMP_Text>();

            if (i < inventory.items.Count)
            {
                if (inventory.items[i] != null && inventory.items[i].item != null)
                {
                    if (iconImage != null)
                    {
                        iconImage.sprite = inventory.items[i].item.itemIcon;
                        iconImage.color = Color.white;
                    }
                    if (countText != null)
                    {
                        countText.text = inventory.items[i].stackSize.ToString();
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

    public void UpdateSelectedItemDisplay(Item selectedItem)
    {
        if (selectedItemDisplay != null)
        {
            Destroy(selectedItemDisplay);
            selectedItemDisplay = null;
        }

        if (selectedItem != null)
        {
            selectedItemDisplay = Instantiate(selectedItemDisplayPrefab);
            Image itemImage = selectedItemDisplay.GetComponent<Image>();
            if (itemImage != null)
            {
                itemImage.sprite = selectedItem.itemIcon;
            }

            selectedItemDisplay.transform.SetParent(playerHeadTransform);
            selectedItemDisplay.transform.localPosition = new Vector3(0, 2, 0); // Adjust position above the player's head

            selectedItemDisplay.transform.localPosition = Vector3.zero;
            selectedItemDisplay.transform.localRotation = Quaternion.identity;
            selectedItemDisplay.transform.localScale = Vector3.one;
        }
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= UpdateInventoryUI;
            inventory.OnSelectedItemChanged -= UpdateSelectedItemDisplay;
        }
    }
}

