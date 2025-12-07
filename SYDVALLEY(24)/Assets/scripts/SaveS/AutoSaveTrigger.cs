using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSaveTrigger : MonoBehaviour
{
    [Header("Auto-Save Settings")]
    [SerializeField] private bool saveOnSceneChange = true;
    [SerializeField] private float autoSaveInterval = 300f; // Save every 5 minutes
    [SerializeField] private bool showSaveNotification = true;
    
    private float timeSinceLastSave = 0f;

    void Start()
    {
        if (saveOnSceneChange)
        {
            SceneManager.sceneLoaded += OnSceneChanged;
        }
    }

    void Update()
    {
        // Periodic auto-save
        if (autoSaveInterval > 0)
        {
            timeSinceLastSave += Time.deltaTime;
            
            if (timeSinceLastSave >= autoSaveInterval)
            {
                SaveGame();
                timeSinceLastSave = 0f;
            }
        }
        
        // Manual save with F5
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveGame();
            Debug.Log("Manual save triggered (F5)");
        }
    }

    private void OnSceneChanged(Scene scene, LoadSceneMode mode)
    {
        // Don't save when loading to menu
        if (scene.name == "MainMenu" || scene.name == "Menu")
            return;
        
        SaveGame();
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
        // You can implement a UI notification here
        Debug.Log("✓ Game Saved");
    }

    private void OnDestroy()
    {
        if (saveOnSceneChange)
        {
            SceneManager.sceneLoaded -= OnSceneChanged;
        }
    }
}