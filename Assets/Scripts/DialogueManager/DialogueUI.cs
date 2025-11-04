// ============================================
// COMPLETE UPDATED DialogueUI.cs
// ============================================

using System; 
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    /* UI References */
    [Header("UI Components")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private Image characterIcon;
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private Button choiceButtonPrefab;
    [SerializeField] private Button continueButton;

    [Header("Typewriter Effect")]
    [SerializeField] private TypewriterEffect typewriterEffect;
    //[SerializeField] private float typingSpeed = 0.04f;
    
    [Header("Audio")]
    [SerializeField] private DialogueAudioInfoSO defaultAudioInfo;
    [SerializeField] private DialogueAudioInfoSO[] audioInfos;
    [SerializeField] private bool makePredictable;
    private DialogueAudioInfoSO currentAudioInfo;
    private Dictionary<string, DialogueAudioInfoSO> audioInfoDictionary;
    private AudioSource audioSource;
    
    [Header("Ink Animators")]
    [SerializeField] private Animator portraitAnimator;
    [SerializeField] private Animator layoutAnimator;

    [Header("Dialogue System")]
    [SerializeField] private DialogueManager dialogueManager;
    
    [Header("Input Settings")]
    [SerializeField] private bool useReturnKey = true;
    [SerializeField] private bool useMouseClick = true;
    [SerializeField] private bool useSpaceKey = true;

    private Dialogue currentDialogue;
    private List<Button> activeChoiceButtons = new List<Button>();
    private bool isWaitingForChoice = false;
    private Coroutine displayLineCoroutine;
    private List<Button> activeInkChoiceButtons = new List<Button>();
    private bool inputConsumedThisFrame = false;
    private bool canProgress = false;
    private bool inputSubmitDetected = false;
    private bool inputInteractDetected = false;
    private bool inputMouseDetected = false;

    // Public getters
    //public float GetTypingSpeed() => typingSpeed;
    public TextMeshProUGUI GetDialogueTextComponent() => dialogueText;
    public TextMeshProUGUI GetSpeakerNameComponent() => speakerNameText;
    public void ShowContinueIcon() => continueButton?.gameObject.SetActive(true);
    public void HideContinueIcon() => continueButton?.gameObject.SetActive(false);
    public void PlayPortraitAnimation(string stateName) => portraitAnimator?.Play(stateName);
    public void PlayLayoutAnimation(string stateName) => layoutAnimator?.Play(stateName);
    public void ShowPanel() => dialoguePanel?.SetActive(true);
    public void HidePanel() => dialoguePanel?.SetActive(false);
    
    /// <summary>
    /// Check if input was consumed this frame (prevents double-processing)
    /// </summary>
    public bool WasInputConsumedThisFrame() => inputConsumedThisFrame;
    
    /// <summary>
    /// Get the audio source for external use (Ink manager)
    /// </summary>
    public AudioSource GetAudioSource() => audioSource;
    
    /// <summary>
    /// Public method to play dialogue sound (for Ink manager to use)
    /// </summary>
    public void PlayTypingSound(int charIndex, char character)
    {
        PlayDialogueSound(charIndex, character);
    }

    private void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
            continueButton.gameObject.SetActive(false);
            Debug.Log("<color=green>[DialogueUI.Start]</color> Continue button listener registered");
        }
        else
        {
            Debug.LogWarning("<color=red>[DialogueUI.Start]</color> Continue button is NULL!");
        }

        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<DialogueManager>();

        if (dialogueManager != null)
            dialogueManager.DialogueEnded += OnDialogueEnded;

        // Initialize audio system
        InitializeAudioSystem();
    }

private void Update()
{
    // Reset per-frame input guard
    inputConsumedThisFrame = false;

    // --------------------------
    // CHOICE SELECTION INPUT
    // --------------------------
    if (isWaitingForChoice)
    {
        inputSubmitDetected = InputManager.GetInstance().GetSubmitPressed();
        inputInteractDetected = InputManager.GetInstance().GetInteractPressed();
        inputMouseDetected = Input.GetMouseButtonDown(0);
    
        canProgress = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || inputMouseDetected || inputSubmitDetected || inputInteractDetected;

        if (canProgress)
        {
            Debug.Log("<color=cyan>[DialogueUI.Update]</color> Submit input detected while waiting for choice");

            GameObject selectedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

            if (selectedObject != null)
            {
                Button selectedButton = selectedObject.GetComponent<Button>();
                if (selectedButton != null)
                {
                    Debug.Log("<color=lime>[DialogueUI.Update]</color> Invoking selected button via Submit");
                    selectedButton.onClick.Invoke();
                    inputConsumedThisFrame = true;
                    return; // Stop here — handled input
                }
                else
                {
                    Debug.LogWarning("<color=orange>[DialogueUI.Update]</color> Selected object has no Button component");
                }
            }
            else
            {
                Debug.LogWarning("<color=orange>[DialogueUI.Update]</color> No UI button selected while waiting for choice!");
            }
        }

        // Also allow numeric shortcuts (1–9) for quick choice selection
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                int index = i;
                if (index < activeChoiceButtons.Count)
                {
                    Button btn = activeChoiceButtons[index];
                    if (btn != null)
                    {
                        Debug.Log($"<color=lime>[DialogueUI.Update]</color> Numeric key selected choice {index + 1}");
                        btn.onClick.Invoke();
                        inputConsumedThisFrame = true;
                        return;
                    }
                }
                else if (index < activeInkChoiceButtons.Count)
                {
                    Button btn = activeInkChoiceButtons[index];
                    if (btn != null)
                    {
                        Debug.Log($"<color=lime>[DialogueUI.Update]</color> Numeric key selected Ink choice {index + 1}");
                        btn.onClick.Invoke();
                        inputConsumedThisFrame = true;
                        return;
                    }
                }
            }
        }

        // Don’t continue to dialogue progression if we’re waiting for a choice
        return;
    }

    // --------------------------
    // DIALOGUE PROGRESSION INPUT
    // --------------------------
    if (dialogueManager == null || !dialogueManager.IsDialogueActive)
        return;

    if (inputConsumedThisFrame)
        return;

        if (dialogueManager.CurrentDialogue != null && dialogueManager.CurrentDialogue.Type == DialogueType.Ink)
            return;
        

    // You can also handle dialogue progression through InputManager if desired:
    bool progressInput = (useSpaceKey && Input.GetKeyDown(KeyCode.Space)) ||
                         (useReturnKey && Input.GetKeyDown(KeyCode.Return)) ||
                         InputManager.GetInstance().GetSubmitPressed() || 
                         InputManager.GetInstance().GetInteractPressed()  ;

    if (progressInput)
    {
        Debug.Log("<color=yellow>[DialogueUI.Update]</color> Progress input detected");
        inputConsumedThisFrame = true;
        OnInputPressed();
    }
}

    private void InitializeAudioSystem()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        if (defaultAudioInfo != null)
            currentAudioInfo = defaultAudioInfo;

        // Initialize audio dictionary
        audioInfoDictionary = new Dictionary<string, DialogueAudioInfoSO>();

        if (defaultAudioInfo != null)
            audioInfoDictionary.Add(defaultAudioInfo.id, defaultAudioInfo);

        if (audioInfos != null)
        {
            foreach (DialogueAudioInfoSO audioInfo in audioInfos)
            {
                if (audioInfo != null && !audioInfoDictionary.ContainsKey(audioInfo.id))
                    audioInfoDictionary.Add(audioInfo.id, audioInfo);
            }
        }
    }

    /// <summary>
    /// Set the current audio info by ID (can be called by Ink tags)
    /// </summary>
    public void SetCurrentAudioInfo(string id)
    {
        if (audioInfoDictionary == null)
            return;

        DialogueAudioInfoSO audioInfo = null;
        audioInfoDictionary.TryGetValue(id, out audioInfo);
        
        if (audioInfo != null)
            currentAudioInfo = audioInfo;
        else
            Debug.LogWarning($"Failed to find audio info for id: {id}");
    }

    /// <summary>
    /// Reset audio to default
    /// </summary>
    public void ResetAudioToDefault()
    {
        if (defaultAudioInfo != null)
            currentAudioInfo = defaultAudioInfo;
    }

    private void OnDestroy()
    {
        if (dialogueManager != null)
            dialogueManager.DialogueEnded -= OnDialogueEnded;

        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);
    }

    public void ShowDialogue(Dialogue dialogue)
    {
        if (dialogue == null)
        {
            Debug.LogWarning("Dialogue is null!");
            return;
        }

        currentDialogue = dialogue;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);


   // Apply character settings
if (dialogue.Character != null)
{
    // Set speaker name
    if (speakerNameText != null)
        speakerNameText.text = dialogue.Character.Name;

    // Get the animation state for this emotion
    string animationState = dialogue.Character.GetAnimationStateForEmotion(dialogue.Emotion);
    
    // Play the portrait animation
    if (!string.IsNullOrEmpty(animationState) && portraitAnimator != null)
    {
        PlayPortraitAnimation(animationState);
    }
    
    // Apply default layout
    if (layoutAnimator != null)
    {
        PlayLayoutAnimation(dialogue.Character.DefaultLayoutString);
    }

    // Set character icon (graph-based system uses sprites)
    if (characterIcon != null)
    {
        characterIcon.sprite = dialogue.Character.GetEmotionSprite(dialogue.Emotion);
        characterIcon.gameObject.SetActive(true);
    }
}

        // UPDATED: Use TypewriterEffect
        if (typewriterEffect != null && dialogueText != null)
        {
            HideContinueIcon();
            ClearChoices();
            isWaitingForChoice = false;

            typewriterEffect.TypeText(
                dialogue.Text,
                onCharTyped: (index, character) => PlayDialogueSound(index, character),
                onComplete: () => ShowButtons()
            );
        }
        else
        {
            ShowButtons();
        }

        ClearInkChoices();

        // Reset audio to default for each new dialogue node
        ResetAudioToDefault();
    }
        /*  
        ///
        /// Orginal Typewritter effect
        /// 
    public IEnumerator DisplayLine(string line)
    {
        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0;

        if (continueButton != null) 
            continueButton.gameObject.SetActive(false);
        
        ClearChoices();
        isWaitingForChoice = false;
        
        bool isAddingRichTextTag = false;

        // The line below should be changed to use a 'for' loop for better control over the index
        int i = 0;
        int totalVisibleCharacters = line.Length;
    
    while (i < totalVisibleCharacters)
    {
        char letter = line[i];

        // 1. Check for input to skip typing (Mouse click is included here)
        if (useSpaceKey && Input.GetKeyDown(KeyCode.Space) ||
            useReturnKey && Input.GetKeyDown(KeyCode.Return) ||
            (useMouseClick && Input.GetMouseButtonDown(0)) ||
            (useReturnKey && InputManager.GetInstance().GetSubmitPressed()) ||
            (useSpaceKey &&InputManager.GetInstance().GetInteractPressed()) )
        {
            // *** FIX 1: Set maxVisibleCharacters to the full length and BREAK the loop ***
            dialogueText.maxVisibleCharacters = line.Length;
            inputConsumedThisFrame = true; // Consume input to prevent double-processing
            break;
        }

            // 2. Handle Rich Text Tags (e.g., <color=red>text</color>)
            if (line[i] == '<')
            {
                // Find the end of the tag (the '>')
                int tagEndIndex = line.IndexOf('>', i);

                if (tagEndIndex != -1)
                {
                    // Calculate how many characters the tag occupies
                    int tagLength = tagEndIndex - i + 1;

                    // CRITICAL FIX: Advance the internal index past the tag
                    i = tagEndIndex + 1;

                    // Also advance the visible characters count for the tag itself
                    dialogueText.maxVisibleCharacters = Mathf.Min(i, totalVisibleCharacters);

                    // Do NOT yield, tags should appear instantly
                    continue; // Skip the rest of the loop and start the next iteration
                }
            }
        
        // 3. Handle Normal Character Typing
        
        // Play typing sound before showing the character
        PlayDialogueSound(i, line[i]);
        
        // Show the character
        dialogueText.maxVisibleCharacters++;
        i++; // Move to the next character
        
        // Yield for the typing speed
        yield return new WaitForSeconds(typingSpeed);
    }

    // --- The code below runs ONLY after the loop finishes (either by completing or by 'break') ---
    
    // Ensure all text is visible (Redundant if break was hit, but safe)
    dialogueText.maxVisibleCharacters = line.Length; 
    
    // *** FIX 2: Set the coroutine to null and show buttons/continue icon ***
    displayLineCoroutine = null;
    ShowButtons(); // This will enable the continue button or choices


    }
        */
    private void PlayDialogueSound(int currentDisplayedCharacterCount, char currentCharacter)
    {
        // Safety checks
        if (currentAudioInfo == null || audioSource == null)
            return;

        AudioClip[] dialogueTypingSoundClips = currentAudioInfo.dialogueTypingSoundClips;
        
        if (dialogueTypingSoundClips == null || dialogueTypingSoundClips.Length == 0)
            return;

        int frequencyLevel = currentAudioInfo.frequencyLevel;
        float minPitch = currentAudioInfo.minPitch;
        float maxPitch = currentAudioInfo.maxPitch;
        bool stopAudioSource = currentAudioInfo.stopAudioSource;

        // Play sound based on frequency
        if (currentDisplayedCharacterCount % frequencyLevel == 0)
        {
            if (stopAudioSource)
                audioSource.Stop();

            AudioClip soundClip = null;

            if (makePredictable)
            {
                // Predictable audio from character hash
                int hashCode = currentCharacter.GetHashCode();
                int predictableIndex = hashCode % dialogueTypingSoundClips.Length;
                soundClip = dialogueTypingSoundClips[predictableIndex];

                // Predictable pitch
                int minPitchInt = (int)(minPitch * 100);
                int maxPitchInt = (int)(maxPitch * 100);
                int pitchRangeInt = maxPitchInt - minPitchInt;

                if (pitchRangeInt != 0)
                {
                    int predictablePitchInt = (hashCode % pitchRangeInt) + minPitchInt;
                    float predictablePitch = predictablePitchInt / 100f;
                    audioSource.pitch = predictablePitch;
                }
                else
                {
                    audioSource.pitch = minPitch;
                }
            }
            else
            {
                // Random audio
                int randomIndex = UnityEngine.Random.Range(0, dialogueTypingSoundClips.Length);
                soundClip = dialogueTypingSoundClips[randomIndex];
                audioSource.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
            }

            if (soundClip != null)
                audioSource.PlayOneShot(soundClip);
        }
    }

    private void ShowButtons()
    {
        if (currentDialogue == null)
            return;

        if (currentDialogue.Type == DialogueType.MultipleChoice)
        {
            if (continueButton != null)
                continueButton.gameObject.SetActive(false);

            CreateChoiceButtons(currentDialogue.Choices);
            isWaitingForChoice = true;
        }
        else
        {
            if (continueButton != null)
                continueButton.gameObject.SetActive(true);
            
            isWaitingForChoice = false;
        }
    }

    public void StartDialogue(DialogueChoicer dialogueChoicer)
    {
        if (dialogueChoicer == null || dialogueChoicer.Dialogue == null)
        {
            Debug.LogWarning("DialogueChoicer or its Dialogue is null!");
            return;
        }

        ShowDialogue(dialogueChoicer.Dialogue);
    }

    private void CreateChoiceButtons(List<DialogueChoiceData> choices)
    {
        if (choicesContainer == null || choiceButtonPrefab == null)
        {
            Debug.LogWarning("Choices container or button prefab is not assigned!");
            return;
        }

        if (choices == null || choices.Count == 0)
        {
            Debug.LogWarning("No choices available for multiple choice dialogue!");
            return;
        }

        foreach (DialogueChoiceData choice in choices)
        {
            Button btn = Instantiate(choiceButtonPrefab, choicesContainer);
            
            TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
                btnText.text = choice.Text;

            Dialogue nextDialogue = choice.NextDialogue;
            btn.onClick.AddListener(() => OnChoiceSelected(nextDialogue));

            activeChoiceButtons.Add(btn);
        }
        
        // Auto-select first button for keyboard/controller navigation
        if (activeChoiceButtons.Count > 0)
        {
            StartCoroutine(SelectFirstButton(activeChoiceButtons[0]));
        }
    }
    
    private IEnumerator SelectFirstButton(Button button)
    {
        if (button == null)
            yield break;
            
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        
        if (button != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }

    private void ClearChoices()
    {
        foreach (Button btn in activeChoiceButtons)
        {
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                Destroy(btn.gameObject);
            }
        }
        activeChoiceButtons.Clear();
    }

    private void OnChoiceSelected(Dialogue nextDialogue)
    {
        isWaitingForChoice = false;
        inputConsumedThisFrame = true; // Consume input to prevent DialogueManager from processing it
        
        if (dialogueManager != null)
            dialogueManager.SelectDialogue(nextDialogue);
        else if (nextDialogue != null)
            ShowDialogue(nextDialogue);
        else
            EndDialogue();
    }

    /// <summary>
    /// Clears any choice buttons created by Ink.
    /// </summary>
    public void ClearInkChoices()
    {
        foreach (Button btn in activeInkChoiceButtons)
        {
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                Destroy(btn.gameObject);
            }
        }
        activeInkChoiceButtons.Clear();
    }

    /// <summary>
    /// Creates and displays choice buttons from an Ink story.
    /// </summary>
    public void DisplayInkChoices(List<Ink.Runtime.Choice> choices, Action<int> onChoiceSelected)
    {
        ClearInkChoices();
        HideContinueIcon();

        if (choicesContainer == null || choiceButtonPrefab == null)
        {
            Debug.LogWarning("Choices container or button prefab is not assigned!");
            return;
        }

        // CRITICAL FIX: Check if there are actually choices before proceeding
        if (choices == null || choices.Count == 0)
        {
            // No choices to display, show continue icon instead
            ShowContinueIcon();
            return;
        }

        foreach (Ink.Runtime.Choice choice in choices)
        {
            Button btn = Instantiate(choiceButtonPrefab, choicesContainer);

            TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
                btnText.text = choice.text;

            int choiceIndex = choice.index;
            btn.onClick.AddListener(() => onChoiceSelected(choiceIndex));

            activeInkChoiceButtons.Add(btn);
        }

        // CRITICAL FIX: Only select first choice if buttons were actually created
        if (activeInkChoiceButtons.Count > 0)
        {
            StartCoroutine(SelectFirstInkChoice());
        }
    }
    
    private IEnumerator SelectFirstInkChoice() 
    {
        // CRITICAL FIX: Double-check the list isn't empty
        if (activeInkChoiceButtons == null || activeInkChoiceButtons.Count == 0)
            yield break;

        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        
        // CRITICAL FIX: Check again after waiting
        if (activeInkChoiceButtons != null && activeInkChoiceButtons.Count > 0 && activeInkChoiceButtons[0] != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(activeInkChoiceButtons[0].gameObject);
        }
    }

    public void EndDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        ClearChoices();
        ClearInkChoices();
        currentDialogue = null;
        isWaitingForChoice = false;

        if (displayLineCoroutine != null)
        {
            StopCoroutine(displayLineCoroutine);
            displayLineCoroutine = null;
        }

        Debug.Log("Dialogue ended");
    }

    private void OnContinueClicked()
    {
        Debug.Log("<color=magenta>[DialogueUI.OnContinueClicked]</color> Continue button clicked!");
        OnInputPressed();
    }

    /// <summary>
    /// Central method for handling input - skips typing or advances dialogue
    /// </summary>
public void OnInputPressed()
{
    if (typewriterEffect != null && typewriterEffect.IsTyping)
    {
        // Skip typewriter effect
        typewriterEffect.Skip();
        Debug.Log("<color=green>[DialogueUI]</color> Skipped typewriter");
    }
    else
    {
        // Progress to next dialogue
        if (dialogueManager != null)
        {
            Debug.Log("<color=green>[DialogueUI]</color> Progressing dialogue");
            dialogueManager.ProgressDialogue();
        }
    }
}

    private void OnDialogueEnded()
    {
        EndDialogue();
    }

    public bool IsDialogueActive()
    {
        return dialoguePanel != null && dialoguePanel.activeSelf;
    }
}