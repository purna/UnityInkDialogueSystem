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
    private SerializedProperty npcTransformProp;
    private SerializedProperty dialoguePositionProp;
    private SerializedProperty horizontalOffsetProp;
    private SerializedProperty verticalOffsetProp;
    private SerializedProperty screenEdgeMarginProp;
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
    
    // New prompt positioning properties
    private SerializedProperty promptLeftOffsetProp;
    private SerializedProperty promptRightOffsetProp;
    private SerializedProperty promptAboveOffsetProp;
    private SerializedProperty promptBelowOffsetProp;
    private SerializedProperty promptVerticalOffsetProp;
    private SerializedProperty continuousPositionUpdateProp;

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
        npcTransformProp = serializedObject.FindProperty("npcTransform");
        dialoguePositionProp = serializedObject.FindProperty("dialoguePosition");
        horizontalOffsetProp = serializedObject.FindProperty("horizontalOffset");
        verticalOffsetProp = serializedObject.FindProperty("verticalOffset");
        screenEdgeMarginProp = serializedObject.FindProperty("screenEdgeMargin");
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
        
        // New prompt positioning properties
        promptLeftOffsetProp = serializedObject.FindProperty("promptLeftOffset");
        promptRightOffsetProp = serializedObject.FindProperty("promptRightOffset");
        promptAboveOffsetProp = serializedObject.FindProperty("promptAboveOffset");
        promptBelowOffsetProp = serializedObject.FindProperty("promptBelowOffset");
        promptVerticalOffsetProp = serializedObject.FindProperty("promptVerticalOffset");
        continuousPositionUpdateProp = serializedObject.FindProperty("continuousPositionUpdate");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DialogueTrigger trigger = (DialogueTrigger)target;

        // Configuration Warning Box at top
        DrawConfigurationWarning();
        EditorGUILayout.Space(5);

        // Visual Cue Header
        DrawSectionHeader("Visual Cue");
        EditorGUILayout.PropertyField(visualCueProp, new GUIContent("Visual Cue GameObject"));
        EditorGUILayout.Space();

        // Emote Animator Header
        DrawSectionHeader("Emote Animator");
        EditorGUILayout.PropertyField(emoteAnimatorProp, new GUIContent("Animator (Optional)"));
        EditorGUILayout.Space();

        // World Space Settings
        DrawWorldSpaceSettings();
        EditorGUILayout.Space();

        // Trigger Settings (moved up for better workflow)
        DrawTriggerSettings();
        EditorGUILayout.Space();

        // UI Prompt Section
        if (requiresInputProp.boolValue && !triggerOnEnterProp.boolValue)
        {
            DrawUIPromptSection();
            EditorGUILayout.Space();
        }

        // Dialogue Settings Header
        DrawDialogueSettings();

        EditorGUILayout.Space();

        // Utility Buttons
        DrawUtilityButtons();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSectionHeader(string title)
    {
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        rect.height = 1;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        EditorGUILayout.Space(3);
    }

    private void DrawWorldSpaceSettings()
    {
        DrawSectionHeader("World Space Settings");
        
        // Check if DialogueController is in WorldSpace mode
        DialogueController controller = dialogueControllerProp.objectReferenceValue as DialogueController;
        bool isWorldSpaceMode = controller != null && controller.GetSetupMode() == DialogueSetupMode.WorldSpace;
        
        if (isWorldSpaceMode)
        {
            EditorGUILayout.HelpBox(
                "✓ DialogueController is in WorldSpace mode.\n" +
                "Configure NPC attachment and positioning below.",
                MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox(
                "DialogueController is in ScreenSpace mode.\n" +
                "These settings only apply in WorldSpace mode.",
                MessageType.None);
        }
        
        EditorGUILayout.Space(3);
        
        // NPC Transform
        EditorGUILayout.PropertyField(npcTransformProp, new GUIContent(
            "NPC Transform",
            "Optional: NPC to attach WorldSpace dialogue to. If not set, uses this trigger's transform."));
        
        if (npcTransformProp.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox(
                "ℹ No NPC Transform specified. WorldSpace dialogue will attach to this trigger's transform.",
                MessageType.Info);
        }
        
        EditorGUILayout.Space(5);
        
        // Position Mode
        EditorGUILayout.PropertyField(dialoguePositionProp, new GUIContent(
            "Position Mode",
            "Where to place the dialogue relative to the NPC. Auto will choose the best position based on screen edges."));
        
        WorldSpaceDialoguePosition currentPosition = (WorldSpaceDialoguePosition)dialoguePositionProp.enumValueIndex;
        
        // Position mode info
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        switch (currentPosition)
        {
            case WorldSpaceDialoguePosition.Auto:
                EditorGUILayout.LabelField("🤖 Auto Mode", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(
                    "• Automatically chooses best position\n" +
                    "• Avoids screen edges\n" +
                    "• Places dialogue where it's most visible", 
                    EditorStyles.wordWrappedMiniLabel);
                break;
            case WorldSpaceDialoguePosition.Above:
                EditorGUILayout.LabelField("⬆️ Above", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Dialogue appears above NPC (overridden if near top edge)", EditorStyles.wordWrappedMiniLabel);
                break;
            case WorldSpaceDialoguePosition.Below:
                EditorGUILayout.LabelField("⬇️ Below", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Dialogue appears below NPC (overridden if near bottom edge)", EditorStyles.wordWrappedMiniLabel);
                break;
            case WorldSpaceDialoguePosition.Left:
                EditorGUILayout.LabelField("⬅️ Left", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Dialogue appears to the left of NPC (overridden if near left edge)", EditorStyles.wordWrappedMiniLabel);
                break;
            case WorldSpaceDialoguePosition.Right:
                EditorGUILayout.LabelField("➡️ Right", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Dialogue appears to the right of NPC (overridden if near right edge)", EditorStyles.wordWrappedMiniLabel);
                break;
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        // Offset Controls
        EditorGUILayout.LabelField("Dialogue Offset Distance", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;
        
        EditorGUILayout.PropertyField(horizontalOffsetProp, new GUIContent(
            "Horizontal Offset",
            "Distance from NPC when positioned Left or Right"));
        
        EditorGUILayout.PropertyField(verticalOffsetProp, new GUIContent(
            "Vertical Offset",
            "Distance from NPC when positioned Above or Below"));
        
        EditorGUI.indentLevel--;
        
        EditorGUILayout.Space(5);
        
        // Edge Margin
        EditorGUILayout.PropertyField(screenEdgeMarginProp, new GUIContent(
            "Screen Edge Margin",
            "How close to screen edges before overriding position (0-0.5, percentage of screen)"));
        
        // Visual preview of current settings
        if (isWorldSpaceMode)
        {
            EditorGUILayout.Space(3);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("📍 Current Configuration", EditorStyles.boldLabel);
            
            Transform npc = npcTransformProp.objectReferenceValue as Transform;
            string npcName = npc != null ? npc.name : "[Trigger Transform]";
            
            EditorGUILayout.LabelField($"Target: {npcName}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Position: {currentPosition}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"H-Offset: {horizontalOffsetProp.floatValue:F2} | V-Offset: {verticalOffsetProp.floatValue:F2}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Edge Margin: {screenEdgeMarginProp.floatValue:P0}", EditorStyles.miniLabel);
            
            EditorGUILayout.EndVertical();
        }
    }

    private void DrawUIPromptSection()
    {
        DrawSectionHeader("UI Prompt");
        
        EditorGUILayout.PropertyField(interactPromptProp, new GUIContent("Prompt GameObject"));
        EditorGUILayout.PropertyField(promptTextComponentProp, new GUIContent("Text Component"));
        EditorGUILayout.PropertyField(promptTextProp, new GUIContent("Prompt Message"));
        
        EditorGUILayout.Space(5);
        
        // Fade Animation
        EditorGUILayout.LabelField("Fade Animation", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(fadeInDurationProp, new GUIContent("Fade In Duration"));
        EditorGUILayout.PropertyField(displayDurationProp, new GUIContent("Display Duration"));
        EditorGUILayout.PropertyField(fadeOutDurationProp, new GUIContent("Fade Out Duration"));
        EditorGUILayout.PropertyField(loopPromptProp, new GUIContent("Loop Animation"));
        EditorGUI.indentLevel--;
        
        EditorGUILayout.Space(5);
        
        // Dynamic Prompt Positioning
        EditorGUILayout.LabelField("Dynamic Positioning", EditorStyles.miniBoldLabel);
        
        EditorGUILayout.HelpBox(
            "Prompt automatically positions itself on the side of the trigger furthest from screen edges.\n" +
            "Configure the offset distances for each position below.",
            MessageType.Info);
        
        EditorGUI.indentLevel++;
        
        EditorGUILayout.PropertyField(promptLeftOffsetProp, new GUIContent(
            "Left Offset",
            "Horizontal offset when prompt is positioned to the left (negative value)"));
        
        EditorGUILayout.PropertyField(promptRightOffsetProp, new GUIContent(
            "Right Offset",
            "Horizontal offset when prompt is positioned to the right (positive value)"));
        
        EditorGUILayout.PropertyField(promptAboveOffsetProp, new GUIContent(
            "Above Offset",
            "Vertical offset when prompt is positioned above (positive value)"));
        
        EditorGUILayout.PropertyField(promptBelowOffsetProp, new GUIContent(
            "Below Offset",
            "Vertical offset when prompt is positioned below (negative value)"));
        
        EditorGUILayout.PropertyField(promptVerticalOffsetProp, new GUIContent(
            "Additional Vertical Offset",
            "Extra vertical offset applied to all positions"));
        
        EditorGUILayout.Space(3);
        
        EditorGUILayout.PropertyField(continuousPositionUpdateProp, new GUIContent(
            "Continuous Update",
            "Update prompt position every frame (enable for moving triggers)"));
        
        EditorGUI.indentLevel--;
        
        // Preview current settings
        EditorGUILayout.Space(3);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("📍 Prompt Offset Configuration", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Left: {promptLeftOffsetProp.floatValue:F2} | Right: {promptRightOffsetProp.floatValue:F2}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Above: {promptAboveOffsetProp.floatValue:F2} | Below: {promptBelowOffsetProp.floatValue:F2}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Extra Vertical: {promptVerticalOffsetProp.floatValue:F2}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Continuous Update: {(continuousPositionUpdateProp.boolValue ? "Enabled" : "Disabled")}", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();
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
        DrawSectionHeader("Trigger Settings");
        
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
        
        EditorGUILayout.PropertyField(toggleWithInteractKeyProp, new GUIContent(
            "Toggle with Key",
            "If TRUE, pressing interact key again will close the dialogue"));
    }

    private void DrawDialogueSettings()
    {
        DrawSectionHeader("Dialogue Settings");
        EditorGUILayout.PropertyField(dialogueControllerProp, new GUIContent("Controller"));
        
        // Show current mode
        DialogueController controller = dialogueControllerProp.objectReferenceValue as DialogueController;
        if (controller != null)
        {
            DialogueSetupMode mode = controller.GetSetupMode();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Controller Mode: {mode}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3);
        }
        
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
        DrawSectionHeader("Utilities");
        
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
            
            // Show NPC attachment info
            if (npcTransformProp.objectReferenceValue != null)
            {
                Transform npc = npcTransformProp.objectReferenceValue as Transform;
                EditorGUILayout.LabelField($"NPC Transform: {npc.name}");
            }
            else
            {
                EditorGUILayout.LabelField("NPC Transform: Using trigger transform");
            }
            
            EditorGUILayout.LabelField($"Position Mode: {trigger.GetDialoguePosition()}");
            
            EditorGUILayout.EndVertical();
        }
        else
        {
            // Editor Setup
            EditorGUILayout.HelpBox(
                "✓ Trigger is ready! Make sure to:\n" +
                "• Add a 2D Collider with 'Is Trigger' enabled\n" +
                "• Set the collider size to your desired trigger area\n" +
                "• Tag your player GameObject as 'Player'\n" +
                "• (Optional) Assign NPC Transform for WorldSpace mode\n" +
                "• (Optional) Configure positioning for WorldSpace dialogue",
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