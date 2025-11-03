using System;
using UnityEngine;

[Serializable]
public class DialogueChoiceSaveData {
    [SerializeField] private string _text;
    [SerializeField] private string _nodeID;

    public string Text => _text;
    public string NodeID => _nodeID;

    public DialogueChoiceSaveData(string text, string nodeId = null) {
        _text = text;
        _nodeID = nodeId;
    }

    public void SetText(string text) {
        _text = text;
    }

    public void SetNode(DialogueBaseNode nextNode) {
        _nodeID = nextNode.ID;
    }

    public void ResetNode() {
        _nodeID = "";
    }

    public DialogueChoiceSaveData Copy() {
        return new DialogueChoiceSaveData(_text, _nodeID);
    }

    public DialogueChoiceData ToDialogueChoice() {
        return new(_text);
    }
}
