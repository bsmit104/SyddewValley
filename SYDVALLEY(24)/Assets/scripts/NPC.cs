using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPC
{
    public string name; // The name of the NPC
    public List<Item> likedGifts; // A list of items that this NPC likes
    public int heartPoints; // The current heart points (relationship level) with this NPC
    public List<string> dialogues; // A list of dialogues that this NPC can say
}