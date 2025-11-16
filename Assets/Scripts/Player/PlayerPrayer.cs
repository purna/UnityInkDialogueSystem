using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPrayer : MonoBehaviour
{
    [SerializeField] private GameObject prayerObject;

    [SerializeField] private  PlayerUpgrades playerUpgrades;

    [SerializeField] public  bool IsActive = false;

     private void Update()
    {
       
        if (playerUpgrades.PrayerUpgradeUnlocked == true)
        {
             // Check if the E key was pressed this frame
            if  (Keyboard.current.eKey.wasPressedThisFrame)
            {
                
            }

            // Check if the F key was pressed this frame
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                playerUpgrades.LockPrayer();
            }
        }

    }



    public void SetPrayerObject(GameObject collectedObject)
    {
        prayerObject = collectedObject;
    }

}
