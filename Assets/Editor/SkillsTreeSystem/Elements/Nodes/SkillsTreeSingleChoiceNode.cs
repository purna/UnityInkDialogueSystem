using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SkillsTreeSingleChoiceNode : SkillsTreeBaseNode {
    protected override SkillsTreeType _type => SkillsTreeType.SingleChoice;

    public override void Initialize(string nodeName, SkillsTreeSystemGraphView graphView, Vector2 position) {
        base.Initialize(nodeName, graphView, position);
        SkillsTreeChoiceSaveData choice = new("Next Tier");
        _choices.Add(choice);
    }

    protected override Port CreateChoicePort(object userData) {
        return this.CreatePort(new("Next Tier"));
    }
}
