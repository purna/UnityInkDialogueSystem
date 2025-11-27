using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueContainer", menuName = "Dialogue System/Dialogue Container")]

public class DialogueContainer : ScriptableObject {
    [SerializeField] private string _fileName;
    [SerializeField] private SerializableDictionary<DialogueGroup, List<Dialogue>> _groups;
    [SerializeField] private List<Dialogue> _ungroupedDialogues;

    public string FileName => _fileName;
    public SerializableDictionary<DialogueGroup, List<Dialogue>> Groups => _groups;
    public List<Dialogue> UngroupedDialogues => _ungroupedDialogues;

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

   /// <summary>
    /// Get all group names
    /// </summary>
    public string[] GetGroupsNames() {
        return _groups.Keys.Select(group => group.name).ToArray();
    }

    /// <summary>
    /// Get dialogues in a specific group
    /// </summary>
    public List<Dialogue> GetDialoguesInGroup(DialogueGroup group)
    {
        return _groups.ContainsKey(group) ? _groups[group] : new List<Dialogue>();
    }

  /// <summary>
    /// Get dialogue from a specific group with optional starting levels filter
    /// </summary>
    public List<Dialogue> GetGroupedDialoguess(DialogueGroup group, bool startingLevelsOnly = false)
    {
        if (!_groups.ContainsKey(group))
            return new List<Dialogue>();

        var dialogues = _groups[group];

        return new List<Dialogue>(dialogues);
    }

  /// <summary>
    /// Get grouped dialogue names with optional filter
    /// </summary>
    public List<string> GetGroupedDialoguesNames(DialogueGroup dialogueGroup, bool isOnlyStartingDialogues) {
        List<string> dialogues = new();
        foreach (var dialogue in _groups[dialogueGroup]) {
            if (isOnlyStartingDialogues && !dialogue.IsStartingDialogue)
                continue;
            dialogues.Add(dialogue.Name);
        }

        return dialogues;
    }
   /// <summary>
    /// Get ungrouped dialogue names
    /// </summary>
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

    /// <summary>
    /// Get a dialogues from a specific group by name
    /// </summary>
    public Dialogue GetGroupDialogue(string groupName, string dialogueName) {
        
        var group = _groups.Keys.FirstOrDefault(g => g.GroupName == groupName);

        if (group != null && _groups.TryGetValue(group, out var dialogues))
        {
            return dialogues.FirstOrDefault(d => d.Name == dialogueName);
        }

        return null;
    }

      /// <summary>
    /// Get an ungrouped level by name
    /// </summary>
    public Dialogue GetUngroupedDialogue(string dialogueName) {
        return _ungroupedDialogues.FirstOrDefault(d => d.Name == dialogueName);
    }

      /// <summary>
    /// Get all dialogues (both grouped and ungrouped)
    /// </summary>
    public List<Dialogue> GetAllDialogues()
    {
        List<Dialogue> allDialogues = new List<Dialogue>();

        foreach (var group in _groups)
        {
            allDialogues.AddRange(group.Value);
        }

        allDialogues.AddRange(_ungroupedDialogues);

        return allDialogues;
    }

        /// <summary>
    /// Remove an ungrouped level
    /// </summary>
    public void RemoveUngroupedLevel(Dialogue dialgoue)
    {
        _ungroupedDialogues.Remove(dialgoue);
    }

       /// <summary>
    /// Check if container has any groups
    /// </summary>
    public bool HasGroups()
    {
        return _groups.Count > 0;
    }

    
}
