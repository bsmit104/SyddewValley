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

    [System.Obsolete]
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

    [System.Obsolete]
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

            // Get the Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();

            // Convert screen point to local point
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, pointerData.position, canvas.worldCamera, out localPoint);

            // Set the position of the dragged item
            RectTransform draggedItemRectTransform = draggedItem.GetComponent<RectTransform>();
            draggedItemRectTransform.anchoredPosition = localPoint;
        }
    }

    [System.Obsolete]
    public void EndDrag(BaseEventData data)
    {
        if (draggedItem != null)
        {
            slots[draggedItemIndex].GetComponent<CanvasGroup>().blocksRaycasts = true;

            if (IsPointerOverUIObject(trashCanImage.gameObject))
            {
                Debug.Log("Item dropped on trash can.");
                if (draggedItemIndex >= 0 && draggedItemIndex < inventory.items.Count)
                {
                    inventory.RemoveItem(inventory.items[draggedItemIndex].item, 1);  // Always remove one item
                    inventory.SelectItem(-1); // Deselect the item after removing it
                }
            }

            Destroy(draggedItem);
            draggedItem = null;
            draggedItemIndex = -1;

            UpdateInventoryUI();
        }
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

    public void UpdateInventoryUI()
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
    }

    public void UpdateSelectedItemDisplay(Item selectedItem)
    {
        // Destroy the previous selectedItemDisplay if it exists
        if (selectedItemDisplay != null)
        {
            Destroy(selectedItemDisplay);
            selectedItemDisplay = null;
        }

        // Check if a new item is selected
        if (selectedItem != null)
        {
            // Instantiate a new selectedItemDisplay
            selectedItemDisplay = Instantiate(selectedItemDisplayPrefab);
            Image itemImage = selectedItemDisplay.GetComponent<Image>();
            if (itemImage != null)
            {
                itemImage.sprite = selectedItem.itemIcon;
            }

            // Set the parent to the player's head transform
            selectedItemDisplay.transform.SetParent(playerHeadTransform);

            // Set position relative to the player's head
            selectedItemDisplay.transform.localPosition = new Vector3(0, 2, 0); // Adjust position above the player's head

            // Ensure the item is positioned in world space
            selectedItemDisplay.transform.localPosition = Vector3.zero; // Reset local position
            selectedItemDisplay.transform.localRotation = Quaternion.identity; // Reset local rotation
            selectedItemDisplay.transform.localScale = Vector3.one; // Reset local scale
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







// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.EventSystems;
// using TMPro;

// public class InventoryUI : MonoBehaviour
// {
//     [SerializeField]
//     private Inventory inventory;
//     public Transform slotPanel;
//     public Image trashCanImage; // Assign your trash can image in the editor
//     public Transform playerHeadTransform; // Assign the player's head transform in the editor
//     public GameObject selectedItemDisplayPrefab; // Prefab for displaying selected item above the player's head
//     public GameObject draggedItemPrefab; // Prefab for displaying dragged item

//     private List<GameObject> slots = new List<GameObject>();
//     private GameObject selectedItemDisplay;
//     private GameObject draggedItem;
//     private int draggedItemIndex = -1;

//     [System.Obsolete]
//     void Start()
//     {
//         Debug.Log("Initializing inventory UI...");

//         if (inventory == null)
//         {
//             Debug.LogError("Inventory is not assigned!");
//             return;
//         }

//         inventory.OnInventoryChanged += UpdateInventoryUI;
//         inventory.OnSelectedItemChanged += UpdateSelectedItemDisplay;

//         InitializeSlots();
//         UpdateInventoryUI();
//     }

//     [System.Obsolete]
//     void InitializeSlots()
//     {
//         Debug.Log("Initializing slots...");
//         for (int i = 0; i < slotPanel.childCount; i++)
//         {
//             GameObject slot = slotPanel.GetChild(i).gameObject;
//             slots.Add(slot);

//             if (slot.GetComponent<CanvasGroup>() == null)
//             {
//                 slot.AddComponent<CanvasGroup>();
//             }

//             EventTrigger trigger = slot.AddComponent<EventTrigger>();
//             AddEventTrigger(trigger, EventTriggerType.PointerClick, OnSlotClick);
//             AddEventTrigger(trigger, EventTriggerType.BeginDrag, BeginDrag);
//             AddEventTrigger(trigger, EventTriggerType.Drag, Drag);
//             AddEventTrigger(trigger, EventTriggerType.EndDrag, EndDrag);
//         }
//         Debug.Log("Slots initialized. Total slots: " + slots.Count);
//     }

//     void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> action)
//     {
//         EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
//         entry.callback.AddListener(action);
//         trigger.triggers.Add(entry);
//     }

//     void OnSlotClick(BaseEventData data)
//     {
//         PointerEventData pointerData = (PointerEventData)data;
//         int slotIndex = GetSlotIndex(pointerData.pointerPress);
//         inventory.SelectItem(slotIndex);
//     }

//     public void BeginDrag(BaseEventData data)
//     {
//         PointerEventData pointerData = (PointerEventData)data;
//         draggedItemIndex = GetSlotIndex(pointerData.pointerPress);
//         if (draggedItemIndex >= 0 && draggedItemIndex < inventory.items.Count)
//         {
//             draggedItem = CreateDraggedItem(draggedItemIndex);
//             slots[draggedItemIndex].GetComponent<CanvasGroup>().blocksRaycasts = false;
//         }
//     }

//     GameObject CreateDraggedItem(int index)
//     {
//         Debug.Log("Creating dragged item...");
//         GameObject itemObject = Instantiate(draggedItemPrefab);
//         Image itemImage = itemObject.GetComponent<Image>();
//         itemImage.sprite = inventory.items[index].item.itemIcon;
//         itemImage.SetNativeSize();
//         Canvas canvas = FindFirstObjectByType<Canvas>();
//         itemObject.transform.SetParent(canvas.transform, false);
//         itemObject.transform.SetAsLastSibling();
//         return itemObject;
//     }

//     public void Drag(BaseEventData data)
//     {
//         if (draggedItem != null)
//         {
//             PointerEventData pointerData = (PointerEventData)data;
//             Vector3 newPosition = Camera.main.ScreenToWorldPoint(pointerData.position);
//             newPosition.z = 0; // Assuming your dragged items should be on the same z-plane
//             draggedItem.transform.position = newPosition;
//         }
//     }

//     [System.Obsolete]
//     public void EndDrag(BaseEventData data)
//     {
//         if (draggedItem != null)
//         {
//             slots[draggedItemIndex].GetComponent<CanvasGroup>().blocksRaycasts = true;

//             if (IsPointerOverUIObject(trashCanImage.gameObject))
//             {
//                 Debug.Log("Item dropped on trash can.");
//                 if (draggedItemIndex >= 0 && draggedItemIndex < inventory.items.Count)
//                 {
//                     inventory.RemoveItem(inventory.items[draggedItemIndex].item, 1);  // Always remove one item
//                     inventory.SelectItem(-1); // Deselect the item after removing it
//                 }
//             }

//             Destroy(draggedItem);
//             draggedItem = null;
//             draggedItemIndex = -1;

//             UpdateInventoryUI();
//         }
//     }

//     int GetSlotIndex(GameObject slot)
//     {
//         return slots.IndexOf(slot);
//     }

//     bool IsPointerOverUIObject(GameObject uiObject)
//     {
//         PointerEventData pointerEventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
//         List<RaycastResult> results = new List<RaycastResult>();
//         EventSystem.current.RaycastAll(pointerEventData, results);
//         return results.Exists(result => result.gameObject == uiObject);
//     }

//     public void UpdateInventoryUI()
//     {
//         Debug.Log("Updating inventory UI...");
//         for (int i = 0; i < slots.Count; i++)
//         {
//             Transform iconTransform = slots[i].transform.Find("Icon");
//             Transform countTransform = slots[i].transform.Find("Count");

//             Image iconImage = iconTransform?.GetComponent<Image>();
//             TMP_Text countText = countTransform?.GetComponent<TMP_Text>();

//             if (i < inventory.items.Count)
//             {
//                 if (iconImage != null)
//                 {
//                     iconImage.sprite = inventory.items[i].item.itemIcon;
//                     iconImage.color = Color.white;
//                 }
//                 if (countText != null)
//                 {
//                     countText.text = inventory.items[i].stackSize.ToString();
//                 }
//             }
//             else
//             {
//                 if (iconImage != null) iconImage.sprite = null;
//                 if (countText != null) countText.text = "";
//             }
//         }
//     }

//     public void UpdateSelectedItemDisplay(Item selectedItem)
//     {
//         // Destroy the previous selectedItemDisplay if it exists
//         if (selectedItemDisplay != null)
//         {
//             Destroy(selectedItemDisplay);
//             selectedItemDisplay = null;
//         }

//         // Check if a new item is selected
//         if (selectedItem != null)
//         {
//             // Instantiate a new selectedItemDisplay
//             selectedItemDisplay = Instantiate(selectedItemDisplayPrefab);
//             Image itemImage = selectedItemDisplay.GetComponent<Image>();
//             if (itemImage != null)
//             {
//                 itemImage.sprite = selectedItem.itemIcon;
//             }

//             // Set the parent to the player's head transform
//             selectedItemDisplay.transform.SetParent(playerHeadTransform);

//             // Set position relative to the player's head
//             selectedItemDisplay.transform.localPosition = new Vector3(0, 2, 0); // Adjust position above the player's head

//             // Ensure the item is positioned in world space
//             selectedItemDisplay.transform.localPosition = Vector3.zero; // Reset local position
//             selectedItemDisplay.transform.localRotation = Quaternion.identity; // Reset local rotation
//             selectedItemDisplay.transform.localScale = Vector3.one; // Reset local scale
//         }
//     }

//     private void OnDestroy()
//     {
//         if (inventory != null)
//         {
//             inventory.OnInventoryChanged -= UpdateInventoryUI;
//             inventory.OnSelectedItemChanged -= UpdateSelectedItemDisplay;
//         }
//     }
// }

