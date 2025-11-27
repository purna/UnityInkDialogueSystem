using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

/// <summary>
/// Base interface for all player upgrade abilities
/// </summary>
public interface IPlayerUpgrade
{
    /// <summary>
    /// The unique ID/Name of this upgrade (e.g., "Bomb", "DoubleJump").
    /// Matches the string used in Skill ScriptableObjects.
    /// </summary>
    string UpgradeName { get; }

    /// <summary>
    /// Tracks if the upgrade is currently enabled for the player.
    /// </summary>
    bool IsActive { get; set; }

    /// <summary>
    /// Logic to run when this upgrade is unlocked or equipped.
    /// </summary>
    void Activate();

    /// <summary>
    /// Logic to run when this upgrade is removed or disabled.
    /// </summary>
    void Deactivate();
}

/// <summary>
/// Centralized upgrade management system
/// Manages active and inactive upgrades with easy activation/deactivation
/// </summary>
public class PlayerUpgrades : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text UIText;

    // Lists to track upgrade states
    private List<IPlayerUpgrade> activeUpgrades = new List<IPlayerUpgrade>();
    private List<IPlayerUpgrade> inactiveUpgrades = new List<IPlayerUpgrade>();
    private Dictionary<string, IPlayerUpgrade> allUpgrades = new Dictionary<string, IPlayerUpgrade>();

    [Header("Upgrade Limit")]
    [SerializeField] private int maxActiveUpgrades = 1; // Set to 1 for single active upgrade, or higher for multiple

    private void Awake()
    {
        // Discover all upgrade components on this GameObject
        RegisterUpgrade(GetComponent<PlayerBomb>());
        RegisterUpgrade(GetComponent<PlayerInvisibility>());
        RegisterUpgrade(GetComponent<PlayerShield>());
        RegisterUpgrade(GetComponent<PlayerStaff>());
        RegisterUpgrade(GetComponent<PlayerPrayer>());
    }

    private void Update()
    {
        UpdateUI();
    }

    /// <summary>
    /// Register an upgrade component with the manager
    /// </summary>
    private void RegisterUpgrade(IPlayerUpgrade upgrade)
    {
        if (upgrade != null)
        {
            allUpgrades[upgrade.UpgradeName] = upgrade;
            inactiveUpgrades.Add(upgrade);
            upgrade.Deactivate(); // Start all upgrades as inactive
        }
    }

    /// <summary>
    /// Unlock and activate an upgrade by name
    /// </summary>
    public bool UnlockUpgrade(string upgradeName)
    {
        if (!allUpgrades.ContainsKey(upgradeName))
        {
            Debug.LogWarning($"Upgrade '{upgradeName}' not found.");
            return false;
        }

        IPlayerUpgrade upgrade = allUpgrades[upgradeName];

        // Check if already active
        if (activeUpgrades.Contains(upgrade))
        {
            Debug.Log($"Upgrade '{upgradeName}' is already active.");
            return false;
        }

        // Check if we've reached the max active upgrades limit
        if (activeUpgrades.Count >= maxActiveUpgrades)
        {
            // Deactivate the oldest active upgrade to make room
            DeactivateOldestUpgrade();
        }

        // Move from inactive to active
        if (inactiveUpgrades.Contains(upgrade))
        {
            inactiveUpgrades.Remove(upgrade);
        }
        
        activeUpgrades.Add(upgrade);
        upgrade.Activate();
        
        Debug.Log($"Upgrade '{upgradeName}' activated!");
        return true;
    }

    /// <summary>
    /// Lock/deactivate an upgrade by name
    /// </summary>
    public bool LockUpgrade(string upgradeName)
    {
        if (!allUpgrades.ContainsKey(upgradeName))
        {
            Debug.LogWarning($"Upgrade '{upgradeName}' not found.");
            return false;
        }

        IPlayerUpgrade upgrade = allUpgrades[upgradeName];

        // Check if it's active
        if (!activeUpgrades.Contains(upgrade))
        {
            Debug.Log($"Upgrade '{upgradeName}' is not currently active.");
            return false;
        }

        // Move from active to inactive
        activeUpgrades.Remove(upgrade);
        if (!inactiveUpgrades.Contains(upgrade))
        {
            inactiveUpgrades.Add(upgrade);
        }
        
        upgrade.Deactivate();
        
        Debug.Log($"Upgrade '{upgradeName}' deactivated!");
        return true;
    }

    /// <summary>
    /// Deactivate the oldest active upgrade (used when hitting max limit)
    /// </summary>
    private void DeactivateOldestUpgrade()
    {
        if (activeUpgrades.Count > 0)
        {
            IPlayerUpgrade oldestUpgrade = activeUpgrades[0];
            LockUpgrade(oldestUpgrade.UpgradeName);
        }
    }

    /// <summary>
    /// Check if a specific upgrade is currently active
    /// </summary>
    public bool IsUpgradeActive(string upgradeName)
    {
        if (allUpgrades.ContainsKey(upgradeName))
        {
            return activeUpgrades.Contains(allUpgrades[upgradeName]);
        }
        return false;
    }

    /// <summary>
    /// Get all currently active upgrade names
    /// </summary>
    public List<string> GetActiveUpgradeNames()
    {
        return activeUpgrades.Select(u => u.UpgradeName).ToList();
    }

    /// <summary>
    /// Get all inactive upgrade names
    /// </summary>
    public List<string> GetInactiveUpgradeNames()
    {
        return inactiveUpgrades.Select(u => u.UpgradeName).ToList();
    }

    /// <summary>
    /// Update UI to show active upgrades
    /// </summary>
    private void UpdateUI()
    {
        if (UIText != null)
        {
            if (activeUpgrades.Count > 0)
            {
                string upgradeNames = string.Join(", ", GetActiveUpgradeNames());
                UIText.text = $"Active: {upgradeNames}";
            }
            else
            {
                UIText.text = "No active upgrades";
            }
        }
    }

    /// <summary>
    /// Get the currently active upgrade (if only one is allowed)
    /// </summary>
    public IPlayerUpgrade GetActiveUpgrade()
    {
        return activeUpgrades.Count > 0 ? activeUpgrades[0] : null;
    }

    // Public properties for backward compatibility (optional)
    public bool BombUpgradeUnlocked => IsUpgradeActive("Bomb");
    public bool InvisibilityUpgradeUnlocked => IsUpgradeActive("Invisibility");
    public bool ShieldUpgradeUnlocked => IsUpgradeActive("Shield");
    public bool StaffUpgradeUnlocked => IsUpgradeActive("Staff");
    public bool PrayerUpgradeUnlocked => IsUpgradeActive("Prayer");
}