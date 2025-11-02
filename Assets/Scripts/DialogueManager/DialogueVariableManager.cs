using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime manager for dialogue variables (Singleton)
/// </summary>
public class DialogueVariableManager : MonoBehaviour
{
    private static DialogueVariableManager _instance;
    public static DialogueVariableManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DialogueVariableManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("DialogueVariableManager");
                    _instance = go.AddComponent<DialogueVariableManager>();
                }
            }
            return _instance;
        }
    }

    [Header("Variable Definitions")]
    [SerializeField] private DialogueVariablesContainer _variablesContainer;

    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = true;

    // Runtime storage for variable values
    private Dictionary<string, object> _runtimeVariables = new();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
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
    private void InitializeVariables()
    {
        if (_variablesContainer == null)
        {
            Debug.LogWarning("[DialogueVariableManager] DialogueVariablesContainer not assigned!");
            return;
        }

        _runtimeVariables.Clear();
        foreach (var variable in _variablesContainer.Variables)
        {
            if (variable != null)
            {
                _runtimeVariables[variable.VariableName] = variable.GetDefaultValue();
            }
        }

        if (_showDebugLogs)
            Debug.Log($"<color=green>[DialogueVariableManager]</color> Initialized {_runtimeVariables.Count} variables");
    }

    /// <summary>
    /// Reset all variables to their default values
    /// </summary>
    public void ResetAllVariables()
    {
        InitializeVariables();
    }

    /// <summary>
    /// Get a variable's current value
    /// </summary>
    public object GetVariable(string variableName)
    {
        if (_runtimeVariables.ContainsKey(variableName))
            return _runtimeVariables[variableName];

        Debug.LogWarning($"[DialogueVariableManager] Variable '{variableName}' not found!");
        return null;
    }

    /// <summary>
    /// Get a typed variable value
    /// </summary>
    public T GetVariable<T>(string variableName)
    {
        object value = GetVariable(variableName);
        if (value is T typedValue)
            return typedValue;

        Debug.LogWarning($"[DialogueVariableManager] Variable '{variableName}' is not of type {typeof(T)}!");
        return default;
    }

    /// <summary>
    /// Set a variable's value
    /// </summary>
    public void SetVariable(string variableName, object value)
    {
        if (!_runtimeVariables.ContainsKey(variableName))
        {
            Debug.LogWarning($"[DialogueVariableManager] Variable '{variableName}' not found! Creating it.");
            _runtimeVariables[variableName] = value;
            return;
        }

        _runtimeVariables[variableName] = value;

        if (_showDebugLogs)
            Debug.Log($"<color=yellow>[DialogueVariableManager]</color> Set '{variableName}' = {value}");
    }

    /// <summary>
    /// Modify a variable based on dialogue node data
    /// </summary>
    public void ModifyVariable(Dialogue dialogue)
    {
        if (dialogue.Type != DialogueType.ModifyVariable)
            return;

        string varName = dialogue.VariableName;
        VariableDataType varType = dialogue.VariableType;
        ModificationType modType = dialogue.ModificationType;

        switch (varType)
        {
            case VariableDataType.Bool:
                ModifyBoolVariable(varName, modType, dialogue.BoolValue);
                break;

            case VariableDataType.Int:
                ModifyIntVariable(varName, modType, dialogue.IntValue);
                break;

            case VariableDataType.Float:
                ModifyFloatVariable(varName, modType, dialogue.FloatValue);
                break;

            case VariableDataType.String:
                ModifyStringVariable(varName, modType, dialogue.StringValue);
                break;
        }
    }

    private void ModifyBoolVariable(string varName, ModificationType modType, bool value)
    {
        switch (modType)
        {
            case ModificationType.Set:
                SetVariable(varName, value);
                break;

            case ModificationType.Toggle:
                bool currentValue = GetVariable<bool>(varName);
                SetVariable(varName, !currentValue);
                break;

            default:
                Debug.LogWarning($"[DialogueVariableManager] Modification type '{modType}' not valid for Bool variables!");
                break;
        }
    }

    private void ModifyIntVariable(string varName, ModificationType modType, int value)
    {
        switch (modType)
        {
            case ModificationType.Set:
                SetVariable(varName, value);
                break;

            case ModificationType.Increase:
                int currentInt = GetVariable<int>(varName);
                SetVariable(varName, currentInt + value);
                break;

            case ModificationType.Decrease:
                int currentInt2 = GetVariable<int>(varName);
                SetVariable(varName, currentInt2 - value);
                break;

            default:
                Debug.LogWarning($"[DialogueVariableManager] Modification type '{modType}' not valid for Int variables!");
                break;
        }
    }

    private void ModifyFloatVariable(string varName, ModificationType modType, float value)
    {
        switch (modType)
        {
            case ModificationType.Set:
                SetVariable(varName, value);
                break;

            case ModificationType.Increase:
                float currentFloat = GetVariable<float>(varName);
                SetVariable(varName, currentFloat + value);
                break;

            case ModificationType.Decrease:
                float currentFloat2 = GetVariable<float>(varName);
                SetVariable(varName, currentFloat2 - value);
                break;

            default:
                Debug.LogWarning($"[DialogueVariableManager] Modification type '{modType}' not valid for Float variables!");
                break;
        }
    }

    private void ModifyStringVariable(string varName, ModificationType modType, string value)
    {
        switch (modType)
        {
            case ModificationType.Set:
                SetVariable(varName, value);
                break;

            default:
                Debug.LogWarning($"[DialogueVariableManager] Modification type '{modType}' not valid for String variables!");
                break;
        }
    }

    /// <summary>
    /// Check a condition and return true/false
    /// </summary>
    public bool CheckCondition(Dialogue dialogue)
    {
        if (dialogue.Type != DialogueType.VariableCondition)
            return false;

        string varName = dialogue.VariableName;
        VariableDataType varType = dialogue.VariableType;
        ConditionType condition = dialogue.ConditionType;

        switch (varType)
        {
            case VariableDataType.Bool:
                return CheckBoolCondition(varName, condition, dialogue.BoolValue);

            case VariableDataType.Int:
                return CheckIntCondition(varName, condition, dialogue.IntValue);

            case VariableDataType.Float:
                return CheckFloatCondition(varName, condition, dialogue.FloatValue);

            case VariableDataType.String:
                return CheckStringCondition(varName, condition, dialogue.StringValue);

            default:
                return false;
        }
    }

    private bool CheckBoolCondition(string varName, ConditionType condition, bool targetValue)
    {
        bool currentValue = GetVariable<bool>(varName);

        return condition switch
        {
            ConditionType.Equal => currentValue == targetValue,
            ConditionType.NotEqual => currentValue != targetValue,
            _ => false
        };
    }

    private bool CheckIntCondition(string varName, ConditionType condition, int targetValue)
    {
        int currentValue = GetVariable<int>(varName);

        return condition switch
        {
            ConditionType.Equal => currentValue == targetValue,
            ConditionType.NotEqual => currentValue != targetValue,
            ConditionType.Greater => currentValue > targetValue,
            ConditionType.GreaterOrEqual => currentValue >= targetValue,
            ConditionType.Less => currentValue < targetValue,
            ConditionType.LessOrEqual => currentValue <= targetValue,
            _ => false
        };
    }

    private bool CheckFloatCondition(string varName, ConditionType condition, float targetValue)
    {
        float currentValue = GetVariable<float>(varName);

        return condition switch
        {
            ConditionType.Equal => Mathf.Approximately(currentValue, targetValue),
            ConditionType.NotEqual => !Mathf.Approximately(currentValue, targetValue),
            ConditionType.Greater => currentValue > targetValue,
            ConditionType.GreaterOrEqual => currentValue >= targetValue,
            ConditionType.Less => currentValue < targetValue,
            ConditionType.LessOrEqual => currentValue <= targetValue,
            _ => false
        };
    }

    private bool CheckStringCondition(string varName, ConditionType condition, string targetValue)
    {
        string currentValue = GetVariable<string>(varName);

        return condition switch
        {
            ConditionType.Equal => currentValue == targetValue,
            ConditionType.NotEqual => currentValue != targetValue,
            _ => false
        };
    }

    /// <summary>
    /// Get all variable names (for editor dropdowns)
    /// </summary>
    public List<string> GetAllVariableNames()
    {
        if (_variablesContainer == null)
            return new List<string>();

        return _variablesContainer.GetVariableNames();
    }

    /// <summary>
    /// Debug: Print all current variable values
    /// </summary>
    [ContextMenu("Debug: Print All Variables")]
    public void DebugPrintAllVariables()
    {
        Debug.Log("<color=cyan>=== DIALOGUE VARIABLES ===</color>");
        foreach (var kvp in _runtimeVariables)
        {
            Debug.Log($"<color=yellow>{kvp.Key}</color> = {kvp.Value}");
        }
    }
}