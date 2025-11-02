using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueMultipleChoiceNode : DialogueBaseNode {
    protected override DialogueType _type => DialogueType.MultipleChoice;

    public override void Initialize(string nodeName, DialogueSystemGraphView graphView, Vector2 position) {
        base.Initialize(nodeName, graphView, position);
        DialogueChoiceSaveData choice = new("New Choice");
        _choices.Add(choice);
    }

    public override void Draw() {
        base.Draw();

        Button addChoiceButton = UIElementUtility.CreateButton("Add Choice", delegate {
            DialogueChoiceSaveData choice = new("New Choice");
            outputContainer.Add(CreateChoicePort(choice));
            _choices.Add(choice);
        });
        addChoiceButton.AddToClassList("ds-node__button");
        mainContainer.Insert(1, addChoiceButton);
    }

    protected override Port CreateChoicePort(object userData) {
        Port choicePort = this.CreatePort();
        choicePort.userData = userData;

        DialogueChoiceSaveData choiceData = (DialogueChoiceSaveData)userData;

        Button deleteChoiceButton = UIElementUtility.CreateButton("X", () => {
            if (_choices.Count == 1)
                return;

            if (choicePort.connected)
                _graphView.DeleteElements(choicePort.connections);

            _choices.Remove(choiceData);
            _graphView.RemoveElement(choicePort);
        });
        deleteChoiceButton.AddToClassList("ds-node__button");

        TextField textChoiceField = UIElementUtility.CreateTextField(choiceData.Text);
        textChoiceField.RegisterValueChangedCallback(evt => {
            choiceData.SetText(evt.newValue);
            UpdateTextFieldWidth(textChoiceField);
        });
        textChoiceField.AddClasses(
            "ds-node__text-field",
            "ds-node__text-field__hidden",
            "ds-node__choice-text-field"
        );

        // Make the text field flexible
        textChoiceField.style.minWidth = 100;
        textChoiceField.style.maxWidth = StyleKeyword.None;
        textChoiceField.style.width = StyleKeyword.Auto;
        textChoiceField.style.flexGrow = 1;
        textChoiceField.style.flexShrink = 0;
        
        // Set initial width based on content
        UpdateTextFieldWidth(textChoiceField);

        // Create a container for better layout control
        VisualElement choiceContainer = new VisualElement();
        choiceContainer.style.flexDirection = FlexDirection.Row;
        choiceContainer.style.flexGrow = 1;
        choiceContainer.style.alignItems = Align.Center;

        choiceContainer.Add(textChoiceField);
        choiceContainer.Add(deleteChoiceButton);
        
        choicePort.Add(choiceContainer);
        
        return choicePort;
    }

    private void UpdateTextFieldWidth(TextField textField) {
        // Calculate approximate width based on text length
        string text = textField.value;
        if (string.IsNullOrEmpty(text)) {
            text = textField.label;
        }
        
        // Approximate character width (adjust multiplier as needed)
        float charWidth = 7.5f;
        float calculatedWidth = Mathf.Max(100, text.Length * charWidth + 20); // +20 for padding
        
        // Cap at a reasonable maximum to prevent extremely wide nodes
        calculatedWidth = Mathf.Min(calculatedWidth, 400);
        
        textField.style.width = calculatedWidth;
    }
}