using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCData", menuName = "ScriptableObjects/NPCData", order = 1)]
public class NPCData : ScriptableObject
{
    public List<NPC> npcs; // A list to hold all NPCs
}