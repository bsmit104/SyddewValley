using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Shop", menuName = "Shop/Shop Data")]
public class ShopData : ScriptableObject
{
    public string shopName;
    public List<Item> availableItems = new List<Item>();
    public Sprite shopIcon; // Optional: for different shop UI themes
} 