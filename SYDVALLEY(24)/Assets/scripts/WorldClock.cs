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
        public TextMeshProUGUI penaltyText; // NEW: For displaying penalty messages
        public Button nextDayButton;

        [Header("=== Time Settings ===")]
        public float dayDurationInSeconds = 300f;

        [Header("=== Start Time ===")]
        [Range(0f, 23.99f)]
        [Tooltip("What hour the game starts at (6 = 6:00 AM)")]
        public float startHour = 6f;

        [Header("=== Passout Settings ===")]
        public int minPassoutPenalty = 10;
        public int maxPassoutPenalty = 30;
        
        [Header("=== Death Respawn ===")]
        public string deathRespawnScene = "Town";
        public Vector3 deathRespawnPosition = new Vector3(0, -3, 0);

        [SerializeField] private float currentTimeOfDay = 0f;
        public float CurrentTimeOfDay => currentTimeOfDay;

        private bool isSleeping = false;
        private bool hasPassedOutToday = false; // Prevent multiple passouts in one day
        private bool respawnOnNextDay = false; // Flag to trigger respawn after death
        private int lastDeathMessageIndex = -1; // Track last death message
        private int lastPassoutMessageIndex = -1; // Track last passout message

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
                    newDayDateText = newDayObj.transform.Find("DateText")?.GetComponent<TextMeshProUGUI>();
                    penaltyText = newDayObj.transform.Find("PenaltyText")?.GetComponent<TextMeshProUGUI>();
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

            // Check for 2 AM passout
            float currentHour = currentTimeOfDay * 24f;
            if (currentHour >= 2f && currentHour < 3f && !isSleeping && !hasPassedOutToday)
            {
                HandlePassout();
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

        private void HandlePassout()
        {
            Debug.Log("💤 Player passed out from exhaustion!");
            
            // Calculate passout penalty
            int penalty = Random.Range(minPassoutPenalty, maxPassoutPenalty + 1);
            
            // Deduct money from player (can go negative)
            if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.ForceSpendMoney(penalty);
            }
            
            // Show passout screen
            ShowPassoutScreen(penalty);
            
            hasPassedOutToday = true;
        }

        public void ShowDeathScreen(int medicalBill, bool isFromDeath = false)
        {
            if (!newDayPanel) return;

            SetupNewDayPanel();

            if (newDayDateText && CalendarManager.Instance)
            {
                string suffix = GetDaySuffix(CalendarManager.Instance.CurrentDay);
                newDayDateText.text = $"{CalendarManager.Instance.CurrentMonth} {CalendarManager.Instance.CurrentDay}{suffix}";
            }

            if (penaltyText != null)
            {
                // Random death message (avoid repeating the same one)
                string[] deathMessages = new string[]
                {
                    $"${medicalBill} was taken for your medical bills",
                    $"It was rough, they found you in a pool of blood and thought you wouldn't pull through, but with modern medical technology, you were saved. The bill was ${medicalBill}",
                    $"MAULED. YOU WERE MAULED TO SLIVERS. The doctor said there was nothing they could do, and then a woman showed up on a crab with what looked like a mask. It didn't matter, she saved you with magic. However the doctor pick pocketed you for ${medicalBill}",
                    $"Them critters beat you and robbed you for ${medicalBill}. This was the origin of their gang brotherhood affiliation. They go on robbing people til they are rich and have families. Then the coppers caught up with them and split their families",
                    $"Welp, all you really had was a scratch but they called an ambulance and that shi ain't cheap.. ${medicalBill}",
                    $"Man medical bills are getting pricey, but your health is priority. Bill: ${medicalBill}"
                };
                
                int newIndex;
                do
                {
                    newIndex = Random.Range(0, deathMessages.Length);
                } while (newIndex == lastDeathMessageIndex && deathMessages.Length > 1);
                
                lastDeathMessageIndex = newIndex;
                penaltyText.text = deathMessages[newIndex];
                penaltyText.gameObject.SetActive(true);
            }

            newDayPanel.SetActive(true);
            Time.timeScale = 0f;
            isSleeping = true;
            
            // Set flag to respawn player in Town after clicking next day
            if (isFromDeath)
            {
                respawnOnNextDay = true;
            }
        }

        public void ShowPassoutScreen(int penalty)
        {
            if (!newDayPanel) return;

            SetupNewDayPanel();

            if (newDayDateText && CalendarManager.Instance)
            {
                string suffix = GetDaySuffix(CalendarManager.Instance.CurrentDay);
                newDayDateText.text = $"{CalendarManager.Instance.CurrentMonth} {CalendarManager.Instance.CurrentDay}{suffix}";
            }

            if (penaltyText != null)
            {
                // Random passout message (avoid repeating the same one)
                string[] passoutMessages = new string[]
                {
                    $"You passed out on the floor from what can only be described as a narcoleptic episode. You were so tired, people mugged you for ${penalty} with ease.",
                    $"Really you should take better care of yourself.. in your fit of tiredness you dropped your wallet and some cash fell out. You lost ${penalty}",
                    $"You don't remember it but when you fell asleep Russians picked you up and took you partying at the clubs. It seems like the money just went missing, but no, that was beer money ${penalty}",
                    $"Just get to bed next time, and the universe won't de-materialize money to punish you ${penalty}",
                    $"The government caught you sleeping in a place other than a bed, and that is actually against the law now, so you were fined ${penalty}"
                };
                
                int newIndex;
                do
                {
                    newIndex = Random.Range(0, passoutMessages.Length);
                } while (newIndex == lastPassoutMessageIndex && passoutMessages.Length > 1);
                
                lastPassoutMessageIndex = newIndex;
                penaltyText.text = passoutMessages[newIndex];
                penaltyText.gameObject.SetActive(true);
            }

            newDayPanel.SetActive(true);
            Time.timeScale = 0f;
            isSleeping = true;
        }

        public void ShowNewDayScreen()
        {
            if (!newDayPanel) return;

            SetupNewDayPanel();

            if (newDayDateText && CalendarManager.Instance)
            {
                string suffix = GetDaySuffix(CalendarManager.Instance.CurrentDay);
                newDayDateText.text = $"{CalendarManager.Instance.CurrentMonth} {CalendarManager.Instance.CurrentDay}{suffix}";
            }

            // Hide penalty text for normal sleep
            if (penaltyText != null)
            {
                penaltyText.gameObject.SetActive(false);
            }

            newDayPanel.SetActive(true);
            Time.timeScale = 0f;
            isSleeping = true;
        }

        private void SetupNewDayPanel()
        {
            // Bring New Day Panel to front
            Canvas newDayCanvas = newDayPanel.GetComponent<Canvas>();
            if (newDayCanvas == null)
            {
                newDayCanvas = newDayPanel.AddComponent<Canvas>();
                newDayCanvas.overrideSorting = true;
            }
            newDayCanvas.sortingOrder = 10000;

            // Add GraphicRaycaster if needed
            if (newDayPanel.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                newDayPanel.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        private void OnNextDayClicked()
        {
            CalendarManager.Instance.AdvanceDay();
            SetTimeOfDay(6f / 24f);

            newDayPanel.SetActive(false);
            Time.timeScale = 1f;
            isSleeping = false;
            hasPassedOutToday = false; // Reset passout flag for new day
            
            // Handle death respawn
            if (respawnOnNextDay)
            {
                respawnOnNextDay = false;
                RespawnPlayerInTown();
            }
        }
        
        private void RespawnPlayerInTown()
        {
            // Load Town scene if not already there
            if (SceneManager.GetActiveScene().name != deathRespawnScene)
            {
                SceneManager.LoadScene(deathRespawnScene);
            }
            
            // Find and move the player
            StartCoroutine(MovePlayerAfterSceneLoad());
        }
        
        private IEnumerator MovePlayerAfterSceneLoad()
        {
            // Wait a frame to ensure scene is loaded
            yield return null;
            
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = deathRespawnPosition;
                Debug.Log($"Player respawned at {deathRespawnPosition} in {deathRespawnScene}");
            }
            else
            {
                Debug.LogError("Could not find Player object to respawn!");
            }
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
//         public TextMeshProUGUI penaltyText; // NEW: For displaying penalty messages
//         public Button nextDayButton;

//         [Header("=== Time Settings ===")]
//         public float dayDurationInSeconds = 300f;

//         [Header("=== Start Time ===")]
//         [Range(0f, 23.99f)]
//         [Tooltip("What hour the game starts at (6 = 6:00 AM)")]
//         public float startHour = 6f;

//         [Header("=== Passout Settings ===")]
//         public int minPassoutPenalty = 10;
//         public int maxPassoutPenalty = 30;
        
//         [Header("=== Death Respawn ===")]
//         public string deathRespawnScene = "Town";
//         public Vector3 deathRespawnPosition = new Vector3(0, -3, 0);

//         [SerializeField] private float currentTimeOfDay = 0f;
//         public float CurrentTimeOfDay => currentTimeOfDay;

//         private bool isSleeping = false;
//         private bool hasPassedOutToday = false; // Prevent multiple passouts in one day
//         private bool respawnOnNextDay = false; // Flag to trigger respawn after death

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
//                     newDayDateText = newDayObj.transform.Find("DateText")?.GetComponent<TextMeshProUGUI>();
//                     penaltyText = newDayObj.transform.Find("PenaltyText")?.GetComponent<TextMeshProUGUI>();
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

//             // Check for 2 AM passout
//             float currentHour = currentTimeOfDay * 24f;
//             if (currentHour >= 2f && currentHour < 3f && !isSleeping && !hasPassedOutToday)
//             {
//                 HandlePassout();
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

//         private void HandlePassout()
//         {
//             Debug.Log("💤 Player passed out from exhaustion!");
            
//             // Calculate passout penalty
//             int penalty = Random.Range(minPassoutPenalty, maxPassoutPenalty + 1);
            
//             // Deduct money from player
//             if (MoneyManager.Instance != null)
//             {
//                 MoneyManager.Instance.SpendMoney(penalty);
//             }
            
//             // Show passout screen
//             ShowPassoutScreen(penalty);
            
//             hasPassedOutToday = true;
//         }

//         public void ShowDeathScreen(int medicalBill, bool isFromDeath = false)
//         {
//             if (!newDayPanel) return;

//             SetupNewDayPanel();

//             if (newDayDateText && CalendarManager.Instance)
//             {
//                 string suffix = GetDaySuffix(CalendarManager.Instance.CurrentDay);
//                 newDayDateText.text = $"{CalendarManager.Instance.CurrentMonth} {CalendarManager.Instance.CurrentDay}{suffix}";
//             }

//             if (penaltyText != null)
//             {
//                 penaltyText.text = $"${medicalBill} was taken for your medical bills";
//                 penaltyText.gameObject.SetActive(true);
//             }

//             newDayPanel.SetActive(true);
//             Time.timeScale = 0f;
//             isSleeping = true;
            
//             // Set flag to respawn player in Town after clicking next day
//             if (isFromDeath)
//             {
//                 respawnOnNextDay = true;
//             }
//         }

//         public void ShowPassoutScreen(int penalty)
//         {
//             if (!newDayPanel) return;

//             SetupNewDayPanel();

//             if (newDayDateText && CalendarManager.Instance)
//             {
//                 string suffix = GetDaySuffix(CalendarManager.Instance.CurrentDay);
//                 newDayDateText.text = $"{CalendarManager.Instance.CurrentMonth} {CalendarManager.Instance.CurrentDay}{suffix}";
//             }

//             if (penaltyText != null)
//             {
//                 penaltyText.text = $"You passed out on the floor from what can only be described as a narcoleptic episode. You were so tired, people mugged you for ${penalty} with ease.";
//                 penaltyText.gameObject.SetActive(true);
//             }

//             newDayPanel.SetActive(true);
//             Time.timeScale = 0f;
//             isSleeping = true;
//         }

//         public void ShowNewDayScreen()
//         {
//             if (!newDayPanel) return;

//             SetupNewDayPanel();

//             if (newDayDateText && CalendarManager.Instance)
//             {
//                 string suffix = GetDaySuffix(CalendarManager.Instance.CurrentDay);
//                 newDayDateText.text = $"{CalendarManager.Instance.CurrentMonth} {CalendarManager.Instance.CurrentDay}{suffix}";
//             }

//             // Hide penalty text for normal sleep
//             if (penaltyText != null)
//             {
//                 penaltyText.gameObject.SetActive(false);
//             }

//             newDayPanel.SetActive(true);
//             Time.timeScale = 0f;
//             isSleeping = true;
//         }

//         private void SetupNewDayPanel()
//         {
//             // Bring New Day Panel to front
//             Canvas newDayCanvas = newDayPanel.GetComponent<Canvas>();
//             if (newDayCanvas == null)
//             {
//                 newDayCanvas = newDayPanel.AddComponent<Canvas>();
//                 newDayCanvas.overrideSorting = true;
//             }
//             newDayCanvas.sortingOrder = 10000;

//             // Add GraphicRaycaster if needed
//             if (newDayPanel.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
//                 newDayPanel.AddComponent<UnityEngine.UI.GraphicRaycaster>();
//         }

//         private void OnNextDayClicked()
//         {
//             CalendarManager.Instance.AdvanceDay();
//             SetTimeOfDay(6f / 24f);

//             newDayPanel.SetActive(false);
//             Time.timeScale = 1f;
//             isSleeping = false;
//             hasPassedOutToday = false; // Reset passout flag for new day
            
//             // Handle death respawn
//             if (respawnOnNextDay)
//             {
//                 respawnOnNextDay = false;
//                 RespawnPlayerInTown();
//             }
//         }
        
//         private void RespawnPlayerInTown()
//         {
//             // Load Town scene if not already there
//             if (SceneManager.GetActiveScene().name != deathRespawnScene)
//             {
//                 SceneManager.LoadScene(deathRespawnScene);
//             }
            
//             // Find and move the player
//             StartCoroutine(MovePlayerAfterSceneLoad());
//         }
        
//         private IEnumerator MovePlayerAfterSceneLoad()
//         {
//             // Wait a frame to ensure scene is loaded
//             yield return null;
            
//             GameObject player = GameObject.FindGameObjectWithTag("Player");
//             if (player != null)
//             {
//                 player.transform.position = deathRespawnPosition;
//                 Debug.Log($"Player respawned at {deathRespawnPosition} in {deathRespawnScene}");
//             }
//             else
//             {
//                 Debug.LogError("Could not find Player object to respawn!");
//             }
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




/////old///
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

//             // Instead of hiding HUD, bring New Day Panel to front
//             Canvas newDayCanvas = newDayPanel.GetComponent<Canvas>();
//             if (newDayCanvas == null)
//             {
//                 newDayCanvas = newDayPanel.AddComponent<Canvas>();
//                 newDayCanvas.overrideSorting = true;
//             }
//             newDayCanvas.sortingOrder = 10000; // Render on top of everything

//             // Also add a GraphicRaycaster if needed for button clicks
//             if (newDayPanel.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
//                 newDayPanel.AddComponent<UnityEngine.UI.GraphicRaycaster>();

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
