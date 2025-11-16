using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public static class SkillsTreeSystemSaveManager {
    private static SkillsTreeSystemGraphView _graphView;

    private static string _graphFileName;
    private static string _graphFolderPath;

    private static List<SkillsTreeSystemGroup> _groups;
    private static List<SkillsTreeBaseNode> _nodes;
    private static Dictionary<string, SkillsTreeGroup> _createdSkillsTreeGroups;
    private static Dictionary<string, Skill> _createdSkillsTree;

    private static Dictionary<string, SkillsTreeSystemGroup> _loadedGroups;
    private static Dictionary<string, SkillsTreeBaseNode> _loadedNodes;

    public static void Initialize(SkillsTreeSystemGraphView graphView, string graphName) {
        _graphView = graphView;
        _graphFileName = graphName;
        _graphFolderPath = $"Assets/_Project/ScriptableObjects/SkillsTree/{graphName}";

        _groups = new();
        _nodes = new();
        _createdSkillsTreeGroups = new();
        _createdSkillsTree = new();
        _loadedGroups = new();
        _loadedNodes = new();
    }

    #region Save
    public static void Save() {
        CreateStaticFolders();
        GetElementsFromGraphView();

        SkillsTreeSystemGraphSaveData graphData = AssetsUtility.CreateAsset<SkillsTreeSystemGraphSaveData>("Assets/_Project/Editor/SkillsTreeSystem/Graphs", _graphFileName);
        graphData.Initialize(_graphFileName);

        SkillsTreeContainer skillstreeContainer = AssetsUtility.CreateAsset<SkillsTreeContainer>(_graphFolderPath, _graphFileName);
        
        // CRITICAL FIX: Don't reinitialize if it already exists, just update the name
        if (skillstreeContainer.Groups == null || skillstreeContainer.UngroupedSkills == null)
        {
            skillstreeContainer.Initialize(_graphFileName);
        }
        else
        {
            // Clear existing entries to rebuild them
            skillstreeContainer.Groups.Clear();
            skillstreeContainer.UngroupedSkills.Clear();
        }

        SaveGroups(graphData, skillstreeContainer);
        SaveNodes(graphData, skillstreeContainer);

        graphData.Save();
        skillstreeContainer.Save();
    }

    #region Groups
    private static void SaveGroups(SkillsTreeSystemGraphSaveData graphData, SkillsTreeContainer skillstreeContainer) {
        List<string> groupNames = new();
        
        foreach (var group in _groups) {
            SaveGroupToGraph(group, graphData);
            SaveGroupToScriptableObject(group, skillstreeContainer, graphData);
            groupNames.Add(group.title);
        }

        UpdateOldGroups(groupNames, graphData);
    }

    private static void SaveGroupToGraph(SkillsTreeSystemGroup group, SkillsTreeSystemGraphSaveData graphData) {
        SkillsTreeGroupSaveData groupData = new(
            group.ID,
            group.title,
            group.GetPosition().position
        );

        graphData.AddGroup(groupData);
    }

    private static void SaveGroupToScriptableObject(SkillsTreeSystemGroup group, SkillsTreeContainer skillstreeContainer, SkillsTreeSystemGraphSaveData graphData) {
        string groupName = group.title;
        string groupPath = $"{_graphFolderPath}/Groups/{groupName}";
        
        // Check if this group was renamed
        string oldGroupName = FindOldGroupName(group.ID, graphData);
        if (!string.IsNullOrEmpty(oldGroupName) && oldGroupName != groupName)
        {
            // Rename the folder
            string oldGroupPath = $"{_graphFolderPath}/Groups/{oldGroupName}";
            if (AssetDatabase.IsValidFolder(oldGroupPath))
            {
                Debug.Log($"Renaming group folder from '{oldGroupName}' to '{groupName}'");
                FoldersUtility.RenameEditorFolder(oldGroupPath, groupName);
                groupPath = $"{_graphFolderPath}/Groups/{groupName}";
            }
        }
        else
        {
            // Create folders if they don't exist
            FoldersUtility.CreateEditorFolder($"{_graphFolderPath}/Groups", groupName);
            FoldersUtility.CreateEditorFolder($"{_graphFolderPath}/Groups/{groupName}", "SkillsTree");
        }

        SkillsTreeGroup skillstreeGroup = AssetsUtility.CreateAsset<SkillsTreeGroup>($"{_graphFolderPath}/Groups/{groupName}", groupName);
        skillstreeGroup.Initialize(groupName);
        
        // CRITICAL FIX: Always add the group to the container
        skillstreeContainer.AddGroup(skillstreeGroup);
        _createdSkillsTreeGroups.Add(group.ID, skillstreeGroup);

        skillstreeGroup.Save();
    }

    private static string FindOldGroupName(string groupID, SkillsTreeSystemGraphSaveData graphData)
    {
        foreach (var oldGroup in graphData.Groups)
        {
            if (oldGroup.ID == groupID)
            {
                return oldGroup.Name;
            }
        }
        return null;
    }

    private static void UpdateOldGroups(List<string> currentGroupNames, SkillsTreeSystemGraphSaveData graphData) {
        foreach (var oldName in graphData.OldGroupNames) {
            if (currentGroupNames.Contains(oldName))
                continue;

            FoldersUtility.DeleteEditorFolder($"{_graphFolderPath}/Groups/{oldName}");
        }

        graphData.UpdateOldGroupNames(new(currentGroupNames));
    }
    #endregion

    #region Nodes
    private static void SaveNodes(SkillsTreeSystemGraphSaveData graphData, SkillsTreeContainer skillstreeContainer) {
        List<string> nodeNames = new();
        SerializableDictionary<string, List<string>> groupedNodeNames = new();

        foreach (var node in _nodes) {
            SaveNodeToGraph(node, graphData);
            SaveNodeToScriptableObject(node, skillstreeContainer, graphData);
            
            if (node.Group != null) {
                if (groupedNodeNames.ContainsKey(node.Group.ID))
                    groupedNodeNames[node.Group.ID].Add(node.SkillsTreeName);
                else
                    groupedNodeNames[node.Group.ID] = new() { node.SkillsTreeName };
            }
            else
            {
                nodeNames.Add(node.SkillsTreeName);
            }
        }

        UpdateSkillsTreeChoicesConnections();
        UpdateOldGroupedNodes(groupedNodeNames, graphData);
        UpdateOldUngroupedNodes(nodeNames, graphData);
    }

    private static void SaveNodeToGraph(SkillsTreeBaseNode node, SkillsTreeSystemGraphSaveData graphData) {
        List<SkillsTreeChoiceSaveData> choices = CloneNodeChoices(node.Choices);

        SkillsTreeNodeSaveData nodeData = new SkillsTreeNodeSaveData(
            node.ID,
            node.SkillsTreeName,
            node.Text,
            choices,
            node.Group?.ID,
            node.SkillsTreeType,
            node.GetPosition().position
        );

        if (node.Skill != null)
        {
            nodeData.UpdateFromSkillsTree(node.Skill, AssetDatabase.GetAssetPath(node.Skill));
        }

        graphData.AddNode(nodeData);
    }

    private static void SaveNodeToScriptableObject(SkillsTreeBaseNode node, SkillsTreeContainer skillstreeContainer, SkillsTreeSystemGraphSaveData graphData) {
        Skill skillstree;
        string path;
        string skillName = node.SkillsTreeName;

        if (node.Group != null) {
            path = $"{_graphFolderPath}/Groups/{node.Group.title}/SkillsTree";
        } else {
            path = $"{_graphFolderPath}/Global/SkillsTree";
        }

        // Check if node was renamed
        string oldNodeName = FindOldNodeName(node.ID, graphData);
        bool wasRenamed = !string.IsNullOrEmpty(oldNodeName) && oldNodeName != skillName;

        // Try to get the skill from the node's reference first
        skillstree = node.Skill;

        if (skillstree == null)
        {
            // Try loading with old name if it was renamed
            if (wasRenamed)
            {
                skillstree = AssetsUtility.LoadAsset<Skill>(path, oldNodeName);
            }
            
            if (skillstree == null)
            {
                skillstree = AssetsUtility.LoadAsset<Skill>(path, skillName);
            }
        }

        if (skillstree == null) 
        {
            // --- CREATE NEW SKILL ---
            if (node.Group != null) {
                skillstree = AssetsUtility.CreateAsset<Skill>($"{_graphFolderPath}/Groups/{node.Group.title}/SkillsTree", node.SkillsTreeName);
                // CRITICAL FIX: Add to container using the group from _createdSkillsTreeGroups
                if (_createdSkillsTreeGroups.ContainsKey(node.Group.ID))
                {
                    skillstreeContainer.AddSkillToGroup(_createdSkillsTreeGroups[node.Group.ID], skillstree);
                }
            } else {
                skillstree = AssetsUtility.CreateAsset<Skill>($"{_graphFolderPath}/Global/SkillsTree", node.SkillsTreeName);
                // CRITICAL FIX: Add to ungrouped skills
                skillstreeContainer.AddUngroupedSkill(skillstree);
            }

            skillstree.Initialize(
                node.SkillsTreeName,
                node.Text,
                node.Icon,
                node.LockedIcon,
                node.UnlockedIcon,
                node.Tier,
                node.UnlockCost,
                SkillType.Passive,
                node.Value,
                node.MaxLevel,
                new List<Skill>(),
                new List<Skill>(),
                new List<SkillFunction>(),
                node.GetPosition().position
            );
        }
        else
        {
            // --- UPDATE EXISTING SKILL ---
            
            // Handle rename
            if (wasRenamed)
            {
                Debug.Log($"Renaming skill asset from '{oldNodeName}' to '{skillName}'");
                string assetPath = AssetDatabase.GetAssetPath(skillstree);
                AssetDatabase.RenameAsset(assetPath, skillName);
            }
            
            // Handle group change (move to different folder)
            string currentAssetPath = AssetDatabase.GetAssetPath(skillstree);
            string expectedPath = $"{path}/{skillName}.asset";
            
            if (currentAssetPath != expectedPath)
            {
                Debug.Log($"Moving skill asset from '{currentAssetPath}' to '{expectedPath}'");
                string error = AssetDatabase.MoveAsset(currentAssetPath, expectedPath);
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError($"Failed to move asset: {error}");
                }
            }
            
            // CRITICAL FIX: Re-add skill to container in correct location
            if (node.Group != null) {
                if (_createdSkillsTreeGroups.ContainsKey(node.Group.ID))
                {
                    skillstreeContainer.AddSkillToGroup(_createdSkillsTreeGroups[node.Group.ID], skillstree);
                }
            } else {
                skillstreeContainer.AddUngroupedSkill(skillstree);
            }
            
            skillstree.UpdateName(node.SkillsTreeName);
            skillstree.UpdateDescription(node.Text);
            skillstree.UpdatePosition(node.GetPosition().position);
        }

        if (!_createdSkillsTree.ContainsKey(node.ID))
        {
            _createdSkillsTree.Add(node.ID, skillstree);
        }
        
        _graphView.RegisterNodeSkillMapping(node.ID, skillstree);
        node.Skill = skillstree;
        
        skillstree.Save();
    }

    private static string FindOldNodeName(string nodeID, SkillsTreeSystemGraphSaveData graphData)
    {
        foreach (var oldNode in graphData.Nodes)
        {
            if (oldNode.ID == nodeID)
            {
                return oldNode.Name;
            }
        }
        return null;
    }

    private static List<SkillsTreeChoiceSaveData> CloneNodeChoices(IEnumerable<SkillsTreeChoiceSaveData> originalChoices) {
        List<SkillsTreeChoiceSaveData> choices = new();
        foreach (var choice in originalChoices)
            choices.Add(choice.Copy());
        return choices;
    }

    private static void UpdateSkillsTreeChoicesConnections() {
        foreach (var node in _nodes) {
            Skill skillstree = _createdSkillsTree[node.ID];

            for (int i = 0; i < node.Choices.Count(); i++) {
                SkillsTreeChoiceSaveData nodeChoice = node.GetChoice(i);
                if (string.IsNullOrEmpty(nodeChoice.NodeID))
                    continue;

                Skill childSkill = _createdSkillsTree[nodeChoice.NodeID];
                if (childSkill != null && !skillstree.Children.Contains(childSkill)) {
                    skillstree.Children.Add(childSkill);
                    
                    if (!childSkill.Prerequisites.Contains(skillstree)) {
                        childSkill.Prerequisites.Add(skillstree);
                        childSkill.Save();
                    }
                }
                
                skillstree.Save();
            }
        }
    }

    private static void UpdateOldUngroupedNodes(List<string> currentNodeNames, SkillsTreeSystemGraphSaveData graphData) {
        foreach (var oldName in graphData.OldUngroupedNodeNames) {
            if (currentNodeNames.Contains(oldName))
                continue;

            AssetsUtility.RemoveAsset($"{_graphFolderPath}/Global/SkillsTree", oldName);
        }

        graphData.UpdateOldUngroupedNodeNames(new(currentNodeNames));
    }

    private static void UpdateOldGroupedNodes(SerializableDictionary<string, List<string>> currentGroupedNodeNames, SkillsTreeSystemGraphSaveData graphData) {
        foreach (var oldGroupedNode in graphData.OldGroupedNodeNames) {
            if (!currentGroupedNodeNames.ContainsKey(oldGroupedNode.Key))
                continue;

            foreach (var groupedNode in oldGroupedNode.Value) {
                if (currentGroupedNodeNames[oldGroupedNode.Key].Contains(groupedNode))
                    continue;

                AssetsUtility.RemoveAsset($"{_graphFolderPath}/Groups/{oldGroupedNode.Key}/SkillsTree", groupedNode);
            }
        }

        graphData.UpdateOldGroupedNodeNames(new(currentGroupedNodeNames));
    }
    #endregion

    private static void GetElementsFromGraphView() {
        _graphView.graphElements.ForEach(graphElement => {
            if (graphElement is SkillsTreeBaseNode node)
                _nodes.Add(node);
            else if (graphElement is SkillsTreeSystemGroup group)
                _groups.Add(group);
        });
    }
    #endregion

    #region Load
    public static void Load()
    {
        SkillsTreeSystemGraphSaveData graphData = AssetsUtility.LoadAsset<SkillsTreeSystemGraphSaveData>("Assets/_Project/Editor/SkillsTreeSystem/Graphs", _graphFileName);
        if (graphData == null)
        {
            EditorUtility.DisplayDialog(
                "Cannot load the file!",
                $"File {_graphFileName}.asset cannot be found. Change name and try again.",
                "Ok"
            );
            return;
        }

        SkillsTreeContainer skillstreeContainer = AssetsUtility.LoadAsset<SkillsTreeContainer>(_graphFolderPath, _graphFileName);
        if (skillstreeContainer == null)
        {
            EditorUtility.DisplayDialog(
                "Cannot load the skillstree container!",
                $"File {_graphFileName}.asset cannot be found in {_graphFolderPath}.",
                "Ok"
            );
            return;
        }

        SkillsTreeSystemEditorWindow.UpdateFileName(graphData.FileName);

        LoadGroups(graphData);
        LoadNodes(graphData, skillstreeContainer);
        LoadNodesConnections();
    }
    
    private static void LoadNodes(SkillsTreeSystemGraphSaveData graphData, SkillsTreeContainer skillstreeContainer)
    {
        foreach (var nodeData in graphData.Nodes)
        {
            List<SkillsTreeChoiceSaveData> choices = CloneNodeChoices(nodeData.Choices);
            SkillsTreeBaseNode node = _graphView.CreateNode(nodeData.Name, nodeData.SkillsTreeType, nodeData.Position, false);
            node.Setup(nodeData, choices);

            Skill skillstree = null;
            if (!string.IsNullOrEmpty(nodeData.GroupID))
            {
                if (_loadedGroups.ContainsKey(nodeData.GroupID))
                {
                    skillstree = skillstreeContainer.GetGroupSkill(_loadedGroups[nodeData.GroupID].title, nodeData.Name);
                }
            }
            else
            {
                skillstree = skillstreeContainer.GetUngroupedSkill(nodeData.Name);
            }

            if (skillstree != null)
            {
                node.Skill = skillstree;
                node.RefreshFromSkill();
                _graphView.RegisterNodeSkillMapping(node.ID, skillstree);
                SkillChangeMonitor.TrackSkill(skillstree);
            }

            node.Draw();
            _loadedNodes.Add(node.ID, node);

            if (string.IsNullOrEmpty(nodeData.GroupID))
                continue;

            SkillsTreeSystemGroup group = _loadedGroups[nodeData.GroupID];
            node.ChangeGroup(group);
            group.AddElement(node);
        }
    }

    private static void LoadGroups(SkillsTreeSystemGraphSaveData graphData) {
        foreach (var groupData in graphData.Groups) {
            SkillsTreeSystemGroup group = _graphView.CreateGroup(groupData.Name, groupData.Position);
            group.SetID(groupData.ID);
            _loadedGroups.Add(groupData.ID, group);
        }
    }

    private static void LoadNodesConnections()
    {
        foreach (var loadedNode in _loadedNodes)
        {
            foreach (var element in loadedNode.Value.outputContainer.Children())
            {
                if (element is not Port choicePort)
                    continue;

                SkillsTreeChoiceSaveData choiceData = (SkillsTreeChoiceSaveData)choicePort.userData;
                if (string.IsNullOrEmpty(choiceData.NodeID))
                    continue;

                SkillsTreeBaseNode nextNode = _loadedNodes[choiceData.NodeID];
                Port nextNodeInputPort = (Port)nextNode.inputContainer.Children().First();
                _graphView.AddElement(choicePort.ConnectTo(nextNodeInputPort));
            }
            loadedNode.Value.RefreshPorts();
        }
    }
    #endregion

    private static void CreateStaticFolders() {
        FoldersUtility.CreateEditorFolder("Assets/_Project/Editor/SkillsTreeSystem", "Graphs");
        FoldersUtility.CreateEditorFolder("Assets/_Project", "ScriptableObjects");
        FoldersUtility.CreateEditorFolder("Assets/_Project/ScriptableObjects", "SkillsTree");
        FoldersUtility.CreateEditorFolder("Assets/_Project/ScriptableObjects/SkillsTree", _graphFileName);

        FoldersUtility.CreateEditorFolder(_graphFolderPath, "Global");
        FoldersUtility.CreateEditorFolder(_graphFolderPath, "Groups");
        FoldersUtility.CreateEditorFolder($"{_graphFolderPath}/Global", "SkillsTree");
    }
    
    /// <summary>
    /// Handles renaming the container (graph) itself
    /// </summary>
    public static void RenameContainer(string oldName, string newName)
    {
        string oldPath = $"Assets/_Project/ScriptableObjects/SkillsTree/{oldName}";
        string newPath = $"Assets/_Project/ScriptableObjects/SkillsTree/{newName}";
        
        if (AssetDatabase.IsValidFolder(oldPath))
        {
            Debug.Log($"Renaming container folder from '{oldName}' to '{newName}'");
            FoldersUtility.RenameEditorFolder(oldPath, newName);
        }
        
        // Rename the graph save data asset
        AssetsUtility.RenameAsset("Assets/_Project/Editor/SkillsTreeSystem/Graphs", oldName, newName);
        
        // Rename the container asset
        AssetsUtility.RenameAsset(newPath, oldName, newName);
    }
}