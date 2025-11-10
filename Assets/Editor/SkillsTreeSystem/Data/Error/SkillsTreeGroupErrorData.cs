using System.Collections.Generic;

public class SkillsTreeGroupErrorData {
    private readonly SkillsTreeSystemErrorData _errorData;
    private readonly List<SkillsTreeSystemGroup> _groups;
    private bool _isError;

    public bool IsError => _isError;

    public SkillsTreeGroupErrorData() {
        _errorData = new();
        _groups = new();
    }

    public bool IsEmpty() {
        return _groups.Count == 0;
    }

    public void AddGroup(SkillsTreeSystemGroup group) {
        _groups.Add(group);
        UpdateError();
    }

    public void RemoveGroup(SkillsTreeSystemGroup group) {
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
