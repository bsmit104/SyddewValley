using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WorldTime;
using System.Collections;
using System.Linq;

[System.Serializable]
public class SaveData
{
    public int health, energy, hunger;
    public Vector3 playerPosition;
    public string currentScene;

    public List<InventoryItemData> inventoryItems = new List<InventoryItemData>();
    public int selectedItemIndex = -1;

    public string currentMonth;
    public int currentDay;
    public float timeOfDay;

    public List<PlacedItemData> placedItems = new List<PlacedItemData>();

    // Money
    public int money;

    // Friendship data
    public List<FriendshipData> friendshipData = new List<FriendshipData>();

    // Chest inventories
    public List<ChestData> chestInventories = new List<ChestData>();

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

[System.Serializable]
public class FriendshipData
{
    public string npcName;
    public int friendshipPoints;
    public string lastGiftDate;
}

[System.Serializable]
public class ChestData
{
    public string chestID;
    public List<InventoryItemData> items = new List<InventoryItemData>();
}

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;

    private string saveDirectory;
    private int currentSlot = -1;
    private float sessionStartTime;

    [Header("Auto-Save")]
    [SerializeField] private float autoSaveInterval = 300f;
    private float timeSinceLastSave = 0f;

    // Temporary storage for chest inventories
    private Dictionary<string, List<InventoryItemData>> tempChestData = new Dictionary<string, List<InventoryItemData>>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            saveDirectory = Application.persistentDataPath + "/Saves/";
            if (!Directory.Exists(saveDirectory))
                Directory.CreateDirectory(saveDirectory);

            sessionStartTime = Time.time;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        timeSinceLastSave += Time.deltaTime;
        if (timeSinceLastSave >= autoSaveInterval && currentSlot >= 0)
        {
            AutoSave();
            timeSinceLastSave = 0f;
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            AutoSave();
            Debug.Log("Manual save (F5)");
        }
    }

    private void OnApplicationQuit() => AutoSave();

    // ================================================================
    // CHEST SAVE/LOAD METHODS
    // ================================================================
    public void SaveChestInventory(string chestID, ChestInventory chestInventory)
    {
        if (string.IsNullOrEmpty(chestID) || chestInventory == null) return;

        List<InventoryItemData> items = new List<InventoryItemData>();
        foreach (var stack in chestInventory.items)
        {
            if (stack?.item != null)
            {
                items.Add(new InventoryItemData
                {
                    itemName = stack.item.itemName,
                    stackSize = stack.stackSize
                });
            }
            else
            {
                items.Add(null);
            }
        }

        tempChestData[chestID] = items;
    }

    public void LoadChestInventory(string chestID, ChestInventory chestInventory)
    {
        if (currentSlot < 0 || string.IsNullOrEmpty(chestID) || chestInventory == null) return;

        string path = GetSavePath(currentSlot);
        if (!File.Exists(path)) return;

        try
        {
            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
            if (data.chestInventories == null) return;

            ChestData chestData = data.chestInventories.Find(c => c.chestID == chestID);
            if (chestData == null) return;

            chestInventory.items.Clear();
            foreach (var itemData in chestData.items)
            {
                if (itemData != null)
                {
                    Item item = GetItemByName(itemData.itemName);
                    chestInventory.items.Add(new Inventory.ItemStack
                    {
                        item = item,
                        stackSize = itemData.stackSize
                    });
                }
                else
                {
                    chestInventory.items.Add(new Inventory.ItemStack { item = null, stackSize = 0 });
                }
            }

            Debug.Log($"Loaded chest '{chestID}' with {chestData.items.Count} items");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load chest '{chestID}': {e.Message}");
        }
    }

    // ================================================================
    // SAVE
    // ================================================================
    public void SaveGame(int slot, string saveName = null)
    {
        currentSlot = slot;
        SaveData data = new SaveData();

        // Player stats
        if (PlayerHealth.Instance)
        {
            data.health = PlayerHealth.Instance.CurrentHealth;
            data.energy = PlayerHealth.Instance.CurrentEnergy;
            data.hunger = PlayerHealth.Instance.CurrentHunger;
        }

        // Money
        if (MoneyManager.Instance)
        {
            data.money = MoneyManager.Instance.GetCurrentMoney();
        }

        // Player position & current scene
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) data.playerPosition = player.transform.position;
        data.currentScene = SceneManager.GetActiveScene().name;

        // Inventory
        if (Inventory.Instance)
        {
            data.inventoryItems.Clear();
            foreach (var stack in Inventory.Instance.items)
            {
                data.inventoryItems.Add(stack?.item != null
                    ? new InventoryItemData { itemName = stack.item.itemName, stackSize = stack.stackSize }
                    : null);
            }

            var selected = Inventory.Instance.GetSelectedItem();
            data.selectedItemIndex = Inventory.Instance.items.FindIndex(s => s?.item == selected);
        }

        // Time / Calendar
        if (CalendarManager.Instance)
        {
            data.currentMonth = CalendarManager.Instance.CurrentMonth.ToString();
            data.currentDay = CalendarManager.Instance.CurrentDay;
        }
        var clock = FindObjectOfType<WorldClock>();
        if (clock) data.timeOfDay = clock.CurrentTimeOfDay;

        // Friendship data
        if (FriendshipManager.Instance)
        {
            data.friendshipData.Clear();
            var friendshipDict = FriendshipManager.Instance.GetFriendshipData();
            var lastGiftDates = FriendshipManager.Instance.GetLastGiftDates();
            
            foreach (var kvp in friendshipDict)
            {
                string lastDate = lastGiftDates.ContainsKey(kvp.Key) ? lastGiftDates[kvp.Key] : "";
                
                data.friendshipData.Add(new FriendshipData
                {
                    npcName = kvp.Key,
                    friendshipPoints = kvp.Value,
                    lastGiftDate = lastDate
                });
            }
            Debug.Log($"Saved friendship data for {data.friendshipData.Count} NPCs");
        }

        // === PLACED ITEMS (merge across all scenes) ===
        List<PlacedItemData> merged = new List<PlacedItemData>();
        string currentSceneName = SceneManager.GetActiveScene().name;

        string path = GetSavePath(slot);
        if (File.Exists(path))
        {
            try
            {
                SaveData old = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
                if (old.placedItems != null)
                {
                    foreach (var p in old.placedItems)
                    {
                        if (p != null && p.sceneName != currentSceneName)
                            merged.Add(p);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not load previous save data: {e.Message}");
            }
        }

        PlacedItem[] currentSceneItems = FindObjectsOfType<PlacedItem>(true);
        foreach (var item in currentSceneItems)
        {
            if (item != null && item.itemData != null)
            {
                merged.Add(new PlacedItemData
                {
                    itemName = item.itemData.itemName,
                    position = item.transform.position,
                    sceneName = currentSceneName,
                    gridPosition = item.gridPosition
                });
            }
        }

        data.placedItems = merged;

        // === CHEST INVENTORIES ===
        data.chestInventories.Clear();
        
        foreach (var kvp in tempChestData)
        {
            data.chestInventories.Add(new ChestData
            {
                chestID = kvp.Key,
                items = kvp.Value
            });
        }

        Chest[] allChests = FindObjectsOfType<Chest>();
        foreach (var chest in allChests)
        {
            if (chest != null && chest.ChestInventory != null && !string.IsNullOrEmpty(chest.ChestID))
            {
                if (!tempChestData.ContainsKey(chest.ChestID))
                {
                    List<InventoryItemData> items = new List<InventoryItemData>();
                    foreach (var stack in chest.ChestInventory.items)
                    {
                        if (stack?.item != null)
                        {
                            items.Add(new InventoryItemData
                            {
                                itemName = stack.item.itemName,
                                stackSize = stack.stackSize
                            });
                        }
                        else
                        {
                            items.Add(null);
                        }
                    }

                    data.chestInventories.Add(new ChestData
                    {
                        chestID = chest.ChestID,
                        items = items
                    });
                }
            }
        }

        // Final metadata
        data.saveName = saveName ?? $"Save {slot + 1}";
        data.lastSaveTime = DateTime.Now;
        data.totalPlayTime = Time.time - sessionStartTime;

        File.WriteAllText(path, JsonUtility.ToJson(data, true));
        Debug.Log($"SAVED slot {slot} – Money: ${data.money}, Time: {data.timeOfDay:F2}, Friendships: {data.friendshipData.Count}, Chests: {data.chestInventories.Count}");
    }

    // ================================================================
    // LOAD
    // ================================================================
    public void LoadGame(int slot)
    {
        string path = GetSavePath(slot);
        if (!File.Exists(path))
        {
            Debug.LogError("Save file not found!");
            return;
        }

        currentSlot = slot;
        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        SceneManager.LoadScene("Town");
        StartCoroutine(ApplyAfterTownLoad(data));
    }

    private IEnumerator ApplyAfterTownLoad(SaveData data)
    {
        while (SceneManager.GetActiveScene().name != "Town")
            yield return null;
        yield return new WaitForEndOfFrame();

        // Health
        if (PlayerHealth.Instance)
        {
            PlayerHealth.Instance.SetHealth(data.health);
            PlayerHealth.Instance.SetEnergy(data.energy);
            PlayerHealth.Instance.SetHunger(data.hunger);
        }

        // Money
        if (MoneyManager.Instance)
        {
            MoneyManager.Instance.SetMoney(data.money);
        }

        // Fixed spawn position
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) player.transform.position = new Vector3(0.21f, -2.93f, 0f);

        // Inventory
        if (Inventory.Instance)
        {
            Inventory.Instance.items.Clear();
            foreach (var id in data.inventoryItems)
            {
                if (id != null)
                {
                    var item = GetItemByName(id.itemName);
                    Inventory.Instance.items.Add(new Inventory.ItemStack { item = item, stackSize = id.stackSize });
                }
                else
                {
                    Inventory.Instance.items.Add(new Inventory.ItemStack { item = null, stackSize = 0 });
                }
            }

            if (data.selectedItemIndex >= 0 && data.selectedItemIndex < Inventory.Instance.items.Count)
                Inventory.Instance.SelectItem(data.selectedItemIndex);

            InventoryUI.Instance?.UpdateInventoryUI();
        }

        // Calendar & time
        if (CalendarManager.Instance && Enum.TryParse(data.currentMonth, out CalendarManager.Month m))
            CalendarManager.Instance.SetDate(m, data.currentDay);
        
        var clock = FindObjectOfType<WorldClock>();
        if (clock) clock.SetTimeOfDay(data.timeOfDay);

        // Friendship data
        if (FriendshipManager.Instance && data.friendshipData != null)
        {
            Dictionary<string, int> friendshipDict = new Dictionary<string, int>();
            Dictionary<string, string> lastGiftDict = new Dictionary<string, string>();
            
            foreach (var fd in data.friendshipData)
            {
                if (fd != null && !string.IsNullOrEmpty(fd.npcName))
                {
                    friendshipDict[fd.npcName] = fd.friendshipPoints;
                    
                    if (!string.IsNullOrEmpty(fd.lastGiftDate))
                    {
                        lastGiftDict[fd.npcName] = fd.lastGiftDate;
                    }
                }
            }
            
            FriendshipManager.Instance.LoadFriendshipData(friendshipDict);
            FriendshipManager.Instance.LoadLastGiftDates(lastGiftDict);
            Debug.Log($"Loaded friendship data for {friendshipDict.Count} NPCs");
        }

        // Spawn placed items for Town
        SpawnPlacedItemsForCurrentScene(data);

        sessionStartTime = Time.time - data.totalPlayTime;
        Debug.Log("LOAD COMPLETE");
    }

    // ================================================================
    // Called by PlacedItemLoader when entering any scene
    // ================================================================
    public void LoadPlacedItemsForCurrentScene()
    {
        if (currentSlot < 0)
        {
            Debug.Log("No active save slot, skipping placed items load");
            return;
        }

        string path = GetSavePath(currentSlot);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Save file doesn't exist at: {path}");
            return;
        }

        try
        {
            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
            SpawnPlacedItemsForCurrentScene(data);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load placed items: {e.Message}");
        }
    }

    private void SpawnPlacedItemsForCurrentScene(SaveData data)
    {
        if (data == null || data.placedItems == null) return;

        string current = SceneManager.GetActiveScene().name;

        PlacedItem[] existingItems = FindObjectsOfType<PlacedItem>(true);
        foreach (var p in existingItems)
        {
            if (p != null && p.gameObject != null && p.gameObject.scene.name == current)
                Destroy(p.gameObject);
        }

        int spawned = 0;
        
        foreach (var pd in data.placedItems)
        {
            if (pd == null || pd.sceneName != current) continue;

            Item item = GetItemByName(pd.itemName);
            if (item == null) continue;

            try
            {
                GameObject go = new GameObject($"Placed_{item.itemName}");
                go.transform.position = pd.position;

                PlacedItem pi = go.AddComponent<PlacedItem>();
                pi.Initialize(item, pd.gridPosition);

                SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingLayerName = "Default";
                    sr.sortingOrder = 6;
                }

                spawned++;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error spawning {pd.itemName}: {e.Message}");
            }
        }

        Debug.Log($"SPAWNED {spawned} placed items in {current}");
    }

    private Item GetItemByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        
        Item[] allItems = Resources.LoadAll<Item>("Items");
        return allItems.FirstOrDefault(i => i != null && i.itemName == name);
    }

    private string GetSavePath(int slot) => saveDirectory + $"save_{slot}.json";

    // ================================================================
    // Public helpers
    // ================================================================
    public void AutoSave()
    {
        if (currentSlot >= 0)
        {
            SaveGame(currentSlot);
            Debug.Log("Auto-saved");
        }
    }

    public void DeleteSave(int slot)
    {
        var p = GetSavePath(slot);
        if (File.Exists(p)) File.Delete(p);
    }

    public bool SaveExists(int slot) => File.Exists(GetSavePath(slot));

    public SaveData GetSaveData(int slot)
    {
        if (!SaveExists(slot)) return null;
        try
        {
            return JsonUtility.FromJson<SaveData>(File.ReadAllText(GetSavePath(slot)));
        }
        catch
        {
            return null;
        }
    }

    public int GetCurrentSlot() => currentSlot;
}

// using System;
// using System.IO;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using WorldTime;
// using System.Collections;
// using System.Linq;

// [System.Serializable]
// public class SaveData
// {
//     public int health, energy, hunger;
//     public Vector3 playerPosition;
//     public string currentScene;

//     public List<InventoryItemData> inventoryItems = new List<InventoryItemData>();
//     public int selectedItemIndex = -1;

//     public string currentMonth;
//     public int currentDay;
//     public float timeOfDay;

//     public List<PlacedItemData> placedItems = new List<PlacedItemData>();

//     // NEW: Money
//     public int money;

//     // NEW: Chest inventories
//     public List<ChestData> chestInventories = new List<ChestData>();

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

// [System.Serializable]
// public class ChestData
// {
//     public string chestID;
//     public List<InventoryItemData> items = new List<InventoryItemData>();
// }

// public class SaveSystem : MonoBehaviour
// {
//     public static SaveSystem Instance;

//     private string saveDirectory;
//     private int currentSlot = -1;
//     private float sessionStartTime;

//     [Header("Auto-Save")]
//     [SerializeField] private float autoSaveInterval = 300f;
//     private float timeSinceLastSave = 0f;

//     void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);

//             saveDirectory = Application.persistentDataPath + "/Saves/";
//             if (!Directory.Exists(saveDirectory))
//                 Directory.CreateDirectory(saveDirectory);

//             sessionStartTime = Time.time;
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }

//     void Update()
//     {
//         timeSinceLastSave += Time.deltaTime;
//         if (timeSinceLastSave >= autoSaveInterval && currentSlot >= 0)
//         {
//             AutoSave();
//             timeSinceLastSave = 0f;
//         }

//         if (Input.GetKeyDown(KeyCode.F5))
//         {
//             AutoSave();
//             Debug.Log("Manual save (F5)");
//         }
//     }

//     private void OnApplicationQuit() => AutoSave();

//     // Temporary storage for chest inventories before saving
//     private Dictionary<string, List<InventoryItemData>> tempChestData = new Dictionary<string, List<InventoryItemData>>();

//     // Called by individual chests to register their contents
//     public void SaveChestInventory(string chestID, ChestInventory chestInventory)
//     {
//         if (string.IsNullOrEmpty(chestID) || chestInventory == null) return;

//         List<InventoryItemData> items = new List<InventoryItemData>();
//         foreach (var stack in chestInventory.items)
//         {
//             if (stack?.item != null)
//             {
//                 items.Add(new InventoryItemData
//                 {
//                     itemName = stack.item.itemName,
//                     stackSize = stack.stackSize
//                 });
//             }
//             else
//             {
//                 items.Add(null);
//             }
//         }

//         tempChestData[chestID] = items;
//     }

//     // Called by individual chests to load their contents
//     public void LoadChestInventory(string chestID, ChestInventory chestInventory)
//     {
//         if (currentSlot < 0 || string.IsNullOrEmpty(chestID) || chestInventory == null) return;

//         string path = GetSavePath(currentSlot);
//         if (!File.Exists(path)) return;

//         try
//         {
//             SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
//             if (data.chestInventories == null) return;

//             ChestData chestData = data.chestInventories.Find(c => c.chestID == chestID);
//             if (chestData == null) return;

//             chestInventory.items.Clear();
//             foreach (var itemData in chestData.items)
//             {
//                 if (itemData != null)
//                 {
//                     Item item = GetItemByName(itemData.itemName);
//                     chestInventory.items.Add(new Inventory.ItemStack
//                     {
//                         item = item,
//                         stackSize = itemData.stackSize
//                     });
//                 }
//                 else
//                 {
//                     chestInventory.items.Add(new Inventory.ItemStack { item = null, stackSize = 0 });
//                 }
//             }

//             Debug.Log($"Loaded chest '{chestID}' with {chestData.items.Count} items");
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"Failed to load chest '{chestID}': {e.Message}");
//         }
//     }

//     // ================================================================
//     // SAVE
//     // ================================================================
//     public void SaveGame(int slot, string saveName = null)
//     {
//         currentSlot = slot;
//         SaveData data = new SaveData();

//         // Player stats
//         if (PlayerHealth.Instance)
//         {
//             data.health = PlayerHealth.Instance.CurrentHealth;
//             data.energy = PlayerHealth.Instance.CurrentEnergy;
//             data.hunger = PlayerHealth.Instance.CurrentHunger;
//         }

//         // Money
//         if (MoneyManager.Instance)
//         {
//             data.money = MoneyManager.Instance.GetCurrentMoney();
//         }

//         // Player position & current scene
//         var player = GameObject.FindGameObjectWithTag("Player");
//         if (player) data.playerPosition = player.transform.position;
//         data.currentScene = SceneManager.GetActiveScene().name;

//         // Inventory
//         if (Inventory.Instance)
//         {
//             data.inventoryItems.Clear();
//             foreach (var stack in Inventory.Instance.items)
//             {
//                 data.inventoryItems.Add(stack?.item != null
//                     ? new InventoryItemData { itemName = stack.item.itemName, stackSize = stack.stackSize }
//                     : null);
//             }

//             var selected = Inventory.Instance.GetSelectedItem();
//             data.selectedItemIndex = Inventory.Instance.items.FindIndex(s => s?.item == selected);
//         }

//         // Time / Calendar
//         if (CalendarManager.Instance)
//         {
//             data.currentMonth = CalendarManager.Instance.CurrentMonth.ToString();
//             data.currentDay = CalendarManager.Instance.CurrentDay;
//         }
//         var clock = FindObjectOfType<WorldClock>();
//         if (clock) data.timeOfDay = clock.CurrentTimeOfDay;

//         // === PLACED ITEMS (merge across all scenes) ===
//         List<PlacedItemData> merged = new List<PlacedItemData>();
//         string currentSceneName = SceneManager.GetActiveScene().name;

//         // Load existing save and keep items from OTHER scenes
//         string path = GetSavePath(slot);
//         if (File.Exists(path))
//         {
//             try
//             {
//                 SaveData old = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
//                 if (old.placedItems != null)
//                 {
//                     foreach (var p in old.placedItems)
//                     {
//                         if (p != null && p.sceneName != currentSceneName)
//                             merged.Add(p);
//                     }
//                 }
//             }
//             catch (Exception e)
//             {
//                 Debug.LogWarning($"Could not load previous save data: {e.Message}");
//             }
//         }

//         // Add current scene items (they are still alive here)
//         PlacedItem[] currentSceneItems = FindObjectsOfType<PlacedItem>(true);
//         foreach (var item in currentSceneItems)
//         {
//             if (item != null && item.itemData != null)
//             {
//                 merged.Add(new PlacedItemData
//                 {
//                     itemName = item.itemData.itemName,
//                     position = item.transform.position,
//                     sceneName = currentSceneName,
//                     gridPosition = item.gridPosition
//                 });
//             }
//         }

//         data.placedItems = merged;

//         // === CHEST INVENTORIES ===
//         data.chestInventories.Clear();
        
//         // First, collect all chest data from the temp storage
//         foreach (var kvp in tempChestData)
//         {
//             data.chestInventories.Add(new ChestData
//             {
//                 chestID = kvp.Key,
//                 items = kvp.Value
//             });
//         }

//         // Also save any currently active chests in the scene
//         Chest[] allChests = FindObjectsOfType<Chest>();
//         foreach (var chest in allChests)
//         {
//             if (chest != null && chest.ChestInventory != null && !string.IsNullOrEmpty(chest.ChestID))
//             {
//                 // Check if we already have this chest in temp storage
//                 if (!tempChestData.ContainsKey(chest.ChestID))
//                 {
//                     List<InventoryItemData> items = new List<InventoryItemData>();
//                     foreach (var stack in chest.ChestInventory.items)
//                     {
//                         if (stack?.item != null)
//                         {
//                             items.Add(new InventoryItemData
//                             {
//                                 itemName = stack.item.itemName,
//                                 stackSize = stack.stackSize
//                             });
//                         }
//                         else
//                         {
//                             items.Add(null);
//                         }
//                     }

//                     data.chestInventories.Add(new ChestData
//                     {
//                         chestID = chest.ChestID,
//                         items = items
//                     });
//                 }
//             }
//         }

//         // Final metadata
//         data.saveName = saveName ?? $"Save {slot + 1}";
//         data.lastSaveTime = DateTime.Now;
//         data.totalPlayTime = Time.time - sessionStartTime;

//         File.WriteAllText(path, JsonUtility.ToJson(data, true));
//         Debug.Log($"SAVED slot {slot} – Money: ${data.money}, Time: {data.timeOfDay:F2}, Chests: {data.chestInventories.Count}");
//     }

//     // ================================================================
//     // LOAD
//     // ================================================================
//     public void LoadGame(int slot)
//     {
//         string path = GetSavePath(slot);
//         if (!File.Exists(path))
//         {
//             Debug.LogError("Save file not found!");
//             return;
//         }

//         currentSlot = slot;
//         string json = File.ReadAllText(path);
//         SaveData data = JsonUtility.FromJson<SaveData>(json);

//         SceneManager.LoadScene("Town");
//         StartCoroutine(ApplyAfterTownLoad(data));
//     }

//     private IEnumerator ApplyAfterTownLoad(SaveData data)
//     {
//         while (SceneManager.GetActiveScene().name != "Town")
//             yield return null;
//         yield return new WaitForEndOfFrame();

//         // Health
//         if (PlayerHealth.Instance)
//         {
//             PlayerHealth.Instance.SetHealth(data.health);
//             PlayerHealth.Instance.SetEnergy(data.energy);
//             PlayerHealth.Instance.SetHunger(data.hunger);
//         }

//         // Money
//         if (MoneyManager.Instance)
//         {
//             MoneyManager.Instance.SetMoney(data.money);
//         }

//         // Fixed spawn position
//         var player = GameObject.FindGameObjectWithTag("Player");
//         if (player) player.transform.position = new Vector3(0.21f, -2.93f, 0f);

//         // Inventory
//         if (Inventory.Instance)
//         {
//             Inventory.Instance.items.Clear();
//             foreach (var id in data.inventoryItems)
//             {
//                 if (id != null)
//                 {
//                     var item = GetItemByName(id.itemName);
//                     Inventory.Instance.items.Add(new Inventory.ItemStack { item = item, stackSize = id.stackSize });
//                 }
//                 else
//                 {
//                     Inventory.Instance.items.Add(new Inventory.ItemStack { item = null, stackSize = 0 });
//                 }
//             }

//             if (data.selectedItemIndex >= 0 && data.selectedItemIndex < Inventory.Instance.items.Count)
//                 Inventory.Instance.SelectItem(data.selectedItemIndex);

//             InventoryUI.Instance?.UpdateInventoryUI();
//         }

//         // Calendar & time
//         if (CalendarManager.Instance && Enum.TryParse(data.currentMonth, out CalendarManager.Month m))
//             CalendarManager.Instance.SetDate(m, data.currentDay);
        
//         var clock = FindObjectOfType<WorldClock>();
//         if (clock) clock.SetTimeOfDay(data.timeOfDay);

//         // Spawn placed items for Town
//         SpawnPlacedItemsForCurrentScene(data);

//         sessionStartTime = Time.time - data.totalPlayTime;
//         Debug.Log("LOAD COMPLETE");
//     }

//     // ================================================================
//     // Called by PlacedItemLoader when entering any scene
//     // ================================================================
//     public void LoadPlacedItemsForCurrentScene()
//     {
//         if (currentSlot < 0)
//         {
//             Debug.Log("No active save slot, skipping placed items load");
//             return;
//         }

//         string path = GetSavePath(currentSlot);
//         if (!File.Exists(path))
//         {
//             Debug.LogWarning($"Save file doesn't exist at: {path}");
//             return;
//         }

//         try
//         {
//             SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
//             SpawnPlacedItemsForCurrentScene(data);
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"Failed to load placed items: {e.Message}");
//         }
//     }

//     private void SpawnPlacedItemsForCurrentScene(SaveData data)
//     {
//         if (data == null || data.placedItems == null) return;

//         string current = SceneManager.GetActiveScene().name;

//         // Destroy existing placed items
//         PlacedItem[] existingItems = FindObjectsOfType<PlacedItem>(true);
//         foreach (var p in existingItems)
//         {
//             if (p != null && p.gameObject != null && p.gameObject.scene.name == current)
//                 Destroy(p.gameObject);
//         }

//         int spawned = 0;
        
//         foreach (var pd in data.placedItems)
//         {
//             if (pd == null || pd.sceneName != current) continue;

//             Item item = GetItemByName(pd.itemName);
//             if (item == null) continue;

//             try
//             {
//                 GameObject go = new GameObject($"Placed_{item.itemName}");
//                 go.transform.position = pd.position;

//                 PlacedItem pi = go.AddComponent<PlacedItem>();
//                 pi.Initialize(item, pd.gridPosition);

//                 // Set sorting layer
//                 SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
//                 if (sr != null)
//                 {
//                     sr.sortingLayerName = "Default";
//                     sr.sortingOrder = 6;
//                 }

//                 spawned++;
//             }
//             catch (Exception e)
//             {
//                 Debug.LogError($"Error spawning {pd.itemName}: {e.Message}");
//             }
//         }

//         Debug.Log($"SPAWNED {spawned} placed items in {current}");
//     }

//     private Item GetItemByName(string name)
//     {
//         if (string.IsNullOrEmpty(name)) return null;
        
//         Item[] allItems = Resources.LoadAll<Item>("Items");
//         return allItems.FirstOrDefault(i => i != null && i.itemName == name);
//     }

//     private string GetSavePath(int slot) => saveDirectory + $"save_{slot}.json";

//     // ================================================================
//     // Public helpers
//     // ================================================================
//     public void AutoSave()
//     {
//         if (currentSlot >= 0)
//         {
//             SaveGame(currentSlot);
//             Debug.Log("Auto-saved");
//         }
//     }

//     public void DeleteSave(int slot)
//     {
//         var p = GetSavePath(slot);
//         if (File.Exists(p)) File.Delete(p);
//     }

//     public bool SaveExists(int slot) => File.Exists(GetSavePath(slot));

//     public SaveData GetSaveData(int slot)
//     {
//         if (!SaveExists(slot)) return null;
//         try
//         {
//             return JsonUtility.FromJson<SaveData>(File.ReadAllText(GetSavePath(slot)));
//         }
//         catch
//         {
//             return null;
//         }
//     }

//     public int GetCurrentSlot() => currentSlot;
// }


/////friends////
// using System;
// using System.IO;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using WorldTime;
// using System.Collections;
// using System.Linq;

// [System.Serializable]
// public class SaveData
// {
//     public int health, energy, hunger;
//     public Vector3 playerPosition;
//     public string currentScene;

//     public List<InventoryItemData> inventoryItems = new List<InventoryItemData>();
//     public int selectedItemIndex = -1;

//     public string currentMonth;
//     public int currentDay;
//     public float timeOfDay;

//     public List<PlacedItemData> placedItems = new List<PlacedItemData>();

//     // Money
//     public int money;

//     // Friendship data
//     public List<FriendshipData> friendshipData = new List<FriendshipData>();

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

// [System.Serializable]
// public class FriendshipData
// {
//     public string npcName;
//     public int friendshipPoints;
//     public string lastGiftDate; // "Month-Day" format
// }

// public class SaveSystem : MonoBehaviour
// {
//     public static SaveSystem Instance;

//     private string saveDirectory;
//     private int currentSlot = -1;
//     private float sessionStartTime;

//     [Header("Auto-Save")]
//     [SerializeField] private float autoSaveInterval = 300f;
//     private float timeSinceLastSave = 0f;

//     void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);

//             saveDirectory = Application.persistentDataPath + "/Saves/";
//             if (!Directory.Exists(saveDirectory))
//                 Directory.CreateDirectory(saveDirectory);

//             sessionStartTime = Time.time;
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }

//     void Update()
//     {
//         timeSinceLastSave += Time.deltaTime;
//         if (timeSinceLastSave >= autoSaveInterval && currentSlot >= 0)
//         {
//             AutoSave();
//             timeSinceLastSave = 0f;
//         }

//         if (Input.GetKeyDown(KeyCode.F5))
//         {
//             AutoSave();
//             Debug.Log("Manual save (F5)");
//         }
//     }

//     private void OnApplicationQuit() => AutoSave();

//     // ================================================================
//     // SAVE
//     // ================================================================
//     public void SaveGame(int slot, string saveName = null)
//     {
//         currentSlot = slot;
//         SaveData data = new SaveData();

//         // Player stats
//         if (PlayerHealth.Instance)
//         {
//             data.health = PlayerHealth.Instance.CurrentHealth;
//             data.energy = PlayerHealth.Instance.CurrentEnergy;
//             data.hunger = PlayerHealth.Instance.CurrentHunger;
//         }

//         // Money
//         if (MoneyManager.Instance)
//         {
//             data.money = MoneyManager.Instance.GetCurrentMoney();
//         }

//         // Player position & current scene
//         var player = GameObject.FindGameObjectWithTag("Player");
//         if (player) data.playerPosition = player.transform.position;
//         data.currentScene = SceneManager.GetActiveScene().name;

//         // Inventory
//         if (Inventory.Instance)
//         {
//             data.inventoryItems.Clear();
//             foreach (var stack in Inventory.Instance.items)
//             {
//                 data.inventoryItems.Add(stack?.item != null
//                     ? new InventoryItemData { itemName = stack.item.itemName, stackSize = stack.stackSize }
//                     : null);
//             }

//             var selected = Inventory.Instance.GetSelectedItem();
//             data.selectedItemIndex = Inventory.Instance.items.FindIndex(s => s?.item == selected);
//         }

//         // Time / Calendar
//         if (CalendarManager.Instance)
//         {
//             data.currentMonth = CalendarManager.Instance.CurrentMonth.ToString();
//             data.currentDay = CalendarManager.Instance.CurrentDay;
//         }
//         var clock = FindObjectOfType<WorldClock>();
//         if (clock) data.timeOfDay = clock.CurrentTimeOfDay;

//         // Friendship data
//         if (FriendshipManager.Instance)
//         {
//             data.friendshipData.Clear();
//             var friendshipDict = FriendshipManager.Instance.GetFriendshipData();
//             var lastGiftDates = FriendshipManager.Instance.GetLastGiftDates();
            
//             foreach (var kvp in friendshipDict)
//             {
//                 string lastDate = lastGiftDates.ContainsKey(kvp.Key) ? lastGiftDates[kvp.Key] : "";
                
//                 data.friendshipData.Add(new FriendshipData
//                 {
//                     npcName = kvp.Key,
//                     friendshipPoints = kvp.Value,
//                     lastGiftDate = lastDate
//                 });
//             }
//             Debug.Log($"Saved friendship data for {data.friendshipData.Count} NPCs");
//         }

//         // === PLACED ITEMS (merge across all scenes) ===
//         List<PlacedItemData> merged = new List<PlacedItemData>();
//         string currentSceneName = SceneManager.GetActiveScene().name;

//         // Load existing save and keep items from OTHER scenes
//         string path = GetSavePath(slot);
//         if (File.Exists(path))
//         {
//             try
//             {
//                 SaveData old = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
//                 if (old.placedItems != null)
//                 {
//                     foreach (var p in old.placedItems)
//                     {
//                         if (p != null && p.sceneName != currentSceneName)
//                             merged.Add(p);
//                     }
//                 }
//             }
//             catch (Exception e)
//             {
//                 Debug.LogWarning($"Could not load previous save data: {e.Message}");
//             }
//         }

//         // Add current scene items (they are still alive here)
//         PlacedItem[] currentSceneItems = FindObjectsOfType<PlacedItem>(true);
//         foreach (var item in currentSceneItems)
//         {
//             if (item != null && item.itemData != null)
//             {
//                 merged.Add(new PlacedItemData
//                 {
//                     itemName = item.itemData.itemName,
//                     position = item.transform.position,
//                     sceneName = currentSceneName,
//                     gridPosition = item.gridPosition
//                 });
//             }
//         }

//         data.placedItems = merged;

//         // Final metadata
//         data.saveName = saveName ?? $"Save {slot + 1}";
//         data.lastSaveTime = DateTime.Now;
//         data.totalPlayTime = Time.time - sessionStartTime;

//         File.WriteAllText(path, JsonUtility.ToJson(data, true));
//         Debug.Log($"SAVED slot {slot} – Money: ${data.money}, Time: {data.timeOfDay:F2}, Friendships: {data.friendshipData.Count}");
//     }

//     // ================================================================
//     // LOAD
//     // ================================================================
//     public void LoadGame(int slot)
//     {
//         string path = GetSavePath(slot);
//         if (!File.Exists(path))
//         {
//             Debug.LogError("Save file not found!");
//             return;
//         }

//         currentSlot = slot;
//         string json = File.ReadAllText(path);
//         SaveData data = JsonUtility.FromJson<SaveData>(json);

//         SceneManager.LoadScene("Town");
//         StartCoroutine(ApplyAfterTownLoad(data));
//     }

//     private IEnumerator ApplyAfterTownLoad(SaveData data)
//     {
//         while (SceneManager.GetActiveScene().name != "Town")
//             yield return null;
//         yield return new WaitForEndOfFrame();

//         // Health
//         if (PlayerHealth.Instance)
//         {
//             PlayerHealth.Instance.SetHealth(data.health);
//             PlayerHealth.Instance.SetEnergy(data.energy);
//             PlayerHealth.Instance.SetHunger(data.hunger);
//         }

//         // Money
//         if (MoneyManager.Instance)
//         {
//             MoneyManager.Instance.SetMoney(data.money);
//         }

//         // Fixed spawn position
//         var player = GameObject.FindGameObjectWithTag("Player");
//         if (player) player.transform.position = new Vector3(0.21f, -2.93f, 0f);

//         // Inventory
//         if (Inventory.Instance)
//         {
//             Inventory.Instance.items.Clear();
//             foreach (var id in data.inventoryItems)
//             {
//                 if (id != null)
//                 {
//                     var item = GetItemByName(id.itemName);
//                     Inventory.Instance.items.Add(new Inventory.ItemStack { item = item, stackSize = id.stackSize });
//                 }
//                 else
//                 {
//                     Inventory.Instance.items.Add(new Inventory.ItemStack { item = null, stackSize = 0 });
//                 }
//             }

//             if (data.selectedItemIndex >= 0 && data.selectedItemIndex < Inventory.Instance.items.Count)
//                 Inventory.Instance.SelectItem(data.selectedItemIndex);

//             InventoryUI.Instance?.UpdateInventoryUI();
//         }

//         // Calendar & time
//         if (CalendarManager.Instance && Enum.TryParse(data.currentMonth, out CalendarManager.Month m))
//             CalendarManager.Instance.SetDate(m, data.currentDay);
        
//         var clock = FindObjectOfType<WorldClock>();
//         if (clock) clock.SetTimeOfDay(data.timeOfDay);

//         // Friendship data
//         if (FriendshipManager.Instance && data.friendshipData != null)
//         {
//             Dictionary<string, int> friendshipDict = new Dictionary<string, int>();
//             Dictionary<string, string> lastGiftDict = new Dictionary<string, string>();
            
//             foreach (var fd in data.friendshipData)
//             {
//                 if (fd != null && !string.IsNullOrEmpty(fd.npcName))
//                 {
//                     friendshipDict[fd.npcName] = fd.friendshipPoints;
                    
//                     if (!string.IsNullOrEmpty(fd.lastGiftDate))
//                     {
//                         lastGiftDict[fd.npcName] = fd.lastGiftDate;
//                     }
//                 }
//             }
            
//             FriendshipManager.Instance.LoadFriendshipData(friendshipDict);
//             FriendshipManager.Instance.LoadLastGiftDates(lastGiftDict);
//             Debug.Log($"Loaded friendship data for {friendshipDict.Count} NPCs");
//         }

//         // Spawn placed items for Town
//         SpawnPlacedItemsForCurrentScene(data);

//         sessionStartTime = Time.time - data.totalPlayTime;
//         Debug.Log("LOAD COMPLETE");
//     }

//     // ================================================================
//     // Called by PlacedItemLoader when entering any scene
//     // ================================================================
//     public void LoadPlacedItemsForCurrentScene()
//     {
//         if (currentSlot < 0)
//         {
//             Debug.Log("No active save slot, skipping placed items load");
//             return;
//         }

//         string path = GetSavePath(currentSlot);
//         if (!File.Exists(path))
//         {
//             Debug.LogWarning($"Save file doesn't exist at: {path}");
//             return;
//         }

//         try
//         {
//             SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
//             SpawnPlacedItemsForCurrentScene(data);
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"Failed to load placed items: {e.Message}");
//         }
//     }

//     private void SpawnPlacedItemsForCurrentScene(SaveData data)
//     {
//         if (data == null || data.placedItems == null) return;

//         string current = SceneManager.GetActiveScene().name;

//         // Destroy existing placed items
//         PlacedItem[] existingItems = FindObjectsOfType<PlacedItem>(true);
//         foreach (var p in existingItems)
//         {
//             if (p != null && p.gameObject != null && p.gameObject.scene.name == current)
//                 Destroy(p.gameObject);
//         }

//         int spawned = 0;
        
//         foreach (var pd in data.placedItems)
//         {
//             if (pd == null || pd.sceneName != current) continue;

//             Item item = GetItemByName(pd.itemName);
//             if (item == null) continue;

//             try
//             {
//                 GameObject go = new GameObject($"Placed_{item.itemName}");
//                 go.transform.position = pd.position;

//                 PlacedItem pi = go.AddComponent<PlacedItem>();
//                 pi.Initialize(item, pd.gridPosition);

//                 // Set sorting layer
//                 SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
//                 if (sr != null)
//                 {
//                     sr.sortingLayerName = "Default";
//                     sr.sortingOrder = 6;
//                 }

//                 spawned++;
//             }
//             catch (Exception e)
//             {
//                 Debug.LogError($"Error spawning {pd.itemName}: {e.Message}");
//             }
//         }

//         Debug.Log($"SPAWNED {spawned} placed items in {current}");
//     }

//     private Item GetItemByName(string name)
//     {
//         if (string.IsNullOrEmpty(name)) return null;
        
//         Item[] allItems = Resources.LoadAll<Item>("Items");
//         return allItems.FirstOrDefault(i => i != null && i.itemName == name);
//     }

//     private string GetSavePath(int slot) => saveDirectory + $"save_{slot}.json";

//     // ================================================================
//     // Public helpers
//     // ================================================================
//     public void AutoSave()
//     {
//         if (currentSlot >= 0)
//         {
//             SaveGame(currentSlot);
//             Debug.Log("Auto-saved");
//         }
//     }

//     public void DeleteSave(int slot)
//     {
//         var p = GetSavePath(slot);
//         if (File.Exists(p)) File.Delete(p);
//     }

//     public bool SaveExists(int slot) => File.Exists(GetSavePath(slot));

//     public SaveData GetSaveData(int slot)
//     {
//         if (!SaveExists(slot)) return null;
//         try
//         {
//             return JsonUtility.FromJson<SaveData>(File.ReadAllText(GetSavePath(slot)));
//         }
//         catch
//         {
//             return null;
//         }
//     }

//     public int GetCurrentSlot() => currentSlot;
// }



























// using System;
// using System.IO;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using WorldTime;
// using System.Collections;
// using System.Linq;

// [System.Serializable]
// public class SaveData
// {
//     public int health, energy, hunger;
//     public Vector3 playerPosition;
//     public string currentScene;

//     public List<InventoryItemData> inventoryItems = new List<InventoryItemData>();
//     public int selectedItemIndex = -1;

//     public string currentMonth;
//     public int currentDay;
//     public float timeOfDay;

//     public List<PlacedItemData> placedItems = new List<PlacedItemData>();

//     // Money
//     public int money;

//     // Friendship data
//     public List<FriendshipData> friendshipData = new List<FriendshipData>();

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

// [System.Serializable]
// public class FriendshipData
// {
//     public string npcName;
//     public int friendshipPoints;
// }

// public class SaveSystem : MonoBehaviour
// {
//     public static SaveSystem Instance;

//     private string saveDirectory;
//     private int currentSlot = -1;
//     private float sessionStartTime;

//     [Header("Auto-Save")]
//     [SerializeField] private float autoSaveInterval = 300f;
//     private float timeSinceLastSave = 0f;

//     void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);

//             saveDirectory = Application.persistentDataPath + "/Saves/";
//             if (!Directory.Exists(saveDirectory))
//                 Directory.CreateDirectory(saveDirectory);

//             sessionStartTime = Time.time;
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }

//     void Update()
//     {
//         timeSinceLastSave += Time.deltaTime;
//         if (timeSinceLastSave >= autoSaveInterval && currentSlot >= 0)
//         {
//             AutoSave();
//             timeSinceLastSave = 0f;
//         }

//         if (Input.GetKeyDown(KeyCode.F5))
//         {
//             AutoSave();
//             Debug.Log("Manual save (F5)");
//         }
//     }

//     private void OnApplicationQuit() => AutoSave();

//     // ================================================================
//     // SAVE
//     // ================================================================
//     public void SaveGame(int slot, string saveName = null)
//     {
//         currentSlot = slot;
//         SaveData data = new SaveData();

//         // Player stats
//         if (PlayerHealth.Instance)
//         {
//             data.health = PlayerHealth.Instance.CurrentHealth;
//             data.energy = PlayerHealth.Instance.CurrentEnergy;
//             data.hunger = PlayerHealth.Instance.CurrentHunger;
//         }

//         // Money
//         if (MoneyManager.Instance)
//         {
//             data.money = MoneyManager.Instance.GetCurrentMoney();
//         }

//         // Player position & current scene
//         var player = GameObject.FindGameObjectWithTag("Player");
//         if (player) data.playerPosition = player.transform.position;
//         data.currentScene = SceneManager.GetActiveScene().name;

//         // Inventory
//         if (Inventory.Instance)
//         {
//             data.inventoryItems.Clear();
//             foreach (var stack in Inventory.Instance.items)
//             {
//                 data.inventoryItems.Add(stack?.item != null
//                     ? new InventoryItemData { itemName = stack.item.itemName, stackSize = stack.stackSize }
//                     : null);
//             }

//             var selected = Inventory.Instance.GetSelectedItem();
//             data.selectedItemIndex = Inventory.Instance.items.FindIndex(s => s?.item == selected);
//         }

//         // Time / Calendar
//         if (CalendarManager.Instance)
//         {
//             data.currentMonth = CalendarManager.Instance.CurrentMonth.ToString();
//             data.currentDay = CalendarManager.Instance.CurrentDay;
//         }
//         var clock = FindObjectOfType<WorldClock>();
//         if (clock) data.timeOfDay = clock.CurrentTimeOfDay;

//         // Friendship data
//         if (FriendshipManager.Instance)
//         {
//             data.friendshipData.Clear();
//             var friendshipDict = FriendshipManager.Instance.GetFriendshipData();
//             foreach (var kvp in friendshipDict)
//             {
//                 data.friendshipData.Add(new FriendshipData
//                 {
//                     npcName = kvp.Key,
//                     friendshipPoints = kvp.Value
//                 });
//             }
//             Debug.Log($"Saved friendship data for {data.friendshipData.Count} NPCs");
//         }

//         // === PLACED ITEMS (merge across all scenes) ===
//         List<PlacedItemData> merged = new List<PlacedItemData>();
//         string currentSceneName = SceneManager.GetActiveScene().name;

//         // Load existing save and keep items from OTHER scenes
//         string path = GetSavePath(slot);
//         if (File.Exists(path))
//         {
//             try
//             {
//                 SaveData old = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
//                 if (old.placedItems != null)
//                 {
//                     foreach (var p in old.placedItems)
//                     {
//                         if (p != null && p.sceneName != currentSceneName)
//                             merged.Add(p);
//                     }
//                 }
//             }
//             catch (Exception e)
//             {
//                 Debug.LogWarning($"Could not load previous save data: {e.Message}");
//             }
//         }

//         // Add current scene items (they are still alive here)
//         PlacedItem[] currentSceneItems = FindObjectsOfType<PlacedItem>(true);
//         foreach (var item in currentSceneItems)
//         {
//             if (item != null && item.itemData != null)
//             {
//                 merged.Add(new PlacedItemData
//                 {
//                     itemName = item.itemData.itemName,
//                     position = item.transform.position,
//                     sceneName = currentSceneName,
//                     gridPosition = item.gridPosition
//                 });
//             }
//         }

//         data.placedItems = merged;

//         // Final metadata
//         data.saveName = saveName ?? $"Save {slot + 1}";
//         data.lastSaveTime = DateTime.Now;
//         data.totalPlayTime = Time.time - sessionStartTime;

//         File.WriteAllText(path, JsonUtility.ToJson(data, true));
//         Debug.Log($"SAVED slot {slot} – Money: ${data.money}, Time: {data.timeOfDay:F2}, Friendships: {data.friendshipData.Count}");
//     }

//     // ================================================================
//     // LOAD
//     // ================================================================
//     public void LoadGame(int slot)
//     {
//         string path = GetSavePath(slot);
//         if (!File.Exists(path))
//         {
//             Debug.LogError("Save file not found!");
//             return;
//         }

//         currentSlot = slot;
//         string json = File.ReadAllText(path);
//         SaveData data = JsonUtility.FromJson<SaveData>(json);

//         SceneManager.LoadScene("Town");
//         StartCoroutine(ApplyAfterTownLoad(data));
//     }

//     private IEnumerator ApplyAfterTownLoad(SaveData data)
//     {
//         while (SceneManager.GetActiveScene().name != "Town")
//             yield return null;
//         yield return new WaitForEndOfFrame();

//         // Health
//         if (PlayerHealth.Instance)
//         {
//             PlayerHealth.Instance.SetHealth(data.health);
//             PlayerHealth.Instance.SetEnergy(data.energy);
//             PlayerHealth.Instance.SetHunger(data.hunger);
//         }

//         // Money
//         if (MoneyManager.Instance)
//         {
//             MoneyManager.Instance.SetMoney(data.money);
//         }

//         // Fixed spawn position
//         var player = GameObject.FindGameObjectWithTag("Player");
//         if (player) player.transform.position = new Vector3(0.21f, -2.93f, 0f);

//         // Inventory
//         if (Inventory.Instance)
//         {
//             Inventory.Instance.items.Clear();
//             foreach (var id in data.inventoryItems)
//             {
//                 if (id != null)
//                 {
//                     var item = GetItemByName(id.itemName);
//                     Inventory.Instance.items.Add(new Inventory.ItemStack { item = item, stackSize = id.stackSize });
//                 }
//                 else
//                 {
//                     Inventory.Instance.items.Add(new Inventory.ItemStack { item = null, stackSize = 0 });
//                 }
//             }

//             if (data.selectedItemIndex >= 0 && data.selectedItemIndex < Inventory.Instance.items.Count)
//                 Inventory.Instance.SelectItem(data.selectedItemIndex);

//             InventoryUI.Instance?.UpdateInventoryUI();
//         }

//         // Calendar & time
//         if (CalendarManager.Instance && Enum.TryParse(data.currentMonth, out CalendarManager.Month m))
//             CalendarManager.Instance.SetDate(m, data.currentDay);
        
//         var clock = FindObjectOfType<WorldClock>();
//         if (clock) clock.SetTimeOfDay(data.timeOfDay);

//         // Friendship data
//         if (FriendshipManager.Instance && data.friendshipData != null)
//         {
//             Dictionary<string, int> friendshipDict = new Dictionary<string, int>();
//             foreach (var fd in data.friendshipData)
//             {
//                 if (fd != null && !string.IsNullOrEmpty(fd.npcName))
//                 {
//                     friendshipDict[fd.npcName] = fd.friendshipPoints;
//                 }
//             }
//             FriendshipManager.Instance.LoadFriendshipData(friendshipDict);
//             Debug.Log($"Loaded friendship data for {friendshipDict.Count} NPCs");
//         }

//         // Spawn placed items for Town
//         SpawnPlacedItemsForCurrentScene(data);

//         sessionStartTime = Time.time - data.totalPlayTime;
//         Debug.Log("LOAD COMPLETE");
//     }

//     // ================================================================
//     // Called by PlacedItemLoader when entering any scene
//     // ================================================================
//     public void LoadPlacedItemsForCurrentScene()
//     {
//         if (currentSlot < 0)
//         {
//             Debug.Log("No active save slot, skipping placed items load");
//             return;
//         }

//         string path = GetSavePath(currentSlot);
//         if (!File.Exists(path))
//         {
//             Debug.LogWarning($"Save file doesn't exist at: {path}");
//             return;
//         }

//         try
//         {
//             SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
//             SpawnPlacedItemsForCurrentScene(data);
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"Failed to load placed items: {e.Message}");
//         }
//     }

//     private void SpawnPlacedItemsForCurrentScene(SaveData data)
//     {
//         if (data == null || data.placedItems == null) return;

//         string current = SceneManager.GetActiveScene().name;

//         // Destroy existing placed items
//         PlacedItem[] existingItems = FindObjectsOfType<PlacedItem>(true);
//         foreach (var p in existingItems)
//         {
//             if (p != null && p.gameObject != null && p.gameObject.scene.name == current)
//                 Destroy(p.gameObject);
//         }

//         int spawned = 0;
        
//         foreach (var pd in data.placedItems)
//         {
//             if (pd == null || pd.sceneName != current) continue;

//             Item item = GetItemByName(pd.itemName);
//             if (item == null) continue;

//             try
//             {
//                 GameObject go = new GameObject($"Placed_{item.itemName}");
//                 go.transform.position = pd.position;

//                 PlacedItem pi = go.AddComponent<PlacedItem>();
//                 pi.Initialize(item, pd.gridPosition);

//                 // Set sorting layer
//                 SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
//                 if (sr != null)
//                 {
//                     sr.sortingLayerName = "Default";
//                     sr.sortingOrder = 6;
//                 }

//                 spawned++;
//             }
//             catch (Exception e)
//             {
//                 Debug.LogError($"Error spawning {pd.itemName}: {e.Message}");
//             }
//         }

//         Debug.Log($"SPAWNED {spawned} placed items in {current}");
//     }

//     private Item GetItemByName(string name)
//     {
//         if (string.IsNullOrEmpty(name)) return null;
        
//         Item[] allItems = Resources.LoadAll<Item>("Items");
//         return allItems.FirstOrDefault(i => i != null && i.itemName == name);
//     }

//     private string GetSavePath(int slot) => saveDirectory + $"save_{slot}.json";

//     // ================================================================
//     // Public helpers
//     // ================================================================
//     public void AutoSave()
//     {
//         if (currentSlot >= 0)
//         {
//             SaveGame(currentSlot);
//             Debug.Log("Auto-saved");
//         }
//     }

//     public void DeleteSave(int slot)
//     {
//         var p = GetSavePath(slot);
//         if (File.Exists(p)) File.Delete(p);
//     }

//     public bool SaveExists(int slot) => File.Exists(GetSavePath(slot));

//     public SaveData GetSaveData(int slot)
//     {
//         if (!SaveExists(slot)) return null;
//         try
//         {
//             return JsonUtility.FromJson<SaveData>(File.ReadAllText(GetSavePath(slot)));
//         }
//         catch
//         {
//             return null;
//         }
//     }

//     public int GetCurrentSlot() => currentSlot;
// }
