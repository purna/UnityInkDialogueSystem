// ============================================================================
// SKILLS TREE SYSTEM GRAPH SAVE DATA
// ============================================================================
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SkillsTreeSystemGraphSaveData : ScriptableObject {
    [SerializeField] private string _fileName;
    [SerializeField] private List<SkillsTreeGroupSaveData> _groups;
    [SerializeField] private List<SkillsTreeNodeSaveData> _nodes;
    [SerializeField] private List<string> _oldGroupNames;
    [SerializeField] private List<string> _oldUngroupedNodeNames;
    [SerializeField] private SerializableDictionary<string, List<string>> _oldGroupedNodeNames;

    public string FileName => _fileName;
    public IEnumerable<SkillsTreeGroupSaveData> Groups => _groups;
    public IEnumerable<SkillsTreeNodeSaveData> Nodes => _nodes;
    public IEnumerable<string> OldGroupNames => _oldGroupNames;
    public IEnumerable<KeyValuePair<string, List<string>>> OldGroupedNodeNames => _oldGroupedNodeNames;
    public IEnumerable<string> OldUngroupedNodeNames => _oldUngroupedNodeNames;

    public void Initialize(string fileName) {
        _fileName = fileName;
        _groups = new();
        _nodes = new();
        _oldGroupNames = new();
        _oldUngroupedNodeNames = new();
        _oldGroupedNodeNames = new();
    }

    public void AddGroup(SkillsTreeGroupSaveData groupData) {
        _groups.Add(groupData);
    }

    public void AddNode(SkillsTreeNodeSaveData nodeData) {
        _nodes.Add(nodeData);
    }

    public void UpdateOldGroupNames(List<string> newNames) {
        _oldGroupNames = newNames;
    }

    public void UpdateOldUngroupedNodeNames(List<string> newNames) {
        _oldUngroupedNodeNames = newNames;
    }

    public void UpdateOldGroupedNodeNames(SerializableDictionary<string, List<string>> newNames) {
        _oldGroupedNodeNames = newNames;
    }
    
    // FIX: Add the missing Save() method
    public void Save() {
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
#endif
    }
}