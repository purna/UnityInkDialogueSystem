using UnityEngine;

/// <summary>
/// Represents a group/category of dialgoues in the dialogue
/// Similar to DialogueGroup in your dialogue system
/// </summary>
[CreateAssetMenu(fileName = "DialogueGroup", menuName = "Dialogue/Group")]

public class DialogueGroup : ScriptableObject {
    [SerializeField] private string _groupName;

    public string GroupName => _groupName;

    public void Initialize(string groupName) {
        _groupName = groupName;
    }
}
