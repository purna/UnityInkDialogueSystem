using System.Collections.Generic;

public class DialogueNodeErrorData {
    private DialogueSystemErrorData _errorData;
    private List<DialogueBaseNode> _nodes;
    private bool _isError;

    public bool IsError => _isError;

    public DialogueNodeErrorData() {
        _errorData = new();
        _nodes = new();
    }

    public bool IsEmpty() {
        return _nodes.Count == 0;
    }

    public void AddNode(DialogueBaseNode node) {
        _nodes.Add(node);
        UpdateError();
    }

    public void RemoveNode(DialogueBaseNode node) {
        if (!_nodes.Contains(node))
            return;

        _nodes.Remove(node);
        UpdateError();
    }

    private void UpdateError() {
        _isError = _nodes.Count >= 2;
        UpdateNodesColor();
    }

    private void UpdateNodesColor() {
        if (_isError) {
            foreach (var node in _nodes)
                node.SetErrorStyle(_errorData.Color);
            return;
        }

        foreach (var node in _nodes)
            node.ResetStyle();
    }
}
