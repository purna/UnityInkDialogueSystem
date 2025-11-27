using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/*
// Add a skill point collectable
InventoryManager.Instance.AddItem(mySkillPointsSO);

// Unlock a skill tree key
InventoryManager.Instance.AddItem(myKeySO);

// Check if player has a specific upgrade
if (InventoryManager.Instance.HasItem("HealthUpgrade")) { ... }

// Get all skill tree keys
var keys = InventoryManager.Instance.GetSkillTreeKeys();

// Count how many of a specific bonus function the player has
int bonusCount = InventoryManager.Instance.GetItemCount("DoubleXPBonus");

// Remove an item
InventoryManager.Instance.RemoveItemByName("FireballAbility");


*/

namespace Core.Game
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        // Events
        public static event UnityAction<string> OnCollectableChanged;
        public static event UnityAction<List<CollectableUpgradeSO>> OnInventoryUpdated; // upgrades only
        public static event UnityAction<List<CollectableSOBase>> OnAllItemsUpdated;      // all collectables
        public static event UnityAction<bool> OnInventoryVisibilityChanged;

        [Header("Input Settings")]
        [SerializeField] private KeyCode inventoryKey = KeyCode.I;

        public string CurrentCollectable { get; private set; }
        public bool IsInventoryVisible { get; private set; } = false;

        // Backward-compatible upgrade list
        private List<CollectableUpgradeSO> collectedItems = new();

        // All collectables list (includes all types)
        private List<CollectableSOBase> allCollectedItems = new();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(inventoryKey))
            {
                ToggleInventory();
            }
        }

        public void ToggleInventory()
        {
            IsInventoryVisible = !IsInventoryVisible;
            OnInventoryVisibilityChanged?.Invoke(IsInventoryVisible);
            Debug.Log($"Inventory {(IsInventoryVisible ? "Opened" : "Closed")}");
        }

        public void ShowInventory()
        {
            if (!IsInventoryVisible)
            {
                IsInventoryVisible = true;
                OnInventoryVisibilityChanged?.Invoke(IsInventoryVisible);
            }
        }

        public void HideInventory()
        {
            if (IsInventoryVisible)
            {
                IsInventoryVisible = false;
                OnInventoryVisibilityChanged?.Invoke(IsInventoryVisible);
            }
        }

        public void UpdateInventory(string newItem)
        {
            CurrentCollectable = newItem;
            OnCollectableChanged?.Invoke(CurrentCollectable);
        }

        // Add any collectable type
        public void AddItem(CollectableSOBase item)
        {
            if (item == null) return;

            allCollectedItems.Add(item);
            OnAllItemsUpdated?.Invoke(new List<CollectableSOBase>(allCollectedItems));

            if (item is CollectableUpgradeSO upgrade)
            {
                collectedItems.Add(upgrade);
                OnInventoryUpdated?.Invoke(new List<CollectableUpgradeSO>(collectedItems));
            }

            Debug.Log($"Added {item.ItemName} to inventory");
        }

        // Backward-compatible method for upgrades
        public void AddItem(CollectableUpgradeSO item) => AddItem((CollectableSOBase)item);

        // Remove any collectable type
        public void RemoveItem(CollectableSOBase item)
        {
            if (item == null) return;

            if (allCollectedItems.Remove(item))
            {
                OnAllItemsUpdated?.Invoke(new List<CollectableSOBase>(allCollectedItems));
                Debug.Log($"Removed {item.ItemName} from inventory");
            }

            if (item is CollectableUpgradeSO upgrade && collectedItems.Contains(upgrade))
            {
                collectedItems.Remove(upgrade);
                OnInventoryUpdated?.Invoke(new List<CollectableUpgradeSO>(collectedItems));
            }
        }

        public void RemoveItem(CollectableUpgradeSO item) => RemoveItem((CollectableSOBase)item);

        public bool RemoveItemByName(string itemName)
        {
            for (int i = 0; i < allCollectedItems.Count; i++)
            {
                if (allCollectedItems[i]?.ItemName == itemName)
                {
                    var item = allCollectedItems[i];
                    allCollectedItems.RemoveAt(i);
                    OnAllItemsUpdated?.Invoke(new List<CollectableSOBase>(allCollectedItems));

                    if (item is CollectableUpgradeSO upgrade)
                    {
                        collectedItems.Remove(upgrade);
                        OnInventoryUpdated?.Invoke(new List<CollectableUpgradeSO>(collectedItems));
                    }

                    Debug.Log($"Removed {itemName} from inventory");
                    return true;
                }
            }
            return false;
        }

        // Get all upgrades (legacy)
        public List<CollectableUpgradeSO> GetItems() => new(collectedItems);

        // Get all collectables
        public List<CollectableSOBase> GetAllItems() => new(allCollectedItems);

        // Get collectables of a specific type
        public List<T> GetItemsOfType<T>() where T : CollectableSOBase
        {
            List<T> result = new();
            foreach (var item in allCollectedItems)
                if (item is T typedItem) result.Add(typedItem);
            return result;
        }

        public bool HasItem(string itemName)
        {
            foreach (var item in allCollectedItems)
                if (item?.ItemName == itemName) return true;
            return false;
        }

        public bool HasItem(CollectableSOBase item) => allCollectedItems.Contains(item);

        public int GetItemCount(string itemName)
        {
            int count = 0;
            foreach (var item in allCollectedItems)
                if (item?.ItemName == itemName) count++;
            return count;
        }

        public List<CollectableSkillTreeKeySO> GetSkillTreeKeys() => GetItemsOfType<CollectableSkillTreeKeySO>();

        public bool HasSkillTreeKey(string keyName)
        {
            foreach (var key in GetSkillTreeKeys())
                if (key.ItemName == keyName || key.SkillTreeGroupName == keyName) return true;
            return false;
        }

        public void ClearInventory()
        {
            collectedItems.Clear();
            allCollectedItems.Clear();
            OnInventoryUpdated?.Invoke(new List<CollectableUpgradeSO>());
            OnAllItemsUpdated?.Invoke(new List<CollectableSOBase>());
            Debug.Log("Inventory cleared");
        }
    }
}
