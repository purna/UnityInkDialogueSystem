using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerShield : MonoBehaviour
{

    public PlayerUpgrades playerUpgrades;

    void OnTriggerEnter2D(Collider2D collision)
    {
        //Check if collider is player
        if (collision.CompareTag("Player"))
        {
           
           if (playerUpgrades.ShieldUpgradeUnlocked == false)
           {
            /// Unlock the shield ability when the player reaches a checkpoint
            playerUpgrades.UnlockShield();
            Debug.Log("Shield upgrade unlocked!");
           }
           
        }
    }
}

