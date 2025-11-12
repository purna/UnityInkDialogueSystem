using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(LevelController))]
public class LevelControllerEditor : Editor
{
    private SerializedProperty setupModeProp;
    private SerializedProperty manualLevelNodesProp;
    private SerializedProperty levelContainerProp;
    private SerializedProperty levelGroupProp;
    private SerializedProperty startingLevelProp;
    private SerializedProperty groupedLevelsProp;
    private SerializedProperty startingLevelsOnlyProp;
    private SerializedProperty selectedLevelGroupIndexProp;
    private SerializedProperty selectedLevelIndexProp;
    private SerializedProperty levelManagerProp;
    private SerializedProperty levelUIProp;
    private SerializedProperty initializeOnStartProp;
    private SerializedProperty startDelayProp;
    private SerializedProperty levelNodesParentProp;
    private SerializedProperty levelNodePrefabProp;
    private SerializedProperty gridLayoutProp;
    private SerializedProperty useGridLayoutProp;
    private SerializedProperty useLevelPositionsProp;
    private SerializedProperty autoGenerateLinesProp;
    private SerializedProperty detailsPanelProp;
    
    private int selectedGroupIndex = 0;
    private int selectedLevelIndex = 0;
    private bool showAllLevels = false;
    private Vector2 levelsScrollPosition;
    private bool showManualNodesList = true;

    private void OnEnable()
    {
        setupModeProp = serializedObject.FindProperty("_setupMode");
        manualLevelNodesProp = serializedObject.FindProperty("_manualLevelNodes");
        levelContainerProp = serializedObject.FindProperty("levelContainer");
        levelGroupProp = serializedObject.FindProperty("levelGroup");
        startingLevelProp = serializedObject.FindProperty("startingLevel");
        groupedLevelsProp = serializedObject.FindProperty("groupedLevels");
        startingLevelsOnlyProp = serializedObject.FindProperty("startingLevelsOnly");
        selectedLevelGroupIndexProp = serializedObject.FindProperty("selectedLevelGroupIndex");
        selectedLevelIndexProp = serializedObject.FindProperty("selectedLevelIndex");
        levelManagerProp = serializedObject.FindProperty("levelManager");
        levelUIProp = serializedObject.FindProperty("levelUI");
        initializeOnStartProp = serializedObject.FindProperty("initializeOnStart");
        startDelayProp = serializedObject.FindProperty("startDelay");
        levelNodesParentProp = serializedObject.FindProperty("_levelNodesParent");
        levelNodePrefabProp = serializedObject.FindProperty("_levelNodePrefab");
        gridLayoutProp = serializedObject.FindProperty("_gridLayout");
        useGridLayoutProp = serializedObject.FindProperty("_useGridLayout");
        useLevelPositionsProp = serializedObject.FindProperty("_useLevelPositions");
        autoGenerateLinesProp = serializedObject.FindProperty("_autoGenerateLines");
        detailsPanelProp = serializedObject.FindProperty("_detailsPanel");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawSectionHeader("Level Unlock Manager", 16);
        EditorGUILayout.Space(5);

        DrawSectionHeader("Setup Mode");
        EditorGUILayout.PropertyField(setupModeProp, new GUIContent("Setup Mode"));
        
        LevelSetupMode currentMode = (LevelSetupMode)setupModeProp.enumValueIndex;
        
        if (currentMode == LevelSetupMode.ManualAssignment)
        {
            EditorGUILayout.HelpBox(
                "Manual Assignment: Assign levels to pre-placed UI nodes in your scene. " +
                "Each LevelNode component will have a dropdown to select its level.",
                MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Auto Generate: Levels will be automatically instantiated from the level node prefab. " +
                "All levels from the selected source will be generated.",
                MessageType.Info);
        }
        
        EditorGUILayout.Space(10);

        DrawSectionHeader("Level Data");
        EditorGUILayout.PropertyField(levelContainerProp);

        if (levelContainerProp.objectReferenceValue != null)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Open Level Graph", GUILayout.Height(25), GUILayout.Width(200)))
            {
                OpenLevelGraph();
            }
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
            
            DrawContainerContents();
        }
        else
        {
            EditorGUILayout.HelpBox("Select a Level Container to continue.", MessageType.Info);
        }

        EditorGUILayout.Space(10);

        if (currentMode == LevelSetupMode.ManualAssignment)
        {
            DrawManualAssignmentSettings();
        }
        else
        {
            DrawAutoGenerateSettings();
        }

        EditorGUILayout.Space(10);

        DrawSectionHeader("Level Selection");
        EditorGUILayout.PropertyField(groupedLevelsProp, new GUIContent("Use Grouped Levels"));
        EditorGUILayout.PropertyField(startingLevelsOnlyProp, new GUIContent("Starting Levels Only"));
        
        EditorGUILayout.HelpBox(
            "Level Selection Options:\n" +
            "• Use Grouped Levels: Load from a specific group\n" +
            "• Starting Levels Only: Only include levels marked as starting levels\n" +
            "• If a single level is selected, all connected levels in its tree will be loaded",
            MessageType.Info);

        EditorGUILayout.Space(10);

        DrawSectionHeader("System References");
        EditorGUILayout.PropertyField(levelManagerProp);
        EditorGUILayout.PropertyField(levelUIProp);
        EditorGUILayout.PropertyField(detailsPanelProp);

        EditorGUILayout.Space(10);

        DrawSectionHeader("Auto Initialize Settings");
        EditorGUILayout.PropertyField(initializeOnStartProp);
        
        if (initializeOnStartProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(startDelayProp);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        DrawUtilityButtons();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawManualAssignmentSettings()
    {
        DrawSectionHeader("Manual Assignment Settings");
        
        EditorGUILayout.PropertyField(autoGenerateLinesProp, new GUIContent("Auto Generate Connection Lines"));
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox(
            "How to set up manual nodes:\n" +
            "1. Place UI GameObjects in your scene for each level node\n" +
            "2. Add LevelNode components to each GameObject\n" +
            "3. Assign this controller to each LevelNode\n" +
            "4. Use the dropdown on each LevelNode to select its level",
            MessageType.Info);
        
        EditorGUILayout.Space(5);
        
        if (!Application.isPlaying)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Find LevelNodes in Scene", GUILayout.Height(25)))
            {
                FindAndListNodes();
            }
            if (GUILayout.Button("Refresh All Nodes", GUILayout.Height(25)))
            {
                RefreshAllNodes();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Auto-Assign Levels to All Nodes", GUILayout.Height(30)))
            {
                AutoAssignLevelsToNodes();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Manual node detection only works in Edit mode.", MessageType.Warning);
        }
        
        EditorGUILayout.Space(5);
        
        showManualNodesList = EditorGUILayout.Foldout(showManualNodesList, 
            $"Manual Node Mappings (Legacy - {manualLevelNodesProp.arraySize})", true);
        
        if (showManualNodesList)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox(
                "Note: This list is mainly for legacy support. " +
                "It's easier to use the dropdown on each LevelNode component directly.",
                MessageType.Info);
            EditorGUILayout.PropertyField(manualLevelNodesProp, true);
            EditorGUI.indentLevel--;
        }
    }

    private void DrawAutoGenerateSettings()
    {
        DrawSectionHeader("Auto Generate Settings");
        
        EditorGUILayout.PropertyField(levelNodesParentProp, new GUIContent("Level Nodes Parent"));
        EditorGUILayout.PropertyField(levelNodePrefabProp, new GUIContent("Level Node Prefab"));
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Layout Options", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(useGridLayoutProp, new GUIContent("Use Grid Layout"));
        
        if (useGridLayoutProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(gridLayoutProp, new GUIContent("Grid Layout Component"));
            EditorGUI.indentLevel--;
            
            if (gridLayoutProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Assign a GridLayoutGroup component to use grid layout.", MessageType.Warning);
            }
        }
        else
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(useLevelPositionsProp, new GUIContent("Use Level Positions"));
            EditorGUI.indentLevel--;
            
            if (!useLevelPositionsProp.boolValue)
            {
                EditorGUILayout.HelpBox("Levels will be placed at (0,0). Use the Graph Editor to set positions.", MessageType.Info);
            }
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.PropertyField(autoGenerateLinesProp, new GUIContent("Auto Generate Connection Lines"));
        
        EditorGUILayout.Space(5);
        if (levelNodesParentProp.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Level Nodes Parent is required for auto-generation!", MessageType.Error);
        }
        
        if (levelNodePrefabProp.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Level Node Prefab is required for auto-generation!", MessageType.Error);
        }
    }

    private void DrawUtilityButtons()
    {
        DrawSectionHeader("Utilities");
        
        LevelController controller = (LevelController)target;
        
        if (Application.isPlaying)
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Refresh Level System", GUILayout.Height(30)))
            {
                controller.RefreshLevel();
                Debug.Log("Level system refreshed!");
            }
            
            EditorGUILayout.EndHorizontal();
            
            List<Level> availableLevels = controller.GetAvailableLevels();
            EditorGUILayout.LabelField($"Available Levels: {availableLevels?.Count ?? 0}");
            
            if (availableLevels != null && availableLevels.Count > 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Levels in system:", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                foreach (var level in availableLevels)
                {
                    string status = level.IsCompleted ? "✓" : level.IsUnlocked ? "○" : "🔒";
                    EditorGUILayout.LabelField($"{status} {level.LevelName}");
                }
                EditorGUI.indentLevel--;
            }
        }
        else
        {
            LevelSetupMode currentMode = (LevelSetupMode)setupModeProp.enumValueIndex;
            
            if (currentMode == LevelSetupMode.AutoGenerate)
            {
                EditorGUILayout.HelpBox(
                    "In Play mode, you can refresh the level system to see changes.",
                    MessageType.Info);
                
                List<Level> previewLevels = controller.GetAvailableLevels();
                if (previewLevels != null && previewLevels.Count > 0)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField($"Will generate {previewLevels.Count} levels:", EditorStyles.boldLabel);
                    
                    EditorGUI.indentLevel++;
                    int displayCount = Mathf.Min(10, previewLevels.Count);
                    for (int i = 0; i < displayCount; i++)
                    {
                        EditorGUILayout.LabelField($"• {previewLevels[i].LevelName}");
                    }
                    if (previewLevels.Count > 10)
                    {
                        EditorGUILayout.LabelField($"... and {previewLevels.Count - 10} more");
                    }
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.HelpBox("No levels will be generated with current settings.", MessageType.Warning);
                }
            }
        }
    }

    private void FindAndListNodes()
    {
        LevelNode[] nodes = FindObjectsOfType<LevelNode>();
        
        if (nodes.Length == 0)
        {
            EditorUtility.DisplayDialog("No Nodes Found", 
                "No LevelNode components found in the current scene.", "OK");
            return;
        }
        
        string message = $"Found {nodes.Length} LevelNode(s) in the scene:\n\n";
        int assignedCount = 0;
        int unassignedCount = 0;
        
        foreach (var node in nodes)
        {
            Level level = node.GetLevel();
            if (level != null)
            {
                message += $"✓ {node.gameObject.name} → {level.LevelName}\n";
                assignedCount++;
            }
            else
            {
                message += $"✗ {node.gameObject.name} → (No level assigned)\n";
                unassignedCount++;
            }
        }
        
        message += $"\n{assignedCount} assigned, {unassignedCount} unassigned";
        
        EditorUtility.DisplayDialog("LevelNodes in Scene", message, "OK");
    }

    private void RefreshAllNodes()
    {
        LevelController controller = (LevelController)target;
        LevelNode[] nodes = FindObjectsOfType<LevelNode>();
        
        if (nodes.Length == 0)
        {
            EditorUtility.DisplayDialog("No Nodes Found", 
                "No LevelNode components found in the current scene.", "OK");
            return;
        }
        
        int updated = 0;
        
        foreach (var node in nodes)
        {
            node.SetController(controller);
            EditorUtility.SetDirty(node);
            updated++;
        }
        
        EditorUtility.DisplayDialog("Nodes Refreshed", 
            $"Updated {updated} LevelNode component(s) with the current controller reference.", "OK");
    }
    
    private void AutoAssignLevelsToNodes()
    {
        LevelController controller = (LevelController)target;
        LevelNode[] nodes = FindObjectsOfType<LevelNode>();
        
        if (nodes.Length == 0)
        {
            EditorUtility.DisplayDialog("No Nodes Found", 
                "No LevelNode components found in the current scene.", "OK");
            return;
        }
        
        List<Level> availableLevels = controller.GetAvailableLevels();
        
        if (availableLevels == null || availableLevels.Count == 0)
        {
            EditorUtility.DisplayDialog("No Levels Available", 
                "No levels available from the controller. Make sure a LevelContainer is assigned.", "OK");
            return;
        }
        
        int assigned = 0;
        int skipped = 0;
        
        foreach (var node in nodes)
        {
            if (node.GetLevel() != null)
            {
                skipped++;
                continue;
            }
            
            if (assigned < availableLevels.Count)
            {
                node.SetController(controller);
                node.SetLevelIndex(assigned);
                node.SetLevel(availableLevels[assigned]);
                EditorUtility.SetDirty(node);
                assigned++;
            }
        }
        
        string message = $"Auto-assignment complete!\n\n" +
                        $"Assigned: {assigned}\n" +
                        $"Skipped (already assigned): {skipped}\n" +
                        $"Total nodes: {nodes.Length}";
        
        if (assigned < nodes.Length - skipped)
        {
            message += $"\n\nNote: Not enough levels for all nodes. " +
                      $"You have {nodes.Length} nodes but only {availableLevels.Count} levels available.";
        }
        
        EditorUtility.DisplayDialog("Auto-Assignment Complete", message, "OK");
    }
    
    private void DrawContainerContents()
    {
        LevelContainer container = levelContainerProp.objectReferenceValue as LevelContainer;
        if (container == null) return;
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Container Info", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Name:", container.name);
        EditorGUILayout.LabelField("Groups:", container.HasGroups() ? container.GetGroupsNames().Length.ToString() : "0");
        EditorGUILayout.LabelField("Ungrouped Levels:", container.GetUngroupedLevelNames(false).Count.ToString());
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        if (container.HasGroups())
        {
            DrawSectionHeader("Select Group for Preview");
            
            string[] groupNames = container.GetGroupsNames();
            
            if (selectedGroupIndex >= groupNames.Length)
                selectedGroupIndex = 0;
            
            int newGroupIndex = EditorGUILayout.Popup("Group", selectedGroupIndex, groupNames);
            
            if (newGroupIndex != selectedGroupIndex)
            {
                selectedGroupIndex = newGroupIndex;
                selectedLevelIndex = 0;
            }
            
            string selectedGroupName = groupNames[selectedGroupIndex];
            LevelGroup selectedGroup = AssetsUtility.LoadAsset<LevelGroup>(
                $"Assets/_Project/ScriptableObjects/Level/{container.name}/Groups/{selectedGroupName}",
                selectedGroupName
            );
            
            if (selectedGroup != null)
            {
                levelGroupProp.objectReferenceValue = selectedGroup;
                selectedLevelGroupIndexProp.intValue = selectedGroupIndex;
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"Group: {selectedGroup.GroupName}", EditorStyles.boldLabel);
                
                List<string> groupLevels = container.GetGroupedLevelNames(selectedGroup, false);
                EditorGUILayout.LabelField("Total Levels:", groupLevels.Count.ToString());
                
                List<string> startingLevels = container.GetGroupedLevelNames(selectedGroup, true);
                EditorGUILayout.LabelField("Starting Levels:", startingLevels.Count.ToString());
                
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(5);
                
                if (groupLevels.Count > 0)
                {
                    DrawLevelSelection(container, selectedGroup, groupLevels, 
                        $"Assets/_Project/ScriptableObjects/Level/{container.name}/Groups/{selectedGroupName}/Level");
                }
                else
                {
                    EditorGUILayout.HelpBox("No levels in this group.", MessageType.Info);
                }
            }
        }
        else
        {
            DrawSectionHeader("Ungrouped Levels");
            
            List<string> ungroupedLevels = container.GetUngroupedLevelNames(false);
            
            if (ungroupedLevels.Count > 0)
            {
                DrawLevelSelection(container, null, ungroupedLevels,
                    $"Assets/_Project/ScriptableObjects/Level/{container.name}/Global/Level");
            }
            else
            {
                EditorGUILayout.HelpBox("No ungrouped levels in this container.", MessageType.Info);
            }
        }
    }
    
    private void DrawLevelSelection(LevelContainer container, LevelGroup group, List<string> levelNames, string levelFolderPath)
    {
        DrawSectionHeader("Select Level for Preview");
        
        if (selectedLevelIndex >= levelNames.Count)
            selectedLevelIndex = 0;
        
        selectedLevelIndex = EditorGUILayout.Popup("Level", selectedLevelIndex, levelNames.ToArray());
        
        string selectedLevelName = levelNames[selectedLevelIndex];
        Level selectedLevel = AssetsUtility.LoadAsset<Level>(levelFolderPath, selectedLevelName);
        
        if (selectedLevel != null)
        {
            startingLevelProp.objectReferenceValue = selectedLevel;
            selectedLevelIndexProp.intValue = selectedLevelIndex;
            
            DrawLevelPreview(selectedLevel);
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Select Level Asset", GUILayout.Height(25)))
            {
                Selection.activeObject = selectedLevel;
                EditorGUIUtility.PingObject(selectedLevel);
            }
            
            if (GUILayout.Button("Edit in Inspector", GUILayout.Height(25)))
            {
                Selection.activeObject = selectedLevel;
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
        }
        
        showAllLevels = EditorGUILayout.Foldout(showAllLevels, $"Show All Levels ({levelNames.Count})", true);
        
        if (showAllLevels)
        {
            DrawAllLevelsList(levelNames, levelFolderPath);
        }
    }
    
    private void DrawLevelPreview(Level level)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        
        if (level.Icon != null)
        {
            Rect iconRect = GUILayoutUtility.GetRect(48, 48, GUILayout.Width(48), GUILayout.Height(48));
            GUI.DrawTexture(iconRect, level.Icon.texture, ScaleMode.ScaleToFit);
            GUILayout.Space(10);
        }
        
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(level.LevelName, EditorStyles.boldLabel);
        
        EditorGUILayout.LabelField("Tier:", level.Tier.ToString());
        EditorGUILayout.LabelField("Type:", level.LevelSceneType.ToString());
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        if (!string.IsNullOrEmpty(level.Description))
        {
            EditorGUILayout.LabelField("Description:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(level.Description, EditorStyles.wordWrappedLabel);
        }
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Prerequisites: {level.Prerequisites.Count}", GUILayout.Width(150));
        EditorGUILayout.LabelField($"Children: {level.Children.Count}");
        EditorGUILayout.EndHorizontal();
        
        if (Application.isPlaying)
        {
            EditorGUILayout.Space(5);
            DrawSectionDivider();
            EditorGUILayout.LabelField("Runtime Status", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Unlocked:");
            EditorGUILayout.LabelField(level.IsUnlocked ? "✓ Yes" : "✗ No", 
                level.IsUnlocked ? EditorStyles.boldLabel : EditorStyles.label);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Completed:");
            EditorGUILayout.LabelField(level.IsCompleted ? "✓ Yes" : "✗ No", 
                level.IsCompleted ? EditorStyles.boldLabel : EditorStyles.label);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.LabelField("Can Unlock:", level.CanUnlock() ? "✓ Yes" : "✗ No");
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawAllLevelsList(List<string> levelNames, string levelFolderPath)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        levelsScrollPosition = EditorGUILayout.BeginScrollView(levelsScrollPosition, GUILayout.MaxHeight(300));
        
        for (int i = 0; i < levelNames.Count; i++)
        {
            EditorGUILayout.BeginHorizontal(i % 2 == 0 ? EditorStyles.helpBox : GUIStyle.none);
            
            EditorGUILayout.LabelField((i + 1).ToString(), GUILayout.Width(30));
            
            if (GUILayout.Button(levelNames[i], EditorStyles.label))
            {
                selectedLevelIndex = i;
                
                Level level = AssetsUtility.LoadAsset<Level>(levelFolderPath, levelNames[i]);
                if (level != null)
                {
                    Selection.activeObject = level;
                    EditorGUIUtility.PingObject(level);
                }
            }
            
            Level loadedLevel = AssetsUtility.LoadAsset<Level>(levelFolderPath, levelNames[i]);
            if (loadedLevel != null)
            {
                if (loadedLevel.Icon != null)
                {
                    Rect iconRect = GUILayoutUtility.GetRect(20, 20, GUILayout.Width(20), GUILayout.Height(20));
                    GUI.DrawTexture(iconRect, loadedLevel.Icon.texture, ScaleMode.ScaleToFit);
                }
                
                EditorGUILayout.LabelField($"T{loadedLevel.Tier}", GUILayout.Width(30));
                EditorGUILayout.LabelField(loadedLevel.LevelSceneType.ToString(), GUILayout.Width(80));
            }
            
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                selectedLevelIndex = i;
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
    
    private void DrawSectionHeader(string title, int fontSize = 13)
    {
        EditorGUILayout.Space(5);
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = fontSize;
        EditorGUILayout.LabelField(title, headerStyle);
        DrawSectionDivider();
        EditorGUILayout.Space(3);
    }
    
    private void DrawSectionDivider()
    {
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        rect.height = 1;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
    }

    private void OpenLevelGraph()
    {
        LevelContainer container = levelContainerProp.objectReferenceValue as LevelContainer;
        
        if (container == null)
        {
            Debug.LogWarning("No Level Container assigned!");
            return;
        }

        string containerPath = AssetDatabase.GetAssetPath(container);
        
        if (string.IsNullOrEmpty(containerPath))
        {
            Debug.LogWarning("Could not find asset path for Level Container!");
            return;
        }

        try
        {
            var windowType = System.Type.GetType("LevelSystemEditorWindow");
            if (windowType != null)
            {
                var openMethod = windowType.GetMethod("OpenWithContainer", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                
                if (openMethod != null)
                {
                    openMethod.Invoke(null, new object[] { container });
                }
                else
                {
                    Selection.activeObject = container;
                    EditorGUIUtility.PingObject(container);
                }
            }
            else
            {
                Selection.activeObject = container;
                EditorGUIUtility.PingObject(container);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Could not open Level Graph: {e.Message}");
            Selection.activeObject = container;
            EditorGUIUtility.PingObject(container);
        }
    }
}