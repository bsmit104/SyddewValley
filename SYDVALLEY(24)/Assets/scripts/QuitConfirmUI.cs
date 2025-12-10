using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using WorldTime;
using System;

public class QuitConfirmUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject quitConfirmPanel;
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("Settings")]
    [SerializeField] private int quitPenalty = 5;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        if (quitConfirmPanel != null)
            quitConfirmPanel.SetActive(false);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmQuit);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelQuit);
    }

    public void ShowQuitConfirmation()
    {
        if (quitConfirmPanel == null) return;

        // Calculate next day
        string nextDate = GetNextDayText();

        // Update confirmation message
        if (confirmationText != null)
        {
            confirmationText.text = $"If you quit now, you'll wake up on {nextDate} at 6:00 AM.\n\n" +
                                   $"Safe ride home: ${quitPenalty}\n\n" +
                                   "Confirm quit?";
        }

        quitConfirmPanel.SetActive(true);
        Time.timeScale = 0f; // Pause game
    }

    private void OnConfirmQuit()
    {
        Time.timeScale = 1f; // Unpause

        // Charge the quit penalty
        if (MoneyManager.Instance != null)
        {
            if (MoneyManager.Instance.CanAfford(quitPenalty))
            {
                MoneyManager.Instance.SpendMoney(quitPenalty);
            }
            else
            {
                // Still deduct, even if it goes negative
                MoneyManager.Instance.SetMoney(MoneyManager.Instance.GetCurrentMoney() - quitPenalty);
            }
        }

        // Advance to next day at 6 AM
        if (CalendarManager.Instance != null)
        {
            CalendarManager.Instance.AdvanceDay();
        }

        // Set time to 6 AM (0.25 = 6/24)
        var clock = FindObjectOfType<WorldClock>();
        if (clock != null)
        {
            clock.SetTimeOfDay(6f / 24f);
        }

        // Save game before quitting
        if (SaveSystem.Instance != null)
        {
            int currentSlot = SaveSystem.Instance.GetCurrentSlot();
            if (currentSlot >= 0)
            {
                SaveSystem.Instance.SaveGame(currentSlot);
                Debug.Log($"Saved game before quitting. Money deducted: ${quitPenalty}");
            }
        }

        // Return to main menu
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnCancelQuit()
    {
        if (quitConfirmPanel != null)
            quitConfirmPanel.SetActive(false);

        Time.timeScale = 1f; // Unpause
    }

    private string GetNextDayText()
    {
        if (CalendarManager.Instance == null)
            return "Tomorrow";

        // Get next day
        CalendarManager.Month nextMonth = CalendarManager.Instance.CurrentMonth;
        int nextDay = CalendarManager.Instance.CurrentDay + 1;

        // Check if we need to advance month
        int daysInMonth = GetDaysInMonth(nextMonth);
        if (nextDay > daysInMonth)
        {
            nextDay = 1;
            nextMonth = (CalendarManager.Month)(((int)nextMonth + 1) % 4);
        }

        string suffix = GetDaySuffix(nextDay);
        return $"{nextMonth} {nextDay}{suffix}";
    }

    private int GetDaysInMonth(CalendarManager.Month month)
    {
        switch (month)
        {
            case CalendarManager.Month.Augtomber: return 28;
            case CalendarManager.Month.Novecanuary: return 30;
            case CalendarManager.Month.Febmapril: return 29;
            case CalendarManager.Month.Mayunly: return 31;
            default: return 28;
        }
    }

    private string GetDaySuffix(int day)
    {
        if (day >= 11 && day <= 13)
            return "th";

        switch (day % 10)
        {
            case 1: return "st";
            case 2: return "nd";
            case 3: return "rd";
            default: return "th";
        }
    }

    // Call this from a menu button or ESC key handler
    public void OnQuitButtonPressed()
    {
        ShowQuitConfirmation();
    }
}