using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Runtime manager for dialogue variables - handles variable modifications and condition checking
/// This works alongside your existing DialogueManager
/// </summary>
public class DialogueVariableRuntime : MonoBehaviour {
    private static DialogueVariableRuntime _instance;
    public static DialogueVariableRuntime Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<DialogueVariableRuntime>();
                if (_instance == null) {
                    GameObject go = new GameObject("DialogueVariableRuntime");
                    _instance = go.AddComponent<DialogueVariableRuntime>();
                }
            }
            return _instance;
        }
    }

    [Header("Variable Definitions")]
    [Tooltip("Assign your DialogueVariablesContainer ScriptableObject here")]
    public DialogueVariablesContainer variablesContainer;

    [Header("Debug")]
    [Tooltip("Show debug logs when variables are modified")]
    public bool showDebugLogs = true;

    // Runtime storage for variable values
    private Dictionary<string, object> _runtimeVariables = new();

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeVariables();
    }

    /// <summary>
    /// Initialize all variables with their default values
    /// </summary>
    private void InitializeVariables() {
        if (variablesContainer == null) {
            Debug.LogWarning("[DialogueVariableRuntime] DialogueVariablesContainer not assigned!");
            return;
        }

        _runtimeVariables.Clear();
        foreach (var variable in variablesContainer.Variables) {
            if (variable != null) {
                _runtimeVariables[variable.VariableName] = variable.GetDefaultValue();
            }
        }

        if (showDebugLogs)
            Debug.Log($"<color=green>[DialogueVariableRuntime]</color> Initialized {_runtimeVariables.Count} variables");
    }

    /// <summary>
    /// Reset all variables to their default values
    /// </summary>
    public void ResetAllVariables() {
        InitializeVariables();
    }

    /// <summary>
    /// Get a variable's current value
    /// </summary>
    public object GetVariable(string variableName) {
        if (_runtimeVariables.ContainsKey(variableName))
            return _runtimeVariables[variableName];

        Debug.LogWarning($"[DialogueVariableRuntime] Variable '{variableName}' not found!");
        return null;
    }

    /// <summary>
    /// Get a typed variable value
    /// </summary>
    public T GetVariable<T>(string variableName) {
        object value = GetVariable(variableName);
        if (value is T typedValue)
            return typedValue;

        Debug.LogWarning($"[DialogueVariableRuntime] Variable '{variableName}' is not of type {typeof(T)}!");
        return default;
    }

    /// <summary>
    /// Set a variable's value directly
    /// </summary>
    public void SetVariable(string variableName, object value) {
        if (!_runtimeVariables.ContainsKey(variableName)) {
            Debug.LogWarning($"[DialogueVariableRuntime] Variable '{variableName}' not found! Creating it.");
            _runtimeVariables[variableName] = value;
            return;
        }

        _runtimeVariables[variableName] = value;

        if (showDebugLogs)
            Debug.Log($"<color=yellow>[DialogueVariableRuntime]</color> Set '{variableName}' = {value}");
    }

    /// <summary>
    /// Process a modify variable node from DialogueModifyVariableNode
    /// </summary>
    public void ProcessModifyVariableNode(DialogueModifyVariableNode node) {
        if (node == null || node.VariablesContainer == null) {
            Debug.LogError("[DialogueVariableRuntime] Invalid modify variable node!");
            return;
        }

        DialogueVariable variable = node.VariablesContainer.GetVariable(node.VariableName);
        if (variable == null) {
            Debug.LogError($"[DialogueVariableRuntime] Variable '{node.VariableName}' not found in container!");
            return;
        }

        switch (variable.Type) {
            case VariableDataType.Bool:
                ModifyBoolVariable(node);
                break;
            case VariableDataType.Int:
                ModifyIntVariable(node);
                break;
            case VariableDataType.Float:
                ModifyFloatVariable(node);
                break;
            case VariableDataType.String:
                ModifyStringVariable(node);
                break;
        }
    }

    private void ModifyBoolVariable(DialogueModifyVariableNode node) {
        switch (node.Modification) {
            case ModificationType.Set:
                SetVariable(node.VariableName, node.BoolValue);
                break;

            case ModificationType.Toggle:
                bool currentValue = GetVariable<bool>(node.VariableName);
                SetVariable(node.VariableName, !currentValue);
                break;
        }
    }

    private void ModifyIntVariable(DialogueModifyVariableNode node) {
        int currentValue = GetVariable<int>(node.VariableName);

        switch (node.Modification) {
            case ModificationType.Set:
                SetVariable(node.VariableName, node.IntValue);
                break;

            case ModificationType.Increase:
                SetVariable(node.VariableName, currentValue + node.IntValue);
                break;

            case ModificationType.Decrease:
                SetVariable(node.VariableName, currentValue - node.IntValue);
                break;
        }
    }

    private void ModifyFloatVariable(DialogueModifyVariableNode node) {
        float currentValue = GetVariable<float>(node.VariableName);

        switch (node.Modification) {
            case ModificationType.Set:
                SetVariable(node.VariableName, node.FloatValue);
                break;

            case ModificationType.Increase:
                SetVariable(node.VariableName, currentValue + node.FloatValue);
                break;

            case ModificationType.Decrease:
                SetVariable(node.VariableName, currentValue - node.FloatValue);
                break;
        }
    }

    private void ModifyStringVariable(DialogueModifyVariableNode node) {
        switch (node.Modification) {
            case ModificationType.Set:
                SetVariable(node.VariableName, node.StringValue);
                break;
        }
    }

    /// <summary>
    /// Check a condition from DialogueVariableConditionNode and return true/false
    /// </summary>
    public bool CheckCondition(DialogueVariableConditionNode node) {
        if (node == null || node.VariablesContainer == null) {
            Debug.LogError("[DialogueVariableRuntime] Invalid variable condition node!");
            return false;
        }

        DialogueVariable variable = node.VariablesContainer.GetVariable(node.VariableName);
        if (variable == null) {
            Debug.LogError($"[DialogueVariableRuntime] Variable '{node.VariableName}' not found in container!");
            return false;
        }

        switch (variable.Type) {
            case VariableDataType.Bool:
                return CheckBoolCondition(node);
            case VariableDataType.Int:
                return CheckIntCondition(node);
            case VariableDataType.Float:
                return CheckFloatCondition(node);
            case VariableDataType.String:
                return CheckStringCondition(node);
            default:
                return false;
        }
    }

    private bool CheckBoolCondition(DialogueVariableConditionNode node) {
        bool currentValue = GetVariable<bool>(node.VariableName);

        return node.Condition switch {
            ConditionType.Equals => currentValue == node.BoolTargetValue,
            ConditionType.NotEquals => currentValue != node.BoolTargetValue,
            _ => false
        };
    }

    private bool CheckIntCondition(DialogueVariableConditionNode node) {
        int currentValue = GetVariable<int>(node.VariableName);

        return node.Condition switch {
            ConditionType.Equals => currentValue == node.IntTargetValue,
            ConditionType.NotEquals => currentValue != node.IntTargetValue,
            ConditionType.GreaterThan => currentValue > node.IntTargetValue,
            ConditionType.GreaterThanOrEqual => currentValue >= node.IntTargetValue,
            ConditionType.LessThan => currentValue < node.IntTargetValue,
            ConditionType.LessThanOrEqual => currentValue <= node.IntTargetValue,
            _ => false
        };
    }

    private bool CheckFloatCondition(DialogueVariableConditionNode node) {
        float currentValue = GetVariable<float>(node.VariableName);

        return node.Condition switch {
            ConditionType.Equals => Mathf.Approximately(currentValue, node.FloatTargetValue),
            ConditionType.NotEquals => !Mathf.Approximately(currentValue, node.FloatTargetValue),
            ConditionType.GreaterThan => currentValue > node.FloatTargetValue,
            ConditionType.GreaterThanOrEqual => currentValue >= node.FloatTargetValue,
            ConditionType.LessThan => currentValue < node.FloatTargetValue,
            ConditionType.LessThanOrEqual => currentValue <= node.FloatTargetValue,
            _ => false
        };
    }

    private bool CheckStringCondition(DialogueVariableConditionNode node) {
        string currentValue = GetVariable<string>(node.VariableName);

        return node.Condition switch {
            ConditionType.Equals => currentValue == node.StringTargetValue,
            ConditionType.NotEquals => currentValue != node.StringTargetValue,
            _ => false
        };
    }

    /// <summary>
    /// Debug: Print all current variable values
    /// </summary>
    [ContextMenu("Debug: Print All Variables")]
    public void DebugPrintAllVariables() {
        Debug.Log("<color=cyan>=== DIALOGUE VARIABLES ===</color>");
        foreach (var kvp in _runtimeVariables) {
            Debug.Log($"<color=yellow>{kvp.Key}</color> = {kvp.Value}");
        }
    }
}


// ============================================
// INTEGRATION WITH YOUR EXISTING DIALOGUEMANAGER
// ============================================
/*
Add these to your existing DialogueManager.cs in the SetDialogue method:

case DialogueType.ModifyVariable:
    ProcessModifyVariable(dialogue);
    
    // ADD THESE LINES:
    if (dialogue is DialogueModifyVariableNode modifyNode) {
        DialogueVariableRuntime.Instance.ProcessModifyVariableNode(modifyNode);
    }
    
    SetDialogue(dialogue.GetNextDialogue());
    return;

case DialogueType.VariableCondition:
    // ADD THESE LINES:
    bool conditionMet = false;
    if (dialogue is DialogueVariableConditionNode conditionNode) {
        conditionMet = DialogueVariableRuntime.Instance.CheckCondition(conditionNode);
    }
    
    Dialogue nextNode = ProcessVariableCondition(dialogue, conditionMet);
    SetDialogue(nextNode);
    return;

// Then update ProcessVariableCondition to accept the conditionMet parameter:
private Dialogue ProcessVariableCondition(Dialogue dialogue, bool conditionMet)
{
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
*/
