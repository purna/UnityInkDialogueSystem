using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject that defines a dialogue variable
/// </summary>
[CreateAssetMenu(fileName = "DialogueVariable", menuName = "Dialogue System/Variable")]
public class DialogueVariable : ScriptableObject
{
    [SerializeField] private string _variableName;
    [SerializeField] private VariableDataType _type;
    [SerializeField] private string _description;
    
    // Default values
    [SerializeField] private bool _defaultBoolValue;
    [SerializeField] private int _defaultIntValue;
    [SerializeField] private float _defaultFloatValue;
    [SerializeField] private string _defaultStringValue;

    public string VariableName => _variableName;
    public VariableDataType Type => _type;
    public string Description => _description;
    
    public bool DefaultBoolValue => _defaultBoolValue;
    public int DefaultIntValue => _defaultIntValue;
    public float DefaultFloatValue => _defaultFloatValue;
    public string DefaultStringValue => _defaultStringValue;

    // ADD THIS METHOD
    public void Initialize(string variableName, VariableDataType type, object defaultValue, string description = "")
    {
        _variableName = variableName;
        _type = type;
        _description = description;
        
        switch (type)
        {
            case VariableDataType.Bool:
                _defaultBoolValue = defaultValue is bool boolVal ? boolVal : false;
                break;
            case VariableDataType.Int:
                _defaultIntValue = defaultValue is int intVal ? intVal : 0;
                break;
            case VariableDataType.Float:
                _defaultFloatValue = defaultValue is double doubleVal ? (float)doubleVal : 
                                     defaultValue is float floatVal ? floatVal : 0f;
                break;
            case VariableDataType.String:
                _defaultStringValue = defaultValue?.ToString() ?? "";
                break;
        }
    }

    public object GetDefaultValue()
    {
        return _type switch
        {
            VariableDataType.Bool => _defaultBoolValue,
            VariableDataType.Int => _defaultIntValue,
            VariableDataType.Float => _defaultFloatValue,
            VariableDataType.String => _defaultStringValue,
            _ => null
        };
    }
}