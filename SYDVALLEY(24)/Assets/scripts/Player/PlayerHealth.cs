using UnityEngine;
using UnityEngine.UI;

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
    public Image healthBg;
    public Image energyBg;
    public Image hungerBg;

    [Header("Hunger Drain")]
    public float hungerDrainInterval = 60f;

    [Header("Health Regen")]
    public float healthRegenInterval = 2f; // 1 health every 2 seconds when hunger full

    [Header("Inventory Reference")]
    public Inventory playerInventory;

    [Header("Testing Keys")]
    public KeyCode damageKey = KeyCode.P;
    public KeyCode energyKey = KeyCode.O;
    public KeyCode regenEnergyKey = KeyCode.I;
    public KeyCode hungerDrainKey = KeyCode.H;
    public KeyCode regenHungerKey = KeyCode.J;
    // public KeyCode eatKey = KeyCode.V; // ← REMOVED (now right-click)
    public KeyCode resetKey = KeyCode.R;

    private float hungerDrainTimer = 0f;
    private float healthRegenTimer = 0f;

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
        // 🥩 HUNGER DRAIN
        hungerDrainTimer += Time.deltaTime;
        if (hungerDrainTimer >= hungerDrainInterval)
        {
            TakeHungerDamage(1);
            hungerDrainTimer = 0f;
            Debug.Log("🥩 Auto: Hunger drained by 1!");
        }

        // 🏥 PASSIVE HEALTH REGEN (when hunger FULL)
        if (currentHunger == maxHunger && currentHealth < maxHealth)
        {
            healthRegenTimer += Time.deltaTime;
            if (healthRegenTimer >= healthRegenInterval)
            {
                currentHealth = Mathf.Clamp(currentHealth + 1, 0, maxHealth);
                UpdateHealthUI();
                healthRegenTimer = 0f;
                Debug.Log("🏥 Passive health regen: +1 (hunger full!)");
            }
        }

        // 🎮 TESTING KEYS
        if (Input.GetKeyDown(damageKey))
        {
            TakeDamage(10);
            Debug.Log("🩸 P: Health -10!");
        }

        if (Input.GetKeyDown(energyKey))
        {
            if (UseEnergy(20))
                Debug.Log("⚡ O: Energy -20!");
            else
                Debug.Log("⚡ O: Not enough energy!");
        }

        if (Input.GetKeyDown(regenEnergyKey))
        {
            RegenerateEnergy(20);
            Debug.Log("🔋 I: Energy +20!");
        }

        if (Input.GetKeyDown(hungerDrainKey))
        {
            TakeHungerDamage(10);
            Debug.Log("🥩 H: Hunger -10!");
        }

        if (Input.GetKeyDown(regenHungerKey))
        {
            RegenerateHunger(20);
            Debug.Log("🍗 J: Hunger +20!");
        }

        // 🖱️ RIGHT-CLICK TO EAT SELECTED FOOD (NEW!)
        if (Input.GetMouseButtonDown(1))
        {
            EatSelectedFood();
        }

        if (Input.GetKeyDown(resetKey))
        {
            ResetAll();
            Debug.Log("🔄 R: ALL BARS FULL!");
        }

        // ⚡ Energy auto-regen
        if (Time.frameCount % 60 == 0)
        {
            RegenerateEnergy(1);
        }
    }

    private void EatSelectedFood()
    {
        if (playerInventory == null)
        {
            Debug.LogError("PlayerHealth: Inventory not assigned!");
            return;
        }

        Item selectedFood = playerInventory.GetSelectedItem();
        if (selectedFood == null)
        {
            Debug.Log("🍽️ Right-click: No item selected!");
            return;
        }

        if (!selectedFood.isFood)
        {
            Debug.Log($"🍽️ Right-click: {selectedFood.itemName} is not food!");
            return;
        }

        RegenerateHunger(selectedFood.hungerRestore);
        Debug.Log($"🍽️ Right-click: Ate {selectedFood.itemName}! +{selectedFood.hungerRestore} hunger");

        playerInventory.RemoveItem(selectedFood, 1);
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("💀 Player died!");
        }

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

    public int CurrentHealth => currentHealth;
    public int CurrentEnergy => currentEnergy;
    public int CurrentHunger => currentHunger;
}





// using UnityEngine;
// using UnityEngine.UI;

// public class PlayerHealth : MonoBehaviour
// {
//     [Header("Health Settings")]
//     public int maxHealth = 100;
//     [SerializeField] private int currentHealth;

//     [Header("Energy Settings")]
//     public int maxEnergy = 100;
//     [SerializeField] private int currentEnergy;

//     [Header("UI References")]
//     public Image healthBar;      // Red fill (vertical)
//     public Image energyBar;      // Green fill (vertical)
//     public Image healthBg;       // Black background
//     public Image energyBg;       // Black background

//     [Header("Testing Keys (Press in Play Mode)")]
//     public KeyCode damageKey = KeyCode.P;    // P = Take 10 damage
//     public KeyCode energyKey = KeyCode.O;    // O = Use 20 energy
//     public KeyCode regenKey = KeyCode.I;     // I = Regen 20 energy
//     public KeyCode resetKey = KeyCode.R;     // R = Reset to full

//     private void Start()
//     {
//         currentHealth = maxHealth;
//         currentEnergy = maxEnergy;
//         UpdateHealthUI();
//         UpdateEnergyUI();
//     }

//     private void Update()
//     {
//         // 🎮 TESTING KEYS (Hold Focus on Game View)
//         if (Input.GetKeyDown(damageKey))
//         {
//             TakeDamage(10);
//             Debug.Log("🩸 P Key: Took 10 damage!");
//         }

//         if (Input.GetKeyDown(energyKey))
//         {
//             if (UseEnergy(20))
//                 Debug.Log("⚡ O Key: Used 20 energy!");
//             else
//                 Debug.Log("⚡ O Key: Not enough energy!");
//         }

//         if (Input.GetKeyDown(regenKey))
//         {
//             RegenerateEnergy(20);
//             Debug.Log("🔋 I Key: Regenerated 20 energy!");
//         }

//         if (Input.GetKeyDown(resetKey))
//         {
//             currentHealth = maxHealth;
//             currentEnergy = maxEnergy;
//             UpdateHealthUI();
//             UpdateEnergyUI();
//             Debug.Log("🔄 R Key: Reset to FULL!");
//         }

//         // Optional: Slow auto-regen (1 energy/sec)
//         if (Time.frameCount % 60 == 0) // ~1/sec
//         {
//             RegenerateEnergy(1);
//         }
//     }

//     public void TakeDamage(int damageAmount)
//     {
//         currentHealth -= damageAmount;
//         currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

//         if (currentHealth <= 0)
//         {
//             Debug.Log("💀 Player has died!");
//             // Add death logic here (respawn, etc.)
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

//     // Public getters
//     public int CurrentHealth => currentHealth;
//     public int CurrentEnergy => currentEnergy;
// }
