using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerInvisible : MonoBehaviour
{
    public PlayerUpgrades playerUpgrades;

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if collider is player
        if (collision.CompareTag("Player"))
        {
            // Check if invisibility is not already unlocked
            if (!playerUpgrades.IsUpgradeActive("Invisibility"))
            {
                // Unlock the invisibility ability when the player reaches a checkpoint
                bool success = playerUpgrades.UnlockUpgrade("Invisibility");
                
                if (success)
                {
                    Debug.Log("Invisibility upgrade unlocked!");
                }
                else
                {
                    Debug.LogWarning("Failed to unlock Invisibility upgrade!");
                }
            }
            else
            {
                Debug.Log("Invisibility upgrade already unlocked!");
            }
        }
    }
}