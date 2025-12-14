using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldTime;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
    
    public List<Quest> allQuests = new List<Quest>(); // All possible quests - assign in inspector
    private Dictionary<string, Quest> activeQuests = new Dictionary<string, Quest>();
    
    private int lastQuestDay = -1;
    private CalendarManager.Month lastQuestMonth;
    
    void Awake()
    {
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
            CalendarManager.Instance.OnDayChanged += OnNewDay;
        }
    }

    void OnNewDay(int newDay)
    {
        // Clear all active quests when a new day starts
        ExpireAllQuests();
        Debug.Log("New day! All quests expired.");
    }
    
    public void ActivateQuest(Quest quest)
    {
        if (quest == null)
        {
            Debug.LogWarning("Cannot activate quest - it's null");
            return;
        }
        
        if (!activeQuests.ContainsKey(quest.questID))
        {
            quest.isActive = true;
            quest.isCompleted = false; // Reset completion status for new day
            activeQuests.Add(quest.questID, quest);
            
            // Track when this quest was assigned
            lastQuestDay = CalendarManager.Instance.CurrentDay;
            lastQuestMonth = CalendarManager.Instance.CurrentMonth;
            
            Debug.Log($"Quest activated: {quest.questGiver} wants {quest.requestedItem.itemName}");
        }
        else
        {
            Debug.Log($"Quest already active: {quest.questID}");
        }
    }
    
    public void CompleteQuest(string questID)
    {
        if (activeQuests.ContainsKey(questID))
        {
            Quest quest = activeQuests[questID];
            quest.isCompleted = true;
            quest.isActive = false;
            activeQuests.Remove(questID);
            Debug.Log($"Quest completed: {quest.questGiver}'s request");
        }
        else
        {
            Debug.LogWarning($"Cannot complete quest - quest ID not found: {questID}");
        }
    }
    
    public Quest GetActiveQuestForNPC(string npcName)
    {
        foreach (var quest in activeQuests.Values)
        {
            if (quest.questGiver == npcName)
            {
                return quest;
            }
        }
        return null;
    }
    
    public bool HasActiveQuestForNPC(string npcName)
    {
        return GetActiveQuestForNPC(npcName) != null;
    }
    
    public bool IsQuestActive(string questID)
    {
        return activeQuests.ContainsKey(questID);
    }
    
    public List<Quest> GetActiveQuests()
    {
        return new List<Quest>(activeQuests.Values);
    }

    public void ExpireAllQuests()
    {
        foreach (var quest in activeQuests.Values)
        {
            quest.isActive = false;
            quest.isCompleted = false; // Reset for potential reuse
        }
        activeQuests.Clear();
    }

    public Quest GetRandomQuest()
    {
        if (allQuests.Count == 0)
        {
            Debug.LogWarning("No quests available in allQuests list!");
            return null;
        }

        // Get a random quest
        int randomIndex = Random.Range(0, allQuests.Count);
        return allQuests[randomIndex];
    }

    private void OnDestroy()
    {
        if (CalendarManager.Instance != null)
        {
            CalendarManager.Instance.OnDayChanged -= OnNewDay;
        }
    }
}

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class QuestManager : MonoBehaviour
// {
//     public static QuestManager Instance { get; private set; }
    
//     public List<Quest> allQuests = new List<Quest>(); // All available quests - assign in inspector
//     private Dictionary<string, Quest> activeQuests = new Dictionary<string, Quest>();
    
//     void Awake()
//     {
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
    
//     public void ActivateQuest(Quest quest)
//     {
//         if (quest == null || quest.isCompleted)
//         {
//             Debug.LogWarning("Cannot activate quest - it's null or already completed");
//             return;
//         }
        
//         if (!activeQuests.ContainsKey(quest.questID))
//         {
//             quest.isActive = true;
//             activeQuests.Add(quest.questID, quest);
//             Debug.Log($"Quest activated: {quest.questDescription}");
//         }
//         else
//         {
//             Debug.Log($"Quest already active: {quest.questID}");
//         }
//     }
    
//     public void CompleteQuest(string questID)
//     {
//         if (activeQuests.ContainsKey(questID))
//         {
//             Quest quest = activeQuests[questID];
//             quest.isCompleted = true;
//             quest.isActive = false;
//             activeQuests.Remove(questID);
//             Debug.Log($"Quest completed: {quest.questDescription}");
//         }
//         else
//         {
//             Debug.LogWarning($"Cannot complete quest - quest ID not found: {questID}");
//         }
//     }
    
//     public Quest GetActiveQuestForNPC(string npcName)
//     {
//         foreach (var quest in activeQuests.Values)
//         {
//             if (quest.questGiver == npcName)
//             {
//                 return quest;
//             }
//         }
//         return null;
//     }
    
//     public bool HasActiveQuestForNPC(string npcName)
//     {
//         return GetActiveQuestForNPC(npcName) != null;
//     }
    
//     public bool IsQuestCompleted(string questID)
//     {
//         foreach (var quest in allQuests)
//         {
//             if (quest.questID == questID)
//             {
//                 return quest.isCompleted;
//             }
//         }
//         return false;
//     }
    
//     public bool IsQuestActive(string questID)
//     {
//         return activeQuests.ContainsKey(questID);
//     }
    
//     public List<Quest> GetActiveQuests()
//     {
//         return new List<Quest>(activeQuests.Values);
//     }
// }