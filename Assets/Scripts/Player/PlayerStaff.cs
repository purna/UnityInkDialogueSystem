using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStaff : MonoBehaviour
{
    [SerializeField] private GameObject staffObject;
    [SerializeField] private Transform staffSpawnPosition;

    [SerializeField] private  PlayerUpgrades playerUpgrades;

    [SerializeField] public  bool  IsActive = false;

   
     private void Update()
    {
       
        if (playerUpgrades.StaffUpgradeUnlocked == true)
        {
             // Check if the E key was pressed this frame
            if  (Keyboard.current.eKey.wasPressedThisFrame)
            {
                SpawnStaff();
            }

            // Check if the F key was pressed this frame
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                playerUpgrades.LockStaff();
            }
        }

    }

    private void SpawnStaff()
    {
        // Instantiate the staff prefab at the spawn position
        if (staffObject != null && staffSpawnPosition != null)
        {
            Instantiate(staffObject, staffSpawnPosition.position, staffSpawnPosition.rotation);
        }
        else
        {
            Debug.LogWarning("Staff prefab or spawn position is not assigned.");
        }
    }


    public void SetStaffObject(GameObject collectedObject)
    {
        staffObject = collectedObject;
    }

}
