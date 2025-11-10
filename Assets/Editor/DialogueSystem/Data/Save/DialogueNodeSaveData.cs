using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueNodeSaveData {
    [SerializeField] private string _ID;
    [SerializeField] private string _name;
    [SerializeField] private string _text;
    [SerializeField] private List<DialogueChoiceSaveData> _choices;
    [SerializeField] private string _groupID;
    [SerializeField] private DialogueType _dialogueType;
    [SerializeField] private Vector2 _position;
    [SerializeField] private DialogueCharacter _character;
    [SerializeField] private DialogueCharacterEmotion _emotion;

    [SerializeField] private DialogueVariablesContainer _variablesContainer;


    // Variable modification/condition fields
    [SerializeField] private string _variableName;
    [SerializeField] private VariableDataType _variableType;
    [SerializeField] private ModificationType _modificationType;
    [SerializeField] private ConditionType _conditionType;
    
    // Variable values for different types
    [SerializeField] private bool _boolValue;
    [SerializeField] private int _intValue;
    [SerializeField] private float _floatValue;
    [SerializeField] private string _stringValue;

    // External function fields
    [SerializeField] private ExternalFunctionType _functionType;
    [SerializeField] private string _customFunctionName;

    // Ink node fields
    [SerializeField] private TextAsset _inkJsonAsset;
    [SerializeField] private string _knotName;
    [SerializeField] private bool _startFromBeginning;

    // Properties for external function and Ink data
    public ExternalFunctionType FunctionType => _functionType;
    public string CustomFunctionName => _customFunctionName;
    public TextAsset InkJsonAsset => _inkJsonAsset;
    public string KnotName => _knotName;
    public bool StartFromBeginning => _startFromBeginning;

    public string ID => _ID;
    public string Name => _name;
    public string Text => _text;
    public IEnumerable<DialogueChoiceSaveData> Choices => _choices;
    public string GroupID => _groupID;
    public DialogueType DialogueType => _dialogueType;
    public Vector2 Position => _position;
    public DialogueCharacter Character => _character;
    public DialogueCharacterEmotion Emotion => _emotion;

    public DialogueVariablesContainer VariablesContainer => _variablesContainer;


    // Variable-related properties
    public string VariableName => _variableName;
    public VariableDataType VariableType => _variableType;
    public ModificationType ModificationType => _modificationType;
    public ConditionType ConditionType => _conditionType;
    public bool BoolValue => _boolValue;
    public int IntValue => _intValue;
    public float FloatValue => _floatValue;
    public string StringValue => _stringValue;

    public DialogueNodeSaveData(string id, string name, string text, List<DialogueChoiceSaveData> choices, 
                               string groupID, DialogueType dialogueType, Vector2 position, 
                               DialogueCharacter character, DialogueCharacterEmotion emotion) {
        _ID = id;
        _name = name;
        _text = text;
        _choices = choices;
        _groupID = groupID;
        _dialogueType = dialogueType;
        _position = position;
        _character = character;
        _emotion = emotion;
        
        // Initialize variable fields with defaults
        _variableName = "";
        _variableType = VariableDataType.Bool;
        _modificationType = ModificationType.Set;
        _conditionType = ConditionType.Equals;
        _boolValue = false;
        _intValue = 0;
        _floatValue = 0f;
        _stringValue = "";
    }

    // Constructor overload with variable data
    public DialogueNodeSaveData(string id, string name, string text, List<DialogueChoiceSaveData> choices,
                               string groupID, DialogueType dialogueType, Vector2 position,
                               DialogueCharacter character, DialogueCharacterEmotion emotion,
                               string variableName, VariableDataType variableType,
                               ModificationType modificationType, ConditionType conditionType,
                               bool boolValue, int intValue, float floatValue, string stringValue)
        : this(id, name, text, choices, groupID, dialogueType, position, character, emotion) {
        _variableName = variableName;
        _variableType = variableType;
        _modificationType = modificationType;
        _conditionType = conditionType;
        _boolValue = boolValue;
        _intValue = intValue;
        _floatValue = floatValue;
        _stringValue = stringValue;
    }

    // Constructor overload with external function data
    public DialogueNodeSaveData(string id, string name, string text, List<DialogueChoiceSaveData> choices,
                               string groupID, DialogueType dialogueType, Vector2 position,
                               DialogueCharacter character, DialogueCharacterEmotion emotion,
                               ExternalFunctionType functionType, string customFunctionName)
        : this(id, name, text, choices, groupID, dialogueType, position, character, emotion) {
        _functionType = functionType;
        _customFunctionName = customFunctionName;
    }

    // Constructor overload with Ink data
    public DialogueNodeSaveData(string id, string name, string text, List<DialogueChoiceSaveData> choices,
                               string groupID, DialogueType dialogueType, Vector2 position,
                               DialogueCharacter character, DialogueCharacterEmotion emotion,
                               TextAsset inkJsonAsset, string knotName, bool startFromBeginning)
        : this(id, name, text, choices, groupID, dialogueType, position, character, emotion)
    {
        _inkJsonAsset = inkJsonAsset;
        _knotName = knotName;
        _startFromBeginning = startFromBeginning;
    }
    
    // Add this constructor to DialogueNodeSaveData class
public DialogueNodeSaveData(string id, string name, string text, List<DialogueChoiceSaveData> choices,
                           string groupID, DialogueType dialogueType, Vector2 position,
                           DialogueCharacter character, DialogueCharacterEmotion emotion,
                           DialogueVariablesContainer variablesContainer,
                           string variableName, VariableDataType variableType,
                           ModificationType modificationType, ConditionType conditionType,
                           bool boolValue, int intValue, float floatValue, string stringValue)
    : this(id, name, text, choices, groupID, dialogueType, position, character, emotion) {
    _variablesContainer = variablesContainer;
    _variableName = variableName;
    _variableType = variableType;
    _modificationType = modificationType;
    _conditionType = conditionType;
    _boolValue = boolValue;
    _intValue = intValue;
    _floatValue = floatValue;
    _stringValue = stringValue;
}

}