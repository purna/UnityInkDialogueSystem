using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Triggers the skill tree UI when player enters the trigger zone
/// Shows visual cues and emote animations, links to specific skill groups/skills
/// Integrates with SkillsTreeContainer, SkillsTreeGroup, and Skill system
/// Enhanced with locked/unlocked visual states and feedback
/// </summary>
public class SkillTreeTrigger : BaseSkillTrigger
{
    [Header("=== SKILL TREE TRIGGER SETTINGS ===")]
    [Space(10)]
    
    [Header("Skill Tree Settings")]
    [SerializeField] public SkillsTreeController skillsTreeController;
    [SerializeField] public SkillsTreeContainer skillsTreeContainer;
    [SerializeField] public SkillsTreeGroup selectedGroup;
    [Tooltip("Leave empty to show all skills, or specify a skill to auto-select it")]
    [SerializeField] public string selectedSkillName;

    [Header("Toggle Settings")]
    [Tooltip("If TRUE, pressing the interact key again will close the skill tree")]
    [SerializeField] public bool toggleWithInteractKey = true;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("Sprite to show when skill tree is accessible")]
    [SerializeField] private Sprite unlockedSprite;
    [Tooltip("Sprite to show when skill tree is locked (optional)")]
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private ParticleSystem interactParticles;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    [Header("Lock Requirements (Optional)")]
    [Tooltip("If set, requires this skill to be unlocked before trigger can be used")]
    [SerializeField] private Skill requiredSkill;
    [Tooltip("If TRUE, checks if required skill is unlocked before allowing interaction")]
    [SerializeField] private bool enforceRequirement = false;

    [Header("Locked State UI")]
    [SerializeField] private GameObject lockedPromptObject;
    [SerializeField] private TextMeshProUGUI lockedPromptText;
    [SerializeField] private float lockedPromptDisplayTime = 2f;

    private bool skillTreeWasOpenedByThisTrigger;
    private Skill cachedSkill;
    private bool isLocked = false;

    protected override void Awake()
    {
        // Set default prompt text for tree triggers
        if (string.IsNullOrEmpty(promptText))
        {
            promptText = "Press E to view Skills";
        }

        skillTreeWasOpenedByThisTrigger = false;
        
        base.Awake();
    }

    protected virtual void Start()
    {
        // Hide locked prompt at start
        if (lockedPromptObject != null)
        {
            lockedPromptObject.SetActive(false);
        }
        
        UpdateVisualState();
    }

    protected override void InitializeReferences()
    {
        CacheSkillReferences();
        UpdateLockState();
    }

    private void CacheSkillReferences()
    {
        if (skillsTreeController == null)
        {
            Debug.LogWarning($"[SkillTreeTrigger:{gameObject.name}] SkillsTreeController is not assigned!");
            return;
        }

        if (skillsTreeContainer == null)
        {
            Debug.LogWarning($"[SkillTreeTrigger:{gameObject.name}] SkillsTreeContainer is not assigned!");
            return;
        }

        if (!string.IsNullOrEmpty(selectedSkillName))
        {
            if (selectedGroup != null)
            {
                cachedSkill = skillsTreeContainer.GetGroupSkill(selectedGroup.GroupName, selectedSkillName);
                
                if (cachedSkill == null)
                {
                    Debug.LogWarning($"[SkillTreeTrigger:{gameObject.name}] Could not find skill '{selectedSkillName}' in group '{selectedGroup.GroupName}'");
                }
            }
            else
            {
                cachedSkill = skillsTreeContainer.GetSkillByName(selectedSkillName);
                
                if (cachedSkill == null)
                {
                    Debug.LogWarning($"[SkillTreeTrigger:{gameObject.name}] Could not find skill '{selectedSkillName}' in container");
                }
            }
        }
    }

    private void UpdateLockState()
    {
        if (enforceRequirement && requiredSkill != null)
        {
            isLocked = !requiredSkill.IsUnlocked;
        }
        else
        {
            isLocked = false;
        }

        UpdateVisualState();
    }

    protected override void HandleInput()
    {
        // Update lock state each frame in case skill was just unlocked
        if (enforceRequirement && requiredSkill != null)
        {
            bool wasLocked = isLocked;
            UpdateLockState();
            
            // If just unlocked, play unlock effects
            if (wasLocked && !isLocked)
            {
                PlayUnlockEffects();
            }
        }

        if (playerInRange && requiresInput && !triggerOnEnter)
        {
            if (Input.GetKeyDown(interactKey))
            {
                if (showDebugLogs)
                    Debug.Log($"[SkillTreeTrigger:{gameObject.name}] Key '{interactKey}' pressed!");
                
                // Check if locked
                if (isLocked)
                {
                    ShowLockedPrompt();
                    return;
                }
                
                // Check if skill tree is currently open
                bool isThisSkillTreeOpen = IsSkillTreeOpen();
                
                if (isThisSkillTreeOpen && toggleWithInteractKey && skillTreeWasOpenedByThisTrigger)
                {
                    // Close the skill tree
                    CloseSkillTree();
                }
                else if (!isThisSkillTreeOpen && (canTriggerMultipleTimes || !hasTriggered))
                {
                    // Open the skill tree
                    OnTriggerActivated();
                }
                else if (showDebugLogs)
                {
                    Debug.Log($"[SkillTreeTrigger:{gameObject.name}] Cannot interact (triggered: {hasTriggered}, treeOpen: {isThisSkillTreeOpen})");
                }
            }
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Check lock state before triggering
        UpdateLockState();
        
        if (isLocked && triggerOnEnter)
        {
            // Don't auto-trigger if locked
            ShowLockedPrompt();
            return;
        }

        // Call base implementation
        base.OnTriggerEnter2D(other);
    }

    protected override bool ShouldShowPrompt()
    {
        // Don't show prompt if locked
        if (isLocked)
            return false;

        // Show prompt ONLY if:
        // 1. We require input
        // 2. Not auto-triggering
        // 3. Skill tree is NOT already open (or wasn't opened by this trigger)
        // 4. Haven't shown prompt this entry yet
        bool isSkillTreeOpen = IsSkillTreeOpen();
        return requiresInput && !triggerOnEnter && (!isSkillTreeOpen || !skillTreeWasOpenedByThisTrigger) && !hasShownPromptThisEntry;
    }

    protected override void HandleVisualFeedback()
    {
        base.HandleVisualFeedback();
        
        // Hide prompt if tree is open
        bool isSkillTreeOpen = IsSkillTreeOpen();
        if (playerInRange && interactPrompt != null && interactPrompt.activeSelf && isSkillTreeOpen)
        {
            StopPrompt();
        }
    }

    private bool IsSkillTreeOpen()
    {
        if (skillsTreeController == null)
            return false;

        return skillsTreeController.IsSkillTreeOpen;
    }

    protected override void OnTriggerActivated()
    {
        if (showDebugLogs)
            Debug.Log($"[SkillTreeTrigger:{gameObject.name}] OnTriggerActivated() called");
        
        // Check if locked
        if (isLocked)
        {
            ShowLockedPrompt();
            return;
        }

        if (skillsTreeController == null)
        {
            Debug.LogWarning($"[SkillTreeTrigger:{gameObject.name}] SkillsTreeController not assigned!");
            return;
        }

        if (skillsTreeContainer == null)
        {
            Debug.LogWarning($"[SkillTreeTrigger:{gameObject.name}] SkillsTreeContainer not assigned!");
            return;
        }

        skillsTreeController.SetSkillTreeContainer(skillsTreeContainer);
        skillsTreeController.ShowSkillTree();
        
        if (showDebugLogs)
            Debug.Log($"[SkillTreeTrigger:{gameObject.name}] ✓ Main skill tree panel opened");

        if (selectedGroup != null && showDebugLogs)
        {
            Debug.Log($"[SkillTreeTrigger:{gameObject.name}] Filtering to group: {selectedGroup.GroupName}");
        }

        if (cachedSkill != null)
        {
            StartCoroutine(ShowSkillDetailsDelayed(cachedSkill));
        }
        else if (showDebugLogs)
        {
            Debug.Log($"[SkillTreeTrigger:{gameObject.name}] No auto-select skill - showing main tree only");
        }

        hasTriggered = true;
        skillTreeWasOpenedByThisTrigger = true;

        // Play open effects
        PlayOpenEffects();

        // Play emote
        PlayEmote();

        // Hide prompts after opening
        HideAllPrompts();

        if (showDebugLogs)
        {
            string groupInfo = selectedGroup != null ? $"Group: {selectedGroup.GroupName}" : "All Groups";
            string skillInfo = cachedSkill != null ? $" → Auto-selecting: {cachedSkill.SkillName}" : " (No auto-select)";
            Debug.Log($"[SkillTreeTrigger:{gameObject.name}] ✓ Skill tree opened - {groupInfo}{skillInfo}");
        }
    }

    private void CloseSkillTree()
    {
        if (showDebugLogs)
            Debug.Log($"[SkillTreeTrigger:{gameObject.name}] Closing skill tree via toggle");
        
        if (skillsTreeController != null)
        {
            skillsTreeController.HideSkillTree();
            skillTreeWasOpenedByThisTrigger = false;
            
            // Play close effects
            PlayCloseEffects();
            
            if (showDebugLogs)
                Debug.Log($"[SkillTreeTrigger:{gameObject.name}] ✓ Skill tree closed");
        }
    }

    private IEnumerator ShowSkillDetailsDelayed(Skill skill)
    {
        yield return null;
        
        if (skillsTreeController != null && skill != null)
        {
            skillsTreeController.ShowSkillDetails(skill);
            
            if (showDebugLogs)
                Debug.Log($"[SkillTreeTrigger:{gameObject.name}] ✓ Auto-selected skill details: {skill.SkillName}");
        }
    }

    protected override void OnPlayerEnter()
    {
        // Update lock state when player enters
        UpdateLockState();

        // Reset the flag when entering fresh
        // But only if tree is not currently open
        if (!IsSkillTreeOpen())
        {
            skillTreeWasOpenedByThisTrigger = false;
        }
    }

    protected override void OnPlayerExit()
    {
        // Reset the "opened by this trigger" flag
        // This allows the prompt to show again when returning
        skillTreeWasOpenedByThisTrigger = false;
    }

    /// <summary>
    /// Updates the visual representation based on lock state
    /// </summary>
    private void UpdateVisualState()
    {
        if (spriteRenderer == null)
            return;

        // Determine which sprite to use
        Sprite targetSprite = null;

        if (isLocked && lockedSprite != null)
        {
            // Show locked sprite
            targetSprite = lockedSprite;
        }
        else if (!isLocked && unlockedSprite != null)
        {
            // Show unlocked sprite
            targetSprite = unlockedSprite;
        }
        else if (cachedSkill != null)
        {
            // Fallback to skill icon if available
            targetSprite = isLocked ? cachedSkill.LockedIcon : cachedSkill.UnlockedIcon;
        }

        if (targetSprite != null)
        {
            spriteRenderer.sprite = targetSprite;
        }
    }

    /// <summary>
    /// Show locked prompt message to player
    /// </summary>
    private void ShowLockedPrompt()
    {
        if (lockedPromptObject == null || lockedPromptText == null)
        {
            if (showDebugLogs)
                Debug.Log($"[SkillTreeTrigger:{gameObject.name}] Trigger is locked but no locked prompt configured");
            return;
        }

        string message = GetLockedMessage();
        
        if (showDebugLogs)
            Debug.Log($"[SkillTreeTrigger:{gameObject.name}] Showing locked message: {message}");
        
        lockedPromptObject.SetActive(true);
        lockedPromptText.text = message;
        
        // Auto-hide after delay
        StartCoroutine(HideLockedPromptAfterDelay());
    }

    /// <summary>
    /// Generate the locked message based on missing requirements
    /// </summary>
    private string GetLockedMessage()
    {
        if (requiredSkill != null)
        {
            return $"Requires '{requiredSkill.SkillName}' to be unlocked first!";
        }

        return "Skill tree is locked!";
    }

    private IEnumerator HideLockedPromptAfterDelay()
    {
        yield return new WaitForSeconds(lockedPromptDisplayTime);
        
        if (lockedPromptObject != null)
        {
            lockedPromptObject.SetActive(false);
        }
    }

    /// <summary>
    /// Play visual and audio effects when opening skill tree
    /// </summary>
    private void PlayOpenEffects()
    {
        // Play particle effect
        if (interactParticles != null)
        {
            interactParticles.Play();
        }

        // Play sound effect
        if (openSound != null)
        {
            AudioSource.PlayClipAtPoint(openSound, transform.position);
        }
    }

    /// <summary>
    /// Play audio effect when closing skill tree
    /// </summary>
    private void PlayCloseEffects()
    {
        // Play sound effect
        if (closeSound != null)
        {
            AudioSource.PlayClipAtPoint(closeSound, transform.position);
        }
    }

    /// <summary>
    /// Play visual and audio effects when unlocking
    /// </summary>
    private void PlayUnlockEffects()
    {
        if (showDebugLogs)
            Debug.Log($"[SkillTreeTrigger:{gameObject.name}] Trigger unlocked!");

        // Play particle effect
        if (interactParticles != null)
        {
            interactParticles.Play();
        }

        // Play sound effect
        if (openSound != null)
        {
            AudioSource.PlayClipAtPoint(openSound, transform.position);
        }

        UpdateVisualState();
    }

    public override void ResetTrigger()
    {
        base.ResetTrigger();
        skillTreeWasOpenedByThisTrigger = false;
        UpdateLockState();
    }

    #region Public Methods

    /// <summary>
    /// Set the target skill tree configuration at runtime
    /// </summary>
    public void SetTargetSkill(SkillsTreeContainer container, SkillsTreeGroup group, string skillName)
    {
        skillsTreeContainer = container;
        selectedGroup = group;
        selectedSkillName = skillName;
        CacheSkillReferences();
    }

    /// <summary>
    /// Get the cached skill reference
    /// </summary>
    public Skill GetCachedSkill()
    {
        return cachedSkill;
    }

    /// <summary>
    /// Get the selected skill group
    /// </summary>
    public SkillsTreeGroup GetSelectedGroup()
    {
        return selectedGroup;
    }

    /// <summary>
    /// Check if this trigger is currently locked
    /// </summary>
    public bool IsLocked()
    {
        return isLocked;
    }

    /// <summary>
    /// Get the required skill (if any)
    /// </summary>
    public Skill GetRequiredSkill()
    {
        return requiredSkill;
    }

    /// <summary>
    /// Set the required skill at runtime
    /// </summary>
    public void SetRequiredSkill(Skill skill, bool enforce = true)
    {
        requiredSkill = skill;
        enforceRequirement = enforce;
        UpdateLockState();
    }

    /// <summary>
    /// Force unlock this trigger (bypasses requirement check)
    /// </summary>
    public void ForceUnlock()
    {
        enforceRequirement = false;
        isLocked = false;
        UpdateVisualState();
        PlayUnlockEffects();

        if (showDebugLogs)
            Debug.Log($"[SkillTreeTrigger:{gameObject.name}] Force unlocked");
    }

    #endregion

    #region Editor Support

    protected override void OnValidate()
    {
        base.OnValidate();
        CacheSkillReferences();
        UpdateLockState();
    }

    protected override void DrawCustomGizmoLabels()
    {
        #if UNITY_EDITOR
        string label = "";

        // Lock status
        if (isLocked)
        {
            label += "[🔒 LOCKED]\n";
            if (requiredSkill != null)
                label += $"Requires: {requiredSkill.SkillName}\n";
        }
        else if (enforceRequirement && requiredSkill != null)
        {
            label += "[✓ UNLOCKED]\n";
        }

        if (skillsTreeContainer != null)
        {
            label += $"Skill Tree: {skillsTreeContainer.TreeName}\n";
            if (selectedGroup != null)
                label += $"Group: {selectedGroup.GroupName}\n";
            if (!string.IsNullOrEmpty(selectedSkillName))
                label += $"Skill: {selectedSkillName}\n";
        }
        
        if (triggerOnEnter)
            label += "[AUTO TRIGGER]";
        else if (requiresInput)
        {
            label += $"[Press {interactKey}]";
            if (toggleWithInteractKey)
                label += " (Toggle)";
        }

        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, label);
        
        // Draw prompt position preview in editor
        if (interactPrompt != null && Application.isPlaying && playerInRange)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(interactPrompt.transform.position, 0.2f);
        }
        #endif
    }

    #endregion
}