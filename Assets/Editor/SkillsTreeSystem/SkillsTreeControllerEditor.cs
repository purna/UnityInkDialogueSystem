using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(SkillsTreeController))]
public class SkillsTreeControllerEditor : Editor
{
    private SerializedProperty setupModeProp;
    private SerializedProperty manualSkillNodesProp;
    private SerializedProperty skillstreeContainerProp;
    private SerializedProperty skillstreeGroupProp;
    private SerializedProperty skillstreeProp;
    private SerializedProperty groupedSkillsTreesProp;
    private SerializedProperty startingSkillsTreesOnlyProp;
    private SerializedProperty selectedSkillsTreeGroupIndexProp;
    private SerializedProperty selectedSkillsTreeIndexProp;
    private SerializedProperty skillstreeManagerProp;
    private SerializedProperty skillstreeUIProp;
    private SerializedProperty startSkillsTreeOnStartProp;
    private SerializedProperty startDelayProp;
    private SerializedProperty skillNodesParentProp;
    private SerializedProperty skillNodePrefabProp;
    private SerializedProperty gridLayoutProp;
    private SerializedProperty useGridLayoutProp;
    private SerializedProperty useSkillPositionsProp;
    private SerializedProperty autoGenerateLinesProp;
    private SerializedProperty detailsPanelProp;
    
    // For testing/preview
    private int selectedGroupIndex = 0;
    private int selectedSkillIndex = 0;
    private bool showAllSkills = false;
    private Vector2 skillsScrollPosition;
    private bool showManualNodesList = true;

    private void OnEnable()
    {
        setupModeProp = serializedObject.FindProperty("_setupMode");
        manualSkillNodesProp = serializedObject.FindProperty("_manualSkillNodes");
        skillstreeContainerProp = serializedObject.FindProperty("skillstreeContainer");
        skillstreeGroupProp = serializedObject.FindProperty("skillstreeGroup");
        skillstreeProp = serializedObject.FindProperty("skillstree");
        groupedSkillsTreesProp = serializedObject.FindProperty("groupedSkillsTrees");
        startingSkillsTreesOnlyProp = serializedObject.FindProperty("startingSkillsTreesOnly");
        selectedSkillsTreeGroupIndexProp = serializedObject.FindProperty("selectedSkillsTreeGroupIndex");
        selectedSkillsTreeIndexProp = serializedObject.FindProperty("selectedSkillsTreeIndex");
        skillstreeManagerProp = serializedObject.FindProperty("skillstreeManager");
        skillstreeUIProp = serializedObject.FindProperty("skillstreeUI");
        startSkillsTreeOnStartProp = serializedObject.FindProperty("startSkillsTreeOnStart");
        startDelayProp = serializedObject.FindProperty("startDelay");
        skillNodesParentProp = serializedObject.FindProperty("_skillNodesParent");
        skillNodePrefabProp = serializedObject.FindProperty("_skillNodePrefab");
        gridLayoutProp = serializedObject.FindProperty("_gridLayout");
        useGridLayoutProp = serializedObject.FindProperty("_useGridLayout");
        useSkillPositionsProp = serializedObject.FindProperty("_useSkillPositions");
        autoGenerateLinesProp = serializedObject.FindProperty("_autoGenerateLines");
        detailsPanelProp = serializedObject.FindProperty("_detailsPanel");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Header
        DrawSectionHeader("SkillsTree Controller", 16);
        EditorGUILayout.Space(5);

        // Setup Mode Selection
        DrawSectionHeader("Setup Mode");
        EditorGUILayout.PropertyField(setupModeProp, new GUIContent("Setup Mode"));
        
        SkillTreeSetupMode currentMode = (SkillTreeSetupMode)setupModeProp.enumValueIndex;
        
        // Show appropriate info box
        if (currentMode == SkillTreeSetupMode.ManualAssignment)
        {
            EditorGUILayout.HelpBox(
                "Manual Assignment Mode: Assign skills to pre-placed UI GameObjects in your scene. " +
                "Each SkillsTreeChoicer component will have a dropdown to select its skill.",
                MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Auto Generate Mode: Skills will be automatically instantiated from the skill node prefab. " +
                "All skills from the selected source will be generated.",
                MessageType.Info);
        }
        
        EditorGUILayout.Space(10);

        // SkillsTree Data Section (always show)
        DrawSectionHeader("SkillsTree Data");
        EditorGUILayout.PropertyField(skillstreeContainerProp);

        // Open SkillsTree Graph Button
        if (skillstreeContainerProp.objectReferenceValue != null)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Open SkillsTree Graph", GUILayout.Height(25), GUILayout.Width(200)))
            {
                OpenSkillsTreeGraph();
            }
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
            
            // Container loaded - show group and skill selection
            DrawContainerContents();
        }
        else
        {
            EditorGUILayout.HelpBox("Select a SkillsTree Container to continue.", MessageType.Info);
        }

        EditorGUILayout.Space(10);

        // Mode-specific settings
        if (currentMode == SkillTreeSetupMode.ManualAssignment)
        {
            DrawManualAssignmentSettings();
        }
        else
        {
            DrawAutoGenerateSettings();
        }

        EditorGUILayout.Space(10);

        // SkillsTree Selection Section (for filtering)
        DrawSectionHeader("SkillsTree Selection");
        EditorGUILayout.PropertyField(groupedSkillsTreesProp, new GUIContent("Use Grouped Skills"));
        EditorGUILayout.PropertyField(startingSkillsTreesOnlyProp, new GUIContent("Starting Skills Only"));
        
        EditorGUILayout.HelpBox(
            "These settings determine which skills are available:\n" +
            "• Use Grouped Skills: Load from a specific group\n" +
            "• Starting Skills Only: Only include skills marked as starting skills\n" +
            "• If a single skill is selected, all connected skills in its tree will be loaded",
            MessageType.Info);

        EditorGUILayout.Space(10);

        // System References Section
        DrawSectionHeader("System References");
        EditorGUILayout.PropertyField(skillstreeManagerProp);
        EditorGUILayout.PropertyField(skillstreeUIProp);
        EditorGUILayout.PropertyField(detailsPanelProp);

        EditorGUILayout.Space(10);

        // Auto Start Settings Section
        DrawSectionHeader("Auto Start Settings");
        EditorGUILayout.PropertyField(startSkillsTreeOnStartProp);
        
        if (startSkillsTreeOnStartProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(startDelayProp);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        // Utility buttons
        DrawUtilityButtons();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawManualAssignmentSettings()
    {
        DrawSectionHeader("Manual Assignment Settings");
        
        // Show auto-generate lines option
        EditorGUILayout.PropertyField(autoGenerateLinesProp, new GUIContent("Auto Generate Connection Lines"));
        
        EditorGUILayout.Space(5);
        
        // Instructions
        EditorGUILayout.HelpBox(
            "How to set up manual nodes:\n" +
            "1. Place UI GameObjects in your scene for each skill node\n" +
            "2. Add SkillsTreeChoicer components to each GameObject\n" +
            "3. Assign this controller to each SkillsTreeChoicer\n" +
            "4. Use the dropdown on each SkillsTreeChoicer to select its skill",
            MessageType.Info);
        
        EditorGUILayout.Space(5);
        
        // Find existing choicers in scene
        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Manual node detection only works in Edit mode.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Find SkillsTreeChoicers in Scene", GUILayout.Height(25)))
            {
                FindAndListChoicers();
            }
            if (GUILayout.Button("Refresh All Choicers", GUILayout.Height(25)))
            {
                RefreshAllChoicers();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Auto-Assign Skills to All Choicers", GUILayout.Height(30)))
            {
                AutoAssignSkillsToChoicers();
            }
        }
        
        EditorGUILayout.Space(5);
        
        // Show manual nodes list (legacy support)
        showManualNodesList = EditorGUILayout.Foldout(showManualNodesList, 
            $"Manual Node Mappings (Legacy - {manualSkillNodesProp.arraySize})", true);
        
        if (showManualNodesList)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox(
                "Note: This list is mainly for legacy support. " +
                "It's easier to use the dropdown on each SkillsTreeChoicer component directly.",
                MessageType.Info);
            EditorGUILayout.PropertyField(manualSkillNodesProp, true);
            EditorGUI.indentLevel--;
        }
    }

    private void DrawAutoGenerateSettings()
    {
        DrawSectionHeader("Auto Generate Settings");
        
        EditorGUILayout.PropertyField(skillNodesParentProp, new GUIContent("Skill Nodes Parent"));
        EditorGUILayout.PropertyField(skillNodePrefabProp, new GUIContent("Skill Node Prefab"));
        
        EditorGUILayout.Space(5);
        
        // Layout options
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
            EditorGUILayout.PropertyField(useSkillPositionsProp, new GUIContent("Use Skill Positions"));
            EditorGUI.indentLevel--;
            
            if (!useSkillPositionsProp.boolValue)
            {
                EditorGUILayout.HelpBox("Skills will be placed at (0,0). Use the Graph Editor to set positions.", MessageType.Info);
            }
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.PropertyField(autoGenerateLinesProp, new GUIContent("Auto Generate Connection Lines"));
        
        // Validation
        EditorGUILayout.Space(5);
        if (skillNodesParentProp.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Skill Nodes Parent is required for auto-generation!", MessageType.Error);
        }
        
        if (skillNodePrefabProp.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Skill Node Prefab is required for auto-generation!", MessageType.Error);
        }
    }

    private void DrawUtilityButtons()
    {
        DrawSectionHeader("Utilities");
        
        SkillsTreeController controller = (SkillsTreeController)target;
        
        if (Application.isPlaying)
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Refresh Skill Tree", GUILayout.Height(30)))
            {
                controller.RefreshSkillTree();
                Debug.Log("Skill tree refreshed!");
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Show available skills count
            List<Skill> availableSkills = controller.GetAvailableSkills();
            EditorGUILayout.LabelField($"Available Skills: {availableSkills?.Count ?? 0}");
            
            if (availableSkills != null && availableSkills.Count > 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Skills that will be generated:", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                foreach (var skill in availableSkills)
                {
                    EditorGUILayout.LabelField($"• {skill.SkillName}");
                }
                EditorGUI.indentLevel--;
            }
        }
        else
        {
            // Edit mode utilities
            SkillTreeSetupMode currentMode = (SkillTreeSetupMode)setupModeProp.enumValueIndex;
            
            if (currentMode == SkillTreeSetupMode.AutoGenerate)
            {
                EditorGUILayout.HelpBox(
                    "In Play mode, you can refresh the skill tree to see changes.",
                    MessageType.Info);
                
                // Preview what will be generated
                List<Skill> previewSkills = controller.GetAvailableSkills();
                if (previewSkills != null && previewSkills.Count > 0)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField($"Will generate {previewSkills.Count} skills:", EditorStyles.boldLabel);
                    
                    EditorGUI.indentLevel++;
                    int displayCount = Mathf.Min(10, previewSkills.Count);
                    for (int i = 0; i < displayCount; i++)
                    {
                        EditorGUILayout.LabelField($"• {previewSkills[i].SkillName}");
                    }
                    if (previewSkills.Count > 10)
                    {
                        EditorGUILayout.LabelField($"... and {previewSkills.Count - 10} more");
                    }
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.HelpBox("No skills will be generated with current settings.", MessageType.Warning);
                }
            }
        }
    }

    private void FindAndListChoicers()
    {
        SkillNode[] choicers = FindObjectsOfType<SkillNode>();
        
        if (choicers.Length == 0)
        {
            EditorUtility.DisplayDialog("No Choicers Found", 
                "No SkillsTreeChoicer components found in the current scene.", "OK");
            return;
        }
        
        string message = $"Found {choicers.Length} SkillsTreeChoicer(s) in the scene:\n\n";
        int assignedCount = 0;
        int unassignedCount = 0;
        
        foreach (var choicer in choicers)
        {
            Skill skill = choicer.GetSkill();
            if (skill != null)
            {
                message += $"✓ {choicer.gameObject.name} → {skill.SkillName}\n";
                assignedCount++;
            }
            else
            {
                message += $"✗ {choicer.gameObject.name} → (No skill assigned)\n";
                unassignedCount++;
            }
        }
        
        message += $"\n{assignedCount} assigned, {unassignedCount} unassigned";
        
        EditorUtility.DisplayDialog("SkillsTreeChoicers in Scene", message, "OK");
    }

    private void RefreshAllChoicers()
    {
        SkillsTreeController controller = (SkillsTreeController)target;
        SkillNode[] choicers = FindObjectsOfType<SkillNode>();
        
        if (choicers.Length == 0)
        {
            EditorUtility.DisplayDialog("No Choicers Found", 
                "No SkillsTreeChoicer components found in the current scene.", "OK");
            return;
        }
        
        int updated = 0;
        
        foreach (var choicer in choicers)
        {
            // Set controller reference
            choicer.SetController(controller);
            
            // Mark as dirty so Unity saves the changes
            EditorUtility.SetDirty(choicer);
            updated++;
        }
        
        EditorUtility.DisplayDialog("Choicers Refreshed", 
            $"Updated {updated} SkillsTreeChoicer component(s) with the current controller reference.", "OK");
    }
    
    private void AutoAssignSkillsToChoicers()
    {
        SkillsTreeController controller = (SkillsTreeController)target;
        SkillNode[] choicers = FindObjectsOfType<SkillNode>();
        
        if (choicers.Length == 0)
        {
            EditorUtility.DisplayDialog("No Choicers Found", 
                "No SkillsTreeChoicer components found in the current scene.", "OK");
            return;
        }
        
        List<Skill> availableSkills = controller.GetAvailableSkills();
        
        if (availableSkills == null || availableSkills.Count == 0)
        {
            EditorUtility.DisplayDialog("No Skills Available", 
                "No skills available from the controller. Make sure a SkillsTreeContainer is assigned.", "OK");
            return;
        }
        
        int assigned = 0;
        int skipped = 0;
        
        foreach (var choicer in choicers)
        {
            // Skip if already has a skill
            if (choicer.GetSkill() != null)
            {
                skipped++;
                continue;
            }
            
            // Assign next available skill
            if (assigned < availableSkills.Count)
            {
                choicer.SetController(controller);
                choicer.SetSkillIndex(assigned);
                choicer.SetSkill(availableSkills[assigned]);
                EditorUtility.SetDirty(choicer);
                assigned++;
            }
        }
        
        string message = $"Auto-assignment complete!\n\n" +
                        $"Assigned: {assigned}\n" +
                        $"Skipped (already assigned): {skipped}\n" +
                        $"Total choicers: {choicers.Length}";
        
        if (assigned < choicers.Length - skipped)
        {
            message += $"\n\nNote: Not enough skills for all choicers. " +
                      $"You have {choicers.Length} choicers but only {availableSkills.Count} skills available.";
        }
        
        EditorUtility.DisplayDialog("Auto-Assignment Complete", message, "OK");
    }
    
    private void DrawContainerContents()
    {
        SkillsTreeContainer container = skillstreeContainerProp.objectReferenceValue as SkillsTreeContainer;
        if (container == null) return;
        
        EditorGUILayout.Space(5);
        
        // Show container info
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Container Info", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Name:", container.TreeName);
        EditorGUILayout.LabelField("Groups:", container.HasGroups() ? container.GetGroupsNames().Length.ToString() : "0");
        EditorGUILayout.LabelField("Ungrouped Skills:", container.GetUngroupedSkillNames(false).Count.ToString());
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        // Group Selection
        if (container.HasGroups())
        {
            DrawSectionHeader("Select Group for Testing");
            
            string[] groupNames = container.GetGroupsNames();
            
            // Clamp index
            if (selectedGroupIndex >= groupNames.Length)
                selectedGroupIndex = 0;
            
            int newGroupIndex = EditorGUILayout.Popup("Group", selectedGroupIndex, groupNames);
            
            if (newGroupIndex != selectedGroupIndex)
            {
                selectedGroupIndex = newGroupIndex;
                selectedSkillIndex = 0; // Reset skill index when group changes
            }
            
            string selectedGroupName = groupNames[selectedGroupIndex];
            SkillsTreeGroup selectedGroup = AssetsUtility.LoadAsset<SkillsTreeGroup>(
                $"Assets/_Project/ScriptableObjects/SkillsTree/{container.TreeName}/Groups/{selectedGroupName}",
                selectedGroupName
            );
            
            if (selectedGroup != null)
            {
                // Update the property
                skillstreeGroupProp.objectReferenceValue = selectedGroup;
                selectedSkillsTreeGroupIndexProp.intValue = selectedGroupIndex;
                
                // Show group info
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"Group: {selectedGroup.GroupName}", EditorStyles.boldLabel);
                
                List<string> groupSkills = container.GetGroupedSkillNames(selectedGroup, false);
                EditorGUILayout.LabelField("Total Skills:", groupSkills.Count.ToString());
                
                List<string> startingSkills = container.GetGroupedSkillNames(selectedGroup, true);
                EditorGUILayout.LabelField("Starting Skills:", startingSkills.Count.ToString());
                
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(5);
                
                // Skill Selection
                if (groupSkills.Count > 0)
                {
                    DrawSkillSelection(container, selectedGroup, groupSkills, 
                        $"Assets/_Project/ScriptableObjects/SkillsTree/{container.TreeName}/Groups/{selectedGroupName}/SkillsTree");
                }
                else
                {
                    EditorGUILayout.HelpBox("No skills in this group.", MessageType.Info);
                }
            }
        }
        else
        {
            // Show ungrouped skills
            DrawSectionHeader("Ungrouped Skills");
            
            List<string> ungroupedSkills = container.GetUngroupedSkillNames(false);
            
            if (ungroupedSkills.Count > 0)
            {
                DrawSkillSelection(container, null, ungroupedSkills,
                    $"Assets/_Project/ScriptableObjects/SkillsTree/{container.TreeName}/Global/SkillsTree");
            }
            else
            {
                EditorGUILayout.HelpBox("No ungrouped skills in this container.", MessageType.Info);
            }
        }
    }
    
    private void DrawSkillSelection(SkillsTreeContainer container, SkillsTreeGroup group, List<string> skillNames, string skillFolderPath)
    {
        DrawSectionHeader("Select Skill for Testing");
        
        // Clamp index
        if (selectedSkillIndex >= skillNames.Count)
            selectedSkillIndex = 0;
        
        selectedSkillIndex = EditorGUILayout.Popup("Skill", selectedSkillIndex, skillNames.ToArray());
        
        string selectedSkillName = skillNames[selectedSkillIndex];
        Skill selectedSkill = AssetsUtility.LoadAsset<Skill>(skillFolderPath, selectedSkillName);
        
        if (selectedSkill != null)
        {
            // Update properties
            skillstreeProp.objectReferenceValue = selectedSkill;
            selectedSkillsTreeIndexProp.intValue = selectedSkillIndex;
            
            // Show skill preview
            DrawSkillPreview(selectedSkill);
            
            EditorGUILayout.Space(5);
            
            // Quick action buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Select Skill Asset", GUILayout.Height(25)))
            {
                Selection.activeObject = selectedSkill;
                EditorGUIUtility.PingObject(selectedSkill);
            }
            
            if (GUILayout.Button("Edit in Inspector", GUILayout.Height(25)))
            {
                Selection.activeObject = selectedSkill;
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
        }
        
        // Toggle to show all skills
        showAllSkills = EditorGUILayout.Foldout(showAllSkills, $"Show All Skills ({skillNames.Count})", true);
        
        if (showAllSkills)
        {
            DrawAllSkillsList(skillNames, skillFolderPath);
        }
    }
    
    private void DrawSkillPreview(Skill skill)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Header with icon
        EditorGUILayout.BeginHorizontal();
        
        // Icon preview
        if (skill.Icon != null)
        {
            Rect iconRect = GUILayoutUtility.GetRect(48, 48, GUILayout.Width(48), GUILayout.Height(48));
            GUI.DrawTexture(iconRect, skill.Icon.texture, ScaleMode.ScaleToFit);
            GUILayout.Space(10);
        }
        
        // Skill info
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(skill.SkillName, EditorStyles.boldLabel);
        
        EditorGUILayout.LabelField("Tier:", skill.Tier.ToString());
        EditorGUILayout.LabelField("Type:", skill.SkillType.ToString());
        EditorGUILayout.LabelField("Cost:", skill.UnlockCost.ToString());
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Description
        if (!string.IsNullOrEmpty(skill.Description))
        {
            EditorGUILayout.LabelField("Description:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(skill.Description, EditorStyles.wordWrappedLabel);
        }
        
        EditorGUILayout.Space(5);
        
        // Stats
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Value:", GUILayout.Width(60));
        EditorGUILayout.LabelField(skill.Value.ToString("F2"));
        EditorGUILayout.LabelField("Max Level:", GUILayout.Width(80));
        EditorGUILayout.LabelField(skill.MaxLevel.ToString());
        EditorGUILayout.EndHorizontal();
        
        if (skill.MaxLevel > 1)
        {
            EditorGUILayout.LabelField($"At Max Level: {skill.Value * skill.MaxLevel:F2}");
        }
        
        // Connections
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Prerequisites: {skill.Prerequisites.Count}", GUILayout.Width(150));
        EditorGUILayout.LabelField($"Children: {skill.Children.Count}");
        EditorGUILayout.EndHorizontal();
        
        // Runtime status (if in play mode)
        if (Application.isPlaying)
        {
            EditorGUILayout.Space(5);
            DrawSectionDivider();
            EditorGUILayout.LabelField("Runtime Status", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Unlocked:");
            EditorGUILayout.LabelField(skill.IsUnlocked ? "✓ Yes" : "✗ No", 
                skill.IsUnlocked ? EditorStyles.boldLabel : EditorStyles.label);
            EditorGUILayout.EndHorizontal();
            
            if (skill.IsUnlocked)
            {
                EditorGUILayout.LabelField("Current Level:", skill.CurrentLevel.ToString());
                EditorGUILayout.LabelField("Scaled Value:", skill.GetScaledValue().ToString("F2"));
            }
            
            EditorGUILayout.LabelField("Can Unlock:", skill.CanUnlock() ? "✓ Yes" : "✗ No");
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawAllSkillsList(List<string> skillNames, string skillFolderPath)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        skillsScrollPosition = EditorGUILayout.BeginScrollView(skillsScrollPosition, GUILayout.MaxHeight(300));
        
        for (int i = 0; i < skillNames.Count; i++)
        {
            EditorGUILayout.BeginHorizontal(i % 2 == 0 ? EditorStyles.helpBox : GUIStyle.none);
            
            // Skill number
            EditorGUILayout.LabelField((i + 1).ToString(), GUILayout.Width(30));
            
            // Skill name (clickable)
            if (GUILayout.Button(skillNames[i], EditorStyles.label))
            {
                selectedSkillIndex = i;
                
                Skill skill = AssetsUtility.LoadAsset<Skill>(skillFolderPath, skillNames[i]);
                if (skill != null)
                {
                    Selection.activeObject = skill;
                    EditorGUIUtility.PingObject(skill);
                }
            }
            
            // Quick load skill data
            Skill loadedSkill = AssetsUtility.LoadAsset<Skill>(skillFolderPath, skillNames[i]);
            if (loadedSkill != null)
            {
                // Show icon
                if (loadedSkill.Icon != null)
                {
                    Rect iconRect = GUILayoutUtility.GetRect(20, 20, GUILayout.Width(20), GUILayout.Height(20));
                    GUI.DrawTexture(iconRect, loadedSkill.Icon.texture, ScaleMode.ScaleToFit);
                }
                
                // Show tier
                EditorGUILayout.LabelField($"T{loadedSkill.Tier}", GUILayout.Width(30));
                
                // Show type
                EditorGUILayout.LabelField(loadedSkill.SkillType.ToString(), GUILayout.Width(80));
                
                // Show cost
                EditorGUILayout.LabelField($"${loadedSkill.UnlockCost}", GUILayout.Width(50));
            }
            
            // Select button
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                selectedSkillIndex = i;
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

    private void OpenSkillsTreeGraph()
    {
        SkillsTreeContainer container = skillstreeContainerProp.objectReferenceValue as SkillsTreeContainer;
        
        if (container == null)
        {
            Debug.LogWarning("No SkillsTree Container assigned!");
            return;
        }

        // Get the asset path of the container
        string containerPath = AssetDatabase.GetAssetPath(container);
        
        if (string.IsNullOrEmpty(containerPath))
        {
            Debug.LogWarning("Could not find asset path for SkillsTree Container!");
            return;
        }

        // Try to open the SkillsTree Graph Editor Window using reflection
        // This way it won't break if the window doesn't exist
        try
        {
            var windowType = System.Type.GetType("SkillsTreeSystemEditorWindow");
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
                    Debug.LogWarning("SkillsTreeSystemEditorWindow.OpenWithContainer method not found!");
                    // Fallback: just select the container
                    Selection.activeObject = container;
                    EditorGUIUtility.PingObject(container);
                }
            }
            else
            {
                Debug.LogWarning("SkillsTreeSystemEditorWindow not found! Selecting container instead.");
                // Fallback: just select the container
                Selection.activeObject = container;
                EditorGUIUtility.PingObject(container);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Could not open SkillsTree Graph: {e.Message}");
            // Fallback: just select the container
            Selection.activeObject = container;
            EditorGUIUtility.PingObject(container);
        }
    }
}