using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using NodeDirection = UnityEditor.Experimental.GraphView.Direction;


/// <summary>
/// Node for checking variable conditions in dialogue
/// </summary>
public class DialogueVariableConditionNode : DialogueBaseNode {
    protected override DialogueType _type => DialogueType.VariableCondition;

    private DialogueVariablesContainer _variablesContainer;
    private string _variableName = "";
    private ConditionType _conditionType = ConditionType.Equal;
    
    private bool _boolTargetValue;
    private int _intTargetValue;
    private float _floatTargetValue;
    private string _stringTargetValue = "";

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
    
    public ConditionType Condition { 
        get => _conditionType; 
        set => _conditionType = value; 
    }
    public bool BoolTargetValue { 
        get => _boolTargetValue; 
        set => _boolTargetValue = value; 
    }
    public int IntTargetValue { 
        get => _intTargetValue; 
        set => _intTargetValue = value; 
    }
    public float FloatTargetValue { 
        get => _floatTargetValue; 
        set => _floatTargetValue = value; 
    }
    public string StringTargetValue {
        get => _stringTargetValue;
        set => _stringTargetValue = value;
    }

    private VisualElement _valueContainer;
    private VisualElement _conditionContainer;
    private VisualElement _variableSelectionContainer;
    
    public override void Initialize(string nodeName, DialogueSystemGraphView graphView, Vector2 position) {
        base.Initialize(nodeName, graphView, position);
        
        // Add two output ports: True and False
        DialogueChoiceSaveData trueChoice = new("True");
        DialogueChoiceSaveData falseChoice = new("False");
        _choices.Add(trueChoice);
        _choices.Add(falseChoice);
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

        // Custom data container for condition fields
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

        // Condition type container that will be dynamically updated
        _conditionContainer = new VisualElement();
        customDataContainer.Add(_conditionContainer);

        // Value container that will be dynamically updated
        _valueContainer = new VisualElement();
        customDataContainer.Add(_valueContainer);
        
        // Initial setup based on current type
        UpdateConditionOptions();
        UpdateValueField();

        // Draw output ports (True and False)
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
            UpdateConditionOptions();
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

    private void UpdateConditionOptions() {
        if (_conditionContainer == null) return;

        _conditionContainer.Clear();

        if (_variablesContainer == null || string.IsNullOrEmpty(_variableName)) {
            return;
        }

        DialogueVariable variable = _variablesContainer.GetVariable(_variableName);
        if (variable == null) {
            return;
        }

        // Filter conditions based on variable type
        List<ConditionType> validConditions = new List<ConditionType>();

        switch (variable.Type) {
            case VariableDataType.Bool:
            case VariableDataType.String:
                validConditions.Add(ConditionType.Equal);
                validConditions.Add(ConditionType.NotEqual);
                break;

            case VariableDataType.Int:
            case VariableDataType.Float:
                validConditions.Add(ConditionType.Equal);
                validConditions.Add(ConditionType.NotEqual);
                validConditions.Add(ConditionType.Greater);
                validConditions.Add(ConditionType.GreaterOrEqual);
                validConditions.Add(ConditionType.Less);
                validConditions.Add(ConditionType.LessOrEqual);
                break;
        }

        // Ensure current condition is valid
        if (!validConditions.Contains(_conditionType) && validConditions.Count > 0) {
            _conditionType = validConditions[0];
        }

        EnumField conditionTypeField = UIElementUtility.CreateEnumField("Condition", _conditionType, callback => {
            _conditionType = (ConditionType)callback.newValue;
        });
        _conditionContainer.Add(conditionTypeField);
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

        switch (variable.Type)
        {
            case VariableDataType.Bool:
                Toggle boolField = UIElementUtility.CreateToggle(_boolTargetValue, "Target Value", callback =>
                {
                    _boolTargetValue = callback.newValue;
                });
                _valueContainer.Add(boolField);
                break;

            case VariableDataType.Int:
                IntegerField intField = UIElementUtility.CreateIntegerField(_intTargetValue, "Target Value", callback =>
                {
                    _intTargetValue = callback.newValue;
                });
                _valueContainer.Add(intField);
                break;

            case VariableDataType.Float:
                FloatField floatField = UIElementUtility.CreateFloatField(_floatTargetValue, "Target Value", callback =>
                {
                    _floatTargetValue = callback.newValue;
                });
                _valueContainer.Add(floatField);
                break;

            case VariableDataType.String:
                TextField stringField = UIElementUtility.CreateTextField(_stringTargetValue, "Target Value", callback =>
                {
                    _stringTargetValue = callback.newValue;
                });
                stringField.AddClasses("ds-node__text-field");
                _valueContainer.Add(stringField);
                break;
        }
    }
    
    public void RefreshUI() {
    if (_variableSelectionContainer != null) {
        UpdateVariableSelection();
        UpdateConditionOptions();
        UpdateValueField();
    }
}

    protected override Port CreateChoicePort(object userData) {
        DialogueChoiceSaveData choiceData = userData as DialogueChoiceSaveData;
        Port choicePort = this.CreatePort(choiceData.Text, direction: NodeDirection.Output, capacity: Port.Capacity.Single);
        choicePort.userData = userData;
        return choicePort;
    }
}