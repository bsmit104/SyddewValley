using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Add this to use TMP_Text

public class InventoryUI : MonoBehaviour
{
    [SerializeField]
    private Inventory inventory;
    public GameObject slotPrefab; // Assign your prefab slot
    public Transform slotPanel; // Parent object in your UI where slots are housed

    private List<GameObject> slots = new List<GameObject>();

    void Start()
    {
        Debug.Log("Initializing inventory UI...");

        if (inventory == null)
        {
            Debug.LogError("Inventory is not assigned!");
            return;
        }

        // Subscribe to inventory changes
        inventory.OnInventoryChanged += UpdateInventoryUI;

        InitializeSlots();
        UpdateInventoryUI();
    }

    void InitializeSlots()
    {
        Debug.Log("Initializing slots...");
        for (int i = 0; i < inventory.items.Count; i++)
        {
            GameObject slot = Instantiate(slotPrefab, slotPanel);
            slots.Add(slot);
        }
        Debug.Log("Slots initialized. Total slots: " + slots.Count);
    }

    public void UpdateInventoryUI()
    {
        Debug.Log("Updating inventory UI...");
        Debug.Log("Current slots count: " + slots.Count);

        // Clear existing slots and re-initialize
        foreach (var slot in slots)
        {
            Destroy(slot);
        }
        slots.Clear();

        for (int i = 0; i < inventory.items.Count; i++)
        {
            Debug.Log("Creating slot for item: " + inventory.items[i].item.itemName);

            GameObject slot = Instantiate(slotPrefab, slotPanel);
            Transform iconTransform = slot.transform.Find("Icon");
            Transform countTransform = slot.transform.Find("Count");

            if (iconTransform == null)
            {
                Debug.LogError("Icon child object not found in slot prefab!");
            }
            if (countTransform == null)
            {
                Debug.LogError("Count child object not found in slot prefab!");
            }

            Image iconImage = iconTransform?.GetComponent<Image>();
            TMP_Text countText = countTransform?.GetComponent<TMP_Text>(); // Use TMP_Text instead of Text

            if (iconImage == null)
            {
                Debug.LogError("Icon Image component not found in slot prefab!");
            }
            if (countText == null)
            {
                Debug.LogError("Count TMP_Text component not found in slot prefab!");
            }

            if (iconImage != null)
            {
                iconImage.sprite = inventory.items[i].item.itemIcon;
            }
            if (countText != null)
            {
                countText.text = inventory.items[i].stackSize.ToString();
            }

            slots.Add(slot);
        }
        Debug.Log("Inventory UI updated. Total slots: " + slots.Count);
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event when the object is destroyed to avoid memory leaks
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= UpdateInventoryUI;
        }
    }
}



// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class InventoryUI : MonoBehaviour
// {
//     [SerializeField]
//     private Inventory inventory;
//     public GameObject slotPrefab; // Assign your prefab slot
//     public Transform slotPanel; // Parent object in your UI where slots are housed

//     private List<GameObject> slots = new List<GameObject>();

//     void Start()
//     {
//         Debug.Log("Initializing inventory UI...");

//         if (inventory == null)
//         {
//             Debug.LogError("Inventory is not assigned!");
//             return;
//         }

//         // Subscribe to inventory changes
//         inventory.OnInventoryChanged += UpdateInventoryUI;

//         InitializeSlots();
//         UpdateInventoryUI();
//     }

//     void InitializeSlots()
//     {
//         Debug.Log("Initializing slots...");
//         for (int i = 0; i < inventory.items.Count; i++)
//         {
//             GameObject slot = Instantiate(slotPrefab, slotPanel);
//             slots.Add(slot);
//         }
//         Debug.Log("Slots initialized. Total slots: " + slots.Count);
//     }

//     public void UpdateInventoryUI()
//     {
//         Debug.Log("Updating inventory UI...");
//         Debug.Log("Current slots count: " + slots.Count);

//         // Clear existing slots and re-initialize
//         foreach (var slot in slots)
//         {
//             Destroy(slot);
//         }
//         slots.Clear();

//         for (int i = 0; i < inventory.items.Count; i++)
//         {
//             Debug.Log("Creating slot for item: " + inventory.items[i].item.itemName);

//             GameObject slot = Instantiate(slotPrefab, slotPanel);
//             Image iconImage = slot.transform.Find("Icon")?.GetComponent<Image>();
//             Text countText = slot.transform.Find("Count")?.GetComponent<Text>();

//             if (iconImage == null)
//             {
//                 Debug.LogError("Icon Image component not found in slot prefab!");
//             }
//             if (countText == null)
//             {
//                 Debug.LogError("Count Text component not found in slot prefab!");
//             }

//             if (iconImage != null)
//             {
//                 iconImage.sprite = inventory.items[i].item.itemIcon;
//             }
//             if (countText != null)
//             {
//                 countText.text = inventory.items[i].stackSize.ToString();
//             }

//             slots.Add(slot);
//         }
//         Debug.Log("Inventory UI updated. Total slots: " + slots.Count);
//     }

//     private void OnDestroy()
//     {
//         // Unsubscribe from the event when the object is destroyed to avoid memory leaks
//         if (inventory != null)
//         {
//             inventory.OnInventoryChanged -= UpdateInventoryUI;
//         }
//     }
// }








// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class InventoryUI : MonoBehaviour
// {
//     [SerializeField]
//     private Inventory inventory;
//     public GameObject slotPrefab; // Assign your prefab slot
//     public Transform slotPanel; // Parent object in your UI where slots are housed

//     private List<GameObject> slots = new List<GameObject>();

//     void Start()
//     {
//         Debug.Log("Initializing inventory UI...");

//         // Subscribe to inventory changes
//         inventory.OnInventoryChanged += UpdateInventoryUI;

//         InitializeSlots();
//         UpdateInventoryUI();
//     }

//     void InitializeSlots()
//     {
//         Debug.Log("Initializing slots...");
//         for (int i = 0; i < inventory.items.Count; i++)
//         {
//             GameObject slot = Instantiate(slotPrefab, slotPanel);
//             slots.Add(slot);
//         }
//         Debug.Log("Slots initialized. Total slots: " + slots.Count);
//     }

//     public void UpdateInventoryUI()
//     {
//         Debug.Log("Updating inventory UI...");
//         // Clear existing slots and re-initialize
//         foreach (var slot in slots)
//         {
//             Destroy(slot);
//         }
//         slots.Clear();

//         for (int i = 0; i < inventory.items.Count; i++)
//         {
//             GameObject slot = Instantiate(slotPrefab, slotPanel);
//             slot.transform.Find("Icon").GetComponent<Image>().sprite = inventory.items[i].item.itemIcon;
//             slot.transform.Find("Count").GetComponent<Text>().text = inventory.items[i].stackSize.ToString();
//             slots.Add(slot);
//         }
//         Debug.Log("Inventory UI updated. Total slots: " + slots.Count);
//     }

//     private void OnDestroy()
//     {
//         // Unsubscribe from the event when the object is destroyed to avoid memory leaks
//         inventory.OnInventoryChanged -= UpdateInventoryUI;
//     }
// }









// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class InventoryUI : MonoBehaviour
// {
//     [SerializeField]
//     private Inventory inventory;
//     public GameObject slotPrefab; // Assign your prefab slot
//     public Transform slotPanel; // Parent object in your UI where slots are housed

//     private List<GameObject> slots = new List<GameObject>();

//     void Start()
//     {
//         Debug.Log("Initializing inventory UI...");
//         if (inventory == null)
//         {
//             Debug.LogError("Inventory is not assigned!");
//         }
//         else
//         {
//             Debug.Log("Inventory assigned, item count: " + inventory.items.Count);
//         }
        
//         InitializeSlots();
//         UpdateInventoryUI();
//     }

//     void InitializeSlots()
//     {
//         Debug.Log("Initializing slots...");
//         for (int i = 0; i < inventory.items.Count; i++)
//         {
//             Debug.Log("Creating slot for item: " + inventory.items[i].item.itemName);
//             GameObject slot = Instantiate(slotPrefab, slotPanel);
//             slots.Add(slot);
//         }
//         Debug.Log("Slots initialized. Total slots: " + slots.Count);
//     }

//     public void UpdateInventoryUI()
//     {
//         Debug.Log("Updating inventory UI...");
//         Debug.Log("Current slots count: " + slots.Count);
//         for (int i = 0; i < slots.Count; i++)
//         {
//             Debug.Log("Iterating slot: " + i);
//             if (i < inventory.items.Count)
//             {
//                 Debug.Log("Updating slot with item: " + inventory.items[i].item.itemName);
//                 slots[i].transform.Find("Icon").GetComponent<Image>().sprite = inventory.items[i].item.itemIcon;
//                 slots[i].transform.Find("Count").GetComponent<Text>().text = inventory.items[i].stackSize.ToString();
//                 Debug.Log("Item added to UI.");
//             }
//             else
//             {
//                 Debug.Log("Clearing empty slot: " + i);
//                 slots[i].transform.Find("Icon").GetComponent<Image>().sprite = null;
//                 slots[i].transform.Find("Count").GetComponent<Text>().text = "";
//             }
//         }
//         Debug.Log("Inventory UI updated.");
//     }
// }
















// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class InventoryUI : MonoBehaviour
// {
//     // public Inventory inventory; // Assign in the editor
//     [SerializeField]
//     private Inventory inventory;
//     public GameObject slotPrefab; // Assign your prefab slot
//     public Transform slotPanel; // Parent object in your UI where slots are housed

//     private List<GameObject> slots = new List<GameObject>();

//     void Start()
//     {
//         Debug.Log("Initializing inventory UI...");
//         InitializeSlots();
//         UpdateInventoryUI();
//     }

//     void InitializeSlots()
//     {
//         Debug.Log("Initializing slots...");
//         for (int i = 0; i < inventory.items.Count; i++)
//         {
//             GameObject slot = Instantiate(slotPrefab, slotPanel);
//             slots.Add(slot);
//         }
//         Debug.Log("Slots initialized.");
//         Debug.Log(slots.Count);
//     }

//     public void UpdateInventoryUI()
//     {
//         Debug.Log("Updating inventory UI...");
//         Debug.Log(slots.Count);
//         for (int i = 0; i < slots.Count; i++)
//         {
//             Debug.Log("iterate");
//             if (i < inventory.items.Count)
//             {
//                 slots[i].transform.Find("Icon").GetComponent<Image>().sprite = inventory.items[i].item.itemIcon;
//                 slots[i].transform.Find("Count").GetComponent<Text>().text = inventory.items[i].stackSize.ToString();
//                 Debug.Log("added");
//             }
//             else
//             {
//                 slots[i].transform.Find("Icon").GetComponent<Image>().sprite = null;
//                 slots[i].transform.Find("Count").GetComponent<Text>().text = "";
//                 Debug.Log("empty");
//             }
//         }
//         Debug.Log("Inventory UI updated.");
//     }
// }







// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class InventoryUI : MonoBehaviour
// {
//     // public Inventory inventory; // Assign in the editor
//     [SerializeField]
//     private Inventory inventory;
//     public GameObject slotPrefab; // Assign your prefab slot
//     public Transform slotPanel; // Parent object in your UI where slots are housed

//     private List<GameObject> slots = new List<GameObject>();

//     void Start()
//     {
//         InitializeSlots();
//         UpdateInventoryUI();
//     }

//     void InitializeSlots()
//     {
//         for (int i = 0; i < inventory.items.Count; i++)
//         {
//             GameObject slot = Instantiate(slotPrefab, slotPanel);
//             slots.Add(slot);
//         }
//     }

//     public void UpdateInventoryUI()
//     {
//         for (int i = 0; i < slots.Count; i++)
//         {
//             if (i < inventory.items.Count)
//             {
//                 slots[i].transform.Find("Icon").GetComponent<Image>().sprite = inventory.items[i].item.itemIcon;
//                 slots[i].transform.Find("Count").GetComponent<Text>().text = inventory.items[i].stackSize.ToString();
//             }
//             else
//             {
//                 slots[i].transform.Find("Icon").GetComponent<Image>().sprite = null;
//                 slots[i].transform.Find("Count").GetComponent<Text>().text = "";
//             }
//         }
//     }
// }