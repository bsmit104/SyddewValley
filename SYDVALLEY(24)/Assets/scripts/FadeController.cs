using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public static FadeController Instance;

    [Header("References (drag in Inspector)")]
    [SerializeField] private Image fadeOverlay;         // Your full-screen black Image
    [SerializeField] private GameObject mainMenuRoot;   // The "Main Menu" parent object

    [Header("Settings")]
    [SerializeField] private float fadeTime = 0.7f;

    private Canvas parentCanvas;

    private void Awake()
    {
        // Singleton + make the whole canvas persistent
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        parentCanvas = GetComponentInParent<Canvas>();

        // THIS IS THE IMPORTANT LINE — keeps your entire UI canvas alive
        DontDestroyOnLoad(parentCanvas.gameObject);

        // Auto-find if you forgot to assign in inspector
        if (fadeOverlay == null)
            fadeOverlay = GetComponentInChildren<Image>();

        if (mainMenuRoot == null)
            mainMenuRoot = parentCanvas.transform.Find("Main Menu")?.gameObject;

        // Setup overlay
        if (fadeOverlay != null)
        {
            fadeOverlay.raycastTarget = false;
            fadeOverlay.color = Color.black;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(FadeIn());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Force the canvas to render on top every time a new scene loads
        parentCanvas.sortingOrder = 9999;

        // Show menu only in MainMenu scene
        if (mainMenuRoot != null)
            mainMenuRoot.SetActive(scene.name == "MainMenu");   // <-- change "MainMenu" to your exact scene name if different

        StartCoroutine(FadeIn());
    }

    // Call this from buttons or your ChangeScenes script
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        // Hide menu immediately when leaving
        if (mainMenuRoot != null)
            mainMenuRoot.SetActive(false);

        yield return FadeOut();
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeOut()
    {
        if (fadeOverlay == null) yield break;

        fadeOverlay.color = new Color(0, 0, 0, 0);
        float t = 0;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            fadeOverlay.color = new Color(0, 0, 0, Mathf.Lerp(0, 1, t / fadeTime));
            yield return null;
        }
        fadeOverlay.color = Color.black;
    }

    private IEnumerator FadeIn()
    {
        if (fadeOverlay == null) yield break;

        fadeOverlay.color = Color.black;
        float t = 0;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            fadeOverlay.color = new Color(0, 0, 0, Mathf.Lerp(1, 0, t / fadeTime));
            yield return null;
        }
        fadeOverlay.color = Color.clear;
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
