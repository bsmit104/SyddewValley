using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlacedItemLoader : MonoBehaviour
{
    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu" || scene.name == "Menu") return;

        // Wait TWO frames so GetActiveScene() is definitely correct
        StartCoroutine(LoadAfterFrame());
    }

    private IEnumerator LoadAfterFrame()
    {
        yield return null;
        yield return null;

        SaveSystem.Instance?.LoadPlacedItemsForCurrentScene();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}