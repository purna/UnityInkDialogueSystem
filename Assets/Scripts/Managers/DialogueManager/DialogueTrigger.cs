using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Position modes for WorldSpace dialogue relative to NPC
/// </summary>
public enum WorldSpaceDialoguePosition
{
    Above,
    Below,
    Left,
    Right,
    Auto // Automatically choose best position based on screen edges
}

/// <summary>
/// Triggers dialogue when player enters the trigger zone
/// Supports NPC transform attachment for WorldSpace mode with smart positioning
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
    
    [Header("World Space Settings")]
    [Tooltip("Optional: NPC transform to attach WorldSpace dialogue to. If not set, uses this trigger's transform.")]
    [SerializeField] private Transform npcTransform;
    
    [Tooltip("Position of dialogue relative to NPC. Auto will choose best position based on screen edges.")]
    [SerializeField] private WorldSpaceDialoguePosition dialoguePosition = WorldSpaceDialoguePosition.Auto;
    
    [Tooltip("Horizontal distance from NPC")]
    [SerializeField] private float horizontalOffset = 2f;
    
    [Tooltip("Vertical distance from NPC")]
    [SerializeField] private float verticalOffset = 2.5f;
    
    [Tooltip("Margin from screen edges (0-1, percentage of screen)")]
    [SerializeField][Range(0f, 0.5f)] private float screenEdgeMargin = 0.15f;

    [Header("Trigger Settings")]
    [Tooltip("If TRUE, opens dialogue immediately when player enters (ignores input requirement)")]
    [SerializeField] private bool triggerOnEnter = false;
    [Tooltip("If TRUE, requires key press to open dialogue. If FALSE with triggerOnEnter FALSE, does nothing!")]
    [SerializeField] private bool requiresInput = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [Tooltip("If FALSE, trigger only works once. If TRUE, can be triggered multiple times.")]
    [SerializeField] private bool canTriggerMultipleTimes = true;
    [Tooltip("If TRUE, pressing the interact key again will close the dialogue")]
    [SerializeField] private bool toggleWithInteractKey = false;

    [Header("UI Prompt")]
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private TextMeshProUGUI promptTextComponent;
    [SerializeField] private string promptText = "Press E to Talk";
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private bool loopPrompt = true;
    
    [Header("Dynamic Prompt Positioning")]
    [Tooltip("Horizontal offset for prompt when positioned on the left")]
    [SerializeField] private float promptLeftOffset = -3.5f;
    [Tooltip("Horizontal offset for prompt when positioned on the right")]
    [SerializeField] private float promptRightOffset = 3.5f;
    [Tooltip("Vertical offset for prompt when positioned above")]
    [SerializeField] private float promptAboveOffset = 3.5f;
    [Tooltip("Vertical offset for prompt when positioned below")]
    [SerializeField] private float promptBelowOffset = 3.5f;
    [Tooltip("Additional vertical offset for the prompt")]
    [SerializeField] private float promptVerticalOffset = 0.5f;
    [Tooltip("Update prompt position every frame (enable for moving triggers)")]
    [SerializeField] private bool continuousPositionUpdate = false;

    private bool playerInRange;
    private bool hasTriggered;
    private bool dialogueWasOpenedByThisTrigger;
    private bool hasShownPromptThisEntry;

    private Dialogue cachedDialogue;
    private Coroutine promptCoroutine;
    private CanvasGroup promptCanvasGroup;
    private RectTransform promptRectTransform;
    private Canvas promptCanvas;
    private Camera mainCamera;

    public bool IsPlayerInRange => playerInRange;

    private void Awake() 
    {
        playerInRange = false;
        hasTriggered = false;
        dialogueWasOpenedByThisTrigger = false;
        hasShownPromptThisEntry = false;
        
        mainCamera = Camera.main;

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
            
            promptRectTransform = interactPrompt.GetComponent<RectTransform>();
            promptCanvas = interactPrompt.GetComponentInParent<Canvas>();

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

        // Show interact prompt ONLY if appropriate
        bool isDialogueOpen = IsDialogueOpen();
        bool shouldShowPrompt = requiresInput && !triggerOnEnter && (!isDialogueOpen || !dialogueWasOpenedByThisTrigger) && !hasShownPromptThisEntry;

        if (shouldShowPrompt)
        {
            if (interactPrompt != null && !interactPrompt.activeSelf)
            {
                interactPrompt.SetActive(true);
                UpdatePromptPosition(); // Position when first shown

                if (promptCoroutine == null)
                {
                    loopPrompt = false; // Show once, don't loop
                    promptCoroutine = StartCoroutine(PromptFadeLoop());
                    hasShownPromptThisEntry = true;
                }
            }
            else if (continuousPositionUpdate && interactPrompt != null && interactPrompt.activeSelf)
            {
                // Update position every frame if enabled (for moving triggers)
                UpdatePromptPosition();
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

    /// <summary>
    /// Positions the prompt on the side of the trigger furthest from screen edges (left or right only)
    /// </summary>
    private void UpdatePromptPosition()
    {
        if (interactPrompt == null || promptRectTransform == null || mainCamera == null)
            return;

        // Get trigger position in screen space
        Vector3 triggerWorldPos = transform.position;
        Vector3 triggerScreenPos = mainCamera.WorldToScreenPoint(triggerWorldPos);

        // Get screen dimensions
        float screenWidth = Screen.width;

        // Calculate distances to left and right edges
        float distToLeft = triggerScreenPos.x;
        float distToRight = screenWidth - triggerScreenPos.x;

        // Determine horizontal offset (left = -3.5, right = +3.5)
        float horizontalOffset;
        if (distToLeft > distToRight)
        {
            // More space on left, position to the left
            horizontalOffset = promptLeftOffset;
        }
        else
        {
            // More space on right, position to the right
            horizontalOffset = promptRightOffset;
        }

        // For UI elements, we work with the RectTransform's anchoredPosition
        if (promptCanvas != null)
        {
            if (promptCanvas.renderMode == RenderMode.ScreenSpaceOverlay || promptCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                // For screen space canvases, convert world position to screen space first
                Vector3 screenPos = mainCamera.WorldToScreenPoint(triggerWorldPos);
                
                // Convert screen position to canvas local position
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    promptCanvas.transform as RectTransform,
                    screenPos,
                    promptCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : promptCanvas.worldCamera,
                    out Vector2 localPoint
                );
                
                // Apply the fixed offsets directly to the anchored position
                promptRectTransform.anchoredPosition = new Vector2(localPoint.x + horizontalOffset, localPoint.y + promptVerticalOffset);
            }
            else // World Space
            {
                // For world space, calculate world position and apply
                Vector3 promptWorldPos = triggerWorldPos + new Vector3(horizontalOffset, promptVerticalOffset, 0f);
                promptRectTransform.position = promptWorldPos;
            }
        }
        else
        {
            // Fallback: calculate world position
            Vector3 promptWorldPos = triggerWorldPos + new Vector3(horizontalOffset, promptVerticalOffset, 0f);
            promptRectTransform.position = promptWorldPos;
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

    /// <summary>
    /// Calculate the smart position for WorldSpace dialogue based on NPC position and screen edges
    /// </summary>
    private Vector3 CalculateDialogueOffset(Transform npc)
    {
        if (Camera.main == null)
        {
            Debug.LogWarning("[DialogueTrigger] No main camera found, using default offset");
            return new Vector3(0, verticalOffset, 0);
        }

        // Get NPC's viewport position (0-1 range)
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(npc.position);
        
        WorldSpaceDialoguePosition finalPosition = dialoguePosition;
        
        // Auto mode: choose best position based on screen location
        if (dialoguePosition == WorldSpaceDialoguePosition.Auto)
        {
            finalPosition = DetermineAutoPosition(viewportPos);
        }
        // Override position if NPC is near screen edges
        else
        {
            finalPosition = CheckEdgeOverrides(viewportPos, dialoguePosition);
        }
        
        // Calculate offset based on final position
        return GetOffsetForPosition(finalPosition, viewportPos);
    }

    /// <summary>
    /// Automatically determine best position based on NPC's screen position
    /// </summary>
    private WorldSpaceDialoguePosition DetermineAutoPosition(Vector3 viewportPos)
    {
        // Check if near top or bottom edges
        bool nearTop = viewportPos.y > (1f - screenEdgeMargin);
        bool nearBottom = viewportPos.y < screenEdgeMargin;
        
        // Check if near left or right edges
        bool nearLeft = viewportPos.x < screenEdgeMargin;
        bool nearRight = viewportPos.x > (1f - screenEdgeMargin);
        
        // Priority: avoid screen edges
        if (nearTop && nearBottom)
        {
            // In center vertically, place to the side
            return viewportPos.x < 0.5f ? WorldSpaceDialoguePosition.Right : WorldSpaceDialoguePosition.Left;
        }
        else if (nearTop)
        {
            // Near top, place below or to the side
            if (nearLeft) return WorldSpaceDialoguePosition.Right;
            if (nearRight) return WorldSpaceDialoguePosition.Left;
            return WorldSpaceDialoguePosition.Below;
        }
        else if (nearBottom)
        {
            // Near bottom, place above or to the side
            if (nearLeft) return WorldSpaceDialoguePosition.Right;
            if (nearRight) return WorldSpaceDialoguePosition.Left;
            return WorldSpaceDialoguePosition.Above;
        }
        else if (nearLeft)
        {
            // Near left edge, place to the right
            return WorldSpaceDialoguePosition.Right;
        }
        else if (nearRight)
        {
            // Near right edge, place to the left
            return WorldSpaceDialoguePosition.Left;
        }
        
        // Default: place based on screen quadrant
        if (viewportPos.y > 0.5f)
            return WorldSpaceDialoguePosition.Below; // NPC in top half, show below
        else
            return WorldSpaceDialoguePosition.Above; // NPC in bottom half, show above
    }

    /// <summary>
    /// Check if manual position needs to be overridden due to screen edges
    /// </summary>
    private WorldSpaceDialoguePosition CheckEdgeOverrides(Vector3 viewportPos, WorldSpaceDialoguePosition requestedPosition)
    {
        bool nearTop = viewportPos.y > (1f - screenEdgeMargin);
        bool nearBottom = viewportPos.y < screenEdgeMargin;
        bool nearLeft = viewportPos.x < screenEdgeMargin;
        bool nearRight = viewportPos.x > (1f - screenEdgeMargin);
        
        switch (requestedPosition)
        {
            case WorldSpaceDialoguePosition.Above:
                if (nearTop) return WorldSpaceDialoguePosition.Below;
                break;
                
            case WorldSpaceDialoguePosition.Below:
                if (nearBottom) return WorldSpaceDialoguePosition.Above;
                break;
                
            case WorldSpaceDialoguePosition.Left:
                if (nearLeft) return WorldSpaceDialoguePosition.Right;
                break;
                
            case WorldSpaceDialoguePosition.Right:
                if (nearRight) return WorldSpaceDialoguePosition.Left;
                break;
        }
        
        return requestedPosition;
    }

    /// <summary>
    /// Get the offset vector for a specific position
    /// </summary>
    private Vector3 GetOffsetForPosition(WorldSpaceDialoguePosition position, Vector3 viewportPos)
    {
        switch (position)
        {
            case WorldSpaceDialoguePosition.Above:
                return new Vector3(0, promptAboveOffset, 0);
                
            case WorldSpaceDialoguePosition.Below:
                return new Vector3(0, -promptBelowOffset, 0);
                
            case WorldSpaceDialoguePosition.Left:
                return new Vector3(-horizontalOffset, verticalOffset * 0.5f, 0);
                
            case WorldSpaceDialoguePosition.Right:
                return new Vector3(horizontalOffset, verticalOffset * 0.5f, 0);
                
            default:
                return new Vector3(0, verticalOffset, 0);
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

        // Use npcTransform if specified, otherwise use trigger's transform for WorldSpace
        Transform targetNPC = npcTransform != null ? npcTransform : transform;
        
        // Calculate smart offset for WorldSpace mode
        Vector3 offset = CalculateDialogueOffset(targetNPC);
        
        // Apply offset to DialogueUI (we'll need to update DialogueUI to accept offset)
        DialogueUI dialogueUI = dialogueController.GetActiveDialogueUI();
        if (dialogueUI != null)
        {
            dialogueUI.SetWorldSpaceOffset(offset);
        }

        // Trigger the dialogue with optional emote animator and NPC transform
        dialogueController.TriggerDialogue(cachedDialogue, emoteAnimator, targetNPC);
        
        hasTriggered = true;
        dialogueWasOpenedByThisTrigger = true;

        // Optional: Play emote animation
        if (emoteAnimator != null)
        {
            emoteAnimator.SetTrigger("Talk");
        }

        // Hide prompts after opening
        HideAllPrompts();

        Debug.Log($"[DialogueTrigger:{gameObject.name}] ✓ Dialogue started: {cachedDialogue.Name} at offset: {offset}");
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
    /// Set the NPC transform for WorldSpace attachment at runtime
    /// </summary>
    public void SetNPCTransform(Transform npc)
    {
        npcTransform = npc;
        Debug.Log($"[DialogueTrigger:{gameObject.name}] NPC Transform set to: {(npc != null ? npc.name : "null")}");
    }
    
    /// <summary>
    /// Set the dialogue position mode at runtime
    /// </summary>
    public void SetDialoguePosition(WorldSpaceDialoguePosition position)
    {
        dialoguePosition = position;
    }
    
    /// <summary>
    /// Set custom offsets at runtime
    /// </summary>
    public void SetOffsets(float horizontal, float vertical)
    {
        horizontalOffset = horizontal;
        verticalOffset = vertical;
    }

    /// <summary>
    /// Get the cached dialogue reference
    /// </summary>
    public Dialogue GetCachedDialogue()
    {
        return cachedDialogue;
    }
    
    /// <summary>
    /// Get current dialogue position setting
    /// </summary>
    public WorldSpaceDialoguePosition GetDialoguePosition()
    {
        return dialoguePosition;
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

        // Draw dialogue position indicator in WorldSpace mode
        if (dialogueController != null && dialogueController.GetSetupMode() == DialogueSetupMode.WorldSpace)
        {
            Transform targetNPC = npcTransform != null ? npcTransform : transform;
            Vector3 offset = new Vector3(0, verticalOffset, 0); // Preview position
            
            // Simple preview - actual position will be calculated at runtime
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetNPC.position + offset, 0.3f);
            Gizmos.DrawLine(targetNPC.position, targetNPC.position + offset);
        }
        
        // Draw prompt position preview in editor
        if (interactPrompt != null && Application.isPlaying && playerInRange)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(interactPrompt.transform.position, 0.2f);
        }

        #if UNITY_EDITOR
        if (dialogueContainer != null)
        {
            string label = $"Dialogue: {dialogueContainer.FileName}";
            if (!string.IsNullOrEmpty(selectedGroupName))
                label += $"\nGroup: {selectedGroupName}";
            if (!string.IsNullOrEmpty(selectedDialogueName))
                label += $"\nDialogue: {selectedDialogueName}";
            
            // Show NPC info if WorldSpace
            if (dialogueController != null && dialogueController.GetSetupMode() == DialogueSetupMode.WorldSpace)
            {
                if (npcTransform != null)
                    label += $"\nNPC: {npcTransform.name}";
                else
                    label += "\nNPC: [Using Trigger]";
                    
                label += $"\nPosition: {dialoguePosition}";
                label += $"\nOffset: H:{horizontalOffset} V:{verticalOffset}";
            }
            
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