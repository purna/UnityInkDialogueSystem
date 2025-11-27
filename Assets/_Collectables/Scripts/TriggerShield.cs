using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerShield : MonoBehaviour
{
    public PlayerUpgrades playerUpgrades;

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if collider is player
        if (collision.CompareTag("Player"))
        {
            // Check if shield is not already unlocked
            if (!playerUpgrades.IsUpgradeActive("Shield"))
            {
                // Unlock the shield ability when the player reaches a checkpoint
                bool success = playerUpgrades.UnlockUpgrade("Shield");
                
                if (success)
                {
                    Debug.Log("Shield upgrade unlocked!");
                }
                else
                {
                    Debug.LogWarning("Failed to unlock Shield upgrade!");
                }
            }
            else
            {
                Debug.Log("Shield upgrade already unlocked!");
            }
        }
    }
}