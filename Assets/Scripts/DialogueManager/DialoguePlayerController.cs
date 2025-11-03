using UnityEngine;

/// <summary>
/// Manages player state during dialogue from GameManager.
/// Attach this to your GameManager or as a standalone manager.
/// </summary>
public class DialoguePlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private GameObject player;
    
    [Header("What to Disable")]
    [Tooltip("Name of the player movement script (e.g., 'PlayerController', 'PlayerMovement')")]
    [SerializeField] private string movementScriptName = "PlayerController";
    
    [Tooltip("Also disable rigidbody velocity during dialogue")]
    [SerializeField] private bool stopVelocity = true;
    
    [Tooltip("Freeze rigidbody completely during dialogue")]
    [SerializeField] private bool freezeRigidbody = false;
    
    [Header("Additional Scripts to Disable")]
    [Tooltip("Names of additional scripts to disable (e.g., 'PlayerCombat', 'PlayerInteract')")]
    [SerializeField] private string[] additionalScriptNames;

    private MonoBehaviour movementScript;
    private MonoBehaviour[] additionalScripts;
    private Rigidbody2D rb2D;
    private RigidbodyConstraints2D originalConstraints;
    
    private bool isPlayerDisabled = false;

    private void Awake()
    {
        // Try to find DialogueManager if not assigned
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
            if (dialogueManager == null)
            {
                Debug.LogError("[DialoguePlayerController] DialogueManager not found!");
                return;
            }
        }

        // Try to find player if not assigned
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("[DialoguePlayerController] Player not found! Make sure player has 'Player' tag or assign manually.");
                return;
            }
        }

        InitializePlayerComponents();
    }

    private void InitializePlayerComponents()
    {
        if (player == null) return;

        // Find movement script by name
        if (!string.IsNullOrEmpty(movementScriptName))
        {
            movementScript = player.GetComponent(movementScriptName) as MonoBehaviour;
            if (movementScript == null)
            {
                Debug.LogWarning($"[DialoguePlayerController] Movement script '{movementScriptName}' not found on player!");
            }
        }

        // Find additional scripts
        if (additionalScriptNames != null && additionalScriptNames.Length > 0)
        {
            additionalScripts = new MonoBehaviour[additionalScriptNames.Length];
            for (int i = 0; i < additionalScriptNames.Length; i++)
            {
                if (!string.IsNullOrEmpty(additionalScriptNames[i]))
                {
                    additionalScripts[i] = player.GetComponent(additionalScriptNames[i]) as MonoBehaviour;
                    if (additionalScripts[i] == null)
                    {
                        Debug.LogWarning($"[DialoguePlayerController] Script '{additionalScriptNames[i]}' not found on player!");
                    }
                }
            }
        }

        // Get Rigidbody2D
        rb2D = player.GetComponent<Rigidbody2D>();
        if (rb2D != null)
        {
            originalConstraints = rb2D.constraints;
        }
    }

    private void OnEnable()
    {
        if (dialogueManager != null)
        {
            dialogueManager.DialogueStarted += OnDialogueStarted;
            dialogueManager.DialogueEnded += OnDialogueEnded;
        }
    }

    private void OnDisable()
    {
        if (dialogueManager != null)
        {
            dialogueManager.DialogueStarted -= OnDialogueStarted;
            dialogueManager.DialogueEnded -= OnDialogueEnded;
        }
    }

    private void OnDialogueStarted()
    {
        DisablePlayer();
    }

    private void DisablePlayer()
    {
        if (player == null)
        {
            Debug.LogWarning("[DialoguePlayerController] Cannot disable player - reference is null!");
            return;
        }

        isPlayerDisabled = true;
        Debug.Log("<color=cyan>[DialoguePlayerController]</color> Disabling player controls");

        // Disable movement script
        if (movementScript != null && movementScript.enabled)
        {
            movementScript.enabled = false;
        }

        // Stop velocity
        if (rb2D != null && stopVelocity)
        {
            rb2D.velocity = Vector2.zero;
            rb2D.angularVelocity = 0f;
        }

        // Freeze rigidbody
        if (rb2D != null && freezeRigidbody)
        {
            rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        // Disable additional scripts
        if (additionalScripts != null)
        {
            foreach (var script in additionalScripts)
            {
                if (script != null && script.enabled)
                {
                    script.enabled = false;
                }
            }
        }
    }

    private void EnablePlayer()
    {
        if (player == null)
        {
            Debug.LogWarning("[DialoguePlayerController] Cannot enable player - reference is null!");
            return;
        }

        isPlayerDisabled = false;
        Debug.Log("<color=green>[DialoguePlayerController]</color> Enabling player controls");

        // Re-enable movement script
        if (movementScript != null && !movementScript.enabled)
        {
            movementScript.enabled = true;
        }

        // Restore rigidbody constraints
        if (rb2D != null && freezeRigidbody)
        {
            rb2D.constraints = originalConstraints;
        }

        // Re-enable additional scripts
        if (additionalScripts != null)
        {
            foreach (var script in additionalScripts)
            {
                if (script != null && !script.enabled)
                {
                    script.enabled = true;
                }
            }
        }
    }

    private void OnDialogueEnded()
    {
        // Ensure player is enabled when dialogue ends
        if (isPlayerDisabled)
        {
            EnablePlayer();
        }
    }

    // Public methods for manual control if needed
    public void ForceDisablePlayer()
    {
        DisablePlayer();
    }

    public void ForceEnablePlayer()
    {
        EnablePlayer();
    }

    // Helper method to change player reference at runtime
    public void SetPlayer(GameObject newPlayer)
    {
        player = newPlayer;
        InitializePlayerComponents();
    }
}