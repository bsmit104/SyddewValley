using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using System.Collections;

namespace WorldTime
{
    public class WorldClock : MonoBehaviour
    {
        public static WorldClock Instance { get; private set; }

        [Header("=== UI ===")]
        public TextMeshProUGUI clockText;
        public GameObject newDayPanel;
        public TextMeshProUGUI newDayDateText;
        public Button nextDayButton;

        [Header("=== Time Settings ===")]
        public float dayDurationInSeconds = 300f;

        [Header("=== Start Time ===")]
        [Range(0f, 23.99f)]
        [Tooltip("What hour the game starts at (6 = 6:00 AM)")]
        public float startHour = 6f;

        [SerializeField] private float currentTimeOfDay = 0f;
        public float CurrentTimeOfDay => currentTimeOfDay;

        private bool isSleeping = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            SetTimeOfDay(startHour / 24f);

            if (newDayPanel) newDayPanel.SetActive(false);
            if (nextDayButton) nextDayButton.onClick.AddListener(OnNextDayClicked);

            UpdateClockDisplay();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Reconnect UI references after scene change
            if (clockText == null)
            {
                GameObject clockTextObj = GameObject.Find("ClockText");
                if (clockTextObj != null)
                    clockText = clockTextObj.GetComponent<TextMeshProUGUI>();
            }

            if (newDayPanel == null)
            {
                GameObject newDayObj = GameObject.Find("NewDayPanel");
                if (newDayObj != null)
                {
                    newDayPanel = newDayObj;
                    newDayDateText = newDayObj.GetComponentInChildren<TextMeshProUGUI>();
                    nextDayButton = newDayObj.GetComponentInChildren<Button>();
                    
                    if (nextDayButton != null)
                        nextDayButton.onClick.AddListener(OnNextDayClicked);
                    
                    newDayPanel.SetActive(false);
                }
            }

            UpdateClockDisplay();
        }

        private void Update()
        {
            if (!isSleeping)
            {
                currentTimeOfDay += Time.deltaTime / dayDurationInSeconds;

                if (currentTimeOfDay >= 1f)
                    currentTimeOfDay -= 1f;
            }

            UpdateClockDisplay();

            float currentHour = currentTimeOfDay * 24f;
            if (currentHour >= 2f && currentHour < 3f && !isSleeping && newDayPanel && !newDayPanel.activeSelf)
            {
                ShowNewDayScreen();
            }
        }

        public void SetTimeOfDay(float time01)
        {
            currentTimeOfDay = Mathf.Clamp01(time01);

            // Sync WorldLight
            if (WorldLight.Instance != null)
            {
                float elapsedToday = currentTimeOfDay * WorldLight.Instance.duration;
                WorldLight.Instance.startTime = Time.time - elapsedToday;
                WorldLight.Instance.lastDayTime = Time.time - elapsedToday;
                WorldLight.Instance.SetTimeOfDay(currentTimeOfDay);
            }

            UpdateClockDisplay();
        }

        private void UpdateClockDisplay()
        {
            if (!clockText) return;

            float hours24 = currentTimeOfDay * 24f;
            int hour = Mathf.FloorToInt(hours24);
            int minute = Mathf.FloorToInt((hours24 - hour) * 60f);

            string period = hour < 12 ? "AM" : "PM";
            int displayHour = hour % 12;
            if (displayHour == 0) displayHour = 12;

            clockText.text = $"{displayHour:D2}:{minute:00} {period}";
        }

        public void ShowNewDayScreen()
        {
            if (!newDayPanel) return;

            // Instead of hiding HUD, bring New Day Panel to front
            Canvas newDayCanvas = newDayPanel.GetComponent<Canvas>();
            if (newDayCanvas == null)
            {
                newDayCanvas = newDayPanel.AddComponent<Canvas>();
                newDayCanvas.overrideSorting = true;
            }
            newDayCanvas.sortingOrder = 10000; // Render on top of everything

            // Also add a GraphicRaycaster if needed for button clicks
            if (newDayPanel.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                newDayPanel.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            newDayPanel.SetActive(true);
            Time.timeScale = 0f;
            isSleeping = true;

            if (newDayDateText && CalendarManager.Instance)
            {
                string suffix = GetDaySuffix(CalendarManager.Instance.CurrentDay);
                newDayDateText.text = $"{CalendarManager.Instance.CurrentMonth} {CalendarManager.Instance.CurrentDay}{suffix}";
            }
        }

        private void OnNextDayClicked()
        {
            CalendarManager.Instance.AdvanceDay();
            SetTimeOfDay(6f / 24f);

            newDayPanel.SetActive(false);
            Time.timeScale = 1f;
            isSleeping = false;
        }

        public void PlayerEnteredBed()
        {
            // No time restriction - player can sleep anytime
            isSleeping = true;
            ShowNewDayScreen();
        }

        private string GetDaySuffix(int day)
        {
            if (day >= 11 && day <= 13) return "th";
            return (day % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th"
            };
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}

// using UnityEngine;
// using TMPro;
// using UnityEngine.UI;
// using UnityEngine.SceneManagement;
// using UnityEngine.Rendering.Universal;

// namespace WorldTime
// {
//     public class WorldClock : MonoBehaviour
//     {
//         public static WorldClock Instance { get; private set; }

//         [Header("=== UI ===")]
//         public TextMeshProUGUI clockText;
//         public GameObject newDayPanel;
//         public TextMeshProUGUI newDayDateText;
//         public Button nextDayButton;

//         [Header("=== Time Settings ===")]
//         public float dayDurationInSeconds = 300f;

//         [Header("=== Start Time ===")]
//         [Range(0f, 23.99f)]
//         [Tooltip("What hour the game starts at (6 = 6:00 AM)")]
//         public float startHour = 6f;

//         [SerializeField] private float currentTimeOfDay = 0f;
//         public float CurrentTimeOfDay => currentTimeOfDay;

//         private bool isSleeping = false;

//         private void Awake()
//         {
//             if (Instance == null)
//             {
//                 Instance = this;
//                 DontDestroyOnLoad(gameObject);
//                 SceneManager.sceneLoaded += OnSceneLoaded;
//             }
//             else
//             {
//                 Destroy(gameObject);
//             }
//         }

//         private void Start()
//         {
//             SetTimeOfDay(startHour / 24f);

//             if (newDayPanel) newDayPanel.SetActive(false);
//             if (nextDayButton) nextDayButton.onClick.AddListener(OnNextDayClicked);

//             UpdateClockDisplay();
//         }

//         private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//         {
//             // Reconnect UI references after scene change
//             if (clockText == null)
//             {
//                 GameObject clockTextObj = GameObject.Find("ClockText");
//                 if (clockTextObj != null)
//                     clockText = clockTextObj.GetComponent<TextMeshProUGUI>();
//             }

//             if (newDayPanel == null)
//             {
//                 GameObject newDayObj = GameObject.Find("NewDayPanel");
//                 if (newDayObj != null)
//                 {
//                     newDayPanel = newDayObj;
//                     newDayDateText = newDayObj.GetComponentInChildren<TextMeshProUGUI>();
//                     nextDayButton = newDayObj.GetComponentInChildren<Button>();
                    
//                     if (nextDayButton != null)
//                         nextDayButton.onClick.AddListener(OnNextDayClicked);
                    
//                     newDayPanel.SetActive(false);
//                 }
//             }

//             UpdateClockDisplay();
//         }

//         private void Update()
//         {
//             if (!isSleeping)
//             {
//                 currentTimeOfDay += Time.deltaTime / dayDurationInSeconds;

//                 if (currentTimeOfDay >= 1f)
//                     currentTimeOfDay -= 1f;
//             }

//             UpdateClockDisplay();

//             float currentHour = currentTimeOfDay * 24f;
//             if (currentHour >= 2f && currentHour < 3f && !isSleeping && newDayPanel && !newDayPanel.activeSelf)
//             {
//                 ShowNewDayScreen();
//             }
//         }

//         public void SetTimeOfDay(float time01)
//         {
//             currentTimeOfDay = Mathf.Clamp01(time01);

//             // Sync WorldLight
//             if (WorldLight.Instance != null)
//             {
//                 float elapsedToday = currentTimeOfDay * WorldLight.Instance.duration;
//                 WorldLight.Instance.startTime = Time.time - elapsedToday;
//                 WorldLight.Instance.lastDayTime = Time.time - elapsedToday;
//                 WorldLight.Instance.SetTimeOfDay(currentTimeOfDay);
//             }

//             UpdateClockDisplay();
//         }

//         private void UpdateClockDisplay()
//         {
//             if (!clockText) return;

//             float hours24 = currentTimeOfDay * 24f;
//             int hour = Mathf.FloorToInt(hours24);
//             int minute = Mathf.FloorToInt((hours24 - hour) * 60f);

//             string period = hour < 12 ? "AM" : "PM";
//             int displayHour = hour % 12;
//             if (displayHour == 0) displayHour = 12;

//             clockText.text = $"{displayHour:D2}:{minute:00} {period}";
//         }

//         public void ShowNewDayScreen()
//         {
//             if (!newDayPanel) return;

//             newDayPanel.SetActive(true);
//             Time.timeScale = 0f;
//             isSleeping = true;

//             if (newDayDateText && CalendarManager.Instance)
//             {
//                 string suffix = GetDaySuffix(CalendarManager.Instance.CurrentDay);
//                 newDayDateText.text = $"{CalendarManager.Instance.CurrentMonth} {CalendarManager.Instance.CurrentDay}{suffix}";
//             }
//         }

//         private void OnNextDayClicked()
//         {
//             CalendarManager.Instance.AdvanceDay();
//             SetTimeOfDay(6f / 24f);

//             newDayPanel.SetActive(false);
//             Time.timeScale = 1f;
//             isSleeping = false;
//         }

//         public void PlayerEnteredBed()
//         {
//             if (currentTimeOfDay * 24f < 20f)
//             {
//                 Debug.Log("Too early to sleep! Come back after 8 PM.");
//                 return;
//             }

//             isSleeping = true;
//             ShowNewDayScreen();
//         }

//         private string GetDaySuffix(int day)
//         {
//             if (day >= 11 && day <= 13) return "th";
//             return (day % 10) switch
//             {
//                 1 => "st",
//                 2 => "nd",
//                 3 => "rd",
//                 _ => "th"
//             };
//         }

//         private void OnDestroy()
//         {
//             SceneManager.sceneLoaded -= OnSceneLoaded;
//         }
//     }
// }










// using UnityEngine;
// using TMPro;
// using UnityEngine.UI;
// using UnityEngine.SceneManagement;
// using UnityEngine.Rendering.Universal;
// using System.Collections;

// namespace WorldTime
// {
//     public class WorldClock : MonoBehaviour
//     {
//         public static WorldClock Instance { get; private set; }

//         [Header("=== UI ===")]
//         public TextMeshProUGUI clockText;
//         public GameObject newDayPanel;
//         public TextMeshProUGUI newDayDateText;
//         public Button nextDayButton;

//         [Header("=== Time Settings ===")]
//         public float dayDurationInSeconds = 300f;

//         [Header("=== Start Time ===")]
//         [Range(0f, 23.99f)]
//         [Tooltip("What hour the game starts at (6 = 6:00 AM)")]
//         public float startHour = 6f;

//         [SerializeField] private float currentTimeOfDay = 0f;
//         public float CurrentTimeOfDay => currentTimeOfDay;

//         private bool isSleeping = false;

//         private void Awake()
//         {
//             if (Instance == null)
//             {
//                 Instance = this;
//                 DontDestroyOnLoad(gameObject);
//                 SceneManager.sceneLoaded += OnSceneLoaded;
//             }
//             else
//             {
//                 Destroy(gameObject);
//             }
//         }

//         private void Start()
//         {
//             SetTimeOfDay(startHour / 24f);

//             if (newDayPanel) newDayPanel.SetActive(false);
//             if (nextDayButton) nextDayButton.onClick.AddListener(OnNextDayClicked);

//             UpdateClockDisplay();
//         }

//         private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//         {
//             // Reconnect UI references after scene change
//             if (clockText == null)
//             {
//                 GameObject clockTextObj = GameObject.Find("ClockText");
//                 if (clockTextObj != null)
//                     clockText = clockTextObj.GetComponent<TextMeshProUGUI>();
//             }

//             if (newDayPanel == null)
//             {
//                 GameObject newDayObj = GameObject.Find("NewDayPanel");
//                 if (newDayObj != null)
//                 {
//                     newDayPanel = newDayObj;
//                     newDayDateText = newDayObj.GetComponentInChildren<TextMeshProUGUI>();
//                     nextDayButton = newDayObj.GetComponentInChildren<Button>();
                    
//                     if (nextDayButton != null)
//                         nextDayButton.onClick.AddListener(OnNextDayClicked);
                    
//                     newDayPanel.SetActive(false);
//                 }
//             }

//             UpdateClockDisplay();
            
//             // Ensure HUD visibility after all scene loading completes
//             StartCoroutine(EnsureHUDVisibility());
//         }

//         private System.Collections.IEnumerator EnsureHUDVisibility()
//         {
//             // Wait one frame to let all scene initialization complete
//             yield return null;
            
//             // Make sure HUD is visible when scene loads (unless we're sleeping)
//             if (!isSleeping)
//             {
//                 GameObject hud = GameObject.Find("HUD");
//                 if (hud != null)
//                 {
//                     hud.SetActive(true);
//                     Debug.Log("[WorldClock] HUD re-enabled after scene load");
//                 }
//             }
//         }

//         private void Update()
//         {
//             if (!isSleeping)
//             {
//                 currentTimeOfDay += Time.deltaTime / dayDurationInSeconds;

//                 if (currentTimeOfDay >= 1f)
//                     currentTimeOfDay -= 1f;
//             }

//             UpdateClockDisplay();

//             float currentHour = currentTimeOfDay * 24f;
//             if (currentHour >= 2f && currentHour < 3f && !isSleeping && newDayPanel && !newDayPanel.activeSelf)
//             {
//                 ShowNewDayScreen();
//             }
//         }

//         public void SetTimeOfDay(float time01)
//         {
//             currentTimeOfDay = Mathf.Clamp01(time01);

//             // Sync WorldLight
//             if (WorldLight.Instance != null)
//             {
//                 float elapsedToday = currentTimeOfDay * WorldLight.Instance.duration;
//                 WorldLight.Instance.startTime = Time.time - elapsedToday;
//                 WorldLight.Instance.lastDayTime = Time.time - elapsedToday;
//                 WorldLight.Instance.SetTimeOfDay(currentTimeOfDay);
//             }

//             UpdateClockDisplay();
//         }

//         private void UpdateClockDisplay()
//         {
//             if (!clockText) return;

//             float hours24 = currentTimeOfDay * 24f;
//             int hour = Mathf.FloorToInt(hours24);
//             int minute = Mathf.FloorToInt((hours24 - hour) * 60f);

//             string period = hour < 12 ? "AM" : "PM";
//             int displayHour = hour % 12;
//             if (displayHour == 0) displayHour = 12;

//             clockText.text = $"{displayHour:D2}:{minute:00} {period}";
//         }

//         public void ShowNewDayScreen()
//         {
//             if (!newDayPanel) return;

//             // Hide HUD
//             GameObject hud = GameObject.Find("HUD");
//             if (hud != null) hud.SetActive(false);

//             newDayPanel.SetActive(true);
//             Time.timeScale = 0f;
//             isSleeping = true;

//             if (newDayDateText && CalendarManager.Instance)
//             {
//                 string suffix = GetDaySuffix(CalendarManager.Instance.CurrentDay);
//                 newDayDateText.text = $"{CalendarManager.Instance.CurrentMonth} {CalendarManager.Instance.CurrentDay}{suffix}";
//             }
//         }

//         private void OnNextDayClicked()
//         {
//             CalendarManager.Instance.AdvanceDay();
//             SetTimeOfDay(6f / 24f);

//             newDayPanel.SetActive(false);
//             Time.timeScale = 1f;
//             isSleeping = false;

//             // Show HUD again
//             GameObject hud = GameObject.Find("HUD");
//             if (hud != null) hud.SetActive(true);
//         }

//         public void PlayerEnteredBed()
//         {
//             // No time restriction - player can sleep anytime
//             isSleeping = true;
//             ShowNewDayScreen();
//         }

//         private string GetDaySuffix(int day)
//         {
//             if (day >= 11 && day <= 13) return "th";
//             return (day % 10) switch
//             {
//                 1 => "st",
//                 2 => "nd",
//                 3 => "rd",
//                 _ => "th"
//             };
//         }

//         private void OnDestroy()
//         {
//             SceneManager.sceneLoaded -= OnSceneLoaded;
//         }
//     }
// }



// using UnityEngine;
// using TMPro;
// using UnityEngine.UI;
// using UnityEngine.SceneManagement;
// using UnityEngine.Rendering.Universal;

// namespace WorldTime
// {
//     public class WorldClock : MonoBehaviour
//     {
//         public static WorldClock Instance { get; private set; }

//         [Header("=== UI ===")]
//         public TextMeshProUGUI clockText;
//         public GameObject newDayPanel;
//         public TextMeshProUGUI newDayDateText;
//         public Button nextDayButton;

//         [Header("=== Time Settings ===")]
//         public float dayDurationInSeconds = 300f;

//         [Header("=== Start Time ===")]
//         [Range(0f, 23.99f)]
//         [Tooltip("What hour the game starts at (6 = 6:00 AM)")]
//         public float startHour = 6f;

//         [SerializeField] private float currentTimeOfDay = 0f;
//         public float CurrentTimeOfDay => currentTimeOfDay;

//         private bool isSleeping = false;

//         private void Awake()
//         {
//             if (Instance == null)
//             {
//                 Instance = this;
//                 DontDestroyOnLoad(gameObject);
//                 SceneManager.sceneLoaded += OnSceneLoaded;
//             }
//             else
//             {
//                 Destroy(gameObject);
//             }
//         }

//         private void Start()
//         {
//             SetTimeOfDay(startHour / 24f);

//             if (newDayPanel) newDayPanel.SetActive(false);
//             if (nextDayButton) nextDayButton.onClick.AddListener(OnNextDayClicked);

//             UpdateClockDisplay();
//         }

//         private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//         {
//             // Reconnect UI references after scene change
//             if (clockText == null)
//             {
//                 GameObject clockTextObj = GameObject.Find("ClockText");
//                 if (clockTextObj != null)
//                     clockText = clockTextObj.GetComponent<TextMeshProUGUI>();
//             }

//             if (newDayPanel == null)
//             {
//                 GameObject newDayObj = GameObject.Find("NewDayPanel");
//                 if (newDayObj != null)
//                 {
//                     newDayPanel = newDayObj;
//                     newDayDateText = newDayObj.GetComponentInChildren<TextMeshProUGUI>();
//                     nextDayButton = newDayObj.GetComponentInChildren<Button>();
                    
//                     if (nextDayButton != null)
//                         nextDayButton.onClick.AddListener(OnNextDayClicked);
                    
//                     newDayPanel.SetActive(false);
//                 }
//             }

//             UpdateClockDisplay();
//         }

//         private void Update()
//         {
//             if (!isSleeping)
//             {
//                 currentTimeOfDay += Time.deltaTime / dayDurationInSeconds;

//                 if (currentTimeOfDay >= 1f)
//                     currentTimeOfDay -= 1f;
//             }

//             UpdateClockDisplay();

//             float currentHour = currentTimeOfDay * 24f;
//             if (currentHour >= 2f && currentHour < 3f && !isSleeping && newDayPanel && !newDayPanel.activeSelf)
//             {
//                 ShowNewDayScreen();
//             }
//         }

//         public void SetTimeOfDay(float time01)
//         {
//             currentTimeOfDay = Mathf.Clamp01(time01);

//             // Sync WorldLight
//             if (WorldLight.Instance != null)
//             {
//                 float elapsedToday = currentTimeOfDay * WorldLight.Instance.duration;
//                 WorldLight.Instance.startTime = Time.time - elapsedToday;
//                 WorldLight.Instance.lastDayTime = Time.time - elapsedToday;
//                 WorldLight.Instance.SetTimeOfDay(currentTimeOfDay);
//             }

//             UpdateClockDisplay();
//         }

//         private void UpdateClockDisplay()
//         {
//             if (!clockText) return;

//             float hours24 = currentTimeOfDay * 24f;
//             int hour = Mathf.FloorToInt(hours24);
//             int minute = Mathf.FloorToInt((hours24 - hour) * 60f);

//             string period = hour < 12 ? "AM" : "PM";
//             int displayHour = hour % 12;
//             if (displayHour == 0) displayHour = 12;

//             clockText.text = $"{displayHour:D2}:{minute:00} {period}";
//         }

//         public void ShowNewDayScreen()
//         {
//             if (!newDayPanel) return;

//             newDayPanel.SetActive(true);
//             Time.timeScale = 0f;
//             isSleeping = true;

//             if (newDayDateText && CalendarManager.Instance)
//             {
//                 string suffix = GetDaySuffix(CalendarManager.Instance.CurrentDay);
//                 newDayDateText.text = $"{CalendarManager.Instance.CurrentMonth} {CalendarManager.Instance.CurrentDay}{suffix}";
//             }
//         }

//         private void OnNextDayClicked()
//         {
//             CalendarManager.Instance.AdvanceDay();
//             SetTimeOfDay(6f / 24f);

//             newDayPanel.SetActive(false);
//             Time.timeScale = 1f;
//             isSleeping = false;
//         }

//         public void PlayerEnteredBed()
//         {
//             if (currentTimeOfDay * 24f < 20f)
//             {
//                 Debug.Log("Too early to sleep! Come back after 8 PM.");
//                 return;
//             }

//             isSleeping = true;
//             ShowNewDayScreen();
//         }

//         private string GetDaySuffix(int day)
//         {
//             if (day >= 11 && day <= 13) return "th";
//             return (day % 10) switch
//             {
//                 1 => "st",
//                 2 => "nd",
//                 3 => "rd",
//                 _ => "th"
//             };
//         }

//         private void OnDestroy()
//         {
//             SceneManager.sceneLoaded -= OnSceneLoaded;
//         }
//     }
// }
