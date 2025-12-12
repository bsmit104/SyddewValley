using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FriendshipUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject friendshipPanel;  // Main panel to show/hide
    public Transform npcListContainer;  // Container for NPC entries
    public GameObject npcEntryPrefab;   // Prefab for each NPC entry

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.Z;
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;
    public Sprite halfHeartSprite;  // Optional

    private bool isOpen = false;
    private List<NPCFriendshipEntry> npcEntries = new List<NPCFriendshipEntry>();

    void Start()
    {
        if (friendshipPanel != null)
        {
            friendshipPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleFriendshipUI();
        }
    }

    public void ToggleFriendshipUI()
    {
        isOpen = !isOpen;
        
        if (friendshipPanel != null)
        {
            friendshipPanel.SetActive(isOpen);
        }

        if (isOpen)
        {
            RefreshFriendshipList();
        }
    }

    void RefreshFriendshipList()
    {
        // Clear existing entries
        foreach (var entry in npcEntries)
        {
            if (entry.gameObject != null)
                Destroy(entry.gameObject);
        }
        npcEntries.Clear();

        // Get all NPCs from FriendshipManager
        if (FriendshipManager.Instance == null)
        {
            Debug.LogWarning("FriendshipManager not found!");
            return;
        }

        List<string> npcNames = FriendshipManager.Instance.GetAllKnownNPCs();

        // Create entry for each NPC
        foreach (string npcName in npcNames)
        {
            CreateNPCEntry(npcName);
        }
    }

    void CreateNPCEntry(string npcName)
    {
        if (npcEntryPrefab == null || npcListContainer == null)
        {
            Debug.LogWarning("NPC Entry Prefab or Container not assigned!");
            return;
        }

        GameObject entryObj = Instantiate(npcEntryPrefab, npcListContainer);
        NPCFriendshipEntry entry = entryObj.GetComponent<NPCFriendshipEntry>();

        if (entry != null)
        {
            int heartLevel = FriendshipManager.Instance.GetHeartLevel(npcName);
            float progress = FriendshipManager.Instance.GetHeartProgress(npcName);

            entry.Initialize(npcName, heartLevel, progress, fullHeartSprite, emptyHeartSprite);
            npcEntries.Add(entry);
        }
    }

    // Call this to update a specific NPC's display
    public void UpdateNPCDisplay(string npcName)
    {
        if (isOpen)
        {
            RefreshFriendshipList();
        }
    }
}