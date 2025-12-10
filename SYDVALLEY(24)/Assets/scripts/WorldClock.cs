using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

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
            if (currentTimeOfDay * 24f < 20f)
            {
                Debug.Log("Too early to sleep! Come back after 8 PM.");
                return;
            }

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

// namespace WorldTime
// {
//     public class WorldClock : MonoBehaviour
//     {
//         [Header("=== UI ===")]
//         public TextMeshProUGUI clockText;
//         public GameObject newDayPanel;
//         public TextMeshProUGUI newDayDateText;
//         public Button nextDayButton;

//         [Header("=== Time Settings ===")]
//         public float dayDurationInSeconds = 300f;   // 5-minute day. Must match WorldLight.duration!

//         [Header("=== Start Time ===")]
//         [Range(0f, 23.99f)]
//         [Tooltip("What hour the game starts at (6 = 6:00 AM)")]
//         public float startHour = 6f;

//         // Internal time (0 = midnight, 0.25 = 6 AM, 0.5 = noon, 1 = next midnight)
//         [SerializeField] private float currentTimeOfDay = 0f;

//         // Public accessor used by SaveSystem
//         public float CurrentTimeOfDay => currentTimeOfDay;

//         private bool isSleeping = false;

//         private void Start()
//         {
//             // Initialize time and sync lighting immediately
//             SetTimeOfDay(startHour / 24f);

//             if (newDayPanel) newDayPanel.SetActive(false);
//             if (nextDayButton) nextDayButton.onClick.AddListener(OnNextDayClicked);

//             UpdateClockDisplay();
//         }

//         private void Update()
//         {
//             // Don't advance time while the "New Day" panel is open (player is sleeping/confirming)
//             if (!isSleeping)
//             {
//                 currentTimeOfDay += Time.deltaTime / dayDurationInSeconds;

//                 if (currentTimeOfDay >= 1f)
//                     currentTimeOfDay -= 1f;
//             }

//             UpdateClockDisplay();

//             // Automatically show the "New Day" screen at 2:00 AM if player hasn't slept yet
//             float currentHour = currentTimeOfDay * 24f;
//             if (currentHour >= 2f && currentHour < 3f && !isSleeping && newDayPanel && !newDayPanel.activeSelf)
//             {
//                 ShowNewDayScreen();
//             }
//         }

//         /// <summary>
//         /// Public method used by SaveSystem to restore exact time when loading a save
//         /// </summary>
//         public void SetTimeOfDay(float time01)
//         {
//             currentTimeOfDay = Mathf.Clamp01(time01);

//             // Instantly update lighting and sync WorldLight's internal timer so it continues correctly
//             var worldLight = FindObjectOfType<WorldLight>();
//             if (worldLight != null)
//             {
//                 float elapsedToday = currentTimeOfDay * worldLight.duration;
//                 worldLight.startTime = Time.time - elapsedToday;
//                 worldLight.lastDayTime = Time.time - elapsedToday;
//                 worldLight.SetTimeOfDay(currentTimeOfDay);
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
//             // Advance the calendar
//             CalendarManager.Instance.AdvanceDay();

//             // Jump straight to 6:00 AM
//             SetTimeOfDay(6f / 24f);

//             newDayPanel.SetActive(false);
//             Time.timeScale = 1f;
//             isSleeping = false;
//         }

//         // Called by Bed.cs when the player chooses to sleep
//         public void PlayerEnteredBed()
//         {
//             if (currentTimeOfDay * 24f < 20f) // before 8 PM
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
//     }
// }

// using UnityEngine;
// using TMPro;
// using UnityEngine.UI;

// namespace WorldTime
// {
//     public class WorldClock : MonoBehaviour
//     {
//         [Header("=== UI ===")]
//         public TextMeshProUGUI clockText;
//         public GameObject newDayPanel;
//         public TextMeshProUGUI newDayDateText;
//         public Button nextDayButton;

//         [Header("=== Time Settings ===")]
//         public float dayDurationInSeconds = 300f;   // 5-minute day. Must match WorldLight.duration!

//         [Header("=== Start Time ===")]
//         [Range(0f, 23.99f)]
//         [Tooltip("What hour the game starts at (6 = 6:00 AM)")]
//         public float startHour = 6f;   // ← Change this in Inspector anytime

//         private float currentTimeOfDay = 0f; // 0..1 (0 = midnight)
//         private bool isSleeping = false;

//         private void Start()
//         {
//             // Set starting time (6 AM by default)
//             currentTimeOfDay = startHour / 24f;

//             // Force WorldLight to match this exact time immediately
//             var worldLight = FindObjectOfType<WorldLight>();
//             if (worldLight != null)
//             {
//                 float elapsedToday = currentTimeOfDay * worldLight.duration;
//                 worldLight.startTime = Time.time - elapsedToday;
//                 worldLight.lastDayTime = Time.time - elapsedToday;
//                 worldLight.SetTimeOfDay(currentTimeOfDay);
//             }

//             if (newDayPanel) newDayPanel.SetActive(false);
//             nextDayButton?.onClick.AddListener(OnNextDayClicked);

//             UpdateClockDisplay();
//         }

//         private void Update()
//         {
//             // Advance time
//             currentTimeOfDay += Time.deltaTime / dayDurationInSeconds;
//             if (currentTimeOfDay >= 1f) currentTimeOfDay -= 1f;

//             UpdateClockDisplay();

//             // Auto show "New Day" screen at 2:00 AM
//             float currentHour = currentTimeOfDay * 24f;
//             if (currentHour >= 2f && currentHour < 3f && !isSleeping && newDayPanel && !newDayPanel.activeSelf)
//             {
//                 ShowNewDayScreen();
//             }
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

//             if (newDayDateText && CalendarManager.Instance)
//             {
//                 string suffix = GetDaySuffix(CalendarManager.Instance.CurrentDay);
//                 newDayDateText.text = $"{CalendarManager.Instance.CurrentMonth} {CalendarManager.Instance.CurrentDay}{suffix}";
//             }
//         }

//         private void OnNextDayClicked()
//         {
//             // Advance calendar
//             CalendarManager.Instance.AdvanceDay();

//             // Jump to 6:00 AM
//             currentTimeOfDay = 6f / 24f;

//             // Sync light perfectly
//             var light = FindObjectOfType<WorldLight>();
//             if (light != null)
//             {
//                 float elapsed = currentTimeOfDay * light.duration;
//                 light.startTime = Time.time - elapsed;
//                 light.lastDayTime = Time.time - elapsed;
//                 light.SetTimeOfDay(currentTimeOfDay);
//             }

//             newDayPanel.SetActive(false);
//             Time.timeScale = 1f;
//             isSleeping = false;
//             UpdateClockDisplay();
//         }

//         // Called by Bed.cs when player sleeps
//         public void PlayerEnteredBed()
//         {
//             if (currentTimeOfDay * 24f < 20f) // before 8 PM
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
//             return (day % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" };
//         }
//     }
// }