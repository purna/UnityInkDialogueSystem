using UnityEngine;

/// <summary>
/// Triggers the skill tree UI when player enters the trigger zone
/// Shows visual cues and emote animations, links to specific skill groups/skills
/// Integrates with LevelContainer, LevelGroup, and Level system
/// </summary>
public class LevelTrigger : MonoBehaviour
{
    [Header("Visual Cue")]
    [SerializeField] private GameObject visualCue;

    [Header("Emote Animator")]
    [SerializeField] private Animator emoteAnimator;

    [Header("Level Tree Settings")]
    [SerializeField] private LevelController levelController;
    [SerializeField] private LevelContainer levelContainer;
    [SerializeField] private LevelGroup selectedGroup;
    [Tooltip("Leave empty to show all skills, or specify a skill to auto-select it")]
    [SerializeField] private string selectedLevelName;

    [Header("Trigger Settings")]
    [SerializeField] private bool triggerOnEnter = false;
    [SerializeField] private bool requiresInput = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool canTriggerMultipleTimes = true;

    [Header("UI Prompt")]
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private string promptText = "Press E to view Levels";

    public bool playerInRange;
    private bool hasTriggered;
    private Level cachedLevel;

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
        CacheLevelReferences();
    }

    private void CacheLevelReferences()
    {
        // Validate references
        if (levelController == null)
        {
            Debug.LogWarning("[LevelTrigger] LevelController is not assigned!");
            return;
        }

        if (levelContainer == null)
        {
            Debug.LogWarning("[LevelTrigger] LevelContainer is not assigned!");
            return;
        }

        // Try to find the specific skill if a name is provided
        if (!string.IsNullOrEmpty(selectedLevelName))
        {
            if (selectedGroup != null)
            {
                // Look in the specific group
                cachedLevel = levelContainer.GetGroupLevel(selectedGroup.GroupName, selectedLevelName);
                
                if (cachedLevel == null)
                {
                    Debug.LogWarning($"[LevelTrigger] Could not find skill '{selectedLevelName}' in group '{selectedGroup.GroupName}'");
                }
            }
            else
            {
                // Search in container (both grouped and ungrouped)
                cachedLevel = levelContainer.GetLevelByName(selectedLevelName);
                
                if (cachedLevel == null)
                {
                    Debug.LogWarning($"[LevelTrigger] Could not find skill '{selectedLevelName}' in container");
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
                        TriggerLevel();
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

    private void TriggerLevel()
    {
        if (levelController == null)
        {
            Debug.LogWarning("[LevelTrigger] LevelController not assigned!");
            return;
        }

        if (levelContainer == null)
        {
            Debug.LogWarning("[LevelTrigger] LevelContainer not assigned!");
            return;
        }

        // Set the container on the controller if needed
        levelController.SetLevelContainer(levelContainer);

        // Open the skill tree UI
        levelController.ShowLevel();

        // If a specific group is selected, configure the controller to show it
        if (selectedGroup != null)
        {
            // You may need to add a method to the controller to set the group
            // For now, we'll just note it in the log
            Debug.Log($"[LevelTrigger] Opening skill tree with group: {selectedGroup.GroupName}");
            
            // If your controller has a method to filter by group, call it here:
            // levelController.SetLevelGroup(selectedGroup);
        }

        // If a specific skill is selected, show its details
        if (cachedLevel != null)
        {
            levelController.ShowLevelDetails(cachedLevel);
            Debug.Log($"[LevelTrigger] Auto-selecting skill: {cachedLevel.LevelName}");
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
        string skillInfo = cachedLevel != null ? $", Level: {cachedLevel.LevelName}" : "";
        Debug.Log($"[LevelTrigger] Opened skill tree - {groupInfo}{skillInfo}");
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
                    TriggerLevel();
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
        TriggerLevel();
    }

    // Public method to set which container/group/skill to show
    public void SetTargetLevel(LevelContainer container, LevelGroup group, string skillName)
    {
        levelContainer = container;
        selectedGroup = group;
        selectedLevelName = skillName;
        CacheLevelReferences();
    }

    // Public method to get cached skill (useful for debugging)
    public Level GetCachedLevel()
    {
        return cachedLevel;
    }

    // Public method to get selected group
    public LevelGroup GetSelectedGroup()
    {
        return selectedGroup;
    }

    // Validation in editor
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        CacheLevelReferences();
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
        if (levelContainer != null)
        {
            string label = $"Level Tree: {levelContainer.LevelName}";
            if (selectedGroup != null)
                label += $"\nGroup: {selectedGroup.GroupName}";
            if (!string.IsNullOrEmpty(selectedLevelName))
                label += $"\nLevel: {selectedLevelName}";

            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, label);
        }
        #endif
    }
}