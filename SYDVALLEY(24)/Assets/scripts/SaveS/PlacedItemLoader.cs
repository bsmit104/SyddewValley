using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Add this to your SaveSystem GameObject to automatically load placed items when entering scenes
/// </summary>
public class PlacedItemLoader : MonoBehaviour
{
    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Don't load items in MainMenu
        if (scene.name == "MainMenu" || scene.name == "Menu")
            return;
        
        // Small delay to ensure everything is initialized
        StartCoroutine(LoadPlacedItemsForCurrentScene());
    }

    private System.Collections.IEnumerator LoadPlacedItemsForCurrentScene()
    {
        yield return new WaitForEndOfFrame();
        
        // Only load if we have an active save
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.LoadPlacedItemsForCurrentScene();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}