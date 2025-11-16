using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(DialogueController))]
public class DialogueControllerEditor : Editor
{
    private SerializedProperty setupModeProp;
    private SerializedProperty dialogueContainerProp;
    private SerializedProperty dialogueGroupProp;
    private SerializedProperty dialogueProp;
    private SerializedProperty emoteAnimatorProp;
    private SerializedProperty groupedDialoguesProp;
    private SerializedProperty startingDialoguesOnlyProp;
    private SerializedProperty selectedDialogueGroupIndexProp;
    private SerializedProperty selectedDialogueIndexProp;
    private SerializedProperty dialogueManagerProp;
    private SerializedProperty screenSpaceDialogueUIProp;
    private SerializedProperty worldSpaceDialogueUIProp;
    private SerializedProperty screenSpaceTypewriterProp;
    private SerializedProperty worldSpaceTypewriterProp;
    private SerializedProperty initializeOnStartProp;
    private SerializedProperty startDelayProp;
    private SerializedProperty allowCloseWithKeyProp;
    private SerializedProperty closeKeyProp;
    private SerializedProperty playerObjectProp;
    private SerializedProperty disablePlayerWhenOpenProp;

    private int selectedGroupIndex = 0;
    private int selectedDialogueIndex = 0;
    private bool showAllDialogues = false;
    private Vector2 dialoguesScrollPosition;

    private void OnEnable()
    {
        setupModeProp = serializedObject.FindProperty("_setupMode");
        dialogueContainerProp = serializedObject.FindProperty("dialogueContainer");
        dialogueGroupProp = serializedObject.FindProperty("dialogueGroup");
        dialogueProp = serializedObject.FindProperty("dialogue");
        emoteAnimatorProp = serializedObject.FindProperty("emoteAnimator");
        groupedDialoguesProp = serializedObject.FindProperty("groupedDialogues");
        startingDialoguesOnlyProp = serializedObject.FindProperty("startingDialoguesOnly");
        selectedDialogueGroupIndexProp = serializedObject.FindProperty("selectedDialogueGroupIndex");
        selectedDialogueIndexProp = serializedObject.FindProperty("selectedDialogueIndex");
        dialogueManagerProp = serializedObject.FindProperty("dialogueManager");
        screenSpaceDialogueUIProp = serializedObject.FindProperty("screenSpaceDialogueUI");
        worldSpaceDialogueUIProp = serializedObject.FindProperty("worldSpaceDialogueUI");
        screenSpaceTypewriterProp = serializedObject.FindProperty("screenSpaceTypewriter");
        worldSpaceTypewriterProp = serializedObject.FindProperty("worldSpaceTypewriter");
        initializeOnStartProp = serializedObject.FindProperty("initializeOnStart");
        startDelayProp = serializedObject.FindProperty("startDelay");
        allowCloseWithKeyProp = serializedObject.FindProperty("_allowCloseWithKey");
        closeKeyProp = serializedObject.FindProperty("_closeKey");
        playerObjectProp = serializedObject.FindProperty("_playerObject");
        disablePlayerWhenOpenProp = serializedObject.FindProperty("_disablePlayerWhenOpen");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawSectionHeader("Dialogue Controller", 16);
        EditorGUILayout.Space(5);

        DrawSectionHeader("Setup Mode");
        EditorGUILayout.PropertyField(setupModeProp, new GUIContent("Setup Mode"));
        
        DialogueSetupMode currentMode = (DialogueSetupMode)setupModeProp.enumValueIndex;
        
        if (currentMode == DialogueSetupMode.ScreenSpace)
        {
            EditorGUILayout.HelpBox(
                "Screen Space: Dialogue UI appears as an overlay on the screen.",
                MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox(
                "World Space: Dialogue UI appears in world space (e.g., above NPC heads).",
                MessageType.Info);
        }
        
        EditorGUILayout.Space(10);

        DrawSectionHeader("Dialogue Data");
        EditorGUILayout.PropertyField(dialogueContainerProp);

        if (dialogueContainerProp.objectReferenceValue != null)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Open Dialogue Graph", GUILayout.Height(25), GUILayout.Width(200)))
            {
                OpenDialogueGraph();
            }
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
            
            DrawContainerContents();
        }
        else
        {
            EditorGUILayout.HelpBox("Select a Dialogue Container to continue.", MessageType.Info);
        }

        EditorGUILayout.Space(10);

        DrawSectionHeader("Emote Animator");
        EditorGUILayout.PropertyField(emoteAnimatorProp, new GUIContent("Animator"));
        EditorGUILayout.HelpBox(
            "Optional: Animator for character emotes during dialogue (especially for Ink nodes).",
            MessageType.Info);

        EditorGUILayout.Space(10);

        DrawSectionHeader("Dialogue Selection");
        EditorGUILayout.PropertyField(groupedDialoguesProp, new GUIContent("Use Grouped Dialogues"));
        EditorGUILayout.PropertyField(startingDialoguesOnlyProp, new GUIContent("Starting Dialogues Only"));
        
        EditorGUILayout.HelpBox(
            "Dialogue Selection Options:\n" +
            "• Use Grouped Dialogues: Load from a specific group\n" +
            "• Starting Dialogues Only: Only include dialogues marked as starting dialogues",
            MessageType.Info);

        EditorGUILayout.Space(10);

        DrawSectionHeader("System References");
        EditorGUILayout.PropertyField(dialogueManagerProp);
        
        EditorGUILayout.Space(5);
        
        // Screen Space UI References
        EditorGUILayout.LabelField("Screen Space UI", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(screenSpaceDialogueUIProp, new GUIContent("Dialogue UI"));
        EditorGUILayout.PropertyField(screenSpaceTypewriterProp, new GUIContent("Typewriter Effect"));
        EditorGUI.indentLevel--;
        
        EditorGUILayout.Space(3);
        
        // World Space UI References
        EditorGUILayout.LabelField("World Space UI", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(worldSpaceDialogueUIProp, new GUIContent("Dialogue UI"));
        EditorGUILayout.PropertyField(worldSpaceTypewriterProp, new GUIContent("Typewriter Effect"));
        EditorGUI.indentLevel--;
        
        // Validation warnings
        EditorGUILayout.Space(5);
        ValidateUIReferences(currentMode);

        EditorGUILayout.Space(10);

        DrawSectionHeader("Player Control");
        EditorGUILayout.PropertyField(disablePlayerWhenOpenProp, new GUIContent("Disable Player When Open"));
        
        if (disablePlayerWhenOpenProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(playerObjectProp, new GUIContent("Player GameObject"));
            
            if (playerObjectProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(
                    "Player GameObject not assigned. Will try to find by 'Player' tag at runtime.",
                    MessageType.Warning);
            }
            else
            {
                GameObject playerObj = playerObjectProp.objectReferenceValue as GameObject;
                IPlayerController controller = playerObj?.GetComponent<IPlayerController>();
                
                if (controller == null)
                {
                    EditorGUILayout.HelpBox(
                        "⚠️ Player GameObject doesn't have IPlayerController interface!\n" +
                        "Make sure your player script implements IPlayerController.",
                        MessageType.Error);
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "✓ Player implements IPlayerController - player control will work!",
                        MessageType.Info);
                }
            }
            
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        DrawSectionHeader("Close Button Settings");
        EditorGUILayout.PropertyField(allowCloseWithKeyProp, new GUIContent("Allow Close with Key"));
        
        if (allowCloseWithKeyProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(closeKeyProp, new GUIContent("Close Key"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        DrawSectionHeader("Auto Initialize Settings");
        EditorGUILayout.PropertyField(initializeOnStartProp, new GUIContent("Initialize On Start"));
        
        if (initializeOnStartProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(startDelayProp);
            EditorGUI.indentLevel--;
            
            EditorGUILayout.HelpBox(
                "⚠️ WARNING: Initialize On Start will auto-start dialogue when the scene loads!\n" +
                "This is usually used for cutscenes or tutorials. For normal NPC dialogues, " +
                "leave this OFF and use DialogueTrigger instead.",
                MessageType.Warning);
        }

        EditorGUILayout.Space(10);

        DrawUtilityButtons();

        serializedObject.ApplyModifiedProperties();
    }

    private void ValidateUIReferences(DialogueSetupMode currentMode)
    {
        bool hasScreenSpaceUI = screenSpaceDialogueUIProp.objectReferenceValue != null;
        bool hasWorldSpaceUI = worldSpaceDialogueUIProp.objectReferenceValue != null;
        bool hasScreenSpaceTypewriter = screenSpaceTypewriterProp.objectReferenceValue != null;
        bool hasWorldSpaceTypewriter = worldSpaceTypewriterProp.objectReferenceValue != null;

        if (currentMode == DialogueSetupMode.ScreenSpace)
        {
            if (!hasScreenSpaceUI)
            {
                EditorGUILayout.HelpBox(
                    "⚠️ Screen Space mode is active but no Screen Space UI is assigned!\n" +
                    "The system will search for one at runtime.",
                    MessageType.Warning);
            }
            else if (!hasScreenSpaceTypewriter)
            {
                EditorGUILayout.HelpBox(
                    "⚠️ Screen Space UI assigned but no Typewriter Effect!\n" +
                    "The system will search for one at runtime.",
                    MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "✓ Screen Space UI properly configured!",
                    MessageType.Info);
            }
        }
        else // WorldSpace
        {
            if (!hasWorldSpaceUI)
            {
                EditorGUILayout.HelpBox(
                    "⚠️ World Space mode is active but no World Space UI is assigned!\n" +
                    "The system will search for one at runtime.",
                    MessageType.Warning);
            }
            else if (!hasWorldSpaceTypewriter)
            {
                EditorGUILayout.HelpBox(
                    "⚠️ World Space UI assigned but no Typewriter Effect!\n" +
                    "The system will search for one at runtime.",
                    MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "✓ World Space UI properly configured!",
                    MessageType.Info);
            }
        }
    }

    private void DrawContainerContents()
    {
        DialogueContainer container = dialogueContainerProp.objectReferenceValue as DialogueContainer;
        if (container == null) return;
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Container Info", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Name:", container.FileName);
        EditorGUILayout.LabelField("Groups:", container.HaveGroups() ? container.GetGroupsNames().Length.ToString() : "0");
        EditorGUILayout.LabelField("Ungrouped Dialogues:", container.GetUngroupedDialoguesNames(false).Count.ToString());
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        if (container.HaveGroups())
        {
            DrawSectionHeader("Select Group for Preview");
            
            string[] groupNames = container.GetGroupsNames();
            
            if (selectedGroupIndex >= groupNames.Length)
                selectedGroupIndex = 0;
            
            int newGroupIndex = EditorGUILayout.Popup("Group", selectedGroupIndex, groupNames);
            
            if (newGroupIndex != selectedGroupIndex)
            {
                selectedGroupIndex = newGroupIndex;
                selectedDialogueIndex = 0;
            }
            
            string selectedGroupName = groupNames[selectedGroupIndex];
            
            DialogueGroup selectedGroup = FindDialogueGroup(container, selectedGroupName);
            
            if (selectedGroup != null)
            {
                dialogueGroupProp.objectReferenceValue = selectedGroup;
                selectedDialogueGroupIndexProp.intValue = selectedGroupIndex;
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"Group: {selectedGroup.GroupName}", EditorStyles.boldLabel);
                
                List<string> groupDialogues = container.GetGroupedDialoguesNames(selectedGroup, false);
                EditorGUILayout.LabelField("Total Dialogues:", groupDialogues.Count.ToString());
                
                List<string> startingDialogues = container.GetGroupedDialoguesNames(selectedGroup, true);
                EditorGUILayout.LabelField("Starting Dialogues:", startingDialogues.Count.ToString());
                
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(5);
                
                if (groupDialogues.Count > 0)
                {
                    DrawDialogueSelection(container, selectedGroup, groupDialogues);
                }
                else
                {
                    EditorGUILayout.HelpBox("No dialogues in this group.", MessageType.Info);
                }
            }
        }
        else
        {
            DrawSectionHeader("Ungrouped Dialogues");
            
            List<string> ungroupedDialogues = container.GetUngroupedDialoguesNames(false);
            
            if (ungroupedDialogues.Count > 0)
            {
                DrawDialogueSelection(container, null, ungroupedDialogues);
            }
            else
            {
                EditorGUILayout.HelpBox("No ungrouped dialogues in this container.", MessageType.Info);
            }
        }
    }

    private DialogueGroup FindDialogueGroup(DialogueContainer container, string groupName)
    {
        var groupsField = typeof(DialogueContainer).GetField("_groups", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (groupsField != null)
        {
            var groups = groupsField.GetValue(container) as SerializableDictionary<DialogueGroup, List<Dialogue>>;
            if (groups != null)
            {
                foreach (var kvp in groups)
                {
                    if (kvp.Key.GroupName == groupName)
                    {
                        return kvp.Key;
                    }
                }
            }
        }
        
        return null;
    }
    
    private void DrawDialogueSelection(DialogueContainer container, DialogueGroup group, List<string> dialogueNames)
    {
        DrawSectionHeader("Select Dialogue for Preview");
        
        if (selectedDialogueIndex >= dialogueNames.Count)
            selectedDialogueIndex = 0;
        
        selectedDialogueIndex = EditorGUILayout.Popup("Dialogue", selectedDialogueIndex, dialogueNames.ToArray());
        
        string selectedDialogueName = dialogueNames[selectedDialogueIndex];
        Dialogue selectedDialogue = container.GetDialogueByName(selectedDialogueName);
        
        if (selectedDialogue != null)
        {
            dialogueProp.objectReferenceValue = selectedDialogue;
            selectedDialogueIndexProp.intValue = selectedDialogueIndex;
            
            DrawDialoguePreview(selectedDialogue);
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Select Dialogue Asset", GUILayout.Height(25)))
            {
                Selection.activeObject = selectedDialogue;
                EditorGUIUtility.PingObject(selectedDialogue);
            }
            
            if (GUILayout.Button("Edit in Inspector", GUILayout.Height(25)))
            {
                Selection.activeObject = selectedDialogue;
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
        }
        
        showAllDialogues = EditorGUILayout.Foldout(showAllDialogues, $"Show All Dialogues ({dialogueNames.Count})", true);
        
        if (showAllDialogues)
        {
            DrawAllDialoguesList(container, dialogueNames);
        }
    }
    
    private void DrawDialoguePreview(Dialogue dialogue)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField(dialogue.Name, EditorStyles.boldLabel);
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Type:", GUILayout.Width(100));
        EditorGUILayout.LabelField(dialogue.Type.ToString(), EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Starting Dialogue:", GUILayout.Width(100));
        EditorGUILayout.LabelField(dialogue.IsStartingDialogue ? "✓ Yes" : "✗ No");
        EditorGUILayout.EndHorizontal();
        
        if (dialogue.Type == DialogueType.SingleChoice || dialogue.Type == DialogueType.MultipleChoice)
        {
            if (!string.IsNullOrEmpty(dialogue.Text))
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Text:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(dialogue.Text, EditorStyles.wordWrappedLabel);
            }
            
            if (!string.IsNullOrEmpty(dialogue.Character.name))
            {
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField($"Speaker: {dialogue.Character.name}");
            }
        }
        
        if (dialogue.Type == DialogueType.Ink)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Ink Settings:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Asset: {(dialogue.InkJsonAsset != null ? dialogue.InkJsonAsset.name : "None")}");
            if (!string.IsNullOrEmpty(dialogue.KnotName))
            {
                EditorGUILayout.LabelField($"Knot: {dialogue.KnotName}");
            }
        }
        
        EditorGUILayout.Space(5);
        
        if (dialogue.Choices != null && dialogue.Choices.Count > 0)
        {
            EditorGUILayout.LabelField($"Choices: {dialogue.Choices.Count}");
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawAllDialoguesList(DialogueContainer container, List<string> dialogueNames)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        dialoguesScrollPosition = EditorGUILayout.BeginScrollView(dialoguesScrollPosition, GUILayout.MaxHeight(300));
        
        for (int i = 0; i < dialogueNames.Count; i++)
        {
            EditorGUILayout.BeginHorizontal(i % 2 == 0 ? EditorStyles.helpBox : GUIStyle.none);
            
            EditorGUILayout.LabelField((i + 1).ToString(), GUILayout.Width(30));
            
            if (GUILayout.Button(dialogueNames[i], EditorStyles.label))
            {
                selectedDialogueIndex = i;
                
                Dialogue dialogue = container.GetDialogueByName(dialogueNames[i]);
                if (dialogue != null)
                {
                    Selection.activeObject = dialogue;
                    EditorGUIUtility.PingObject(dialogue);
                }
            }
            
            Dialogue loadedDialogue = container.GetDialogueByName(dialogueNames[i]);
            if (loadedDialogue != null)
            {
                EditorGUILayout.LabelField(loadedDialogue.Type.ToString(), GUILayout.Width(100));
                
                if (loadedDialogue.IsStartingDialogue)
                {
                    EditorGUILayout.LabelField("⭐", GUILayout.Width(20));
                }
            }
            
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                selectedDialogueIndex = i;
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawUtilityButtons()
    {
        DrawSectionHeader("Utilities");
        
        DialogueController controller = (DialogueController)target;
        
        if (Application.isPlaying)
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Start Dialogue", GUILayout.Height(30)))
            {
                controller.StartDialogue();
                Debug.Log("Dialogue started!");
            }
            
            if (GUILayout.Button("End Dialogue", GUILayout.Height(30)))
            {
                controller.EndDialogue();
                Debug.Log("Dialogue ended!");
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Runtime Status", EditorStyles.boldLabel);
            
            bool isActive = controller.IsDialogueActive();
            bool isOpen = controller.IsDialogueOpen;
            
            GUIStyle statusStyle = new GUIStyle(EditorStyles.label);
            statusStyle.normal.textColor = isActive ? Color.green : Color.gray;
            EditorGUILayout.LabelField($"Dialogue Active: {isActive}", statusStyle);
            
            statusStyle.normal.textColor = isOpen ? Color.green : Color.gray;
            EditorGUILayout.LabelField($"UI Open: {isOpen}", statusStyle);
            
            DialogueSetupMode mode = controller.GetSetupMode();
            EditorGUILayout.LabelField($"Active Mode: {mode}");
            
            Dialogue currentDialogue = controller.GetDialogue();
            if (currentDialogue != null)
            {
                EditorGUILayout.LabelField($"Current: {currentDialogue.Name}");
            }
            
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox(
                "In Play mode, you can manually start/stop dialogue for testing.",
                MessageType.Info);
            
            bool hasManager = dialogueManagerProp.objectReferenceValue != null;
            bool hasScreenSpaceUI = screenSpaceDialogueUIProp.objectReferenceValue != null;
            bool hasWorldSpaceUI = worldSpaceDialogueUIProp.objectReferenceValue != null;
            bool hasContainer = dialogueContainerProp.objectReferenceValue != null;
            
            DialogueSetupMode currentMode = (DialogueSetupMode)setupModeProp.enumValueIndex;
            bool hasActiveUI = currentMode == DialogueSetupMode.ScreenSpace ? hasScreenSpaceUI : hasWorldSpaceUI;
            
            if (!hasManager || !hasActiveUI)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("⚠️ Setup Warnings:", EditorStyles.boldLabel);
                
                if (!hasManager)
                {
                    EditorGUILayout.LabelField("• DialogueManager not assigned - will search at runtime");
                }
                if (!hasActiveUI)
                {
                    EditorGUILayout.LabelField($"• {currentMode} DialogueUI not assigned - will search at runtime");
                }
                
                EditorGUILayout.EndVertical();
            }
            else if (hasContainer)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("✓ Setup Complete!", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Ready to use in {currentMode} mode");
                EditorGUILayout.EndVertical();
            }
        }
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

    private void OpenDialogueGraph()
    {
        DialogueContainer container = dialogueContainerProp.objectReferenceValue as DialogueContainer;
        
        if (container == null)
        {
            Debug.LogWarning("No Dialogue Container assigned!");
            return;
        }

        string containerPath = AssetDatabase.GetAssetPath(container);
        
        if (string.IsNullOrEmpty(containerPath))
        {
            Debug.LogWarning("Could not find asset path for Dialogue Container!");
            return;
        }

        try
        {
            var windowType = System.Type.GetType("DialogueSystemEditorWindow");
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
            Debug.LogWarning($"Could not open Dialogue Graph: {e.Message}");
            Selection.activeObject = container;
            EditorGUIUtility.PingObject(container);
        }
    }
}