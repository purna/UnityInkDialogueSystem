using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerStaff : MonoBehaviour
{

    public PlayerUpgrades playerUpgrades;

    void OnTriggerEnter2D(Collider2D collision)
    {
        //Check if collider is player
        if (collision.CompareTag("Player"))
        {
           
           if (playerUpgrades.StaffUpgradeUnlocked == false)
           {
            /// Unlock the staff ability when the player reaches a checkpoint
            playerUpgrades.UnlockStaff();
            Debug.Log("Staff upgrade unlocked!");
           }
           
        }
    }
}

