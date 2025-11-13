using UnityEngine;
using TMPro;
using System.Collections;

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
    [Tooltip("If TRUE, opens skill tree immediately when player enters (ignores input requirement)")]
    [SerializeField] private bool triggerOnEnter = false;
    [Tooltip("If TRUE, requires key press to open skill tree. If FALSE with triggerOnEnter FALSE, does nothing!")]
    [SerializeField] private bool requiresInput = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [Tooltip("If FALSE, trigger only works once. If TRUE, can be triggered multiple times.")]
    [SerializeField] private bool canTriggerMultipleTimes = true;
    [Tooltip("If TRUE, pressing the interact key again will close the skill tree")]
    [SerializeField] private bool toggleWithInteractKey = true;

    [Header("UI Prompt")]
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private TextMeshProUGUI promptTextComponent;
    [SerializeField] private string promptText = "Press E to view Skills";
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private bool loopPrompt = true;

    private bool playerInRange;
    private bool hasTriggered;
    private bool skillTreeWasOpenedByThisTrigger;
    private bool hasShownPromptThisEntry;
    private Skill cachedSkill;
    private Coroutine promptCoroutine;
    private CanvasGroup promptCanvasGroup;

    public bool IsPlayerInRange => playerInRange;

    private void Awake()
    {
        playerInRange = false;
        hasTriggered = false;
        skillTreeWasOpenedByThisTrigger = false;
        hasShownPromptThisEntry = false;

        if (visualCue != null)
        {
            visualCue.SetActive(false);
        }

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
            
            promptCanvasGroup = interactPrompt.GetComponent<CanvasGroup>();
            if (promptCanvasGroup == null)
            {
                promptCanvasGroup = interactPrompt.AddComponent<CanvasGroup>();
            }
            promptCanvasGroup.alpha = 0f;
            
            if (promptTextComponent != null)
            {
                promptTextComponent.text = promptText;
            }
        }

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

    private void Update()
    {
        // Handle input when player is in range
        if (playerInRange && requiresInput && !triggerOnEnter)
        {
            if (Input.GetKeyDown(interactKey))
            {
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
                    TriggerSkillTree();
                }
                else
                {
                    Debug.Log($"[SkillTreeTrigger:{gameObject.name}] Cannot interact (triggered: {hasTriggered}, treeOpen: {isThisSkillTreeOpen})");
                }
            }
        }
        
        // Handle visual feedback
        if (!playerInRange)
        {
            HideAllPrompts();
            return;
        }

        // Player is in range
        if (visualCue != null && !visualCue.activeSelf)
        {
            visualCue.SetActive(true);
        }

        // Show interact prompt ONLY if:
        // 1. We require input
        // 2. Not auto-triggering
        // 3. Skill tree is NOT already open (or wasn't opened by this trigger)
        // 4. Haven't shown prompt this entry yet
        bool isSkillTreeOpen = IsSkillTreeOpen();
        bool shouldShowPrompt = requiresInput && !triggerOnEnter && (!isSkillTreeOpen || !skillTreeWasOpenedByThisTrigger) && !hasShownPromptThisEntry;

        if (shouldShowPrompt)
        {
            if (interactPrompt != null && !interactPrompt.activeSelf)
            {
                interactPrompt.SetActive(true);

                if (promptCoroutine == null)
                {
                    loopPrompt = false; // Show once, don't loop
                    promptCoroutine = StartCoroutine(PromptFadeLoop());
                    hasShownPromptThisEntry = true;
                }
            }
        }
        else
        {
            // Hide prompt if tree is open
            if (interactPrompt != null && interactPrompt.activeSelf && isSkillTreeOpen)
            {
                StopPrompt();
            }
        }
    }

    private bool IsSkillTreeOpen()
    {
        if (skillsTreeController == null)
            return false;

        return skillsTreeController.IsSkillTreeOpen;
    }

    private void HideAllPrompts()
    {
        if (visualCue != null && visualCue.activeSelf)
        {
            visualCue.SetActive(false);
        }

        StopPrompt();
    }

    private void StopPrompt()
    {
        if (promptCoroutine != null)
        {
            StopCoroutine(promptCoroutine);
            promptCoroutine = null;
        }
        
        if (interactPrompt != null && interactPrompt.activeSelf)
        {
            interactPrompt.SetActive(false);
            if (promptCanvasGroup != null)
                promptCanvasGroup.alpha = 0f;
        }
    }

    private IEnumerator PromptFadeLoop()
    {
        if (promptCanvasGroup == null)
            yield break;

        while (true)
        {
            // Fade in
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                promptCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                yield return null;
            }
            promptCanvasGroup.alpha = 1f;

            // Display
            yield return new WaitForSeconds(displayDuration);

            // Fade out
            elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                promptCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
                yield return null;
            }
            promptCanvasGroup.alpha = 0f;

            if (!loopPrompt)
                break;

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void TriggerSkillTree()
    {
        Debug.Log($"[SkillTreeTrigger:{gameObject.name}] TriggerSkillTree() called");
        
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
        
        Debug.Log($"[SkillTreeTrigger:{gameObject.name}] ✓ Main skill tree panel opened");

        if (selectedGroup != null)
        {
            Debug.Log($"[SkillTreeTrigger:{gameObject.name}] Filtering to group: {selectedGroup.GroupName}");
        }

        if (cachedSkill != null)
        {
            StartCoroutine(ShowSkillDetailsDelayed(cachedSkill));
        }
        else
        {
            Debug.Log($"[SkillTreeTrigger:{gameObject.name}] No auto-select skill - showing main tree only");
        }

        hasTriggered = true;
        skillTreeWasOpenedByThisTrigger = true;

        if (emoteAnimator != null)
        {
            emoteAnimator.SetTrigger("Talk");
        }

        // Hide prompts after opening
        HideAllPrompts();

        string groupInfo = selectedGroup != null ? $"Group: {selectedGroup.GroupName}" : "All Groups";
        string skillInfo = cachedSkill != null ? $" → Auto-selecting: {cachedSkill.SkillName}" : " (No auto-select)";
        Debug.Log($"[SkillTreeTrigger:{gameObject.name}] ✓ Skill tree opened - {groupInfo}{skillInfo}");
    }

    private void CloseSkillTree()
    {
        Debug.Log($"[SkillTreeTrigger:{gameObject.name}] Closing skill tree via toggle");
        
        if (skillsTreeController != null)
        {
            skillsTreeController.HideSkillTree();
            skillTreeWasOpenedByThisTrigger = false;
            Debug.Log($"[SkillTreeTrigger:{gameObject.name}] ✓ Skill tree closed");
        }
    }

    private IEnumerator ShowSkillDetailsDelayed(Skill skill)
    {
        yield return null;
        
        if (skillsTreeController != null && skill != null)
        {
            skillsTreeController.ShowSkillDetails(skill);
            Debug.Log($"[SkillTreeTrigger:{gameObject.name}] ✓ Auto-selected skill details: {skill.SkillName}");
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!collider.gameObject.CompareTag("Player"))
            return;

        Debug.Log($"[SkillTreeTrigger:{gameObject.name}] ✓ Player ENTERED trigger zone");
        playerInRange = true;
        hasShownPromptThisEntry = false;

        // Reset the flag when entering fresh
        // But only if tree is not currently open
        if (!IsSkillTreeOpen())
        {
            skillTreeWasOpenedByThisTrigger = false;
        }

        if (triggerOnEnter)
        {
            if (canTriggerMultipleTimes || !hasTriggered)
            {
                Debug.Log($"[SkillTreeTrigger:{gameObject.name}] Auto-triggering on enter");
                TriggerSkillTree();
            }
        }
        else
        {
            Debug.Log($"[SkillTreeTrigger:{gameObject.name}] Waiting for key press: {interactKey}");
        }
    }

    private void OnTriggerExit2D(Collider2D collider) 
    {
        if (!collider.gameObject.CompareTag("Player"))
            return;

        Debug.Log($"[SkillTreeTrigger:{gameObject.name}] ✓ Player EXITED trigger zone");
        playerInRange = false;
        
        // Reset triggered state when player leaves (if repeatable)
        if (canTriggerMultipleTimes)
        {
            hasTriggered = false;
        }
        
        // Reset the "opened by this trigger" flag
        // This allows the prompt to show again when returning
        skillTreeWasOpenedByThisTrigger = false;
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
        skillTreeWasOpenedByThisTrigger = false;
        Debug.Log($"[SkillTreeTrigger:{gameObject.name}] Trigger reset");
    }

    public void ManualTrigger()
    {
        if (canTriggerMultipleTimes || !hasTriggered)
        {
            TriggerSkillTree();
        }
    }

    public void SetTargetSkill(SkillsTreeContainer container, SkillsTreeGroup group, string skillName)
    {
        skillsTreeContainer = container;
        selectedGroup = group;
        selectedSkillName = skillName;
        CacheSkillReferences();
    }

    public Skill GetCachedSkill()
    {
        return cachedSkill;
    }

    public SkillsTreeGroup GetSelectedGroup()
    {
        return selectedGroup;
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;
        
        if (!triggerOnEnter && !requiresInput)
        {
            Debug.LogWarning($"[SkillTreeTrigger:{gameObject.name}] Both triggerOnEnter and requiresInput are FALSE - trigger will never activate!");
        }
        
        if (promptTextComponent != null && !string.IsNullOrEmpty(promptText))
        {
            promptTextComponent.text = promptText;
        }
        
        CacheSkillReferences();
    }

    private void OnDrawGizmos()
    {
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
        #endif
    }
}