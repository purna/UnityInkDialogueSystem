using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Core.Game;

/// <summary>
/// Triggers skill unlocks, ability unlocks, and skill point rewards when player enters the trigger zone
/// Can unlock multiple abilities and award skill points in a single trigger
/// Shows visual cues (sprite, particles) and UI prompts based on lock state
/// Can require a specific skill to be unlocked before activating (like a skill gate)
/// Can spawn collectable rewards when unlocked
/// </summary>
public class SkillTreeUnlockTrigger : BaseSkillTrigger
{
    [Header("=== UNLOCK TRIGGER SETTINGS ===")]
    [Space(10)]
    
    [Header("System References")]
    [SerializeField] public SkillTreeManager skillTreeManager;
    [SerializeField] public SkillsTreeController skillsTreeController;

    [Header("Unlock Settings")]
    [Tooltip("List of ability IDs to unlock (e.g., 'double_jump', 'dash', 'wall_climb')")]
    [SerializeField] public List<string> abilitiesToUnlock = new List<string>();
    
    [Tooltip("List of specific skills to unlock directly (bypasses cost/prerequisites)")]
    [SerializeField] public List<Skill> skillsToUnlock = new List<Skill>();
    
    [Tooltip("If TRUE, unlocks skills normally (respects prerequisites/costs). If FALSE, force unlocks.")]
    [SerializeField] public bool respectSkillRequirements = false;

    [Header("Skill Points Reward")]
    [Tooltip("Skill points to award when triggered")]
    [SerializeField] public int skillPointsReward = 0;
    
    [Tooltip("If TRUE, adds to current points only. If FALSE, also updates total earned.")]
    [SerializeField] public bool addToCurrentOnly = false;

    [Header("Collectable Rewards")]
    [Tooltip("Collectable prefabs to spawn when unlocked (coins, items, etc.)")]
    [SerializeField] private GameObject[] collectableRewards;
    [Tooltip("Offset position for spawning rewards (relative to trigger position)")]
    [SerializeField] private Vector3 rewardSpawnOffset = Vector3.up;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem unlockParticles;
    [SerializeField] private AudioClip unlockSound;
    
    [Header("UI Prompt (For Locked State)")]
    [SerializeField] private GameObject lockedPromptObject;
    [SerializeField] private TextMeshProUGUI lockedPromptText;
    [SerializeField] private float lockedPromptDisplayTime = 2f;

    private bool hasBeenUnlocked = false;

    protected override void Awake()
    {
        // Set default prompt text for unlock triggers
        if (string.IsNullOrEmpty(promptText))
        {
            promptText = "Press E to Claim Reward";
        }

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
        // Find references if not assigned
        if (skillTreeManager == null)
        {
            skillTreeManager = SkillTreeManager.Instance;
            if (skillTreeManager == null && showDebugLogs)
            {
                Debug.LogWarning($"[SkillTreeUnlockTrigger:{gameObject.name}] SkillTreeManager not found!");
            }
        }

        if (skillsTreeController == null)
        {
            skillsTreeController = FindObjectOfType<SkillsTreeController>();
            if (skillsTreeController == null && showDebugLogs)
            {
                Debug.LogWarning($"[SkillTreeUnlockTrigger:{gameObject.name}] SkillsTreeController not found!");
            }
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // If already unlocked, don't do anything
        if (hasBeenUnlocked && !canTriggerMultipleTimes)
        {
            return;
        }

        // Check if prerequisites are met (if respecting requirements)
        if (respectSkillRequirements && !CanUnlock())
        {
            ShowLockedPrompt();
            return;
        }

        // Call base implementation for normal trigger behavior
        base.OnTriggerEnter2D(other);
    }

    protected override void OnTriggerActivated()
    {
        if (showDebugLogs)
            Debug.Log($"[SkillTreeUnlockTrigger:{gameObject.name}] OnTriggerActivated() called");
        
        if (skillTreeManager == null)
        {
            Debug.LogError($"[SkillTreeUnlockTrigger:{gameObject.name}] SkillTreeManager not found! Cannot unlock abilities.");
            return;
        }

        // Check prerequisites if respecting requirements
        if (respectSkillRequirements && !CanUnlock())
        {
            ShowLockedPrompt();
            return;
        }

        int unlockedCount = 0;

        // Unlock abilities
        if (abilitiesToUnlock != null && abilitiesToUnlock.Count > 0)
        {
            foreach (string abilityID in abilitiesToUnlock)
            {
                if (!string.IsNullOrEmpty(abilityID))
                {
                    skillTreeManager.UnlockAbility(abilityID);
                    unlockedCount++;
                    
                    if (showDebugLogs)
                        Debug.Log($"[SkillTreeUnlockTrigger:{gameObject.name}] ✓ Unlocked ability: {abilityID}");
                }
            }
        }

        // Unlock skills
        if (skillsToUnlock != null && skillsToUnlock.Count > 0)
        {
            foreach (Skill skill in skillsToUnlock)
            {
                if (skill != null)
                {
                    if (respectSkillRequirements)
                    {
                        // Use normal unlock system
                        bool success = skillTreeManager.TryUnlockSkill(skill);
                        if (success)
                        {
                            unlockedCount++;
                            if (showDebugLogs)
                                Debug.Log($"[SkillTreeUnlockTrigger:{gameObject.name}] ✓ Unlocked skill: {skill.SkillName}");
                        }
                        else if (showDebugLogs)
                        {
                            Debug.LogWarning($"[SkillTreeUnlockTrigger:{gameObject.name}] Failed to unlock skill: {skill.SkillName} (requirements not met)");
                        }
                    }
                    else
                    {
                        // Force unlock (bypass cost and prerequisites)
                        if (!skill.IsUnlocked)
                        {
                            skill.Unlock();
                            unlockedCount++;
                            
                            if (showDebugLogs)
                                Debug.Log($"[SkillTreeUnlockTrigger:{gameObject.name}] ✓ Force unlocked skill: {skill.SkillName}");
                        }
                        else if (showDebugLogs)
                        {
                            Debug.Log($"[SkillTreeUnlockTrigger:{gameObject.name}] Skill already unlocked: {skill.SkillName}");
                        }
                    }
                }
            }
        }

        // Award skill points
        if (skillPointsReward > 0)
        {
            if (addToCurrentOnly)
            {
                if (showDebugLogs)
                    Debug.LogWarning($"[SkillTreeUnlockTrigger:{gameObject.name}] 'addToCurrentOnly' is not fully supported - adds to both current and total");
                
                skillTreeManager.AddSkillPoints(skillPointsReward);
            }
            else
            {
                skillTreeManager.AddSkillPoints(skillPointsReward);
            }
            
            if (showDebugLogs)
                Debug.Log($"[SkillTreeUnlockTrigger:{gameObject.name}] ✓ Awarded {skillPointsReward} skill points");
        }

        // Spawn collectable rewards
        if (collectableRewards != null && collectableRewards.Length > 0)
        {
            SpawnCollectableRewards();
        }

        // Mark as unlocked
        hasBeenUnlocked = true;
        hasTriggered = true;

        // Update visual state
        UpdateVisualState();

        // Play unlock effects
        PlayUnlockEffects();

        // Play emote
        PlayEmote();

        // Hide prompts
        HideAllPrompts();

        // Refresh skill tree UI if it's open
        if (skillsTreeController != null)
        {
            skillsTreeController.RefreshSkillTree();
        }

        if (showDebugLogs)
        {
            Debug.Log($"[SkillTreeUnlockTrigger:{gameObject.name}] ✓ Trigger complete! " +
                      $"Unlocked: {unlockedCount} items, Awarded: {skillPointsReward} SP, Spawned: {collectableRewards.Length} collectables");
        }
    }

    /// <summary>
    /// Check if all prerequisites are met to unlock this trigger
    /// Only used when respectSkillRequirements is TRUE
    /// </summary>
    private bool CanUnlock()
    {
        // If already unlocked and can't trigger multiple times, return false
        if (hasBeenUnlocked && !canTriggerMultipleTimes)
            return false;

        // Check if all skills have their prerequisites met
        if (skillsToUnlock != null && skillsToUnlock.Count > 0)
        {
            foreach (Skill skill in skillsToUnlock)
            {
                if (skill != null && !skill.CanUnlock())
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Updates the visual representation based on lock state
    /// Uses the skill's locked/unlocked icons from the Skill ScriptableObject
    /// </summary>
    private void UpdateVisualState()
    {
        if (spriteRenderer == null)
            return;

        // Use the first skill's icon if available
        if (skillsToUnlock != null && skillsToUnlock.Count > 0 && skillsToUnlock[0] != null)
        {
            Skill firstSkill = skillsToUnlock[0];
            spriteRenderer.sprite = hasBeenUnlocked ? firstSkill.UnlockedIcon : firstSkill.LockedIcon;
        }
    }

    /// <summary>
    /// Spawn all collectable rewards at the trigger position
    /// </summary>
    private void SpawnCollectableRewards()
    {
        foreach (var reward in collectableRewards)
        {
            if (reward != null)
            {
                Instantiate(reward, transform.position + rewardSpawnOffset, Quaternion.identity);
                
                if (showDebugLogs)
                    Debug.Log($"[SkillTreeUnlockTrigger:{gameObject.name}] ✓ Spawned reward: {reward.name}");
            }
        }
    }

    /// <summary>
    /// Show locked prompt message to player
    /// </summary>
    private void ShowLockedPrompt()
    {
        if (lockedPromptObject == null || lockedPromptText == null)
            return;

        string message = GetLockedMessage();
        
        if (showDebugLogs)
            Debug.Log($"[SkillTreeUnlockTrigger:{gameObject.name}] Showing locked message: {message}");
        
        lockedPromptObject.SetActive(true);
        lockedPromptText.text = message;
        
        // Auto-hide after delay
        StartCoroutine(HideLockedPromptAfterDelay());
    }

    /// <summary>
    /// Generate the locked message based on missing prerequisites
    /// Checks skill prerequisites for missing requirements
    /// </summary>
    private string GetLockedMessage()
    {
        // Check skill prerequisites (only if respecting requirements)
        if (respectSkillRequirements && skillsToUnlock != null && skillsToUnlock.Count > 0)
        {
            // Check each skill for missing prerequisites
            foreach (Skill skill in skillsToUnlock)
            {
                if (skill != null && !skill.CanUnlock())
                {
                    // Find which prerequisite is missing
                    if (skill.Prerequisites != null && skill.Prerequisites.Count > 0)
                    {
                        foreach (Skill prereq in skill.Prerequisites)
                        {
                            if (prereq != null && !prereq.IsUnlocked)
                            {
                                return $"Requires '{prereq.SkillName}' to be unlocked first!";
                            }
                        }
                    }
                    
                    return $"Prerequisites for '{skill.SkillName}' not met!";
                }
            }
        }

        return "Prerequisites not met!";
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
    /// Play unlock visual and audio effects
    /// </summary>
    private void PlayUnlockEffects()
    {
        // Play particle effect
        if (unlockParticles != null)
        {
            unlockParticles.Play();
        }

        // Play sound effect
        if (unlockSound != null)
        {
            AudioSource.PlayClipAtPoint(unlockSound, transform.position);
        }
    }

    public override void ResetTrigger()
    {
        base.ResetTrigger();
        hasBeenUnlocked = false;
        UpdateVisualState();
    }

    #region Public Methods

    /// <summary>
    /// Add an ability ID to unlock at runtime
    /// </summary>
    public void AddAbilityToUnlock(string abilityID)
    {
        if (!string.IsNullOrEmpty(abilityID) && !abilitiesToUnlock.Contains(abilityID))
        {
            abilitiesToUnlock.Add(abilityID);
        }
    }

    /// <summary>
    /// Add a skill to unlock at runtime
    /// </summary>
    public void AddSkillToUnlock(Skill skill)
    {
        if (skill != null && !skillsToUnlock.Contains(skill))
        {
            skillsToUnlock.Add(skill);
        }
    }

    /// <summary>
    /// Set the skill points reward at runtime
    /// </summary>
    public void SetSkillPointsReward(int points)
    {
        skillPointsReward = points;
    }

    /// <summary>
    /// Check if a specific ability is in the unlock list
    /// </summary>
    public bool ContainsAbility(string abilityID)
    {
        return abilitiesToUnlock.Contains(abilityID);
    }

    /// <summary>
    /// Check if a specific skill is in the unlock list
    /// </summary>
    public bool ContainsSkill(Skill skill)
    {
        return skillsToUnlock.Contains(skill);
    }

    /// <summary>
    /// Get the list of abilities that will be unlocked
    /// </summary>
    public List<string> GetAbilitiesToUnlock()
    {
        return new List<string>(abilitiesToUnlock);
    }

    /// <summary>
    /// Get the list of skills that will be unlocked
    /// </summary>
    public List<Skill> GetSkillsToUnlock()
    {
        return new List<Skill>(skillsToUnlock);
    }

    /// <summary>
    /// Check if this trigger has been unlocked
    /// </summary>
    public bool HasBeenUnlocked()
    {
        return hasBeenUnlocked;
    }

    /// <summary>
    /// Get the collectable rewards array
    /// </summary>
    public GameObject[] GetCollectableRewards()
    {
        return collectableRewards;
    }

    #endregion

    #region Editor Support

    protected override void DrawCustomGizmoLabels()
    {
        #if UNITY_EDITOR
        string label = "Skill Unlock Trigger\n";
        
        if (hasBeenUnlocked)
            label += "[UNLOCKED]\n";
        else
            label += "[LOCKED]\n";
        
        if (abilitiesToUnlock != null && abilitiesToUnlock.Count > 0)
        {
            label += $"Abilities: {abilitiesToUnlock.Count}\n";
        }
        
        if (skillsToUnlock != null && skillsToUnlock.Count > 0)
        {
            label += $"Skills: {skillsToUnlock.Count}\n";
            if (skillsToUnlock[0] != null)
            {
                label += $"Primary: {skillsToUnlock[0].SkillName}\n";
            }
        }
        
        if (skillPointsReward > 0)
        {
            label += $"SP Reward: {skillPointsReward}\n";
        }

        if (collectableRewards != null && collectableRewards.Length > 0)
        {
            label += $"Rewards: {collectableRewards.Length}\n";
        }
        
        if (triggerOnEnter)
            label += "[AUTO TRIGGER]";
        else if (requiresInput)
            label += $"[Press {interactKey}]";
        
        if (!canTriggerMultipleTimes)
            label += "\n[ONE TIME USE]";

        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, label);
        #endif
    }

    #endregion
}