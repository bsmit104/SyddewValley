using UnityEngine;
using System;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private int startingMoney = 100;
    private int currentMoney = 0;

    public event Action<int> OnMoneyChanged;

    void Awake()
    {
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

    void Start()
    {
        currentMoney = startingMoney;
        UpdateMoneyDisplay();
        Debug.Log($"MoneyManager initialized with ${currentMoney}");
    }

    public bool CanAfford(int amount)
    {
        return currentMoney >= amount;
    }

    public void AddMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("Cannot add negative money amount!");
            return;
        }

        currentMoney += amount;
        UpdateMoneyDisplay();
        OnMoneyChanged?.Invoke(currentMoney);
        Debug.Log($"Added ${amount}. New balance: ${currentMoney}");
    }

    public bool SpendMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("Cannot spend negative money amount!");
            return false;
        }

        if (!CanAfford(amount))
        {
            Debug.Log($"Not enough money! Have ${currentMoney}, need ${amount}");
            return false;
        }

        currentMoney -= amount;
        UpdateMoneyDisplay();
        OnMoneyChanged?.Invoke(currentMoney);
        Debug.Log($"Spent ${amount}. New balance: ${currentMoney}");
        return true;
    }

    private void UpdateMoneyDisplay()
    {
        if (moneyText != null)
        {
            moneyText.text = $"${currentMoney:N0}";
        }
    }

    public int GetCurrentMoney()
    {
        return currentMoney;
    }
} 