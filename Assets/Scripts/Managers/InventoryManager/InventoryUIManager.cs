using System.Collections.Generic;
using UnityEngine;
using Core.Game;

public class InventoryUIManager : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private InventoryUI inventoryUI;

    [Header("Collected Items (Inspector Preview)")]
    [SerializeField] private List<CollectableUpgradeSO> collectedUpgrades = new();
    [SerializeField] private List<CollectableSkillTreeUpgradeSO> skillTreeUpgrades = new();
    [SerializeField] private List<CollectableSkillTreeKeySO> skillTreeKeys = new();
    [SerializeField] private List<CollectableSkillPointsSO> skillPoints = new();
    [SerializeField] private List<CollectableCurrencySO> currencies = new();

    private void OnEnable()
    {
        InventoryManager.OnAllItemsUpdated += HandleInventoryUpdate;
        InventoryManager.OnInventoryVisibilityChanged += HandleInventoryVisibilityChanged;

        if (InventoryManager.Instance != null)
        {
            HandleInventoryUpdate(InventoryManager.Instance.GetAllItems());
            HandleInventoryVisibilityChanged(InventoryManager.Instance.IsInventoryVisible);
        }
    }

    private void OnDisable()
    {
        InventoryManager.OnAllItemsUpdated -= HandleInventoryUpdate;
        InventoryManager.OnInventoryVisibilityChanged -= HandleInventoryVisibilityChanged;
    }

    private void HandleInventoryUpdate(List<CollectableSOBase> items)
    {
        // Clear previous lists
        collectedUpgrades.Clear();
        skillTreeUpgrades.Clear();
        skillTreeKeys.Clear();
        skillPoints.Clear();
        currencies.Clear();

        // Sort items into lists
        foreach (var item in items)
        {
            if (item is CollectableUpgradeSO upgrade)
                collectedUpgrades.Add(upgrade);
            else if (item is CollectableSkillTreeUpgradeSO skillUpgrade)
                skillTreeUpgrades.Add(skillUpgrade);
            else if (item is CollectableSkillTreeKeySO key)
                skillTreeKeys.Add(key);
            else if (item is CollectableSkillPointsSO points)
                skillPoints.Add(points);
            else if (item is CollectableCurrencySO currency)
                currencies.Add(currency);
            else
                Debug.LogWarning($"Unknown collectable type: {item.name}");
        }

        // Update InventoryUI dictionary
        // Update InventoryUI dictionary
        if (inventoryUI != null)
        {
            var itemCounts = new Dictionary<ScriptableObject, int>();

            // Use an array instead of trying to instantiate IEnumerable directly
            var lists = new IEnumerable<ScriptableObject>[] {
                collectedUpgrades, skillTreeUpgrades, skillTreeKeys, skillPoints, currencies
            };

            foreach (var list in lists)
            {
                foreach (var obj in list)
                {
                    if (itemCounts.ContainsKey(obj))
                        itemCounts[obj]++;
                    else
                        itemCounts[obj] = 1;
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

        inventoryUI.SetVisibility(isVisible);
    }
}
