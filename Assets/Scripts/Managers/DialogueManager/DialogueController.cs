using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Main controller for the dialogue system 
/// Supports both ScreenSpace and WorldSpace dialogue via unified DialogueUI
/// </summary>
public class DialogueController : MonoBehaviour
{
    [Header("Setup Mode")]
    [SerializeField] private DialogueSetupMode _setupMode = DialogueSetupMode.ScreenSpace;
    
    [Header("Dialogue Data")]
    [SerializeField] private DialogueContainer dialogueContainer;
    [SerializeField] private DialogueGroup dialogueGroup;
    [SerializeField] private Dialogue dialogue;

    private bool _isDialogueOpen = false;
    public bool IsDialogueOpen => _isDialogueOpen;

    [Header("Emote Animator")]
    [Tooltip("Animator used for emote animations during dialogue (especially for Ink nodes)")]
    [SerializeField] private Animator emoteAnimator;

    // Events for external systems (like DialoguePlayerController)
    public event Action OnDialogueStarted;
    public event Action OnDialogueEnded;

    [Header("Dialogue Selection")]
    [SerializeField] private bool groupedDialogues;
    [SerializeField] private bool startingDialoguesOnly;
    [SerializeField] private int selectedDialogueGroupIndex;
    [SerializeField] private int selectedDialogueIndex;

    [Header("System References")]
    [SerializeField] private DialogueManager dialogueManager;
    
    [Header("Unified UI Reference")]
    [Tooltip("Single DialogueUI that manages both ScreenSpace and WorldSpace canvases")]
    [SerializeField] private DialogueUI dialogueUI;
    
    [Header("Typewriter Reference")]
    [Tooltip("TypewriterEffect component")]
    [SerializeField] private TypewriterEffect typewriter;

    [Header("Auto Start Settings")]
    [SerializeField] private bool initializeOnStart = false;
    [SerializeField] private float startDelay = 0f;

    [Header("Close Button Settings")]
    [SerializeField] private bool _allowCloseWithKey = true;
    [SerializeField] private KeyCode _closeKey = KeyCode.Escape;
   
    [Header("Player Control")]
    [SerializeField] private GameObject _playerObject;
    [SerializeField] private bool _disablePlayerWhenOpen = true;
    
    private IPlayerController _playerController;

    // Events for tutorial/sequential systems
    public event Action DialoguePartEnded;

    // Public property for DialogueManager to access
    public Dialogue Dialogue => dialogue;

    private void Awake() 
    {
        // Subscribe to dialogue manager events
        if (dialogueManager != null)
        {
            dialogueManager.DialogueEnded += HandleDialogueManagerEnded;
        }
    }

    private void Start()
    {
        // Try to find dialogue manager if not assigned
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
            if (dialogueManager != null)
            {
                dialogueManager.DialogueEnded += HandleDialogueManagerEnded;
            }
        }

        // Try to find unified DialogueUI if not assigned
        if (dialogueUI == null)
        {
            dialogueUI = FindObjectOfType<DialogueUI>();
            if (dialogueUI == null)
            {
                Debug.LogWarning("[DialogueController] No DialogueUI found in scene!");
            }
        }
        
        // Try to find typewriter if not assigned
        if (typewriter == null && dialogueUI != null)
        {
            typewriter = dialogueUI.GetComponent<TypewriterEffect>();
            if (typewriter == null)
            {
                Debug.LogWarning("[DialogueController] DialogueUI has no TypewriterEffect component!");
            }
        }

        // Find player controller
        if (_playerObject != null && _disablePlayerWhenOpen)
        {
            _playerController = _playerObject.GetComponent<IPlayerController>();
            if (_playerController == null)
            {
                Debug.LogWarning("[DialogueController] Player object doesn't implement IPlayerController interface!");
            }
        }
        else if (_disablePlayerWhenOpen)
        {
            // Try to find player by tag
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerObject = player;
                _playerController = player.GetComponent<IPlayerController>();
                if (_playerController == null)
                {
                    Debug.LogWarning("[DialogueController] Player doesn't implement IPlayerController interface!");
                }
            }
        }

        // FIXED: Only initialize if initializeOnStart is TRUE
        if (initializeOnStart)
        {
            if (startDelay > 0)
            {
                Invoke(nameof(AutoStartDialogue), startDelay);
            }
            else
            {
                AutoStartDialogue();
            }
        }
        else
        {
            // IMPORTANT: Hide the UI if we're not auto-starting
            if (dialogueUI != null)
                dialogueUI.gameObject.SetActive(false);
                
            Debug.Log("[DialogueController] DialogueUI initialized but hidden. Waiting for trigger.");
        }
    }

    private void Update()
    {
        // Handle close key input when dialogue UI is open
        if (_isDialogueOpen && _allowCloseWithKey)
        {
            if (Input.GetKeyDown(_closeKey))
            {
                HideDialogue();
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (dialogueManager != null)
        {
            dialogueManager.DialogueEnded -= HandleDialogueManagerEnded;
        }
    }

    private void AutoStartDialogue()
    {
        if (dialogueContainer != null)
        {
            StartDialogueFromContainer();
        }
        else if (dialogue != null)
        {
            StartDialogue();
        }
        else
        {
            Debug.LogWarning("Cannot auto-start dialogue: No dialogue or container assigned!");
        }
    }

    #region Public Dialogue Methods

    /// <summary>
    /// Shows the dialogue UI without starting a specific dialogue
    /// Used by DialogueTrigger to open the UI first
    /// </summary>
    public void ShowDialogue()
    {
        ShowDialogue(null);
    }
    
    /// <summary>
    /// Shows the dialogue UI and optionally attaches to NPC
    /// </summary>
    public void ShowDialogue(Transform npcTransform)
    {
        Debug.Log($"[DialogueController] ShowDialogue() called - Mode: {_setupMode}");
        
        if (dialogueUI != null)
        {
            // Configure the DialogueUI for the current mode and NPC
            dialogueUI.SetupMode(_setupMode, npcTransform);
            dialogueUI.gameObject.SetActive(true);
            _isDialogueOpen = true;
            Debug.Log($"[DialogueController] ✓ {_setupMode} dialogue UI activated");
        }
        else
        {
            Debug.LogWarning($"[DialogueController] DialogueUI is not assigned!");
        }

        // Disable player movement
        if (_disablePlayerWhenOpen && _playerController != null)
        {
            _playerController.DisablePlayer();
            Debug.Log("[DialogueController] Player movement disabled");
        }
    }

    /// <summary>
    /// Hides the dialogue UI
    /// </summary>
    public void HideDialogue()
    {
        if (dialogueUI != null)
        {
            dialogueUI.gameObject.SetActive(false);
            _isDialogueOpen = false;
            Debug.Log($"[DialogueController] {_setupMode} Dialogue UI hidden");
        }

        // Re-enable player movement
        if (_disablePlayerWhenOpen && _playerController != null)
        {
            _playerController.EnablePlayer();
            Debug.Log("[DialogueController] Player movement enabled");
        }
    }

    /// <summary>
    /// Toggles dialogue UI visibility
    /// </summary>
    public void ToggleDialogue()
    {
        if (dialogueUI != null)
        {
            if (dialogueUI.gameObject.activeSelf)
                HideDialogue();
            else
                ShowDialogue();
        }
    }

    /// <summary>
    /// Changes the setup mode at runtime
    /// </summary>
    public void SetSetupMode(DialogueSetupMode mode)
    {
        bool wasOpen = _isDialogueOpen;
        
        // Hide current UI
        if (wasOpen)
            HideDialogue();
        
        // Switch mode
        _setupMode = mode;
        
        Debug.Log($"[DialogueController] Setup mode changed to: {_setupMode}");
        
        // Show new UI if dialogue was open
        if (wasOpen)
            ShowDialogue();
    }

    /// <summary>
    /// Gets the currently active DialogueUI
    /// </summary>
    public DialogueUI GetActiveDialogueUI()
    {
        return dialogueUI;
    }
    
    /// <summary>
    /// Gets the currently active TypewriterEffect
    /// </summary>
    public TypewriterEffect GetActiveTypewriter()
    {
        return typewriter;
    }

    /// <summary>
    /// Starts the dialogue assigned in the inspector
    /// Can be called from Unity Events or code
    /// </summary>
    public void StartDialogue()
    {
        if (dialogue == null)
        {
            Debug.LogWarning("No dialogue assigned!");
            return;
        }

        StartDialogueInternal(dialogue, emoteAnimator, null);
    }

    /// <summary>
    /// Starts dialogue with a custom animator
    /// </summary>
    public void StartDialogue(Animator customEmoteAnimator)
    {
        if (dialogue == null)
        {
            Debug.LogWarning("No dialogue assigned!");
            return;
        }

        StartDialogueInternal(dialogue, customEmoteAnimator, null);
    }

    /// <summary>
    /// Starts a dialogue part (compatible with tutorial/sequential systems)
    /// </summary>
    public void StartDialoguePart()
    {
        StartDialogue();
    }

    /// <summary>
    /// Starts dialogue from the assigned container
    /// </summary>
    public void StartDialogueFromContainer()
    {
        if (dialogueContainer == null)
        {
            Debug.LogWarning("No dialogue container assigned!");
            return;
        }

        Dialogue startDialogue = GetStartingDialogue();
        if (startDialogue != null)
        {
            // Update the dialogue reference for DialogueManager
            dialogue = startDialogue;
            StartDialogueInternal(startDialogue, emoteAnimator, null);
        }
        else
        {
            Debug.LogWarning("No starting dialogue found in container!");
        }
    }

    /// <summary>
    /// Triggers a specific dialogue at runtime
    /// This is called by DialogueTrigger
    /// </summary>
    public void TriggerDialogue(Dialogue dialogueToStart)
    {
        TriggerDialogue(dialogueToStart, emoteAnimator, null);
    }

    /// <summary>
    /// Triggers a specific dialogue with custom animator at runtime
    /// </summary>
    public void TriggerDialogue(Dialogue dialogueToStart, Animator customEmoteAnimator)
    {
        TriggerDialogue(dialogueToStart, customEmoteAnimator, null);
    }
    
    /// <summary>
    /// Triggers a specific dialogue with optional NPC transform for WorldSpace attachment
    /// </summary>
    public void TriggerDialogue(Dialogue dialogueToStart, Animator customEmoteAnimator, Transform npcTransform)
    {
        if (dialogueToStart == null)
        {
            Debug.LogWarning("Cannot trigger null dialogue!");
            return;
        }

        // Update the dialogue reference
        dialogue = dialogueToStart;
        StartDialogueInternal(dialogueToStart, customEmoteAnimator, npcTransform);
    }

    /// <summary>
    /// Ends the current dialogue
    /// </summary>
    public void EndDialogue()
    {
        // Invoke the event to notify subscribers (like DialoguePlayerController)
        OnDialogueEnded?.Invoke();
        Debug.Log("<color=orange>[DialogueController]</color> OnDialogueEnded event invoked!");
 
        if (dialogueManager != null)
        {
            dialogueManager.EndDialogue();
        }
        else if (dialogueUI != null)
        {
            dialogueUI.EndDialogue();
        }

        // Hide the UI when dialogue ends
        HideDialogue();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Gets the starting dialogue from the container based on settings
    /// </summary>
    private Dialogue GetStartingDialogue()
    {
        if (dialogueContainer == null)
            return null;

        List<string> dialogueNames;
        
        if (groupedDialogues && dialogueGroup != null)
        {
            // Get dialogues from the specified group
            dialogueNames = dialogueContainer.GetGroupedDialoguesNames(dialogueGroup, startingDialoguesOnly);
        }
        else
        {
            // Get ungrouped dialogues
            dialogueNames = dialogueContainer.GetUngroupedDialoguesNames(startingDialoguesOnly);
        }

        if (dialogueNames.Count == 0)
        {
            Debug.LogWarning("No dialogues found with current filter settings!");
            return null;
        }

        // Return the dialogue at the selected index, or first one if index is invalid
        int index = selectedDialogueIndex < dialogueNames.Count ? selectedDialogueIndex : 0;
        
        // Get dialogue by name from container
        string dialogueName = dialogueNames[index];
        return dialogueContainer.GetDialogueByName(dialogueName);
    }

    /// <summary>
    /// Internal method to start dialogue with animator support and NPC transform
    /// </summary>
    private void StartDialogueInternal(Dialogue dialogueToStart, Animator customEmoteAnimator, Transform npcTransform)
    {
        if (dialogueToStart == null)
        {
            Debug.LogWarning("Dialogue to start is null!");
            return;
        }

        // Update the current dialogue reference
        dialogue = dialogueToStart;

        // Show the UI if not already shown (with NPC attachment if WorldSpace)
        if (!_isDialogueOpen)
        {
            ShowDialogue(npcTransform);
        }
        else
        {
            // UI is already open, but we might need to reconfigure it for WorldSpace
            if (_setupMode == DialogueSetupMode.WorldSpace && dialogueUI != null)
            {
                dialogueUI.SetupMode(_setupMode, npcTransform);
            }
        }

        // Invoke the event BEFORE starting dialogue (so player gets disabled first)
        OnDialogueStarted?.Invoke();
        Debug.Log("<color=orange>[DialogueController]</color> OnDialogueStarted event invoked!");

        // Get the UI reference
        if (dialogueUI != null)
        {
            // Update DialogueManager's UI reference
            if (dialogueManager != null)
            {
                dialogueManager.SetDialogueUI(dialogueUI);
                dialogueManager.StartDialogue(this, customEmoteAnimator);
            }
            // Fallback: Use DialogueUI directly (no animator support in this path)
            else
            {
                Debug.LogWarning("[DialogueController] Starting via DialogueUI fallback - Ink nodes may not work correctly!");
                dialogueUI.ShowDialogue(dialogueToStart);
            }
        }
        else
        {
            Debug.LogWarning($"[DialogueController] No DialogueUI assigned!");
        }
    }

    /// <summary>
    /// Called when dialogue ends (from DialogueManager event)
    /// This is the EVENT HANDLER, not the event itself
    /// </summary>
    private void HandleDialogueManagerEnded()
    {
        Debug.Log("<color=orange>[DialogueController]</color> DialogueManager ended event received");
        
        // Invoke DialoguePartEnded for any tutorial/sequential systems
        DialoguePartEnded?.Invoke();
        
        // Also invoke OnDialogueEnded for external systems
        OnDialogueEnded?.Invoke();

        // Hide dialogue UI when dialogue ends
        HideDialogue();
    }

    #endregion

    #region Public Utility Methods

    /// <summary>
    /// Checks if dialogue is currently active
    /// </summary>
    public bool IsDialogueActive()
    {
        if (dialogueManager != null)
        {
            return dialogueManager.IsDialogueActive;
        }
        else if (dialogueUI != null)
        {
            return dialogueUI.IsDialogueActive();
        }
        return false;
    }

    /// <summary>
    /// Set a custom emote animator at runtime
    /// </summary>
    public void SetEmoteAnimator(Animator animator)
    {
        emoteAnimator = animator;
    }

    /// <summary>
    /// Get the current emote animator
    /// </summary>
    public Animator GetEmoteAnimator() => emoteAnimator;

    /// <summary>
    /// Get the current setup mode
    /// </summary>
    public DialogueSetupMode GetSetupMode() => _setupMode;

    // Public getters for inspector/editor use
    public DialogueContainer GetDialogueContainer() => dialogueContainer;
    public Dialogue GetDialogue() => dialogue;
    public DialogueGroup GetDialogueGroup() => dialogueGroup;
    public bool IsGroupedDialogues() => groupedDialogues;
    public bool IsStartingDialoguesOnly() => startingDialoguesOnly;

    #endregion
}

public enum DialogueSetupMode
{
    ScreenSpace,
    WorldSpace
}