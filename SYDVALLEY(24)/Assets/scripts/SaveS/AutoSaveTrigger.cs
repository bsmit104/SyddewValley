using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSaveTrigger : MonoBehaviour
{
    [Header("Auto-Save Settings")]
    [SerializeField] private bool saveOnSceneChange = true;
    [SerializeField] private float autoSaveInterval = 300f;
    [SerializeField] private bool showSaveNotification = true;
    
    private float timeSinceLastSave = 0f;

    void Update()
    {
        if (autoSaveInterval > 0)
        {
            timeSinceLastSave += Time.deltaTime;
            
            if (timeSinceLastSave >= autoSaveInterval)
            {
                SaveGame();
                timeSinceLastSave = 0f;
            }
        }
        
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveGame();
            Debug.Log("Manual save triggered (F5)");
        }
    }

    private void OnDestroy()
    {
        // Don't save when destroying in menu scenes
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "MainMenu" || sceneName == "Menu")
            return;
        
        if (saveOnSceneChange)
        {
            Debug.Log($"Auto-saving before destroying/unloading {sceneName}");
            SaveGame();
        }
    }

    private void SaveGame()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.AutoSave();
            
            if (showSaveNotification)
            {
                ShowSaveNotification();
            }
        }
    }

    private void ShowSaveNotification()
    {
        Debug.Log("✓ Game Saved");
    }
}



// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class AutoSaveTrigger : MonoBehaviour
// {
//     [Header("Auto-Save Settings")]
//     [SerializeField] private bool saveOnSceneChange = true;
//     [SerializeField] private float autoSaveInterval = 300f;
//     [SerializeField] private bool showSaveNotification = true;
    
//     private float timeSinceLastSave = 0f;
//     private bool isSavingBeforeSceneChange = false;

//     void Start()
//     {
//         if (saveOnSceneChange)
//         {
//             // Save BEFORE the scene unloads
//             SceneManager.sceneUnloaded += OnSceneUnloaded;
//         }
//     }

//     void Update()
//     {
//         if (autoSaveInterval > 0)
//         {
//             timeSinceLastSave += Time.deltaTime;
            
//             if (timeSinceLastSave >= autoSaveInterval)
//             {
//                 SaveGame();
//                 timeSinceLastSave = 0f;
//             }
//         }
        
//         if (Input.GetKeyDown(KeyCode.F5))
//         {
//             SaveGame();
//             Debug.Log("Manual save triggered (F5)");
//         }
//     }

//     private void OnSceneUnloaded(Scene scene)
//     {
//         // Don't save when unloading menu scenes
//         if (scene.name == "MainMenu" || scene.name == "Menu")
//             return;
        
//         Debug.Log($"Auto-saving before leaving {scene.name}");
//         SaveGame();
//     }

//     private void SaveGame()
//     {
//         if (SaveSystem.Instance != null)
//         {
//             SaveSystem.Instance.AutoSave();
            
//             if (showSaveNotification)
//             {
//                 ShowSaveNotification();
//             }
//         }
//     }

//     private void ShowSaveNotification()
//     {
//         Debug.Log("✓ Game Saved");
//     }

//     private void OnDestroy()
//     {
//         if (saveOnSceneChange)
//         {
//             SceneManager.sceneUnloaded -= OnSceneUnloaded;
//         }
//     }
// }
















// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class AutoSaveTrigger : MonoBehaviour
// {
//     [Header("Auto-Save Settings")]
//     [SerializeField] private bool saveOnSceneChange = true;
//     [SerializeField] private float autoSaveInterval = 300f; // Save every 5 minutes
//     [SerializeField] private bool showSaveNotification = true;
    
//     private float timeSinceLastSave = 0f;

//     void Start()
//     {
//         if (saveOnSceneChange)
//         {
//             SceneManager.sceneLoaded += OnSceneChanged;
//         }
//     }

//     void Update()
//     {
//         // Periodic auto-save
//         if (autoSaveInterval > 0)
//         {
//             timeSinceLastSave += Time.deltaTime;
            
//             if (timeSinceLastSave >= autoSaveInterval)
//             {
//                 SaveGame();
//                 timeSinceLastSave = 0f;
//             }
//         }
        
//         // Manual save with F5
//         if (Input.GetKeyDown(KeyCode.F5))
//         {
//             SaveGame();
//             Debug.Log("Manual save triggered (F5)");
//         }
//     }

//     private void OnSceneChanged(Scene scene, LoadSceneMode mode)
//     {
//         // Don't save when loading to menu
//         if (scene.name == "MainMenu" || scene.name == "Menu")
//             return;
        
//         SaveGame();
//     }

//     private void SaveGame()
//     {
//         if (SaveSystem.Instance != null)
//         {
//             SaveSystem.Instance.AutoSave();
            
//             if (showSaveNotification)
//             {
//                 ShowSaveNotification();
//             }
//         }
//     }

//     private void ShowSaveNotification()
//     {
//         // You can implement a UI notification here
//         Debug.Log("✓ Game Saved");
//     }

//     private void OnDestroy()
//     {
//         if (saveOnSceneChange)
//         {
//             SceneManager.sceneLoaded -= OnSceneChanged;
//         }
//     }
// }