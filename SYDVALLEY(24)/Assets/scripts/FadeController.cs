using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public static FadeController Instance;

    [Header("References (drag in Inspector)")]
    [SerializeField] private Image fadeOverlay;         // Full-screen black Image for fade
    [SerializeField] private GameObject mainMenuRoot;   // "Main Menu" parent object to show/hide

    [Header("Settings")]
    [SerializeField] private float fadeTime = 0.7f;     // Duration of fade in seconds

    private Canvas parentCanvas;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Find parent canvas
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogError("FadeController: No parent Canvas found! Attach this to a UI element under a Canvas.");
            return;
        }

        // Make the entire canvas persistent
        DontDestroyOnLoad(parentCanvas.gameObject);

        // Auto-find references if not assigned
        if (fadeOverlay == null)
            fadeOverlay = GetComponentInChildren<Image>();

        if (fadeOverlay == null)
        {
            Debug.LogError("FadeController: No fadeOverlay Image found! Add a full-screen black Image.");
            return;
        }

        // After auto-finding or assigning fadeOverlay...
        if (fadeOverlay != null)
        {
            fadeOverlay.raycastTarget = false;
            fadeOverlay.color = Color.black;
            fadeOverlay.gameObject.SetActive(true);  // ← Add this
        }

        if (mainMenuRoot == null)
            mainMenuRoot = parentCanvas.transform.Find("Main Menu")?.gameObject;

        // Setup fade overlay
        fadeOverlay.raycastTarget = false;
        fadeOverlay.color = Color.black;  // Start fully black for initial fade in

        // Ensure canvas is Overlay mode for persistence
        parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Subscribe to scene load
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Initial fade in
        StartCoroutine(FadeIn());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Force these to stay active no matter what
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);           // The black Image itself
            fadeOverlay.transform.parent?.gameObject.SetActive(true); // Its direct panel if it has one
        }

        parentCanvas.sortingOrder = 9999;

        // Show/hide Main Menu UI
        if (mainMenuRoot != null)
            mainMenuRoot.SetActive(scene.name == "MainMenu");

        // Hide in-game UI when in MainMenu
        if (scene.name == "MainMenu")
        {
            HideInGameUI();  // We'll define this below
        }
        else
        {
            ShowInGameUI();
        }

        StartCoroutine(FadeIn());
    }

    private void HideInGameUI()
    {
        // Hide HUD
        var hud = parentCanvas.transform.Find("HUD")?.gameObject;
        if (hud != null) hud.SetActive(false);

        // Hide Quit Confirm panel
        if (QuitConfirmUI.Instance?.quitConfirmPanel != null)
            QuitConfirmUI.Instance.quitConfirmPanel.SetActive(false);

        // Hide any other in-game panels (PauseMenu, Inventory, etc.)
        var pauseMenu = parentCanvas.transform.Find("PauseMenu")?.gameObject;
        if (pauseMenu != null) pauseMenu.SetActive(false);

        // ... add more as needed
    }

    private void ShowInGameUI()
    {
        var hud = parentCanvas.transform.Find("HUD")?.gameObject;
        if (hud != null) hud.SetActive(true);

        // Quit panel starts hidden anyway — only shown when player presses Quit
    }


    // private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    // {
    //     // Ensure canvas renders on top
    //     parentCanvas.sortingOrder = 9999;

    //     // Show/hide main menu based on scene
    //     if (mainMenuRoot != null)
    //         mainMenuRoot.SetActive(scene.name == "MainMenu");  // Change "MainMenu" if your scene name differs

    //     // Fade in on every scene load
    //     StartCoroutine(FadeIn());
    // }

    // Public method to load scene with fade (call this from buttons or ChangeScenes)
    public void FadeOutAndLoadScene(string sceneName)
    {
        StartCoroutine(LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        // Hide main menu if leaving MainMenu
        if (mainMenuRoot != null && SceneManager.GetActiveScene().name == "MainMenu")
            mainMenuRoot.SetActive(false);

        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeOut()
    {
        if (fadeOverlay == null) yield break;

        float t = 0f;
        Color startColor = fadeOverlay.color = Color.clear;  // Ensure starting from transparent
        Color endColor = Color.black;

        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            fadeOverlay.color = Color.Lerp(startColor, endColor, t / fadeTime);
            yield return null;
        }

        fadeOverlay.color = endColor;
    }

    private IEnumerator FadeIn()
    {
        if (fadeOverlay == null) yield break;

        float t = 0f;
        Color startColor = fadeOverlay.color = Color.black;  // Ensure starting from black
        Color endColor = Color.clear;

        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            fadeOverlay.color = Color.Lerp(startColor, endColor, t / fadeTime);
            yield return null;
        }

        fadeOverlay.color = endColor;
    }
}

// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;
// using System.Collections;

// public class FadeController : MonoBehaviour
// {
//     public static FadeController Instance;

//     [Header("References (drag in Inspector)")]
//     [SerializeField] private Image fadeOverlay;         // Your full-screen black Image
//     [SerializeField] private GameObject mainMenuRoot;   // The "Main Menu" parent object

//     [Header("Settings")]
//     [SerializeField] private float fadeTime = 0.7f;

//     private Canvas parentCanvas;

//     private void Awake()
//     {
//         // Singleton + make the whole canvas persistent
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }

//         Instance = this;
//         parentCanvas = GetComponentInParent<Canvas>();

//         // THIS IS THE IMPORTANT LINE — keeps your entire UI canvas alive
//         DontDestroyOnLoad(parentCanvas.gameObject);

//         // Auto-find if you forgot to assign in inspector
//         if (fadeOverlay == null)
//             fadeOverlay = GetComponentInChildren<Image>();

//         if (mainMenuRoot == null)
//             mainMenuRoot = parentCanvas.transform.Find("Main Menu")?.gameObject;

//         // Setup overlay
//         if (fadeOverlay != null)
//         {
//             fadeOverlay.raycastTarget = false;
//             fadeOverlay.color = Color.black;
//         }

//         SceneManager.sceneLoaded += OnSceneLoaded;
//         StartCoroutine(FadeIn());
//     }

//     private void OnDestroy()
//     {
//         SceneManager.sceneLoaded -= OnSceneLoaded;
//     }

//     private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//     {
//         // Force the canvas to render on top every time a new scene loads
//         parentCanvas.sortingOrder = 9999;

//         // Show menu only in MainMenu scene
//         if (mainMenuRoot != null)
//             mainMenuRoot.SetActive(scene.name == "MainMenu");   // <-- change "MainMenu" to your exact scene name if different

//         StartCoroutine(FadeIn());
//     }

//     // Call this from buttons or your ChangeScenes script
//     public void LoadScene(string sceneName)
//     {
//         StartCoroutine(LoadRoutine(sceneName));
//     }

//     private IEnumerator LoadRoutine(string sceneName)
//     {
//         // Hide menu immediately when leaving
//         if (mainMenuRoot != null)
//             mainMenuRoot.SetActive(false);

//         yield return FadeOut();
//         SceneManager.LoadScene(sceneName);
//     }

//     private IEnumerator FadeOut()
//     {
//         if (fadeOverlay == null) yield break;

//         fadeOverlay.color = new Color(0, 0, 0, 0);
//         float t = 0;
//         while (t < fadeTime)
//         {
//             t += Time.unscaledDeltaTime;
//             fadeOverlay.color = new Color(0, 0, 0, Mathf.Lerp(0, 1, t / fadeTime));
//             yield return null;
//         }
//         fadeOverlay.color = Color.black;
//     }

//     private IEnumerator FadeIn()
//     {
//         if (fadeOverlay == null) yield break;

//         fadeOverlay.color = Color.black;
//         float t = 0;
//         while (t < fadeTime)
//         {
//             t += Time.unscaledDeltaTime;
//             fadeOverlay.color = new Color(0, 0, 0, Mathf.Lerp(1, 0, t / fadeTime));
//             yield return null;
//         }
//         fadeOverlay.color = Color.clear;
//     }
// }





















// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;
// using System.Collections;

// public class FadeController : MonoBehaviour
// {
//     public static FadeController Instance;

//     [Header("References (drag in Inspector)")]
//     [SerializeField] private Image fadeOverlay;         // Your full-screen black Image
//     [SerializeField] private GameObject mainMenuRoot;   // The "Main Menu" parent object

//     [Header("Settings")]
//     [SerializeField] private float fadeTime = 0.7f;

//     private Canvas parentCanvas;

//     private void Awake()
//     {
//         // Singleton + make the whole canvas persistent
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }

//         Instance = this;
//         parentCanvas = GetComponentInParent<Canvas>();

//         // THIS IS THE IMPORTANT LINE — keeps your entire UI canvas alive
//         DontDestroyOnLoad(parentCanvas.gameObject);

//         // Auto-find if you forgot to assign in inspector
//         if (fadeOverlay == null)
//             fadeOverlay = GetComponentInChildren<Image>();

//         if (mainMenuRoot == null)
//             mainMenuRoot = parentCanvas.transform.Find("Main Menu")?.gameObject;

//         // Setup overlay
//         if (fadeOverlay != null)
//         {
//             fadeOverlay.raycastTarget = false;
//             fadeOverlay.color = Color.black;
//         }

//         SceneManager.sceneLoaded += OnSceneLoaded;
//         StartCoroutine(FadeIn());
//     }

//     private void OnDestroy()
//     {
//         SceneManager.sceneLoaded -= OnSceneLoaded;
//     }

//     private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//     {
//         // Force the canvas to render on top every time a new scene loads
//         parentCanvas.sortingOrder = 9999;

//         // Show menu only in MainMenu scene
//         if (mainMenuRoot != null)
//             mainMenuRoot.SetActive(scene.name == "MainMenu");   // <-- change "MainMenu" to your exact scene name if different

//         StartCoroutine(FadeIn());
//     }

//     // Call this from buttons or your ChangeScenes script
//     public void LoadScene(string sceneName)
//     {
//         StartCoroutine(LoadRoutine(sceneName));
//     }

//     private IEnumerator LoadRoutine(string sceneName)
//     {
//         // Hide menu immediately when leaving
//         if (mainMenuRoot != null)
//             mainMenuRoot.SetActive(false);

//         yield return FadeOut();
//         SceneManager.LoadScene(sceneName);
//     }

//     private IEnumerator FadeOut()
//     {
//         if (fadeOverlay == null) yield break;

//         fadeOverlay.color = new Color(0, 0, 0, 0);
//         float t = 0;
//         while (t < fadeTime)
//         {
//             t += Time.unscaledDeltaTime;
//             fadeOverlay.color = new Color(0, 0, 0, Mathf.Lerp(0, 1, t / fadeTime));
//             yield return null;
//         }
//         fadeOverlay.color = Color.black;
//     }

//     private IEnumerator FadeIn()
//     {
//         if (fadeOverlay == null) yield break;

//         fadeOverlay.color = Color.black;
//         float t = 0;
//         while (t < fadeTime)
//         {
//             t += Time.unscaledDeltaTime;
//             fadeOverlay.color = new Color(0, 0, 0, Mathf.Lerp(1, 0, t / fadeTime));
//             yield return null;
//         }
//         fadeOverlay.color = Color.clear;
//     }
// }

// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;
// using System.Collections;

// public class FadeController : MonoBehaviour
// {
//     public static FadeController Instance;

//     private Image fadeImage;
//     public float fadeDuration = 0.5f;

//     void Awake()
//     {
//         // Singleton for global access
//         if (Instance == null)
//         {
//             Instance = this;

//             // Make sure we're on a root GameObject
//             if (transform.parent != null)
//             {
//                 transform.SetParent(null);
//             }

//             DontDestroyOnLoad(gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//             return;
//         }

//         fadeImage = GetComponent<Image>();

//         // If no Image component, try to find it in children
//         if (fadeImage == null)
//         {
//             fadeImage = GetComponentInChildren<Image>();
//         }

//         if (fadeImage == null)
//         {
//             Debug.LogError("FadeController: No Image component found!");
//         }
//     }

//     void Start()
//     {
//         // Fade in on scene load
//         StartCoroutine(FadeIn());
//     }

//     public IEnumerator FadeIn()
//     {
//         if (fadeImage == null) yield break;

//         float t = fadeDuration;
//         while (t > 0)
//         {
//             t -= Time.deltaTime;
//             float a = t / fadeDuration;
//             fadeImage.color = new Color(0, 0, 0, a);
//             yield return null;
//         }
//         fadeImage.color = Color.clear;
//     }

//     public IEnumerator FadeOutAndLoad(string sceneName)
//     {
//         if (fadeImage == null) yield break;

//         float t = 0;
//         while (t < fadeDuration)
//         {
//             t += Time.deltaTime;
//             float a = t / fadeDuration;
//             fadeImage.color = new Color(0, 0, 0, a);
//             yield return null;
//         }

//         // Load the next scene
//         SceneManager.LoadScene(sceneName);

//         // Fade back in on new scene
//         StartCoroutine(FadeIn());
//     }
// }














// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;
// using System.Collections;

// public class FadeController : MonoBehaviour
// {
//     public static FadeController Instance;

//     private Image fadeImage;
//     public float fadeDuration = 0.5f; // Adjust speed of fade

//     void Awake()
//     {
//         // Singleton for global access
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//             return;
//         }

//         fadeImage = GetComponent<Image>();
//     }

//     void Start()
//     {
//         // Fade in on scene load
//         StartCoroutine(FadeIn());
//     }

//     public IEnumerator FadeIn()
//     {
//         float t = fadeDuration;
//         while (t > 0)
//         {
//             t -= Time.deltaTime;
//             float a = t / fadeDuration;
//             fadeImage.color = new Color(0, 0, 0, a);
//             yield return null;
//         }
//         fadeImage.color = Color.clear;
//     }

//     public IEnumerator FadeOutAndLoad(string sceneName)
//     {
//         float t = 0;
//         while (t < fadeDuration)
//         {
//             t += Time.deltaTime;
//             float a = t / fadeDuration;
//             fadeImage.color = new Color(0, 0, 0, a);
//             yield return null;
//         }

//         // Load the next scene
//         SceneManager.LoadScene(sceneName);

//         // Fade back in on new scene
//         StartCoroutine(FadeIn());
//     }
// }
