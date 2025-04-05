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
    [SerializeField] private Vector2 itemSize = new Vector2(60, 60);
    [SerializeField] private Vector2 itemSpacing = new Vector2(2, 2);
    [SerializeField] private int columnsCount = 5;
    
    private Inventory playerInventory;
    private Item selectedItem;
    private bool isSelling;
    private GridLayoutGroup layoutGroup;

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
        layoutGroup = shopItemsContainer.GetComponent<GridLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = shopItemsContainer.gameObject.AddComponent<GridLayoutGroup>();
        }

        // Configure layout for more compact display
        layoutGroup.cellSize = itemSize;
        layoutGroup.spacing = itemSpacing;
        layoutGroup.padding = new RectOffset(2, 2, 2, 2);
        layoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
        layoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
        layoutGroup.childAlignment = TextAnchor.UpperLeft;
        layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layoutGroup.constraintCount = columnsCount;

        // IMPORTANT: For proper scrolling do NOT use ContentSizeFitter
        // Remove ContentSizeFitter if it exists - it prevents proper scrolling
        ContentSizeFitter sizeFitter = shopItemsContainer.GetComponent<ContentSizeFitter>();
        if (sizeFitter != null)
        {
            DestroyImmediate(sizeFitter);
        }
        
        // Ensure the container has correct settings for scrolling
        RectTransform containerRect = shopItemsContainer.GetComponent<RectTransform>();
        if (containerRect)
        {
            // Reset anchors for proper layout
            containerRect.anchorMin = new Vector2(0, 1);
            containerRect.anchorMax = new Vector2(1, 1);
            containerRect.pivot = new Vector2(0.5f, 1);
            
            // Make sure the width matches parent width
            containerRect.offsetMin = new Vector2(0, 0);
            containerRect.offsetMax = new Vector2(0, 0);
        }
        
        // Double check scroll rect is properly set up
        if (scrollRect != null)
        {
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.scrollSensitivity = 20;
            
            // Make sure content is assigned
            if (scrollRect.content == null)
            {
                scrollRect.content = containerRect;
            }
            
            // Set scrollbar visibility
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        }
        else
        {
            Debug.LogWarning("ScrollRect not assigned - shop items may not scroll properly");
        }
    }

    // Calculate the proper height for the content based on items
    private void UpdateContentSize()
    {
        if (shopItemsContainer == null) return;
        
        RectTransform containerRect = shopItemsContainer.GetComponent<RectTransform>();
        if (containerRect == null) return;
        
        // Calculate how many rows we need based on item count and columns
        int itemCount = 0;
        foreach (Item item in availableItems)
        {
            if (item.isAvailableInShop) itemCount++;
        }
        
        int rows = Mathf.CeilToInt((float)itemCount / columnsCount);
        
        // Calculate total height needed (rows * (itemHeight + spacing) - spacing + padding*2)
        float totalHeight = rows * (itemSize.y + itemSpacing.y) - itemSpacing.y + (layoutGroup?.padding?.top ?? 0) + (layoutGroup?.padding?.bottom ?? 0);
        
        // Set the height of content rect
        containerRect.sizeDelta = new Vector2(0, totalHeight);
        
        // Force layout update
        LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
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
        
        // Update the content size to match items
        UpdateContentSize();

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
        
        if (iconImage) 
        {
            iconImage.sprite = item.itemIcon;
            iconImage.preserveAspect = true;
            
            // Optimize icon placement
            RectTransform iconRect = iconImage.GetComponent<RectTransform>();
            if (iconRect)
            {
                // Make icon take up most of the button space
                float padding = itemSize.x * 0.1f; // 10% padding
                iconRect.offsetMin = new Vector2(padding, padding + (itemSize.y * 0.2f)); // Add extra bottom padding for text
                iconRect.offsetMax = new Vector2(-padding, -padding);
            }
        }
        
        if (nameText) 
        {
            nameText.text = item.itemName;
            nameText.fontSize = itemSize.x * 0.15f; // Scale font based on item size
            
            // Optimize name text placement
            RectTransform nameRect = nameText.GetComponent<RectTransform>();
            if (nameRect)
            {
                nameRect.anchorMin = new Vector2(0, 0);
                nameRect.anchorMax = new Vector2(1, 0.3f);
                nameRect.offsetMin = new Vector2(2, 2);
                nameRect.offsetMax = new Vector2(-2, 0);
            }
        }
        
        if (priceText) 
        {
            priceText.text = $"${item.buyPrice:N0}";
            priceText.fontSize = itemSize.x * 0.14f; // Scale font based on item size
            priceText.color = Color.yellow; // Make price stand out
            
            // Optimize price text placement
            RectTransform priceRect = priceText.GetComponent<RectTransform>();
            if (priceRect)
            {
                priceRect.anchorMin = new Vector2(0.6f, 0);
                priceRect.anchorMax = new Vector2(1, 0.25f);
                priceRect.offsetMin = new Vector2(0, 0);
                priceRect.offsetMax = new Vector2(-2, 0);
            }
        }

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