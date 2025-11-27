using UnityEngine;

public class PlayerUpgradesManager : MonoBehaviour
{
    private PlayerUpgrades playerUpgrades;

    private void Start()
    {
        playerUpgrades = FindObjectOfType<PlayerUpgrades>();
        
        if (playerUpgrades == null)
        {
            Debug.LogError("PlayerUpgrades not found in scene!");
        }
    }

    private void Update()
    {
        if (playerUpgrades == null) return;

        // Bomb upgrade
        if (Input.GetKeyDown(KeyCode.B))
        {
            bool success = playerUpgrades.UnlockUpgrade("Bomb");
            if (success)
            {
                Debug.Log("Bomb upgrade unlocked!");
            }
        }

        // Invisibility upgrade
        if (Input.GetKeyDown(KeyCode.I))
        {
            bool success = playerUpgrades.UnlockUpgrade("Invisibility");
            if (success)
            {
                Debug.Log("Invisibility upgrade unlocked!");
            }
        }

        // Shield upgrade
        if (Input.GetKeyDown(KeyCode.S))
        {
            bool success = playerUpgrades.UnlockUpgrade("Shield");
            if (success)
            {
                Debug.Log("Shield upgrade unlocked!");
            }
        }

        // Staff upgrade
        if (Input.GetKeyDown(KeyCode.T))
        {
            bool success = playerUpgrades.UnlockUpgrade("Staff");
            if (success)
            {
                Debug.Log("Staff upgrade unlocked!");
            }
        }

        // Prayer upgrade
        if (Input.GetKeyDown(KeyCode.P))
        {
            bool success = playerUpgrades.UnlockUpgrade("Prayer");
            if (success)
            {
                Debug.Log("Prayer upgrade unlocked!");
            }
        }

        // Lock current upgrade (useful for testing)
        if (Input.GetKeyDown(KeyCode.L))
        {
            var activeUpgrades = playerUpgrades.GetActiveUpgradeNames();
            if (activeUpgrades.Count > 0)
            {
                string upgradeToLock = activeUpgrades[0];
                playerUpgrades.LockUpgrade(upgradeToLock);
                Debug.Log($"{upgradeToLock} upgrade locked!");
            }
        }

        // Display active upgrades
        if (Input.GetKeyDown(KeyCode.D))
        {
            var activeUpgrades = playerUpgrades.GetActiveUpgradeNames();
            if (activeUpgrades.Count > 0)
            {
                Debug.Log($"Active Upgrades: {string.Join(", ", activeUpgrades)}");
            }
            else
            {
                Debug.Log("No active upgrades");
            }
        }
    }
}