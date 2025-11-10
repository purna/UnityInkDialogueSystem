using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogueContainer : ScriptableObject {
    [SerializeField] private string _fileName;
    [SerializeField] private SerializableDictionary<DialogueGroup, List<Dialogue>> _groups;
    [SerializeField] private List<Dialogue> _ungroupedDialogues;

    public string FileName => _fileName;

    public void Initialize(string fileName) {
        _fileName = fileName;
        _groups = new();
        _ungroupedDialogues = new();
    }

    public void AddGroup(DialogueGroup group) {
        _groups.Add(group, new());
    }

    public void AddGroupDialogue(DialogueGroup dialogueGroup, Dialogue dialogue) {
        if (!_groups.ContainsKey(dialogueGroup))
            AddGroup(dialogueGroup);

        _groups[dialogueGroup].Add(dialogue);
    }

    public void AddUngroupDialogue(Dialogue dialogue) {
        _ungroupedDialogues.Add(dialogue);
    }

    public bool HaveGroups() {
        return _groups.Count > 0;
    }

    public string[] GetGroupsNames() {
        return _groups.Keys.Select(group => group.name).ToArray();
    }

    public List<string> GetGroupedDialoguesNames(DialogueGroup dialogueGroup, bool isOnlyStartingDialogues) {
        List<string> dialogues = new();
        foreach (var dialogue in _groups[dialogueGroup]) {
            if (isOnlyStartingDialogues && !dialogue.IsStartingDialogue)
                continue;
            dialogues.Add(dialogue.Name);
        }

        return dialogues;
    }

    public List<string> GetUngroupedDialoguesNames(bool isOnlyStartingDialogues) {
        List<string> dialogues = new();
        foreach (var dialogue in _ungroupedDialogues) {
            if (isOnlyStartingDialogues && !dialogue.IsStartingDialogue)
                continue;
            dialogues.Add(dialogue.Name);
        }

        return dialogues;
    }

    public Dialogue GetDialogueByName(string dialogueName) {
        // Search in grouped dialogues
        foreach (var group in _groups) {
            foreach (var dialogue in group.Value) {
                if (dialogue.Name == dialogueName) {
                    return dialogue;
                }
            }
        }

        // Search in ungrouped dialogues
        foreach (var dialogue in _ungroupedDialogues) {
            if (dialogue.Name == dialogueName) {
                return dialogue;
            }
        }

        // Dialogue not found
        return null;
    }

    public Dialogue GetGroupDialogue(string groupName, string dialogueName) {
        if (_groups.TryGetValue(_groups.Keys.FirstOrDefault(g => g.name == groupName), out var dialogues)) {
            return dialogues.FirstOrDefault(d => d.Name == dialogueName);
        }
        return null;
    }

    public Dialogue GetUngroupedDialogue(string dialogueName) {
        return _ungroupedDialogues.FirstOrDefault(d => d.Name == dialogueName);
    }
}
