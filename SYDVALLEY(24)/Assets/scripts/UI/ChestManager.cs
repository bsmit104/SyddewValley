using UnityEngine;
using UnityEngine.SceneManagement;

public class ChestManager : MonoBehaviour
{
    public static ChestManager Instance { get; private set; }
    
    [SerializeField] private GameObject chestCanvas;
    [SerializeField] private ChestInventoryUI chestInventoryUI;
    private Chest currentChest;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Make the chest canvas persistent too
            if (chestCanvas != null)
            {
                DontDestroyOnLoad(chestCanvas);
            }
            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (chestCanvas == null || chestInventoryUI == null)
        {
            Debug.LogError("ChestManager: Required components not assigned!");
            return;
        }
        chestCanvas.SetActive(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Close any open chest when changing scenes
        if (currentChest != null)
        {
            CloseChest();
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public void OpenChest(Chest chest)
    {
        // If we're already looking at a chest, close it first
        if (currentChest != null && currentChest != chest)
        {
            CloseChest();
        }

        currentChest = chest;
        chestInventoryUI.chestInventory = chest.ChestInventory;
        chestCanvas.SetActive(true);
        
        // Force UI update to show the correct inventory
        chestInventoryUI.UpdateChestInventoryUI();
    }

    public void CloseChest()
    {
        if (currentChest != null)
        {
            currentChest = null;
            chestInventoryUI.chestInventory = null;
            chestCanvas.SetActive(false);
            
            // Clear the UI
            chestInventoryUI.UpdateChestInventoryUI();
        }
    }

    public bool IsChestOpen() => chestCanvas.activeSelf;
}

// using UnityEngine;

// public class ChestManager : MonoBehaviour
// {
//     public static ChestManager Instance { get; private set; }
    
//     [SerializeField] private GameObject chestCanvas;
//     [SerializeField] private ChestInventoryUI chestInventoryUI;
//     private Chest currentChest;

//     void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }

//     void Start()
//     {
//         if (chestCanvas == null || chestInventoryUI == null)
//         {
//             Debug.LogError("ChestManager: Required components not assigned!");
//             return;
//         }
//         chestCanvas.SetActive(false);
//     }

//     void OnDestroy()
//     {
//         if (Instance == this)
//         {
//             Instance = null;
//         }
//     }

//     void OnApplicationQuit()
//     {
//         if (Instance == this)
//         {
//             Instance = null;
//         }
//     }

//     public void OpenChest(Chest chest)
//     {
//         // If we're already looking at a chest, close it first
//         if (currentChest != null && currentChest != chest)
//         {
//             CloseChest();
//         }

//         currentChest = chest;
//         chestInventoryUI.chestInventory = chest.ChestInventory;
//         chestCanvas.SetActive(true);
        
//         // Force UI update to show the correct inventory
//         chestInventoryUI.UpdateChestInventoryUI();
//     }

//     public void CloseChest()
//     {
//         if (currentChest != null)
//         {
//             currentChest = null;
//             chestInventoryUI.chestInventory = null;
//             chestCanvas.SetActive(false);
            
//             // Clear the UI
//             chestInventoryUI.UpdateChestInventoryUI();
//         }
//     }

//     public bool IsChestOpen() => chestCanvas.activeSelf;
// } 