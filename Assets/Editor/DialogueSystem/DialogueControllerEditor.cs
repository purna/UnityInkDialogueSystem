using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogueController))]
public class DialogueControllerEditor : Editor
{
    private SerializedProperty dialogueContainerProp;
    private SerializedProperty dialogueGroupProp;
    private SerializedProperty dialogueProp;
    private SerializedProperty groupedDialoguesProp;
    private SerializedProperty startingDialoguesOnlyProp;
    private SerializedProperty selectedDialogueGroupIndexProp;
    private SerializedProperty selectedDialogueIndexProp;
    private SerializedProperty dialogueManagerProp;
    private SerializedProperty dialogueUIProp;
    private SerializedProperty startDialogueOnStartProp;
    private SerializedProperty startDelayProp;

    private void OnEnable()
    {
        dialogueContainerProp = serializedObject.FindProperty("dialogueContainer");
        dialogueGroupProp = serializedObject.FindProperty("dialogueGroup");
        dialogueProp = serializedObject.FindProperty("dialogue");
        groupedDialoguesProp = serializedObject.FindProperty("groupedDialogues");
        startingDialoguesOnlyProp = serializedObject.FindProperty("startingDialoguesOnly");
        selectedDialogueGroupIndexProp = serializedObject.FindProperty("selectedDialogueGroupIndex");
        selectedDialogueIndexProp = serializedObject.FindProperty("selectedDialogueIndex");
        dialogueManagerProp = serializedObject.FindProperty("dialogueManager");
        dialogueUIProp = serializedObject.FindProperty("dialogueUI");
        startDialogueOnStartProp = serializedObject.FindProperty("startDialogueOnStart");
        startDelayProp = serializedObject.FindProperty("startDelay");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Header
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Dialogue Controller", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // Dialogue Data Section
        EditorGUILayout.LabelField("Dialogue Data", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(dialogueContainerProp);

        // Open Dialogue Graph Button
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
        }

        EditorGUILayout.PropertyField(dialogueGroupProp);
        EditorGUILayout.PropertyField(dialogueProp);

        EditorGUILayout.Space(10);

        // Dialogue Selection Section
        EditorGUILayout.LabelField("Dialogue Selection", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(groupedDialoguesProp);
        EditorGUILayout.PropertyField(startingDialoguesOnlyProp);
        EditorGUILayout.PropertyField(selectedDialogueGroupIndexProp);
        EditorGUILayout.PropertyField(selectedDialogueIndexProp);

        EditorGUILayout.Space(10);

        // System References Section
        EditorGUILayout.LabelField("System References", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(dialogueManagerProp);
        EditorGUILayout.PropertyField(dialogueUIProp);

        EditorGUILayout.Space(10);

        // Auto Start Settings Section
        EditorGUILayout.LabelField("Auto Start Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(startDialogueOnStartProp);
        
        if (startDialogueOnStartProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(startDelayProp);
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void OpenDialogueGraph()
    {
        DialogueContainer container = dialogueContainerProp.objectReferenceValue as DialogueContainer;
        
        if (container == null)
        {
            Debug.LogWarning("No Dialogue Container assigned!");
            return;
        }

        // Get the asset path of the container
        string containerPath = AssetDatabase.GetAssetPath(container);
        
        if (string.IsNullOrEmpty(containerPath))
        {
            Debug.LogWarning("Could not find asset path for Dialogue Container!");
            return;
        }

        // Try to find the DialogueSystemGraphView window
        // This assumes you have a method to open the graph window with a specific container
        OpenDialogueGraphWindow(container);
    }

    private void OpenDialogueGraphWindow(DialogueContainer container)
    {
        // Open the window with the container
        DialogueSystemEditorWindow.OpenWithContainer(container);
    }
}