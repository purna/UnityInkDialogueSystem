using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

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
        skillstreeContainer.Initialize(_graphFileName);

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
            SaveGroupToScriptableObject(group, skillstreeContainer);
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

    private static void SaveGroupToScriptableObject(SkillsTreeSystemGroup group, SkillsTreeContainer skillstreeContainer) {
        string groupName = group.title;
        FoldersUtility.CreateEditorFolder($"{_graphFolderPath}/Groups", groupName);
        FoldersUtility.CreateEditorFolder($"{_graphFolderPath}/Groups/{groupName}", "SkillsTree");

        SkillsTreeGroup skillstreeGroup = AssetsUtility.CreateAsset<SkillsTreeGroup>($"{_graphFolderPath}/Groups/{groupName}", groupName);
        skillstreeGroup.Initialize(groupName);
        skillstreeContainer.AddGroup(skillstreeGroup);
        _createdSkillsTreeGroups.Add(group.ID, skillstreeGroup);

        skillstreeGroup.Save();
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
            SaveNodeToScriptableObject(node, skillstreeContainer);
            if (node.Group != null) {
                if (groupedNodeNames.ContainsKey(node.Group.ID))
                    groupedNodeNames[node.Group.ID].Add(node.SkillsTreeName);
                else
                    groupedNodeNames[node.Group.ID] = new() { node.SkillsTreeName };
                continue;
            }

            nodeNames.Add(node.SkillsTreeName);
        }

        UpdateSkillsTreeChoicesConnections();
        UpdateOldGroupedNodes(groupedNodeNames, graphData);
        UpdateOldUngroupedNodes(nodeNames, graphData);
    }

private static void SaveNodeToGraph(SkillsTreeBaseNode node, SkillsTreeSystemGraphSaveData graphData) {
    List<SkillsTreeChoiceSaveData> choices = CloneNodeChoices(node.Choices);

    // Create the base save data
    SkillsTreeNodeSaveData nodeData = new SkillsTreeNodeSaveData(
        node.ID,
        node.SkillsTreeName,
        node.Text,
        choices,
        node.Group?.ID,
        node.SkillsTreeType,
        node.GetPosition().position
    );

    // NOW, update it with the skill data from the SO
    // This ensures the save data matches the SO
    if (node.Skill != null)
    {
        // This method already exists in SkillsTreeNodeSaveData!
        nodeData.UpdateFromSkillsTree(node.Skill, AssetDatabase.GetAssetPath(node.Skill));
    }

    graphData.AddNode(nodeData);
}


    // In SkillsTreeSystemSaveManager.cs

private static void SaveNodeToScriptableObject(SkillsTreeBaseNode node, SkillsTreeContainer skillstreeContainer) {
    Skill skillstree;
    string path;
    string skillName = node.SkillsTreeName;

    if (node.Group != null) {
        path = $"{_graphFolderPath}/Groups/{node.Group.title}/SkillsTree";
    } else {
        path = $"{_graphFolderPath}/Global/SkillsTree";
    }

    // Try to get the skill from the node's reference first
    // This is the SO that the inspector was modifying
    skillstree = node.Skill; 

    if (skillstree == null)
    {
        // No reference, try to load from asset path (legacy/safety check)
        skillstree = AssetsUtility.LoadAsset<Skill>(path, skillName);
    }

    if (skillstree == null) 
    {
        // --- CREATE NEW SKILL ---
        // Asset truly doesn't exist, create a new one
        if (node.Group != null) {
            skillstree = AssetsUtility.CreateAsset<Skill>($"{_graphFolderPath}/Groups/{node.Group.title}/SkillsTree", node.SkillsTreeName);
            skillstreeContainer.AddSkillToGroup(_createdSkillsTreeGroups[node.Group.ID], skillstree);
        } else {
            skillstree = AssetsUtility.CreateAsset<Skill>($"{_graphFolderPath}/Global/SkillsTree", node.SkillsTreeName);
            skillstreeContainer.AddUngroupedSkill(skillstree);
        }

        // Initialize the new skill using data from the node
        // (This assumes SkillsTreeBaseNode has these properties)
        skillstree.Initialize(
            node.SkillsTreeName,
            node.Text, // node.Text is the description
            node.Icon,
            node.LockedIcon,
            node.UnlockedIcon,
            node.Tier,
            node.UnlockCost,
            SkillType.Passive, // TODO: Get this from node if possible
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
       
        // TODO: Handle asset rename if node.SkillsTreeName != skillstree.SkillName
        // AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(skillstree), node.SkillsTreeName);
        
        // Use the new methods you added to Skill.cs
        skillstree.UpdateName(node.SkillsTreeName);
        skillstree.UpdateDescription(node.Text);
        skillstree.UpdatePosition(node.GetPosition().position);
    }

    // Add to dictionary for connection-linking
    if (!_createdSkillsTree.ContainsKey(node.ID))
    {
        _createdSkillsTree.Add(node.ID, skillstree);
    }
    
    _graphView.RegisterNodeSkillMapping(node.ID, skillstree);
    
    // This is the most important line, ensuring the node
    // holds the reference to the single source of truth.
    node.Skill = skillstree; 
    
    skillstree.Save();
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

                // Add the connected skill as a child
                Skill childSkill = _createdSkillsTree[nodeChoice.NodeID];
                if (childSkill != null && !skillstree.Children.Contains(childSkill)) {
                    skillstree.Children.Add(childSkill);
                    
                    // Also add this skill as a prerequisite to the child
                    if (!childSkill.Prerequisites.Contains(skillstree)) {
                        childSkill.Prerequisites.Add(skillstree);
                        childSkill.Save();
                    }
                }
                
                skillstree.Save();
            }
        }
    }

    private static List<SkillsTreeChoiceData> ConvertNodeChoicesToSkillsTreeChoices(IEnumerable<SkillsTreeChoiceSaveData> nodeChoices) {
        List<SkillsTreeChoiceData> skillstreeChoices = new();
        foreach (var nodeChoice in nodeChoices)
            skillstreeChoices.Add(nodeChoice.ToSkillsTreeChoice());
        return skillstreeChoices;
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

        // Load the corresponding Skill ScriptableObject
        Skill skillstree = null;
        if (!string.IsNullOrEmpty(nodeData.GroupID))
        {
            // Check _loadedGroups, not graphData, as graphData.Groups might not be in order
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
            // --- REVISED LOAD LOGIC ---
            
            // 1. Set the skill reference on the node
            node.Skill = skillstree;
            
            // 2. Use your existing extension method to pull all data from skill to node
            node.RefreshFromSkill(); 
            
            // 3. Register mapping
            _graphView.RegisterNodeSkillMapping(node.ID, skillstree);
            
            // 4. Track for further changes
            SkillChangeMonitor.TrackSkill(skillstree);
        }

        // Draw AFTER applying the data
        node.Draw();

        _loadedNodes.Add(node.ID, node);

        if (string.IsNullOrEmpty(nodeData.GroupID))
            continue;

        SkillsTreeSystemGroup group = _loadedGroups[nodeData.GroupID];
        node.ChangeGroup(group);
        group.AddElement(node);
    }
}

    /*
    private static void ApplySkillDataToNode(SkillsTreeBaseNode node, Skill skillstree)
    {
        // Apply Skill data to the node
        node.Text = skillstree.Description;
        
        // You can add more data mapping here as needed
        // For example:
        // node.SomeField = skillstree.UnlockCost;
        // node.AnotherField = skillstree.Tier;
    }
    */

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
        FoldersUtility.CreateEditorFolder("Assets", "ScriptableObjects");
        FoldersUtility.CreateEditorFolder("Assets/_Project/ScriptableObjects", "SkillsTree");
        FoldersUtility.CreateEditorFolder("Assets/_Project/ScriptableObjects/SkillsTree", _graphFileName);

        FoldersUtility.CreateEditorFolder(_graphFolderPath, "Global");
        FoldersUtility.CreateEditorFolder(_graphFolderPath, "Groups");
        FoldersUtility.CreateEditorFolder($"{_graphFolderPath}/Global", "SkillsTree");
    }
}