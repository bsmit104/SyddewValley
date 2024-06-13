using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Import TextMeshPro namespace

public class DialogueManager : MonoBehaviour
{
    public TMP_Text dialogueText; // Use TextMeshProUGUI instead of Text
    public GameObject dialoguePanel;

    private bool isDialogueVisible = false;

    void Update()
    {
        if (isDialogueVisible && Input.GetKeyDown(KeyCode.Space)) // Example key for closing dialogue
        {
            HideDialogue();
        }
    }

    public void ShowDialogue(string dialogue)
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = dialogue;
        isDialogueVisible = true;
    }

    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);
        isDialogueVisible = false;
    }
}


// using UnityEngine;
// using UnityEngine.UI;

// public class DialogueManager : MonoBehaviour
// {
//     public Text dialogueText;       // Reference to the UI Text component for displaying dialogue
//     public GameObject dialoguePanel; // Reference to the UI Panel that contains the dialogue text

//     // Method to show dialogue
//     public void ShowDialogue(string dialogue)
//     {
//         dialoguePanel.SetActive(true); // Show the dialogue panel
//         dialogueText.text = dialogue;  // Set the dialogue text
//     }

//     // Method to hide dialogue
//     public void HideDialogue()
//     {
//         dialoguePanel.SetActive(false); // Hide the dialogue panel
//     }
// }