using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// ==================== PLAYER SHIELD ====================
public class PlayerShield : MonoBehaviour, IPlayerUpgrade
{
    public string UpgradeName => "Shield";
    public bool IsActive { get; set; }

    [SerializeField] private GameObject shieldObject;
    [SerializeField] private Transform shieldSpawnPosition;
    [SerializeField] private float timeForShield = 2f;
    [SerializeField] private PlayerUpgrades playerUpgrades;

    public bool ShouldBeProtecting { get; private set; } = false;

    private void Update()
    {
        if (!IsActive) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            SpawnShield();
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
        ShouldBeProtecting = false;
    }

    private void SpawnShield()
    {
        if (shieldObject != null && shieldSpawnPosition != null)
        {
            StartCoroutine(ProtectPlayer());
        }
        else
        {
            Debug.LogWarning("Shield prefab or spawn position is not assigned.");
        }
    }

    public IEnumerator ProtectPlayer()
    {
        GameObject spawnedShieldInstance = Instantiate(shieldObject, shieldSpawnPosition.position, shieldSpawnPosition.rotation);
        ShouldBeProtecting = true;

        float _elapsedTime = 0f;
        while (_elapsedTime < timeForShield)
        {
            _elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        ShouldBeProtecting = false;

        if (spawnedShieldInstance != null)
        {
            Destroy(spawnedShieldInstance);
        }
    }

    public void SetShieldObject(GameObject collectedObject)
    {
        shieldObject = collectedObject;
    }
}