using System;
using UnityEditor;
using UnityEngine;

public static class InspectorUtility {
    public static void DrawHeader(string label) {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
    }

    public static void DrawHelpBox(string message, MessageType messageType = MessageType.Info, bool wide = true) {
        EditorGUILayout.HelpBox(message, messageType, wide);
    }

    public static void DrawPropertyField(this SerializedProperty property) {
        if (property == null)
        {
            EditorGUILayout.HelpBox("Property is null!", MessageType.Error);
            return;
        }
        
        EditorGUILayout.PropertyField(property);
    }

    public static void DrawDisabledField(Action action) {
        if (action == null)
        {
            Debug.LogWarning("DrawDisabledField: Action is null");
            return;
        }
        
        EditorGUI.BeginDisabledGroup(true);
        action.Invoke();
        EditorGUI.EndDisabledGroup();
    }

    public static int DrawPopup(string label, SerializedProperty selectedIndexProperty, string[] options) {
        if (selectedIndexProperty == null)
        {
            EditorGUILayout.HelpBox("Selected index property is null!", MessageType.Error);
            return 0;
        }
        
        if (options == null || options.Length == 0)
        {
            EditorGUILayout.HelpBox("No options available for popup!", MessageType.Warning);
            return 0;
        }
        
        return EditorGUILayout.Popup(label, selectedIndexProperty.intValue, options);
    }

    public static void DrawSpace(int amount = 4) {
        EditorGUILayout.Space(amount);
    }
}