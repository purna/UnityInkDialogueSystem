using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.EventSystems;

public class DialogueInkManager : MonoBehaviour
{
    [Header("Load Globals JSON")]
    [SerializeField] private TextAsset loadGlobalsJSON;

    // --- AUDIO REMOVED - Now handled by DialogueUI ---

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
    private TextMeshProUGUI dialogueText; // Cached component
    private TextMeshProUGUI displayNameText; // Cached component

    private void Awake() 
    {
        if (instance != null)
        {
            Debug.LogWarning("Found more than one Dialogue Manager in the scene");
        }
        instance = this;

        dialogueInkVariables = new DialogueInkVariables(loadGlobalsJSON);
        inkExternalFunctions = new InkExternalFunctions();

        // --- AUDIO INITIALIZATION REMOVED ---
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

        // Cache components for performance
        dialogueText = dialogueUI.GetDialogueTextComponent();
        displayNameText = dialogueUI.GetSpeakerNameComponent();

        dialogueUI.HidePanel(); // Start with panel hidden
        
        // --- AUDIO DICTIONARY INITIALIZATION REMOVED ---
    }

    // --- SetCurrentAudioInfo() METHOD REMOVED ---

    private void Update() 
    {
        // Return if dialogue isn't playing
        if (!dialogueIsPlaying || dialogueUI == null)
            return;

        // Cache input - check for BOTH Space and Return
        bool canProgress = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);

        // Only progress if we can continue AND there are no choices
        if (canContinueToNextLine && currentStory.currentChoices.Count == 0 && canProgress)
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

        dialogueInkVariables.StopListening(currentStory);
        inkExternalFunctions.Unbind(currentStory);

        dialogueIsPlaying = false;
        
        if (dialogueUI != null)
        {
            dialogueText.text = "";
            dialogueUI.ClearInkChoices();
            dialogueUI.ResetAudioToDefault(); // --- ADDED: Reset audio ---
        }
    }

    private void ContinueStory() 
    {
        if (currentStory.canContinue) 
        {
            if (displayLineCoroutine != null) 
            {
                StopCoroutine(displayLineCoroutine);
            }
            
            string nextLine = currentStory.Continue();
            
            // handle case where the last line is an external function
            if (nextLine.Equals("") && !currentStory.canContinue)
            {
                StartCoroutine(ExitDialogueMode());
            }
            else 
            {
                HandleTags(currentStory.currentTags);
                displayLineCoroutine = StartCoroutine(DisplayLine(nextLine));
            }
        }
        else 
        {
            StartCoroutine(ExitDialogueMode());
        }
    }

    private IEnumerator DisplayLine(string line) 
    {
        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0;
        
        dialogueUI.HideContinueIcon();
        dialogueUI.ClearInkChoices();

        canContinueToNextLine = false;

        bool isAddingRichTextTag = false;
        
        // Get shared typing speed from DialogueUI
        float currentTypingSpeed = (dialogueUI != null) ? dialogueUI.GetTypingSpeed() : 0.04f;

        // display each letter one at a time
        foreach (char letter in line.ToCharArray())
        {
            // Check for input to skip typing
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                dialogueText.maxVisibleCharacters = line.Length;
                break;
            }

            // check for rich text tag
            if (letter == '<' || isAddingRichTextTag) 
            {
                isAddingRichTextTag = true;
                if (letter == '>')
                {
                    isAddingRichTextTag = false;
                }
            }
            // if not rich text, add the next letter and wait a small time
            else
            {
                // --- UPDATED: Use DialogueUI's audio system ---
                dialogueUI.PlayTypingSound(dialogueText.maxVisibleCharacters, dialogueText.text[dialogueText.maxVisibleCharacters]);
                
                dialogueText.maxVisibleCharacters++;
                yield return new WaitForSeconds(currentTypingSpeed);
            }
        }

        // actions to take after the entire line has finished displaying
        dialogueUI.ShowContinueIcon();
        dialogueUI.DisplayInkChoices(currentStory.currentChoices, MakeChoice);

        canContinueToNextLine = true;
    }

    // --- PlayDialogueSound() METHOD REMOVED - Now in DialogueUI ---

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
                    // --- UPDATED: Use DialogueUI's audio system ---
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
            InputManager.GetInstance().RegisterSubmitPressed();
            ContinueStory();
        }
    }

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
        dialogueInkVariables.SaveVariables();
    }
}