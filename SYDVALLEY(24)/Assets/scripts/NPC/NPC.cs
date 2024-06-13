using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNPC", menuName = "NPC/NPCData")]
public class NPC : ScriptableObject
{
    public string npcName;
    public List<Item> lovedGifts;
    public List<Item> likedGifts;
    public List<Item> hatedGifts;
    public int heartPoints;

    public List<string> dialogues; // Various dialogues the NPC can say
}

// using System.Collections.Generic;
// using UnityEngine;

// [System.Serializable]
// public class NPC
// {
//     public string name; // The name of the NPC
//     public List<Item> likedGifts; // A list of items that this NPC likes
//     public int heartPoints; // The current heart points (relationship level) with this NPC
//     public List<string> dialogues; // A list of dialogues that this NPC can say
// }