using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

public static class DialogueSystemSaveManager {
    private static DialogueSystemGraphView _graphView;

    private static string _graphFileName;
    private static string _graphFolderPath;

    private static List<DialogueSystemGroup> _groups;
    private static List<DialogueBaseNode> _nodes;
    private static Dictionary<string, DialogueGroup> _createdDialogueGroups;
    private static Dictionary<string, Dialogue> _createdDialogues;

    private static Dictionary<string, DialogueSystemGroup> _loadedGroups;
    // Map from DialogueSystemGroup ID to DialogueGroup ScriptableObject for loading
    private static Dictionary<string, DialogueGroup> _loadedDialogueGroups;
    private static Dictionary<string, DialogueBaseNode> _loadedNodes;

    public static void Initialize(DialogueSystemGraphView graphView, string graphName) {
        _graphView = graphView;
        _graphFileName = graphName;
        _graphFolderPath = $"Assets/_Project/ScriptableObjects/Dialogues/{graphName}";

        _groups = new();
        _nodes = new();
        _createdDialogueGroups = new();
        _createdDialogues = new();
        _loadedGroups = new();
        _loadedDialogueGroups = new();
        _loadedNodes = new();
    }

    #region Save
    public static void Save() {
        CreateStaticFolders();
        GetElementsFromGraphView();

        DialogueSystemGraphSaveData graphData = AssetsUtility.CreateAsset<DialogueSystemGraphSaveData>("Assets/_Project/Editor/DialogueSystem/Graphs", _graphFileName);
        graphData.Initialize(_graphFileName);

        DialogueContainer dialogueContainer = AssetsUtility.CreateAsset<DialogueContainer>(_graphFolderPath, _graphFileName);

        // CRITICAL FIX: Always clear and rebuild the container to ensure consistency
        if (dialogueContainer.Groups == null || dialogueContainer.UngroupedDialogues == null)
        {
            dialogueContainer.Initialize(_graphFileName);
        }
        
        // Clear existing entries to rebuild them fresh
        dialogueContainer.Groups.Clear();
        dialogueContainer.UngroupedDialogues.Clear();

        SaveGroups(graphData, dialogueContainer);
        SaveNodes(graphData, dialogueContainer);

        graphData.Save();
        dialogueContainer.Save();
    }

    #region Groups
    private static void SaveGroups(DialogueSystemGraphSaveData graphData, DialogueContainer dialogueContainer) {
        List<string> groupNames = new();
        foreach (var group in _groups) {
            SaveGroupToGraph(group, graphData);
            SaveGroupToScriptableObject(group, dialogueContainer);
            groupNames.Add(group.title);
        }

        UpdateOldGroups(groupNames, graphData);
    }

    private static void SaveGroupToGraph(DialogueSystemGroup group, DialogueSystemGraphSaveData graphData) {
        DialogueGroupSaveData groupData = new(
            group.ID,
            group.title,
            group.GetPosition().position
        );

        graphData.AddGroup(groupData);
    }

    private static void SaveGroupToScriptableObject(DialogueSystemGroup group, DialogueContainer dialogueContainer) {
        string groupName = group.title;
        FoldersUtility.CreateEditorFolder($"{_graphFolderPath}/Groups", groupName);
        FoldersUtility.CreateEditorFolder($"{_graphFolderPath}/Groups/{groupName}", "Dialogues");

        DialogueGroup dialogueGroup = AssetsUtility.CreateAsset<DialogueGroup>($"{_graphFolderPath}/Groups/{groupName}", groupName);
        dialogueGroup.Initialize(groupName);
        dialogueContainer.AddGroup(dialogueGroup);
        _createdDialogueGroups.Add(group.ID, dialogueGroup);

        dialogueGroup.Save();
    }

    private static void UpdateOldGroups(List<string> currentGroupNames, DialogueSystemGraphSaveData graphData) {
        foreach (var oldName in graphData.OldGroupNames) {
            if (currentGroupNames.Contains(oldName))
                continue;

            FoldersUtility.DeleteEditorFolder($"{_graphFolderPath}/Groups/{oldName}");
        }

        graphData.UpdateOldGroupNames(new(currentGroupNames));
    }
    #endregion

    #region Nodes
    private static void SaveNodes(DialogueSystemGraphSaveData graphData, DialogueContainer dialogueContainer) {
        List<string> nodeNames = new();
        SerializableDictionary<string, List<string>> groupedNodeNames = new();

        foreach (var node in _nodes) {
            SaveNodeToGraph(node, graphData);
            SaveNodeToScriptableObject(node, dialogueContainer);
            if (node.Group != null) {
                if (groupedNodeNames.ContainsKey(node.Group.ID))
                    groupedNodeNames[node.Group.ID].Add(node.DialogueName);
                else
                    groupedNodeNames[node.Group.ID] = new() { node.DialogueName };
                continue;
            }

            nodeNames.Add(node.DialogueName);
        }

        UpdateDialoguesChoicesConnections();
        UpdateOldGroupedNodes(groupedNodeNames, graphData);
        UpdateOldUngroupedNodes(nodeNames, graphData);
    }

    private static void SaveNodeToGraph(DialogueBaseNode node, DialogueSystemGraphSaveData graphData) {
        List<DialogueChoiceSaveData> choices = CloneNodeChoices(node.Choices);
        DialogueNodeSaveData nodeData;

        // Create the appropriate save data based on node type
        switch (node.DialogueType) {
            case DialogueType.Ink:
                if (node is DialogueInkNode inkNode) {
                    nodeData = new DialogueNodeSaveData(
                        node.ID,
                        node.DialogueName,
                        node.Text,
                        choices,
                        node.Group?.ID,
                        node.DialogueType,
                        node.GetPosition().position,
                        node.Character,
                        node.Emotion,
                        inkNode.InkJsonAsset,
                        inkNode.KnotName,
                        inkNode.StartFromBeginning
                    );
                } else {
                    nodeData = CreateDefaultNodeSaveData(node, choices);
                }
                break;

            case DialogueType.ExternalFunction:
                if (node is DialogueExternalFunctionNode funcNode) {
                    nodeData = new DialogueNodeSaveData(
                        node.ID,
                        node.DialogueName,
                        node.Text,
                        choices,
                        node.Group?.ID,
                        node.DialogueType,
                        node.GetPosition().position,
                        node.Character,
                        node.Emotion,
                        funcNode.FunctionType,
                        funcNode.FunctionParameter
                    );
                } else {
                    nodeData = CreateDefaultNodeSaveData(node, choices);
                }
                break;

            case DialogueType.ModifyVariable:
                if (node is DialogueModifyVariableNode modifyNode) {
                    nodeData = new DialogueNodeSaveData(
                        node.ID,
                        node.DialogueName,
                        node.Text,
                        choices,
                        node.Group?.ID,
                        node.DialogueType,
                        node.GetPosition().position,
                        node.Character,
                        node.Emotion,
                        modifyNode.VariablesContainer,
                        modifyNode.VariableName,
                        modifyNode.VariableType,
                        modifyNode.Modification,
                        ConditionType.Equals, // Not used for modify
                        modifyNode.BoolValue,
                        modifyNode.IntValue,
                        modifyNode.FloatValue,
                        modifyNode.StringValue
                    );
                } else {
                    nodeData = CreateDefaultNodeSaveData(node, choices);
                }
                break;

            case DialogueType.VariableCondition:
                if (node is DialogueVariableConditionNode conditionNode) {
                    nodeData = new DialogueNodeSaveData(
                        node.ID,
                        node.DialogueName,
                        node.Text,
                        choices,
                        node.Group?.ID,
                        node.DialogueType,
                        node.GetPosition().position,
                        node.Character,
                        node.Emotion,
                        conditionNode.VariablesContainer,
                        conditionNode.VariableName,
                        conditionNode.VariableType,
                        ModificationType.Set, // Not used for conditions
                        conditionNode.Condition,
                        conditionNode.BoolTargetValue,
                        conditionNode.IntTargetValue,
                        conditionNode.FloatTargetValue,
                        conditionNode.StringTargetValue
                    );
                } else {
                    nodeData = CreateDefaultNodeSaveData(node, choices);
                }
                break;

            default:
                nodeData = CreateDefaultNodeSaveData(node, choices);
                break;
        }

        graphData.AddNode(nodeData);
    }

    private static DialogueNodeSaveData CreateDefaultNodeSaveData(DialogueBaseNode node, List<DialogueChoiceSaveData> choices) {
        return new DialogueNodeSaveData(
            node.ID,
            node.DialogueName,
            node.Text,
            choices,
            node.Group?.ID,
            node.DialogueType,
            node.GetPosition().position,
            node.Character,
            node.Emotion
        );
    }

    private static List<DialogueChoiceSaveData> CloneNodeChoices(IEnumerable<DialogueChoiceSaveData> originalChoices) {
        List<DialogueChoiceSaveData> choices = new();
        foreach (var choice in originalChoices)
            choices.Add(choice.Copy());
        return choices;
    }

    private static void SaveNodeToScriptableObject(DialogueBaseNode node, DialogueContainer dialogueContainer) {
        Dialogue dialogue;
        if (node.Group != null) {
            dialogue = AssetsUtility.CreateAsset<Dialogue>($"{_graphFolderPath}/Groups/{node.Group.title}/Dialogues", node.DialogueName);
            dialogueContainer.AddGroupDialogue(_createdDialogueGroups[node.Group.ID], dialogue);
        } else {
            dialogue = AssetsUtility.CreateAsset<Dialogue>($"{_graphFolderPath}/Global/Dialogues", node.DialogueName);
            dialogueContainer.AddUngroupDialogue(dialogue);
        }

        // Handle different node types with specific initialization
        switch (node.DialogueType) {
            case DialogueType.Ink:
                if (node is DialogueInkNode inkNode) {
                    dialogue.InitializeWithInkData(
                        node.DialogueName,
                        node.Text,
                        ConvertNodeChoicesToDialogueChoices(node.Choices),
                        node.DialogueType,
                        node.Character,
                        node.Emotion,
                        node.IsStartingNode(),
                        inkNode.InkJsonAsset,
                        inkNode.KnotName,
                        inkNode.StartFromBeginning
                    );
                }
                break;

            case DialogueType.ExternalFunction:
                if (node is DialogueExternalFunctionNode funcNode) {
                    dialogue.InitializeWithFunctionData(
                        node.DialogueName,
                        node.Text,
                        ConvertNodeChoicesToDialogueChoices(node.Choices),
                        node.DialogueType,
                        node.Character,
                        node.Emotion,
                        node.IsStartingNode(),
                        funcNode.FunctionType,
                        funcNode.FunctionParameter
                    );
                }
                break;

            case DialogueType.ModifyVariable:
                if (node is DialogueModifyVariableNode modifyNode) {
                    dialogue.InitializeWithVariableData(
                        node.DialogueName,
                        node.Text,
                        ConvertNodeChoicesToDialogueChoices(node.Choices),
                        node.DialogueType,
                        node.Character,
                        node.Emotion,
                        node.IsStartingNode(),
                        modifyNode.VariablesContainer,
                        modifyNode.VariableName,
                        modifyNode.VariableType,
                        modifyNode.Modification,
                        ConditionType.Equals, // Not used for modify
                        modifyNode.BoolValue,
                        modifyNode.IntValue,
                        modifyNode.FloatValue,
                        modifyNode.StringValue
                    );
                }
                break;

            case DialogueType.VariableCondition:
                if (node is DialogueVariableConditionNode conditionNode) {
                    dialogue.InitializeWithVariableData(
                        node.DialogueName,
                        node.Text,
                        ConvertNodeChoicesToDialogueChoices(node.Choices),
                        node.DialogueType,
                        node.Character,
                        node.Emotion,
                        node.IsStartingNode(),
                        conditionNode.VariablesContainer,
                        conditionNode.VariableName,
                        conditionNode.VariableType,
                        ModificationType.Set, // Not used for conditions
                        conditionNode.Condition,
                        conditionNode.BoolTargetValue,
                        conditionNode.IntTargetValue,
                        conditionNode.FloatTargetValue,
                        conditionNode.StringTargetValue
                    );
                }
                break;

            default:
                // Standard dialogue node
                dialogue.Initialize(
                    node.DialogueName,
                    node.Text,
                    ConvertNodeChoicesToDialogueChoices(node.Choices),
                    node.DialogueType,
                    node.Character,
                    node.Emotion,
                    node.IsStartingNode()
                );
                break;
        }

        _createdDialogues.Add(node.ID, dialogue);
        dialogue.Save();
    }

    private static void UpdateDialoguesChoicesConnections() {
        foreach (var node in _nodes) {
            Dialogue dialogue = _createdDialogues[node.ID];

            for (int i = 0; i < node.Choices.Count(); i++) {
                DialogueChoiceSaveData nodeChoice = node.GetChoice(i);
                if (string.IsNullOrEmpty(nodeChoice.NodeID))
                    continue;

                dialogue.SetChoiceNextDialogue(_createdDialogues[nodeChoice.NodeID], i);
                dialogue.Save();
            }
        }
    }

    private static List<DialogueChoiceData> ConvertNodeChoicesToDialogueChoices(IEnumerable<DialogueChoiceSaveData> nodeChoices) {
        List<DialogueChoiceData> dialogueChoices = new();
        foreach (var nodeChoice in nodeChoices)
            dialogueChoices.Add(nodeChoice.ToDialogueChoice());
        return dialogueChoices;
    }

    private static void UpdateOldUngroupedNodes(List<string> currentNodeNames, DialogueSystemGraphSaveData graphData) {
        foreach (var oldName in graphData.OldUngroupedNodeNames) {
            if (currentNodeNames.Contains(oldName))
                continue;

            AssetsUtility.RemoveAsset($"{_graphFolderPath}/Global/Dialogues", oldName);
        }

        graphData.UpdateOldUngroupedNodeNames(new(currentNodeNames));
    }

    private static void UpdateOldGroupedNodes(SerializableDictionary<string, List<string>> currentGroupedNodeNames, DialogueSystemGraphSaveData graphData) {
        foreach (var oldGroupedNode in graphData.OldGroupedNodeNames) {
            if (!currentGroupedNodeNames.ContainsKey(oldGroupedNode.Key))
                continue;

            foreach (var groupedNode in oldGroupedNode.Value) {
                if (currentGroupedNodeNames[oldGroupedNode.Key].Contains(groupedNode))
                    continue;

                AssetsUtility.RemoveAsset($"{_graphFolderPath}/Groups/{oldGroupedNode.Key}/Dialogues", groupedNode);
            }
        }

        graphData.UpdateOldGroupedNodeNames(new(currentGroupedNodeNames));
    }
    #endregion

    private static void GetElementsFromGraphView() {
        _graphView.graphElements.ForEach(graphElement => {
            if (graphElement is DialogueBaseNode node)
                _nodes.Add(node);
            else if (graphElement is DialogueSystemGroup group)
                _groups.Add(group);
        });
    }
    #endregion


    #region Load
    public static void Load() {
        DialogueSystemGraphSaveData graphData = AssetsUtility.LoadAsset<DialogueSystemGraphSaveData>("Assets/_Project/Editor/DialogueSystem/Graphs", _graphFileName);
        if (graphData == null) {
            EditorUtility.DisplayDialog(
                "Cannot load the file!",
                $"File {_graphFileName}.asset cannot be found. Change name and try again.",
                "Ok"
            );
            return;
        }

        DialogueContainer dialogueContainer = AssetsUtility.LoadAsset<DialogueContainer>(_graphFolderPath, _graphFileName);
        if (dialogueContainer == null) {
            EditorUtility.DisplayDialog(
                "Cannot load the dialogue container!",
                $"File {_graphFileName}.asset cannot be found in {_graphFolderPath}.",
                "Ok"
            );
            return;
        }

        DialogueSystemEditorWindow.UpdateFileName(graphData.FileName);

        LoadGroups(graphData);
        LoadNodes(graphData, dialogueContainer);
        LoadNodesConnections();
    }

    private static void LoadNodes(DialogueSystemGraphSaveData graphData, DialogueContainer dialogueContainer)
    {
        foreach (var nodeData in graphData.Nodes)
        {
            List<DialogueChoiceSaveData> choices = CloneNodeChoices(nodeData.Choices);
            DialogueBaseNode node = _graphView.CreateNode(nodeData.Name, nodeData.DialogueType, nodeData.Position, false);
            node.Setup(nodeData, choices);

            // Load and apply Dialogue data to the node BEFORE drawing
            Dialogue dialogue = null;
            if (!string.IsNullOrEmpty(nodeData.GroupID))
            {
                dialogue = dialogueContainer.GetGroupDialogue(_loadedGroups[nodeData.GroupID].title, nodeData.Name);
            }
            else
            {
                dialogue = dialogueContainer.GetUngroupedDialogue(nodeData.Name);
            }

            if (dialogue != null)
            {
                ApplyDialogueDataToNode(node, dialogue);
            }

            // Draw AFTER applying the data
            node.Draw();

            // Refresh UI for variable nodes after drawing
            if (node is DialogueModifyVariableNode modifyNode)
            {
                modifyNode.RefreshUI();
            }
            else if (node is DialogueVariableConditionNode conditionNode)
            {
                conditionNode.RefreshUI();
            }

            _loadedNodes.Add(node.ID, node);

            if (string.IsNullOrEmpty(nodeData.GroupID))
                continue;

            DialogueSystemGroup group = _loadedGroups[nodeData.GroupID];
            node.ChangeGroup(group);
            group.AddElement(node);
        }
    }

    private static void ApplyDialogueDataToNode(DialogueBaseNode node, Dialogue dialogue)
    {
        // Apply common data to ALL node types first
        node.Text = dialogue.Text;
        node.Character = dialogue.Character;
        node.Emotion = dialogue.Emotion;

        // Then apply type-specific data
        switch (dialogue.Type)
        {
            case DialogueType.Ink:
                if (node is DialogueInkNode inkNode)
                {
                    inkNode.InkJsonAsset = dialogue.InkJsonAsset;
                    inkNode.KnotName = dialogue.KnotName;
                    inkNode.StartFromBeginning = dialogue.StartFromBeginning;
                }
                break;

            case DialogueType.ExternalFunction:
                if (node is DialogueExternalFunctionNode funcNode)
                {
                    funcNode.FunctionType = dialogue.ExternalFunctionType;
                    funcNode.FunctionParameter = dialogue.FunctionParameter;
                }
                break;

            case DialogueType.ModifyVariable:
                if (node is DialogueModifyVariableNode modifyNode)
                {
                    modifyNode.VariablesContainer = dialogue.VariablesContainer;
                    modifyNode.VariableName = dialogue.VariableName;
                    modifyNode.Modification = dialogue.ModificationType;
                    modifyNode.BoolValue = dialogue.BoolValue;
                    modifyNode.IntValue = dialogue.IntValue;
                    modifyNode.FloatValue = dialogue.FloatValue;
                    modifyNode.StringValue = dialogue.StringValue;
                }
                break;

            case DialogueType.VariableCondition:
                if (node is DialogueVariableConditionNode conditionNode)
                {
                    conditionNode.VariablesContainer = dialogue.VariablesContainer;
                    conditionNode.VariableName = dialogue.VariableName;
                    conditionNode.Condition = dialogue.ConditionType;
                    conditionNode.BoolTargetValue = dialogue.BoolValue;
                    conditionNode.IntTargetValue = dialogue.IntValue;
                    conditionNode.FloatTargetValue = dialogue.FloatValue;
                    conditionNode.StringTargetValue = dialogue.StringValue;
                }
                break;
        }
    }    private static void LoadGroups(DialogueSystemGraphSaveData graphData) {
        foreach (var groupData in graphData.Groups) {
            DialogueSystemGroup group = _graphView.CreateGroup(groupData.Name, groupData.Position);
            group.SetID(groupData.ID);
            _loadedGroups.Add(groupData.ID, group);
        }
    }

    private static void LoadNodesConnections() {
        foreach (var loadedNode in _loadedNodes) {
            foreach (var element in loadedNode.Value.outputContainer.Children()) {
                if (element is not Port choicePort)
                    continue;

                DialogueChoiceSaveData choiceData = (DialogueChoiceSaveData)choicePort.userData;
                if (string.IsNullOrEmpty(choiceData.NodeID))
                    continue;

                DialogueBaseNode nextNode = _loadedNodes[choiceData.NodeID];
                Port nextNodeInputPort = (Port)nextNode.inputContainer.Children().First();
                _graphView.AddElement(choicePort.ConnectTo(nextNodeInputPort));
            }
            loadedNode.Value.RefreshPorts();
        }
    }
    #endregion

    private static void CreateStaticFolders() {
        FoldersUtility.CreateEditorFolder("Assets/_Project/Editor/DialogueSystem", "Graphs");
        FoldersUtility.CreateEditorFolder("Assets", "ScriptableObjects");
        FoldersUtility.CreateEditorFolder("Assets/_Project/ScriptableObjects", "Dialogues");
        FoldersUtility.CreateEditorFolder("Assets/_Project/ScriptableObjects/Dialogues", _graphFileName);

        FoldersUtility.CreateEditorFolder(_graphFolderPath, "Global");
        FoldersUtility.CreateEditorFolder(_graphFolderPath, "Groups");
        FoldersUtility.CreateEditorFolder($"{_graphFolderPath}/Global", "Dialogues");
    }
}