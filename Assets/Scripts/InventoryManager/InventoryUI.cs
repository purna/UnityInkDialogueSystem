using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Core.Game;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject itemTextPrefab;
    [SerializeField] private Transform itemListParent;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text itemHeaderText;
    [SerializeField] private TMP_Text itemDescriptionText;
    [SerializeField] private Image itemImage;
    [SerializeField] private PlayerUpgrades playerUpgrades;
  
    private int selectedItemIndex = -1;
    private List<CollectableUpgradeSO> currentItems = new();

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Validate references
        ValidateReferences();
    }

    private void ValidateReferences()
    {
        if (itemHeaderText == null)
            Debug.LogError("InventoryUI: itemHeaderText is not assigned!");
        if (itemDescriptionText == null)
            Debug.LogError("InventoryUI: itemDescriptionText is not assigned!");
        if (itemImage == null)
            Debug.LogWarning("InventoryUI: itemImage is not assigned (optional)");
        if (playerUpgrades == null)
            Debug.LogError("InventoryUI: playerUpgrades is not assigned!");
        if (itemTextPrefab == null)
            Debug.LogError("InventoryUI: itemTextPrefab is not assigned!");
        if (itemListParent == null)
            Debug.LogError("InventoryUI: itemListParent is not assigned!");
    }

    private void Update()
    {
        if (canvasGroup.alpha == 1) // Only allow selection when inventory is visible
        {
            HandleItemSelection();
        }
    }

    public void SetVisibility(bool isVisible)
    {
        Debug.Log($"Setting inventory visibility to: {isVisible}");
        canvasGroup.alpha = isVisible ? 1 : 0;
        canvasGroup.interactable = isVisible;
        canvasGroup.blocksRaycasts = isVisible;
    }

    public void UpdateUI(Dictionary<ScriptableObject, int> itemCounts)

    {
        if (itemListParent == null || itemTextPrefab == null)
        {
            Debug.LogError("InventoryUI: UI components not assigned!");
            return;
        }

        // Clear existing UI elements
        foreach (Transform child in itemListParent)
        {
            Destroy(child.gameObject);
        }

        currentItems.Clear();

        int index = 0;
        foreach (var itemPair in itemCounts)
        {
            if (itemPair.Key is CollectableUpgradeSO upgrade)
            {
                int count = itemPair.Value;
                currentItems.Add(upgrade);

                // Instantiate UI element for upgrade
                GameObject itemGO = Instantiate(itemTextPrefab, itemListParent);
                itemGO.SetActive(true);

                // Initialize InventoryItemUI if present
                InventoryItemUI itemUI = itemGO.GetComponent<InventoryItemUI>();
                if (itemUI != null)
                {
                    itemUI.Initialize(this, upgrade, count, index);
                }
                       
                // Fallback in case InventoryItemUI is not attached
                Image itemIcon = itemGO.GetComponentInChildren<Image>();
                if (itemIcon != null && upgrade.ItemIcon != null)
                {
                    itemIcon.sprite = upgrade.ItemIcon;
                }

                TMP_Text[] textComponents = itemGO.GetComponentsInChildren<TMP_Text>();
                TMP_Text itemCountText = textComponents.Length > 1 ? textComponents[0] : null;
                TMP_Text itemNameText = itemGO.transform.Find("ItemName")?.GetComponent<TMP_Text>();
                TMP_Text itemDescriptionText = itemGO.transform.Find("ItemDescription")?.GetComponent<TMP_Text>();

                if (itemCountText == null && itemGO.transform.Find("count") != null)
                {
                    itemCountText = itemGO.transform.Find("count").GetComponent<TMP_Text>();
                }
                
                if (itemNameText != null)
                {
                    itemNameText.text = upgrade.ItemName;
                }

                if (itemDescriptionText != null)
                {
                    itemDescriptionText.text = upgrade.ItemDescription;
                }

                if (itemCountText != null)
                {
                    itemCountText.text = count > 1 ? count.ToString() : "";
                }
            }

            index++;
        }

        // Reset selection and details if empty
        if (currentItems.Count == 0)
        {
            selectedItemIndex = -1;
            UpdateDetails();
        }
        else
        {
            // Select first item by default
            if (selectedItemIndex < 0 || selectedItemIndex >= currentItems.Count)
                selectedItemIndex = 0;

            UpdateSelection();
            UpdateDetails();
        }
    }

    private void HandleItemSelection()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            selectedItemIndex = (selectedItemIndex + 1) % itemListParent.childCount;
            UpdateSelection();
            UpdateDetails();
        }

        for (int i = 0; i < itemListParent.childCount; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                selectedItemIndex = i;
                UpdateSelection();
                UpdateDetails();
                break;
            }
        }
        
        // Add activation key (e.g., Enter or Space to use/equip the selected item)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            ActivateSelectedItem();
        }
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < itemListParent.childCount; i++)
        {
            var item = itemListParent.GetChild(i);
            var image = item.GetComponent<Image>();
            if (image != null)
            {
                image.color = i == selectedItemIndex ? Color.yellow : Color.white;
            }
        }
    }

    // FIXED: Only display item info, don't unlock upgrades here
    private void UpdateDetails()
    {
        if (selectedItemIndex >= 0 && selectedItemIndex < currentItems.Count)
        {
            CollectableUpgradeSO selectedItem = currentItems[selectedItemIndex];
            
            // Update header text
            if (itemHeaderText != null)
            {
                itemHeaderText.text = selectedItem.ItemName;
            }
            
            // Update description text
            if (itemDescriptionText != null)
            {
                itemDescriptionText.text = selectedItem.ItemDescription;
            }
            
            // Update item image if assigned
            if (itemImage != null && selectedItem.ItemIcon != null)
            {
                itemImage.sprite = selectedItem.ItemIcon;
                itemImage.enabled = true;
            }
            
            // REMOVED: Don't unlock upgrades when just viewing details!
            // The upgrade should already be unlocked when the item is collected
        }
        else
        {
            // Clear details when no item selected
            if (itemHeaderText != null)
                itemHeaderText.text = "";
                
            if (itemDescriptionText != null)
                itemDescriptionText.text = "";
                
            if (itemImage != null)
                itemImage.enabled = false;
        }
    }

    // NEW: Separate method to activate/use selected item
    private void ActivateSelectedItem()
    {
        if (selectedItemIndex >= 0 && selectedItemIndex < currentItems.Count)
        {
            CollectableUpgradeSO selectedItem = currentItems[selectedItemIndex];
            
            if (playerUpgrades != null)
            {
                // Only unlock when player explicitly activates the item
                playerUpgrades.UnlockUpgrade(selectedItem.ItemName);
                Debug.Log($"Activated upgrade: {selectedItem.ItemName}");
            }
            else
            {
                Debug.LogError("PlayerUpgrades is not assigned in InventoryUI!");
            }
        }
    }

    public void SelectItem(int index)
    {
        if (index < 0 || index >= currentItems.Count) return;

        selectedItemIndex = index;
        UpdateSelection();
        UpdateDetails();
    }

    public void ShowItemDetails(CollectableUpgradeSO item)
    {
        Debug.Log($"Show details: {item.name}");

        if (item != null)
        {
            if (itemHeaderText != null)
            {
                itemHeaderText.text = item.ItemName;
                Debug.Log($"Updated headerText: {itemHeaderText.text}");
            }
            else
            {
                Debug.LogError("headerText is NULL!");
            }

            if (itemDescriptionText != null)
            {
                itemDescriptionText.text = item.ItemDescription;
                Debug.Log($"Updated descriptionText: {itemDescriptionText.text}");
            }
        }
    }

    public void HideItemDetails()
    {
        if (itemHeaderText != null)
            itemHeaderText.text = "";
            
        if (itemDescriptionText != null)
            itemDescriptionText.text = "";
            
        if (itemImage != null)
            itemImage.enabled = false;
    }

    public int GetSelectedItemIndex()
    {
        return selectedItemIndex;
    }
}