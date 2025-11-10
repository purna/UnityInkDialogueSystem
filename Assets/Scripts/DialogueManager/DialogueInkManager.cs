using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.EventSystems;

public class DialogueInkManager : MonoBehaviour
{
    // --- REMOVED: [SerializeField] private TextAsset loadGlobalsJSON; ---
    // Now using DialogueVariableManager.Instance for global variables

    private Story currentStory;
    public bool dialogueIsPlaying { get; private set; }

    private bool canContinueToNextLine = false;

    private Coroutine displayLineCoroutine;

    private static DialogueInkManager instance;

    private const string SPEAKER_TAG = "speaker";
    private const string PORTRAIT_TAG = "portrait";
    private const string LAYOUT_TAG = "layout";
    private const string AUDIO_TAG = "audio";

    private DialogueInkVariables dialogueInkVariables;
    private InkExternalFunctions inkExternalFunctions;
    private DialogueUI dialogueUI; // Reference to the one true UI

    private TypewriterEffect typewriterEffect;

    private TextMeshProUGUI dialogueText; // Cached component
    private TextMeshProUGUI displayNameText; // Cached component

    private bool canProgress = false;

    private bool inputSubmitDetected = false;
    private bool inputInteractDetected = false;
    private bool inputMouseDetected = false;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Found more than one Dialogue Manager in the scene");
        }
        instance = this;

        // UPDATED: No longer passing loadGlobalsJSON
        // DialogueInkVariables will sync with DialogueVariableManager instead
        dialogueInkVariables = new DialogueInkVariables();
        inkExternalFunctions = new InkExternalFunctions();
    }

    public static DialogueInkManager GetInstance() 
    {
        return instance;
    }

    private void Start() 
    {
        dialogueIsPlaying = false;

        // Find the DialogueUI to get the shared typing speed
        dialogueUI = FindObjectOfType<DialogueUI>();
        if (dialogueUI == null)
        {
            Debug.LogError("DialogueInkManager could not find DialogueUI! Ink dialogue will not work.");
            return;
        }
        
        typewriterEffect = dialogueUI.GetComponent<TypewriterEffect>();
        if (typewriterEffect == null)
        {
            typewriterEffect = dialogueUI.gameObject.AddComponent<TypewriterEffect>();
            typewriterEffect.Initialize(dialogueText);
        }

        // Cache components for performance
        dialogueText = dialogueUI.GetDialogueTextComponent();
        displayNameText = dialogueUI.GetSpeakerNameComponent();

        dialogueUI.HidePanel(); // Start with panel hidden
    }

    private void Update()
    {
        // ONLY handle input if Ink dialogue is actually playing
        if (!dialogueIsPlaying || dialogueUI == null)
            return;

        // Get input state
        inputSubmitDetected = InputManager.GetInstance().GetSubmitPressed();
        inputInteractDetected = InputManager.GetInstance().GetInteractPressed();
        inputMouseDetected = Input.GetMouseButtonDown(0);

        canProgress = Input.GetKeyDown(KeyCode.Space) || 
                      Input.GetKeyDown(KeyCode.Return) || 
                      inputMouseDetected || 
                      inputSubmitDetected || 
                      inputInteractDetected;

        // Skip typing if in progress
        if (typewriterEffect != null && typewriterEffect.IsTyping && canProgress)
        {
            typewriterEffect.Skip();
            return;
        }

        // CRITICAL: Don't progress if DialogueUI is handling choices
        // The UI manages choice selection for both systems
        if (currentStory.currentChoices.Count > 0)
        {
            // Choices are displayed - let DialogueUI handle input
            return;
        }

        // Only progress if we can continue and there are no choices
        if (canContinueToNextLine && canProgress)
        {
            ContinueStory();
        }
    }

    public void EnterDialogueMode(TextAsset inkJSON, Animator emoteAnimator) 
    {
        EnterDialogueMode(inkJSON, emoteAnimator, null, true);
    }
    
    /// <summary>
    /// Enters dialogue mode with a specific knot and starting configuration.
    /// Used by the graph-based DialogueManager.
    /// </summary>
    public void EnterDialogueMode(TextAsset inkJSON, Animator emoteAnimator, string knotName, bool startFromBeginning) 
    {
        if (dialogueUI == null)
        {
            Debug.LogError("DialogueUI not found. Cannot enter Ink dialogue.");
            return;
        }

        currentStory = new Story(inkJSON.text);
        dialogueIsPlaying = true;
        dialogueUI.ShowPanel();

        // UPDATED: Sync variables from DialogueVariableManager to this Ink story
        if (DialogueVariableManager.Instance != null)
        {
            DialogueVariableManager.Instance.SyncToInkStory(currentStory);
        }

        dialogueInkVariables.StartListening(currentStory);
        inkExternalFunctions.Bind(currentStory, emoteAnimator);

        // reset portrait, layout, and speaker
        displayNameText.text = "???";
        dialogueUI.PlayPortraitAnimation("default");
        dialogueUI.PlayLayoutAnimation("right");

        // Jump to knot if specified
        if (!startFromBeginning && !string.IsNullOrEmpty(knotName))
        {
            try
            {
                Debug.Log($"[DialogueInkManager] Attempting to jump to knot: {knotName}");
                currentStory.ChoosePathString(knotName);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[DialogueInkManager] Could not jump to knot '{knotName}'. Starting from beginning. Error: {e.Message}");
            }
        }

        ContinueStory();
    }

    private IEnumerator ExitDialogueMode() 
    {
        yield return new WaitForSeconds(0.2f);

        // UPDATED: Sync variables back to DialogueVariableManager before exiting
        if (DialogueVariableManager.Instance != null && currentStory != null)
        {
            DialogueVariableManager.Instance.SyncFromInkStory(currentStory);
        }

        dialogueInkVariables.StopListening(currentStory);
        inkExternalFunctions.Unbind(currentStory);

        dialogueIsPlaying = false;
        
        if (dialogueUI != null)
        {
            dialogueText.text = "";
            dialogueUI.ClearInkChoices();
            dialogueUI.ResetAudioToDefault();
        }
    }

    private void ContinueStory() 
    {
        if (currentStory.canContinue) 
        {
            string nextLine = currentStory.Continue();
            
            if (nextLine.Equals("") && !currentStory.canContinue)
            {
                StartCoroutine(ExitDialogueMode());
            }
            else
            {
                HandleTags(currentStory.currentTags);
                DisplayLine(nextLine);
            }
        }
        else 
        {
            StartCoroutine(ExitDialogueMode());
        }
    }

    private void DisplayLine(string line)
    {
        dialogueUI.HideContinueIcon();
        dialogueUI.ClearInkChoices();
        canContinueToNextLine = false;

        if (typewriterEffect != null)
        {
            typewriterEffect.TypeText(
                line,
                onCharTyped: (index, character) => dialogueUI.PlayTypingSound(index, character),
                onComplete: () => OnTypingComplete()
            );
        }
        else
        {
            dialogueText.text = line;
            OnTypingComplete();
        }
    }

    private void OnTypingComplete()
    {
        // CRITICAL: Only show continue icon if there are no choices
        if (currentStory.currentChoices.Count == 0)
        {
            dialogueUI.ShowContinueIcon();
        }
        else
        {
            dialogueUI.HideContinueIcon();
        }
        dialogueUI.DisplayInkChoices(currentStory.currentChoices, MakeChoice);
        canContinueToNextLine = true;
    }

    private void HandleTags(List<string> currentTags)
    {
        if (dialogueUI == null) return;

        // loop through each tag and handle it accordingly
        foreach (string tag in currentTags)
        {
            string[] splitTag = tag.Split(':');

            if (splitTag.Length != 2)
            {
                Debug.LogError("Tag could not be appropriately parsed: " + tag);
                continue;
            }
            
            string tagKey = splitTag[0].Trim();
            string tagValue = splitTag[1].Trim();

            // handle the tag
            switch (tagKey)
            {
                case SPEAKER_TAG:
                    displayNameText.text = tagValue;
                    break;
                case PORTRAIT_TAG:
                    dialogueUI.PlayPortraitAnimation(tagValue);
                    break;
                case LAYOUT_TAG:
                    dialogueUI.PlayLayoutAnimation(tagValue);
                    break;
                case AUDIO_TAG:
                    dialogueUI.SetCurrentAudioInfo(tagValue);
                    break;
                default:
                    Debug.LogWarning("Tag came in but is not currently being handled: " + tag);
                    break;
            }
        }
    }

    public void MakeChoice(int choiceIndex)
    {
        if (canContinueToNextLine)
        {
            currentStory.ChooseChoiceIndex(choiceIndex);

            if (canProgress)
            {
                ContinueStory();
            }
        }
    }
    
    /*
    public void MakeChoice(int choiceIndex)
    {
        // CRITICAL FIX: Don't check canProgress here
        // This method is called directly from button onClick events
        if (!canContinueToNextLine) 
        {
            Debug.LogWarning("[DialogueInkManager] MakeChoice called but cannot continue to next line");
            return;
        }

        if (currentStory == null)
        {
            Debug.LogError("[DialogueInkManager] MakeChoice called but currentStory is null");
            return;
        }

        Debug.Log($"[DialogueInkManager] Making choice {choiceIndex} out of {currentStory.currentChoices.Count} choices");
        
        try
        {
            currentStory.ChooseChoiceIndex(choiceIndex);
            ContinueStory();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DialogueInkManager] Error making choice {choiceIndex}: {e.Message}");
        }
    }

    */

    public Ink.Runtime.Object GetVariableState(string variableName) 
    {
        Ink.Runtime.Object variableValue = null;
        dialogueInkVariables.variables.TryGetValue(variableName, out variableValue);
        if (variableValue == null) 
        {
            Debug.LogWarning("Ink Variable was found to be null: " + variableName);
        }
        return variableValue;
    }

    public void OnApplicationQuit() 
    {
        // UPDATED: DialogueVariableManager handles saving now
        // dialogueInkVariables.SaveVariables(); // REMOVED
        
        if (DialogueVariableManager.Instance != null && currentStory != null)
        {
            DialogueVariableManager.Instance.SyncFromInkStory(currentStory);
        }
    }
}