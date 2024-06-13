using UnityEngine;

public class NPCComponent : MonoBehaviour
{
    public string npcName; // Identifier for the NPC

    // Change access modifier to public to make it accessible from other classes
    public NPCData npcData;
    private NPC npc;

    private void Start()
    {
        npcData = Resources.Load<NPCData>("NPCData");
        npc = npcData.npcs.Find(n => n.name == npcName);
        
        if (npc == null)
        {
            Debug.LogError($"NPC with name {npcName} not found in NPCData.");
        }
    }

    public NPC GetNPC()
    {
        return npc;
    }
}