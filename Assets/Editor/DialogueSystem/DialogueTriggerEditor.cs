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
    private SerializedProperty dialogueControllerProp;
    private SerializedProperty dialogueContainerProp;
    private SerializedProperty selectedGroupNameProp;
    private SerializedProperty selectedDialogueNameProp;
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
    private int selectedDialogueIndex = 0;
    private string[] groupNames;
    private string[] dialogueNames;

    private void OnEnable()
    {
        visualCueProp = serializedObject.FindProperty("visualCue");
        emoteAnimatorProp = serializedObject.FindProperty("emoteAnimator");
        dialogueControllerProp = serializedObject.FindProperty("dialogueController");
        dialogueContainerProp = serializedObject.FindProperty("dialogueContainer");
        selectedGroupNameProp = serializedObject.FindProperty("selectedGroupName");
        selectedDialogueNameProp = serializedObject.FindProperty("selectedDialogueName");
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

        DialogueTrigger trigger = (DialogueTrigger)target;

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

        // Trigger Settings (moved up for better workflow)
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

        // Dialogue Settings Header
        DrawDialogueSettings();

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
                "✓ AUTO TRIGGER MODE: Dialogue will start immediately when player enters.",
                MessageType.Info);
        }
        else if (requiresInput)
        {
            KeyCode key = (KeyCode)interactKeyProp.enumValueIndex;
            EditorGUILayout.HelpBox(
                $"✓ INPUT MODE: Player must press '{key}' to start dialogue.",
                MessageType.Info);
        }
    }

    private void DrawTriggerSettings()
    {
        EditorGUILayout.LabelField("Trigger Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.PropertyField(triggerOnEnterProp, new GUIContent(
            "Trigger On Enter",
            "If TRUE, starts dialogue immediately when player enters (ignores input requirement)"));
        
        EditorGUILayout.PropertyField(requiresInputProp, new GUIContent(
            "Requires Input",
            "If TRUE, requires key press to start dialogue"));
        
        if (requiresInputProp.boolValue && !triggerOnEnterProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(interactKeyProp, new GUIContent("Interact Key"));
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
        
        // Force toggle to false and disable it (dialogue must be completed)
        EditorGUI.BeginDisabledGroup(true);
        toggleWithInteractKeyProp.boolValue = false;
        EditorGUILayout.PropertyField(toggleWithInteractKeyProp, new GUIContent(
            "Toggle with Key (Disabled for Dialogue)",
            "Dialogues cannot be toggled - they must be completed"));
        EditorGUI.EndDisabledGroup();
    }

    private void DrawDialogueSettings()
    {
        EditorGUILayout.LabelField("Dialogue Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(dialogueControllerProp, new GUIContent("Controller"));
        EditorGUILayout.PropertyField(dialogueContainerProp, new GUIContent("Container"));

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
                    
                    // Get dialogues for this group (only starting dialogues)
                    dialoguesList = GetGroupDialogues(container, groupName);
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
    }

    private void DrawUtilityButtons()
    {
        EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);
        
        DialogueTrigger trigger = (DialogueTrigger)target;
        
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
            
            Dialogue cachedDialogue = trigger.GetCachedDialogue();
            if (cachedDialogue != null)
            {
                EditorGUILayout.LabelField($"Cached Dialogue: {cachedDialogue.Name}");
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