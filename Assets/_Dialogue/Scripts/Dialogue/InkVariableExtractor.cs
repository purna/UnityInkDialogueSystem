using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

public class InkVariableExtractor
{
    /// <summary>
    /// Extracts global variables from an Ink JSON file and returns them as a dictionary
    /// </summary>
    public static Dictionary<string, object> ExtractGlobalVariables(TextAsset inkJSON)
    {
        Dictionary<string, object> extractedVariables = new Dictionary<string, object>();
        
        if (inkJSON == null)
        {
            Debug.LogWarning("InkVariableExtractor: Ink JSON is null");
            return extractedVariables;
        }

        try
        {
            Story tempStory = new Story(inkJSON.text);
            
            // Iterate through all global variables in the story
            foreach (string varName in tempStory.variablesState)
            {
                object value = tempStory.variablesState[varName];
                extractedVariables[varName] = value;
                
                Debug.Log($"Extracted Ink variable: {varName} = {value} (Type: {value?.GetType().Name})");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to extract variables from Ink JSON: {e.Message}");
        }
        
        return extractedVariables;
    }
    
    /// <summary>
    /// Converts Ink variable type to DialogueSystem VariableDataType
    /// </summary>
public static VariableDataType GetVariableDataType(object value)
{
    if (value == null) return VariableDataType.String;
    
    System.Type type = value.GetType();
    
    // Ink uses Ink.Runtime.Value types
    if (value is Ink.Runtime.IntValue || type == typeof(int))
        return VariableDataType.Int;
    else if (value is Ink.Runtime.FloatValue || type == typeof(float) || type == typeof(double))
        return VariableDataType.Float;
    else if (value is Ink.Runtime.BoolValue || type == typeof(bool))
        return VariableDataType.Bool;
    else if (value is Ink.Runtime.StringValue || type == typeof(string))
        return VariableDataType.String;
    else
    {
        // Fallback: try to determine from string representation
        string stringValue = value.ToString();
        
        if (bool.TryParse(stringValue, out _))
            return VariableDataType.Bool;
        else if (int.TryParse(stringValue, out _))
            return VariableDataType.Int;
        else if (float.TryParse(stringValue, out _) && stringValue.Contains("."))
            return VariableDataType.Float;
        else
            return VariableDataType.String;
    }
}
}