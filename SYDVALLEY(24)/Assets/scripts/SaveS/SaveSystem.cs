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

    // NEW: Money
    public int money;

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
    public static SaveSystem Instance;

    private string saveDirectory;
    private int currentSlot = -1;
    private float sessionStartTime;

    [Header("Auto-Save")]
    [SerializeField] private float autoSaveInterval = 300f;
    private float timeSinceLastSave = 0f;

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

        // === PLACED ITEMS (merge across all scenes) ===
        List<PlacedItemData> merged = new List<PlacedItemData>();
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Load existing save and keep items from OTHER scenes
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

        // Add current scene items (they are still alive here)
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

        // Final metadata
        data.saveName = saveName ?? $"Save {slot + 1}";
        data.lastSaveTime = DateTime.Now;
        data.totalPlayTime = Time.time - sessionStartTime;

        File.WriteAllText(path, JsonUtility.ToJson(data, true));
        Debug.Log($"SAVED slot {slot} – Money: ${data.money}, Time: {data.timeOfDay:F2}");
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

        // Destroy existing placed items
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

                // Set sorting layer
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

/////////////so good//////////
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

//         // Final metadata
//         data.saveName = saveName ?? $"Save {slot + 1}";
//         data.lastSaveTime = DateTime.Now;
//         data.totalPlayTime = Time.time - sessionStartTime;

//         File.WriteAllText(path, JsonUtility.ToJson(data, true));
//         Debug.Log($"SAVED slot {slot} – {merged.Count} placed items total");
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
//         Debug.Log("=== SpawnPlacedItemsForCurrentScene called ===");
        
//         if (data == null)
//         {
//             Debug.LogError("SaveData is null!");
//             return;
//         }

//         if (data.placedItems == null)
//         {
//             Debug.LogWarning("placedItems list is null, initializing empty list");
//             data.placedItems = new List<PlacedItemData>();
//             return;
//         }

//         string current = SceneManager.GetActiveScene().name;
//         Debug.Log($"Current scene: {current}, Total placed items in save: {data.placedItems.Count}");

//         // Destroy any existing placed items in this scene first
//         try
//         {
//             PlacedItem[] existingItems = FindObjectsOfType<PlacedItem>(true);
//             foreach (var p in existingItems)
//             {
//                 if (p != null && p.gameObject != null && p.gameObject.scene.name == current)
//                 {
//                     Destroy(p.gameObject);
//                 }
//             }
//         }
//         catch (Exception e)
//         {
//             Debug.LogWarning($"Error destroying existing items: {e.Message}");
//         }

//         int spawned = 0;
//         int skipped = 0;
        
//         for (int i = 0; i < data.placedItems.Count; i++)
//         {
//             PlacedItemData pd = data.placedItems[i];
            
//             if (pd == null)
//             {
//                 Debug.LogWarning($"PlacedItemData at index {i} is null");
//                 skipped++;
//                 continue;
//             }

//             if (string.IsNullOrEmpty(pd.sceneName))
//             {
//                 Debug.LogWarning($"PlacedItemData at index {i} has null/empty sceneName");
//                 skipped++;
//                 continue;
//             }

//             if (pd.sceneName != current)
//             {
//                 skipped++;
//                 continue;
//             }

//             Debug.Log($"Attempting to spawn: {pd.itemName} at {pd.position}");

//             if (string.IsNullOrEmpty(pd.itemName))
//             {
//                 Debug.LogWarning($"PlacedItemData at index {i} has null/empty itemName");
//                 skipped++;
//                 continue;
//             }

//             Item item = GetItemByName(pd.itemName);
//             if (item == null)
//             {
//                 Debug.LogWarning($"Could not find item definition: {pd.itemName}");
//                 skipped++;
//                 continue;
//             }

//             try
//             {
//                 // CREATE THE PLACED ITEM
//                 GameObject go = new GameObject($"Placed_{item.itemName}");
//                 if (go == null)
//                 {
//                     Debug.LogError("Failed to create GameObject!");
//                     continue;
//                 }
                
//                 go.transform.position = pd.position;

//                 // Add PlacedItem component and initialize
//                 // Initialize() will call SetupVisuals() which adds SpriteRenderer and Collider
//                 PlacedItem pi = go.AddComponent<PlacedItem>();
//                 if (pi == null)
//                 {
//                     Debug.LogError("Failed to add PlacedItem component!");
//                     Destroy(go);
//                     continue;
//                 }
                
//                 pi.Initialize(item, pd.gridPosition);

//                 // Set sorting layer to make items visible above ground
//                 SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
//                 if (sr != null)
//                 {
//                     sr.sortingLayerName = "Default"; // or whatever your layer is named
//                     sr.sortingOrder = 6; // Render above tilemap
//                 }

//                 spawned++;
//                 Debug.Log($"Successfully spawned: {item.itemName}");
//             }
//             catch (Exception e)
//             {
//                 Debug.LogError($"Error spawning {pd.itemName}: {e.Message}\nStack: {e.StackTrace}");
//                 skipped++;
//             }
//         }

//         Debug.Log($"=== SPAWNING COMPLETE: {spawned} spawned, {skipped} skipped in {current} ===");
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
// }
