using System;
using UnityEngine;

[Serializable]
public class SkillsTreeChoiceSaveData {
    [SerializeField] private string _text;
    [SerializeField] private string _nodeID;

    public string Text => _text;
    public string NodeID => _nodeID;

    public SkillsTreeChoiceSaveData(string text, string nodeId = null) {
        _text = text;
        _nodeID = nodeId;
    }

    public void SetText(string text) {
        _text = text;
    }

    public void SetNode(SkillsTreeBaseNode nextNode) {
        _nodeID = nextNode.ID;
    }

    public void ResetNode() {
        _nodeID = "";
    }

    public SkillsTreeChoiceSaveData Copy() {
        return new SkillsTreeChoiceSaveData(_text, _nodeID);
    }

    public SkillsTreeChoiceData ToSkillsTreeChoice() {
        return new(_text);
    }
}
