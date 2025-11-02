using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Visual Cue")]
    [SerializeField] private GameObject visualCue;

    [Header("Emote Animator")]
    [SerializeField] private Animator emoteAnimator;

    [Header("Dialogue Settings")]
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private Dialogue dialogueToStart;

    [Header("Trigger Settings")]
    [SerializeField] private bool triggerOnEnter = false;
    [SerializeField] private bool requiresInput = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    public bool playerInRange;
    private bool hasTriggered;

    private void Awake() 
    {
        playerInRange = false;
        hasTriggered = false;


        if (visualCue != null)
        {
            visualCue.SetActive(false);
        }

        // Try to find DialogueController if not assigned
        if (dialogueController == null)
        {
            dialogueController = FindObjectOfType<DialogueController>();
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
        if (dialogueController == null)
        {
            Debug.LogWarning("DialogueController is not assigned!");
            return;
        }

        if (dialogueToStart == null)
        {
            Debug.LogWarning("No dialogue assigned to trigger!");
            return;
        }

        // Trigger the dialogue
        dialogueController.TriggerDialogue(dialogueToStart);
        hasTriggered = true;

        // Optional: Play emote animation
        if (emoteAnimator != null)
        {
            emoteAnimator.SetTrigger("Talk"); // Adjust trigger name as needed
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
}