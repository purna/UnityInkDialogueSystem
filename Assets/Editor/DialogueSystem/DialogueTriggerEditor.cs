#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(DialogueTrigger))]
public class DialogueTriggerEditor : Editor
{
    private SerializedProperty visualCueProp;
    private SerializedProperty emoteAnimatorProp;
    private SerializedProperty dialogueContainerProp;
    private SerializedProperty selectedGroupNameProp;
    private SerializedProperty selectedDialogueNameProp;
    private SerializedProperty triggerOnEnterProp;
    private SerializedProperty requiresInputProp;
    private SerializedProperty interactKeyProp;

    private int selectedGroupIndex = 0;
    private int selectedDialogueIndex = 0;
    private string[] groupNames;
    private string[] dialogueNames;

    private void OnEnable()
    {
        visualCueProp = serializedObject.FindProperty("visualCue");
        emoteAnimatorProp = serializedObject.FindProperty("emoteAnimator");
        dialogueContainerProp = serializedObject.FindProperty("dialogueContainer");
        selectedGroupNameProp = serializedObject.FindProperty("selectedGroupName");
        selectedDialogueNameProp = serializedObject.FindProperty("selectedDialogueName");
        triggerOnEnterProp = serializedObject.FindProperty("triggerOnEnter");
        requiresInputProp = serializedObject.FindProperty("requiresInput");
        interactKeyProp = serializedObject.FindProperty("interactKey");
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

        // Dialogue Settings Header
        EditorGUILayout.LabelField("Dialogue Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(dialogueContainerProp);

        DialogueContainer container = dialogueContainerProp.objectReferenceValue as DialogueContainer;

        if (container != null)
        {
            EditorGUILayout.Space(5);

            // Get group names
            if (container.HaveGroups())
            {
                groupNames = container.GetGroupsNames();
                
                // Add "Ungrouped" option
                List<string> groupOptions = new List<string> { "Ungrouped" };
                groupOptions.AddRange(groupNames);
                
                // Find current group index
                if (!string.IsNullOrEmpty(selectedGroupNameProp.stringValue))
                {
                    selectedGroupIndex = System.Array.IndexOf(groupNames, selectedGroupNameProp.stringValue) + 1;
                    if (selectedGroupIndex < 0) selectedGroupIndex = 0;
                }

                // Group dropdown
                EditorGUI.BeginChangeCheck();
                selectedGroupIndex = EditorGUILayout.Popup("Group", selectedGroupIndex, groupOptions.ToArray());
                if (EditorGUI.EndChangeCheck())
                {
                    // Update selected group name
                    if (selectedGroupIndex == 0)
                    {
                        selectedGroupNameProp.stringValue = "";
                    }
                    else
                    {
                        selectedGroupNameProp.stringValue = groupNames[selectedGroupIndex - 1];
                    }
                    
                    // Reset dialogue selection
                    selectedDialogueNameProp.stringValue = "";
                    selectedDialogueIndex = 0;
                }

                // Get dialogues for selected group
                List<string> dialoguesList;
                if (selectedGroupIndex == 0)
                {
                    // Ungrouped dialogues
                    dialoguesList = container.GetUngroupedDialoguesNames(true);
                }
                else
                {
                    // Get DialogueGroup object
                    string groupName = groupNames[selectedGroupIndex - 1];
                    DialogueGroup group = null;
                    
                    // Find the group by name (we need to get it from the container's internal structure)
                    string[] allGroupNames = container.GetGroupsNames();
                    int groupIdx = System.Array.IndexOf(allGroupNames, groupName);
                    
                    if (groupIdx >= 0)
                    {
                        // Get dialogues for this group (only starting dialogues)
                        dialoguesList = GetGroupDialogues(container, groupName);
                    }
                    else
                    {
                        dialoguesList = new List<string>();
                    }
                }

                if (dialoguesList.Count > 0)
                {
                    dialogueNames = dialoguesList.ToArray();

                    // Find current dialogue index
                    if (!string.IsNullOrEmpty(selectedDialogueNameProp.stringValue))
                    {
                        selectedDialogueIndex = System.Array.IndexOf(dialogueNames, selectedDialogueNameProp.stringValue);
                        if (selectedDialogueIndex < 0) selectedDialogueIndex = 0;
                    }

                    // Dialogue dropdown
                    EditorGUI.BeginChangeCheck();
                    selectedDialogueIndex = EditorGUILayout.Popup("Dialogue", selectedDialogueIndex, dialogueNames);
                    if (EditorGUI.EndChangeCheck())
                    {
                        selectedDialogueNameProp.stringValue = dialogueNames[selectedDialogueIndex];
                    }

                    // Auto-select first dialogue if none selected
                    if (string.IsNullOrEmpty(selectedDialogueNameProp.stringValue) && dialogueNames.Length > 0)
                    {
                        selectedDialogueNameProp.stringValue = dialogueNames[0];
                        selectedDialogueIndex = 0;
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No starting dialogues found in selected group.", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No groups found in DialogueContainer.", MessageType.Warning);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Please assign a DialogueContainer.", MessageType.Info);
        }

        EditorGUILayout.Space();

        // Trigger Settings Header
        EditorGUILayout.LabelField("Trigger Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(triggerOnEnterProp);
        EditorGUILayout.PropertyField(requiresInputProp);
        EditorGUILayout.PropertyField(interactKeyProp);

        serializedObject.ApplyModifiedProperties();
    }

    private List<string> GetGroupDialogues(DialogueContainer container, string groupName)
    {
        // This is a workaround since we can't directly access the DialogueGroup from the name
        // We'll use reflection to get the actual DialogueGroup object
        var groupsField = typeof(DialogueContainer).GetField("_groups", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (groupsField != null)
        {
            var groups = groupsField.GetValue(container) as SerializableDictionary<DialogueGroup, List<Dialogue>>;
            if (groups != null)
            {
                foreach (var kvp in groups)
                {
                    if (kvp.Key.name == groupName)
                    {
                        return container.GetGroupedDialoguesNames(kvp.Key, true);
                    }
                }
            }
        }
        
        return new List<string>();
    }
}
#endif