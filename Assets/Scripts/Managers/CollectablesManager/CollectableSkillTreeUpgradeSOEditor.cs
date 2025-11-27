using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CollectableSkillTreeUpgradeSO))]
public class CollectableSkillTreeUpgradeSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CollectableSkillTreeUpgradeSO upgrade = (CollectableSkillTreeUpgradeSO)target;

        // Draw preview icon
        DrawPreviewIcon(upgrade.ItemIcon, "🔑 Skill Tree Key Preview");


        DrawTitleHeader();
        DrawInfoBox(upgrade);

        EditorGUILayout.Space(5);

        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        DrawSkillPreview(upgrade);
        DrawWarnings(upgrade);
    }

    // ───────────────────────────────────────────────────────────────
    // TITLE HEADER
    // ───────────────────────────────────────────────────────────────
    private void DrawTitleHeader()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 14;
        titleStyle.normal.textColor = new Color(0.3f, 1f, 1f); // Cyan

        EditorGUILayout.LabelField("✨ SKILL TREE UPGRADE COLLECTABLE", titleStyle);

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }

    // ───────────────────────────────────────────────────────────────
    // INFO BOX (Purpose, Modes, Notes)
    // ───────────────────────────────────────────────────────────────
    private void DrawInfoBox(CollectableSkillTreeUpgradeSO upgrade)
    {
        // PURPOSE
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("📋 PURPOSE", new Color(0.4f, 0.8f, 0.4f));

        EditorGUILayout.LabelField(
            "Collectable that automatically unlocks a skill or grants skill points.\n"
            + "Useful for story gating, tutorials, major boss rewards, or bonus points.",
            EditorStyles.wordWrappedLabel
        );
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(3);

        // WHEN TO USE
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("✅ WHEN TO USE", new Color(0.9f, 0.7f, 0.3f));
        EditorGUILayout.LabelField("• Tutorial auto-skill unlocks", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Story-required abilities", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Bonus skill point rewards", EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(3);

        // MODES
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("🎮 MODES", new Color(0.6f, 0.6f, 1f));

        EditorGUILayout.LabelField("• Mode 1 → Auto-unlock a skill", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Mode 2 → Grant skill points only", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Mode 3 → Do both (unlock + points)", EditorStyles.wordWrappedLabel);

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(3);

        // IMPORTANT NOTES
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("⚠️ IMPORTANT NOTES", new Color(1f, 0.4f, 0.4f));
        EditorGUILayout.LabelField("• Auto-unlock checks prerequisites", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Unlock cost still applies (uses points)", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Points-only mode possible", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Name-based lookup used if SO reference missing", EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);
    }

    private void DrawSectionHeader(string text, Color color)
    {
        GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
        style.normal.textColor = color;
        EditorGUILayout.LabelField(text, style);
        EditorGUILayout.Space(2);
    }

    // ───────────────────────────────────────────────────────────────
    // PREVIEW OF SKILL RESOLUTION
    // ───────────────────────────────────────────────────────────────
    private void DrawSkillPreview(CollectableSkillTreeUpgradeSO upgrade)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        GUIStyle previewStyle = new GUIStyle(EditorStyles.boldLabel);
        previewStyle.normal.textColor = new Color(0.3f, 1f, 1f);

        EditorGUILayout.LabelField("🔍 SKILL RESOLUTION PREVIEW", previewStyle);
        EditorGUILayout.Space(3);

        // Direct reference
        SerializedProperty skillProp = serializedObject.FindProperty("skillToUnlock");
        SerializedProperty nameProp = serializedObject.FindProperty("skillNameToUnlock");
        SerializedProperty autoUnlockProp = serializedObject.FindProperty("autoUnlockSkill");
        SerializedProperty pointsProp = serializedObject.FindProperty("skillPointsToGrant");

        bool autoUnlock = autoUnlockProp.boolValue;

        if (autoUnlock)
        {
            if (skillProp.objectReferenceValue != null)
            {
                EditorGUILayout.LabelField("Skill: Uses Direct Skill Reference", EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField($"→ {skillProp.objectReferenceValue.name}", EditorStyles.miniLabel);
            }
            else if (!string.IsNullOrEmpty(nameProp.stringValue))
            {
                EditorGUILayout.LabelField("Skill: Will lookup by name", EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField($"→ \"{nameProp.stringValue}\"", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField("⚠ No skill provided — auto-unlock will fail", EditorStyles.wordWrappedLabel);
            }
        }
        else
        {
            EditorGUILayout.LabelField("Auto-Unlock Disabled", EditorStyles.wordWrappedLabel);
        }

        EditorGUILayout.Space(5);

        // Points preview
        int pts = pointsProp.intValue;
        if (pts > 0)
        {
            EditorGUILayout.LabelField($"Skill Points Granted: +{pts}", EditorStyles.wordWrappedLabel);
        }
        else
        {
            EditorGUILayout.LabelField("Skill Points Granted: None", EditorStyles.wordWrappedLabel);
        }

        EditorGUILayout.EndVertical();
    }

    // ───────────────────────────────────────────────────────────────
    // WARNINGS & VALIDATION
    // ───────────────────────────────────────────────────────────────
    private void DrawWarnings(CollectableSkillTreeUpgradeSO upgrade)
    {
        SerializedProperty skillProp = serializedObject.FindProperty("skillToUnlock");
        SerializedProperty nameProp = serializedObject.FindProperty("skillNameToUnlock");
        SerializedProperty autoProp = serializedObject.FindProperty("autoUnlockSkill");
        SerializedProperty pointsProp = serializedObject.FindProperty("skillPointsToGrant");

        bool autoUnlock = autoProp.boolValue;
        bool hasSkillReference = skillProp.objectReferenceValue != null;
        bool hasSkillName = !string.IsNullOrEmpty(nameProp.stringValue);
        int points = pointsProp.intValue;

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUIStyle warningStyle = new GUIStyle(EditorStyles.boldLabel);
        warningStyle.normal.textColor = Color.yellow;
        EditorGUILayout.LabelField("⚠ VALIDATION", warningStyle);

        // Auto-unlock enabled but no skill provided
        if (autoUnlock && !hasSkillReference && !hasSkillName)
        {
            EditorGUILayout.HelpBox(
                "Auto-unlock is enabled, but no skill is assigned!\nThis collectable will NOT unlock anything.",
                MessageType.Warning
            );
        }

        // Both reference AND name assigned
        if (hasSkillReference && hasSkillName)
        {
            EditorGUILayout.HelpBox(
                "Both a skill reference AND a skill name are assigned.\nThe direct reference will be used.",
                MessageType.Info
            );
        }

        // Neither mode enabled
        if (!autoUnlock && points == 0)
        {
            EditorGUILayout.HelpBox(
                "This collectable does nothing! (No auto-unlock, no skill points)",
                MessageType.Warning
            );
        }

        EditorGUILayout.EndVertical();
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
