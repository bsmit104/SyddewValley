using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class SaveSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject[] slotPanels; // 3 slot panels
    [SerializeField] private Button[] playButtons;
    [SerializeField] private Button[] deleteButtons;
    [SerializeField] private TextMeshProUGUI[] saveNameTexts;
    [SerializeField] private TextMeshProUGUI[] saveInfoTexts; // Date, playtime, etc.
    [SerializeField] private GameObject[] emptySlotIndicators;
    
    [Header("Confirmation Dialog")]
    [SerializeField] private GameObject deleteConfirmPanel;
    [SerializeField] private TextMeshProUGUI deleteConfirmText;
    [SerializeField] private Button confirmDeleteButton;
    [SerializeField] private Button cancelDeleteButton;
    
    [Header("New Game Settings")]
    [SerializeField] private string firstGameScene = "Town"; // Scene to load for new game
    
    private int pendingDeleteSlot = -1;

    void Start()
    {
        if (deleteConfirmPanel != null)
            deleteConfirmPanel.SetActive(false);
        
        // Setup button listeners
        for (int i = 0; i < 3; i++)
        {
            int slotIndex = i; // Capture for lambda
            
            if (playButtons[i] != null)
                playButtons[i].onClick.AddListener(() => OnPlaySlot(slotIndex));
            
            if (deleteButtons[i] != null)
                deleteButtons[i].onClick.AddListener(() => OnRequestDelete(slotIndex));
        }
        
        if (confirmDeleteButton != null)
            confirmDeleteButton.onClick.AddListener(OnConfirmDelete);
        
        if (cancelDeleteButton != null)
            cancelDeleteButton.onClick.AddListener(OnCancelDelete);
        
        RefreshSlots();
    }

    private void RefreshSlots()
    {
        for (int i = 0; i < 3; i++)
        {
            bool saveExists = SaveSystem.Instance.SaveExists(i);
            
            if (saveExists)
            {
                SaveData data = SaveSystem.Instance.GetSaveData(i);
                
                // Show save info
                if (emptySlotIndicators[i] != null)
                    emptySlotIndicators[i].SetActive(false);
                
                if (saveNameTexts[i] != null)
                    saveNameTexts[i].text = data.saveName;
                
                if (saveInfoTexts[i] != null)
                {
                    string info = $"{data.currentMonth} {data.currentDay}\n";
                    info += $"Last Played: {data.lastSaveTime:MM/dd/yyyy}\n";
                    info += $"Playtime: {FormatPlayTime(data.totalPlayTime)}";
                    saveInfoTexts[i].text = info;
                }
                
                if (playButtons[i] != null)
                {
                    var buttonText = playButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                        buttonText.text = "Continue";
                }
                
                if (deleteButtons[i] != null)
                    deleteButtons[i].gameObject.SetActive(true);
            }
            else
            {
                // Show empty slot
                if (emptySlotIndicators[i] != null)
                    emptySlotIndicators[i].SetActive(true);
                
                if (saveNameTexts[i] != null)
                    saveNameTexts[i].text = $"Empty Slot {i + 1}";
                
                if (saveInfoTexts[i] != null)
                    saveInfoTexts[i].text = "No save data";
                
                if (playButtons[i] != null)
                {
                    var buttonText = playButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                        buttonText.text = "New Game";
                }
                
                if (deleteButtons[i] != null)
                    deleteButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnPlaySlot(int slot)
    {
        if (SaveSystem.Instance.SaveExists(slot))
        {
            // Load existing save
            Debug.Log($"Loading save slot {slot}");
            SaveSystem.Instance.LoadGame(slot);
        }
        else
        {
            // Start new game
            Debug.Log($"Starting new game in slot {slot}");
            StartNewGame(slot);
        }
    }

    private void StartNewGame(int slot)
    {
        // Clear any existing persistent data
        ClearPersistentObjects();
        
        // Load first game scene
        SceneManager.sceneLoaded += (scene, mode) => OnNewGameSceneLoaded(slot);
        SceneManager.LoadScene(firstGameScene);
    }

    private void OnNewGameSceneLoaded(int slot)
    {
        SceneManager.sceneLoaded -= (scene, mode) => OnNewGameSceneLoaded(slot);
        
        // Wait for everything to initialize
        StartCoroutine(InitializeNewGame(slot));
    }

    private System.Collections.IEnumerator InitializeNewGame(int slot)
    {
        yield return new WaitForEndOfFrame();
        
        // Reset player stats
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.maxHealth = 100;
            PlayerHealth.Instance.maxEnergy = 100;
            PlayerHealth.Instance.maxHunger = 100;
            // You'll need public methods to set these
        }
        
        // Clear inventory
        if (Inventory.Instance != null)
        {
            Inventory.Instance.items.Clear();
            Inventory.Instance.OnInventoryChanged?.Invoke();
        }
        
        // Reset calendar
        if (CalendarManager.Instance != null)
        {
            CalendarManager.Instance.SetDate(CalendarManager.Month.Augtomber, 1);
        }
        
        // Save initial state
        SaveSystem.Instance.SaveGame(slot, $"Save {slot + 1}");
        
        Debug.Log($"New game initialized in slot {slot}");
    }

    private void OnRequestDelete(int slot)
    {
        pendingDeleteSlot = slot;
        
        SaveData data = SaveSystem.Instance.GetSaveData(slot);
        if (deleteConfirmText != null && data != null)
        {
            deleteConfirmText.text = $"Delete '{data.saveName}'?\nThis cannot be undone!";
        }
        
        if (deleteConfirmPanel != null)
            deleteConfirmPanel.SetActive(true);
    }

    private void OnConfirmDelete()
    {
        if (pendingDeleteSlot >= 0)
        {
            SaveSystem.Instance.DeleteSave(pendingDeleteSlot);
            pendingDeleteSlot = -1;
            RefreshSlots();
        }
        
        if (deleteConfirmPanel != null)
            deleteConfirmPanel.SetActive(false);
    }

    private void OnCancelDelete()
    {
        pendingDeleteSlot = -1;
        
        if (deleteConfirmPanel != null)
            deleteConfirmPanel.SetActive(false);
    }

    private string FormatPlayTime(float seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(seconds);
        
        if (time.TotalHours >= 1)
            return $"{(int)time.TotalHours}h {time.Minutes}m";
        else
            return $"{time.Minutes}m {time.Seconds}s";
    }

    private void ClearPersistentObjects()
    {
        // This ensures a clean slate for new game
        // Persistent objects will reinitialize themselves
    }
}