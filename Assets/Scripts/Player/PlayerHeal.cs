using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// ==================== PLAYER HEAL ====================
public class PlayerHeal : MonoBehaviour, IPlayerUpgrade
{
    public string UpgradeName => "Heal";
    public bool IsActive { get; set; }

    [Header("Heal Configuration")]
    [SerializeField] private GameObject healEffectObject;
    [SerializeField] private PlayerUpgrades playerUpgrades;

    [Header("Heal Settings")]
    [SerializeField] private int healAmount = 50;
    [SerializeField] private int maxHeals = 3;
    [SerializeField] private float healEffectDuration = 1f;

    private int currentHealCount;

    private void Start()
    {
        currentHealCount = maxHeals;
    }

    private void Update()
    {
        if (!IsActive) return;

        if (Keyboard.current.eKey.wasPressedThisFrame && currentHealCount > 0)
        {
            PerformHeal();
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

    private void PerformHeal()
    {
        // Heal the player (you'll need to integrate with your health system)
        // Example: GetComponent<PlayerHealth>().Heal(healAmount);
        
        currentHealCount--;
        Debug.Log($"Player healed for {healAmount} HP! Remaining heals: {currentHealCount}");

        if (healEffectObject != null)
        {
            StartCoroutine(ShowHealEffect());
        }

        if (currentHealCount <= 0)
        {
            playerUpgrades.LockUpgrade(UpgradeName);
        }
    }

    private IEnumerator ShowHealEffect()
    {
        GameObject healEffect = Instantiate(healEffectObject, transform.position, Quaternion.identity);
        healEffect.transform.SetParent(transform);

        yield return new WaitForSeconds(healEffectDuration);

        if (healEffect != null)
        {
            Destroy(healEffect);
        }
    }

    public void SetHealEffectObject(GameObject collectedObject)
    {
        healEffectObject = collectedObject;
    }
}