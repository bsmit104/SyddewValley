using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        }
        else
        {
            Debug.Log("Inventory assigned, item count: " + inventory.items.Count);
        }
        
        InitializeSlots();
        UpdateInventoryUI();
    }

    void InitializeSlots()
    {
        Debug.Log("Initializing slots...");
        for (int i = 0; i < inventory.items.Count; i++)
        {
            Debug.Log("Creating slot for item: " + inventory.items[i].item.itemName);
            GameObject slot = Instantiate(slotPrefab, slotPanel);
            slots.Add(slot);
        }
        Debug.Log("Slots initialized. Total slots: " + slots.Count);
    }

    public void UpdateInventoryUI()
    {
        Debug.Log("Updating inventory UI...");
        Debug.Log("Current slots count: " + slots.Count);
        for (int i = 0; i < slots.Count; i++)
        {
            Debug.Log("Iterating slot: " + i);
            if (i < inventory.items.Count)
            {
                Debug.Log("Updating slot with item: " + inventory.items[i].item.itemName);
                slots[i].transform.Find("Icon").GetComponent<Image>().sprite = inventory.items[i].item.itemIcon;
                slots[i].transform.Find("Count").GetComponent<Text>().text = inventory.items[i].stackSize.ToString();
                Debug.Log("Item added to UI.");
            }
            else
            {
                Debug.Log("Clearing empty slot: " + i);
                slots[i].transform.Find("Icon").GetComponent<Image>().sprite = null;
                slots[i].transform.Find("Count").GetComponent<Text>().text = "";
            }
        }
        Debug.Log("Inventory UI updated.");
    }
}

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