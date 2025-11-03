using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Visual Cue")]
    [SerializeField] private GameObject visualCue;

    [Header("Emote Animator")]
    [SerializeField] private Animator emoteAnimator;

    [Header("Dialogue Settings")]
    [SerializeField] private DialogueContainer dialogueContainer;
    [SerializeField] private string selectedGroupName;
    [SerializeField] private string selectedDialogueName;

    [Header("Trigger Settings")]
    [SerializeField] private bool triggerOnEnter = false;
    [SerializeField] private bool requiresInput = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    public bool playerInRange;
    private bool hasTriggered;
    private Dialogue cachedDialogue;

    private void Awake() 
    {
        playerInRange = false;
        hasTriggered = false;

        if (visualCue != null)
        {
            visualCue.SetActive(false);
        }

        // Cache the dialogue reference
        CacheDialogue();
    }

    private void CacheDialogue()
    {
        if (dialogueContainer == null)
        {
            Debug.LogWarning("DialogueContainer is not assigned!");
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
            Debug.LogWarning($"Could not find dialogue: {selectedDialogueName} in group: {selectedGroupName}");
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

            // Check for interaction input
            if (requiresInput)
            {
                if (Input.GetKeyDown(interactKey) && !hasTriggered)
                {
                    TriggerDialogue();
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
        }
    }

    private void TriggerDialogue()
    {
        if (cachedDialogue == null)
        {
            Debug.LogWarning("No dialogue cached or assigned!");
            return;
        }

        // Find DialogueController in the scene
        DialogueController dialogueController = FindObjectOfType<DialogueController>();
        if (dialogueController == null)
        {
            Debug.LogWarning("DialogueController not found in scene!");
            return;
        }

        // Trigger the dialogue
        dialogueController.TriggerDialogue(cachedDialogue);
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
    }

    private void OnTriggerEnter2D(Collider2D collider) 
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            playerInRange = true;

            // Auto-trigger on enter if enabled
            if (triggerOnEnter && !hasTriggered)
            {
                TriggerDialogue();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider) 
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            playerInRange = false;
            
            // Reset triggered state when player leaves
            hasTriggered = false;
        }
    }

    // Public method to reset the trigger (useful for repeatable dialogues)
    public void ResetTrigger()
    {
        hasTriggered = false;
    }

    // Public method to manually trigger dialogue from other scripts
    public void ManualTrigger()
    {
        TriggerDialogue();
    }

    // Validation in editor
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        CacheDialogue();
    }
}