using UnityEngine;
using Core.Game;
using System.Collections.Generic;

public class InventoryUIManager : MonoBehaviour
{
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private List<CollectableUpgradeSO> collectedItems = new();


    private void OnEnable()
    {
        InventoryManager.OnInventoryUpdated += HandleInventoryUpdate;
        InventoryManager.OnInventoryVisibilityChanged += HandleInventoryVisibilityChanged;

        if (InventoryManager.Instance != null)
        {
            HandleInventoryUpdate(InventoryManager.Instance.GetItems());
            HandleInventoryVisibilityChanged(InventoryManager.Instance.IsInventoryVisible);
        }
    }

    private void OnDisable()
    {
        InventoryManager.OnInventoryUpdated -= HandleInventoryUpdate;
        InventoryManager.OnInventoryVisibilityChanged -= HandleInventoryVisibilityChanged;
    }

    private void HandleInventoryUpdate(List<CollectableUpgradeSO> items)
    {
        collectedItems = items;
        if (inventoryUI != null)
        {
            var itemCounts = new Dictionary<CollectableUpgradeSO, int>();
            foreach (var item in items)
            {
                if (itemCounts.ContainsKey(item))
                {
                    itemCounts[item]++;
                }
                else
                {
                    itemCounts[item] = 1;
                }
            }
            inventoryUI.UpdateUI(itemCounts);
        }
    }

    private void HandleInventoryVisibilityChanged(bool isVisible)
    {
        if (inventoryUI == null)
        {
            Debug.LogError("InventoryUIManager: inventoryUI is not assigned!");
            return;
        }

        Debug.Log($"HandleInventoryVisibilityChanged called with: {isVisible}");
        inventoryUI.SetVisibility(isVisible);
    }
}