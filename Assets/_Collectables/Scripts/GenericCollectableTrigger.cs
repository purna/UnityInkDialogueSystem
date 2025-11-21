using UnityEngine;
using System;

/// <summary>
/// Generic trigger for unlocking player upgrades/collectables
/// Supports multiple collectable types via dropdown selection
/// </summary>
public class GenericCollectableTrigger : MonoBehaviour
{
    [Header("Collectable Settings")]
    [Tooltip("Type of collectable this trigger will unlock")]
    [SerializeField] private CollectableType collectableType;
    
    [Tooltip("Check if already unlocked before triggering")]
    [SerializeField] private bool checkIfAlreadyUnlocked = true;
    
    [Header("References")]
    [SerializeField] private PlayerUpgrades playerUpgrades;
    
    [Header("Trigger Behavior")]
    [Tooltip("If TRUE, trigger only works once. If FALSE, can trigger multiple times.")]
    [SerializeField] private bool oneTimeUse = true;
    
    [Tooltip("Destroy this GameObject after triggering")]
    [SerializeField] private bool destroyOnTrigger = false;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject visualCue;
    [SerializeField] private Animator animator;
    [SerializeField] private string animationTriggerName = "Collect";
    
    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioSource audioSource;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private bool hasTriggered = false;

    private void Awake()
    {
        // Auto-find PlayerUpgrades if not assigned
        if (playerUpgrades == null)
        {
            playerUpgrades = FindObjectOfType<PlayerUpgrades>();
            if (playerUpgrades == null && showDebugLogs)
            {
                Debug.LogWarning($"[GenericCollectableTrigger:{gameObject.name}] PlayerUpgrades not found!");
            }
        }
        
        // Auto-find AudioSource if not assigned
        if (audioSource == null && collectSound != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Initialize visual cue
        if (visualCue != null)
        {
            visualCue.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if collider is player
        if (!collision.CompareTag("Player"))
            return;
        
        // Check if already triggered (one-time use)
        if (oneTimeUse && hasTriggered)
        {
            if (showDebugLogs)
                Debug.Log($"[GenericCollectableTrigger:{gameObject.name}] Already triggered (one-time use)");
            return;
        }
        
        // Check if PlayerUpgrades is assigned
        if (playerUpgrades == null)
        {
            Debug.LogError($"[GenericCollectableTrigger:{gameObject.name}] PlayerUpgrades is not assigned!");
            return;
        }
        
        // Check if already unlocked
        if (checkIfAlreadyUnlocked && IsCollectableUnlocked())
        {
            if (showDebugLogs)
                Debug.Log($"[GenericCollectableTrigger:{gameObject.name}] {collectableType} already unlocked!");
            return;
        }
        
        // Unlock the collectable
        UnlockCollectable();
        
        // Mark as triggered
        hasTriggered = true;
        
        // Play effects
        PlayVisualEffects();
        PlayAudio();
        
        // Log success
        if (showDebugLogs)
            Debug.Log($"[GenericCollectableTrigger:{gameObject.name}] ✓ {collectableType} unlocked!");
        
        // Destroy if needed
        if (destroyOnTrigger)
        {
            Destroy(gameObject, collectSound != null ? collectSound.length : 0.1f);
        }
        else if (visualCue != null)
        {
            visualCue.SetActive(false);
        }
    }

    private bool IsCollectableUnlocked()
    {
        switch (collectableType)
        {
            case CollectableType.Bomb:
                return playerUpgrades.BombUpgradeUnlocked;
            
            case CollectableType.Invisibility:
                return playerUpgrades.InvisibilityUpgradeUnlocked;
            
            case CollectableType.Shield:
                return playerUpgrades.ShieldUpgradeUnlocked;
            
            case CollectableType.Staff:
                return playerUpgrades.StaffUpgradeUnlocked;
            
            default:
                Debug.LogWarning($"[GenericCollectableTrigger:{gameObject.name}] Unknown collectable type: {collectableType}");
                return false;
        }
    }

    private void UnlockCollectable()
    {
        switch (collectableType)
        {
            case CollectableType.Bomb:
                playerUpgrades.UnlockBomb();
                break;
            
            case CollectableType.Invisibility:
                playerUpgrades.UnlockInvisibility();
                break;
            
            case CollectableType.Shield:
                playerUpgrades.UnlockShield();
                break;
            
            case CollectableType.Staff:
                playerUpgrades.UnlockStaff();
                break;
            
            default:
                Debug.LogError($"[GenericCollectableTrigger:{gameObject.name}] Cannot unlock unknown collectable type: {collectableType}");
                break;
        }
    }

    private void PlayVisualEffects()
    {
        if (animator != null && !string.IsNullOrEmpty(animationTriggerName))
        {
            animator.SetTrigger(animationTriggerName);
        }
    }

    private void PlayAudio()
    {
        if (audioSource != null && collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }
    }

    #region Public Methods

    /// <summary>
    /// Reset the trigger to allow re-triggering
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
        if (showDebugLogs)
            Debug.Log($"[GenericCollectableTrigger:{gameObject.name}] Trigger reset");
    }

    /// <summary>
    /// Manually trigger the collectable unlock
    /// </summary>
    public void ManualTrigger()
    {
        if (!oneTimeUse || !hasTriggered)
        {
            UnlockCollectable();
            hasTriggered = true;
            PlayVisualEffects();
            PlayAudio();
        }
    }

    /// <summary>
    /// Change the collectable type at runtime
    /// </summary>
    public void SetCollectableType(CollectableType newType)
    {
        collectableType = newType;
    }

    /// <summary>
    /// Get the current collectable type
    /// </summary>
    public CollectableType GetCollectableType()
    {
        return collectableType;
    }

    #endregion

    #region Editor Support

    private void OnValidate()
    {
        // Auto-find PlayerUpgrades in editor
        if (playerUpgrades == null && !Application.isPlaying)
        {
            playerUpgrades = FindObjectOfType<PlayerUpgrades>();
        }
    }

    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            // Draw trigger area
            Gizmos.color = hasTriggered ? Color.gray : GetCollectableColor();
            
            if (col is BoxCollider2D boxCol)
            {
                Gizmos.DrawWireCube(transform.position + (Vector3)boxCol.offset, boxCol.size);
            }
            else if (col is CircleCollider2D circleCol)
            {
                Gizmos.DrawWireSphere(transform.position + (Vector3)circleCol.offset, circleCol.radius);
            }
        }

        #if UNITY_EDITOR
        // Draw label
        string label = $"Collectable: {collectableType}\n";
        
        if (oneTimeUse)
            label += "[ONE TIME USE]";
        else
            label += "[REPEATABLE]";
        
        if (hasTriggered)
            label += "\n[TRIGGERED]";

        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, label);
        #endif
    }

    private Color GetCollectableColor()
    {
        switch (collectableType)
        {
            case CollectableType.Bomb:
                return new Color(1f, 0.3f, 0f); // Orange
            
            case CollectableType.Invisibility:
                return new Color(0.5f, 0.5f, 1f); // Light blue
            
            case CollectableType.Shield:
                return new Color(0f, 0.8f, 1f); // Cyan
            
            case CollectableType.Staff:
                return new Color(0.8f, 0f, 1f); // Purple
            
            default:
                return Color.yellow;
        }
    }

    #endregion
}

/// <summary>
/// Enum for different types of collectables/upgrades
/// Add new types here as needed
/// </summary>
[Serializable]
public enum CollectableType
{
    Bomb,
    Invisibility,
    Shield,
    Staff,
    // Add more types as needed:
    // DoubleJump,
    // Dash,
    // WallClimb,
    // etc.
}