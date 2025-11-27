using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// ==================== PLAYER PRAYER ====================
public class PlayerPrayer : MonoBehaviour, IPlayerUpgrade
{
    public string UpgradeName => "Prayer";
    public bool IsActive { get; set; }

    [SerializeField] private GameObject prayerObject;
    [SerializeField] private PlayerUpgrades playerUpgrades;

    private void Update()
    {
        if (!IsActive) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            // Prayer activation logic
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

    public void SetPrayerObject(GameObject collectedObject)
    {
        prayerObject = collectedObject;
    }
}