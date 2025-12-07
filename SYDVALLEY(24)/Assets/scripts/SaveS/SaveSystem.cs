using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WorldTime;

[System.Serializable]
public class SaveData
{
    // Player Data
    public int health;
    public int energy;
    public int hunger;
    public Vector3 playerPosition;
    public string currentScene;
    
    // Inventory Data
    public List<InventoryItemData> inventoryItems = new List<InventoryItemData>();
    public int selectedItemIndex = -1;
    
    // World Data
    public string currentMonth;
    public int currentDay;
    public float timeOfDay; // 0-1, where 0 = midnight
    
    // Placed Items Data
    public List<PlacedItemData> placedItems = new List<PlacedItemData>();
    
    // Metadata
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
        
        // Save player data
        if (PlayerHealth.Instance != null)
        {
            data.health = PlayerHealth.Instance.CurrentHealth;
            data.energy = PlayerHealth.Instance.CurrentEnergy;
            data.hunger = PlayerHealth.Instance.CurrentHunger;
        }
        
        // Save player position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            data.playerPosition = player.transform.position;
        }
        
        // Save current scene
        data.currentScene = SceneManager.GetActiveScene().name;
        
        // Save inventory
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
                    data.inventoryItems.Add(null); // Preserve empty slots
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
        
        // Save calendar data
        if (CalendarManager.Instance != null)
        {
            data.currentMonth = CalendarManager.Instance.CurrentMonth.ToString();
            data.currentDay = CalendarManager.Instance.CurrentDay;
        }
        
        // Save time of day
        WorldClock clock = FindObjectOfType<WorldClock>();
        if (clock != null)
        {
            // We'll need to expose currentTimeOfDay in WorldClock
            data.timeOfDay = GetTimeOfDay(clock);
        }
        
        // Save all placed items across all scenes
        data.placedItems.Clear();
        PlacedItem[] allPlacedItems = FindObjectsOfType<PlacedItem>(true);
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
            }
        }
        
        // Save metadata
        data.saveName = saveName ?? $"Save {slot + 1}";
        data.lastSaveTime = DateTime.Now;
        data.totalPlayTime = Time.time - sessionStartTime;
        
        // Write to file
        string path = GetSavePath(slot);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        
        Debug.Log($"Game saved to slot {slot}: {path}");
    }

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
        
        // Load the saved scene first
        SceneManager.sceneLoaded += (scene, mode) => OnSceneLoadedForLoad(data);
        SceneManager.LoadScene(data.currentScene);
    }

    private void OnSceneLoadedForLoad(SaveData data)
    {
        SceneManager.sceneLoaded -= (scene, mode) => OnSceneLoadedForLoad(data);
        
        // Wait a frame for everything to initialize
        StartCoroutine(ApplyLoadedData(data));
    }

    private System.Collections.IEnumerator ApplyLoadedData(SaveData data)
    {
        yield return new WaitForEndOfFrame();
        
        // Restore player health
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.maxHealth = data.health; // Temp set max to restore value
            PlayerHealth.Instance.maxEnergy = data.energy;
            PlayerHealth.Instance.maxHunger = data.hunger;
            // You'll need to add public setters for these in PlayerHealth
        }
        
        // Restore player position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = data.playerPosition;
        }
        
        // Restore inventory
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
            
            Inventory.Instance.OnInventoryChanged?.Invoke();
        }
        
        // Restore calendar
        if (CalendarManager.Instance != null)
        {
            CalendarManager.Month month = (CalendarManager.Month)Enum.Parse(typeof(CalendarManager.Month), data.currentMonth);
            CalendarManager.Instance.SetDate(month, data.currentDay);
        }
        
        // Restore time of day
        WorldClock clock = FindObjectOfType<WorldClock>();
        if (clock != null)
        {
            SetTimeOfDay(clock, data.timeOfDay);
        }
        
        // Restore placed items for current scene
        string currentScene = SceneManager.GetActiveScene().name;
        foreach (var itemData in data.placedItems)
        {
            if (itemData.sceneName == currentScene)
            {
                Item item = GetItemByName(itemData.itemName);
                if (item != null)
                {
                    ItemPlacement placement = FindObjectOfType<ItemPlacement>();
                    if (placement != null)
                    {
                        // You'll need a public method to spawn items
                        SpawnPlacedItem(item, itemData.position, itemData.gridPosition);
                    }
                }
            }
        }
        
        sessionStartTime = Time.time - data.totalPlayTime;
        Debug.Log($"Game loaded from slot {currentSlot}");
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

    private Item GetItemByName(string itemName)
    {
        // Load all Item ScriptableObjects
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
        ItemPlacement placement = FindObjectOfType<ItemPlacement>();
        // You'll need to expose placedItemPrefab as public or add a spawn method
        GameObject prefab = GetPlacedItemPrefab();
        if (prefab != null)
        {
            GameObject obj = Instantiate(prefab, position, Quaternion.identity);
            PlacedItem placedItem = obj.GetComponent<PlacedItem>();
            if (placedItem != null)
            {
                placedItem.Initialize(item, gridPos);
            }
        }
    }

    private GameObject GetPlacedItemPrefab()
    {
        // You'll need to make this accessible
        ItemPlacement placement = FindObjectOfType<ItemPlacement>();
        // For now, load from Resources
        return Resources.Load<GameObject>("Prefabs/PlacedItem");
    }

    private float GetTimeOfDay(WorldClock clock)
    {
        // We'll need to expose this in WorldClock
        // For now return 0.25 (6 AM) as default
        return 0.25f;
    }

    private void SetTimeOfDay(WorldClock clock, float timeOfDay)
    {
        // We'll need to add a public method in WorldClock
    }

    public void AutoSave()
    {
        if (currentSlot >= 0)
        {
            SaveGame(currentSlot);
            Debug.Log("Auto-saved!");
        }
    }
}