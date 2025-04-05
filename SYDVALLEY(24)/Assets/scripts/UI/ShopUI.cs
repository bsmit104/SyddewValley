using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class ShopUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform shopItemsContainer;
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Shop Settings")]
    [SerializeField] private List<Item> availableItems = new List<Item>();
    [SerializeField] private Vector2 itemSize = new Vector2(100, 100);
    [SerializeField] private Vector2 itemSpacing = new Vector2(10, 10);
    [SerializeField] private int columnsCount = 4;
    
    private Inventory playerInventory;
    private Item selectedItem;
    private bool isSelling;

    void Start()
    {
        playerInventory = FindObjectOfType<Inventory>();
        
        // Ensure references are valid
        if (shopItemsContainer == null)
        {
            Debug.LogError("ShopUI: Shop items container is not assigned!");
            return;
        }
        
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("ShopUI: Confirmation panel is not assigned!");
        }

        // Find ScrollRect if not assigned
        if (scrollRect == null)
        {
            scrollRect = GetComponentInParent<ScrollRect>();
            if (scrollRect == null)
            {
                // Try to find it in the parent hierarchy
                Transform parent = transform.parent;
                while (parent != null && scrollRect == null)
                {
                    scrollRect = parent.GetComponent<ScrollRect>();
                    parent = parent.parent;
                }
                
                if (scrollRect == null)
                {
                    Debug.LogWarning("ShopUI: ScrollRect not found. Scrolling to top may not work.");
                }
            }
        }

        // Set up the shop layout
        SetupShopLayout();

        if (confirmButton != null && cancelButton != null)
        {
            confirmButton.onClick.AddListener(ConfirmTransaction);
            cancelButton.onClick.AddListener(() => confirmationPanel.SetActive(false));
        }
        else
        {
            Debug.LogWarning("ShopUI: Confirm or cancel button not assigned!");
        }

        // Wait a frame before populating items to ensure everything is initialized
        StartCoroutine(PopulateItemsNextFrame());
    }

    private System.Collections.IEnumerator PopulateItemsNextFrame()
    {
        yield return null; // Wait one frame
        PopulateShopItems();
    }

    private void SetupShopLayout()
    {
        // Make sure we have a layout group
        GridLayoutGroup layoutGroup = shopItemsContainer.GetComponent<GridLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = shopItemsContainer.gameObject.AddComponent<GridLayoutGroup>();
        }

        // Configure layout
        layoutGroup.cellSize = itemSize;
        layoutGroup.spacing = itemSpacing;
        layoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
        layoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
        layoutGroup.childAlignment = TextAnchor.UpperLeft;
        layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layoutGroup.constraintCount = columnsCount;

        // Set up content sizing
        ContentSizeFitter sizeFitter = shopItemsContainer.GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
        {
            sizeFitter = shopItemsContainer.gameObject.AddComponent<ContentSizeFitter>();
        }
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    public void RefreshShopItems()
    {
        PopulateShopItems();
    }

    private void PopulateShopItems()
    {
        // Clear existing items
        foreach (Transform child in shopItemsContainer)
        {
            Destroy(child.gameObject);
        }

        // Add available items
        foreach (Item item in availableItems)
        {
            if (item.isAvailableInShop)
            {
                CreateShopItemButton(item);
            }
        }

        // Make sure the content size is updated
        Canvas.ForceUpdateCanvases();
        
        // Safely check for scroll rect and reset position
        try 
        {
            if (scrollRect != null && scrollRect.isActiveAndEnabled && 
                scrollRect.content != null && scrollRect.content.gameObject.activeInHierarchy)
            {
                scrollRect.normalizedPosition = new Vector2(0, 1); // Scroll to top
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error setting scroll position: {e.Message}");
        }
    }

    private void CreateShopItemButton(Item item)
    {
        GameObject buttonObj = Instantiate(shopItemPrefab, shopItemsContainer);
        
        // Set up the button's visuals
        Image iconImage = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
        TextMeshProUGUI nameText = buttonObj.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI priceText = buttonObj.transform.Find("Price")?.GetComponent<TextMeshProUGUI>();
        
        if (iconImage) iconImage.sprite = item.itemIcon;
        if (nameText) nameText.text = item.itemName;
        if (priceText) priceText.text = $"${item.buyPrice:N0}";

        // Make sure the button has the right size
        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
        if (rectTransform)
        {
            rectTransform.sizeDelta = itemSize;
        }

        // Add click handler
        Button button = buttonObj.GetComponent<Button>();
        button.onClick.AddListener(() => ShowBuyConfirmation(item));
    }

    public void ShowBuyConfirmation(Item item)
    {
        selectedItem = item;
        isSelling = false;
        confirmationText.text = $"Buy {item.itemName} for ${item.buyPrice:N0}?";
        confirmationPanel.SetActive(true);
    }

    public void ShowSellConfirmation(Item item)
    {
        selectedItem = item;
        isSelling = true;
        confirmationText.text = $"Sell {item.itemName} for ${item.sellPrice:N0}?";
        confirmationPanel.SetActive(true);
    }

    private void ConfirmTransaction()
    {
        if (MoneyManager.Instance == null)
        {
            Debug.LogError("MoneyManager instance not found!");
            confirmationPanel.SetActive(false);
            return;
        }

        if (isSelling)
        {
            // Selling
            playerInventory.RemoveItem(selectedItem, 1);
            MoneyManager.Instance.AddMoney(selectedItem.sellPrice);
            Debug.Log($"Sold {selectedItem.itemName} for ${selectedItem.sellPrice}");
        }
        else
        {
            // Buying
            if (MoneyManager.Instance.SpendMoney(selectedItem.buyPrice))
            {
                playerInventory.AddItem(selectedItem);
                Debug.Log($"Bought {selectedItem.itemName} for ${selectedItem.buyPrice}");
            }
            else
            {
                // Not enough money
                Debug.Log("Not enough money to buy this item!");
            }
        }

        confirmationPanel.SetActive(false);
        
        // Force refresh inventory UI to update highlights
        var inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.UpdateInventoryUI();
        }
    }
} 