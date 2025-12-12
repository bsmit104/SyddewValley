using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldTime;

public class FriendshipManager : MonoBehaviour
{
    public static FriendshipManager Instance { get; private set; }

    // Dictionary to track friendship points for each NPC
    private Dictionary<string, int> friendshipPoints = new Dictionary<string, int>();
    
    // Track last gift date for each NPC (stored as "Month-Day" string)
    private Dictionary<string, string> lastGiftDate = new Dictionary<string, string>();
    
    // Track how many gifts each NPC received today
    private Dictionary<string, int> giftsReceivedToday = new Dictionary<string, int>();

    // Constants for friendship system
    public const int POINTS_PER_HEART = 250;  // Similar to Stardew Valley
    public const int MAX_HEARTS = 10;
    public const int MAX_POINTS = POINTS_PER_HEART * MAX_HEARTS; // 2500 points total

    [Header("Gift Limit Settings")]
    [Tooltip("Maximum gifts per NPC per day (Stardew Valley uses 1, but 2 feels more generous)")]
    public int maxGiftsPerDay = 2;

    [Header("Friendship Decay Settings")]
    [Tooltip("How many days without a gift before friendship starts to decay")]
    public int daysBeforeDecay = 3;
    
    [Tooltip("How many points to lose per day after the grace period")]
    public int decayPointsPerDay = 5;
    
    [Tooltip("Minimum heart level where decay stops (0 = can decay to 0 hearts)")]
    public int minHeartLevelForDecay = 0;

    private bool hasCheckedDecayToday = false;
    private string currentDateString = "";

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Subscribe to day change event
        if (CalendarManager.Instance != null)
        {
            CalendarManager.Instance.OnDayChanged += OnDayChanged;
            currentDateString = GetCurrentDateString();
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (CalendarManager.Instance != null)
        {
            CalendarManager.Instance.OnDayChanged -= OnDayChanged;
        }
    }

    private void OnDayChanged(int newDay)
    {
        // Reset the daily check flag
        hasCheckedDecayToday = false;
        
        // Clear daily gift counters for new day
        giftsReceivedToday.Clear();
        currentDateString = GetCurrentDateString();
        Debug.Log($"[Friendship] New day! Gift counters reset.");
        
        // Process friendship decay for all NPCs
        ProcessDailyFriendshipDecay();
    }

    private void ProcessDailyFriendshipDecay()
    {
        if (hasCheckedDecayToday) return;
        hasCheckedDecayToday = true;

        if (CalendarManager.Instance == null) return;

        string today = GetCurrentDateString();
        List<string> npcsToProcess = new List<string>(friendshipPoints.Keys);

        foreach (string npcName in npcsToProcess)
        {
            // Skip if NPC is at or below minimum decay level
            int currentHeartLevel = GetHeartLevel(npcName);
            if (currentHeartLevel <= minHeartLevelForDecay)
                continue;

            // Check if NPC has ever received a gift
            if (!lastGiftDate.ContainsKey(npcName))
                continue;

            // Calculate days since last gift
            int daysSinceGift = GetDaysSinceDate(lastGiftDate[npcName], today);

            // Apply decay if past grace period
            if (daysSinceGift > daysBeforeDecay)
            {
                int pointsToLose = -decayPointsPerDay;
                ModifyFriendship(npcName, pointsToLose);
                Debug.Log($"[Friendship Decay] {npcName} lost {decayPointsPerDay} points (no gift for {daysSinceGift} days)");
            }
        }
    }

    // Check if NPC can receive another gift today
    public bool CanReceiveGift(string npcName)
    {
        // Check if date has changed (in case we missed the event)
        string today = GetCurrentDateString();
        if (today != currentDateString)
        {
            giftsReceivedToday.Clear();
            currentDateString = today;
        }

        if (!giftsReceivedToday.ContainsKey(npcName))
        {
            giftsReceivedToday[npcName] = 0;
        }

        return giftsReceivedToday[npcName] < maxGiftsPerDay;
    }

    // Get how many gifts this NPC has received today
    public int GetGiftsReceivedToday(string npcName)
    {
        if (!giftsReceivedToday.ContainsKey(npcName))
        {
            return 0;
        }
        return giftsReceivedToday[npcName];
    }

    // Get current friendship points for an NPC
    public int GetFriendshipPoints(string npcName)
    {
        if (!friendshipPoints.ContainsKey(npcName))
        {
            friendshipPoints[npcName] = 0;
        }
        return friendshipPoints[npcName];
    }

    // Get current heart level for an NPC (0-10)
    public int GetHeartLevel(string npcName)
    {
        int points = GetFriendshipPoints(npcName);
        return Mathf.Clamp(points / POINTS_PER_HEART, 0, MAX_HEARTS);
    }

    // Get progress towards next heart (0.0 to 1.0)
    public float GetHeartProgress(string npcName)
    {
        int points = GetFriendshipPoints(npcName);
        int currentHeartLevel = GetHeartLevel(npcName);
        
        if (currentHeartLevel >= MAX_HEARTS)
            return 1f;

        int pointsInCurrentHeart = points % POINTS_PER_HEART;
        return (float)pointsInCurrentHeart / POINTS_PER_HEART;
    }

    // Add or subtract friendship points
    public void ModifyFriendship(string npcName, int pointsToAdd)
    {
        if (!friendshipPoints.ContainsKey(npcName))
        {
            friendshipPoints[npcName] = 0;
        }

        int oldPoints = friendshipPoints[npcName];
        int oldHeartLevel = GetHeartLevel(npcName);

        friendshipPoints[npcName] = Mathf.Clamp(
            friendshipPoints[npcName] + pointsToAdd, 
            0, 
            MAX_POINTS
        );

        int newHeartLevel = GetHeartLevel(npcName);

        // Log the change
        Debug.Log($"Friendship with {npcName}: {oldPoints} -> {friendshipPoints[npcName]} points ({oldHeartLevel} -> {newHeartLevel} hearts)");

        // Optional: Trigger heart level change event
        if (newHeartLevel > oldHeartLevel)
        {
            OnHeartLevelIncreased(npcName, newHeartLevel);
        }
        else if (newHeartLevel < oldHeartLevel)
        {
            OnHeartLevelDecreased(npcName, newHeartLevel);
        }
    }

    // Called when NPC receives a gift
    public void OnGiftGiven(string npcName)
    {
        if (CalendarManager.Instance != null)
        {
            lastGiftDate[npcName] = GetCurrentDateString();
            
            // Increment daily gift counter
            if (!giftsReceivedToday.ContainsKey(npcName))
            {
                giftsReceivedToday[npcName] = 0;
            }
            giftsReceivedToday[npcName]++;
            
            Debug.Log($"[Friendship] {npcName} received gift #{giftsReceivedToday[npcName]} on {lastGiftDate[npcName]}");
        }
    }

    // Get all NPCs we have friendship data for
    public List<string> GetAllKnownNPCs()
    {
        return new List<string>(friendshipPoints.Keys);
    }

    // Optional event handlers for heart level changes
    private void OnHeartLevelIncreased(string npcName, int newLevel)
    {
        Debug.Log($"Heart level increased! {npcName} is now at {newLevel} hearts!");
        // You can trigger cutscenes, unlock dialogues, etc. here
    }

    private void OnHeartLevelDecreased(string npcName, int newLevel)
    {
        Debug.Log($"Heart level decreased. {npcName} is now at {newLevel} hearts.");
    }

    // Helper: Get current date as "Month-Day" string
    private string GetCurrentDateString()
    {
        if (CalendarManager.Instance == null) return "Augtomber-1";
        return $"{CalendarManager.Instance.CurrentMonth}-{CalendarManager.Instance.CurrentDay}";
    }

    // Helper: Calculate days between two dates
    private int GetDaysSinceDate(string fromDate, string toDate)
    {
        if (string.IsNullOrEmpty(fromDate) || string.IsNullOrEmpty(toDate))
            return 0;

        // Parse dates
        string[] fromParts = fromDate.Split('-');
        string[] toParts = toDate.Split('-');

        if (fromParts.Length != 2 || toParts.Length != 2)
            return 0;

        CalendarManager.Month fromMonth = System.Enum.Parse<CalendarManager.Month>(fromParts[0]);
        int fromDay = int.Parse(fromParts[1]);

        CalendarManager.Month toMonth = System.Enum.Parse<CalendarManager.Month>(toParts[0]);
        int toDay = int.Parse(toParts[1]);

        // Calculate total days
        int fromTotalDays = GetTotalDays(fromMonth, fromDay);
        int toTotalDays = GetTotalDays(toMonth, toDay);

        return toTotalDays - fromTotalDays;
    }

    // Helper: Convert month/day to total days in year
    private int GetTotalDays(CalendarManager.Month month, int day)
    {
        int totalDays = day;
        
        // Add days from previous months
        if (month >= CalendarManager.Month.Novecanuary)
            totalDays += 28; // Augtomber has 28 days
        if (month >= CalendarManager.Month.Febmapril)
            totalDays += 30; // Novecanuary has 30 days
        if (month >= CalendarManager.Month.Mayunly)
            totalDays += 29; // Febmapril has 29 days

        return totalDays;
    }

    // Save/Load methods
    public Dictionary<string, int> GetFriendshipData()
    {
        return new Dictionary<string, int>(friendshipPoints);
    }

    public Dictionary<string, string> GetLastGiftDates()
    {
        return new Dictionary<string, string>(lastGiftDate);
    }

    public void LoadFriendshipData(Dictionary<string, int> data)
    {
        if (data != null)
        {
            friendshipPoints = new Dictionary<string, int>(data);
            Debug.Log($"Loaded friendship data for {friendshipPoints.Count} NPCs");
        }
    }

    public void LoadLastGiftDates(Dictionary<string, string> dates)
    {
        if (dates != null)
        {
            lastGiftDate = new Dictionary<string, string>(dates);
            Debug.Log($"Loaded last gift dates for {lastGiftDate.Count} NPCs");
        }
    }

    public void ClearFriendshipData()
    {
        friendshipPoints.Clear();
        lastGiftDate.Clear();
        giftsReceivedToday.Clear();
        hasCheckedDecayToday = false;
        currentDateString = "";
        Debug.Log("Friendship data cleared");
    }
}

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class FriendshipManager : MonoBehaviour
// {
//     public static FriendshipManager Instance { get; private set; }

//     // Dictionary to track friendship points for each NPC
//     private Dictionary<string, int> friendshipPoints = new Dictionary<string, int>();

//     // Constants for friendship system
//     public const int POINTS_PER_HEART = 250;  // Similar to Stardew Valley
//     public const int MAX_HEARTS = 10;
//     public const int MAX_POINTS = POINTS_PER_HEART * MAX_HEARTS; // 2500 points total

//     void Awake()
//     {
//         // Singleton pattern
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }

//     // Get current friendship points for an NPC
//     public int GetFriendshipPoints(string npcName)
//     {
//         if (!friendshipPoints.ContainsKey(npcName))
//         {
//             friendshipPoints[npcName] = 0;
//         }
//         return friendshipPoints[npcName];
//     }

//     // Get current heart level for an NPC (0-10)
//     public int GetHeartLevel(string npcName)
//     {
//         int points = GetFriendshipPoints(npcName);
//         return Mathf.Clamp(points / POINTS_PER_HEART, 0, MAX_HEARTS);
//     }

//     // Get progress towards next heart (0.0 to 1.0)
//     public float GetHeartProgress(string npcName)
//     {
//         int points = GetFriendshipPoints(npcName);
//         int currentHeartLevel = GetHeartLevel(npcName);
        
//         if (currentHeartLevel >= MAX_HEARTS)
//             return 1f;

//         int pointsInCurrentHeart = points % POINTS_PER_HEART;
//         return (float)pointsInCurrentHeart / POINTS_PER_HEART;
//     }

//     // Add or subtract friendship points
//     public void ModifyFriendship(string npcName, int pointsToAdd)
//     {
//         if (!friendshipPoints.ContainsKey(npcName))
//         {
//             friendshipPoints[npcName] = 0;
//         }

//         int oldPoints = friendshipPoints[npcName];
//         int oldHeartLevel = GetHeartLevel(npcName);

//         friendshipPoints[npcName] = Mathf.Clamp(
//             friendshipPoints[npcName] + pointsToAdd, 
//             0, 
//             MAX_POINTS
//         );

//         int newHeartLevel = GetHeartLevel(npcName);

//         // Log the change
//         Debug.Log($"Friendship with {npcName}: {oldPoints} -> {friendshipPoints[npcName]} points ({oldHeartLevel} -> {newHeartLevel} hearts)");

//         // Optional: Trigger heart level change event
//         if (newHeartLevel > oldHeartLevel)
//         {
//             OnHeartLevelIncreased(npcName, newHeartLevel);
//         }
//         else if (newHeartLevel < oldHeartLevel)
//         {
//             OnHeartLevelDecreased(npcName, newHeartLevel);
//         }
//     }

//     // Get all NPCs we have friendship data for
//     public List<string> GetAllKnownNPCs()
//     {
//         return new List<string>(friendshipPoints.Keys);
//     }

//     // Optional event handlers for heart level changes
//     private void OnHeartLevelIncreased(string npcName, int newLevel)
//     {
//         Debug.Log($"Heart level increased! {npcName} is now at {newLevel} hearts!");
//         // You can trigger cutscenes, unlock dialogues, etc. here
//     }

//     private void OnHeartLevelDecreased(string npcName, int newLevel)
//     {
//         Debug.Log($"Heart level decreased. {npcName} is now at {newLevel} hearts.");
//     }

//     // Save/Load methods (optional - implement when you add save system)
//     public Dictionary<string, int> GetFriendshipData()
//     {
//         return new Dictionary<string, int>(friendshipPoints);
//     }

//     public void LoadFriendshipData(Dictionary<string, int> data)
//     {
//         if (data != null)
//         {
//             friendshipPoints = new Dictionary<string, int>(data);
//             Debug.Log($"Loaded friendship data for {friendshipPoints.Count} NPCs");
//         }
//     }

//     public void ClearFriendshipData()
//     {
//         friendshipPoints.Clear();
//         Debug.Log("Friendship data cleared");
//     }
// }
