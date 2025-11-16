using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Game
{
    public class CurrencyManager : MonoBehaviour
    {
        // Standardized singleton pattern
        public static CurrencyManager Instance { get; private set; }

        // Event to decouple from HUDManager
        public static event UnityAction<int> OnCurrencyChanged;

        [SerializeField] public int CurrentCurrency { get; private set; }

        [SerializeField] public int _CurrentCurrency;

        private void Awake()
        {
            if (transform.parent != null)
            {
                Debug.LogWarning($"{nameof(CurrencyManager)} must be attached to a root GameObject for DontDestroyOnLoad to work.");
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
            // Initialize currency
            _CurrentCurrency = CurrentCurrency;
        }

        public void IncrementCurrency(int amount)
        {
            CurrentCurrency += amount;
            
            // Notify listeners about currency change instead of directly calling HUDManager
            OnCurrencyChanged?.Invoke(CurrentCurrency);
        }
    }
}
