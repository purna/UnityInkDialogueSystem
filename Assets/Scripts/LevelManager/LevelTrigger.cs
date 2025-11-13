// old verion

using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Triggers the level UI when player enters the trigger zone
/// Shows visual cues and emote animations, links to specific level groups/levels
/// Integrates with LevelContainer, LevelGroup, and Level system
/// </summary>
public class LevelTrigger : MonoBehaviour
{
    [Header("Visual Cue")]
    [SerializeField] private GameObject visualCue;

    [Header("Emote Animator")]
    [SerializeField] private Animator emoteAnimator;

    [Header("Level Settings")]
    [SerializeField] private LevelController levelController;
    [SerializeField] private LevelContainer levelContainer;
    [SerializeField] private LevelGroup selectedGroup;


    [Tooltip("Leave empty to show all levels, or specify a level to auto-select it")]
    [SerializeField] private string selectedLevelName;

    [Header("Trigger Settings")]
    [Tooltip("If TRUE, opens level UI immediately when player enters (ignores input requirement)")]
    [SerializeField] private bool triggerOnEnter = false;
    [Tooltip("If TRUE, requires key press to open level UI. If FALSE with triggerOnEnter FALSE, does nothing!")]
    [SerializeField] private bool requiresInput = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [Tooltip("If FALSE, trigger only works once. If TRUE, can be triggered multiple times.")]
    [SerializeField] private bool canTriggerMultipleTimes = true;
    [Tooltip("If TRUE, pressing the interact key again will close the level UI")]
    [SerializeField] private bool toggleWithInteractKey = true;

    [Header("System References")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameObject levelUI;

    [Header("Auto Start Settings")]
    [SerializeField] private bool startLeveleOnStart = false;
    [SerializeField] private float startDelay = 0f;

    [Header("UI Prompt")]
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private TextMeshProUGUI promptTextComponent;
    [SerializeField] private string promptText = "Press E to view Levels";
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private bool loopPrompt = true;




    [Header("Tooltip Panel")]
    [SerializeField] private SkillTooltip _skillTooltip;

    [Header("Details Panel")]
    [SerializeField] private GameObject _detailsPanel;
    [SerializeField] private SkillTooltip _detailsPanelTooltip;


    [Header("Close Button")]
    [SerializeField] private Button _closeButton;
    [SerializeField] private bool _allowCloseWithKey = true;
    [SerializeField] private KeyCode _closeKey = KeyCode.Escape;

    [Header("Player Control")]
    [SerializeField] private GameObject _playerObject;
    [SerializeField] private bool _disablePlayerWhenOpen = true;


     private bool playerInRange;
    private bool hasTriggered;
    private bool levelWasOpenedByThisTrigger;
    private bool hasShownPromptThisEntry;

    private Level cachedLevel;
    private Coroutine promptCoroutine;
    private CanvasGroup promptCanvasGroup;

    public bool IsPlayerInRange => playerInRange;

    private void Awake()
    {
        playerInRange = false;
        hasTriggered = false;
        levelWasOpenedByThisTrigger = false;
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

        CacheLevelReferences();
    }

    private void CacheLevelReferences()
    {
        if (levelController == null)
        {
            Debug.LogWarning($"[LevelTrigger:{gameObject.name}] LevelController is not assigned!");
            return;
        }

        if (levelContainer == null)
        {
            Debug.LogWarning($"[LevelTrigger:{gameObject.name}] LevelContainer is not assigned!");
            return;
        }

        if (!string.IsNullOrEmpty(selectedLevelName))
        {
            if (selectedGroup != null)
            {
                cachedLevel = levelContainer.GetGroupLevel(selectedGroup.GroupName, selectedLevelName);

                if (cachedLevel == null)
                {
                    Debug.LogWarning($"[LevelTrigger:{gameObject.name}] Could not find level '{selectedLevelName}' in group '{selectedGroup.GroupName}'");
                }
            }
            else
            {
                cachedLevel = levelContainer.GetLevelByName(selectedLevelName);

                if (cachedLevel == null)
                {
                    Debug.LogWarning($"[LevelTrigger:{gameObject.name}] Could not find level '{selectedLevelName}' in container");
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
                bool isThisSkillTreeOpen = IsLevelOpen();
                
                if (isThisSkillTreeOpen && toggleWithInteractKey && levelWasOpenedByThisTrigger)
                {
                    // Close the skill tree
                    CloseLevel();
                }
                else if (!isThisSkillTreeOpen && (canTriggerMultipleTimes || !hasTriggered))
                {
                    // Open the skill tree
                    TriggerLevel();
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
        bool isLevelOpen = IsLevelOpen();
        bool shouldShowPrompt = requiresInput && !triggerOnEnter && (!isLevelOpen || !levelWasOpenedByThisTrigger) && !hasShownPromptThisEntry;

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
            if (interactPrompt != null && interactPrompt.activeSelf && isLevelOpen)
            {
                StopPrompt();
            }
        }
    }

        private bool IsLevelOpen()
    {
        if (levelController == null)
            return false;

        return levelController.IsLevelOpen;
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

    private void TriggerLevel()
    {
        Debug.Log($"[LevelTrigger:{gameObject.name}] TriggerLevel() called");
        
        if (levelController == null)
        {
            Debug.LogWarning($"[LevelTrigger:{gameObject.name}] LevelController not assigned!");
            return;
        }

        if (levelContainer == null)
        {
            Debug.LogWarning($"[LevelTrigger:{gameObject.name}] LevelContainer not assigned!");
            return;
        }

        levelController.SetLevelContainer(levelContainer);
        levelController.ShowLevel();
        
        Debug.Log($"[LevelTrigger:{gameObject.name}] ✓ Main level UI panel opened");

        if (selectedGroup != null)
        {
            Debug.Log($"[LevelTrigger:{gameObject.name}] Filtering to group: {selectedGroup.GroupName}");
        }

        if (cachedLevel != null)
        {
            StartCoroutine(ShowLevelDetailsDelayed(cachedLevel));
        }
        else
        {
            Debug.Log($"[LevelTrigger:{gameObject.name}] No auto-select level - showing main UI only");
        }

        hasTriggered = true;
        levelWasOpenedByThisTrigger = true;

        if (emoteAnimator != null)
        {
            emoteAnimator.SetTrigger("Talk");
        }

        // Hide prompts after opening
        HideAllPrompts();

        string groupInfo = selectedGroup != null ? $"Group: {selectedGroup.GroupName}" : "All Groups";
        string levelInfo = cachedLevel != null ? $" → Auto-selecting: {cachedLevel.LevelName}" : " (No auto-select)";
        Debug.Log($"[LevelTrigger:{gameObject.name}] ✓ Level UI opened - {groupInfo}{levelInfo}");
    }

    private void CloseLevel()
    {
        Debug.Log($"[LevelTrigger:{gameObject.name}] Closing level UI via toggle");
        
        if (levelController != null)
        {
            levelController.HideLevel();
            levelWasOpenedByThisTrigger = false;
            Debug.Log($"[LevelTrigger:{gameObject.name}] ✓ Level UI closed");
        }
    }

    private IEnumerator ShowLevelDetailsDelayed(Level level)
    {
        yield return null;
        
        if (levelController != null && level != null)
        {
            levelController.ShowLevelDetails(level);
            Debug.Log($"[LevelTrigger:{gameObject.name}] ✓ Auto-selected level details: {level.LevelName}");
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!collider.gameObject.CompareTag("Player"))
            return;

        Debug.Log($"[LevelTrigger:{gameObject.name}] ✓ Player ENTERED trigger zone");
        playerInRange = true;
        hasShownPromptThisEntry = false;

        // Reset the flag when entering fresh
        // But only if UI is not currently open
        if (!IsLevelOpen())
        {
            levelWasOpenedByThisTrigger = false;
        }

        if (triggerOnEnter)
        {
            if (canTriggerMultipleTimes || !hasTriggered)
            {
                Debug.Log($"[LevelTrigger:{gameObject.name}] Auto-triggering on enter");
                TriggerLevel();
            }
        }
        else
        {
            Debug.Log($"[LevelTrigger:{gameObject.name}] Waiting for key press: {interactKey}");
        }
    }

    private void OnTriggerExit2D(Collider2D collider) 
    {
        if (!collider.gameObject.CompareTag("Player"))
            return;

        Debug.Log($"[LevelTrigger:{gameObject.name}] ✓ Player EXITED trigger zone");
        playerInRange = false;
        
        // Reset triggered state when player leaves (if repeatable)
        if (canTriggerMultipleTimes)
        {
            hasTriggered = false;
        }
        
        // Reset the "opened by this trigger" flag
        // This allows the prompt to show again when returning
        levelWasOpenedByThisTrigger = false;
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
        levelWasOpenedByThisTrigger = false;
        Debug.Log($"[LevelTrigger:{gameObject.name}] Trigger reset");
    }

    public void ManualTrigger()
    {
        if (canTriggerMultipleTimes || !hasTriggered)
        {
            TriggerLevel();
        }
    }

    public void SetTargetLevel(LevelContainer container, LevelGroup group, string levelName)
    {
        levelContainer = container;
        selectedGroup = group;
        selectedLevelName = levelName;
        CacheLevelReferences();
    }

    public Level GetCachedLevel()
    {
        return cachedLevel;
    }

    public LevelGroup GetSelectedGroup()
    {
        return selectedGroup;
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;
        
        if (!triggerOnEnter && !requiresInput)
        {
            Debug.LogWarning($"[LevelTrigger:{gameObject.name}] Both triggerOnEnter and requiresInput are FALSE - trigger will never activate!");
        }
        
        if (promptTextComponent != null && !string.IsNullOrEmpty(promptText))
        {
            promptTextComponent.text = promptText;
        }
        
        CacheLevelReferences();
    }

    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = playerInRange ? Color.green : Color.cyan;
            
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
        if (levelContainer != null)
        {
            string label = $"Level UI: {levelContainer.LevelName}";
            if (selectedGroup != null)
                label += $"\nGroup: {selectedGroup.GroupName}";
            if (!string.IsNullOrEmpty(selectedLevelName))
                label += $"\nLevel: {selectedLevelName}";
            
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