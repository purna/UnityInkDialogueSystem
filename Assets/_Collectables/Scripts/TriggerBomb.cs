using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBomb : MonoBehaviour
{

    public PlayerUpgrades playerUpgrades;

    void OnTriggerEnter2D(Collider2D collision)
    {
        //Check if collider is player
        if (collision.CompareTag("Player"))
        {
            if (playerUpgrades.BombUpgradeUnlocked == false)
            {
                /// Unlock the bomb ability when the boss is defeated
                //playerUpgrades.UnlockBomb();
                Debug.Log("Bomb upgrade unlocked!");

            }
            
   
        }
    }

}

