using UnityEngine;

public class DialogueGroup : ScriptableObject {
    [SerializeField] private string _groupName;

    public string GroupName => _groupName;

    public void Initialize(string groupName) {
        _groupName = groupName;
    }
}
