using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Base class for all skill-related triggers
/// Handles common functionality: visual cues, prompts, emotes, player detection
/// </summary>
public abstract class BaseSkillTrigger : MonoBehaviour
{
    [Header("=== BASE TRIGGER SETTINGS ===")]
    [Space(5)]
    
    [Header("Visual Cue")]
    [SerializeField] public GameObject visualCue;

    [Header("Emote Animator")]
    [SerializeField] public Animator emoteAnimator;

    [Header("Trigger Settings")]
    [Tooltip("If TRUE, triggers immediately when player enters (ignores input requirement)")]
    [SerializeField] public bool triggerOnEnter = false;
    [Tooltip("If TRUE, requires key press to trigger. If FALSE with triggerOnEnter FALSE, does nothing!")]
    [SerializeField] public bool requiresInput = true;
    [SerializeField] public KeyCode interactKey = KeyCode.E;
    [Tooltip("If FALSE, trigger only works once. If TRUE, can be triggered multiple times.")]
    [SerializeField] public bool canTriggerMultipleTimes = true;

    [Header("UI Prompt")]
    [SerializeField] public GameObject interactPrompt;
    [SerializeField] public TextMeshProUGUI promptTextComponent;
    [SerializeField] public string promptText = "Press E to Interact";
    [SerializeField] public float fadeInDuration = 0.3f;
    [SerializeField] public float displayDuration = 2f;
    [SerializeField] public float fadeOutDuration = 0.3f;
    [SerializeField] public bool loopPrompt = true;
    
    [Header("Dynamic Prompt Positioning")]
    [Tooltip("Horizontal offset for prompt when positioned on the left")]
    [SerializeField] public float promptLeftOffset = -3.5f;
    [Tooltip("Horizontal offset for prompt when positioned on the right")]
    [SerializeField] public float promptRightOffset = 3.5f;
    [Tooltip("Additional vertical offset for the prompt")]
    [SerializeField] public float promptVerticalOffset = 0.5f;
    [Tooltip("Update prompt position every frame (enable for moving triggers)")]
    [SerializeField] public bool continuousPositionUpdate = false;

    [Header("Feedback")]
    [Tooltip("Show console logs for debugging")]
    [SerializeField] public bool showDebugLogs = true;
    [Tooltip("Play emote animation when triggered")]
    [SerializeField] public bool playEmoteOnTrigger = true;

    // Protected state
    protected bool playerInRange;
    protected bool hasTriggered;
    protected bool hasShownPromptThisEntry;
    protected Coroutine promptCoroutine;
    protected CanvasGroup promptCanvasGroup;
    protected RectTransform promptRectTransform;
    protected Canvas promptCanvas;
    protected Camera mainCamera;

    // Public properties
    public bool IsPlayerInRange => playerInRange;
    public bool HasTriggered => hasTriggered;

    protected virtual void Awake()
    {
        playerInRange = false;
        hasTriggered = false;
        hasShownPromptThisEntry = false;
        
        mainCamera = Camera.main;

        InitializeVisualCue();
        InitializePrompt();
        InitializeReferences();
    }

    protected virtual void InitializeVisualCue()
    {
        if (visualCue != null)
        {
            visualCue.SetActive(false);
        }
    }

    protected virtual void InitializePrompt()
    {
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
    }

    protected virtual void InitializeReferences()
    {
        // Override in derived classes to initialize specific references
    }

    protected virtual void Update()
    {
        HandleInput();
        HandleVisualFeedback();
    }

    protected virtual void HandleInput()
    {
        if (playerInRange && requiresInput && !triggerOnEnter)
        {
            if (Input.GetKeyDown(interactKey))
            {
                if (showDebugLogs)
                    Debug.Log($"[{GetType().Name}:{gameObject.name}] Key '{interactKey}' pressed!");
                
                if (canTriggerMultipleTimes || !hasTriggered)
                {
                    OnTriggerActivated();
                }
                else if (showDebugLogs)
                {
                    Debug.Log($"[{GetType().Name}:{gameObject.name}] Already triggered and cannot trigger multiple times");
                }
            }
        }
    }

    protected virtual void HandleVisualFeedback()
    {
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

        // Show interact prompt if conditions are met
        bool shouldShowPrompt = ShouldShowPrompt();

        if (shouldShowPrompt)
        {
            if (interactPrompt != null && !interactPrompt.activeSelf)
            {
                interactPrompt.SetActive(true);
                UpdatePromptPosition();

                if (promptCoroutine == null)
                {
                    promptCoroutine = StartCoroutine(PromptFadeLoop());
                    hasShownPromptThisEntry = true;
                }
            }
            else if (continuousPositionUpdate && interactPrompt != null && interactPrompt.activeSelf)
            {
                UpdatePromptPosition();
            }
        }
    }

    protected virtual bool ShouldShowPrompt()
    {
        return requiresInput && !triggerOnEnter && !hasShownPromptThisEntry && (canTriggerMultipleTimes || !hasTriggered);
    }

    protected void UpdatePromptPosition()
    {
        if (interactPrompt == null || promptRectTransform == null || mainCamera == null)
            return;

        Vector3 triggerWorldPos = transform.position;
        Vector3 triggerScreenPos = mainCamera.WorldToScreenPoint(triggerWorldPos);

        float screenWidth = Screen.width;
        float distToLeft = triggerScreenPos.x;
        float distToRight = screenWidth - triggerScreenPos.x;

        float horizontalOffset = distToLeft > distToRight ? promptLeftOffset : promptRightOffset;

        if (promptCanvas != null)
        {
            if (promptCanvas.renderMode == RenderMode.ScreenSpaceOverlay || promptCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                Vector3 screenPos = mainCamera.WorldToScreenPoint(triggerWorldPos);
                
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    promptCanvas.transform as RectTransform,
                    screenPos,
                    promptCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : promptCanvas.worldCamera,
                    out Vector2 localPoint
                );
                
                promptRectTransform.anchoredPosition = new Vector2(localPoint.x + horizontalOffset, localPoint.y + promptVerticalOffset);
            }
            else
            {
                Vector3 promptWorldPos = triggerWorldPos + new Vector3(horizontalOffset, promptVerticalOffset, 0f);
                promptRectTransform.position = promptWorldPos;
            }
        }
        else
        {
            Vector3 promptWorldPos = triggerWorldPos + new Vector3(horizontalOffset, promptVerticalOffset, 0f);
            promptRectTransform.position = promptWorldPos;
        }
    }

    protected void HideAllPrompts()
    {
        if (visualCue != null && visualCue.activeSelf)
        {
            visualCue.SetActive(false);
        }

        StopPrompt();
    }

    protected void StopPrompt()
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

    protected IEnumerator PromptFadeLoop()
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

    protected void PlayEmote()
    {
        if (playEmoteOnTrigger && emoteAnimator != null)
        {
            emoteAnimator.SetTrigger("Talk");
        }
    }

    /// <summary>
    /// Override this method to implement specific trigger behavior
    /// </summary>
    protected abstract void OnTriggerActivated();

    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        if (!collider.gameObject.CompareTag("Player"))
            return;

        if (showDebugLogs)
            Debug.Log($"[{GetType().Name}:{gameObject.name}] ✓ Player ENTERED trigger zone");
        
        playerInRange = true;
        hasShownPromptThisEntry = false;

        OnPlayerEnter();

        if (triggerOnEnter)
        {
            if (canTriggerMultipleTimes || !hasTriggered)
            {
                if (showDebugLogs)
                    Debug.Log($"[{GetType().Name}:{gameObject.name}] Auto-triggering on enter");
                OnTriggerActivated();
            }
            else if (showDebugLogs)
            {
                Debug.Log($"[{GetType().Name}:{gameObject.name}] Already triggered (not repeatable)");
            }
        }
        else if (showDebugLogs)
        {
            Debug.Log($"[{GetType().Name}:{gameObject.name}] Waiting for key press: {interactKey}");
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collider)
    {
        if (!collider.gameObject.CompareTag("Player"))
            return;

        if (showDebugLogs)
            Debug.Log($"[{GetType().Name}:{gameObject.name}] ✓ Player EXITED trigger zone");
        
        playerInRange = false;
        
        OnPlayerExit();

        // Reset triggered state when player leaves (if repeatable)
        if (canTriggerMultipleTimes)
        {
            hasTriggered = false;
        }
    }

    /// <summary>
    /// Called when player enters the trigger zone (before auto-trigger check)
    /// </summary>
    protected virtual void OnPlayerEnter() { }

    /// <summary>
    /// Called when player exits the trigger zone
    /// </summary>
    protected virtual void OnPlayerExit() { }

    #region Public Methods

    /// <summary>
    /// Reset the trigger to allow re-triggering
    /// </summary>
    public virtual void ResetTrigger()
    {
        hasTriggered = false;
        if (showDebugLogs)
            Debug.Log($"[{GetType().Name}:{gameObject.name}] Trigger reset");
    }

    /// <summary>
    /// Manually trigger from other scripts
    /// </summary>
    public virtual void ManualTrigger()
    {
        if (canTriggerMultipleTimes || !hasTriggered)
        {
            OnTriggerActivated();
        }
        else if (showDebugLogs)
        {
            Debug.Log($"[{GetType().Name}:{gameObject.name}] Cannot manual trigger - already used");
        }
    }

    #endregion

    #region Editor Support

    protected virtual void OnValidate()
    {
        if (Application.isPlaying) return;
        
        if (!triggerOnEnter && !requiresInput)
        {
            Debug.LogWarning($"[{GetType().Name}:{gameObject.name}] Both triggerOnEnter and requiresInput are FALSE - trigger will never activate!");
        }
        
        if (promptTextComponent != null && !string.IsNullOrEmpty(promptText))
        {
            promptTextComponent.text = promptText;
        }
    }

    protected virtual void OnDrawGizmos()
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

        DrawCustomGizmoLabels();
    }

    /// <summary>
    /// Override to add custom gizmo labels in derived classes
    /// </summary>
    protected virtual void DrawCustomGizmoLabels()
    {
        #if UNITY_EDITOR
        string label = GetType().Name;
        
        if (triggerOnEnter)
            label += "\n[AUTO TRIGGER]";
        else if (requiresInput)
            label += $"\n[Press {interactKey}]";
        
        if (!canTriggerMultipleTimes)
            label += "\n[ONE TIME USE]";

        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, label);
        #endif
    }

    #endregion
}