using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(LevelTrigger))]
public class LevelTriggerEditor : Editor
{
    private SerializedProperty visualCueProp;
    private SerializedProperty emoteAnimatorProp;
    private SerializedProperty levelControllerProp;
    private SerializedProperty levelContainerProp;
    private SerializedProperty selectedGroupProp;
    private SerializedProperty selectedLevelNameProp;
    private SerializedProperty triggerOnEnterProp;
    private SerializedProperty requiresInputProp;
    private SerializedProperty interactKeyProp;
    private SerializedProperty canTriggerMultipleTimesProp;
    private SerializedProperty toggleWithInteractKeyProp;
    private SerializedProperty interactPromptProp;
    private SerializedProperty promptTextComponentProp;
    private SerializedProperty promptTextProp;
    private SerializedProperty fadeInDurationProp;
    private SerializedProperty displayDurationProp;
    private SerializedProperty fadeOutDurationProp;
    private SerializedProperty loopPromptProp;
    private SerializedProperty promptLeftOffsetProp;
    private SerializedProperty promptRightOffsetProp;
    private SerializedProperty promptVerticalOffsetProp;
    private SerializedProperty continuousPositionUpdateProp;

    private int selectedGroupIndex = 0;
    private int selectedLevelIndex = 0;
    private string[] groupNames;
    private string[] levelNames;

    private void OnEnable()
    {
        visualCueProp = serializedObject.FindProperty("visualCue");
        emoteAnimatorProp = serializedObject.FindProperty("emoteAnimator");
        levelControllerProp = serializedObject.FindProperty("levelController");
        levelContainerProp = serializedObject.FindProperty("levelContainer");
        selectedGroupProp = serializedObject.FindProperty("selectedGroup");
        selectedLevelNameProp = serializedObject.FindProperty("selectedLevelName");
        triggerOnEnterProp = serializedObject.FindProperty("triggerOnEnter");
        requiresInputProp = serializedObject.FindProperty("requiresInput");
        interactKeyProp = serializedObject.FindProperty("interactKey");
        canTriggerMultipleTimesProp = serializedObject.FindProperty("canTriggerMultipleTimes");
        toggleWithInteractKeyProp = serializedObject.FindProperty("toggleWithInteractKey");
        interactPromptProp = serializedObject.FindProperty("interactPrompt");
        promptTextComponentProp = serializedObject.FindProperty("promptTextComponent");
        promptTextProp = serializedObject.FindProperty("promptText");
        fadeInDurationProp = serializedObject.FindProperty("fadeInDuration");
        displayDurationProp = serializedObject.FindProperty("displayDuration");
        fadeOutDurationProp = serializedObject.FindProperty("fadeOutDuration");
        loopPromptProp = serializedObject.FindProperty("loopPrompt");
        promptLeftOffsetProp = serializedObject.FindProperty("promptLeftOffset");
        promptRightOffsetProp = serializedObject.FindProperty("promptRightOffset");
        promptVerticalOffsetProp = serializedObject.FindProperty("promptVerticalOffset");
        continuousPositionUpdateProp = serializedObject.FindProperty("continuousPositionUpdate");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        LevelTrigger trigger = (LevelTrigger)target;

        // Configuration Warning Box at top
        DrawConfigurationWarning();

        EditorGUILayout.Space(5);

        // Visual Cue Header
        EditorGUILayout.LabelField("Visual Cue", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(visualCueProp, new GUIContent("Visual Cue GameObject"));
        EditorGUILayout.Space();

        // Emote Animator Header
        EditorGUILayout.LabelField("Emote Animator", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(emoteAnimatorProp, new GUIContent("Animator (Optional)"));
        EditorGUILayout.Space();

        // Trigger Settings Header (Moved up for better workflow)
        DrawTriggerSettings();
        EditorGUILayout.Space();

        // UI Prompt Section
        if (requiresInputProp.boolValue && !triggerOnEnterProp.boolValue)
        {
            DrawPromptSettings();
            EditorGUILayout.Space();
        }

        // Level Settings Header
        DrawLevelSettings();

        EditorGUILayout.Space();

        // Utility Buttons
        DrawUtilityButtons();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawConfigurationWarning()
    {
        bool triggerOnEnter = triggerOnEnterProp.boolValue;
        bool requiresInput = requiresInputProp.boolValue;

        if (!triggerOnEnter && !requiresInput)
        {
            EditorGUILayout.HelpBox(
                "⚠️ WARNING: Both 'Trigger On Enter' and 'Requires Input' are FALSE!\n" +
                "This trigger will NEVER activate. Please enable one of them.",
                MessageType.Error);
        }
        else if (triggerOnEnter)
        {
            EditorGUILayout.HelpBox(
                "✓ AUTO TRIGGER MODE: Level UI will open immediately when player enters.",
                MessageType.Info);
        }
         else if (requiresInput)
        {
            KeyCode key = (KeyCode)interactKeyProp.enumValueIndex;
            EditorGUILayout.HelpBox(
                $"✓ INPUT MODE: Player must press '{key}' to open level UI.",
                MessageType.Info);
        }
    }

    private void DrawTriggerSettings()
    {
        EditorGUILayout.LabelField("Trigger Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.PropertyField(triggerOnEnterProp, new GUIContent(
            "Trigger On Enter",
            "If TRUE, opens level UI immediately when player enters (ignores input requirement)"));
        
        EditorGUILayout.PropertyField(requiresInputProp, new GUIContent(
            "Requires Input",
            "If TRUE, requires key press to open level UI"));
        
        if (requiresInputProp.boolValue && !triggerOnEnterProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(interactKeyProp, new GUIContent("Interact Key"));
            EditorGUILayout.PropertyField(toggleWithInteractKeyProp, new GUIContent(
                "Toggle with Key",
                "If TRUE, pressing the interact key again will close the level UI"));
            EditorGUI.indentLevel--;
        }
        else if (requiresInputProp.boolValue && triggerOnEnterProp.boolValue)
        {
            EditorGUILayout.HelpBox(
                "Note: 'Requires Input' is ignored because 'Trigger On Enter' is enabled.",
                MessageType.Warning);
        }
        
        EditorGUILayout.PropertyField(canTriggerMultipleTimesProp, new GUIContent(
            "Can Trigger Multiple Times",
            "If FALSE, trigger only works once. If TRUE, can be triggered every time player enters."));
    }

    private void DrawPromptSettings()
    {
        EditorGUILayout.LabelField("UI Prompt", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Basic Prompt Settings
        EditorGUILayout.LabelField("Basic Settings", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(interactPromptProp, new GUIContent("Prompt GameObject"));
        EditorGUILayout.PropertyField(promptTextComponentProp, new GUIContent("Text Component"));
        EditorGUILayout.PropertyField(promptTextProp, new GUIContent("Prompt Message"));
        
        EditorGUILayout.Space(8);
        
        // Position Settings
        EditorGUILayout.LabelField("Dynamic Positioning", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;
        
        EditorGUILayout.PropertyField(promptLeftOffsetProp, new GUIContent(
            "Left Offset",
            "Horizontal offset when prompt appears on the left side of trigger"));
        
        EditorGUILayout.PropertyField(promptRightOffsetProp, new GUIContent(
            "Right Offset", 
            "Horizontal offset when prompt appears on the right side of trigger"));
        
        EditorGUILayout.PropertyField(promptVerticalOffsetProp, new GUIContent(
            "Vertical Offset",
            "Additional vertical offset for the prompt"));
        
        EditorGUILayout.PropertyField(continuousPositionUpdateProp, new GUIContent(
            "Continuous Update",
            "Update prompt position every frame (enable for moving triggers)"));
        
        EditorGUI.indentLevel--;
        
        EditorGUILayout.Space(8);
        
        // Animation Settings
        EditorGUILayout.LabelField("Fade Animation", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(fadeInDurationProp, new GUIContent("Fade In Duration"));
        EditorGUILayout.PropertyField(displayDurationProp, new GUIContent("Display Duration"));
        EditorGUILayout.PropertyField(fadeOutDurationProp, new GUIContent("Fade Out Duration"));
        EditorGUILayout.PropertyField(loopPromptProp, new GUIContent("Loop Animation"));
        EditorGUI.indentLevel--;
        
        EditorGUILayout.EndVertical();
        
        // Show positioning help
        if (interactPromptProp.objectReferenceValue != null)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "💡 Positioning Tip:\n" +
                "The prompt automatically positions itself on the side with more screen space. " +
                "Adjust the Left/Right offsets to control how far from the trigger it appears. " +
                "Negative left values move it left, positive right values move it right.",
                MessageType.Info);
        }
    }

    private void DrawLevelSettings()
    {
        EditorGUILayout.LabelField("Level Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(levelControllerProp, new GUIContent("Controller"));
        EditorGUILayout.PropertyField(levelContainerProp, new GUIContent("Container"));

        LevelContainer container = levelContainerProp.objectReferenceValue as LevelContainer;

        if (container != null)
        {
            EditorGUILayout.Space(5);

            // Get group names
            if (container.HasGroups())
            {
                groupNames = container.GetGroupsNames();
                
                // Add "All Groups" option
                List<string> groupOptions = new List<string> { "All Groups (Show Everything)" };
                groupOptions.AddRange(groupNames);
                
                // Find current group index
                LevelGroup currentGroup = selectedGroupProp.objectReferenceValue as LevelGroup;
                if (currentGroup != null)
                {
                    selectedGroupIndex = System.Array.IndexOf(groupNames, currentGroup.GroupName) + 1;
                    if (selectedGroupIndex < 0) selectedGroupIndex = 0;
                }
                else
                {
                    selectedGroupIndex = 0;
                }

                // Group dropdown
                EditorGUI.BeginChangeCheck();
                selectedGroupIndex = EditorGUILayout.Popup(
                    new GUIContent("Filter by Group", "Select a specific group to open, or 'All Groups' to show entire UI"),
                    selectedGroupIndex, 
                    groupOptions.ToArray());
                    
                if (EditorGUI.EndChangeCheck())
                {
                    // Update selected group
                    if (selectedGroupIndex == 0)
                    {
                        selectedGroupProp.objectReferenceValue = null;
                    }
                    else
                    {
                        // Find and assign the group
                        string selectedGroupName = groupNames[selectedGroupIndex - 1];
                        foreach (var kvp in container.Groups)
                        {
                            if (kvp.Key.GroupName == selectedGroupName)
                            {
                                selectedGroupProp.objectReferenceValue = kvp.Key;
                                break;
                            }
                        }
                    }
                    
                    // Reset level selection
                    selectedLevelNameProp.stringValue = "";
                    selectedLevelIndex = 0;
                }

                // Get levels for selected group
                List<Level> levelsList;
                if (selectedGroupIndex == 0)
                {
                    // All levels
                    levelsList = container.GetAllLevels();
                }
                else
                {
                    // Get levels for this group
                    LevelGroup group = selectedGroupProp.objectReferenceValue as LevelGroup;
                    if (group != null)
                    {
                        levelsList = container.GetLevelsInGroup(group);
                    }
                    else
                    {
                        levelsList = new List<Level>();
                    }
                }

                if (levelsList.Count > 0)
                {
                    // Add "(None)" option for levels
                    List<string> levelOptions = new List<string> { "(None - Show Group)" };
                    levelNames = levelsList.Select(l => l.LevelName).ToArray();
                    levelOptions.AddRange(levelNames);

                    // Find current level index
                    if (!string.IsNullOrEmpty(selectedLevelNameProp.stringValue))
                    {
                        selectedLevelIndex = System.Array.IndexOf(levelNames, selectedLevelNameProp.stringValue) + 1;
                        if (selectedLevelIndex < 0) selectedLevelIndex = 0;
                    }
                    else
                    {
                        selectedLevelIndex = 0;
                    }

                    // Level dropdown
                    EditorGUI.BeginChangeCheck();
                    selectedLevelIndex = EditorGUILayout.Popup(
                        new GUIContent("Auto-Select Level", "Optional: Automatically open this level's details when triggered"),
                        selectedLevelIndex, 
                        levelOptions.ToArray());
                        
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (selectedLevelIndex == 0)
                        {
                            // None selected
                            selectedLevelNameProp.stringValue = "";
                        }
                        else
                        {
                            selectedLevelNameProp.stringValue = levelNames[selectedLevelIndex - 1];
                        }
                    }

                    // Show level preview
                    if (!string.IsNullOrEmpty(selectedLevelNameProp.stringValue))
                    {
                        Level selectedLevel = levelsList.Find(l => l.LevelName == selectedLevelNameProp.stringValue);
                        if (selectedLevel != null)
                        {
                            EditorGUILayout.Space(5);
                            DrawLevelPreview(selectedLevel);
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No levels found in selected group.", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No groups found in LevelContainer.", MessageType.Warning);
                
                // Still show levels if ungrouped
                List<Level> allLevels = container.GetAllLevels();
                if (allLevels.Count > 0)
                {
                    List<string> levelOptions = new List<string> { "(None)" };
                    levelNames = allLevels.Select(l => l.LevelName).ToArray();
                    levelOptions.AddRange(levelNames);
                    
                    if (!string.IsNullOrEmpty(selectedLevelNameProp.stringValue))
                    {
                        selectedLevelIndex = System.Array.IndexOf(levelNames, selectedLevelNameProp.stringValue) + 1;
                        if (selectedLevelIndex < 0) selectedLevelIndex = 0;
                    }
                    else
                    {
                        selectedLevelIndex = 0;
                    }

                    EditorGUI.BeginChangeCheck();
                    selectedLevelIndex = EditorGUILayout.Popup("Auto-Select Level", selectedLevelIndex, levelOptions.ToArray());
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (selectedLevelIndex == 0)
                        {
                            selectedLevelNameProp.stringValue = "";
                        }
                        else
                        {
                            selectedLevelNameProp.stringValue = levelNames[selectedLevelIndex - 1];
                        }
                    }
                    
                    // Show level preview
                    if (!string.IsNullOrEmpty(selectedLevelNameProp.stringValue))
                    {
                        Level selectedLevel = allLevels.Find(l => l.LevelName == selectedLevelNameProp.stringValue);
                        if (selectedLevel != null)
                        {
                            EditorGUILayout.Space(5);
                            DrawLevelPreview(selectedLevel);
                        }
                    }
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Please assign a LevelContainer.", MessageType.Info);
        }
    }

    private void DrawLevelPreview(Level level)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Selected Level Preview", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        // Icon
        if (level.Icon != null)
        {
            Rect iconRect = GUILayoutUtility.GetRect(48, 48, GUILayout.Width(48), GUILayout.Height(48));
            GUI.DrawTexture(iconRect, level.Icon.texture, ScaleMode.ScaleToFit);
            GUILayout.Space(8);
        }
        
        // Info
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(level.LevelName, EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Tier {level.Tier} | Cost: {level.UnlockCost} SP", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndHorizontal();
        
        if (!string.IsNullOrEmpty(level.Description))
        {
            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField(level.Description, EditorStyles.wordWrappedLabel);
        }
        
        EditorGUILayout.EndVertical();
    }

    private void DrawUtilityButtons()
    {
        EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);
        
        LevelTrigger trigger = (LevelTrigger)target;
        
        if (Application.isPlaying)
        {
            // Runtime Controls
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Manual Trigger", GUILayout.Height(30)))
            {
                trigger.ManualTrigger();
            }
            
            if (GUILayout.Button("Reset Trigger", GUILayout.Height(30)))
            {
                trigger.ResetTrigger();
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            
            // Show current state
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Runtime Info", EditorStyles.boldLabel);
            
            GUIStyle statusStyle = new GUIStyle(EditorStyles.label);
            statusStyle.normal.textColor = trigger.IsPlayerInRange ? Color.green : Color.gray;
            EditorGUILayout.LabelField($"Player In Range: {trigger.IsPlayerInRange}", statusStyle);
            
            Level cachedLevel = trigger.GetCachedLevel();
            if (cachedLevel != null)
            {
                EditorGUILayout.LabelField($"Cached Level: {cachedLevel.LevelName}");
            }
            
            LevelGroup cachedGroup = trigger.GetSelectedGroup();
            if (cachedGroup != null)
            {
                EditorGUILayout.LabelField($"Selected Group: {cachedGroup.GroupName}");
            }
            
            EditorGUILayout.EndVertical();
        }
        else
        {
            // Editor Setup
            EditorGUILayout.HelpBox(
                "✓ Trigger is ready! Make sure to:\n" +
                "• Add a 2D Collider with 'Is Trigger' enabled\n" +
                "• Set the collider size to your desired trigger area\n" +
                "• Tag your player GameObject as 'Player'",
                MessageType.Info);
            
            // Check for collider
            Collider2D col = trigger.GetComponent<Collider2D>();
            if (col == null)
            {
                EditorGUILayout.HelpBox(
                    "⚠️ Missing Collider2D component! This trigger won't detect the player.",
                    MessageType.Warning);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Box Collider 2D", GUILayout.Height(25)))
                {
                    BoxCollider2D newCol = trigger.gameObject.AddComponent<BoxCollider2D>();
                    newCol.isTrigger = true;
                    newCol.size = new Vector2(2f, 2f); // Default size
                    EditorUtility.SetDirty(trigger);
                }
                
                if (GUILayout.Button("Add Circle Collider 2D", GUILayout.Height(25)))
                {
                    CircleCollider2D newCol = trigger.gameObject.AddComponent<CircleCollider2D>();
                    newCol.isTrigger = true;
                    newCol.radius = 1f; // Default radius
                    EditorUtility.SetDirty(trigger);
                }
                EditorGUILayout.EndHorizontal();
            }
            else if (!col.isTrigger)
            {
                EditorGUILayout.HelpBox(
                    "⚠️ Collider2D is not set as trigger! Enable 'Is Trigger' in the collider settings.",
                    MessageType.Warning);
                
                if (GUILayout.Button("Set as Trigger", GUILayout.Height(25)))
                {
                    col.isTrigger = true;
                    EditorUtility.SetDirty(trigger);
                }
            }
            else
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Setup Complete!", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Collider Type: {col.GetType().Name}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("✓ Is Trigger: Enabled", EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
            }
        }
    }
}