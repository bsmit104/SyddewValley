using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    public NPC npcData;

    private bool isPlayerInRange;

    // Gift point values (similar to Stardew Valley)
    private const int LOVED_GIFT_POINTS = 80;
    private const int LIKED_GIFT_POINTS = 45;
    private const int NEUTRAL_GIFT_POINTS = 20;
    private const int HATED_GIFT_POINTS = -40;

    void Update()
    {
        // Detect player interaction (e.g., pressing a key while near the NPC)
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    public void GiveGift(Item gift)
    {
        if (npcData == null)
        {
            Debug.LogWarning("NPC Data is not assigned!");
            return;
        }

        // Check if this NPC has an active quest and if the item is the quest item
        Quest activeQuest = QuestManager.Instance?.GetActiveQuestForNPC(npcData.npcName);
        if (activeQuest != null && activeQuest.requestedItem == gift)
        {
            // This is a quest item! Complete the quest
            CompleteQuest(activeQuest, gift);
            return;
        }

        // Check if NPC can receive another gift today
        if (FriendshipManager.Instance != null && !FriendshipManager.Instance.CanReceiveGift(npcData.npcName))
        {
            int giftsReceived = FriendshipManager.Instance.GetGiftsReceivedToday(npcData.npcName);
            int maxGifts = FriendshipManager.Instance.maxGiftsPerDay;
            ShowDialogue($"I've already received {giftsReceived} gift{(giftsReceived > 1 ? "s" : "")} today. Maybe tomorrow!");
            return; // Gift declined - item stays in inventory
        }

        int pointsToAdd = 0;
        string dialogue = "";

        // Determine gift reaction and points based on your three categories
        if (npcData.lovedGifts.Contains(gift))
        {
            pointsToAdd = LOVED_GIFT_POINTS;
            dialogue = "I love this gift! Thank you so much!";
        }
        else if (npcData.likedGifts.Contains(gift))
        {
            pointsToAdd = LIKED_GIFT_POINTS;
            dialogue = "This is nice, thank you!";
        }
        else if (npcData.hatedGifts.Contains(gift))
        {
            pointsToAdd = HATED_GIFT_POINTS;
            dialogue = "I really don't like this...";
        }
        else
        {
            // Neutral gift - not in any of the three lists
            pointsToAdd = NEUTRAL_GIFT_POINTS;
            dialogue = "Thank you for thinking of me.";
        }

        // Update friendship points through FriendshipManager
        if (FriendshipManager.Instance != null)
        {
            FriendshipManager.Instance.ModifyFriendship(npcData.npcName, pointsToAdd);
            
            // Track that this NPC received a gift today
            FriendshipManager.Instance.OnGiftGiven(npcData.npcName);
            
            // Update UI if it's open
            FriendshipUI friendshipUI = FindObjectOfType<FriendshipUI>();
            if (friendshipUI != null)
            {
                friendshipUI.UpdateNPCDisplay(npcData.npcName);
            }
        }
        else
        {
            Debug.LogWarning("FriendshipManager not found! Cannot update friendship points.");
        }

        // Also update the old heartPoints system for backward compatibility
        npcData.heartPoints = FriendshipManager.Instance != null 
            ? FriendshipManager.Instance.GetHeartLevel(npcData.npcName) 
            : npcData.heartPoints;

        // Remove item from inventory only if gift was accepted
        if (Inventory.Instance != null)
        {
            Inventory.Instance.RemoveItem(gift, 1);
        }

        ShowDialogue(dialogue);
    }

    private void CompleteQuest(Quest quest, Item questItem)
    {
        // Remove the quest item from inventory
        if (Inventory.Instance != null)
        {
            Inventory.Instance.RemoveItem(questItem, 1);
        }

        // Add money reward using your existing MoneyManager
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddMoney(quest.reward);
        }

        // Show completion dialogue
        ShowDialogue(quest.acceptDialogue);

        // Mark quest as completed
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest(quest.questID);
        }
    }

    public void Interact()
    {
        if (npcData == null)
        {
            Debug.LogWarning("NPC Data is not assigned!");
            return;
        }

        // Check if this NPC has an active quest
        Quest activeQuest = QuestManager.Instance?.GetActiveQuestForNPC(npcData.npcName);
        if (activeQuest != null)
        {
            // Show reminder dialogue about the quest
            ShowDialogue(activeQuest.reminderDialogue);
            return;
        }

        // Get current heart level to potentially show different dialogues
        int currentHearts = 0;
        if (FriendshipManager.Instance != null)
        {
            currentHearts = FriendshipManager.Instance.GetHeartLevel(npcData.npcName);
        }

        // Pick a random dialogue
        if (npcData.dialogues.Count > 0)
        {
            int index = Random.Range(0, npcData.dialogues.Count);
            ShowDialogue(npcData.dialogues[index]);
        }
        else
        {
            ShowDialogue("Hello!");
        }
    }

    private void ShowDialogue(string dialogue)
    {
        if (DialogueManager.Instance != null)
        {
            Debug.Log($"{npcData.npcName}: {dialogue}");
            DialogueManager.Instance.ShowDialogue(dialogue);
        }
        else
        {
            Debug.LogWarning("DialogueManager not found in the scene.");
            Debug.Log($"{npcData.npcName}: {dialogue}");
        }
    }

    // Trigger detection methods
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
}

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class NPCInteraction : MonoBehaviour
// {
//     public NPC npcData;

//     private bool isPlayerInRange;

//     // Gift point values (similar to Stardew Valley)
//     private const int LOVED_GIFT_POINTS = 80;
//     private const int LIKED_GIFT_POINTS = 45;
//     private const int NEUTRAL_GIFT_POINTS = 20;  // For gifts not in any category
//     private const int HATED_GIFT_POINTS = -40;

//     void Update()
//     {
//         // Detect player interaction (e.g., pressing a key while near the NPC)
//         if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
//         {
//             Interact();
//         }
//     }

//     public void GiveGift(Item gift)
//     {
//         if (npcData == null)
//         {
//             Debug.LogWarning("NPC Data is not assigned!");
//             return;
//         }

//         // Check if NPC can receive another gift today
//         if (FriendshipManager.Instance != null && !FriendshipManager.Instance.CanReceiveGift(npcData.npcName))
//         {
//             int giftsReceived = FriendshipManager.Instance.GetGiftsReceivedToday(npcData.npcName);
//             int maxGifts = FriendshipManager.Instance.maxGiftsPerDay;
//             ShowDialogue($"I've already received {giftsReceived} gift{(giftsReceived > 1 ? "s" : "")} today. Maybe tomorrow!");
//             return; // Gift declined - item stays in inventory
//         }

//         int pointsToAdd = 0;
//         string dialogue = "";

//         // Determine gift reaction and points based on your three categories
//         if (npcData.lovedGifts.Contains(gift))
//         {
//             pointsToAdd = LOVED_GIFT_POINTS;
//             dialogue = "I love this gift! Thank you so much!";
//         }
//         else if (npcData.likedGifts.Contains(gift))
//         {
//             pointsToAdd = LIKED_GIFT_POINTS;
//             dialogue = "This is nice, thank you!";
//         }
//         else if (npcData.hatedGifts.Contains(gift))
//         {
//             pointsToAdd = HATED_GIFT_POINTS;
//             dialogue = "I really don't like this...";
//         }
//         else
//         {
//             // Neutral gift - not in any of the three lists
//             pointsToAdd = NEUTRAL_GIFT_POINTS;
//             dialogue = "Thank you for thinking of me.";
//         }

//         // Update friendship points through FriendshipManager
//         if (FriendshipManager.Instance != null)
//         {
//             FriendshipManager.Instance.ModifyFriendship(npcData.npcName, pointsToAdd);
            
//             // Track that this NPC received a gift today
//             FriendshipManager.Instance.OnGiftGiven(npcData.npcName);
            
//             // Update UI if it's open
//             FriendshipUI friendshipUI = FindObjectOfType<FriendshipUI>();
//             if (friendshipUI != null)
//             {
//                 friendshipUI.UpdateNPCDisplay(npcData.npcName);
//             }
//         }
//         else
//         {
//             Debug.LogWarning("FriendshipManager not found! Cannot update friendship points.");
//         }

//         // Also update the old heartPoints system for backward compatibility
//         npcData.heartPoints = FriendshipManager.Instance != null 
//             ? FriendshipManager.Instance.GetHeartLevel(npcData.npcName) 
//             : npcData.heartPoints;

//         // Remove item from inventory only if gift was accepted
//         if (Inventory.Instance != null)
//         {
//             Inventory.Instance.RemoveItem(gift, 1);
//         }

//         ShowDialogue(dialogue);
//     }

//     public void Interact()
//     {
//         if (npcData == null)
//         {
//             Debug.LogWarning("NPC Data is not assigned!");
//             return;
//         }

//         // Get current heart level to potentially show different dialogues
//         int currentHearts = 0;
//         if (FriendshipManager.Instance != null)
//         {
//             currentHearts = FriendshipManager.Instance.GetHeartLevel(npcData.npcName);
//         }

//         // Pick a random dialogue
//         if (npcData.dialogues.Count > 0)
//         {
//             int index = Random.Range(0, npcData.dialogues.Count);
//             ShowDialogue(npcData.dialogues[index]);
//         }
//         else
//         {
//             ShowDialogue("Hello!");
//         }
//     }

//     private void ShowDialogue(string dialogue)
//     {
//         if (DialogueManager.Instance != null)
//         {
//             Debug.Log($"{npcData.npcName}: {dialogue}");
//             DialogueManager.Instance.ShowDialogue(dialogue);
//         }
//         else
//         {
//             Debug.LogWarning("DialogueManager not found in the scene.");
//             Debug.Log($"{npcData.npcName}: {dialogue}");
//         }
//     }

//     // Trigger detection methods
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
// }
