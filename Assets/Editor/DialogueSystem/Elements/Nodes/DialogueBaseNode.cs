using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using NodeDirection = UnityEditor.Experimental.GraphView.Direction;

public abstract class DialogueBaseNode : Node {
    private string _id;
    protected string _dialogueName;
    protected List<DialogueChoiceSaveData> _choices;

    private string _text;
    private DialogueCharacter _character;
    private DialogueCharacterEmotion _emotion;


    public string Text
    {
        get => _text;
        set => _text = value;
    }
    
    public DialogueCharacter Character {
        get => _character;
        set => _character = value;
    }
    
    public DialogueCharacterEmotion Emotion {
        get => _emotion;
        set => _emotion = value;
    }


    protected abstract DialogueType _type { get; }

    private Color _defaultBackgroundColor;
    protected DialogueSystemGraphView _graphView;
    protected DialogueSystemGroup _group;

    public string DialogueName => _dialogueName;
    public DialogueSystemGroup Group => _group;
    public string ID => _id;
    public IEnumerable<DialogueChoiceSaveData> Choices => _choices;
    public DialogueType DialogueType => _type;

    public virtual void Initialize(string nodeName, DialogueSystemGraphView graphView, Vector2 position) {
        _id = Guid.NewGuid().ToString();
        _dialogueName = nodeName;
        _choices = new();
        _text = "Dialogue text.";
        _emotion = DialogueCharacterEmotion.None;
        _defaultBackgroundColor = new(29f / 255f, 29f / 255f, 30f / 255f);
        _graphView = graphView;

        SetPosition(new(position, Vector2.zero));

        mainContainer.AddToClassList("ds-node__main-container");
        extensionContainer.AddToClassList("ds-node__extension-container");
    }

    #region Overrided Methods
    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
        evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
        evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());
        base.BuildContextualMenu(evt);
    }
    #endregion

    public virtual void Draw() {
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

        Port inputPort = this.CreatePort("Dialogue Connection", direction: NodeDirection.Input, capacity: Port.Capacity.Multi);
        inputContainer.Add(inputPort);

        VisualElement customDataContainer = new();
        customDataContainer.AddToClassList("ds-node__custom-data-container");
        extensionContainer.Add(customDataContainer);

        ObjectField characterField = UIElementUtility.CreateObjectField("Character", typeof(DialogueCharacter), _character, callback => {
            _character = callback.newValue as DialogueCharacter;
        });
        customDataContainer.Add(characterField);

        EnumField emotionField = UIElementUtility.CreateEnumField("Emotion", _emotion, callback => {
            _emotion = (DialogueCharacterEmotion)callback.newValue;
        });
        customDataContainer.Add(emotionField);

        Foldout textFoldout = UIElementUtility.CreateFoldout("Dialogue Text");
        TextField dialogueTextField = UIElementUtility.CreateTextArea(_text, onValueChanged: callback => {
            _text = callback.newValue;
        });
        dialogueTextField.AddClasses(
            "ds-node__text-field",
            "ds-node__quote-text-field"
        );
        textFoldout.Add(dialogueTextField);
        customDataContainer.Add(textFoldout);

        foreach (var choice in _choices) {
            Port choicePort = CreateChoicePort(choice);
            choicePort.userData = choice;
            outputContainer.Add(choicePort);
        }

        RefreshExpandedState();
    }

    public void ChangeGroup(DialogueSystemGroup group) {
        _group = group;
    }

    #region Utility
    public void DisconnectAllPorts() {
        DisconnectInputPorts();
        DisconnectOutputPorts();
    }

    private void DisconnectInputPorts() {
        DisconnectPorts(inputContainer);
    }

    private void DisconnectOutputPorts() {
        DisconnectPorts(outputContainer);
    }

    public DialogueChoiceSaveData GetChoice(int i) {
        return _choices[i];
    }

    private void DisconnectPorts(VisualElement container) {
        foreach (var element in container.Children())
            if (element is Port port)
                if (port.connected)
                    _graphView.DeleteElements(port.connections);
    }

    public void SetErrorStyle(Color color) {
        mainContainer.style.backgroundColor = color;
    }

    public void ResetStyle() {
        mainContainer.style.backgroundColor = _defaultBackgroundColor;
    }

    public bool IsStartingNode() {
        Port port = inputContainer.Children().First() as Port;
        return !port.connected;
    }
    #endregion

    public void Setup(DialogueNodeSaveData data, List<DialogueChoiceSaveData> choices) {
        _id = data.ID;
        _choices = choices;
        _text = data.Text;
        _character = data.Character;
        _emotion = data.Emotion;
    }

    protected abstract Port CreateChoicePort(object userData);
}
