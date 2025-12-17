// File: WorldLight.cs
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace WorldTime
{
    [RequireComponent(typeof(Light2D))]
    [DefaultExecutionOrder(-1000)] // Execute before other scripts
    public class WorldLight : MonoBehaviour
    {
        public static WorldLight Instance { get; private set; }
        private static bool hasBeenCreated = false;

        [Header("=== Runtime Settings ===")]
        public float duration = 300f;
        public Gradient gradient;

        public float startTime;
        public float lastDayTime;

        private Light2D sunLight;

        private void Awake()
        {
            // If this is the first WorldLight ever created
            if (!hasBeenCreated)
            {
                hasBeenCreated = true;
                Instance = this;
                sunLight = GetComponent<Light2D>();
                
                // Ensure it's set to Global type
                if (sunLight != null)
                {
                    sunLight.lightType = Light2D.LightType.Global;
                }
                
                DontDestroyOnLoad(gameObject);
                
                startTime = Time.time;
                lastDayTime = startTime;
                
                Debug.Log("[WorldLight] First instance - persisting across scenes");
            }
            else
            {
                // This is a duplicate - destroy it immediately before Light2D can enable
                Debug.Log("[WorldLight] Duplicate detected - destroying immediately");
                DestroyImmediate(gameObject);
            }
        }

        private void Update()
        {
            if (sunLight == null || gradient == null) return;

            float t = (Time.time - startTime) / duration;
            float percentage = Mathf.Sin(t * Mathf.PI * 2f) * 0.5f + 0.5f;
            sunLight.color = gradient.Evaluate(Mathf.Clamp01(percentage));
        }

        public void SetTimeOfDay(float time01)
        {
            if (sunLight == null || gradient == null) return;

            float percentage = Mathf.Sin(time01 * Mathf.PI * 2f) * 0.5f + 0.5f;
            sunLight.color = gradient.Evaluate(Mathf.Clamp01(percentage));
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                hasBeenCreated = false;
            }
        }
    }
}


// // File: WorldLight.cs
// using UnityEngine;
// using UnityEngine.Rendering.Universal;

// namespace WorldTime
// {
//     [RequireComponent(typeof(Light2D))]
//     public class WorldLight : MonoBehaviour
//     {
//         public static WorldLight Instance { get; private set; }

//         [Header("=== Runtime Settings (auto-loaded) ===")]
//         public float duration = 300f;           // Will be overridden by ScriptableObject
//         public Gradient gradient;               // Will be overridden by ScriptableObject

//         public float startTime;
//         public float lastDayTime;

//         private Light2D sunLight;

//         // THIS IS THE MAGIC: Creates the light before any scene loads
//         [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
//         private static void AutoCreate()
//         {
//             if (Instance != null) return;

//             // Load settings from Resources (you'll create this asset next)
//             var settings = Resources.Load<DayNightSettings>("DayNightSettings");
//             if (settings == null)
//             {
//                 Debug.LogError("[WorldLight] Could not find DayNightSettings asset in Resources folder! Create one.");
//                 return;
//             }

//             // Create the global light object
//             GameObject go = new GameObject("WorldLight (Global)");
//             DontDestroyOnLoad(go);

//             Light2D light2D = go.AddComponent<Light2D>();
//             light2D.lightType = Light2D.LightType.Global;
//             light2D.intensity = 1f;
//             light2D.color = Color.white;

//             var worldLight = go.AddComponent<WorldLight>();
//             worldLight.duration = settings.dayDurationInSeconds;
//             worldLight.gradient = settings.gradient;
//             worldLight.sunLight = light2D;

//             Instance = worldLight;
//             worldLight.startTime = Time.time;
//             worldLight.lastDayTime = worldLight.startTime;

//             Debug.Log("[WorldLight] Created globally with custom gradient!");
//         }

//         private void Awake()
//         {
//             // Safety net in case someone accidentally puts it in a scene
//             if (Instance != null && Instance != this)
//             {
//                 Destroy(gameObject);
//                 return;
//             }

//             Instance = this;
//             sunLight = GetComponent<Light2D>();
//             DontDestroyOnLoad(gameObject);
//         }

//         private void Update()
//         {
//             if (sunLight == null || gradient == null) return;

//             float t = (Time.time - startTime) / duration;
//             float percentage = Mathf.Sin(t * Mathf.PI * 2f) * 0.5f + 0.5f;
//             sunLight.color = gradient.Evaluate(Mathf.Clamp01(percentage));
//         }

//         public void SetTimeOfDay(float time01)
//         {
//             if (sunLight == null || gradient == null) return;

//             float percentage = Mathf.Sin(time01 * Mathf.PI * 2f) * 0.5f + 0.5f;
//             sunLight.color = gradient.Evaluate(Mathf.Clamp01(percentage));
//         }
//     }
// }


















// using UnityEngine;
// using UnityEngine.Rendering.Universal;

// namespace WorldTime
// {
//     [RequireComponent(typeof(Light2D))]
//     public class WorldLight : MonoBehaviour
//     {
//         public static WorldLight Instance { get; private set; }

//         [Header("Day/Night Cycle")]
//         public float duration = 300f;
//         [SerializeField] private Gradient gradient;

//         public float startTime;
//         public float lastDayTime;

//         private Light2D sunLight;

//         private void Awake()
//         {
//             if (Instance != null && Instance != this)
//             {
//                 Destroy(gameObject);
//                 return;
//             }

//             Instance = this;
//             DontDestroyOnLoad(gameObject);

//             sunLight = GetComponent<Light2D>();
//             sunLight.lightType = Light2D.LightType.Global; // explicit

//             startTime = Time.time;
//             lastDayTime = startTime;
//         }

//         // Call this from a boot scene or GameManager
//         [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
//         private static void CreateGlobalLight()
//         {
//             if (Instance != null) return;

//             GameObject go = new GameObject("WorldLight (Global)");
//             go.tag = "EditorOnly"; // hides in hierarchy if you want
//             Light2D light2D = go.AddComponent<Light2D>();
//             light2D.lightType = Light2D.LightType.Global;
//             light2D.intensity = 1f;
//             light2D.color = Color.white;

//             WorldLight worldLight = go.AddComponent<WorldLight>();
//             // You can expose gradient via a ScriptableObject if you want to edit it
//         }

//         private void Update()
//         {
//             if (sunLight == null) return;

//             float timeElapsed = Time.time - startTime;
//             float t = timeElapsed / duration;
//             float percentage = Mathf.Sin(t * Mathf.PI * 2f) * 0.5f + 0.5f;
//             sunLight.color = gradient.Evaluate(Mathf.Clamp01(percentage));
//         }

//         public void SetTimeOfDay(float time01)
//         {
//             if (sunLight == null) return;
//             float percentage = Mathf.Sin(time01 * Mathf.PI * 2f) * 0.5f + 0.5f;
//             sunLight.color = gradient.Evaluate(Mathf.Clamp01(percentage));
//         }
//     }
// }

