using System;
using UnityEngine;

[Serializable]
public class LevelChoiceSaveData {
    [SerializeField] private string _text;
    [SerializeField] private string _nodeID;

    public string Text => _text;
    public string NodeID => _nodeID;

    public LevelChoiceSaveData(string text, string nodeId = null) {
        _text = text;
        _nodeID = nodeId;
    }

    public void SetText(string text) {
        _text = text;
    }

    public void SetNode(LevelBaseNode nextNode) {
        _nodeID = nextNode.ID;
    }

    public void ResetNode() {
        _nodeID = "";
    }

    public LevelChoiceSaveData Copy() {
        return new LevelChoiceSaveData(_text, _nodeID);
    }

    public LevelChoiceData ToLevelChoice() {
        return new(_text);
    }
}
