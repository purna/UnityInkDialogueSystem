using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using NodeDirection = UnityEditor.Experimental.GraphView.Direction;

/// <summary>
/// Node that references an Ink JSON story file
/// </summary>
public class DialogueInkNode : DialogueBaseNode {
    protected override DialogueType _type => DialogueType.Ink;

    private TextAsset _inkJsonAsset;
    private string _knotName = "";
    private bool _startFromBeginning = true;

    public TextAsset InkJsonAsset { get => _inkJsonAsset; set => _inkJsonAsset = value; }
    public string KnotName { get => _knotName; set => _knotName = value; }
    public bool StartFromBeginning { get => _startFromBeginning; set => _startFromBeginning = value; }

    public override void Initialize(string nodeName, DialogueSystemGraphView graphView, Vector2 position) {
        base.Initialize(nodeName, graphView, position);
        
        // Ink node has a single output that continues after the Ink story completes
        DialogueChoiceSaveData choice = new("Continue After Ink Story");
        _choices.Add(choice);
    }

    public override void Draw() {
        // Draw the base node elements (title, input port, etc.)
        TextField dialogueNameField = UIElementUtility.CreateTextField(_dialogueName, onValueChanged: callback => {
            TextField target = callback.target as TextField;
            target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

            if (string.IsNullOrEmpty(target.value))
                target.value = DialogueName;

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

        // Input port
        Port inputPort = this.CreatePort("Dialogue Connection", direction: NodeDirection.Input, capacity: Port.Capacity.Multi);
        inputContainer.Add(inputPort);

        // Custom data container for Ink-specific fields
        VisualElement customDataContainer = new();
        customDataContainer.AddToClassList("ds-node__custom-data-container");
        extensionContainer.Add(customDataContainer);

        // Ink JSON file reference - restricted to .json files only
        ObjectField inkJsonField = new ObjectField("Ink JSON File")
        {
            objectType = typeof(TextAsset),
            value = _inkJsonAsset,
            allowSceneObjects = false
        };
        
        inkJsonField.RegisterValueChangedCallback(callback => {
            TextAsset newAsset = callback.newValue as TextAsset;
            // Validate that the file is a .json file
            if (newAsset != null) {
                string assetPath = UnityEditor.AssetDatabase.GetAssetPath(newAsset);
                if (!assetPath.EndsWith(".json", System.StringComparison.OrdinalIgnoreCase)) {
                    Debug.LogWarning("Only .json files are allowed for Ink stories.");
                    inkJsonField.SetValueWithoutNotify(_inkJsonAsset); // Revert to previous value
                    return;
                }
            }
            _inkJsonAsset = newAsset;
        });
        
        inkJsonField.AddToClassList("ds-node__text-field");
        customDataContainer.Add(inkJsonField);

        // Display file info if assigned
        if (_inkJsonAsset != null) {
            Label fileInfoLabel = new Label($"File: {_inkJsonAsset.name}");
            fileInfoLabel.AddToClassList("ds-node__label");
            fileInfoLabel.style.fontSize = 10;
            fileInfoLabel.style.color = new StyleColor(new Color(0.7f, 0.7f, 0.7f));
            fileInfoLabel.style.marginTop = -5;
            fileInfoLabel.style.marginBottom = 5;
            customDataContainer.Add(fileInfoLabel);
        }

        // Start from beginning toggle
        Toggle startFromBeginningToggle = UIElementUtility.CreateToggle(
            "Start From Beginning",
            _startFromBeginning,
            callback => {
                _startFromBeginning = callback.newValue;
                RefreshExpandedState();
            }
        );
        customDataContainer.Add(startFromBeginningToggle);

        // Knot/Stitch name field (only shown if not starting from beginning)
        if (!_startFromBeginning) {
            Label knotLabel = new Label("Knot/Stitch Name:");
            knotLabel.AddToClassList("ds-node__label");
            knotLabel.style.marginTop = 5;
            customDataContainer.Add(knotLabel);

            TextField knotNameField = UIElementUtility.CreateTextField(
                _knotName,
                onValueChanged: callback => {
                    _knotName = callback.newValue;
                }
            );
            knotNameField.AddToClassList("ds-node__text-field");
            customDataContainer.Add(knotNameField);
        }

        // Info box
        Box infoBox = new Box();
        infoBox.style.backgroundColor = new StyleColor(new Color(0.2f, 0.4f, 0.6f, 0.3f));
        infoBox.style.marginTop = 10;
        infoBox.style.paddingTop = 5;
        infoBox.style.paddingBottom = 5;
        infoBox.style.paddingLeft = 5;
        infoBox.style.paddingRight = 5;
        infoBox.style.borderTopLeftRadius = 3;
        infoBox.style.borderTopRightRadius = 3;
        infoBox.style.borderBottomLeftRadius = 3;
        infoBox.style.borderBottomRightRadius = 3;

        Label infoLabel = new Label("This node will run the Ink story.\nAfter completion, it continues to the next node.");
        infoLabel.style.fontSize = 10;
        infoLabel.style.color = new StyleColor(new Color(0.9f, 0.9f, 0.9f));
        infoLabel.style.whiteSpace = WhiteSpace.Normal;
        infoBox.Add(infoLabel);
        customDataContainer.Add(infoBox);

        // Output port for continuation after Ink story
        foreach (var choice in _choices) {
            Port choicePort = CreateChoicePort(choice);
            choicePort.userData = choice;
            outputContainer.Add(choicePort);
        }

        RefreshExpandedState();
    }

    protected override Port CreateChoicePort(object userData) {
        Port choicePort = this.CreatePort(
            "Continue After Ink Story", 
            direction: NodeDirection.Output, 
            capacity: Port.Capacity.Single
        );
        choicePort.userData = userData;
        return choicePort;
    }

    /// <summary>
    /// Setup method for loading saved data
    /// </summary>
    public void Setup(DialogueInkNodeSaveData data) {
        base.Setup(data, new List<DialogueChoiceSaveData>(data.Choices));
        _inkJsonAsset = data.InkJsonAsset;
        _knotName = data.KnotName;
        _startFromBeginning = data.StartFromBeginning;
    }
}

/// <summary>
/// Save data structure for DialogueInkNode
/// </summary>
[System.Serializable]
public class DialogueInkNodeSaveData : DialogueNodeSaveData {
    public TextAsset InkJsonAsset;
    public string KnotName;
    public bool StartFromBeginning;

    public DialogueInkNodeSaveData(string id, string name, DialogueType type, TextAsset inkJsonAsset,
                                    string knotName, bool startFromBeginning, Vector2 position,
                                    string text, DialogueCharacter character, DialogueCharacterEmotion emotion)
        : base(id, name, text, new List<DialogueChoiceSaveData>(), "", type, position, character, emotion) {
        InkJsonAsset = inkJsonAsset;
        KnotName = knotName;
        StartFromBeginning = startFromBeginning;
    }
}