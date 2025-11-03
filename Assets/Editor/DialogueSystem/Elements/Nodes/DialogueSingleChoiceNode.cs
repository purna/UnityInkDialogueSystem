using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DialogueSingleChoiceNode : DialogueBaseNode {
    protected override DialogueType _type => DialogueType.SingleChoice;

    public override void Initialize(string nodeName, DialogueSystemGraphView graphView, Vector2 position) {
        base.Initialize(nodeName, graphView, position);
        DialogueChoiceSaveData choice = new("Next Dialogue");
        _choices.Add(choice);
    }

    protected override Port CreateChoicePort(object userData) {
        return this.CreatePort(new("Next Dialogue"));
    }
}
