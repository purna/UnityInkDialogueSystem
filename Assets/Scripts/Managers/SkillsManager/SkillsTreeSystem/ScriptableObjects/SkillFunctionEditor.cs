

// ═══════════════════════════════════════════════════════════════════════════
// SKILL FUNCTION EDITOR
// ═══════════════════════════════════════════════════════════════════════════
using UnityEditor;
using UnityEngine;
using Core.Game;

[CustomEditor(typeof(SkillFunction), true)]
public class SkillFunctionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SkillFunction function = (SkillFunction)target;

        DrawTitleHeader(function);
        DrawInfoBox(function);
        DrawDefaultInspector();
        DrawPreview(function);
    }

    private void DrawTitleHeader(SkillFunction func)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 14;
        titleStyle.normal.textColor = new Color(0.7f, 0.4f, 1f); // purple
        EditorGUILayout.LabelField($"⚡ SKILL FUNCTION: {func.name}", titleStyle);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }

    private void DrawInfoBox(SkillFunction func)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("📋 PURPOSE", new Color(0.4f, 0.8f, 0.4f));
        EditorGUILayout.LabelField(func.Description, EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(3);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("⚙️ HOW IT WORKS", new Color(0.6f, 0.6f, 1f));
        EditorGUILayout.LabelField("This function will run automatically when its Skill is unlocked or leveled.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(8);
    }

    private void DrawSectionHeader(string text, Color color)
    {
        GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
        style.normal.textColor = color;
        EditorGUILayout.LabelField(text, style);
        EditorGUILayout.Space(2);
    }

    private void DrawPreview(SkillFunction func)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUIStyle previewStyle = new GUIStyle(EditorStyles.boldLabel);
        previewStyle.normal.textColor = new Color(1f, 0.8f, 0.3f);
        EditorGUILayout.LabelField("🔎 PREVIEW", previewStyle);
        EditorGUILayout.Space(3);

        if (func is StatModifierFunction stat)
        {
            EditorGUILayout.LabelField("Stat Modifier Function", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField($"• Stat: {stat.GetType().GetField("_statType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(stat)}");
            EditorGUILayout.LabelField($"• Modifier: {stat.GetType().GetField("_modifierType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(stat)}");
            EditorGUILayout.LabelField($"• Uses Skill Value: {stat.GetType().GetField("_useSkillValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(stat)}");
        }
        else if (func is UnlockAbilityFunction ability)
        {
            EditorGUILayout.LabelField("Unlock Ability Function:", EditorStyles.wordWrappedLabel);
            
            // Get available ability options - using reflection to access the serialized field
            var serializedObject = new SerializedObject(ability);
            var abilityIdProperty = serializedObject.FindProperty("_abilityID");
            
            // Use EditorGUILayout.PropertyField for proper serialized property editing
            serializedObject.Update();
            EditorGUILayout.PropertyField(abilityIdProperty, new GUIContent("Ability ID"));
            serializedObject.ApplyModifiedProperties();
        }
        else if (func is CustomEventFunction evt)
        {
            EditorGUILayout.LabelField("Custom Event Function:", EditorStyles.wordWrappedLabel);
            
            // Use serialized properties for proper field access
            var serializedObject = new SerializedObject(evt);
            var eventNameProperty = serializedObject.FindProperty("_eventName");
            var eventParameterProperty = serializedObject.FindProperty("_eventParameter");
            
            serializedObject.Update();
            EditorGUILayout.PropertyField(eventNameProperty, new GUIContent("Event Name"));
            EditorGUILayout.PropertyField(eventParameterProperty, new GUIContent("Event Parameter"));
            serializedObject.ApplyModifiedProperties();
        }
        else
        {
            EditorGUILayout.LabelField("This function has no preview configuration.");
        }

        EditorGUILayout.EndVertical();
    }
}
