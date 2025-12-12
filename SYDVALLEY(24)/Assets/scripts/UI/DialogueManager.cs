using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Import TextMeshPro namespace

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    public TMP_Text dialogueText; // Use TextMeshProUGUI instead of Text
    public GameObject dialoguePanel;

    private bool isDialogueVisible = false;

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

    void Update()
    {
        if (isDialogueVisible && Input.GetKeyDown(KeyCode.Space)) // Example key for closing dialogue
        {
            HideDialogue();
        }
    }

    public void ShowDialogue(string dialogue)
    {
        if (dialoguePanel != null && dialogueText != null)
        {
            dialoguePanel.SetActive(true);
            dialogueText.text = dialogue;
            isDialogueVisible = true;
        }
        else
        {
            Debug.LogWarning("DialogueManager: Panel or Text is not assigned!");
        }
    }

    public void HideDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
            isDialogueVisible = false;
        }
    }
}

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using TMPro; // Import TextMeshPro namespace

// public class DialogueManager : MonoBehaviour
// {
//     public TMP_Text dialogueText; // Use TextMeshProUGUI instead of Text
//     public GameObject dialoguePanel;

//     private bool isDialogueVisible = false;

//     void Update()
//     {
//         if (isDialogueVisible && Input.GetKeyDown(KeyCode.Space)) // Example key for closing dialogue
//         {
//             HideDialogue();
//         }
//     }

//     public void ShowDialogue(string dialogue)
//     {
//         dialoguePanel.SetActive(true);
//         dialogueText.text = dialogue;
//         isDialogueVisible = true;
//     }

//     public void HideDialogue()
//     {
//         dialoguePanel.SetActive(false);
//         isDialogueVisible = false;
//     }
// }
