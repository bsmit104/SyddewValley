////negs///
using UnityEngine;
using System;
using TMPro;
using UnityEngine.SceneManagement;

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
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        currentMoney = startingMoney;
        UpdateMoneyDisplay();
        Debug.Log($"MoneyManager initialized with ${currentMoney}");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reconnect money text reference after scene load
        if (moneyText == null)
        {
            // Try to find MoneyText in the new scene
            GameObject moneyTextObj = GameObject.Find("MoneyText");
            if (moneyTextObj != null)
            {
                moneyText = moneyTextObj.GetComponent<TextMeshProUGUI>();
            }
        }
        UpdateMoneyDisplay();
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

        // Check if player can afford (normal purchases)
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
    
    // NEW: Force spend money (for penalties like death/passout that can go negative)
    public void ForceSpendMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("Cannot force spend negative money amount!");
            return;
        }

        currentMoney -= amount;
        UpdateMoneyDisplay();
        OnMoneyChanged?.Invoke(currentMoney);
        
        if (currentMoney < 0)
        {
            Debug.Log($"Force spent ${amount}. New balance: ${currentMoney} (IN DEBT!)");
        }
        else
        {
            Debug.Log($"Force spent ${amount}. New balance: ${currentMoney}");
        }
    }

    // NEW: Set money directly (used by SaveSystem when loading)
    public void SetMoney(int amount)
    {
        currentMoney = amount;
        UpdateMoneyDisplay();
        OnMoneyChanged?.Invoke(currentMoney);
        Debug.Log($"Money set to ${currentMoney}");
    }

    private void UpdateMoneyDisplay()
    {
        if (moneyText != null)
        {
            // Show negative sign for debt
            if (currentMoney < 0)
            {
                moneyText.text = $"-${Mathf.Abs(currentMoney):N0}";
            }
            else
            {
                moneyText.text = $"${currentMoney:N0}";
            }
        }
    }

    public int GetCurrentMoney()
    {
        return currentMoney;
    }

    public bool IsInDebt()
    {
        return currentMoney < 0;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

////no negs///
// using UnityEngine;
// using System;
// using TMPro;
// using UnityEngine.SceneManagement;

// public class MoneyManager : MonoBehaviour
// {
//     public static MoneyManager Instance { get; private set; }

//     [SerializeField] private TextMeshProUGUI moneyText;
//     [SerializeField] private int startingMoney = 100;
//     private int currentMoney = 0;

//     public event Action<int> OnMoneyChanged;

//     void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//             SceneManager.sceneLoaded += OnSceneLoaded;
//         }
//         else
//         {
//             Destroy(gameObject);
//             return;
//         }
//     }

//     void Start()
//     {
//         currentMoney = startingMoney;
//         UpdateMoneyDisplay();
//         Debug.Log($"MoneyManager initialized with ${currentMoney}");
//     }

//     private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//     {
//         // Reconnect money text reference after scene load
//         if (moneyText == null)
//         {
//             // Try to find MoneyText in the new scene
//             GameObject moneyTextObj = GameObject.Find("MoneyText");
//             if (moneyTextObj != null)
//             {
//                 moneyText = moneyTextObj.GetComponent<TextMeshProUGUI>();
//             }
//         }
//         UpdateMoneyDisplay();
//     }

//     public bool CanAfford(int amount)
//     {
//         return currentMoney >= amount;
//     }

//     public void AddMoney(int amount)
//     {
//         if (amount < 0)
//         {
//             Debug.LogError("Cannot add negative money amount!");
//             return;
//         }

//         currentMoney += amount;
//         UpdateMoneyDisplay();
//         OnMoneyChanged?.Invoke(currentMoney);
//         Debug.Log($"Added ${amount}. New balance: ${currentMoney}");
//     }

//     public bool SpendMoney(int amount)
//     {
//         if (amount < 0)
//         {
//             Debug.LogError("Cannot spend negative money amount!");
//             return false;
//         }

//         if (!CanAfford(amount))
//         {
//             Debug.Log($"Not enough money! Have ${currentMoney}, need ${amount}");
//             return false;
//         }

//         currentMoney -= amount;
//         UpdateMoneyDisplay();
//         OnMoneyChanged?.Invoke(currentMoney);
//         Debug.Log($"Spent ${amount}. New balance: ${currentMoney}");
//         return true;
//     }

//     // NEW: Set money directly (used by SaveSystem when loading)
//     public void SetMoney(int amount)
//     {
//         currentMoney = amount;
//         UpdateMoneyDisplay();
//         OnMoneyChanged?.Invoke(currentMoney);
//         Debug.Log($"Money set to ${currentMoney}");
//     }

//     private void UpdateMoneyDisplay()
//     {
//         if (moneyText != null)
//         {
//             moneyText.text = $"${currentMoney:N0}";
//         }
//     }

//     public int GetCurrentMoney()
//     {
//         return currentMoney;
//     }

//     private void OnDestroy()
//     {
//         SceneManager.sceneLoaded -= OnSceneLoaded;
//     }
// }














// using UnityEngine;
// using System;
// using TMPro;

// public class MoneyManager : MonoBehaviour
// {
//     public static MoneyManager Instance { get; private set; }

//     [SerializeField] private TextMeshProUGUI moneyText;
//     [SerializeField] private int startingMoney = 100;
//     private int currentMoney = 0;

//     public event Action<int> OnMoneyChanged;

//     void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }

//     void Start()
//     {
//         currentMoney = startingMoney;
//         UpdateMoneyDisplay();
//         Debug.Log($"MoneyManager initialized with ${currentMoney}");
//     }

//     public bool CanAfford(int amount)
//     {
//         return currentMoney >= amount;
//     }

//     public void AddMoney(int amount)
//     {
//         if (amount < 0)
//         {
//             Debug.LogError("Cannot add negative money amount!");
//             return;
//         }

//         currentMoney += amount;
//         UpdateMoneyDisplay();
//         OnMoneyChanged?.Invoke(currentMoney);
//         Debug.Log($"Added ${amount}. New balance: ${currentMoney}");
//     }

//     public bool SpendMoney(int amount)
//     {
//         if (amount < 0)
//         {
//             Debug.LogError("Cannot spend negative money amount!");
//             return false;
//         }

//         if (!CanAfford(amount))
//         {
//             Debug.Log($"Not enough money! Have ${currentMoney}, need ${amount}");
//             return false;
//         }

//         currentMoney -= amount;
//         UpdateMoneyDisplay();
//         OnMoneyChanged?.Invoke(currentMoney);
//         Debug.Log($"Spent ${amount}. New balance: ${currentMoney}");
//         return true;
//     }

//     private void UpdateMoneyDisplay()
//     {
//         if (moneyText != null)
//         {
//             moneyText.text = $"${currentMoney:N0}";
//         }
//     }

//     public int GetCurrentMoney()
//     {
//         return currentMoney;
//     }
// } 