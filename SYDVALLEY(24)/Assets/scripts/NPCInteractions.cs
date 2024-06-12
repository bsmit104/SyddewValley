using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    public NPC npcData;

    private bool isPlayerInRange;

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
        if (npcData.lovedGifts.Contains(gift))
        {
            npcData.heartPoints += 10;
            ShowDialogue("I love this gift! Thank you!");
        }
        else if (npcData.likedGifts.Contains(gift))
        {
            npcData.heartPoints += 5;
            ShowDialogue("This is nice, thank you!");
        }
        else if (npcData.hatedGifts.Contains(gift))
        {
            npcData.heartPoints -= 5;
            ShowDialogue("I really don't like this.");
        }
        else
        {
            ShowDialogue("Thank you, but I'm not sure about this gift.");
        }

        // Optionally, trigger some event or update the UI to reflect the new heart points
    }

    public void Interact()
    {
        // Pick a random dialogue
        int index = Random.Range(0, npcData.dialogues.Count);
        ShowDialogue(npcData.dialogues[index]);
    }

    // private void ShowDialogue(string dialogue)
    // {
    //     // Implement your method to display dialogue on screen
    //     Debug.Log($"{npcData.npcName}: {dialogue}");
    // }
    private void ShowDialogue(string dialogue)
    {
        Debug.Log("beep");
        DialogueManager dialogueManager = Object.FindFirstObjectByType<DialogueManager>();
        Debug.Log("boop");
        //FindObjectOfType<DialogueManager>();
        if (dialogueManager != null)
        {
            Debug.Log("dmanager not null");
            Debug.Log($"{npcData.npcName}: {dialogue}");
            dialogueManager.ShowDialogue(dialogue);
            Debug.Log($"{npcData.npcName}: {dialogue}");
        }
        else
        {
            Debug.LogWarning("DialogueManager not found in the scene.");
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


// using UnityEngine;

// public class NPCInteractions : MonoBehaviour
// {
//     public NPCData npcData; // Reference to the ScriptableObject containing all NPC data

//     // This method should be called when interacting with an NPC game object
//     public void InteractWithNPC(GameObject npcGameObject)
//     {
//         NPCComponent npcComponent = npcGameObject.GetComponent<NPCComponent>();
//         if (npcComponent != null)
//         {
//             NPC currentNPC = npcComponent.npcData;
//             // Display dialogue based on heart points
//             string dialogue = GetDialogue(currentNPC.heartPoints, currentNPC);
//             Debug.Log(dialogue);
//         }
//         else
//         {
//             Debug.LogError("No NPCComponent found on the game object");
//         }
//     }

//     public void GiveGift(GameObject npcGameObject, Item gift)
//     {
//         NPCComponent npcComponent = npcGameObject.GetComponent<NPCComponent>();
//         if (npcComponent != null)
//         {
//             NPC currentNPC = npcComponent.npcData;
//             if (currentNPC.likedGifts.Contains(gift))
//             {
//                 currentNPC.heartPoints++;
//                 Debug.Log("NPC liked the gift! Heart points: " + currentNPC.heartPoints);
//             }
//             else
//             {
//                 Debug.Log("NPC did not like the gift.");
//             }
//         }
//         else
//         {
//             Debug.LogError("No NPCComponent found on the game object");
//         }
//     }

//     private string GetDialogue(int heartPoints, NPC npc)
//     {
//         // Choose dialogue based on heart points
//         return npc.dialogues[Mathf.Min(heartPoints, npc.dialogues.Count - 1)];
//     }
// }





// using UnityEngine;

// public class NPCInteractions : MonoBehaviour
// {
//     public string npcName; // Identifier for the NPC
//     public NPCData npcData; // Reference to the ScriptableObject containing all NPC data

//     private int heartPoints = 0; // Heart points of the NPC, starts at 0
//     private string[] dialogues; // Array of dialogues for the NPC

//     private void Start()
//     {
//         // Load NPC data
//         if (npcData == null)
//         {
//             Debug.LogError("NPCData is not assigned.");
//             return;
//         }

//         // Find NPC data based on the name
//         foreach (var npc in npcData.npcs)
//         {
//             if (npc.name == npcName)
//             {
//                 dialogues = npc.dialogues;
//                 break;
//             }
//         }

//         if (dialogues == null)
//         {
//             Debug.LogError($"Dialogues for NPC {npcName} not found in NPCData.");
//         }
//     }

//     // Interact with the NPC
//     public void Interact()
//     {
//         // Display dialogue based on heart points
//         string dialogue = GetDialogue();
//         Debug.Log(dialogue);
//     }

//     // Give gift to the NPC
//     public void GiveGift(Item gift)
//     {
//         if (npcData == null)
//         {
//             Debug.LogError("NPCData is not assigned.");
//             return;
//         }

//         foreach (var npc in npcData.npcs)
//         {
//             if (npc.name == npcName)
//             {
//                 if (npc.likedGifts.Contains(gift))
//                 {
//                     heartPoints++;
//                     Debug.Log("NPC liked the gift! Heart points: " + heartPoints);
//                 }
//                 else
//                 {
//                     Debug.Log("NPC did not like the gift.");
//                 }
//                 break;
//             }
//         }
//     }

//     // Get dialogue based on heart points
//     private string GetDialogue()
//     {
//         int dialogueIndex = Mathf.Min(heartPoints, dialogues.Length - 1);
//         return dialogues[dialogueIndex];
//     }
// }