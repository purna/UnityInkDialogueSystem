using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerInvisible : MonoBehaviour
{

    public PlayerUpgrades playerUpgrades;

    void OnTriggerEnter2D(Collider2D collision)
    {
        //Check if collider is player
        if (collision.CompareTag("Player"))
        {
           
           if (playerUpgrades.InvisibilityUpgradeUnlocked == false)
           {
            /// Unlock the invisibility ability when the player reaches a checkpoint
            playerUpgrades.UnlockInvisibility();

            Debug.Log("Invisibility upgrade unlocked!");
           }
           
        }
    }
}

