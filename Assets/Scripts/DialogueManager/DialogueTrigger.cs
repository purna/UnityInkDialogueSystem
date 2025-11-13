using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Triggers dialogue when player enters the trigger zone
/// Matches LevelTrigger pattern with proper prompt management
/// </summary>
public class DialogueTrigger : MonoBehaviour
{
    [Header("Visual Cue")]
    [SerializeField] private GameObject visualCue;

    [Header("Emote Animator")]
    [SerializeField] private Animator emoteAnimator;

    [Header("Dialogue Settings")]
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private DialogueContainer dialogueContainer;
    [SerializeField] private string selectedGroupName;
    [SerializeField] private string selectedDialogueName;

    [Header("Trigger Settings")]
    [Tooltip("If TRUE, opens dialogue immediately when player enters (ignores input requirement)")]
    [SerializeField] private bool triggerOnEnter = false;
    [Tooltip("If TRUE, requires key press to open dialogue. If FALSE with triggerOnEnter FALSE, does nothing!")]
    [SerializeField] private bool requiresInput = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [Tooltip("If FALSE, trigger only works once. If TRUE, can be triggered multiple times.")]
    [SerializeField] private bool canTriggerMultipleTimes = true;
    [Tooltip("If TRUE, pressing the interact key again will close the dialogue")]
    [SerializeField] private bool toggleWithInteractKey = true;

    [Header("UI Prompt")]
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private TextMeshProUGUI promptTextComponent;
    [SerializeField] private string promptText = "Press E to Talk";
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private bool loopPrompt = true;

    private bool playerInRange;
    private bool hasTriggered;
    private bool dialogueWasOpenedByThisTrigger;
    private bool hasShownPromptThisEntry;

    private Dialogue cachedDialogue;
    private Coroutine promptCoroutine;
    private CanvasGroup promptCanvasGroup;

    public bool IsPlayerInRange => playerInRange;

    private void Awake() 
    {
        playerInRange = false;
        hasTriggered = false;
        dialogueWasOpenedByThisTrigger = false;
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

        CacheDialogueReferences();
    }

    private void CacheDialogueReferences()
    {
        if (dialogueController == null)
        {
            dialogueController = FindObjectOfType<DialogueController>();
            if (dialogueController == null)
            {
                Debug.LogWarning($"[DialogueTrigger:{gameObject.name}] DialogueController not found in scene!");
            }
        }

        if (dialogueContainer == null)
        {
            Debug.LogWarning($"[DialogueTrigger:{gameObject.name}] DialogueContainer is not assigned!");
            return;
        }

        if (!string.IsNullOrEmpty(selectedGroupName) && !string.IsNullOrEmpty(selectedDialogueName))
        {
            cachedDialogue = dialogueContainer.GetGroupDialogue(selectedGroupName, selectedDialogueName);
        }
        else if (!string.IsNullOrEmpty(selectedDialogueName))
        {
            cachedDialogue = dialogueContainer.GetUngroupedDialogue(selectedDialogueName);
        }

        if (cachedDialogue == null)
        {
            Debug.LogWarning($"[DialogueTrigger:{gameObject.name}] Could not find dialogue: {selectedDialogueName} in group: {selectedGroupName}");
        }
    }

    private void Update()
    {
        // Handle input when player is in range
        if (playerInRange && requiresInput && !triggerOnEnter)
        {
            if (Input.GetKeyDown(interactKey))
            {
                Debug.Log($"[DialogueTrigger:{gameObject.name}] Key '{interactKey}' pressed!");
                
                // Check if dialogue is currently open
                bool isThisDialogueOpen = IsDialogueOpen();
                
                if (isThisDialogueOpen && toggleWithInteractKey && dialogueWasOpenedByThisTrigger)
                {
                    // Close the dialogue
                    CloseDialogue();
                }
                else if (!isThisDialogueOpen && (canTriggerMultipleTimes || !hasTriggered))
                {
                    // Open the dialogue
                    TriggerDialogue();
                }
                else
                {
                    Debug.Log($"[DialogueTrigger:{gameObject.name}] Cannot interact (triggered: {hasTriggered}, dialogueOpen: {isThisDialogueOpen})");
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
        // 3. Dialogue is NOT already open (or wasn't opened by this trigger)
        // 4. Haven't shown prompt this entry yet
        bool isDialogueOpen = IsDialogueOpen();
        bool shouldShowPrompt = requiresInput && !triggerOnEnter && (!isDialogueOpen || !dialogueWasOpenedByThisTrigger) && !hasShownPromptThisEntry;

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
            // Hide prompt if dialogue is open
            if (interactPrompt != null && interactPrompt.activeSelf && isDialogueOpen)
            {
                StopPrompt();
            }
        }
    }

    private bool IsDialogueOpen()
    {
        if (dialogueController == null)
            return false;

        return dialogueController.IsDialogueOpen;
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

    private void TriggerDialogue()
    {
        Debug.Log($"[DialogueTrigger:{gameObject.name}] TriggerDialogue() called");
        
        if (dialogueController == null)
        {
            Debug.LogWarning($"[DialogueTrigger:{gameObject.name}] DialogueController not assigned!");
            return;
        }

        if (cachedDialogue == null)
        {
            Debug.LogWarning($"[DialogueTrigger:{gameObject.name}] No dialogue cached or assigned!");
            return;
        }

        // Trigger the dialogue with optional emote animator
        dialogueController.TriggerDialogue(cachedDialogue, emoteAnimator);
        
        hasTriggered = true;
        dialogueWasOpenedByThisTrigger = true;

        // Optional: Play emote animation
        if (emoteAnimator != null)
        {
            emoteAnimator.SetTrigger("Talk");
        }

        // Hide prompts after opening
        HideAllPrompts();

        Debug.Log($"[DialogueTrigger:{gameObject.name}] ✓ Dialogue started: {cachedDialogue.Name}");
    }

    private void CloseDialogue()
    {
        Debug.Log($"[DialogueTrigger:{gameObject.name}] Closing dialogue via toggle");
        
        if (dialogueController != null)
        {
            dialogueController.HideDialogue();
            dialogueWasOpenedByThisTrigger = false;
            Debug.Log($"[DialogueTrigger:{gameObject.name}] ✓ Dialogue UI closed");
        }
    }

    private void OnTriggerEnter2D(Collider2D collider) 
    {
        if (!collider.gameObject.CompareTag("Player"))
            return;

        Debug.Log($"[DialogueTrigger:{gameObject.name}] ✓ Player ENTERED trigger zone");
        playerInRange = true;
        hasShownPromptThisEntry = false;

        // Reset the flag when entering fresh
        // But only if UI is not currently open
        if (!IsDialogueOpen())
        {
            dialogueWasOpenedByThisTrigger = false;
        }

        if (triggerOnEnter)
        {
            if (canTriggerMultipleTimes || !hasTriggered)
            {
                Debug.Log($"[DialogueTrigger:{gameObject.name}] Auto-triggering on enter");
                TriggerDialogue();
            }
        }
        else
        {
            Debug.Log($"[DialogueTrigger:{gameObject.name}] Waiting for key press: {interactKey}");
        }
    }

    private void OnTriggerExit2D(Collider2D collider) 
    {
        if (!collider.gameObject.CompareTag("Player"))
            return;

        Debug.Log($"[DialogueTrigger:{gameObject.name}] ✓ Player EXITED trigger zone");
        playerInRange = false;
        
        // Reset triggered state when player leaves (if repeatable)
        if (canTriggerMultipleTimes)
        {
            hasTriggered = false;
        }
        
        // Reset the "opened by this trigger" flag
        // This allows the prompt to show again when returning
        dialogueWasOpenedByThisTrigger = false;
    }

    #region Public Methods

    /// <summary>
    /// Reset the trigger to allow re-triggering
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
        dialogueWasOpenedByThisTrigger = false;
        Debug.Log($"[DialogueTrigger:{gameObject.name}] Trigger reset");
    }

    /// <summary>
    /// Manually trigger dialogue from other scripts
    /// </summary>
    public void ManualTrigger()
    {
        if (canTriggerMultipleTimes || !hasTriggered)
        {
            TriggerDialogue();
        }
    }

    /// <summary>
    /// Set the target dialogue at runtime
    /// </summary>
    public void SetTargetDialogue(DialogueContainer container, string groupName, string dialogueName)
    {
        dialogueContainer = container;
        selectedGroupName = groupName;
        selectedDialogueName = dialogueName;
        CacheDialogueReferences();
    }

    /// <summary>
    /// Get the cached dialogue reference
    /// </summary>
    public Dialogue GetCachedDialogue()
    {
        return cachedDialogue;
    }

    #endregion

    #region Editor Support

    private void OnValidate()
    {
        if (Application.isPlaying) return;
        
        if (!triggerOnEnter && !requiresInput)
        {
            Debug.LogWarning($"[DialogueTrigger:{gameObject.name}] Both triggerOnEnter and requiresInput are FALSE - trigger will never activate!");
        }
        
        if (promptTextComponent != null && !string.IsNullOrEmpty(promptText))
        {
            promptTextComponent.text = promptText;
        }
        
        CacheDialogueReferences();
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
        if (dialogueContainer != null)
        {
            string label = $"Dialogue: {dialogueContainer.FileName}";
            if (!string.IsNullOrEmpty(selectedGroupName))
                label += $"\nGroup: {selectedGroupName}";
            if (!string.IsNullOrEmpty(selectedDialogueName))
                label += $"\nDialogue: {selectedDialogueName}";
            
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

    #endregion
}