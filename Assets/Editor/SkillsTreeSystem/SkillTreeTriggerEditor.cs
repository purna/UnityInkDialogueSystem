using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(SkillTreeTrigger))]
public class SkillTreeTriggerEditor : Editor
{
    private SerializedProperty visualCueProp;
    private SerializedProperty emoteAnimatorProp;
    private SerializedProperty skillsTreeControllerProp;
    private SerializedProperty skillsTreeContainerProp;
    private SerializedProperty selectedGroupProp;
    private SerializedProperty selectedSkillNameProp;
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

    private int selectedGroupIndex = 0;
    private int selectedSkillIndex = 0;
    private string[] groupNames;
    private string[] skillNames;

    private void OnEnable()
    {
        visualCueProp = serializedObject.FindProperty("visualCue");
        emoteAnimatorProp = serializedObject.FindProperty("emoteAnimator");
        skillsTreeControllerProp = serializedObject.FindProperty("skillsTreeController");
        skillsTreeContainerProp = serializedObject.FindProperty("skillsTreeContainer");
        selectedGroupProp = serializedObject.FindProperty("selectedGroup");
        selectedSkillNameProp = serializedObject.FindProperty("selectedSkillName");
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
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        SkillTreeTrigger trigger = (SkillTreeTrigger)target;

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
            EditorGUILayout.LabelField("UI Prompt", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(interactPromptProp, new GUIContent("Prompt GameObject"));
            EditorGUILayout.PropertyField(promptTextComponentProp, new GUIContent("Text Component"));
            EditorGUILayout.PropertyField(promptTextProp, new GUIContent("Prompt Message"));
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Fade Animation", EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(fadeInDurationProp, new GUIContent("Fade In Duration"));
            EditorGUILayout.PropertyField(displayDurationProp, new GUIContent("Display Duration"));
            EditorGUILayout.PropertyField(fadeOutDurationProp, new GUIContent("Fade Out Duration"));
            EditorGUILayout.PropertyField(loopPromptProp, new GUIContent("Loop Animation"));
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
        }

        // Skill Tree Settings Header
        DrawSkillTreeSettings();

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
                "✓ AUTO TRIGGER MODE: Skill tree will open immediately when player enters.",
                MessageType.Info);
        }
        else if (requiresInput)
        {
            KeyCode key = (KeyCode)interactKeyProp.enumValueIndex;
            EditorGUILayout.HelpBox(
                $"✓ INPUT MODE: Player must press '{key}' to open skill tree.",
                MessageType.Info);
        }
    }

    private void DrawTriggerSettings()
    {
        EditorGUILayout.LabelField("Trigger Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.PropertyField(triggerOnEnterProp, new GUIContent(
            "Trigger On Enter",
            "If TRUE, opens skill tree immediately when player enters (ignores input requirement)"));
        
        EditorGUILayout.PropertyField(requiresInputProp, new GUIContent(
            "Requires Input",
            "If TRUE, requires key press to open skill tree"));
        
        if (requiresInputProp.boolValue && !triggerOnEnterProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(interactKeyProp, new GUIContent("Interact Key"));
            EditorGUILayout.PropertyField(toggleWithInteractKeyProp, new GUIContent(
                "Toggle with Key",
                "If TRUE, pressing the interact key again will close the skill tree"));
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

    private void DrawSkillTreeSettings()
    {
        EditorGUILayout.LabelField("Skill Tree Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(skillsTreeControllerProp, new GUIContent("Controller"));
        EditorGUILayout.PropertyField(skillsTreeContainerProp, new GUIContent("Container"));

        SkillsTreeContainer container = skillsTreeContainerProp.objectReferenceValue as SkillsTreeContainer;

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
                SkillsTreeGroup currentGroup = selectedGroupProp.objectReferenceValue as SkillsTreeGroup;
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
                    new GUIContent("Filter by Group", "Select a specific group to open, or 'All Groups' to show entire tree"),
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
                    
                    // Reset skill selection
                    selectedSkillNameProp.stringValue = "";
                    selectedSkillIndex = 0;
                }

                // Get skills for selected group
                List<Skill> skillsList;
                if (selectedGroupIndex == 0)
                {
                    // All skills
                    skillsList = container.GetAllSkills();
                }
                else
                {
                    // Get skills for this group
                    SkillsTreeGroup group = selectedGroupProp.objectReferenceValue as SkillsTreeGroup;
                    if (group != null)
                    {
                        skillsList = container.GetSkillsInGroup(group);
                    }
                    else
                    {
                        skillsList = new List<Skill>();
                    }
                }

                if (skillsList.Count > 0)
                {
                    // Add "(None)" option for skills
                    List<string> skillOptions = new List<string> { "(None - Show Group)" };
                    skillNames = skillsList.Select(s => s.SkillName).ToArray();
                    skillOptions.AddRange(skillNames);

                    // Find current skill index
                    if (!string.IsNullOrEmpty(selectedSkillNameProp.stringValue))
                    {
                        selectedSkillIndex = System.Array.IndexOf(skillNames, selectedSkillNameProp.stringValue) + 1;
                        if (selectedSkillIndex < 0) selectedSkillIndex = 0;
                    }
                    else
                    {
                        selectedSkillIndex = 0;
                    }

                    // Skill dropdown
                    EditorGUI.BeginChangeCheck();
                    selectedSkillIndex = EditorGUILayout.Popup(
                        new GUIContent("Auto-Select Skill", "Optional: Automatically open this skill's details when triggered"),
                        selectedSkillIndex, 
                        skillOptions.ToArray());
                        
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (selectedSkillIndex == 0)
                        {
                            // None selected
                            selectedSkillNameProp.stringValue = "";
                        }
                        else
                        {
                            selectedSkillNameProp.stringValue = skillNames[selectedSkillIndex - 1];
                        }
                    }

                    // Show skill preview
                    if (!string.IsNullOrEmpty(selectedSkillNameProp.stringValue))
                    {
                        Skill selectedSkill = skillsList.Find(s => s.SkillName == selectedSkillNameProp.stringValue);
                        if (selectedSkill != null)
                        {
                            EditorGUILayout.Space(5);
                            DrawSkillPreview(selectedSkill);
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No skills found in selected group.", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No groups found in SkillsTreeContainer.", MessageType.Warning);
                
                // Still show skills if ungrouped
                List<Skill> allSkills = container.GetAllSkills();
                if (allSkills.Count > 0)
                {
                    List<string> skillOptions = new List<string> { "(None)" };
                    skillNames = allSkills.Select(s => s.SkillName).ToArray();
                    skillOptions.AddRange(skillNames);
                    
                    if (!string.IsNullOrEmpty(selectedSkillNameProp.stringValue))
                    {
                        selectedSkillIndex = System.Array.IndexOf(skillNames, selectedSkillNameProp.stringValue) + 1;
                        if (selectedSkillIndex < 0) selectedSkillIndex = 0;
                    }
                    else
                    {
                        selectedSkillIndex = 0;
                    }

                    EditorGUI.BeginChangeCheck();
                    selectedSkillIndex = EditorGUILayout.Popup("Auto-Select Skill", selectedSkillIndex, skillOptions.ToArray());
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (selectedSkillIndex == 0)
                        {
                            selectedSkillNameProp.stringValue = "";
                        }
                        else
                        {
                            selectedSkillNameProp.stringValue = skillNames[selectedSkillIndex - 1];
                        }
                    }
                    
                    // Show skill preview
                    if (!string.IsNullOrEmpty(selectedSkillNameProp.stringValue))
                    {
                        Skill selectedSkill = allSkills.Find(s => s.SkillName == selectedSkillNameProp.stringValue);
                        if (selectedSkill != null)
                        {
                            EditorGUILayout.Space(5);
                            DrawSkillPreview(selectedSkill);
                        }
                    }
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Please assign a SkillsTreeContainer.", MessageType.Info);
        }
    }

    private void DrawSkillPreview(Skill skill)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Selected Skill Preview", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        // Icon
        if (skill.Icon != null)
        {
            Rect iconRect = GUILayoutUtility.GetRect(48, 48, GUILayout.Width(48), GUILayout.Height(48));
            GUI.DrawTexture(iconRect, skill.Icon.texture, ScaleMode.ScaleToFit);
            GUILayout.Space(8);
        }
        
        // Info
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(skill.SkillName, EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Tier {skill.Tier} | Cost: {skill.UnlockCost} SP", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndHorizontal();
        
        if (!string.IsNullOrEmpty(skill.Description))
        {
            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField(skill.Description, EditorStyles.wordWrappedLabel);
        }
        
        EditorGUILayout.EndVertical();
    }

    private void DrawUtilityButtons()
    {
        EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);
        
        SkillTreeTrigger trigger = (SkillTreeTrigger)target;
        
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
            
            Skill cachedSkill = trigger.GetCachedSkill();
            if (cachedSkill != null)
            {
                EditorGUILayout.LabelField($"Cached Skill: {cachedSkill.SkillName}");
            }
            
            SkillsTreeGroup cachedGroup = trigger.GetSelectedGroup();
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