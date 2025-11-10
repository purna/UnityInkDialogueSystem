using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Container for all dialogue variables
/// </summary>
[CreateAssetMenu(fileName = "DialogueVariablesContainer", menuName = "Dialogue System/Variables Container")]
public class DialogueVariablesContainer : ScriptableObject
{
    [SerializeField] private List<DialogueVariable> _variables = new();

    public List<DialogueVariable> Variables => _variables;

    public DialogueVariable GetVariable(string variableName)
    {
        return _variables.Find(v => v.VariableName == variableName);
    }

    public List<string> GetVariableNames()
    {
        List<string> names = new();
        foreach (var variable in _variables)
        {
            if (variable != null)
                names.Add(variable.VariableName);
        }
        return names;
    }

    public List<string> GetVariableNamesByType(VariableDataType type)
    {
        List<string> names = new();
        foreach (var variable in _variables)
        {
            if (variable != null && variable.Type == type)
                names.Add(variable.VariableName);
        }
        return names;
    }



    // Add this method to your DialogueVariablesContainer class
    // Updated method for DialogueVariablesContainer class
public void PopulateFromInkJSON(TextAsset inkJSON)
    {
        if (inkJSON == null) return;

        Dictionary<string, object> inkVariables = InkVariableExtractor.ExtractGlobalVariables(inkJSON);

        foreach (var kvp in inkVariables)
        {
            string varName = kvp.Key;
            object value = kvp.Value;

            // Check if variable already exists
            if (GetVariable(varName) != null)
            {
                Debug.Log($"Variable '{varName}' already exists in container. Skipping.");
                continue;
            }

            // Create new variable based on type
            VariableDataType dataType = InkVariableExtractor.GetVariableDataType(value);
            DialogueVariable newVar = ScriptableObject.CreateInstance<DialogueVariable>();

            // Initialize with the extracted data
            newVar.Initialize(varName, dataType, value, $"Imported from Ink JSON");

            _variables.Add(newVar);
            Debug.Log($"Added Ink variable to container: {varName} ({dataType}) = {value}");
        }
    }

}