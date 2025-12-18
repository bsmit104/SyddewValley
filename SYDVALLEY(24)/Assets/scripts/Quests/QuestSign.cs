using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldTime;

public class QuestSign : MonoBehaviour
{
    // Static variable to persist the current quest across scene loads
    private static Quest staticCurrentDailyQuest;
    private static int staticLastAssignedDay = -1;
    private static CalendarManager.Month staticLastAssignedMonth;
    private static bool staticInitialized = false;
    
    private bool isPlayerInRange = false;
    private bool hasSubscribedToEvents = false;
    
    void Start()
    {
        // Only initialize once across all scene loads
        if (!staticInitialized)
        {
            staticInitialized = true;
            AssignNewQuest();
        }
        else
        {
            // Check if day has changed since last time
            if (CalendarManager.Instance != null)
            {
                if (staticLastAssignedDay != CalendarManager.Instance.CurrentDay || 
                    staticLastAssignedMonth != CalendarManager.Instance.CurrentMonth)
                {
                    AssignNewQuest();
                }
            }
        }
        
        // Subscribe to day changes (only once per sign instance)
        if (CalendarManager.Instance != null && !hasSubscribedToEvents)
        {
            CalendarManager.Instance.OnDayChanged += OnNewDay;
            hasSubscribedToEvents = true;
        }
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            InteractWithSign();
        }
    }

    void OnNewDay(int newDay)
    {
        // Assign a new random quest each day
        AssignNewQuest();
        Debug.Log("Quest board updated with new daily quest!");
    }

    private void AssignNewQuest()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("QuestManager not found!");
            return;
        }

        // Get a random quest from the quest pool
        staticCurrentDailyQuest = QuestManager.Instance.GetRandomQuest();
        
        if (staticCurrentDailyQuest != null && CalendarManager.Instance != null)
        {
            staticLastAssignedDay = CalendarManager.Instance.CurrentDay;
            staticLastAssignedMonth = CalendarManager.Instance.CurrentMonth;
            Debug.Log($"New quest assigned: {staticCurrentDailyQuest.questGiver} wants {staticCurrentDailyQuest.requestedItem.itemName}");
        }
    }
    
    private void InteractWithSign()
    {
        if (staticCurrentDailyQuest == null)
        {
            ShowDialogue("No quests available today. Check back tomorrow!");
            return;
        }
        
        // Check if this quest is already active
        if (staticCurrentDailyQuest.isActive)
        {
            string reminderMessage = $"Reminder: {staticCurrentDailyQuest.questGiver} wants a {staticCurrentDailyQuest.requestedItem.itemName}. " +
                                    $"Bring it to them for ${staticCurrentDailyQuest.reward}!";
            ShowDialogue(reminderMessage);
            return;
        }
        
        // Check if the quest was already completed today
        if (staticCurrentDailyQuest.isCompleted)
        {
            // Assign a new quest since the previous one was completed
            AssignNewQuest();
            
            if (staticCurrentDailyQuest == null)
            {
                ShowDialogue("No more quests available today. Check back tomorrow!");
                return;
            }
        }
        
        // Activate the quest and show the quest description from the ScriptableObject
        QuestManager.Instance.ActivateQuest(staticCurrentDailyQuest);
        ShowDialogue(staticCurrentDailyQuest.questDescription);
    }
    
    private void ShowDialogue(string dialogue)
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ShowDialogue(dialogue);
        }
        else
        {
            Debug.Log(dialogue);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    private void OnDestroy()
    {
        if (CalendarManager.Instance != null && hasSubscribedToEvents)
        {
            CalendarManager.Instance.OnDayChanged -= OnNewDay;
            hasSubscribedToEvents = false;
        }
    }
    
    // Reset static data when game quits or starts fresh
    private void OnApplicationQuit()
    {
        staticInitialized = false;
        staticCurrentDailyQuest = null;
        staticLastAssignedDay = -1;
    }
}

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using WorldTime;

// public class QuestSign : MonoBehaviour
// {
//     // Static variable to persist the current quest across scene loads
//     private static Quest staticCurrentDailyQuest;
//     private static int staticLastAssignedDay = -1;
//     private static CalendarManager.Month staticLastAssignedMonth;
//     private static bool staticInitialized = false;
    
//     private bool isPlayerInRange = false;
//     private bool hasSubscribedToEvents = false;
    
//     void Start()
//     {
//         // Only initialize once across all scene loads
//         if (!staticInitialized)
//         {
//             staticInitialized = true;
//             AssignNewQuest();
//         }
//         else
//         {
//             // Check if day has changed since last time
//             if (CalendarManager.Instance != null)
//             {
//                 if (staticLastAssignedDay != CalendarManager.Instance.CurrentDay || 
//                     staticLastAssignedMonth != CalendarManager.Instance.CurrentMonth)
//                 {
//                     AssignNewQuest();
//                 }
//             }
//         }
        
//         // Subscribe to day changes (only once per sign instance)
//         if (CalendarManager.Instance != null && !hasSubscribedToEvents)
//         {
//             CalendarManager.Instance.OnDayChanged += OnNewDay;
//             hasSubscribedToEvents = true;
//         }
//     }

//     void Update()
//     {
//         if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
//         {
//             InteractWithSign();
//         }
//     }

//     void OnNewDay(int newDay)
//     {
//         // Assign a new random quest each day
//         AssignNewQuest();
//         Debug.Log("Quest board updated with new daily quest!");
//     }

//     private void AssignNewQuest()
//     {
//         if (QuestManager.Instance == null)
//         {
//             Debug.LogWarning("QuestManager not found!");
//             return;
//         }

//         // Get a random quest from the quest pool
//         staticCurrentDailyQuest = QuestManager.Instance.GetRandomQuest();
        
//         if (staticCurrentDailyQuest != null && CalendarManager.Instance != null)
//         {
//             staticLastAssignedDay = CalendarManager.Instance.CurrentDay;
//             staticLastAssignedMonth = CalendarManager.Instance.CurrentMonth;
//             Debug.Log($"New quest assigned: {staticCurrentDailyQuest.questGiver} wants {staticCurrentDailyQuest.requestedItem.itemName}");
//         }
//     }
    
//     private void InteractWithSign()
//     {
//         if (staticCurrentDailyQuest == null)
//         {
//             ShowDialogue("No quests available today. Check back tomorrow!");
//             return;
//         }
        
//         // Check if this quest is already active
//         if (staticCurrentDailyQuest.isActive)
//         {
//             string reminderMessage = $"Reminder: {staticCurrentDailyQuest.questGiver} wants a {staticCurrentDailyQuest.requestedItem.itemName}. " +
//                                     $"Bring it to them for ${staticCurrentDailyQuest.reward}!";
//             ShowDialogue(reminderMessage);
//             return;
//         }
        
//         // Check if the quest was already completed today
//         if (staticCurrentDailyQuest.isCompleted)
//         {
//             // Assign a new quest since the previous one was completed
//             AssignNewQuest();
            
//             if (staticCurrentDailyQuest == null)
//             {
//                 ShowDialogue("No more quests available today. Check back tomorrow!");
//                 return;
//             }
//         }
        
//         // Activate the quest
//         QuestManager.Instance.ActivateQuest(staticCurrentDailyQuest);
        
//         string questMessage = $"Hey this is {staticCurrentDailyQuest.questGiver}, Can someone bring me a {staticCurrentDailyQuest.requestedItem.itemName}? " +
//                              $"I will pay you ${staticCurrentDailyQuest.reward}!!";
        
//         ShowDialogue(questMessage);
//     }
    
//     private void ShowDialogue(string dialogue)
//     {
//         if (DialogueManager.Instance != null)
//         {
//             DialogueManager.Instance.ShowDialogue(dialogue);
//         }
//         else
//         {
//             Debug.Log(dialogue);
//         }
//     }
    
//     private void OnTriggerEnter2D(Collider2D other)
//     {
//         if (other.CompareTag("Player"))
//         {
//             isPlayerInRange = true;
//         }
//     }
    
//     private void OnTriggerExit2D(Collider2D other)
//     {
//         if (other.CompareTag("Player"))
//         {
//             isPlayerInRange = false;
//         }
//     }

//     private void OnDestroy()
//     {
//         if (CalendarManager.Instance != null && hasSubscribedToEvents)
//         {
//             CalendarManager.Instance.OnDayChanged -= OnNewDay;
//             hasSubscribedToEvents = false;
//         }
//     }
    
//     // Reset static data when game quits or starts fresh
//     private void OnApplicationQuit()
//     {
//         staticInitialized = false;
//         staticCurrentDailyQuest = null;
//         staticLastAssignedDay = -1;
//     }
// }

