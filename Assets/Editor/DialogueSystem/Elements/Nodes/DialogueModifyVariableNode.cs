using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using NodeDirection = UnityEditor.Experimental.GraphView.Direction;

/// <summary>
/// Node for modifying variables in dialogue
/// </summary>
public class DialogueModifyVariableNode : DialogueBaseNode {
    protected override DialogueType _type => DialogueType.ModifyVariable;

    private DialogueVariablesContainer _variablesContainer;
    private string _variableName = "";
    private ModificationType _modifyType = ModificationType.Set;
    
    private bool _boolValue;
    private int _intValue;
    private float _floatValue;
    private string _stringValue = "";

    public DialogueVariablesContainer VariablesContainer { 
        get => _variablesContainer; 
        set => _variablesContainer = value; 
    }
    
    public string VariableName { 
        get => _variableName; 
        set => _variableName = value; 
    }
    public VariableDataType VariableType { 
        get {
            if (_variablesContainer != null) {
                var variable = _variablesContainer.GetVariable(_variableName);
                return variable?.Type ?? VariableDataType.Bool;
            }
            return VariableDataType.Bool;
        }
    }
    
    public ModificationType Modification { 
        get => _modifyType; 
        set => _modifyType = value; 
    }
    public bool BoolValue { 
        get => _boolValue; 
        set => _boolValue = value; 
    }
    public int IntValue { 
        get => _intValue; 
        set => _intValue = value; 
    }
    public float FloatValue { 
        get => _floatValue; 
        set => _floatValue = value; 
    }
    public string StringValue { 
        get => _stringValue; 
        set => _stringValue = value; 
    }

    private VisualElement _valueContainer;
    private VisualElement _modificationContainer;
    private VisualElement _variableSelectionContainer;

    public override void Initialize(string nodeName, DialogueSystemGraphView graphView, Vector2 position) {
        base.Initialize(nodeName, graphView, position);
        
        DialogueChoiceSaveData choice = new("Next");
        _choices.Add(choice);
    }

    public override void Draw() {
        // Draw the dialogue name field
        TextField dialogueNameField = UIElementUtility.CreateTextField(_dialogueName, onValueChanged: callback => {
            TextField target = callback.target as TextField;
            target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

            if (string.IsNullOrEmpty(target.value))
                target.value = _dialogueName;

            if (_group == null) {
                _graphView.RemoveUngroupedNode(this);
                _dialogueName = target.value;
                _graphView.AddUngroupedNode(this);
                return;
            }

            DialogueSystemGroup currentGroup = _group;
            _graphView.RemoveGroupedNode(this, _group);
            _dialogueName = target.value;
            _graphView.AddGroupedNode(this, currentGroup);
        });
        dialogueNameField.AddClasses(
            "ds-node__text-field",
            "ds-node__text-field__hidden",
            "ds-node__filename-text-field"
        );
        titleContainer.Insert(0, dialogueNameField);

        // Draw input port
        Port inputPort = this.CreatePort("Dialogue Connection", direction: NodeDirection.Input, capacity: Port.Capacity.Multi);
        inputContainer.Add(inputPort);

        // Custom data container for modification fields
        VisualElement customDataContainer = new();
        customDataContainer.AddToClassList("ds-node__custom-data-container");
        extensionContainer.Add(customDataContainer);

        // Variables Container object field (drag and drop DialogueVariablesContainer)
        ObjectField containerField = UIElementUtility.CreateObjectField("Variables Container", typeof(DialogueVariablesContainer), _variablesContainer, callback => {
            _variablesContainer = callback.newValue as DialogueVariablesContainer;
            UpdateVariableSelection();
        });
        customDataContainer.Add(containerField);

        // Variable selection container (dropdown will be created here)
        _variableSelectionContainer = new VisualElement();
        customDataContainer.Add(_variableSelectionContainer);
        UpdateVariableSelection();

        // Modification type container
        _modificationContainer = new VisualElement();
        customDataContainer.Add(_modificationContainer);
        UpdateModificationOptions();

        // Value container that will be dynamically updated
        _valueContainer = new VisualElement();
        customDataContainer.Add(_valueContainer);
        
        // Initial value field based on current type
        UpdateValueField();

        // Draw output port
        foreach (var choice in _choices) {
            Port choicePort = CreateChoicePort(choice);
            choicePort.userData = choice;
            outputContainer.Add(choicePort);
        }

        RefreshExpandedState();
    }

    private void UpdateVariableSelection() {
        if (_variableSelectionContainer == null) return;

        _variableSelectionContainer.Clear();

        if (_variablesContainer == null || _variablesContainer.Variables.Count == 0) {
            Label warningLabel = new Label("⚠ Assign a Variables Container");
            warningLabel.style.color = new StyleColor(Color.yellow);
            warningLabel.style.marginTop = 5;
            warningLabel.style.marginBottom = 5;
            _variableSelectionContainer.Add(warningLabel);
            return;
        }

        // Create dropdown with variable names from container
        List<string> variableNames = _variablesContainer.GetVariableNames();
        
        if (variableNames.Count == 0) {
            Label noVarsLabel = new Label("⚠ No variables in container");
            noVarsLabel.style.color = new StyleColor(Color.yellow);
            noVarsLabel.style.marginTop = 5;
            noVarsLabel.style.marginBottom = 5;
            _variableSelectionContainer.Add(noVarsLabel);
            return;
        }

        // Set default if not already set
        if (string.IsNullOrEmpty(_variableName) || !variableNames.Contains(_variableName)) {
            _variableName = variableNames[0];
        }

        PopupField<string> variableDropdown = new PopupField<string>(
            "Variable",
            variableNames,
            _variableName
        );
        variableDropdown.RegisterValueChangedCallback(callback => {
            _variableName = callback.newValue;
            UpdateModificationOptions();
            UpdateValueField();
        });
        _variableSelectionContainer.Add(variableDropdown);

        // Show variable type info
        DialogueVariable selectedVar = _variablesContainer.GetVariable(_variableName);
        if (selectedVar != null) {
            Label typeLabel = new Label($"Type: {selectedVar.Type}");
            typeLabel.style.fontSize = 10;
            typeLabel.style.color = new StyleColor(new Color(0.7f, 0.7f, 0.7f));
            typeLabel.style.marginTop = -5;
            typeLabel.style.marginBottom = 5;
            _variableSelectionContainer.Add(typeLabel);
        }
    }

    private void UpdateModificationOptions() {
        if (_modificationContainer == null) return;

        _modificationContainer.Clear();

        if (_variablesContainer == null || string.IsNullOrEmpty(_variableName)) {
            return;
        }

        DialogueVariable variable = _variablesContainer.GetVariable(_variableName);
        if (variable == null) {
            return;
        }

        // Filter modification types based on variable type
        List<ModificationType> validModifications = new List<ModificationType>();

        switch (variable.Type) {
            case VariableDataType.Bool:
                validModifications.Add(ModificationType.Set);
                validModifications.Add(ModificationType.Toggle);
                break;

            case VariableDataType.Int:
            case VariableDataType.Float:
                validModifications.Add(ModificationType.Set);
                validModifications.Add(ModificationType.Increase);
                validModifications.Add(ModificationType.Decrease);
                break;

            case VariableDataType.String:
                validModifications.Add(ModificationType.Set);
                break;
        }

        // Ensure current modification is valid
        if (!validModifications.Contains(_modifyType) && validModifications.Count > 0) {
            _modifyType = validModifications[0];
        }

        EnumField modifyTypeField = UIElementUtility.CreateEnumField("Action", _modifyType, callback => {
            _modifyType = (ModificationType)callback.newValue;
            UpdateValueField();
        });
        _modificationContainer.Add(modifyTypeField);
    }

    private void UpdateValueField()
    {
        if (_valueContainer == null) return;

        _valueContainer.Clear();

        if (_variablesContainer == null || string.IsNullOrEmpty(_variableName))
        {
            return;
        }

        DialogueVariable variable = _variablesContainer.GetVariable(_variableName);
        if (variable == null)
        {
            return;
        }

        // For Toggle, we don't need a value field
        if (_modifyType == ModificationType.Toggle)
        {
            Label infoLabel = new Label("(Toggles between true/false)");
            infoLabel.style.fontSize = 10;
            infoLabel.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f));
            _valueContainer.Add(infoLabel);
            return;
        }

        switch (variable.Type)
        {
            case VariableDataType.Bool:
                Toggle boolField = UIElementUtility.CreateToggle(_boolValue, "Value", callback =>
                {
                    _boolValue = callback.newValue;
                });
                _valueContainer.Add(boolField);
                break;

            case VariableDataType.Int:
                IntegerField intField = UIElementUtility.CreateIntegerField(_intValue, "Value", callback =>
                {
                    _intValue = callback.newValue;
                });
                _valueContainer.Add(intField);
                break;

            case VariableDataType.Float:
                FloatField floatField = UIElementUtility.CreateFloatField(_floatValue, "Value", callback =>
                {
                    _floatValue = callback.newValue;
                });
                _valueContainer.Add(floatField);
                break;

            case VariableDataType.String:
                TextField stringField = UIElementUtility.CreateTextField(_stringValue, "Value", callback =>
                {
                    _stringValue = callback.newValue;
                });
                stringField.AddClasses("ds-node__text-field");
                _valueContainer.Add(stringField);
                break;
        }
    }
    
    public void RefreshUI() {
    if (_variableSelectionContainer != null) {
        UpdateVariableSelection();
        UpdateModificationOptions();
        UpdateValueField();
    }
}

    protected override Port CreateChoicePort(object userData) {
        Port choicePort = this.CreatePort("Next", direction: NodeDirection.Output, capacity: Port.Capacity.Single);
        choicePort.userData = userData;
        return choicePort;
    }
}