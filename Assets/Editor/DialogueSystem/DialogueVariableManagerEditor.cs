#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DialogueVariableManager))]
public class DialogueVariableManagerEditor : Editor
{
    private SerializedProperty _registeredVariables;
    private SerializedProperty _inkGlobalsJSON;
    private SerializedProperty _enablePersistence;
    private SerializedProperty _saveKey;

    private bool _showRuntimeValues = false;

    private void OnEnable()
    {
        _registeredVariables = serializedObject.FindProperty("_registeredVariables");
        _inkGlobalsJSON = serializedObject.FindProperty("_inkGlobalsJSON");
        _enablePersistence = serializedObject.FindProperty("_enablePersistence");
        _saveKey = serializedObject.FindProperty("_saveKey");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DialogueVariableManager manager = (DialogueVariableManager)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Dialogue Variable Manager", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Manages all dialogue variables for both graph-based and Ink dialogue systems.", MessageType.Info);
        EditorGUILayout.Space(5);

        // Variable Configuration Section
        EditorGUILayout.LabelField("Variable Configuration", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_registeredVariables, new GUIContent("Registered Variables"), true);
        
        EditorGUILayout.Space(5);
        
        // Quick create button
        if (GUILayout.Button("Create New Variable ScriptableObject"))
        {
            CreateNewVariable();
        }

        EditorGUILayout.Space(10);

        // Ink Integration Section
        EditorGUILayout.LabelField("Ink Integration", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_inkGlobalsJSON);
        
        EditorGUILayout.Space(10);

        // Save/Load Settings Section
        EditorGUILayout.LabelField("Save/Load Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_enablePersistence);
        EditorGUILayout.PropertyField(_saveKey);

        EditorGUILayout.Space(10);

        // Runtime Tools Section
        EditorGUILayout.LabelField("Runtime Tools", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = Application.isPlaying;
        
        if (GUILayout.Button("Save Variables"))
        {
            manager.SaveVariables();
            EditorUtility.DisplayDialog("Variables Saved", "All variables have been saved to PlayerPrefs.", "OK");
        }
        
        if (GUILayout.Button("Load Variables"))
        {
            manager.LoadVariables();
            EditorUtility.DisplayDialog("Variables Loaded", "Variables have been loaded from PlayerPrefs.", "OK");
        }
        
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Clear Save Data"))
        {
            if (EditorUtility.DisplayDialog("Clear Save Data", "Are you sure you want to clear all saved variable data?", "Yes", "Cancel"))
            {
                manager.ClearSaveData();
                EditorUtility.DisplayDialog("Data Cleared", "All saved variable data has been cleared.", "OK");
            }
        }
        
        GUI.enabled = Application.isPlaying;
        if (GUILayout.Button("Reset All Variables"))
        {
            if (EditorUtility.DisplayDialog("Reset Variables", "Reset all variables to their default values?", "Yes", "Cancel"))
            {
                manager.ResetAllVariables();
            }
        }
        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();

        // Runtime Values Display (only in Play Mode)
        if (Application.isPlaying)
        {
            EditorGUILayout.Space(10);
            _showRuntimeValues = EditorGUILayout.Foldout(_showRuntimeValues, "Runtime Variable Values", true);
            
            if (_showRuntimeValues)
            {
                EditorGUI.indentLevel++;
                DrawRuntimeVariables(manager);
                EditorGUI.indentLevel--;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawRuntimeVariables(DialogueVariableManager manager)
    {
        EditorGUILayout.HelpBox("Current variable values at runtime (read-only)", MessageType.None);
        
        // Use reflection to access private dictionaries for display
        var variableValuesField = typeof(DialogueVariableManager).GetField("_variableValues", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var variableTypesField = typeof(DialogueVariableManager).GetField("_variableTypes", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (variableValuesField != null && variableTypesField != null)
        {
            var variableValues = variableValuesField.GetValue(manager) as System.Collections.IDictionary;
            var variableTypes = variableTypesField.GetValue(manager) as System.Collections.IDictionary;

            if (variableValues != null && variableValues.Count > 0)
            {
                GUI.enabled = false;
                foreach (System.Collections.DictionaryEntry entry in variableValues)
                {
                    string varName = entry.Key.ToString();
                    object value = entry.Value;
                    
                    string displayValue = value?.ToString() ?? "null";
                    string typeStr = variableTypes != null && variableTypes.Contains(varName) 
                        ? $" ({variableTypes[varName]})" 
                        : "";

                    EditorGUILayout.TextField($"{varName}{typeStr}", displayValue);
                }
                GUI.enabled = true;
            }
            else
            {
                EditorGUILayout.LabelField("No variables initialized yet", EditorStyles.miniLabel);
            }
        }
    }

    private void CreateNewVariable()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create Dialogue Variable",
            "NewVariable",
            "asset",
            "Choose a location to save the variable"
        );

        if (!string.IsNullOrEmpty(path))
        {
            DialogueVariable newVariable = CreateInstance<DialogueVariable>();
            AssetDatabase.CreateAsset(newVariable, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newVariable;

            Debug.Log($"Created new DialogueVariable at {path}");
        }
    }
}
#endif