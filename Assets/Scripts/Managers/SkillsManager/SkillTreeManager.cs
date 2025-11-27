using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Example of how a skill tree would interact with the new PlayerUpgrades system
/// </summary>
public class SkillTreeManager : MonoBehaviour
{
    // Singleton Instance
    public static SkillTreeManager Instance { get; private set; }

    [SerializeField] private PlayerUpgrades playerUpgrades;

    private void Awake()
    {
        // Basic Singleton setup so the ScriptableObject can find it
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (playerUpgrades == null)
        {
            playerUpgrades = FindObjectOfType<PlayerUpgrades>();
        }
    }

    /// <summary>
    /// Called by UnlockAbilityFunction.cs
    /// Bridges the Skill Tree asset to the PlayerUpgrades system.
    /// </summary>
    public void UnlockAbility(string abilityID)
    {
        if (playerUpgrades != null)
        {
            bool success = playerUpgrades.UnlockUpgrade(abilityID);
            
            if (success)
            {
                Debug.Log($"[SkillTreeManager] Ability '{abilityID}' unlocked via Skill Tree.");
            }
        }
        else
        {
            Debug.LogError("[SkillTreeManager] PlayerUpgrades reference is missing!");
        }
    }

    // ========== Skill Tree Node Methods ==========
    
    /// <summary>
    /// Called when player clicks on a skill tree node
    /// </summary>
    public void OnSkillNodeClicked(string upgradeName)
    {
        // Check if player has enough skill points, currency, etc.
        if (CanAffordUpgrade(upgradeName))
        {
            bool success = playerUpgrades.UnlockUpgrade(upgradeName);
            
            if (success)
            {
                Debug.Log($"Successfully unlocked {upgradeName}!");
                // Deduct cost, play animation, etc.
                DeductUpgradeCost(upgradeName);
            }
        }
        else
        {
            Debug.Log($"Cannot afford {upgradeName} upgrade!");
        }
    }

    /// <summary>
    /// Switch between upgrades easily
    /// </summary>
    public void SwitchToUpgrade(string upgradeName)
    {
        // This will automatically deactivate the current upgrade and activate the new one
        playerUpgrades.UnlockUpgrade(upgradeName);
    }

    // ========== Query Methods ==========
    
    /// <summary>
    /// Get all currently active upgrades for display in UI
    /// </summary>
    public List<string> GetActiveUpgrades()
    {
        return playerUpgrades.GetActiveUpgradeNames();
    }

    /// <summary>
    /// Get all available but inactive upgrades
    /// </summary>
    public List<string> GetInactiveUpgrades()
    {
        return playerUpgrades.GetInactiveUpgradeNames();
    }

    /// <summary>
    /// Check if a specific upgrade is active
    /// </summary>
    public bool IsUpgradeActive(string upgradeName)
    {
        return playerUpgrades.IsUpgradeActive(upgradeName);
    }

    // ========== Helper Methods (implement based on your game's economy) ==========
    
    private bool CanAffordUpgrade(string upgradeName)
    {
        // Check if player has enough currency/skill points
        // This is just a placeholder - implement based on your game
        return true;
    }

    private void DeductUpgradeCost(string upgradeName)
    {
        // Deduct the cost from player's resources
        // This is just a placeholder - implement based on your game
    }

    // ========== Testing/Debug Methods ==========
    
    private void Update()
    {
        // Quick testing keys
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            //UnlockBomb();
             playerUpgrades.LockUpgrade("Bomb");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            //UnlockInvisibility();
            playerUpgrades.LockUpgrade("Invisibility");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            //UnlockShield();
            playerUpgrades.LockUpgrade("Shield");
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            //UnlockStaff();
            playerUpgrades.LockUpgrade("Staff");
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            //UnlockPrayer();
            playerUpgrades.LockUpgrade("Prayer");
        }

        // Display active upgrades
        if (Input.GetKeyDown(KeyCode.L))
        {
            LogActiveUpgrades();
        }
    }

    private void LogActiveUpgrades()
    {
        List<string> active = GetActiveUpgrades();
        Debug.Log($"Active Upgrades: {string.Join(", ", active)}");
        
        List<string> inactive = GetInactiveUpgrades();
        Debug.Log($"Inactive Upgrades: {string.Join(", ", inactive)}");
    }
}