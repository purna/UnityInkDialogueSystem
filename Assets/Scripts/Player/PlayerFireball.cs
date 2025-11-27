using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// ==================== PLAYER FIREBALL ====================
public class PlayerFireball : MonoBehaviour, IPlayerUpgrade
{
    public string UpgradeName => "Fireball";
    public bool IsActive { get; set; }

    [Header("Fireball Configuration")]
    [SerializeField] private GameObject fireballObject;
    [SerializeField] private Transform fireballSpawnPosition;
    [SerializeField] private PlayerUpgrades playerUpgrades;

    [Header("Fireball Settings")]
    [SerializeField] private int maxFireballs = 10;
    [SerializeField] private float fireballCooldown = 0.5f;

    private int currentFireballCount;
    private bool canShootFireball = true;

    private void Start()
    {
        currentFireballCount = maxFireballs;
    }

    private void Update()
    {
        if (!IsActive) return;

        if (Keyboard.current.eKey.wasPressedThisFrame && canShootFireball && currentFireballCount > 0)
        {
            ShootFireball();
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

    private void ShootFireball()
    {
        if (fireballObject != null && fireballSpawnPosition != null)
        {
            Instantiate(fireballObject, fireballSpawnPosition.position, fireballSpawnPosition.rotation);
            currentFireballCount--;
            StartCoroutine(FireballCooldown());

            Debug.Log($"Fireball shot! Remaining fireballs: {currentFireballCount}");

            if (currentFireballCount <= 0)
            {
                playerUpgrades.LockUpgrade(UpgradeName);
            }
        }
        else
        {
            Debug.LogWarning("Fireball prefab or spawn position is not assigned.");
        }
    }

    private IEnumerator FireballCooldown()
    {
        canShootFireball = false;
        yield return new WaitForSeconds(fireballCooldown);
        canShootFireball = true;
    }

    public void SetFireballObject(GameObject collectedObject)
    {
        fireballObject = collectedObject;
    }
}