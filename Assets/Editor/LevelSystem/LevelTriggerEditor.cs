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
    private SerializedProperty interactPromptProp;
    private SerializedProperty promptTextProp;

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
        interactPromptProp = serializedObject.FindProperty("interactPrompt");
        promptTextProp = serializedObject.FindProperty("promptText");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Visual Cue Header
        EditorGUILayout.LabelField("Visual Cue", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(visualCueProp);
        EditorGUILayout.Space();

        // Emote Animator Header
        EditorGUILayout.LabelField("Emote Animator", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(emoteAnimatorProp);
        EditorGUILayout.Space();

        // UI Prompt Section
        EditorGUILayout.LabelField("UI Prompt", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(interactPromptProp);
        EditorGUILayout.PropertyField(promptTextProp);
        EditorGUILayout.Space();

        // Level Settings Header
        EditorGUILayout.LabelField("Level Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(levelControllerProp);
        EditorGUILayout.PropertyField(levelContainerProp);

        LevelContainer container = levelContainerProp.objectReferenceValue as LevelContainer;

        if (container != null)
        {
            EditorGUILayout.Space(5);

            // Get group names
            if (container.HasGroups())
            {
                groupNames = container.GetGroupsNames();
                
                // Add "All Groups" option
                List<string> groupOptions = new List<string> { "All Groups" };
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
                selectedGroupIndex = EditorGUILayout.Popup("Group", selectedGroupIndex, groupOptions.ToArray());
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
                    List<string> levelOptions = new List<string> { "(None)" };
                    levelNames = levelsList.Select(s => s.LevelName).ToArray();
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
                    selectedLevelIndex = EditorGUILayout.Popup("Level (Optional)", selectedLevelIndex, levelOptions.ToArray());
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
                        Level selectedLevel = levelsList.Find(s => s.LevelName == selectedLevelNameProp.stringValue);
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
                    levelNames = allLevels.Select(s => s.LevelName).ToArray();
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
                    selectedLevelIndex = EditorGUILayout.Popup("Level (Optional)", selectedLevelIndex, levelOptions.ToArray());
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
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Please assign a LevelContainer.", MessageType.Info);
        }

        EditorGUILayout.Space();

        // Trigger Settings Header
        EditorGUILayout.LabelField("Trigger Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(triggerOnEnterProp);
        EditorGUILayout.PropertyField(requiresInputProp);
        EditorGUILayout.PropertyField(interactKeyProp);
        EditorGUILayout.PropertyField(canTriggerMultipleTimesProp);

        EditorGUILayout.Space();

        // Utility Buttons
        DrawUtilityButtons();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawLevelPreview(Level level)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Selected Level Preview", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        // Icon
        if (level.Icon != null)
        {
            Rect iconRect = GUILayoutUtility.GetRect(32, 32, GUILayout.Width(32), GUILayout.Height(32));
            GUI.DrawTexture(iconRect, level.Icon.texture, ScaleMode.ScaleToFit);
            GUILayout.Space(5);
        }
        
        // Info
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(level.LevelName, EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Tier {level.Tier} | Cost: {level.UnlockCost} SP");
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
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Manual Trigger", GUILayout.Height(25)))
            {
                trigger.ManualTrigger();
            }
            
            if (GUILayout.Button("Reset Trigger", GUILayout.Height(25)))
            {
                trigger.ResetTrigger();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Show current state
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Runtime Info:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Player In Range: {trigger.playerInRange}");
            
            Level cachedLevel = trigger.GetCachedLevel();
            if (cachedLevel != null)
            {
                EditorGUILayout.LabelField($"Cached Level: {cachedLevel.LevelName}");
            }
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Setup complete! Add a 2D Collider with 'Is Trigger' enabled to this GameObject.",
                MessageType.Info);
            
            // Check for collider
            Collider2D col = trigger.GetComponent<Collider2D>();
            if (col == null)
            {
                EditorGUILayout.HelpBox("Missing Collider2D component! Add one to detect player.", MessageType.Warning);
                
                if (GUILayout.Button("Add Box Collider 2D", GUILayout.Height(25)))
                {
                    BoxCollider2D newCol = trigger.gameObject.AddComponent<BoxCollider2D>();
                    newCol.isTrigger = true;
                    EditorUtility.SetDirty(trigger);
                }
            }
            else if (!col.isTrigger)
            {
                EditorGUILayout.HelpBox("Collider2D is not set as trigger! Enable 'Is Trigger'.", MessageType.Warning);
                
                if (GUILayout.Button("Set as Trigger", GUILayout.Height(25)))
                {
                    col.isTrigger = true;
                    EditorUtility.SetDirty(trigger);
                }
            }
        }
    }
}