using UnityEngine;
using System.Collections;

/// <summary>
/// Triggers the skill tree UI when player enters the trigger zone
/// Shows visual cues and emote animations, links to specific skill groups/skills
/// Integrates with SkillsTreeContainer, SkillsTreeGroup, and Skill system
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

    private bool skillTreeWasOpenedByThisTrigger;
    private Skill cachedSkill;

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

    protected override void InitializeReferences()
    {
        CacheSkillReferences();
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

    protected override void HandleInput()
    {
        if (playerInRange && requiresInput && !triggerOnEnter)
        {
            if (Input.GetKeyDown(interactKey))
            {
                if (showDebugLogs)
                    Debug.Log($"[SkillTreeTrigger:{gameObject.name}] Key '{interactKey}' pressed!");
                
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

    protected override bool ShouldShowPrompt()
    {
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

    public override void ResetTrigger()
    {
        base.ResetTrigger();
        skillTreeWasOpenedByThisTrigger = false;
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

    #endregion

    #region Editor Support

    protected override void OnValidate()
    {
        base.OnValidate();
        CacheSkillReferences();
    }

    protected override void DrawCustomGizmoLabels()
    {
        #if UNITY_EDITOR
        if (skillsTreeContainer != null)
        {
            string label = $"Skill Tree: {skillsTreeContainer.TreeName}";
            if (selectedGroup != null)
                label += $"\nGroup: {selectedGroup.GroupName}";
            if (!string.IsNullOrEmpty(selectedSkillName))
                label += $"\nSkill: {selectedSkillName}";
            
            if (triggerOnEnter)
                label += "\n[AUTO TRIGGER]";
            else if (requiresInput)
            {
                label += $"\n[Press {interactKey}]";
                if (toggleWithInteractKey)
                    label += " (Toggle)";
            }

            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, label);
        }
        
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