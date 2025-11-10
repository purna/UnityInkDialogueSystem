using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class LevelSingleChoiceNode : LevelBaseNode {
    protected override LevelType _type => LevelType.SingleChoice;

    public override void Initialize(string nodeName, LevelSystemGraphView graphView, Vector2 position) {
        base.Initialize(nodeName, graphView, position);
        LevelChoiceSaveData choice = new("Next Tier");
        _choices.Add(choice);
    }

    protected override Port CreateChoicePort(object userData) {
        return this.CreatePort(new("Next Tier"));
    }
}
