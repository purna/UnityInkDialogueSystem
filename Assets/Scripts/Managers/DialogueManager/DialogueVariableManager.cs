using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using System.Linq;

/// <summary>
/// Manages dialogue variables across both graph-based and Ink dialogue systems
/// Integrates with DialogueVariable ScriptableObjects
/// </summary>
public class DialogueVariableManager : MonoBehaviour
{
    [Header("Variable Configuration")]
    [Tooltip("List of all dialogue variables used in the game")]
    [SerializeField] private List<DialogueVariable> _registeredVariables = new List<DialogueVariable>();
    
    [Header("Ink Integration")]
    [Tooltip("JSON file containing Ink global variables")]
    [SerializeField] private TextAsset _inkGlobalsJSON;
    
    [Header("Save/Load Settings")]
    [Tooltip("Enable persistent variable saving")]
    [SerializeField] private bool _enablePersistence = true;
    
    [Tooltip("PlayerPrefs key for saving variables")]
    [SerializeField] private string _saveKey = "DIALOGUE_VARIABLES";

    // Runtime storage
    private Dictionary<string, object> _variableValues = new Dictionary<string, object>();
    private Dictionary<string, VariableDataType> _variableTypes = new Dictionary<string, VariableDataType>();
    
    // Ink integration
    private Story _globalVariablesStory;
    private Dictionary<string, Ink.Runtime.Object> _inkVariables = new Dictionary<string, Ink.Runtime.Object>();

    // Singleton
    private static DialogueVariableManager _instance;
    public static DialogueVariableManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DialogueVariableManager>();
            }
            return _instance;
        }
    }

    // Events
    public event System.Action<string, object> OnVariableChanged;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeVariables();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initialize all variables from ScriptableObjects and Ink globals
    /// </summary>
    private void InitializeVariables()
    {
        Debug.Log("[DialogueVariableManager] Initializing dialogue variables...");

        // Clear existing data
        _variableValues.Clear();
        _variableTypes.Clear();
        _inkVariables.Clear();

        // Load from Ink globals if available
        if (_inkGlobalsJSON != null)
        {
            InitializeInkGlobals();
        }

        // Register all DialogueVariable ScriptableObjects
        foreach (var variable in _registeredVariables)
        {
            if (variable == null) continue;

            RegisterVariable(
                variable.VariableName,
                variable.Type,
                variable.GetDefaultValue()
            );
        }

        // Load saved data if persistence is enabled
        if (_enablePersistence)
        {
            LoadVariables();
        }

        Debug.Log($"[DialogueVariableManager] Initialized {_variableValues.Count} variables");
    }

    /// <summary>
    /// Initialize Ink global variables
    /// </summary>
    private void InitializeInkGlobals()
    {
        try
        {
            _globalVariablesStory = new Story(_inkGlobalsJSON.text);

            foreach (string name in _globalVariablesStory.variablesState)
            {
                Ink.Runtime.Object inkValue = _globalVariablesStory.variablesState.GetVariableWithName(name);
                _inkVariables.Add(name, inkValue);

                // Convert Ink variable to managed variable
                VariableDataType type = GetTypeFromInkValue(inkValue);
                object value = ConvertInkValue(inkValue, type);

                RegisterVariable(name, type, value);
                
                Debug.Log($"[DialogueVariableManager] Loaded Ink variable: {name} = {value} ({type})");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DialogueVariableManager] Failed to load Ink globals: {e.Message}");
        }
    }

    /// <summary>
    /// Register a new variable or update existing one
    /// </summary>
    public void RegisterVariable(string variableName, VariableDataType type, object defaultValue)
    {
        if (string.IsNullOrEmpty(variableName))
        {
            Debug.LogWarning("[DialogueVariableManager] Cannot register variable with empty name");
            return;
        }

        // Register type
        if (!_variableTypes.ContainsKey(variableName))
        {
            _variableTypes.Add(variableName, type);
        }

        // Set default value if not already set
        if (!_variableValues.ContainsKey(variableName))
        {
            _variableValues.Add(variableName, defaultValue);
            Debug.Log($"[DialogueVariableManager] Registered variable: {variableName} = {defaultValue} ({type})");
        }
    }

    /// <summary>
    /// Get a variable's value
    /// </summary>
    public object GetVariable(string variableName)
    {
        if (_variableValues.TryGetValue(variableName, out object value))
        {
            return value;
        }

        Debug.LogWarning($"[DialogueVariableManager] Variable '{variableName}' not found");
        return null;
    }

    /// <summary>
    /// Get a typed variable value
    /// </summary>
    public T GetVariable<T>(string variableName)
    {
        object value = GetVariable(variableName);
        
        if (value == null)
            return default(T);

        try
        {
            return (T)System.Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            Debug.LogWarning($"[DialogueVariableManager] Cannot convert variable '{variableName}' to type {typeof(T)}");
            return default(T);
        }
    }

    /// <summary>
    /// Set a variable's value
    /// </summary>
    public void SetVariable(string variableName, object value)
    {
        if (!_variableTypes.ContainsKey(variableName))
        {
            Debug.LogWarning($"[DialogueVariableManager] Variable '{variableName}' is not registered");
            return;
        }

        // Validate type
        VariableDataType expectedType = _variableTypes[variableName];
        if (!IsValueValidForType(value, expectedType))
        {
            Debug.LogWarning($"[DialogueVariableManager] Type mismatch for variable '{variableName}'. Expected {expectedType}");
            return;
        }

        // Update value
        if (_variableValues.ContainsKey(variableName))
        {
            _variableValues[variableName] = value;
        }
        else
        {
            _variableValues.Add(variableName, value);
        }

        // Notify listeners
        OnVariableChanged?.Invoke(variableName, value);

        Debug.Log($"[DialogueVariableManager] Set variable: {variableName} = {value}");
    }

    /// <summary>
    /// Modify a variable using the dialogue system's modification types
    /// </summary>
    public void ModifyVariable(Dialogue dialogue)
    {
        if (dialogue == null || string.IsNullOrEmpty(dialogue.VariableName))
            return;

        string varName = dialogue.VariableName;
        
        if (!_variableValues.ContainsKey(varName))
        {
            Debug.LogWarning($"[DialogueVariableManager] Variable '{varName}' not found for modification");
            return;
        }

        object currentValue = _variableValues[varName];
        object newValue = null;

        switch (dialogue.VariableType)
        {
            case VariableDataType.Bool:
                newValue = ModifyBoolVariable(currentValue, dialogue.ModificationType, dialogue.BoolValue);
                break;
            case VariableDataType.Int:
                newValue = ModifyIntVariable(currentValue, dialogue.ModificationType, dialogue.IntValue);
                break;
            case VariableDataType.Float:
                newValue = ModifyFloatVariable(currentValue, dialogue.ModificationType, dialogue.FloatValue);
                break;
            case VariableDataType.String:
                newValue = dialogue.StringValue;
                break;
        }

        if (newValue != null)
        {
            SetVariable(varName, newValue);
        }
    }

    /// <summary>
    /// Check a condition against a variable
    /// </summary>
    public bool CheckCondition(Dialogue dialogue)
    {
        if (dialogue == null || string.IsNullOrEmpty(dialogue.VariableName))
            return false;

        string varName = dialogue.VariableName;
        
        if (!_variableValues.ContainsKey(varName))
        {
            Debug.LogWarning($"[DialogueVariableManager] Variable '{varName}' not found for condition check");
            return false;
        }

        object currentValue = _variableValues[varName];

        switch (dialogue.VariableType)
        {
            case VariableDataType.Bool:
                return CheckBoolCondition(currentValue, dialogue.ConditionType, dialogue.BoolValue);
            case VariableDataType.Int:
                return CheckIntCondition(currentValue, dialogue.ConditionType, dialogue.IntValue);
            case VariableDataType.Float:
                return CheckFloatCondition(currentValue, dialogue.ConditionType, dialogue.FloatValue);
            case VariableDataType.String:
                return CheckStringCondition(currentValue, dialogue.ConditionType, dialogue.StringValue);
        }

        return false;
    }

    /// <summary>
    /// Sync variables to an Ink Story
    /// </summary>
    public void SyncToInkStory(Story story)
    {
        if (story == null) return;

        foreach (var kvp in _variableValues)
        {
            try
            {
                // Convert to Ink value
                Ink.Runtime.Object inkValue = ConvertToInkValue(kvp.Value, _variableTypes[kvp.Key]);
                
                if (inkValue != null)
                {
                    story.variablesState.SetGlobal(kvp.Key, inkValue);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[DialogueVariableManager] Failed to sync variable '{kvp.Key}' to Ink: {e.Message}");
            }
        }

        Debug.Log($"[DialogueVariableManager] Synced {_variableValues.Count} variables to Ink story");
    }

    /// <summary>
    /// Sync variables from an Ink Story
    /// </summary>
    public void SyncFromInkStory(Story story)
    {
        if (story == null) return;

        int syncedCount = 0;
        
        foreach (string varName in story.variablesState)
        {
            if (_variableValues.ContainsKey(varName))
            {
                Ink.Runtime.Object inkValue = story.variablesState.GetVariableWithName(varName);
                object value = ConvertInkValue(inkValue, _variableTypes[varName]);
                
                if (value != null)
                {
                    _variableValues[varName] = value;
                    OnVariableChanged?.Invoke(varName, value);
                    syncedCount++;
                }
            }
        }

        Debug.Log($"[DialogueVariableManager] Synced {syncedCount} variables from Ink story");
    }

    /// <summary>
    /// Save all variables to PlayerPrefs
    /// </summary>
    public void SaveVariables()
    {
        if (!_enablePersistence) return;

        var saveData = new VariableSaveData
        {
            variables = _variableValues.Select(kvp => new VariableEntry
            {
                name = kvp.Key,
                type = _variableTypes[kvp.Key],
                value = kvp.Value?.ToString() ?? ""
            }).ToList()
        };

        string json = JsonUtility.ToJson(saveData, true);
        PlayerPrefs.SetString(_saveKey, json);
        PlayerPrefs.Save();

        Debug.Log($"[DialogueVariableManager] Saved {saveData.variables.Count} variables");
    }

    /// <summary>
    /// Load all variables from PlayerPrefs
    /// </summary>
    public void LoadVariables()
    {
        if (!_enablePersistence || !PlayerPrefs.HasKey(_saveKey))
            return;

        try
        {
            string json = PlayerPrefs.GetString(_saveKey);
            var saveData = JsonUtility.FromJson<VariableSaveData>(json);

            int loadedCount = 0;
            
            foreach (var entry in saveData.variables)
            {
                if (_variableValues.ContainsKey(entry.name))
                {
                    object value = ParseValueFromString(entry.value, entry.type);
                    _variableValues[entry.name] = value;
                    loadedCount++;
                }
            }

            Debug.Log($"[DialogueVariableManager] Loaded {loadedCount} variables");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DialogueVariableManager] Failed to load variables: {e.Message}");
        }
    }

    /// <summary>
    /// Clear all saved data
    /// </summary>
    public void ClearSaveData()
    {
        PlayerPrefs.DeleteKey(_saveKey);
        Debug.Log("[DialogueVariableManager] Cleared saved variables");
    }

    /// <summary>
    /// Reset all variables to their default values
    /// </summary>
    public void ResetAllVariables()
    {
        foreach (var variable in _registeredVariables)
        {
            if (variable == null) continue;
            SetVariable(variable.VariableName, variable.GetDefaultValue());
        }

        Debug.Log("[DialogueVariableManager] Reset all variables to defaults");
    }

    // ============================================
    // PRIVATE HELPER METHODS
    // ============================================

    private bool ModifyBoolVariable(object current, ModificationType modType, bool value)
    {
        return value; // Bool is always "Set"
    }

    private int ModifyIntVariable(object current, ModificationType modType, int value)
    {
        int currentInt = System.Convert.ToInt32(current);
        
        return modType switch
        {
            ModificationType.Add => currentInt + value,
            ModificationType.Subtract => currentInt - value,
            ModificationType.Multiply => currentInt * value,
            ModificationType.Divide => value != 0 ? currentInt / value : currentInt,
            ModificationType.Set => value,
            _ => currentInt
        };
    }

    private float ModifyFloatVariable(object current, ModificationType modType, float value)
    {
        float currentFloat = System.Convert.ToSingle(current);
        
        return modType switch
        {
            ModificationType.Add => currentFloat + value,
            ModificationType.Subtract => currentFloat - value,
            ModificationType.Multiply => currentFloat * value,
            ModificationType.Divide => value != 0 ? currentFloat / value : currentFloat,
            ModificationType.Set => value,
            _ => currentFloat
        };
    }

    private bool CheckBoolCondition(object current, ConditionType condType, bool targetValue)
    {
        bool currentBool = System.Convert.ToBoolean(current);
        return condType switch
        {
            ConditionType.Equals => currentBool == targetValue,
            ConditionType.NotEquals => currentBool != targetValue,
            _ => false
        };
    }

    private bool CheckIntCondition(object current, ConditionType condType, int targetValue)
    {
        int currentInt = System.Convert.ToInt32(current);
        
        return condType switch
        {
            ConditionType.Equals => currentInt == targetValue,
            ConditionType.NotEquals => currentInt != targetValue,
            ConditionType.GreaterThan => currentInt > targetValue,
            ConditionType.LessThan => currentInt < targetValue,
            ConditionType.GreaterThanOrEqual => currentInt >= targetValue,
            ConditionType.LessThanOrEqual => currentInt <= targetValue,
            _ => false
        };
    }

    private bool CheckFloatCondition(object current, ConditionType condType, float targetValue)
    {
        float currentFloat = System.Convert.ToSingle(current);
        
        return condType switch
        {
            ConditionType.Equals => Mathf.Approximately(currentFloat, targetValue),
            ConditionType.NotEquals => !Mathf.Approximately(currentFloat, targetValue),
            ConditionType.GreaterThan => currentFloat > targetValue,
            ConditionType.LessThan => currentFloat < targetValue,
            ConditionType.GreaterThanOrEqual => currentFloat >= targetValue,
            ConditionType.LessThanOrEqual => currentFloat <= targetValue,
            _ => false
        };
    }

    private bool CheckStringCondition(object current, ConditionType condType, string targetValue)
    {
        string currentString = current?.ToString() ?? "";
        return condType == ConditionType.Equals ? currentString == targetValue : currentString != targetValue;
    }

    private bool IsValueValidForType(object value, VariableDataType type)
    {
        if (value == null) return type == VariableDataType.String;

        return type switch
        {
            VariableDataType.Bool => value is bool,
            VariableDataType.Int => value is int,
            VariableDataType.Float => value is float or double,
            VariableDataType.String => value is string,
            _ => false
        };
    }

    private VariableDataType GetTypeFromInkValue(Ink.Runtime.Object inkValue)
    {
        if (inkValue is IntValue) return VariableDataType.Int;
        if (inkValue is FloatValue) return VariableDataType.Float;
        if (inkValue is StringValue) return VariableDataType.String;
        if (inkValue is BoolValue) return VariableDataType.Bool;
        return VariableDataType.String;
    }

    private object ConvertInkValue(Ink.Runtime.Object inkValue, VariableDataType targetType)
    {
        if (inkValue == null) return null;

        return targetType switch
        {
            VariableDataType.Bool => ((BoolValue)inkValue).value,
            VariableDataType.Int => ((IntValue)inkValue).value,
            VariableDataType.Float => ((FloatValue)inkValue).value,
            VariableDataType.String => ((StringValue)inkValue).value,
            _ => null
        };
    }

    private Ink.Runtime.Object ConvertToInkValue(object value, VariableDataType type)
    {
        if (value == null) return null;

        return type switch
        {
            VariableDataType.Bool => new BoolValue(System.Convert.ToBoolean(value)),
            VariableDataType.Int => new IntValue(System.Convert.ToInt32(value)),
            VariableDataType.Float => new FloatValue(System.Convert.ToSingle(value)),
            VariableDataType.String => new StringValue(value.ToString()),
            _ => null
        };
    }

    private object ParseValueFromString(string valueStr, VariableDataType type)
    {
        return type switch
        {
            VariableDataType.Bool => bool.Parse(valueStr),
            VariableDataType.Int => int.Parse(valueStr),
            VariableDataType.Float => float.Parse(valueStr),
            VariableDataType.String => valueStr,
            _ => null
        };
    }

    private void OnApplicationQuit()
    {
        if (_enablePersistence)
        {
            SaveVariables();
        }
    }
}

// ============================================
// SAVE DATA STRUCTURES
// ============================================

[System.Serializable]
public class VariableSaveData
{
    public List<VariableEntry> variables = new List<VariableEntry>();
}

[System.Serializable]
public class VariableEntry
{
    public string name;
    public VariableDataType type;
    public string value;
}