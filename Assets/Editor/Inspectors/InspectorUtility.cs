using System;
using UnityEditor;

public static class InspectorUtility {
    public static void DrawHeader(string label) {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
    }

    public static void DrawHelpBox(string message, MessageType messageType = MessageType.Info, bool wide = true) {
        EditorGUILayout.HelpBox(message, messageType, wide);
    }

    public static void DrawPropertyField(this SerializedProperty property) {
        EditorGUILayout.PropertyField(property);
    }

    public static void DrawDisabledField(Action action) {
        EditorGUI.BeginDisabledGroup(true);
        action.Invoke();
        EditorGUI.EndDisabledGroup();
    }

    public static int DrawPopup(string label, SerializedProperty selectedIndexProperty, string[] options) {
        return EditorGUILayout.Popup(label, selectedIndexProperty.intValue, options);
    }

    public static void DrawSpace(int amount = 4) {
        EditorGUILayout.Space(amount);
    }
}
