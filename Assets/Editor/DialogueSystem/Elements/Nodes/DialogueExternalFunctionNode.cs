using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using NodeDirection = UnityEditor.Experimental.GraphView.Direction;

public class DialogueExternalFunctionNode : DialogueBaseNode
{
    protected override DialogueType _type => DialogueType.ExternalFunction;

    private ExternalFunctionType _functionType = ExternalFunctionType.PlayEmote;
    private string _functionParameter = "";

    public ExternalFunctionType FunctionType
    {
        get => _functionType;
        set => _functionType = value;
    }
    public string FunctionParameter
    {
        get => _functionParameter;
        set => _functionParameter = value;
    }

    private VisualElement _parameterContainer;

    public override void Initialize(string nodeName, DialogueSystemGraphView graphView, Vector2 position)
    {
        base.Initialize(nodeName, graphView, position);
        DialogueChoiceSaveData choice = new("Next Dialogue");
        _choices.Add(choice);
    }

    public override void Draw()
    {
        // Draw title field
        TextField dialogueNameField = UIElementUtility.CreateTextField(_dialogueName, onValueChanged: callback =>
        {
            TextField target = callback.target as TextField;
            target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

            if (string.IsNullOrEmpty(target.value))
                target.value = _dialogueName;

            if (_group == null)
            {
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

        // Custom data container
        VisualElement customDataContainer = new();
        customDataContainer.AddToClassList("ds-node__custom-data-container");
        extensionContainer.Add(customDataContainer);

        // External Function Type dropdown
        EnumField functionTypeField = UIElementUtility.CreateEnumField("Function Type", _functionType, callback =>
        {
            _functionType = (ExternalFunctionType)callback.newValue;
            UpdateParameterField();
        });
        customDataContainer.Add(functionTypeField);

        // Parameter container (will be populated based on function type)
        _parameterContainer = new VisualElement();
        customDataContainer.Add(_parameterContainer);

        // Initial parameter field setup
        UpdateParameterField();

        // Info box explaining the function
        Box infoBox = new Box();
        infoBox.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.4f, 0.3f));
        infoBox.style.marginTop = 10;
        infoBox.style.paddingTop = 5;
        infoBox.style.paddingBottom = 5;
        infoBox.style.paddingLeft = 5;
        infoBox.style.paddingRight = 5;
        infoBox.style.borderTopLeftRadius = 3;
        infoBox.style.borderTopRightRadius = 3;
        infoBox.style.borderBottomLeftRadius = 3;
        infoBox.style.borderBottomRightRadius = 3;

        Label infoLabel = new Label(GetFunctionDescription(_functionType));
        infoLabel.style.fontSize = 10;
        infoLabel.style.color = new StyleColor(new Color(0.8f, 0.8f, 0.9f));
        infoLabel.style.whiteSpace = WhiteSpace.Normal;
        infoBox.Add(infoLabel);
        customDataContainer.Add(infoBox);

        // Draw output ports
        foreach (var choice in _choices)
        {
            Port choicePort = CreateChoicePort(choice);
            choicePort.userData = choice;
            outputContainer.Add(choicePort);
        }

        RefreshExpandedState();
    }

    private void UpdateParameterField()
    {
        if (_parameterContainer == null) return;

        _parameterContainer.Clear();

        switch (_functionType)
        {
            case ExternalFunctionType.PlayEmote:
                CreateDropdownParameter("Emote", new List<string> {
                    "Wave", "Dance", "Cheer", "Laugh", "Cry", "Angry", "Think", "Sleep"
                });
                break;

            case ExternalFunctionType.PlayAnimation:
                CreateDropdownParameter("Animation", new List<string> {
                    "Darkness", "Sunshine", "Jump", "Run", "Idle", "Attack", "Defend"
                });
                break;

            case ExternalFunctionType.GiveItem:
            case ExternalFunctionType.RemoveItem:
                CreateTextParameter("Item Name");
                break;

            case ExternalFunctionType.PlaySound:
                CreateDropdownParameter("Sound", new List<string> {
                    "Click", "Success", "Fail", "Notification", "Background", "Ambient"
                });
                break;

            case ExternalFunctionType.UpdateQuest:
                CreateTextParameter("Quest Data (Format: QuestName:Progress)");
                break;

            case ExternalFunctionType.TeleportPlayer:
                CreateTextParameter("Position (Format: x,y,z)");
                break;

            case ExternalFunctionType.SpawnNPC:
                CreateTextParameter("NPC Data (Format: NPCName:x,y,z)");
                break;

            case ExternalFunctionType.ShowUI:
            case ExternalFunctionType.HideUI:
                CreateDropdownParameter("UI Panel", new List<string> {
                    "Inventory", "QuestLog", "Map", "Settings", "MainMenu", "Dialogue"
                });
                break;

            case ExternalFunctionType.SetVariable:
                CreateTextParameter("Variable Data (Format: VarName:Value)");
                break;

            case ExternalFunctionType.TriggerEvent:
                CreateTextParameter("Event Name");
                break;

            case ExternalFunctionType.Custom:
                CreateTextParameter("Custom Function Name");
                break;

            case ExternalFunctionType.PausePlayer:
            case ExternalFunctionType.ResumePlayer:
                // These don't need parameters
                Label noParamLabel = new Label("(No parameters required)");
                noParamLabel.style.fontSize = 10;
                noParamLabel.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f));
                noParamLabel.style.marginTop = 5;
                _parameterContainer.Add(noParamLabel);
                break;
        }
    }

    private void CreateTextParameter(string label)
    {
        TextField paramField = UIElementUtility.CreateTextField(_functionParameter, label, callback =>
        {
            _functionParameter = callback.newValue;
        });
        paramField.AddClasses("ds-node__text-field");
        _parameterContainer.Add(paramField);
    }

    private void CreateDropdownParameter(string label, List<string> options)
    {
        Label dropdownLabel = new Label(label);
        dropdownLabel.style.marginTop = 5;
        dropdownLabel.style.marginBottom = 2;
        _parameterContainer.Add(dropdownLabel);

        // Create a dropdown using PopupField
        PopupField<string> dropdown = new PopupField<string>(
            options,
            string.IsNullOrEmpty(_functionParameter) ? options[0] : _functionParameter
        );
        dropdown.RegisterValueChangedCallback(callback =>
        {
            _functionParameter = callback.newValue;
        });
        dropdown.AddToClassList("ds-node__text-field");
        _parameterContainer.Add(dropdown);
    }

    private string GetFunctionDescription(ExternalFunctionType type)
    {
        return type switch
        {
            ExternalFunctionType.PlayEmote => "Plays a character emote animation.",
            ExternalFunctionType.PausePlayer => "Pauses player movement and input.",
            ExternalFunctionType.ResumePlayer => "Resumes player movement and input.",
            ExternalFunctionType.GiveItem => "Adds an item to the player's inventory.",
            ExternalFunctionType.RemoveItem => "Removes an item from the player's inventory.",
            ExternalFunctionType.PlayAnimation => "Plays a specific animation.",
            ExternalFunctionType.PlaySound => "Plays a sound effect.",
            ExternalFunctionType.UpdateQuest => "Updates quest progress.",
            ExternalFunctionType.TeleportPlayer => "Teleports the player to a position.",
            ExternalFunctionType.SpawnNPC => "Spawns an NPC at a location.",
            ExternalFunctionType.ShowUI => "Shows a UI panel.",
            ExternalFunctionType.HideUI => "Hides a UI panel.",
            ExternalFunctionType.SetVariable => "Sets a dialogue variable value.",
            ExternalFunctionType.TriggerEvent => "Triggers a custom game event.",
            ExternalFunctionType.Custom => "Executes a custom function by name.",
            _ => "External function."
        };
    }

    protected override Port CreateChoicePort(object userData)
    {
        Port choicePort = this.CreatePort("Next Dialogue", direction: NodeDirection.Output, capacity: Port.Capacity.Single);
        choicePort.userData = userData;
        return choicePort;
    }

    public void Setup(DialogueNodeSaveData data)
    {
        base.Setup(data, new List<DialogueChoiceSaveData>(data.Choices));
        _functionType = data.FunctionType;
        _functionParameter = data.CustomFunctionName;
    }
}

