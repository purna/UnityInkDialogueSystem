using System; 
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Unified DialogueUI that manages both ScreenSpace and WorldSpace canvases
/// Switches between them based on DialogueSetupMode
/// </summary>
public class DialogueUI : MonoBehaviour
{
    /* Canvas References */
    [Header("Canvas Setup")]
    [SerializeField] private Canvas screenSpaceCanvas;
    [SerializeField] private Canvas worldSpaceCanvas;
    
    // Current active canvas (set at runtime)
    private Canvas activeCanvas;
    
    /* UI References - Screen Space */
    [Header("Screen Space UI Components")]
    [SerializeField] private GameObject screenSpaceDialoguePanel;
    [SerializeField] private TextMeshProUGUI screenSpaceDialogueText;
    [SerializeField] private TextMeshProUGUI screenSpaceSpeakerNameText;
    [SerializeField] private Image screenSpaceCharacterIcon;
    [SerializeField] private Transform screenSpaceChoicesContainer;
    [SerializeField] private Button screenSpaceChoiceButtonPrefab;
    [SerializeField] private Button screenSpaceContinueButton;
    [SerializeField] private Animator screenSpacePortraitAnimator;
    [SerializeField] private Animator screenSpaceLayoutAnimator;
    
    /* UI References - World Space */
    [Header("World Space UI Components")]
    [SerializeField] private GameObject worldSpaceDialoguePanel;
    [SerializeField] private TextMeshProUGUI worldSpaceDialogueText;
    [SerializeField] private TextMeshProUGUI worldSpaceSpeakerNameText;
    [SerializeField] private Image worldSpaceCharacterIcon;
    [SerializeField] private Transform worldSpaceChoicesContainer;
    [SerializeField] private Button worldSpaceChoiceButtonPrefab;
    [SerializeField] private Button worldSpaceContinueButton;
    [SerializeField] private Animator worldSpacePortraitAnimator;
    [SerializeField] private Animator worldSpaceLayoutAnimator;
    
    /* Active Component References (set dynamically) */
    private GameObject dialoguePanel;
    private TextMeshProUGUI dialogueText;
    private TextMeshProUGUI speakerNameText;
    private Image characterIcon;
    private Transform choicesContainer;
    private Button choiceButtonPrefab;
    private Button continueButton;
    private Animator portraitAnimator;
    private Animator layoutAnimator;

    [Header("Typewriter Effect")]
    [SerializeField] private TypewriterEffect typewriterEffect;
    
    [Header("Audio")]
    [SerializeField] private DialogueAudioInfoSO defaultAudioInfo;
    [SerializeField] private DialogueAudioInfoSO[] audioInfos;
    [SerializeField] private bool makePredictable;
    private DialogueAudioInfoSO currentAudioInfo;
    private Dictionary<string, DialogueAudioInfoSO> audioInfoDictionary;
    private AudioSource audioSource;

    [Header("Dialogue System")]
    [SerializeField] private DialogueManager dialogueManager;
    
    [Header("Input Settings")]
    [SerializeField] private bool useReturnKey = true;
    [SerializeField] private bool useMouseClick = true;
    [SerializeField] private bool useSpaceKey = true;
    
    [Header("World Space Settings")]
    [SerializeField] private Vector3 worldSpaceOffset = new Vector3(0, 2.5f, 0);
    [SerializeField] private bool worldSpaceFaceCamera = true;
    
    // Runtime custom offset (set by DialogueTrigger for smart positioning)
    private Vector3 customWorldSpaceOffset;
    private bool hasCustomOffset = false;

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
    
    // Track current mode and NPC attachment
    private DialogueSetupMode currentMode = DialogueSetupMode.ScreenSpace;
    private Transform attachedNPC = null;

    // Public getters
    public TextMeshProUGUI GetDialogueTextComponent() => dialogueText;
    public TextMeshProUGUI GetSpeakerNameComponent() => speakerNameText;
    public void ShowContinueIcon() => continueButton?.gameObject.SetActive(true);
    public void HideContinueIcon() => continueButton?.gameObject.SetActive(false);
    public void PlayPortraitAnimation(string stateName) => portraitAnimator?.Play(stateName);
    public void PlayLayoutAnimation(string stateName) => layoutAnimator?.Play(stateName);
    public void ShowPanel() => dialoguePanel?.SetActive(true);
    public void HidePanel() => dialoguePanel?.SetActive(false);
    public bool WasInputConsumedThisFrame() => inputConsumedThisFrame;
    public AudioSource GetAudioSource() => audioSource;

    private void Start()
    {
        // Initialize with ScreenSpace by default
        SetupMode(DialogueSetupMode.ScreenSpace);
        
        // Hide both panels initially
        if (screenSpaceDialoguePanel != null)
            screenSpaceDialoguePanel.SetActive(false);
        if (worldSpaceDialoguePanel != null)
            worldSpaceDialoguePanel.SetActive(false);

        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<DialogueManager>();

        if (dialogueManager != null)
            dialogueManager.DialogueEnded += OnDialogueEnded;

        InitializeAudioSystem();
    }

    /// <summary>
    /// Switch between ScreenSpace and WorldSpace modes
    /// Called by DialogueController before starting dialogue
    /// </summary>
    public void SetupMode(DialogueSetupMode mode, Transform npcTransform = null)
    {
        currentMode = mode;
        
        if (mode == DialogueSetupMode.ScreenSpace)
        {
            SetupScreenSpace();
        }
        else
        {
            SetupWorldSpace(npcTransform);
        }
        
        // IMPORTANT: Update typewriter's text component reference
        if (typewriterEffect != null)
        {
            typewriterEffect.UpdateTextComponent();
        }
        
        Debug.Log($"[DialogueUI] Switched to {mode} mode");
    }

    private void SetupScreenSpace()
    {
        activeCanvas = screenSpaceCanvas;
        dialoguePanel = screenSpaceDialoguePanel;
        dialogueText = screenSpaceDialogueText;
        speakerNameText = screenSpaceSpeakerNameText;
        characterIcon = screenSpaceCharacterIcon;
        choicesContainer = screenSpaceChoicesContainer;
        choiceButtonPrefab = screenSpaceChoiceButtonPrefab;
        continueButton = screenSpaceContinueButton;
        portraitAnimator = screenSpacePortraitAnimator;
        layoutAnimator = screenSpaceLayoutAnimator;
        
        // Detach from NPC if previously attached
        if (attachedNPC != null && worldSpaceCanvas != null)
        {
            worldSpaceCanvas.transform.SetParent(transform);
            attachedNPC = null;
        }
        
        // Setup continue button listener
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueClicked);
            continueButton.gameObject.SetActive(false);
        }
        
        // Hide WorldSpace, ensure ScreenSpace is ready
        if (worldSpaceDialoguePanel != null)
            worldSpaceDialoguePanel.SetActive(false);
    }

    private void SetupWorldSpace(Transform npcTransform)
    {
        activeCanvas = worldSpaceCanvas;
        dialoguePanel = worldSpaceDialoguePanel;
        dialogueText = worldSpaceDialogueText;
        speakerNameText = worldSpaceSpeakerNameText;
        characterIcon = worldSpaceCharacterIcon;
        choicesContainer = worldSpaceChoicesContainer;
        choiceButtonPrefab = worldSpaceChoiceButtonPrefab;
        continueButton = worldSpaceContinueButton;
        portraitAnimator = worldSpacePortraitAnimator;
        layoutAnimator = worldSpaceLayoutAnimator;
        
        // Setup continue button listener
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueClicked);
            continueButton.gameObject.SetActive(false);
        }
        
        // Hide ScreenSpace
        if (screenSpaceDialoguePanel != null)
            screenSpaceDialoguePanel.SetActive(false);
        
        // Attach to NPC if provided
        if (npcTransform != null && worldSpaceCanvas != null)
        {
            AttachToNPC(npcTransform);
        }
    }

    /// <summary>
    /// Attach the WorldSpace canvas to an NPC transform
    /// </summary>
    public void AttachToNPC(Transform npcTransform)
    {
        if (worldSpaceCanvas == null)
        {
            Debug.LogWarning("[DialogueUI] Cannot attach to NPC - WorldSpace canvas not assigned!");
            return;
        }
        
        attachedNPC = npcTransform;
        worldSpaceCanvas.transform.SetParent(npcTransform);
        
        // Use custom offset if set, otherwise use default
        Vector3 offsetToUse = hasCustomOffset ? customWorldSpaceOffset : worldSpaceOffset;
        worldSpaceCanvas.transform.localPosition = offsetToUse;
        
        // Make canvas face camera
        if (worldSpaceFaceCamera && Camera.main != null)
        {
            worldSpaceCanvas.transform.LookAt(Camera.main.transform);
            worldSpaceCanvas.transform.Rotate(0, 180, 0);
        }
        
        Debug.Log($"[DialogueUI] Attached to NPC: {npcTransform.name} with offset: {offsetToUse}");
    }

    /// <summary>
    /// Set custom WorldSpace offset (called by DialogueTrigger for smart positioning)
    /// </summary>
    public void SetWorldSpaceOffset(Vector3 offset)
    {
        customWorldSpaceOffset = offset;
        hasCustomOffset = true;
        
        // If already attached to NPC, update position immediately
        if (attachedNPC != null && worldSpaceCanvas != null)
        {
            worldSpaceCanvas.transform.localPosition = offset;
        }
        
        Debug.Log($"[DialogueUI] Custom WorldSpace offset set to: {offset}");
    }
    
    /// <summary>
    /// Reset to default WorldSpace offset
    /// </summary>
    public void ResetWorldSpaceOffset()
    {
        customWorldSpaceOffset = Vector3.zero;
        hasCustomOffset = false;
        
        // If attached to NPC, reset to default position
        if (attachedNPC != null && worldSpaceCanvas != null)
        {
            worldSpaceCanvas.transform.localPosition = worldSpaceOffset;
        }
        
        Debug.Log("[DialogueUI] Reset to default WorldSpace offset");
    }

    /// <summary>
    /// Detach WorldSpace canvas from NPC
    /// </summary>
    public void DetachFromNPC()
    {
        if (attachedNPC != null && worldSpaceCanvas != null)
        {
            worldSpaceCanvas.transform.SetParent(transform);
            attachedNPC = null;
            
            // Reset custom offset when detaching
            hasCustomOffset = false;
            customWorldSpaceOffset = Vector3.zero;
            
            Debug.Log("[DialogueUI] Detached from NPC");
        }
    }

    private void LateUpdate()
    {
        // Keep WorldSpace dialogue facing camera
        if (currentMode == DialogueSetupMode.WorldSpace && 
            attachedNPC != null && 
            worldSpaceCanvas != null && 
            worldSpaceFaceCamera &&
            dialoguePanel != null &&
            dialoguePanel.activeSelf &&
            Camera.main != null)
        {
            worldSpaceCanvas.transform.LookAt(Camera.main.transform);
            worldSpaceCanvas.transform.Rotate(0, 180, 0);
        }
    }

    private void Update()
    {
        inputConsumedThisFrame = false;

        inputSubmitDetected = InputManager.GetInstance().GetSubmitPressed();
        inputInteractDetected = InputManager.GetInstance().GetInteractPressed();
        inputMouseDetected = Input.GetMouseButtonDown(0);
        
        canProgress = Input.GetKeyDown(KeyCode.Space) || 
                      Input.GetKeyDown(KeyCode.Return) || 
                      inputMouseDetected || 
                      inputSubmitDetected || 
                      inputInteractDetected;

        // CHOICE SELECTION INPUT (Both Graph and Ink)
        bool hasGraphChoices = activeChoiceButtons != null && activeChoiceButtons.Count > 0;
        bool hasInkChoices = activeInkChoiceButtons != null && activeInkChoiceButtons.Count > 0;
        bool hasAnyChoices = hasGraphChoices || hasInkChoices;

        if (isWaitingForChoice || hasAnyChoices)
        {
            if (canProgress)
            {
                GameObject selectedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

                if (selectedObject != null)
                {
                    Button selectedButton = selectedObject.GetComponent<Button>();
                    if (selectedButton != null)
                    {
                        selectedButton.onClick.Invoke();
                        inputConsumedThisFrame = true;
                        return;
                    }
                }
                else
                {
                    // Auto-select first choice
                    if (hasGraphChoices && activeChoiceButtons[0] != null)
                    {
                        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(activeChoiceButtons[0].gameObject);
                    }
                    else if (hasInkChoices && activeInkChoiceButtons[0] != null)
                    {
                        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(activeInkChoiceButtons[0].gameObject);
                    }
                    return;
                }
            }

            // Numeric shortcuts (1–9)
            for (int i = 0; i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    int index = i;
                    
                    if (hasGraphChoices && index < activeChoiceButtons.Count)
                    {
                        Button btn = activeChoiceButtons[index];
                        if (btn != null)
                        {
                            btn.onClick.Invoke();
                            inputConsumedThisFrame = true;
                            return;
                        }
                    }
                    else if (hasInkChoices && index < activeInkChoiceButtons.Count)
                    {
                        Button btn = activeInkChoiceButtons[index];
                        if (btn != null)
                        {
                            btn.onClick.Invoke();
                            inputConsumedThisFrame = true;
                            return;
                        }
                    }
                }
            }

            if (hasAnyChoices)
                return;
        }

        // DIALOGUE PROGRESSION INPUT
        if (dialogueManager == null || !dialogueManager.IsDialogueActive)
            return;

        if (inputConsumedThisFrame)
            return;

        // Don't handle progression if we're in Ink mode
        if (dialogueManager.CurrentDialogue != null && dialogueManager.CurrentDialogue.Type == DialogueType.Ink)
            return;
            
        bool progressInput = (useSpaceKey && Input.GetKeyDown(KeyCode.Space)) ||
                             (useReturnKey && Input.GetKeyDown(KeyCode.Return)) ||
                             InputManager.GetInstance().GetSubmitPressed() || 
                             InputManager.GetInstance().GetInteractPressed();

        if (progressInput)
        {
            inputConsumedThisFrame = true;
            OnInputPressed();
        }
    }

    private void InitializeAudioSystem()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        if (defaultAudioInfo != null)
            currentAudioInfo = defaultAudioInfo;

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
            if (speakerNameText != null)
                speakerNameText.text = dialogue.Character.Name;

            string animationState = dialogue.Character.GetAnimationStateForEmotion(dialogue.Emotion);
            
            if (!string.IsNullOrEmpty(animationState) && portraitAnimator != null)
            {
                PlayPortraitAnimation(animationState);
            }
            
            if (layoutAnimator != null)
            {
                PlayLayoutAnimation(dialogue.Character.DefaultLayoutString);
            }

            if (characterIcon != null)
            {
                characterIcon.sprite = dialogue.Character.GetEmotionSprite(dialogue.Emotion);
                characterIcon.gameObject.SetActive(true);
            }
        }

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
        ResetAudioToDefault();
    }

    public void PlayTypingSound(int charIndex, char character)
    {
        PlayDialogueSound(charIndex, character);
    }

    private void PlayDialogueSound(int currentDisplayedCharacterCount, char currentCharacter)
    {
        if (currentAudioInfo == null || audioSource == null)
            return;

        AudioClip[] dialogueTypingSoundClips = currentAudioInfo.dialogueTypingSoundClips;
        
        if (dialogueTypingSoundClips == null || dialogueTypingSoundClips.Length == 0)
            return;

        int frequencyLevel = currentAudioInfo.frequencyLevel;
        float minPitch = currentAudioInfo.minPitch;
        float maxPitch = currentAudioInfo.maxPitch;
        bool stopAudioSource = currentAudioInfo.stopAudioSource;

        if (currentDisplayedCharacterCount % frequencyLevel == 0)
        {
            if (stopAudioSource)
                audioSource.Stop();

            AudioClip soundClip = null;

            if (makePredictable)
            {
                int hashCode = currentCharacter.GetHashCode();
                int predictableIndex = hashCode % dialogueTypingSoundClips.Length;
                soundClip = dialogueTypingSoundClips[predictableIndex];

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
        inputConsumedThisFrame = true;
        
        if (dialogueManager != null)
            dialogueManager.SelectDialogue(nextDialogue);
        else if (nextDialogue != null)
            ShowDialogue(nextDialogue);
        else
            EndDialogue();
    }

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

   public void DisplayInkChoices(List<Ink.Runtime.Choice> choices, Action<int> onChoiceSelected)
    {
        ClearInkChoices();
        HideContinueIcon();

        if (choicesContainer == null || choiceButtonPrefab == null)
        {
            Debug.LogWarning("Choices container or button prefab is not assigned!");
            return;
        }

        if (choices == null || choices.Count == 0)
        {
            ShowContinueIcon();
            return;
        }

        for (int i = 0; i < choices.Count; i++)
        {
            Ink.Runtime.Choice choice = choices[i];
            Button btn = Instantiate(choiceButtonPrefab, choicesContainer);

            TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
                btnText.text = choice.text;

            // CRITICAL: Use choice.index (the original index) not the filtered index
            int originalChoiceIndex = choice.index;
            btn.onClick.AddListener(() => onChoiceSelected(originalChoiceIndex));

            activeInkChoiceButtons.Add(btn);
        }

        if (activeInkChoiceButtons.Count > 0)
        {
            StartCoroutine(SelectFirstInkChoice());
        }
    }
    private IEnumerator SelectFirstInkChoice() 
    {
        if (activeInkChoiceButtons == null || activeInkChoiceButtons.Count == 0)
            yield break;

        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        
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
        
        // Detach from NPC when dialogue ends
        if (currentMode == DialogueSetupMode.WorldSpace)
        {
            DetachFromNPC();
        }

        Debug.Log("Dialogue ended");
    }

    private void OnContinueClicked()
    {
        OnInputPressed();
    }

    public void OnInputPressed()
    {
        if (typewriterEffect != null && typewriterEffect.IsTyping)
        {
            typewriterEffect.Skip();
        }
        else
        {
            if (dialogueManager != null)
            {
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