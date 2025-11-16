using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Triggers skill unlocks, ability unlocks, and skill point rewards when player enters the trigger zone
/// Can unlock multiple abilities and award skill points in a single trigger
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

    protected override void Awake()
    {
        // Set default prompt text for unlock triggers
        if (string.IsNullOrEmpty(promptText))
        {
            promptText = "Press E to Claim Reward";
        }

        base.Awake();
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

    protected override void OnTriggerActivated()
    {
        if (showDebugLogs)
            Debug.Log($"[SkillTreeUnlockTrigger:{gameObject.name}] OnTriggerActivated() called");
        
        if (skillTreeManager == null)
        {
            Debug.LogError($"[SkillTreeUnlockTrigger:{gameObject.name}] SkillTreeManager not found! Cannot unlock abilities.");
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
                // Only add to current points (e.g., temporary bonus, refund)
                // This modifies the private field directly through reflection or a new manager method
                // For now, we'll use AddSkillPoints which updates both
                // You may want to add a new method to SkillTreeManager: AddCurrentSkillPointsOnly(int amount)
                
                if (showDebugLogs)
                    Debug.LogWarning($"[SkillTreeUnlockTrigger:{gameObject.name}] 'addToCurrentOnly' is not fully supported - adds to both current and total");
                
                skillTreeManager.AddSkillPoints(skillPointsReward);
            }
            else
            {
                // Normal: add to both current and total earned
                skillTreeManager.AddSkillPoints(skillPointsReward);
            }
            
            if (showDebugLogs)
                Debug.Log($"[SkillTreeUnlockTrigger:{gameObject.name}] ✓ Awarded {skillPointsReward} skill points");
        }

        // Mark as triggered
        hasTriggered = true;

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
                      $"Unlocked: {unlockedCount} items, Awarded: {skillPointsReward} SP");
        }
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

    #endregion

    #region Editor Support

    protected override void DrawCustomGizmoLabels()
    {
        #if UNITY_EDITOR
        string label = "Skill Unlock Trigger\n";
        
        if (abilitiesToUnlock != null && abilitiesToUnlock.Count > 0)
        {
            label += $"Abilities: {abilitiesToUnlock.Count}\n";
        }
        
        if (skillsToUnlock != null && skillsToUnlock.Count > 0)
        {
            label += $"Skills: {skillsToUnlock.Count}\n";
        }
        
        if (skillPointsReward > 0)
        {
            label += $"SP Reward: {skillPointsReward}\n";
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