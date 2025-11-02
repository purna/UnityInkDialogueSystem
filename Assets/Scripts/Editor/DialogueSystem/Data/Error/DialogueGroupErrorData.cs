using System.Collections.Generic;

public class DialogueGroupErrorData {
    private readonly DialogueSystemErrorData _errorData;
    private readonly List<DialogueSystemGroup> _groups;
    private bool _isError;

    public bool IsError => _isError;

    public DialogueGroupErrorData() {
        _errorData = new();
        _groups = new();
    }

    public bool IsEmpty() {
        return _groups.Count == 0;
    }

    public void AddGroup(DialogueSystemGroup group) {
        _groups.Add(group);
        UpdateError();
    }

    public void RemoveGroup(DialogueSystemGroup group) {
        if (!_groups.Contains(group))
            return;

        _groups.Remove(group);
        UpdateError();
    }

    private void UpdateError() {
        _isError = _groups.Count >= 2;
        UpdateGroupsColor();
    }

    private void UpdateGroupsColor() {
        if (_isError) {
            foreach (var group in _groups)
                group.SetErrorStyle(_errorData.Color);
            return;
        }

        foreach (var node in _groups)
            node.ResetStyle();
    }
}
