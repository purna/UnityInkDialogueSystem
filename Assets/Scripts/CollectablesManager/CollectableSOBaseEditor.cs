using UnityEditor;
using UnityEngine;

/// <summary>
/// Base editor for all Collectable ScriptableObjects
/// Provides common functionality like info boxes and help sections
/// </summary>
public class CollectableSOBaseEditor : Editor
{
    protected void DrawInfoBox(string title, string purpose, string[] whenToUse, string[] examples, string whatHappens, string[] importantNotes = null)
    {
        // Main title box
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 14;
        titleStyle.normal.textColor = new Color(0.3f, 0.8f, 1f); // Light blue
        EditorGUILayout.LabelField(title, titleStyle);
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        // PURPOSE section
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("📋 PURPOSE", new Color(0.4f, 0.8f, 0.4f));
        EditorGUILayout.LabelField(purpose, EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(3);
        
        // WHEN TO USE section
        if (whenToUse != null && whenToUse.Length > 0)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawSectionHeader("✅ WHEN TO USE", new Color(0.8f, 0.6f, 0.2f));
            foreach (string item in whenToUse)
            {
                EditorGUILayout.LabelField("• " + item, EditorStyles.wordWrappedLabel);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(3);
        }
        
        // EXAMPLE USES section
        if (examples != null && examples.Length > 0)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawSectionHeader("💡 EXAMPLE USES", new Color(1f, 0.8f, 0.2f));
            foreach (string example in examples)
            {
                EditorGUILayout.LabelField("• " + example, EditorStyles.wordWrappedLabel);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(3);
        }
        
        // WHAT HAPPENS section
        if (!string.IsNullOrEmpty(whatHappens))
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawSectionHeader("⚙️ WHAT HAPPENS ON COLLECT", new Color(0.6f, 0.6f, 1f));
            EditorGUILayout.LabelField(whatHappens, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(3);
        }
        
        // IMPORTANT NOTES section
        if (importantNotes != null && importantNotes.Length > 0)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawSectionHeader("⚠️ IMPORTANT NOTES", new Color(1f, 0.4f, 0.4f));
            foreach (string note in importantNotes)
            {
                EditorGUILayout.LabelField("• " + note, EditorStyles.wordWrappedLabel);
            }
            EditorGUILayout.EndVertical();
        }
        
        EditorGUILayout.Space(10);
    }
    
    protected void DrawSectionHeader(string text, Color color)
    {
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.normal.textColor = color;
        EditorGUILayout.LabelField(text, headerStyle);
        EditorGUILayout.Space(2);
    }
    
    protected void DrawPreviewIcon(Sprite icon, string label = "Preview")
    {
        if (icon != null)
        {
            GUILayout.Label(label, EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical("box");
            GUILayout.Space(10);
            
            float maxPreviewSize = 64f;
            Rect rect = GUILayoutUtility.GetRect(maxPreviewSize, maxPreviewSize, GUILayout.ExpandWidth(false));
            EditorGUI.DrawPreviewTexture(rect, icon.texture, null, ScaleMode.ScaleToFit);
            
            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
        }
    }
}