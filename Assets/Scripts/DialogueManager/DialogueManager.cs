using System;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueUI _dialogueUI;
    [SerializeField] private DialogueInkManager _inkManager;

    [Header("Input Settings")]
    [SerializeField] private bool useReturnKey = true;
    [SerializeField] private bool useMouseClick = true;
    [SerializeField] private bool useSpaceKey = true;
        
    [Header("External System References")]
    [Tooltip("Animator used for external Ink functions (e.g., player emotes). Required for Ink nodes.")]
    [SerializeField] private Animator playerEmoteAnimator;
    
    [Tooltip("Audio Source for playing dialogue sounds")]
    [SerializeField] private AudioSource audioSource;
    
    [Tooltip("Player GameObject for pause/resume functionality")]
    [SerializeField] private GameObject player;

    private Dialogue _nowDialogue;
    private bool _isWaitingForChoice = false;
    private bool _isWaitingForInk = false;

    // Events for external systems to subscribe to
    public event Action DialogueStarted;
    public event Action DialogueEnded;
    public event Action<ExternalFunctionType> OnExternalFunction;
    public event Action<string> OnCustomFunction;
    public event Action<string> OnGiveItem;
    public event Action<string> OnRemoveItem;
    public event Action<string, string> OnPlayAnimation; // animatorName, animationName
    public event Action<string> OnPlaySound;
    public event Action<string, int> OnUpdateQuest; // questName, progress
    public event Action<Vector3> OnTeleportPlayer;
    public event Action<string, Vector3> OnSpawnNPC; // npcName, position
    public event Action<string> OnShowUI; // uiName
    public event Action<string> OnHideUI; // uiName
    public event Action<string, object> OnSetVariable; // variableName, value
    public event Action<string> OnTriggerEvent; // eventName

    public bool IsDialogueActive => _nowDialogue != null;
    public Dialogue CurrentDialogue => _nowDialogue;

    private void Awake()
    {
        if (_dialogueUI == null)
        {
            _dialogueUI = FindObjectOfType<DialogueUI>();
        }

        if (_dialogueUI == null)
        {
            Debug.LogWarning("DialogueUI not found! DialogueManager requires a DialogueUI component.");
        }

        _inkManager = DialogueInkManager.GetInstance();
        if (_inkManager == null)
        {
            Debug.LogWarning("DialogueInkManager.GetInstance() returned null. Ink nodes will not function.");
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    // ... (Keep all existing methods: StartDialogue, Update, ProgressDialogue, SelectDialogue, SetDialogue, StopDialogue, EndDialogue)

    public void StartDialogue(DialogueController dialogueController)
    {
        if (dialogueController == null || dialogueController.Dialogue == null)
        {
            Debug.LogWarning("DialogueController or its Dialogue is null!");
            return;
        }
        StartDialogue(dialogueController.Dialogue);
    }

    public void StartDialogue(Dialogue dialogue)
    {
        if (dialogue == null)
        {
            Debug.LogWarning("Cannot start null dialogue!");
            return;
        }
        SetDialogue(dialogue);
    }

    public void StartDialogueFromContainer(DialogueContainer container, DialogueGroup group = null, bool startingOnly = true)
    {
        if (container == null)
        {
            Debug.LogWarning("DialogueContainer is null!");
            return;
        }
        Dialogue startDialogue = GetStartingDialogueFromContainer(container, group, startingOnly);
        if (startDialogue != null)
        {
            StartDialogue(startDialogue);
        }
        else
        {
            Debug.LogWarning("No starting dialogue found in container!");
        }
    }

    private Dialogue GetStartingDialogueFromContainer(DialogueContainer container, DialogueGroup group, bool startingOnly)
    {
        System.Collections.Generic.List<string> dialogueNames;

        if (group != null)
        {
            dialogueNames = container.GetGroupedDialoguesNames(group, startingOnly);
        }
        else
        {
            dialogueNames = container.GetUngroupedDialoguesNames(startingOnly);
        }

        if (dialogueNames.Count == 0)
        {
            return null;
        }
        
        return container.GetDialogueByName(dialogueNames[0]);
    }

    private void Update()
    {
        // Handle Ink node completion
        if (_isWaitingForInk)
        {
            if (_inkManager != null && !_inkManager.dialogueIsPlaying)
            {
                _isWaitingForInk = false;
                ProgressDialogue();
            }
            return; // Don't process any other input while Ink is active
        }

        // Don't process input if waiting for choice or no dialogue active
        if (_nowDialogue == null || _isWaitingForChoice)
            return;

        // IMPORTANT: Only handle input for non-Ink nodes
        // Ink nodes handle their own input in DialogueInkManager
        if (_nowDialogue.Type == DialogueType.Ink)
            return;


    }

    public void SelectDialogue(Dialogue dialogue)
    {
            Debug.Log($"<color=lime>[SelectDialogue]</color> Player selected a choice - Setting _isWaitingForChoice = FALSE");
        _isWaitingForChoice = false;
        SetDialogue(dialogue);
    }
    
    public bool IsWaitingForChoice()
{
    return _isWaitingForChoice;
}

private void SetDialogue(Dialogue dialogue)
{
    _nowDialogue = dialogue;

        if (dialogue == null)
        {
            StopDialogue();
            return;
        }
    
     // AInvoke DialogueStarted when setting first dialogue
    if (DialogueStarted != null && !IsDialogueActive)
    {
        DialogueStarted?.Invoke();
    }


    Debug.Log($"<color=orange>[SetDialogue]</color> Setting dialogue: {dialogue.Name}, Type: {dialogue.Type}");
    
    LogVariableNodeDebug(dialogue);
    
    switch (dialogue.Type)
    {
        case DialogueType.ExternalFunction:
            Debug.Log("<color=yellow>[SetDialogue]</color> Processing External Function - Auto-progressing");
            ProcessExternalFunction(dialogue);
            SetDialogue(dialogue.GetNextDialogue());
            return;

        case DialogueType.ModifyVariable:
            Debug.Log("<color=yellow>[SetDialogue]</color> Processing Modify Variable - Auto-progressing");
            ProcessModifyVariable(dialogue);
            SetDialogue(dialogue.GetNextDialogue());
            return;

        case DialogueType.VariableCondition:
            Debug.Log("<color=yellow>[SetDialogue]</color> Processing Variable Condition - Auto-progressing");
            Dialogue nextNode = ProcessVariableCondition(dialogue);
            SetDialogue(nextNode);
            return;

        case DialogueType.Ink:
            Debug.Log("<color=yellow>[SetDialogue]</color> Processing Ink Node - Waiting for Ink");
            if (_inkManager != null)
            {
                _isWaitingForInk = true;
                if (_dialogueUI != null) { _dialogueUI.EndDialogue(); } 
                _inkManager.EnterDialogueMode(
                    dialogue.InkJsonAsset, 
                    playerEmoteAnimator,
                    dialogue.KnotName, 
                    dialogue.StartFromBeginning
                );
            }
            else
            {
                Debug.LogError("Ink Node hit, but no Ink Manager! Skipping...");
                SetDialogue(dialogue.GetNextDialogue());
            }
            return;

        case DialogueType.MultipleChoice:
            // Set waiting flag BEFORE showing UI
            _isWaitingForChoice = true;
            Debug.Log($"<color=cyan>[SetDialogue]</color> Multiple Choice Node - Setting _isWaitingForChoice = TRUE");
            Debug.Log($"<color=cyan>[SetDialogue]</color> Number of choices: {dialogue.Choices.Count}");
            
            // Show the dialogue with choices in UI
            if (_dialogueUI != null)
            {
                _dialogueUI.ShowDialogue(dialogue);
            }
            return;

        case DialogueType.SingleChoice:
        default:
            // Reset waiting flag for non-choice dialogues
            _isWaitingForChoice = false;
            Debug.Log($"<color=green>[SetDialogue]</color> Normal dialogue - Setting _isWaitingForChoice = FALSE");
            
            // Show normal dialogue in UI
            if (_dialogueUI != null)
            {
                _dialogueUI.ShowDialogue(dialogue);
            }
            break;
    }
}


public void ProgressDialogue()
{
    Debug.Log($"<color=magenta>[ProgressDialogue]</color> Called - _isWaitingForChoice: {_isWaitingForChoice}");

    if (_nowDialogue == null)
        return;

    if (_nowDialogue.Type == DialogueType.MultipleChoice)
    {
        _isWaitingForChoice = true;
        Debug.Log($"<color=red>[ProgressDialogue]</color> BLOCKED: Multiple choice detected, not progressing");
        return;
    }

    Dialogue nextDialogue = _nowDialogue.GetNextDialogue();
    Debug.Log($"<color=magenta>[ProgressDialogue]</color> Getting next dialogue: {(nextDialogue != null ? nextDialogue.Name : "NULL")}");
    SetDialogue(nextDialogue);
}

    public void StopDialogue()
    {
        _nowDialogue = null;
        _isWaitingForChoice = false;
        _isWaitingForInk = false;

        if (_dialogueUI != null)
        {
            _dialogueUI.EndDialogue();
        }

        DialogueEnded?.Invoke();
    }

    public void EndDialogue()
    {
        StopDialogue();
    }
    
    // ============================================
    // == EXTERNAL FUNCTION PROCESSING ==
    // ============================================

    private void ProcessExternalFunction(Dialogue dialogue)
    {
        if (dialogue.Type != DialogueType.ExternalFunction)
            return;

        Debug.Log($"<color=cyan>[DialogueManager]</color> External Function Triggered!");
        Debug.Log($"<color=yellow>Function Type:</color> {dialogue.ExternalFunctionType}");
        
        if (dialogue.ExternalFunctionType == ExternalFunctionType.Custom)
        {
            Debug.Log($"<color=yellow>Custom Function Name:</color> {dialogue.CustomFunctionName}");
            OnCustomFunction?.Invoke(dialogue.CustomFunctionName);
            ExecuteCustomFunction(dialogue.CustomFunctionName);
        }
        else
        {
            Debug.Log($"<color=green>Predefined Function:</color> {dialogue.ExternalFunctionType}");
            OnExternalFunction?.Invoke(dialogue.ExternalFunctionType);
            ExecuteExternalFunction(dialogue.ExternalFunctionType, dialogue.CustomFunctionName);
        }
    }

    private void ExecuteExternalFunction(ExternalFunctionType functionType, string customFunctionName)
    {
        switch (functionType)
        {
            case ExternalFunctionType.PlayEmote:
                PlayEmote(customFunctionName);
                break;
                
            case ExternalFunctionType.PausePlayer:
                PausePlayer();
                break;
                
            case ExternalFunctionType.ResumePlayer:
                ResumePlayer();
                break;
                
            case ExternalFunctionType.GiveItem:
                GiveItem(customFunctionName);
                break;
                
            case ExternalFunctionType.RemoveItem:
                RemoveItem(customFunctionName);
                break;
                
            case ExternalFunctionType.PlayAnimation:
                // Parse animation data from customFunctionName (format: "AnimatorName:AnimationName")
                PlayAnimation(customFunctionName);
                break;
                
            case ExternalFunctionType.PlaySound:
                PlaySound(customFunctionName);
                break;
                
            case ExternalFunctionType.UpdateQuest:
                // Parse quest data from customFunctionName (format: "QuestName:Progress")
                UpdateQuest(customFunctionName);
                break;
                
            case ExternalFunctionType.TeleportPlayer:
                // Parse position from customFunctionName (format: "x,y,z")
                TeleportPlayer(customFunctionName);
                break;
                
            case ExternalFunctionType.SpawnNPC:
                // Parse NPC data from customFunctionName (format: "NPCName:x,y,z")
                SpawnNPC(customFunctionName);
                break;
                
            case ExternalFunctionType.ShowUI:
                ShowUI(customFunctionName);
                break;
                
            case ExternalFunctionType.HideUI:
                HideUI(customFunctionName);
                break;
                
            case ExternalFunctionType.SetVariable:
                // Parse variable data from customFunctionName (format: "VarName:Value")
                SetVariable(customFunctionName);
                break;
                
            case ExternalFunctionType.TriggerEvent:
                TriggerEvent(customFunctionName);
                break;
                
            case ExternalFunctionType.Custom:
                ExecuteCustomFunction(customFunctionName);
                break;
                
            default:
                Debug.LogWarning($"Unknown external function type: {functionType}");
                break;
        }
    }

    // ============================================
    // == INDIVIDUAL FUNCTION IMPLEMENTATIONS ==
    // ============================================

    private void PlayEmote(string emoteName)
    {
        Debug.Log($"<color=magenta>[External Function]</color> Play Emote: {emoteName}");
        
        if (playerEmoteAnimator != null && !string.IsNullOrEmpty(emoteName))
        {
            playerEmoteAnimator.SetTrigger(emoteName);
        }
        else
        {
            Debug.LogWarning("Player Emote Animator not assigned or emote name is empty!");
        }
    }

    private void PausePlayer()
    {
        Debug.Log($"<color=magenta>[External Function]</color> Pause Player");
        
        if (player != null)
        {
            // Disable player movement scripts
            var playerController = player.GetComponent<MonoBehaviour>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }
            
            // Or use Time.timeScale for global pause
            // Time.timeScale = 0f;
        }
    }

    private void ResumePlayer()
    {
        Debug.Log($"<color=magenta>[External Function]</color> Resume Player");
        
        if (player != null)
        {
            // Enable player movement scripts
            var playerController = player.GetComponent<MonoBehaviour>();
            if (playerController != null)
            {
                playerController.enabled = true;
            }
            
            // Or restore time scale
            // Time.timeScale = 1f;
        }
    }

    private void GiveItem(string itemName)
    {
        Debug.Log($"<color=magenta>[External Function]</color> Give Item: {itemName}");
        OnGiveItem?.Invoke(itemName);
        
        // TODO: Integrate with your inventory system
        // Example: InventoryManager.Instance.AddItem(itemName);
    }

    private void RemoveItem(string itemName)
    {
        Debug.Log($"<color=magenta>[External Function]</color> Remove Item: {itemName}");
        OnRemoveItem?.Invoke(itemName);
        
        // TODO: Integrate with your inventory system
        // Example: InventoryManager.Instance.RemoveItem(itemName);
    }

    private void PlayAnimation(string animationData)
    {
        Debug.Log($"<color=magenta>[External Function]</color> Play Animation: {animationData}");
        
        // Parse format: "AnimatorName:AnimationName"
        string[] parts = animationData.Split(':');
        if (parts.Length >= 2)
        {
            string animatorName = parts[0].Trim();
            string animationName = parts[1].Trim();
            
            OnPlayAnimation?.Invoke(animatorName, animationName);
            
            // TODO: Find animator by name and play animation
            // GameObject animatorObject = GameObject.Find(animatorName);
            // if (animatorObject != null)
            // {
            //     Animator animator = animatorObject.GetComponent<Animator>();
            //     animator?.SetTrigger(animationName);
            // }
        }
        else
        {
            Debug.LogWarning($"Invalid animation data format: {animationData}. Expected 'AnimatorName:AnimationName'");
        }
    }

    private void PlaySound(string soundName)
    {
        Debug.Log($"<color=magenta>[External Function]</color> Play Sound: {soundName}");
        OnPlaySound?.Invoke(soundName);
        
        if (audioSource != null)
        {
            // TODO: Load audio clip by name and play
            // AudioClip clip = Resources.Load<AudioClip>($"Sounds/{soundName}");
            // if (clip != null)
            // {
            //     audioSource.PlayOneShot(clip);
            // }
        }
    }

    private void UpdateQuest(string questData)
    {
        Debug.Log($"<color=magenta>[External Function]</color> Update Quest: {questData}");
        
        // Parse format: "QuestName:Progress"
        string[] parts = questData.Split(':');
        if (parts.Length >= 2)
        {
            string questName = parts[0].Trim();
            if (int.TryParse(parts[1].Trim(), out int progress))
            {
                OnUpdateQuest?.Invoke(questName, progress);
                
                // TODO: Integrate with your quest system
                // Example: QuestManager.Instance.UpdateQuest(questName, progress);
            }
        }
        else
        {
            Debug.LogWarning($"Invalid quest data format: {questData}. Expected 'QuestName:Progress'");
        }
    }

    private void TeleportPlayer(string positionData)
    {
        Debug.Log($"<color=magenta>[External Function]</color> Teleport Player: {positionData}");
        
        // Parse format: "x,y,z"
        string[] parts = positionData.Split(',');
        if (parts.Length >= 3)
        {
            if (float.TryParse(parts[0].Trim(), out float x) &&
                float.TryParse(parts[1].Trim(), out float y) &&
                float.TryParse(parts[2].Trim(), out float z))
            {
                Vector3 targetPosition = new Vector3(x, y, z);
                OnTeleportPlayer?.Invoke(targetPosition);
                
                if (player != null)
                {
                    player.transform.position = targetPosition;
                }
            }
        }
        else
        {
            Debug.LogWarning($"Invalid position data format: {positionData}. Expected 'x,y,z'");
        }
    }

    private void SpawnNPC(string npcData)
    {
        Debug.Log($"<color=magenta>[External Function]</color> Spawn NPC: {npcData}");
        
        // Parse format: "NPCName:x,y,z"
        string[] parts = npcData.Split(':');
        if (parts.Length >= 2)
        {
            string npcName = parts[0].Trim();
            string[] posParts = parts[1].Split(',');
            
            if (posParts.Length >= 3 &&
                float.TryParse(posParts[0].Trim(), out float x) &&
                float.TryParse(posParts[1].Trim(), out float y) &&
                float.TryParse(posParts[2].Trim(), out float z))
            {
                Vector3 spawnPosition = new Vector3(x, y, z);
                OnSpawnNPC?.Invoke(npcName, spawnPosition);
                
                // TODO: Integrate with your NPC spawning system
                // Example: NPCManager.Instance.SpawnNPC(npcName, spawnPosition);
            }
        }
        else
        {
            Debug.LogWarning($"Invalid NPC data format: {npcData}. Expected 'NPCName:x,y,z'");
        }
    }

    private void ShowUI(string uiName)
    {
        Debug.Log($"<color=magenta>[External Function]</color> Show UI: {uiName}");
        OnShowUI?.Invoke(uiName);
        
        // TODO: Integrate with your UI system
        // Example: UIManager.Instance.ShowPanel(uiName);
    }

    private void HideUI(string uiName)
    {
        Debug.Log($"<color=magenta>[External Function]</color> Hide UI: {uiName}");
        OnHideUI?.Invoke(uiName);
        
        // TODO: Integrate with your UI system
        // Example: UIManager.Instance.HidePanel(uiName);
    }

    private void SetVariable(string variableData)
    {
        Debug.Log($"<color=magenta>[External Function]</color> Set Variable: {variableData}");
        
        // Parse format: "VarName:Value"
        string[] parts = variableData.Split(':');
        if (parts.Length >= 2)
        {
            string varName = parts[0].Trim();
            string varValue = parts[1].Trim();
            
            OnSetVariable?.Invoke(varName, varValue);
            
            // TODO: Integrate with your variable system
            // Example: DialogueVariableManager.Instance.SetVariable(varName, varValue);
        }
        else
        {
            Debug.LogWarning($"Invalid variable data format: {variableData}. Expected 'VarName:Value'");
        }
    }

    private void TriggerEvent(string eventName)
    {
        Debug.Log($"<color=magenta>[External Function]</color> Trigger Event: {eventName}");
        OnTriggerEvent?.Invoke(eventName);
        
        // TODO: Integrate with your event system
        // Example: EventManager.Instance.TriggerEvent(eventName);
    }

    private void ExecuteCustomFunction(string functionName)
    {
        Debug.Log($"<color=magenta>[External Function]</color> Custom Function: {functionName}");
        
        // This allows for completely custom functionality
        // You can implement your own function routing here
        // Example: CustomFunctionRouter.Instance.Execute(functionName);
    }

    // ============================================
    // == VARIABLE PROCESSING ==
    // ============================================

    private void ProcessModifyVariable(Dialogue dialogue)
    {
        if (DialogueVariableManager.Instance != null)
        {
            DialogueVariableManager.Instance.ModifyVariable(dialogue);
        }
        else
        {
            Debug.LogWarning("[DialogueManager] DialogueVariableManager not found! Variable modification skipped.");
        }
        
        Debug.Log($"<color=orange>VAR MODIFY:</color> '{dialogue.VariableName}' {dialogue.ModificationType} {GetVariableValueString(dialogue, dialogue.VariableType)}");
    }

    private Dialogue ProcessVariableCondition(Dialogue dialogue)
    {
        bool conditionMet = false;
        
        if (DialogueVariableManager.Instance != null)
        {
            conditionMet = DialogueVariableManager.Instance.CheckCondition(dialogue);
        }
        else
        {
            Debug.LogWarning("[DialogueManager] DialogueVariableManager not found! Condition check defaulting to false.");
        }
        
        Debug.Log($"<color=lightblue>VAR CONDITION:</color> '{dialogue.VariableName}' {dialogue.ConditionType} {GetVariableValueString(dialogue, dialogue.VariableType)} -> {conditionMet}");

        if (conditionMet)
        {
            return dialogue.Choices[0].NextDialogue;
        }
        else
        {
            if (dialogue.Choices.Count > 1)
            {
                return dialogue.Choices[1].NextDialogue;
            }
            else
            {
                return null;
            }
        }
    }

    private void LogVariableNodeDebug(Dialogue dialogue)
    {
        try
        {
            switch (dialogue.Type)
            {
                case DialogueType.ModifyVariable:
                {
                    string varName = dialogue.VariableName;
                    VariableDataType varType = dialogue.VariableType;
                    ModificationType modType = dialogue.ModificationType; 
                    string valueStr = GetVariableValueString(dialogue, varType); 
                    
                    Debug.Log($"<color=orange>[DialogueManager]</color> <b>Modify Variable Node</b>" +
                              $"\n<color=yellow>Variable Name:</color> {varName}" +
                              $"\n<color=yellow>Action:</color> {modType}" +
                              $"\n<color=yellow>Value:</color> {valueStr}");
                    break;
                }

                case DialogueType.VariableCondition:
                {
                    string varName = dialogue.VariableName;
                    VariableDataType varType = dialogue.VariableType;
                    ConditionType condition = dialogue.ConditionType; 
                    string targetValueStr = GetVariableValueString(dialogue, varType); 

                    Debug.Log($"<color=lightblue>[DialogueManager]</color> <b>Variable Condition Node</b>" +
                              $"\n<color=yellow>Variable Name:</color> {varName}" +
                              $"\n<color=yellow>Condition:</color> {condition}" +
                              $"\n<color=yellow>Target Value:</color> {targetValueStr}");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[DialogueManager] Failed to log variable debug info. Error: {ex.Message}");
        }
    }
    
    private string GetVariableValueString(Dialogue dialogue, VariableDataType type)
    {
        switch (type)
        {
            case VariableDataType.Bool:
                return dialogue.BoolValue.ToString();
            case VariableDataType.Int:
                return dialogue.IntValue.ToString();
            case VariableDataType.Float:
                return dialogue.FloatValue.ToString();
            case VariableDataType.String:
                return $"\"{dialogue.StringValue}\"";
            default:
                return "[Unknown Type]";
        }
    }
}