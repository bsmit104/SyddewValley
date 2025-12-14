using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest/QuestData")]
public class Quest : ScriptableObject
{
    public string questID; // Unique identifier for the quest
    public string questGiver; // Name of the NPC giving the quest
    public string questDescription; // Description shown on the sign
    public Item requestedItem; // The item the NPC wants
    public int reward; // Money reward (5-30)
    
    [Header("Dialogue")]
    public string acceptDialogue; // What NPC says when accepting the quest item
    public string reminderDialogue; // What NPC says if you talk to them without the item
    
    [Header("Quest State")]
    public bool isActive; // Is the quest currently active?
    public bool isCompleted; // Has the quest been completed?
}
