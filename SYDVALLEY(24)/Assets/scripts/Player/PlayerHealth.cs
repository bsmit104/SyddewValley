using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("Energy Settings")]
    public int maxEnergy = 100;
    [SerializeField] private int currentEnergy;

    [Header("Hunger Settings")]
    public int maxHunger = 100;
    [SerializeField] private int currentHunger;

    [Header("UI References")]
    public Image healthBar;
    public Image energyBar;
    public Image hungerBar;

    [Header("Hunger Drain")]
    public float hungerDrainInterval = 60f;

    [Header("Health Regen")]
    public float healthRegenInterval = 2f;

    [Header("Death Settings")]
    public int minMedicalBill = 10;
    public int maxMedicalBill = 30;

    [Header("Testing Keys")]
    public KeyCode damageKey = KeyCode.P;
    public KeyCode energyKey = KeyCode.O;
    public KeyCode regenEnergyKey = KeyCode.I;
    public KeyCode hungerDrainKey = KeyCode.H;
    public KeyCode regenHungerKey = KeyCode.J;
    public KeyCode resetKey = KeyCode.R;

    private float hungerDrainTimer = 0f;
    private float healthRegenTimer = 0f;
    
    public static PlayerHealth Instance { get; private set; }
    private Inventory PlayerInventory => Inventory.Instance;

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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ReconnectUIReferences());
    }

    private System.Collections.IEnumerator ReconnectUIReferences()
    {
        yield return null;
        UpdateHealthUI();
        UpdateEnergyUI();
        UpdateHungerUI();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        currentHunger = maxHunger;
        UpdateHealthUI();
        UpdateEnergyUI();
        UpdateHungerUI();
    }

    private void Update()
    {
        hungerDrainTimer += Time.deltaTime;
        if (hungerDrainTimer >= hungerDrainInterval)
        {
            TakeHungerDamage(1);
            hungerDrainTimer = 0f;
        }

        if (currentHunger == maxHunger && currentHealth < maxHealth)
        {
            healthRegenTimer += Time.deltaTime;
            if (healthRegenTimer >= healthRegenInterval)
            {
                currentHealth = Mathf.Clamp(currentHealth + 1, 0, maxHealth);
                UpdateHealthUI();
                healthRegenTimer = 0f;
            }
        }

        if (Input.GetKeyDown(damageKey))
        {
            TakeDamage(10);
        }

        if (Input.GetKeyDown(energyKey))
        {
            UseEnergy(20);
        }

        if (Input.GetKeyDown(regenEnergyKey))
        {
            RegenerateEnergy(20);
        }

        if (Input.GetKeyDown(hungerDrainKey))
        {
            TakeHungerDamage(10);
        }

        if (Input.GetKeyDown(regenHungerKey))
        {
            RegenerateHunger(20);
        }

        if (Input.GetMouseButtonDown(1))
        {
            EatSelectedFood();
        }

        if (Input.GetKeyDown(resetKey))
        {
            ResetAll();
        }

        if (Time.frameCount % 60 == 0)
        {
            RegenerateEnergy(1);
        }
    }

    private void EatSelectedFood()
    {
        if (PlayerInventory == null) return;

        Item selectedFood = PlayerInventory.GetSelectedItem();
        if (selectedFood == null || !selectedFood.isFood) return;

        RegenerateHunger(selectedFood.hungerRestore);
        PlayerInventory.RemoveItem(selectedFood, 1);
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0)
        {
            HandleDeath();
        }

        UpdateHealthUI();
    }

    private void HandleDeath()
    {
        Debug.Log("💀 Player died!");
        
        // Calculate medical bill
        int medicalBill = Random.Range(minMedicalBill, maxMedicalBill + 1);
        
        // Deduct money from player
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.SpendMoney(medicalBill);
        }
        
        // Show new day screen with medical bill message
        if (WorldTime.WorldClock.Instance != null)
        {
            WorldTime.WorldClock.Instance.ShowDeathScreen(medicalBill);
        }
        
        // Reset health
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public bool UseEnergy(int energyCost)
    {
        if (currentEnergy >= energyCost)
        {
            currentEnergy -= energyCost;
            currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
            UpdateEnergyUI();
            return true;
        }
        return false;
    }

    public void RegenerateEnergy(int amount)
    {
        currentEnergy += amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        UpdateEnergyUI();
    }

    public void TakeHungerDamage(int damageAmount)
    {
        currentHunger -= damageAmount;
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);

        if (currentHunger <= 0)
        {
            Debug.Log("😵 Starving! (Hunger = 0)");
        }

        UpdateHungerUI();
    }

    public void RegenerateHunger(int amount)
    {
        currentHunger += amount;
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
        UpdateHungerUI();
    }

    private void ResetAll()
    {
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        currentHunger = maxHunger;
        UpdateHealthUI();
        UpdateEnergyUI();
        UpdateHungerUI();
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
            healthBar.fillAmount = (float)currentHealth / maxHealth;
    }

    private void UpdateEnergyUI()
    {
        if (energyBar != null)
            energyBar.fillAmount = (float)currentEnergy / maxEnergy;
    }

    private void UpdateHungerUI()
    {
        if (hungerBar != null)
            hungerBar.fillAmount = (float)currentHunger / maxHunger;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Public accessors
    public int CurrentHealth => currentHealth;
    public int CurrentEnergy => currentEnergy;
    public int CurrentHunger => currentHunger;
    
    // Save/Load methods
    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthUI();
    }
    
    public void SetEnergy(int energy)
    {
        currentEnergy = Mathf.Clamp(energy, 0, maxEnergy);
        UpdateEnergyUI();
    }
    
    public void SetHunger(int hunger)
    {
        currentHunger = Mathf.Clamp(hunger, 0, maxHunger);
        UpdateHungerUI();
    }
}

// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.SceneManagement;

// public class PlayerHealth : MonoBehaviour
// {
//     [Header("Health Settings")]
//     public int maxHealth = 100;
//     [SerializeField] private int currentHealth;

//     [Header("Energy Settings")]
//     public int maxEnergy = 100;
//     [SerializeField] private int currentEnergy;

//     [Header("Hunger Settings")]
//     public int maxHunger = 100;
//     [SerializeField] private int currentHunger;

//     [Header("UI References")]
//     public Image healthBar;
//     public Image energyBar;
//     public Image hungerBar;

//     [Header("Hunger Drain")]
//     public float hungerDrainInterval = 60f;

//     [Header("Health Regen")]
//     public float healthRegenInterval = 2f;

//     [Header("Testing Keys")]
//     public KeyCode damageKey = KeyCode.P;
//     public KeyCode energyKey = KeyCode.O;
//     public KeyCode regenEnergyKey = KeyCode.I;
//     public KeyCode hungerDrainKey = KeyCode.H;
//     public KeyCode regenHungerKey = KeyCode.J;
//     public KeyCode resetKey = KeyCode.R;

//     private float hungerDrainTimer = 0f;
//     private float healthRegenTimer = 0f;
    
//     public static PlayerHealth Instance { get; private set; }
//     private Inventory PlayerInventory => Inventory.Instance;

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

//     private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//     {
//         StartCoroutine(ReconnectUIReferences());
//     }

//     private System.Collections.IEnumerator ReconnectUIReferences()
//     {
//         yield return null;
//         UpdateHealthUI();
//         UpdateEnergyUI();
//         UpdateHungerUI();
//     }

//     private void Start()
//     {
//         currentHealth = maxHealth;
//         currentEnergy = maxEnergy;
//         currentHunger = maxHunger;
//         UpdateHealthUI();
//         UpdateEnergyUI();
//         UpdateHungerUI();
//     }

//     private void Update()
//     {
//         hungerDrainTimer += Time.deltaTime;
//         if (hungerDrainTimer >= hungerDrainInterval)
//         {
//             TakeHungerDamage(1);
//             hungerDrainTimer = 0f;
//         }

//         if (currentHunger == maxHunger && currentHealth < maxHealth)
//         {
//             healthRegenTimer += Time.deltaTime;
//             if (healthRegenTimer >= healthRegenInterval)
//             {
//                 currentHealth = Mathf.Clamp(currentHealth + 1, 0, maxHealth);
//                 UpdateHealthUI();
//                 healthRegenTimer = 0f;
//             }
//         }

//         if (Input.GetKeyDown(damageKey))
//         {
//             TakeDamage(10);
//         }

//         if (Input.GetKeyDown(energyKey))
//         {
//             UseEnergy(20);
//         }

//         if (Input.GetKeyDown(regenEnergyKey))
//         {
//             RegenerateEnergy(20);
//         }

//         if (Input.GetKeyDown(hungerDrainKey))
//         {
//             TakeHungerDamage(10);
//         }

//         if (Input.GetKeyDown(regenHungerKey))
//         {
//             RegenerateHunger(20);
//         }

//         if (Input.GetMouseButtonDown(1))
//         {
//             EatSelectedFood();
//         }

//         if (Input.GetKeyDown(resetKey))
//         {
//             ResetAll();
//         }

//         if (Time.frameCount % 60 == 0)
//         {
//             RegenerateEnergy(1);
//         }
//     }

//     private void EatSelectedFood()
//     {
//         if (PlayerInventory == null) return;

//         Item selectedFood = PlayerInventory.GetSelectedItem();
//         if (selectedFood == null || !selectedFood.isFood) return;

//         RegenerateHunger(selectedFood.hungerRestore);
//         PlayerInventory.RemoveItem(selectedFood, 1);
//     }

//     public void TakeDamage(int damageAmount)
//     {
//         currentHealth -= damageAmount;
//         currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

//         if (currentHealth <= 0)
//         {
//             Debug.Log("💀 Player died!");
//         }

//         UpdateHealthUI();
//     }

//     public bool UseEnergy(int energyCost)
//     {
//         if (currentEnergy >= energyCost)
//         {
//             currentEnergy -= energyCost;
//             currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
//             UpdateEnergyUI();
//             return true;
//         }
//         return false;
//     }

//     public void RegenerateEnergy(int amount)
//     {
//         currentEnergy += amount;
//         currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
//         UpdateEnergyUI();
//     }

//     public void TakeHungerDamage(int damageAmount)
//     {
//         currentHunger -= damageAmount;
//         currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);

//         if (currentHunger <= 0)
//         {
//             Debug.Log("😵 Starving! (Hunger = 0)");
//         }

//         UpdateHungerUI();
//     }

//     public void RegenerateHunger(int amount)
//     {
//         currentHunger += amount;
//         currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
//         UpdateHungerUI();
//     }

//     private void ResetAll()
//     {
//         currentHealth = maxHealth;
//         currentEnergy = maxEnergy;
//         currentHunger = maxHunger;
//         UpdateHealthUI();
//         UpdateEnergyUI();
//         UpdateHungerUI();
//     }

//     private void UpdateHealthUI()
//     {
//         if (healthBar != null)
//             healthBar.fillAmount = (float)currentHealth / maxHealth;
//     }

//     private void UpdateEnergyUI()
//     {
//         if (energyBar != null)
//             energyBar.fillAmount = (float)currentEnergy / maxEnergy;
//     }

//     private void UpdateHungerUI()
//     {
//         if (hungerBar != null)
//             hungerBar.fillAmount = (float)currentHunger / maxHunger;
//     }

//     private void OnDestroy()
//     {
//         SceneManager.sceneLoaded -= OnSceneLoaded;
//     }

//     // Public accessors
//     public int CurrentHealth => currentHealth;
//     public int CurrentEnergy => currentEnergy;
//     public int CurrentHunger => currentHunger;
    
//     // Save/Load methods
//     public void SetHealth(int health)
//     {
//         currentHealth = Mathf.Clamp(health, 0, maxHealth);
//         UpdateHealthUI();
//     }
    
//     public void SetEnergy(int energy)
//     {
//         currentEnergy = Mathf.Clamp(energy, 0, maxEnergy);
//         UpdateEnergyUI();
//     }
    
//     public void SetHunger(int hunger)
//     {
//         currentHunger = Mathf.Clamp(hunger, 0, maxHunger);
//         UpdateHungerUI();
//     }
// }
