using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

/// <summary>
/// Manages Ink-specific variables and syncs with DialogueVariableManager
/// </summary>
public class DialogueInkVariables
{
    public Dictionary<string, Ink.Runtime.Object> variables { get; private set; }

    private Story globalVariablesStory;

    public DialogueInkVariables(TextAsset loadGlobalsJSON = null)
    {
        variables = new Dictionary<string, Ink.Runtime.Object>();

        if (loadGlobalsJSON == null)
        {
            Debug.LogWarning("[DialogueInkVariables] No globals JSON provided");
            return;
        }

        // Create the global variables story
        globalVariablesStory = new Story(loadGlobalsJSON.text);

        // Initialize the dictionary from Ink globals
        foreach (string name in globalVariablesStory.variablesState)
        {
            Ink.Runtime.Object value = globalVariablesStory.variablesState.GetVariableWithName(name);
            variables.Add(name, value);
            Debug.Log($"[DialogueInkVariables] Initialized Ink variable: {name} = {value}");
        }

        // Sync with the centralized variable manager if available
        if (DialogueVariableManager.Instance != null)
        {
            SyncToManager();
        }
    }

    /// <summary>
    /// Sync Ink variables to the centralized DialogueVariableManager
    /// </summary>
    private void SyncToManager()
    {
        if (DialogueVariableManager.Instance == null) return;

        foreach (var kvp in variables)
        {
            VariableDataType type = GetTypeFromInkValue(kvp.Value);
            object value = ConvertInkValueToObject(kvp.Value, type);
            
            DialogueVariableManager.Instance.RegisterVariable(kvp.Key, type, value);
        }

        Debug.Log($"[DialogueInkVariables] Synced {variables.Count} Ink variables to manager");
    }

    /// <summary>
    /// Start listening to a Story's variable changes
    /// </summary>
    public void StartListening(Story story) 
    {
        if (story == null) return;

        // Sync variables FROM manager TO story
        if (DialogueVariableManager.Instance != null)
        {
            DialogueVariableManager.Instance.SyncToInkStory(story);
        }
        else
        {
            // Fallback: use local variables
            VariablesToStory(story);
        }

        // Subscribe to changes
        story.variablesState.variableChangedEvent += VariableChanged;
    }

    /// <summary>
    /// Stop listening to a Story's variable changes
    /// </summary>
    public void StopListening(Story story) 
    {
        if (story == null) return;

        story.variablesState.variableChangedEvent -= VariableChanged;

        // Sync variables FROM story BACK TO manager
        if (DialogueVariableManager.Instance != null)
        {
            DialogueVariableManager.Instance.SyncFromInkStory(story);
        }
    }

    /// <summary>
    /// Called when an Ink variable changes
    /// </summary>
    private void VariableChanged(string name, Ink.Runtime.Object value) 
    {
        // Update local dictionary if it's a tracked variable
        if (variables.ContainsKey(name)) 
        {
            variables[name] = value;
            
            // Also update the centralized manager
            if (DialogueVariableManager.Instance != null)
            {
                VariableDataType type = GetTypeFromInkValue(value);
                object convertedValue = ConvertInkValueToObject(value, type);
                DialogueVariableManager.Instance.SetVariable(name, convertedValue);
            }
        }
    }

    /// <summary>
    /// Save variables (now handled by DialogueVariableManager)
    /// </summary>
    public void SaveVariables() 
    {
        if (DialogueVariableManager.Instance != null)
        {
            DialogueVariableManager.Instance.SaveVariables();
        }
        else
        {
            Debug.LogWarning("[DialogueInkVariables] DialogueVariableManager not found for saving");
        }
    }

    /// <summary>
    /// Apply variables from the dictionary to a Story
    /// </summary>
    private void VariablesToStory(Story story) 
    {
        if (story == null || variables == null) return;

        foreach(KeyValuePair<string, Ink.Runtime.Object> variable in variables) 
        {
            try
            {
                story.variablesState.SetGlobal(variable.Key, variable.Value);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[DialogueInkVariables] Failed to set variable '{variable.Key}': {e.Message}");
            }
        }
    }

    // ============================================
    // CONVERSION HELPERS
    // ============================================

    private VariableDataType GetTypeFromInkValue(Ink.Runtime.Object inkValue)
    {
        if (inkValue is IntValue) return VariableDataType.Int;
        if (inkValue is FloatValue) return VariableDataType.Float;
        if (inkValue is StringValue) return VariableDataType.String;
        if (inkValue is BoolValue) return VariableDataType.Bool;
        return VariableDataType.String;
    }

    private object ConvertInkValueToObject(Ink.Runtime.Object inkValue, VariableDataType targetType)
    {
        if (inkValue == null) return null;

        return targetType switch
        {
            VariableDataType.Bool => ((BoolValue)inkValue).value,
            VariableDataType.Int => ((IntValue)inkValue).value,
            VariableDataType.Float => ((FloatValue)inkValue).value,
            VariableDataType.String => ((StringValue)inkValue).value,
            _ => null
        };
    }
}