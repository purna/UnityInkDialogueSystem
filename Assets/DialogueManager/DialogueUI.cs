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
    [SerializeField] private float typingSpeed = 0.04f;
    
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
    
    // Input settings - can be controlled by DialogueManager
    private bool useReturnKey = true;
    private bool useMouseClick = true;
    private bool useSpaceKey = true;

    private Dialogue currentDialogue;
    private List<Button> activeChoiceButtons = new List<Button>();
    private bool isWaitingForChoice = false;
    private Coroutine displayLineCoroutine;
    private List<Button> activeInkChoiceButtons = new List<Button>();

    // Public getters
    public float GetTypingSpeed() => typingSpeed;
    public TextMeshProUGUI GetDialogueTextComponent() => dialogueText;
    public TextMeshProUGUI GetSpeakerNameComponent() => speakerNameText;
    public void ShowContinueIcon() => continueButton?.gameObject.SetActive(true);
    public void HideContinueIcon() => continueButton?.gameObject.SetActive(false);
    public void PlayPortraitAnimation(string stateName) => portraitAnimator?.Play(stateName);
    public void PlayLayoutAnimation(string stateName) => layoutAnimator?.Play(stateName);
    public void ShowPanel() => dialoguePanel?.SetActive(true);
    public void HidePanel() => dialoguePanel?.SetActive(false);
    
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
        // Allow Space or Return to activate the currently selected button
        if (isWaitingForChoice && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            // Get the currently selected button
            GameObject selectedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            
            if (selectedObject != null)
            {
                Button selectedButton = selectedObject.GetComponent<Button>();
                if (selectedButton != null)
                {
                    selectedButton.onClick.Invoke();
                }
            }
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

        if (speakerNameText != null && dialogue.Character != null)
            speakerNameText.text = dialogue.Character.Name;

        if (characterIcon != null && dialogue.Character != null)
        {
            characterIcon.sprite = dialogue.Character.GetEmotionSprite(dialogue.Emotion);
            characterIcon.gameObject.SetActive(true);
        }

        if (displayLineCoroutine != null)
            StopCoroutine(displayLineCoroutine);

        if (dialogueText != null)
            displayLineCoroutine = StartCoroutine(DisplayLine(dialogue.Text));
        else
            ShowButtons();

        ClearInkChoices();
    }

    private IEnumerator DisplayLine(string line)
    {
        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0;

        if (continueButton != null) 
            continueButton.gameObject.SetActive(false);
        
        ClearChoices();
        isWaitingForChoice = false;
        
        bool isAddingRichTextTag = false;

        foreach (char letter in line.ToCharArray())
        {
            // Check for input to skip typing
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                dialogueText.maxVisibleCharacters = line.Length;
                break;
            }

            if (letter == '<' || isAddingRichTextTag)
            {
                isAddingRichTextTag = true;
                if (letter == '>')
                    isAddingRichTextTag = false;
            }
            else
            {
                // Play typing sound before showing character
                PlayDialogueSound(dialogueText.maxVisibleCharacters, letter);
                dialogueText.maxVisibleCharacters++;
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        // Ensure all text is visible
        dialogueText.maxVisibleCharacters = line.Length;
        displayLineCoroutine = null;
        ShowButtons();
    }

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
        OnInputPressed();
    }

    /// <summary>
    /// Central method for handling input - skips typing or advances dialogue
    /// </summary>
    public void OnInputPressed()
    {
        // 1. If still typing, skip to end
        if (displayLineCoroutine != null)
        {
            StopCoroutine(displayLineCoroutine);
            dialogueText.maxVisibleCharacters = dialogueText.text.Length;
            displayLineCoroutine = null;
            ShowButtons();
            return;
        }

        // 2. If waiting for choice, do nothing
        if (isWaitingForChoice)
            return;

        // 3. Otherwise, advance dialogue
        if (currentDialogue != null)
        {
            Dialogue nextDialogue = currentDialogue.GetNextDialogue();
            
            if (dialogueManager != null)
                dialogueManager.SelectDialogue(nextDialogue);
            else if (nextDialogue != null)
                ShowDialogue(nextDialogue);
            else
                EndDialogue();
        }
        else
        {
            EndDialogue();
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