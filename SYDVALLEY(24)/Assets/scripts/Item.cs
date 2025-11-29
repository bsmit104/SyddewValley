using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public int maxStack;

    [Header("Shop Settings")]
    public bool isSellable = false;
    public int sellPrice = 0;
    public int buyPrice = 0;
    public bool isAvailableInShop = false;

    [Header("Food Settings")]
    public bool isFood = false;
    public int hungerRestore = 25; // how much hunger it restores

    [Header("Placement Settings")]
    public bool isPlaceable = false; // whether item can be placed in world
}

// using UnityEngine;

// [CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
// public class Item : ScriptableObject
// {
//     public string itemName;
//     public Sprite itemIcon;
//     public int maxStack;

//     [Header("Shop Settings")]
//     public bool isSellable = false;
//     public int sellPrice = 0;
//     public int buyPrice = 0;
//     public bool isAvailableInShop = false;

//     [Header("Food Settings")] // ← NEW
//     public bool isFood = false;
//     public int hungerRestore = 25; // ← NEW (how much hunger it restores)

//     // You can add more properties here, like item ID, description, etc.
// }


















// using UnityEngine;

// // This attribute allows us to create new items from the Unity editor
// [CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
// public class Item : ScriptableObject
// {
//     public string itemName; // Name of the item
//     public Sprite itemIcon; // Icon representing the item in the UI
//     public int maxStack; // Maximum stack size for this item

//     [Header("Shop Settings")]
//     public bool isSellable = false;
//     public int sellPrice = 0;
//     public int buyPrice = 0;  // If different from sell price
//     public bool isAvailableInShop = false;  // Whether this item can be bought from shops

//     // You can add more properties here, like item ID, description, etc.
// }