using System.Collections.Generic;
using UnityEngine;

public class Dialogue : ScriptableObject {
    [SerializeField] private string _name;
    [SerializeField, TextArea] private string _text;
    [SerializeField] private List<DialogueChoiceData> _choices;
    [SerializeField] private DialogueType _type;
    [SerializeField] private bool _isStartingDialogue;
    [SerializeField] private DialogueCharacter _character;
    [SerializeField] private DialogueCharacterEmotion _emotion;

    // Variable modification/condition fields
    [SerializeField] private DialogueVariablesContainer _variablesContainer;
    [SerializeField] private string _variableName;
    [SerializeField] private VariableDataType _variableType;
    [SerializeField] private ModificationType _modificationType;
    [SerializeField] private ConditionType _conditionType;
    
    // Variable values for different types
    [SerializeField] private bool _boolValue;
    [SerializeField] private int _intValue;
    [SerializeField] private float _floatValue;
    [SerializeField] private string _stringValue;

    // Ink-specific fields
    [SerializeField] private TextAsset _inkJsonAsset;
    [SerializeField] private string _knotName;
    [SerializeField] private bool _startFromBeginning = true;

    // External function fields
    [SerializeField] private ExternalFunctionType _externalFunctionType;
    [SerializeField] private string _functionParameter;

    public string Name => _name;
    public string Text => _text;
    public DialogueType Type => _type;
    public DialogueCharacter Character => _character;
    public DialogueCharacterEmotion Emotion => _emotion;
    public bool IsStartingDialogue => _isStartingDialogue;
    
    // ADD THIS PUBLIC PROPERTY - CRITICAL FOR UI CHOICES
    public List<DialogueChoiceData> Choices => _choices;

    // Variable-related properties
    public DialogueVariablesContainer VariablesContainer => _variablesContainer;
    public string VariableName => _variableName;
    public VariableDataType VariableType => _variableType;
    public ModificationType ModificationType => _modificationType;
    public ConditionType ConditionType => _conditionType;
    public bool BoolValue => _boolValue;
    public int IntValue => _intValue;
    public float FloatValue => _floatValue;
    public string StringValue => _stringValue;

    // Ink-related properties
    public TextAsset InkJsonAsset => _inkJsonAsset;
    public string KnotName => _knotName;
    public bool StartFromBeginning => _startFromBeginning;

    // External function properties
    public ExternalFunctionType ExternalFunctionType => _externalFunctionType;
    public string FunctionParameter => _functionParameter;
    
    // Backward compatibility property
    public string CustomFunctionName => _functionParameter;

    public void Initialize(string name, string text, List<DialogueChoiceData> choices, DialogueType type, 
                          DialogueCharacter character, DialogueCharacterEmotion emotion, bool isStartingDialogue) {
        _name = name;
        _text = text;
        _choices = choices;
        _type = type;
        _isStartingDialogue = isStartingDialogue;
        _character = character;
        _emotion = emotion;
    }

    public void InitializeWithVariableData(string name, string text, List<DialogueChoiceData> choices, 
                                          DialogueType type, DialogueCharacter character, 
                                          DialogueCharacterEmotion emotion, bool isStartingDialogue,
                                          DialogueVariablesContainer variablesContainer, string variableName, VariableDataType variableType,
                                          ModificationType modificationType, ConditionType conditionType,
                                          bool boolValue, int intValue, float floatValue, string stringValue) {
        Initialize(name, text, choices, type, character, emotion, isStartingDialogue);
        
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

    public void InitializeWithInkData(string name, string text, List<DialogueChoiceData> choices,
                                      DialogueType type, DialogueCharacter character,
                                      DialogueCharacterEmotion emotion, bool isStartingDialogue,
                                      TextAsset inkJsonAsset, string knotName, bool startFromBeginning) {
        Initialize(name, text, choices, type, character, emotion, isStartingDialogue);
        
        _inkJsonAsset = inkJsonAsset;
        _knotName = knotName;
        _startFromBeginning = startFromBeginning;
    }

    public void InitializeWithFunctionData(string name, string text, List<DialogueChoiceData> choices,
                                          DialogueType type, DialogueCharacter character,
                                          DialogueCharacterEmotion emotion, bool isStartingDialogue,
                                          ExternalFunctionType functionType, string functionParameter) {
        Initialize(name, text, choices, type, character, emotion, isStartingDialogue);
        
        _externalFunctionType = functionType;
        _functionParameter = functionParameter;
    }

    public void SetChoiceNextDialogue(Dialogue nextDialogue, int index) {
        _choices[index].SetNextDialogue(nextDialogue);
    }

    public Dialogue GetNextDialogue() {
        foreach (var choice in _choices)
            if (choice.NextDialogue != null)
                return choice.NextDialogue;
        return null;
    }

    // Helper method to get the value based on the current variable type
    public object GetVariableValue() {
        return _variableType switch {
            VariableDataType.Bool => _boolValue,
            VariableDataType.Int => _intValue,
            VariableDataType.Float => _floatValue,
            VariableDataType.String => _stringValue,
            _ => null
        };
    }
}