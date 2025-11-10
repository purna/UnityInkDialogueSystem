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
    private SerializedProperty interactPromptProp;
    private SerializedProperty promptTextProp;

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

        // Skill Tree Settings Header
        EditorGUILayout.LabelField("Skill Tree Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(skillsTreeControllerProp);
        EditorGUILayout.PropertyField(skillsTreeContainerProp);

        SkillsTreeContainer container = skillsTreeContainerProp.objectReferenceValue as SkillsTreeContainer;

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
                    List<string> skillOptions = new List<string> { "(None)" };
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
                    selectedSkillIndex = EditorGUILayout.Popup("Skill (Optional)", selectedSkillIndex, skillOptions.ToArray());
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
                    selectedSkillIndex = EditorGUILayout.Popup("Skill (Optional)", selectedSkillIndex, skillOptions.ToArray());
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
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Please assign a SkillsTreeContainer.", MessageType.Info);
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

    private void DrawSkillPreview(Skill skill)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Selected Skill Preview", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        // Icon
        if (skill.Icon != null)
        {
            Rect iconRect = GUILayoutUtility.GetRect(32, 32, GUILayout.Width(32), GUILayout.Height(32));
            GUI.DrawTexture(iconRect, skill.Icon.texture, ScaleMode.ScaleToFit);
            GUILayout.Space(5);
        }
        
        // Info
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(skill.SkillName, EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Tier {skill.Tier} | Cost: {skill.UnlockCost} SP");
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
            
            Skill cachedSkill = trigger.GetCachedSkill();
            if (cachedSkill != null)
            {
                EditorGUILayout.LabelField($"Cached Skill: {cachedSkill.SkillName}");
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