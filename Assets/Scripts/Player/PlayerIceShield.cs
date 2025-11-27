using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// ==================== PLAYER ICE SHIELD ====================
public class PlayerIceShield : MonoBehaviour, IPlayerUpgrade
{
    public string UpgradeName => "IceShield";
    public bool IsActive { get; set; }

    [Header("Ice Shield Configuration")]
    [SerializeField] private GameObject iceShieldObject;
    [SerializeField] private Transform shieldSpawnPosition;
    [SerializeField] private PlayerUpgrades playerUpgrades;

    [Header("Ice Shield Settings")]
    [SerializeField] private float shieldDuration = 5f;
    [SerializeField] private float slowEffect = 0.5f; // Slow enemies by 50%

    public bool IsShieldActive { get; private set; } = false;

    private void Update()
    {
        if (!IsActive) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            ActivateIceShield();
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
        IsShieldActive = false;
    }

    private void ActivateIceShield()
    {
        if (iceShieldObject != null && shieldSpawnPosition != null)
        {
            StartCoroutine(IceShieldCoroutine());
            playerUpgrades.LockUpgrade(UpgradeName);
        }
        else
        {
            Debug.LogWarning("Ice Shield prefab or spawn position is not assigned.");
        }
    }

    private IEnumerator IceShieldCoroutine()
    {
        GameObject spawnedShield = Instantiate(iceShieldObject, shieldSpawnPosition.position, shieldSpawnPosition.rotation);
        spawnedShield.transform.SetParent(transform);
        IsShieldActive = true;

        Debug.Log("Ice Shield activated!");

        float elapsedTime = 0f;
        while (elapsedTime < shieldDuration)
        {
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        IsShieldActive = false;

        if (spawnedShield != null)
        {
            Destroy(spawnedShield);
        }

        Debug.Log("Ice Shield deactivated!");
    }

    public void SetIceShieldObject(GameObject collectedObject)
    {
        iceShieldObject = collectedObject;
    }

    public float GetSlowEffect()
    {
        return IsShieldActive ? slowEffect : 1f;
    }
}
