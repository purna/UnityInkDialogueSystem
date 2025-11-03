using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DialogueVariablesContainer))]
public class DialogueVariablesContainerEditor : Editor
{
    private TextAsset inkJSONToParse;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        DialogueVariablesContainer container = (DialogueVariablesContainer)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Import from Ink JSON", EditorStyles.boldLabel);
        
        inkJSONToParse = (TextAsset)EditorGUILayout.ObjectField(
            "Ink JSON File", 
            inkJSONToParse, 
            typeof(TextAsset), 
            false
        );
        
        if (GUILayout.Button("Import Variables from Ink JSON"))
        {
            if (inkJSONToParse != null)
            {
                container.PopulateFromInkJSON(inkJSONToParse);
                EditorUtility.SetDirty(container);
                AssetDatabase.SaveAssets();
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "No Ink JSON Selected",
                    "Please select an Ink JSON file to import variables from.",
                    "OK"
                );
            }
        }
        
        EditorGUILayout.HelpBox(
            "This will extract all global variables from the Ink JSON file and add them to this container. Existing variables will not be overwritten.",
            MessageType.Info
        );
    }
}