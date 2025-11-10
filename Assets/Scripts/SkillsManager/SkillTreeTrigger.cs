using UnityEngine;

/// <summary>
/// Triggers the skill tree UI when player enters the trigger zone
/// Shows visual cues and emote animations, links to specific skill groups/skills
/// Integrates with SkillsTreeContainer, SkillsTreeGroup, and Skill system
/// </summary>
public class SkillTreeTrigger : MonoBehaviour
{
    [Header("Visual Cue")]
    [SerializeField] private GameObject visualCue;

    [Header("Emote Animator")]
    [SerializeField] private Animator emoteAnimator;

    [Header("Skill Tree Settings")]
    [SerializeField] private SkillsTreeController skillsTreeController;
    [SerializeField] private SkillsTreeContainer skillsTreeContainer;
    [SerializeField] private SkillsTreeGroup selectedGroup;
    [Tooltip("Leave empty to show all skills, or specify a skill to auto-select it")]
    [SerializeField] private string selectedSkillName;

    [Header("Trigger Settings")]
    [SerializeField] private bool triggerOnEnter = false;
    [SerializeField] private bool requiresInput = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool canTriggerMultipleTimes = true;

    [Header("UI Prompt")]
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private string promptText = "Press E to view Skills";

    public bool playerInRange;
    private bool hasTriggered;
    private Skill cachedSkill;

    private void Awake() 
    {
        playerInRange = false;
        hasTriggered = false;

        if (visualCue != null)
        {
            visualCue.SetActive(false);
        }

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }

        // Cache the skill reference
        CacheSkillReferences();
    }

    private void CacheSkillReferences()
    {
        // Validate references
        if (skillsTreeController == null)
        {
            Debug.LogWarning("[SkillTreeTrigger] SkillsTreeController is not assigned!");
            return;
        }

        if (skillsTreeContainer == null)
        {
            Debug.LogWarning("[SkillTreeTrigger] SkillsTreeContainer is not assigned!");
            return;
        }

        // Try to find the specific skill if a name is provided
        if (!string.IsNullOrEmpty(selectedSkillName))
        {
            if (selectedGroup != null)
            {
                // Look in the specific group
                cachedSkill = skillsTreeContainer.GetGroupSkill(selectedGroup.GroupName, selectedSkillName);
                
                if (cachedSkill == null)
                {
                    Debug.LogWarning($"[SkillTreeTrigger] Could not find skill '{selectedSkillName}' in group '{selectedGroup.GroupName}'");
                }
            }
            else
            {
                // Search in container (both grouped and ungrouped)
                cachedSkill = skillsTreeContainer.GetSkillByName(selectedSkillName);
                
                if (cachedSkill == null)
                {
                    Debug.LogWarning($"[SkillTreeTrigger] Could not find skill '{selectedSkillName}' in container");
                }
            }
        }
    }

    private void Update()
    {
        if (playerInRange) 
        {
            // Show visual cue
            if (visualCue != null)
            {
                visualCue.SetActive(true);
            }

            // Show interact prompt
            if (interactPrompt != null && requiresInput)
            {
                interactPrompt.SetActive(true);
            }

            // Check for interaction input
            if (requiresInput)
            {
                if (Input.GetKeyDown(interactKey))
                {
                    if (canTriggerMultipleTimes || !hasTriggered)
                    {
                        TriggerSkillTree();
                    }
                }
            }
        }
        else 
        {
            // Hide visual cue
            if (visualCue != null)
            {
                visualCue.SetActive(false);
            }

            // Hide interact prompt
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }
    }

    private void TriggerSkillTree()
    {
        if (skillsTreeController == null)
        {
            Debug.LogWarning("[SkillTreeTrigger] SkillsTreeController not assigned!");
            return;
        }

        if (skillsTreeContainer == null)
        {
            Debug.LogWarning("[SkillTreeTrigger] SkillsTreeContainer not assigned!");
            return;
        }

        // Set the container on the controller if needed
        skillsTreeController.SetSkillTreeContainer(skillsTreeContainer);

        // Open the skill tree UI
        skillsTreeController.ShowSkillTree();

        // If a specific group is selected, configure the controller to show it
        if (selectedGroup != null)
        {
            // You may need to add a method to the controller to set the group
            // For now, we'll just note it in the log
            Debug.Log($"[SkillTreeTrigger] Opening skill tree with group: {selectedGroup.GroupName}");
            
            // If your controller has a method to filter by group, call it here:
            // skillsTreeController.SetSkillGroup(selectedGroup);
        }

        // If a specific skill is selected, show its details
        if (cachedSkill != null)
        {
            skillsTreeController.ShowSkillDetails(cachedSkill);
            Debug.Log($"[SkillTreeTrigger] Auto-selecting skill: {cachedSkill.SkillName}");
        }

        hasTriggered = true;

        // Optional: Play emote animation
        if (emoteAnimator != null)
        {
            emoteAnimator.SetTrigger("Talk");
        }

        // Hide visual cue after triggering
        if (visualCue != null)
        {
            visualCue.SetActive(false);
        }

        // Hide interact prompt
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }

        string groupInfo = selectedGroup != null ? $"Group: {selectedGroup.GroupName}" : "All Groups";
        string skillInfo = cachedSkill != null ? $", Skill: {cachedSkill.SkillName}" : "";
        Debug.Log($"[SkillTreeTrigger] Opened skill tree - {groupInfo}{skillInfo}");
    }

    private void OnTriggerEnter2D(Collider2D collider) 
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            playerInRange = true;

            // Auto-trigger on enter if enabled
            if (triggerOnEnter)
            {
                if (canTriggerMultipleTimes || !hasTriggered)
                {
                    TriggerSkillTree();
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider) 
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            playerInRange = false;
            
            // Reset triggered state when player leaves (if repeatable)
            if (canTriggerMultipleTimes)
            {
                hasTriggered = false;
            }
        }
    }

    // Public method to reset the trigger (useful for one-time triggers that need to be reset)
    public void ResetTrigger()
    {
        hasTriggered = false;
    }

    // Public method to manually trigger skill tree from other scripts
    public void ManualTrigger()
    {
        TriggerSkillTree();
    }

    // Public method to set which container/group/skill to show
    public void SetTargetSkill(SkillsTreeContainer container, SkillsTreeGroup group, string skillName)
    {
        skillsTreeContainer = container;
        selectedGroup = group;
        selectedSkillName = skillName;
        CacheSkillReferences();
    }

    // Public method to get cached skill (useful for debugging)
    public Skill GetCachedSkill()
    {
        return cachedSkill;
    }

    // Public method to get selected group
    public SkillsTreeGroup GetSelectedGroup()
    {
        return selectedGroup;
    }

    // Validation in editor
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        CacheSkillReferences();
    }

    private void OnDrawGizmos()
    {
        // Draw the trigger area in the editor
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = playerInRange ? Color.green : Color.yellow;
            
            if (col is BoxCollider2D boxCol)
            {
                Gizmos.DrawWireCube(transform.position + (Vector3)boxCol.offset, boxCol.size);
            }
            else if (col is CircleCollider2D circleCol)
            {
                Gizmos.DrawWireSphere(transform.position + (Vector3)circleCol.offset, circleCol.radius);
            }
        }

        // Draw a label showing what this trigger opens
        #if UNITY_EDITOR
        if (skillsTreeContainer != null)
        {
            string label = $"Skill Tree: {skillsTreeContainer.TreeName}";
            if (selectedGroup != null)
                label += $"\nGroup: {selectedGroup.GroupName}";
            if (!string.IsNullOrEmpty(selectedSkillName))
                label += $"\nSkill: {selectedSkillName}";

            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, label);
        }
        #endif
    }
}