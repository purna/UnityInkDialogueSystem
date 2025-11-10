using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

public static class LevelSystemSaveManager {
    private static LevelSystemGraphView _graphView;

    private static string _graphFileName;
    private static string _graphFolderPath;

    private static List<LevelSystemGroup> _groups;
    private static List<LevelBaseNode> _nodes;
    private static Dictionary<string, LevelGroup> _createdLevelGroups;
    private static Dictionary<string, Level> _createdLevels;

    private static Dictionary<string, LevelSystemGroup> _loadedGroups;
    private static Dictionary<string, LevelBaseNode> _loadedNodes;

    public static void Initialize(LevelSystemGraphView graphView, string graphName) {
        _graphView = graphView;
        _graphFileName = graphName;
        _graphFolderPath = $"Assets/_Project/ScriptableObjects/Level/{graphName}";

        _groups = new();
        _nodes = new();
        _createdLevelGroups = new();
        _createdLevels = new();
        _loadedGroups = new();
        _loadedNodes = new();
    }

    #region Save
    public static void Save() {
        CreateStaticFolders();
        GetElementsFromGraphView();

        LevelSystemGraphSaveData graphData = AssetsUtility.CreateAsset<LevelSystemGraphSaveData>("Assets/_Project/Editor/LevelSystem/Graphs", _graphFileName);
        graphData.Initialize(_graphFileName);

        LevelContainer levelContainer = AssetsUtility.CreateAsset<LevelContainer>(_graphFolderPath, _graphFileName);
        levelContainer.Initialize(_graphFileName);

        SaveGroups(graphData, levelContainer);
        SaveNodes(graphData, levelContainer);

        graphData.Save();
        levelContainer.Save();
    }

    #region Groups
    private static void SaveGroups(LevelSystemGraphSaveData graphData, LevelContainer levelContainer) {
        List<string> groupNames = new();
        foreach (var group in _groups) {
            SaveGroupToGraph(group, graphData);
            SaveGroupToScriptableObject(group, levelContainer);
            groupNames.Add(group.title);
        }

        UpdateOldGroups(groupNames, graphData);
    }

    private static void SaveGroupToGraph(LevelSystemGroup group, LevelSystemGraphSaveData graphData) {
        LevelGroupSaveData groupData = new(
            group.ID,
            group.title,
            group.GetPosition().position
        );

        graphData.AddGroup(groupData);
    }

    private static void SaveGroupToScriptableObject(LevelSystemGroup group, LevelContainer levelContainer) {
        string groupName = group.title;
        FoldersUtility.CreateEditorFolder($"{_graphFolderPath}/Groups", groupName);
        FoldersUtility.CreateEditorFolder($"{_graphFolderPath}/Groups/{groupName}", "Level");

        LevelGroup levelGroup = AssetsUtility.CreateAsset<LevelGroup>($"{_graphFolderPath}/Groups/{groupName}", groupName);
        levelGroup.Initialize(groupName);
        levelContainer.AddGroup(levelGroup);
        _createdLevelGroups.Add(group.ID, levelGroup);

        levelGroup.Save();
    }

    private static void UpdateOldGroups(List<string> currentGroupNames, LevelSystemGraphSaveData graphData) {
        foreach (var oldName in graphData.OldGroupNames) {
            if (currentGroupNames.Contains(oldName))
                continue;

            FoldersUtility.DeleteEditorFolder($"{_graphFolderPath}/Groups/{oldName}");
        }

        graphData.UpdateOldGroupNames(new(currentGroupNames));
    }
    #endregion

    #region Nodes
    private static void SaveNodes(LevelSystemGraphSaveData graphData, LevelContainer levelContainer) {
        List<string> nodeNames = new();
        SerializableDictionary<string, List<string>> groupedNodeNames = new();

        foreach (var node in _nodes) {
            SaveNodeToGraph(node, graphData);
            SaveNodeToScriptableObject(node, levelContainer);
            if (node.Group != null) {
                if (groupedNodeNames.ContainsKey(node.Group.ID))
                    groupedNodeNames[node.Group.ID].Add(node.LevelName);
                else
                    groupedNodeNames[node.Group.ID] = new() { node.LevelName };
                continue;
            }

            nodeNames.Add(node.LevelName);
        }

        UpdateLevelChoicesConnections();
        UpdateOldGroupedNodes(groupedNodeNames, graphData);
        UpdateOldUngroupedNodes(nodeNames, graphData);
    }

    private static void SaveNodeToGraph(LevelBaseNode node, LevelSystemGraphSaveData graphData) {
        List<LevelChoiceSaveData> choices = CloneNodeChoices(node.Choices);

        // Create the base save data
        LevelNodeSaveData nodeData = new LevelNodeSaveData(
            node.ID,
            node.LevelName,
            node.Text,
            choices,
            node.Group?.ID,
            node.LevelType,
            node.GetPosition().position
        );

        // NOW, update it with the Level data from the SO
        // This ensures the save data matches the SO
        if (node.Level != null)
        {
            // This method already exists in LevelNodeSaveData!
            nodeData.UpdateFromLevel(node.Level, AssetDatabase.GetAssetPath(node.Level));
        }

        graphData.AddNode(nodeData);
    }

    private static void SaveNodeToScriptableObject(LevelBaseNode node, LevelContainer levelContainer) {
        Level level;
        string path;
        string levelName = node.LevelName;

        if (node.Group != null) {
            path = $"{_graphFolderPath}/Groups/{node.Group.title}/Level";
        } else {
            path = $"{_graphFolderPath}/Global/Level";
        }

        // Try to get the Level from the node's reference first
        // This is the SO that the inspector was modifying
        level = node.Level; 

        if (level == null)
        {
            // No reference, try to load from asset path (legacy/safety check)
            level = AssetsUtility.LoadAsset<Level>(path, levelName);
        }

        if (level == null) 
        {
            // --- CREATE NEW Level ---
            // Asset truly doesn't exist, create a new one
            if (node.Group != null) {
                level = AssetsUtility.CreateAsset<Level>($"{_graphFolderPath}/Groups/{node.Group.title}/Level", node.LevelName);
                levelContainer.AddLevel(level);
                levelContainer.AddLevelToGroup(level, _createdLevelGroups[node.Group.ID]);
            } else {
                level = AssetsUtility.CreateAsset<Level>($"{_graphFolderPath}/Global/Level", node.LevelName);
                levelContainer.AddLevel(level);
            }

            // Initialize the new Level using data from the node
            level.Initialize(
                node.LevelName,
                node.Text, // node.Text is the description
                node.Icon,
                node.LockedIcon,
                node.UnlockedIcon,
                node.CompletedIcon,
                node.Tier,
                node.LevelIndex,
                node.CompletionThreshold,
                node.MaxAttempts,
                new List<Level>(),
                new List<Level>(),
                node.GetPosition().position
            );
        }
        else
        {
            // TODO: Handle asset rename if node.LevelName != level.LevelName
            // AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(level), node.LevelName);
            
            // Use the new methods you added to Level.cs
            level.UpdateName(node.LevelName);
            level.UpdateDescription(node.Text);
            level.UpdatePosition(node.GetPosition().position);
        }

        // Add to dictionary for connection-linking
        if (!_createdLevels.ContainsKey(node.ID))
        {
            _createdLevels.Add(node.ID, level);
        }
        
        _graphView.RegisterNodeLevelMapping(node.ID, level);
        
        // This is the most important line, ensuring the node
        // holds the reference to the single source of truth.
        node.Level = level; 
        
        level.Save();
    }

    private static List<LevelChoiceSaveData> CloneNodeChoices(IEnumerable<LevelChoiceSaveData> originalChoices) {
        List<LevelChoiceSaveData> choices = new();
        foreach (var choice in originalChoices)
            choices.Add(choice.Copy());
        return choices;
    }

    private static void UpdateLevelChoicesConnections() {
        foreach (var node in _nodes) {
            Level level = _createdLevels[node.ID];

            for (int i = 0; i < node.Choices.Count(); i++) {
                LevelChoiceSaveData nodeChoice = node.GetChoice(i);
                if (string.IsNullOrEmpty(nodeChoice.NodeID))
                    continue;

                // Add the connected Level as a child
                Level childLevel = _createdLevels[nodeChoice.NodeID];
                if (childLevel != null && !level.Children.Contains(childLevel)) {
                    level.Children.Add(childLevel);
                    
                    // Also add this Level as a prerequisite to the child
                    if (!childLevel.Prerequisites.Contains(level)) {
                        childLevel.Prerequisites.Add(level);
                        childLevel.Save();
                    }
                }
                
                level.Save();
            }
        }
    }

    private static List<LevelChoiceData> ConvertNodeChoicesToLevelChoices(IEnumerable<LevelChoiceSaveData> nodeChoices) {
        List<LevelChoiceData> levelChoices = new();
        foreach (var nodeChoice in nodeChoices)
            levelChoices.Add(nodeChoice.ToLevelChoice());
        return levelChoices;
    }

    private static void UpdateOldUngroupedNodes(List<string> currentNodeNames, LevelSystemGraphSaveData graphData) {
        foreach (var oldName in graphData.OldUngroupedNodeNames) {
            if (currentNodeNames.Contains(oldName))
                continue;

            AssetsUtility.RemoveAsset($"{_graphFolderPath}/Global/Level", oldName);
        }

        graphData.UpdateOldUngroupedNodeNames(new(currentNodeNames));
    }

    private static void UpdateOldGroupedNodes(SerializableDictionary<string, List<string>> currentGroupedNodeNames, LevelSystemGraphSaveData graphData) {
        foreach (var oldGroupedNode in graphData.OldGroupedNodeNames) {
            if (!currentGroupedNodeNames.ContainsKey(oldGroupedNode.Key))
                continue;

            foreach (var groupedNode in oldGroupedNode.Value) {
                if (currentGroupedNodeNames[oldGroupedNode.Key].Contains(groupedNode))
                    continue;

                AssetsUtility.RemoveAsset($"{_graphFolderPath}/Groups/{oldGroupedNode.Key}/Level", groupedNode);
            }
        }

        graphData.UpdateOldGroupedNodeNames(new(currentGroupedNodeNames));
    }
    #endregion

    private static void GetElementsFromGraphView() {
        _graphView.graphElements.ForEach(graphElement => {
            if (graphElement is LevelBaseNode node)
                _nodes.Add(node);
            else if (graphElement is LevelSystemGroup group)
                _groups.Add(group);
        });
    }
    #endregion


    #region Load
    public static void Load()
    {
        LevelSystemGraphSaveData graphData = AssetsUtility.LoadAsset<LevelSystemGraphSaveData>("Assets/_Project/Editor/LevelSystem/Graphs", _graphFileName);
        if (graphData == null)
        {
            EditorUtility.DisplayDialog(
                "Cannot load the file!",
                $"File {_graphFileName}.asset cannot be found. Change name and try again.",
                "Ok"
            );
            return;
        }

        LevelContainer levelContainer = AssetsUtility.LoadAsset<LevelContainer>(_graphFolderPath, _graphFileName);
        if (levelContainer == null)
        {
            EditorUtility.DisplayDialog(
                "Cannot load the Level container!",
                $"File {_graphFileName}.asset cannot be found in {_graphFolderPath}.",
                "Ok"
            );
            return;
        }

        LevelSystemEditorWindow.UpdateFileName(graphData.FileName);

        LoadGroups(graphData);
        LoadNodes(graphData, levelContainer);
        LoadNodesConnections();
    }
    
    private static void LoadNodes(LevelSystemGraphSaveData graphData, LevelContainer levelContainer)
    {
        foreach (var nodeData in graphData.Nodes)
        {
            List<LevelChoiceSaveData> choices = CloneNodeChoices(nodeData.Choices);
            LevelBaseNode node = _graphView.CreateNode(nodeData.Name, nodeData.LevelType, nodeData.Position, false);
            node.Setup(nodeData, choices);

            // Load the corresponding Level ScriptableObject
            Level level = null;
            if (!string.IsNullOrEmpty(nodeData.GroupID))
            {
                // Check _loadedGroups, not graphData, as graphData.Groups might not be in order
                if (_loadedGroups.ContainsKey(nodeData.GroupID))
                {
                    level = levelContainer.GetGroupLevel(_loadedGroups[nodeData.GroupID].title, nodeData.Name);
                }
            }
            else
            {
                level = levelContainer.GetUngroupedLevel(nodeData.Name);
            }

            if (level != null)
            {
                // --- REVISED LOAD LOGIC ---

                // 1. Set the Level reference on the node (this pulls all data from Level to node)
                node.Level = level;

                // 2. Register mapping
                _graphView.RegisterNodeLevelMapping(node.ID, level);

                // 3. Track for further changes
                LevelChangeMonitor.TrackLevel(level);
            }

            // Draw AFTER applying the data
            node.Draw();

            _loadedNodes.Add(node.ID, node);

            if (string.IsNullOrEmpty(nodeData.GroupID))
                continue;

            LevelSystemGroup group = _loadedGroups[nodeData.GroupID];
            node.ChangeGroup(group);
            group.AddElement(node);
        }
    }

    private static void LoadGroups(LevelSystemGraphSaveData graphData) {
        foreach (var groupData in graphData.Groups) {
            LevelSystemGroup group = _graphView.CreateGroup(groupData.Name, groupData.Position);
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

                LevelChoiceSaveData choiceData = (LevelChoiceSaveData)choicePort.userData;
                if (string.IsNullOrEmpty(choiceData.NodeID))
                    continue;

                LevelBaseNode nextNode = _loadedNodes[choiceData.NodeID];
                Port nextNodeInputPort = (Port)nextNode.inputContainer.Children().First();
                _graphView.AddElement(choicePort.ConnectTo(nextNodeInputPort));
            }
            loadedNode.Value.RefreshPorts();
        }
    }
    #endregion

    private static void CreateStaticFolders() {
        FoldersUtility.CreateEditorFolder("Assets/_Project/Editor/LevelSystem", "Graphs");
        FoldersUtility.CreateEditorFolder("Assets", "ScriptableObjects");
        FoldersUtility.CreateEditorFolder("Assets/_Project/ScriptableObjects", "Level");
        FoldersUtility.CreateEditorFolder("Assets/_Project/ScriptableObjects/Level", _graphFileName);

        FoldersUtility.CreateEditorFolder(_graphFolderPath, "Global");
        FoldersUtility.CreateEditorFolder(_graphFolderPath, "Groups");
        FoldersUtility.CreateEditorFolder($"{_graphFolderPath}/Global", "Level");
    }
}