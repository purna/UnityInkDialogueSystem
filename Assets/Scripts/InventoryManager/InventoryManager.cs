using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Game
{
    public class InventoryManager : MonoBehaviour
    {
        // Standardized singleton pattern
        public static InventoryManager Instance { get; private set; }

        // Event to decouple from HUDManager
        public static event UnityAction<string> OnCollectableChanged;

        // Event for full inventory update
        public static event UnityAction<List<CollectableUpgradeSO>> OnInventoryUpdated;

        // Event for inventory visibility change
        public static event UnityAction<bool> OnInventoryVisibilityChanged;

        [Header("Input Settings")]
        [SerializeField] private KeyCode inventoryKey = KeyCode.I;

        public string CurrentCollectable { get; private set; }
        public bool IsInventoryVisible { get; private set; } = false;

        private List<CollectableUpgradeSO> collectedItems = new();

        private void Awake()
        {
            if (transform.parent != null)
            {
                Debug.LogWarning($"{nameof(InventoryManager)} must be attached to a root GameObject for DontDestroyOnLoad to work.");
            }

            // Implement proper singleton pattern
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
            // Check for inventory toggle input
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
            Debug.Log("Showing Inventory. Current state: " + IsInventoryVisible);

            if (!IsInventoryVisible)
            {
                IsInventoryVisible = true;
                OnInventoryVisibilityChanged?.Invoke(IsInventoryVisible);
            }
        }

        public void HideInventory()
        {
            Debug.Log("Hiding Inventory. Current state: " + IsInventoryVisible);

            if (IsInventoryVisible)
            {
                IsInventoryVisible = false;
                OnInventoryVisibilityChanged?.Invoke(IsInventoryVisible);
            }
        }

        public void UpdateInventory(string newItem)
        {
            CurrentCollectable = newItem;

            // Notify listeners about collectable change instead of directly calling HUDManager
            OnCollectableChanged?.Invoke(CurrentCollectable);
        }

        public void AddItem(CollectableUpgradeSO item)
        {
            collectedItems.Add(item);
            OnInventoryUpdated?.Invoke(new List<CollectableUpgradeSO>(collectedItems));
        }

        public void RemoveItem(CollectableUpgradeSO item)
        {
            if (collectedItems.Contains(item))
            {
                collectedItems.Remove(item);
                OnInventoryUpdated?.Invoke(new List<CollectableUpgradeSO>(collectedItems));
            }
        }
        
        public List<CollectableUpgradeSO> GetItems()
        {
            return new List<CollectableUpgradeSO>(collectedItems);
        }
    }
}