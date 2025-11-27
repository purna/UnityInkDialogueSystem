using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// ==================== PLAYER STAFF ====================
public class PlayerStaff : MonoBehaviour, IPlayerUpgrade
{
    public string UpgradeName => "Staff";
    public bool IsActive { get; set; }

    [SerializeField] private GameObject staffObject;
    [SerializeField] private Transform staffSpawnPosition;
    [SerializeField] private PlayerUpgrades playerUpgrades;

    private void Update()
    {
        if (!IsActive) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            SpawnStaff();
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            playerUpgrades.LockUpgrade(UpgradeName);
        }
    }

    public void Activate()
    {
        IsActive = true;
        enabled = true;
    }

    public void Deactivate()
    {
        IsActive = false;
        enabled = false;
    }

    private void SpawnStaff()
    {
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