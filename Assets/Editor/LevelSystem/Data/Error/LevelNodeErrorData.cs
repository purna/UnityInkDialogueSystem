using System.Collections.Generic;

public class LevelNodeErrorData {
    private LevelSystemErrorData _errorData;
    private List<LevelBaseNode> _nodes;
    private bool _isError;

    public bool IsError => _isError;

    public LevelNodeErrorData() {
        _errorData = new();
        _nodes = new();
    }

    public bool IsEmpty() {
        return _nodes.Count == 0;
    }

    public void AddNode(LevelBaseNode node) {
        _nodes.Add(node);
        UpdateError();
    }

    public void RemoveNode(LevelBaseNode node) {
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
