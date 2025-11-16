using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInvisibility : MonoBehaviour
{
    [SerializeField] private GameObject invisibilityObject;
    [SerializeField] private float timeForInvisibility = .5f;

    [SerializeField] private  PlayerUpgrades playerUpgrades;
    
    [SerializeField] public  bool  IsActive = false;

    private void Update()
    {
        if (playerUpgrades.InvisibilityUpgradeUnlocked == true)
           {

                // Check if the E key was pressed this frame
                if  (Keyboard.current.eKey.wasPressedThisFrame)
                {
                playerUpgrades.UnlockInvisibility();
                }

                // Check if the F key was pressed this frame
                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    playerUpgrades.LockInvisibility();
                }
            }
 
    }
    

    public void SetInvisibilityObject(GameObject collectedObject)
    {
        invisibilityObject = collectedObject;
    }
}
