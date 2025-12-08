using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WorldTime;
using System.Collections;

[System.Serializable]
public class SaveData
{
    public int health;
    public int energy;
    public int hunger;
    public Vector3 playerPosition;
    public string currentScene;

    public List<InventoryItemData> inventoryItems = new List<InventoryItemData>();
    public int selectedItemIndex = -1;

    public string currentMonth;
    public int currentDay;
    public float timeOfDay;

    public List<PlacedItemData> placedItems = new List<PlacedItemData>();

    public string saveName;
    public DateTime lastSaveTime;
    public float totalPlayTime;
}

[System.Serializable]
public class InventoryItemData
{
    public string itemName;
    public int stackSize;
}

[System.Serializable]
public class PlacedItemData
{
    public string itemName;
    public Vector3 position;
    public string sceneName;
    public Vector3Int gridPosition;
}

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    private string saveDirectory;
    private int currentSlot = -1;
    private float sessionStartTime;

    [Header("Settings")]
    [SerializeField] private int maxSaveSlots = 3;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            saveDirectory = Application.persistentDataPath + "/Saves/";
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }

            sessionStartTime = Time.time;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame(int slot, string saveName = null)
    {
        currentSlot = slot;
        SaveData data = new SaveData();

        if (PlayerHealth.Instance != null)
        {
            data.health = PlayerHealth.Instance.CurrentHealth;
            data.energy = PlayerHealth.Instance.CurrentEnergy;
            data.hunger = PlayerHealth.Instance.CurrentHunger;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            data.playerPosition = player.transform.position;
        }

        data.currentScene = SceneManager.GetActiveScene().name;

        if (Inventory.Instance != null)
        {
            data.inventoryItems.Clear();
            foreach (var itemStack in Inventory.Instance.items)
            {
                if (itemStack?.item != null)
                {
                    data.inventoryItems.Add(new InventoryItemData
                    {
                        itemName = itemStack.item.itemName,
                        stackSize = itemStack.stackSize
                    });
                }
                else
                {
                    data.inventoryItems.Add(null);
                }
            }

            Item selectedItem = Inventory.Instance.GetSelectedItem();
            if (selectedItem != null)
            {
                for (int i = 0; i < Inventory.Instance.items.Count; i++)
                {
                    if (Inventory.Instance.items[i]?.item == selectedItem)
                    {
                        data.selectedItemIndex = i;
                        break;
                    }
                }
            }
        }

        if (CalendarManager.Instance != null)
        {
            data.currentMonth = CalendarManager.Instance.CurrentMonth.ToString();
            data.currentDay = CalendarManager.Instance.CurrentDay;
        }

        WorldClock clock = FindObjectOfType<WorldClock>();
        if (clock != null)
        {
            data.timeOfDay = clock.CurrentTimeOfDay;
        }

        // Save all placed items across all scenes
        data.placedItems.Clear();
        PlacedItem[] allPlacedItems = FindObjectsOfType<PlacedItem>(true);
        Debug.Log($"Saving: Found {allPlacedItems.Length} placed items total");

        foreach (PlacedItem item in allPlacedItems)
        {
            if (item.itemData != null)
            {
                data.placedItems.Add(new PlacedItemData
                {
                    itemName = item.itemData.itemName,
                    position = item.transform.position,
                    sceneName = item.gameObject.scene.name,
                    gridPosition = item.gridPosition
                });
                Debug.Log($"Saved: {item.itemData.itemName} in scene {item.gameObject.scene.name} at {item.transform.position}");
            }
        }

        data.saveName = saveName ?? $"Save {slot + 1}";
        data.lastSaveTime = DateTime.Now;
        data.totalPlayTime = Time.time - sessionStartTime;

        string path = GetSavePath(slot);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);

        Debug.Log($"Game saved to slot {slot} with {data.placedItems.Count} placed items");
    }

    // public void LoadGame(int slot)
    // {
    //     string path = GetSavePath(slot);

    //     if (!File.Exists(path))
    //     {
    //         Debug.LogError($"Save file not found: {path}");
    //         return;
    //     }

    //     currentSlot = slot;
    //     string json = File.ReadAllText(path);
    //     SaveData data = JsonUtility.FromJson<SaveData>(json);

    //     SceneManager.sceneLoaded += (scene, mode) => OnSceneLoadedForLoad(data);
    //     SceneManager.LoadScene("Town");
    // }

    public void LoadGame(int slot)
    {
        string path = GetSavePath(slot);

        if (!File.Exists(path))
        {
            Debug.LogError($"Save file not found: {path}");
            return;
        }

        currentSlot = slot;
        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // Use FadeController to load scene with proper fade
        if (FadeController.Instance != null)
        {
            FadeController.Instance.LoadScene("Town");
        }
        else
        {
            // Fallback if fade not ready
            SceneManager.LoadScene("Town");
        }

        // Store data for when scene finishes loading
        StartCoroutine(WaitForSceneLoadAndApply(data));
    }

    private IEnumerator WaitForSceneLoadAndApply(SaveData data)
    {
        // Wait until the Town scene is fully loaded
        while (SceneManager.GetActiveScene().name != "Town")
        {
            yield return null;
        }

        // Small delay to ensure everything is initialized
        yield return new WaitForEndOfFrame();

        ApplyLoadedDataAfterFade(data);
    }

    private void ApplyLoadedDataAfterFade(SaveData data)
    {
        // Same as your previous ApplyLoadedData but without coroutine hell
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.SetHealth(data.health);
            PlayerHealth.Instance.SetEnergy(data.energy);
            PlayerHealth.Instance.SetHunger(data.hunger);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(0.21f, -2.93f, 0f);
        }

        if (Inventory.Instance != null)
        {
            Inventory.Instance.items.Clear();
            foreach (var itemData in data.inventoryItems)
            {
                if (itemData != null)
                {
                    Item item = GetItemByName(itemData.itemName);
                    if (item != null)
                    {
                        Inventory.Instance.items.Add(new Inventory.ItemStack
                        {
                            item = item,
                            stackSize = itemData.stackSize
                        });
                    }
                }
                else
                {
                    Inventory.Instance.items.Add(new Inventory.ItemStack { item = null, stackSize = 0 });
                }
            }

            if (data.selectedItemIndex >= 0 && data.selectedItemIndex < Inventory.Instance.items.Count)
            {
                Inventory.Instance.SelectItem(data.selectedItemIndex);
            }

            InventoryUI.Instance?.UpdateInventoryUI();
        }

        if (CalendarManager.Instance != null)
        {
            if (Enum.TryParse(data.currentMonth, out CalendarManager.Month month))
            {
                CalendarManager.Instance.SetDate(month, data.currentDay);
            }
        }

        WorldClock clock = FindObjectOfType<WorldClock>();
        clock?.SetTimeOfDay(data.timeOfDay);

        LoadPlacedItemsForScene("Town", data);

        sessionStartTime = Time.time - data.totalPlayTime;

        Debug.Log($"Save loaded successfully! Player in Town.");
    }

    private void OnSceneLoadedForLoad(SaveData data)
    {
        SceneManager.sceneLoaded -= (scene, mode) => OnSceneLoadedForLoad(data);
        StartCoroutine(ApplyLoadedData(data));
    }

    private System.Collections.IEnumerator ApplyLoadedData(SaveData data)
    {
        yield return new WaitForEndOfFrame();

        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.SetHealth(data.health);
            PlayerHealth.Instance.SetEnergy(data.energy);
            PlayerHealth.Instance.SetHunger(data.hunger);
        }

        if (Inventory.Instance != null)
        {
            Inventory.Instance.items.Clear();
            foreach (var itemData in data.inventoryItems)
            {
                if (itemData != null)
                {
                    Item item = GetItemByName(itemData.itemName);
                    if (item != null)
                    {
                        Inventory.Instance.items.Add(new Inventory.ItemStack
                        {
                            item = item,
                            stackSize = itemData.stackSize
                        });
                    }
                }
                else
                {
                    Inventory.Instance.items.Add(new Inventory.ItemStack { item = null, stackSize = 0 });
                }
            }

            if (data.selectedItemIndex >= 0)
            {
                Inventory.Instance.SelectItem(data.selectedItemIndex);
            }

            if (InventoryUI.Instance != null)
            {
                InventoryUI.Instance.UpdateInventoryUI();
            }
        }

        if (CalendarManager.Instance != null)
        {
            CalendarManager.Month month = (CalendarManager.Month)Enum.Parse(typeof(CalendarManager.Month), data.currentMonth);
            CalendarManager.Instance.SetDate(month, data.currentDay);
        }

        WorldClock clock = FindObjectOfType<WorldClock>();
        if (clock != null)
        {
            clock.SetTimeOfDay(data.timeOfDay);
        }

        // Set player to spawn point (0.21, -2.93, 0)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(0.21f, -2.93f, 0f);
        }

        // Load placed items for Town only
        LoadPlacedItemsForScene("Town", data);

        sessionStartTime = Time.time - data.totalPlayTime;

        Debug.Log($"Game loaded from slot {currentSlot} - Player spawned at Town spawn point");
    }

    private void LoadPlacedItemsForScene(string sceneName, SaveData data)
    {
        Debug.Log($"LoadPlacedItemsForScene called for: {sceneName}");

        PlacedItem[] existingItems = FindObjectsOfType<PlacedItem>();
        foreach (var item in existingItems)
        {
            if (item.gameObject.scene.name == sceneName)
            {
                Debug.Log($"Destroying existing placed item: {item.itemData?.itemName}");
                Destroy(item.gameObject);
            }
        }

        int loadedCount = 0;
        foreach (var itemData in data.placedItems)
        {
            if (itemData.sceneName == sceneName)
            {
                Item item = GetItemByName(itemData.itemName);
                if (item != null)
                {
                    SpawnPlacedItem(item, itemData.position, itemData.gridPosition);
                    loadedCount++;
                }
                else
                {
                    Debug.LogWarning($"Could not find item: {itemData.itemName}");
                }
            }
        }

        Debug.Log($"Loaded {loadedCount} placed items for scene {sceneName}");
    }

    private Item GetItemByName(string itemName)
    {
        Item[] allItems = Resources.LoadAll<Item>("Items");
        foreach (Item item in allItems)
        {
            if (item.itemName == itemName)
                return item;
        }
        return null;
    }

    private void SpawnPlacedItem(Item item, Vector3 position, Vector3Int gridPos)
    {
        GameObject obj = new GameObject($"Placed_{item.itemName}");
        obj.transform.position = position;

        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = item.itemIcon;
        renderer.sortingOrder = 5;

        PlacedItem placedItem = obj.AddComponent<PlacedItem>();
        placedItem.Initialize(item, gridPos);

        BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        Debug.Log($"Spawned placed item: {item.itemName} at {position}");
    }

    public void DeleteSave(int slot)
    {
        string path = GetSavePath(slot);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Save slot {slot} deleted");
        }
    }

    public bool SaveExists(int slot)
    {
        return File.Exists(GetSavePath(slot));
    }

    public SaveData GetSaveData(int slot)
    {
        string path = GetSavePath(slot);
        if (!File.Exists(path)) return null;

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }

    private string GetSavePath(int slot)
    {
        return saveDirectory + $"save_{slot}.json";
    }

    public void AutoSave()
    {
        if (currentSlot >= 0)
        {
            SaveGame(currentSlot);
            Debug.Log("Auto-saved!");
        }
    }

    public void LoadPlacedItemsForCurrentScene()
    {
        if (currentSlot < 0) return;

        string path = GetSavePath(currentSlot);
        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        string currentScene = SceneManager.GetActiveScene().name;
        LoadPlacedItemsForScene(currentScene, data);
    }
}


// using System;
// using System.IO;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using WorldTime;

// [System.Serializable]
// public class SaveData
// {
//     // Player Data
//     public int health;
//     public int energy;
//     public int hunger;
//     public Vector3 playerPosition;
//     public string currentScene;

//     // Inventory Data
//     public List<InventoryItemData> inventoryItems = new List<InventoryItemData>();
//     public int selectedItemIndex = -1;

//     // World Data
//     public string currentMonth;
//     public int currentDay;
//     public float timeOfDay; // 0-1, where 0 = midnight

//     // Placed Items Data
//     public List<PlacedItemData> placedItems = new List<PlacedItemData>();

//     // Metadata
//     public string saveName;
//     public DateTime lastSaveTime;
//     public float totalPlayTime;
// }

// [System.Serializable]
// public class InventoryItemData
// {
//     public string itemName;
//     public int stackSize;
// }

// [System.Serializable]
// public class PlacedItemData
// {
//     public string itemName;
//     public Vector3 position;
//     public string sceneName;
//     public Vector3Int gridPosition;
// }

// public class SaveSystem : MonoBehaviour
// {
//     public static SaveSystem Instance { get; private set; }

//     private string saveDirectory;
//     private int currentSlot = -1;
//     private float sessionStartTime;

//     [Header("Settings")]
//     [SerializeField] private int maxSaveSlots = 3;

//     void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);

//             saveDirectory = Application.persistentDataPath + "/Saves/";
//             if (!Directory.Exists(saveDirectory))
//             {
//                 Directory.CreateDirectory(saveDirectory);
//             }

//             sessionStartTime = Time.time;
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }

//     public void SaveGame(int slot, string saveName = null)
//     {
//         currentSlot = slot;
//         SaveData data = new SaveData();

//         // Save player data
//         if (PlayerHealth.Instance != null)
//         {
//             data.health = PlayerHealth.Instance.CurrentHealth;
//             data.energy = PlayerHealth.Instance.CurrentEnergy;
//             data.hunger = PlayerHealth.Instance.CurrentHunger;
//         }

//         // Save player position
//         GameObject player = GameObject.FindGameObjectWithTag("Player");
//         if (player != null)
//         {
//             data.playerPosition = player.transform.position;
//         }

//         // Save current scene
//         data.currentScene = SceneManager.GetActiveScene().name;

//         // Save inventory
//         if (Inventory.Instance != null)
//         {
//             data.inventoryItems.Clear();
//             foreach (var itemStack in Inventory.Instance.items)
//             {
//                 if (itemStack?.item != null)
//                 {
//                     data.inventoryItems.Add(new InventoryItemData
//                     {
//                         itemName = itemStack.item.itemName,
//                         stackSize = itemStack.stackSize
//                     });
//                 }
//                 else
//                 {
//                     data.inventoryItems.Add(null); // Preserve empty slots
//                 }
//             }

//             Item selectedItem = Inventory.Instance.GetSelectedItem();
//             if (selectedItem != null)
//             {
//                 for (int i = 0; i < Inventory.Instance.items.Count; i++)
//                 {
//                     if (Inventory.Instance.items[i]?.item == selectedItem)
//                     {
//                         data.selectedItemIndex = i;
//                         break;
//                     }
//                 }
//             }
//         }

//         // Save calendar data
//         if (CalendarManager.Instance != null)
//         {
//             data.currentMonth = CalendarManager.Instance.CurrentMonth.ToString();
//             data.currentDay = CalendarManager.Instance.CurrentDay;
//         }

//         // Save time of day
//         WorldClock clock = FindObjectOfType<WorldClock>();
//         if (clock != null)
//         {
//             data.timeOfDay = clock.CurrentTimeOfDay;
//         }

//         // Save all placed items across all scenes
//         data.placedItems.Clear();
//         PlacedItem[] allPlacedItems = FindObjectsOfType<PlacedItem>(true);
//         foreach (PlacedItem item in allPlacedItems)
//         {
//             if (item.itemData != null)
//             {
//                 data.placedItems.Add(new PlacedItemData
//                 {
//                     itemName = item.itemData.itemName,
//                     position = item.transform.position,
//                     sceneName = item.gameObject.scene.name,
//                     gridPosition = item.gridPosition
//                 });
//             }
//         }

//         // Save metadata
//         data.saveName = saveName ?? $"Save {slot + 1}";
//         data.lastSaveTime = DateTime.Now;
//         data.totalPlayTime = Time.time - sessionStartTime;

//         // Write to file
//         string path = GetSavePath(slot);
//         string json = JsonUtility.ToJson(data, true);
//         File.WriteAllText(path, json);

//         Debug.Log($"Game saved to slot {slot}: {path}");
//     }

//     public void LoadGame(int slot)
//     {
//         string path = GetSavePath(slot);

//         if (!File.Exists(path))
//         {
//             Debug.LogError($"Save file not found: {path}");
//             return;
//         }

//         currentSlot = slot;
//         string json = File.ReadAllText(path);
//         SaveData data = JsonUtility.FromJson<SaveData>(json);

//         // Load the saved scene first
//         SceneManager.sceneLoaded += (scene, mode) => OnSceneLoadedForLoad(data);
//         SceneManager.LoadScene(data.currentScene);
//     }

//     private void OnSceneLoadedForLoad(SaveData data)
//     {
//         SceneManager.sceneLoaded -= (scene, mode) => OnSceneLoadedForLoad(data);

//         // Wait a frame for everything to initialize
//         StartCoroutine(ApplyLoadedData(data));
//     }

//     private System.Collections.IEnumerator ApplyLoadedData(SaveData data)
//     {
//         yield return new WaitForEndOfFrame();

//         // Restore player health
//         if (PlayerHealth.Instance != null)
//         {
//             PlayerHealth.Instance.SetHealth(data.health);
//             PlayerHealth.Instance.SetEnergy(data.energy);
//             PlayerHealth.Instance.SetHunger(data.hunger);
//         }

//         // Restore player position
//         GameObject player = GameObject.FindGameObjectWithTag("Player");
//         if (player != null)
//         {
//             player.transform.position = data.playerPosition;
//         }

//         // Restore inventory
//         if (Inventory.Instance != null)
//         {
//             Inventory.Instance.items.Clear();
//             foreach (var itemData in data.inventoryItems)
//             {
//                 if (itemData != null)
//                 {
//                     Item item = GetItemByName(itemData.itemName);
//                     if (item != null)
//                     {
//                         Inventory.Instance.items.Add(new Inventory.ItemStack
//                         {
//                             item = item,
//                             stackSize = itemData.stackSize
//                         });
//                     }
//                 }
//                 else
//                 {
//                     Inventory.Instance.items.Add(new Inventory.ItemStack { item = null, stackSize = 0 });
//                 }
//             }

//             if (data.selectedItemIndex >= 0)
//             {
//                 Inventory.Instance.SelectItem(data.selectedItemIndex);
//             }

//             // Trigger inventory update through InventoryUI instead
//             if (InventoryUI.Instance != null)
//             {
//                 InventoryUI.Instance.UpdateInventoryUI();
//             }
//         }

//         // Restore calendar
//         if (CalendarManager.Instance != null)
//         {
//             CalendarManager.Month month = (CalendarManager.Month)Enum.Parse(typeof(CalendarManager.Month), data.currentMonth);
//             CalendarManager.Instance.SetDate(month, data.currentDay);
//         }

//         // Restore time of day
//         WorldClock clock = FindObjectOfType<WorldClock>();
//         if (clock != null)
//         {
//             clock.SetTimeOfDay(data.timeOfDay);
//         }

//         // Restore placed items for current scene
//         string currentScene = SceneManager.GetActiveScene().name;
//         foreach (var itemData in data.placedItems)
//         {
//             if (itemData.sceneName == currentScene)
//             {
//                 Item item = GetItemByName(itemData.itemName);
//                 if (item != null)
//                 {
//                     ItemPlacement placement = FindObjectOfType<ItemPlacement>();
//                     if (placement != null)
//                     {
//                         // You'll need a public method to spawn items
//                         SpawnPlacedItem(item, itemData.position, itemData.gridPosition);
//                     }
//                 }
//             }
//         }

//         sessionStartTime = Time.time - data.totalPlayTime;
//         Debug.Log($"Game loaded from slot {currentSlot}");
//     }

//     public void DeleteSave(int slot)
//     {
//         string path = GetSavePath(slot);
//         if (File.Exists(path))
//         {
//             File.Delete(path);
//             Debug.Log($"Save slot {slot} deleted");
//         }
//     }

//     public bool SaveExists(int slot)
//     {
//         return File.Exists(GetSavePath(slot));
//     }

//     public SaveData GetSaveData(int slot)
//     {
//         string path = GetSavePath(slot);
//         if (!File.Exists(path)) return null;

//         string json = File.ReadAllText(path);
//         return JsonUtility.FromJson<SaveData>(json);
//     }

//     private string GetSavePath(int slot)
//     {
//         return saveDirectory + $"save_{slot}.json";
//     }

//     private Item GetItemByName(string itemName)
//     {
//         // Load all Item ScriptableObjects
//         Item[] allItems = Resources.LoadAll<Item>("Items");
//         foreach (Item item in allItems)
//         {
//             if (item.itemName == itemName)
//                 return item;
//         }
//         return null;
//     }

//     private void SpawnPlacedItem(Item item, Vector3 position, Vector3Int gridPos)
//     {
//         // Load prefab from Resources
//         GameObject prefab = Resources.Load<GameObject>("Prefabs/PlacedItem");

//         if (prefab == null)
//         {
//             Debug.LogError("Could not load PlacedItem prefab from Resources/Prefabs/PlacedItem");
//             return;
//         }

//         GameObject obj = Instantiate(prefab, position, Quaternion.identity);
//         PlacedItem placedItem = obj.GetComponent<PlacedItem>();

//         if (placedItem != null)
//         {
//             placedItem.Initialize(item, gridPos);
//         }
//         else
//         {
//             Debug.LogError("PlacedItem component not found on spawned prefab!");
//         }
//     }

//     public void AutoSave()
//     {
//         if (currentSlot >= 0)
//         {
//             SaveGame(currentSlot);
//             Debug.Log("Auto-saved!");
//         }
//     }
// }
