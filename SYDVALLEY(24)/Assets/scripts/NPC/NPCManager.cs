using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance { get; private set; }

    public Inventory playerInventory; // Reference to the player's inventory
    public KeyCode interactKey = KeyCode.E; // Key for interacting with NPCs
    public KeyCode giftKey = KeyCode.G; // Key for gifting items to NPCs

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

    [System.Obsolete]
    void Update()
    {
        if (Input.GetKeyDown(interactKey)) // Check if the interact key is pressed
        {
            InteractWithNPC(); // Attempt to interact with an NPC
        }

        if (Input.GetKeyDown(giftKey)) // Check if the gift key is pressed
        {
            GiveGiftToNPC(); // Attempt to give a gift to an NPC
        }
    }

    void InteractWithNPC()
    {
        // Convert mouse position to world coordinates
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // Cast a ray at the mouse position to detect any colliders
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider != null)
        {
            // Attempt to get the NPCInteraction component from the hit object
            NPCInteraction npcInteraction = hit.collider.GetComponent<NPCInteraction>();
            if (npcInteraction != null)
            {
                npcInteraction.Interact(); // Trigger NPC interaction (e.g., dialogue)
            }
        }
    }

    [System.Obsolete]
    void GiveGiftToNPC()
    {
        // Convert mouse position to world coordinates
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // Cast a ray at the mouse position to detect any colliders
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider != null)
        {
            // Attempt to get the NPCInteraction component from the hit object
            NPCInteraction npcInteraction = hit.collider.GetComponent<NPCInteraction>();
            if (npcInteraction != null)
            {
                Debug.Log("Gift given");
                playerInventory.GiveSelectedItemToNPC(npcInteraction); // Give the selected item to the NPC
            }
        }
    }
}


// using System.Collections;
// using System.Collections.Generic;

// using UnityEngine;

// public class NPCManager : MonoBehaviour
// {
//     public Inventory playerInventory; // Reference to the player's inventory
//     public KeyCode interactKey = KeyCode.E; // Key for interacting with NPCs
//     public KeyCode giftKey = KeyCode.G; // Key for gifting items to NPCs

//     [System.Obsolete]
//     void Update()
//     {
//         if (Input.GetKeyDown(interactKey)) // Check if the interact key is pressed
//         {
//             InteractWithNPC(); // Attempt to interact with an NPC
//         }

//         if (Input.GetKeyDown(giftKey)) // Check if the gift key is pressed
//         {
//             GiveGiftToNPC(); // Attempt to give a gift to an NPC
//         }
//     }

//     void InteractWithNPC()
//     {
//         // Convert mouse position to world coordinates
//         Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//         // Cast a ray at the mouse position to detect any colliders
//         RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

//         if (hit.collider != null)
//         {
//             // Attempt to get the NPCInteraction component from the hit object
//             NPCInteraction npcInteraction = hit.collider.GetComponent<NPCInteraction>();
//             if (npcInteraction != null)
//             {
//                 npcInteraction.Interact(); // Trigger NPC interaction (e.g., dialogue)
//             }
//         }
//     }

//     [System.Obsolete]
//     void GiveGiftToNPC()
//     {
//         // Convert mouse position to world coordinates
//         Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//         // Cast a ray at the mouse position to detect any colliders
//         RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

//         if (hit.collider != null)
//         {
//             // Attempt to get the NPCInteraction component from the hit object
//             NPCInteraction npcInteraction = hit.collider.GetComponent<NPCInteraction>();
//             if (npcInteraction != null)
//             {
//                 Debug.Log("Gift given");
//                 playerInventory.GiveSelectedItemToNPC(npcInteraction); // Give the selected item to the NPC
//             }
//         }
//     }
// }
