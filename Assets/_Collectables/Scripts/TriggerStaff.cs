using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerStaff : MonoBehaviour
{
    public PlayerUpgrades playerUpgrades;

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if collider is player
        if (collision.CompareTag("Player"))
        {
            // Check if staff is not already unlocked
            if (!playerUpgrades.IsUpgradeActive("Staff"))
            {
                // Unlock the staff ability when the player reaches a checkpoint
                bool success = playerUpgrades.UnlockUpgrade("Staff");
                
                if (success)
                {
                    Debug.Log("Staff upgrade unlocked!");
                }
                else
                {
                    Debug.LogWarning("Failed to unlock Staff upgrade!");
                }
            }
            else
            {
                Debug.Log("Staff upgrade already unlocked!");
            }
        }
    }
}