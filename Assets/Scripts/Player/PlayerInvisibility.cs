using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// ==================== PLAYER INVISIBILITY ====================
public class PlayerInvisibility : MonoBehaviour, IPlayerUpgrade
{
    public string UpgradeName => "Invisibility";
    public bool IsActive { get; set; }

    [SerializeField] private GameObject invisibilityObject;
    [SerializeField] private float timeForInvisibility = .5f;
    [SerializeField] private PlayerUpgrades playerUpgrades;

    private void Update()
    {
        if (!IsActive) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            // Activate invisibility logic here
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

    public void SetInvisibilityObject(GameObject collectedObject)
    {
        invisibilityObject = collectedObject;
    }
}