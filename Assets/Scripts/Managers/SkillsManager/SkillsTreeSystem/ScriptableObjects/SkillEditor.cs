// ═══════════════════════════════════════════════════════════════════════════
// SKILL EDITOR (Using CollectableDropRateSkillEditor Layout)
// ═══════════════════════════════════════════════════════════════════════════
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Skill))]
public class SkillEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Skill skill = (Skill)target;
        
        DrawPreviewIcon(skill.Icon);
       
        DrawTitleHeader();
        DrawInfoBox();


        DrawDefaultInspector();

        DrawPreviewSection(skill);
    }

    private void DrawTitleHeader()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 14;
        titleStyle.normal.textColor = new Color(0.3f, 0.7f, 1f); // Cyan-blue
        EditorGUILayout.LabelField("🎯 SKILL NODE", titleStyle);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }

    private void DrawInfoBox()
    {
        // PURPOSE
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("📋 PURPOSE", new Color(0.4f, 0.8f, 0.4f));
        EditorGUILayout.LabelField(
            "Represents a single ability/upgrade node in the skill tree. Handles unlocking, leveling, prerequisite logic, icons, and SkillFunctions.",
            EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(3);

        // WHEN TO USE
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("✅ WHEN TO USE", new Color(0.8f, 0.6f, 0.2f));
        EditorGUILayout.LabelField("• Passive upgrades (health, damage, stamina)", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Active ability unlocks", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Tier-based progression trees", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Branch connectors and gating nodes", EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(3);

        // HOW IT WORKS
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("⚙️ HOW IT WORKS", new Color(0.6f, 0.6f, 1f));
        EditorGUILayout.LabelField("Skill unlocks only when all prerequisites are unlocked.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("Unlock() applies all SkillFunctions.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("LevelUp() increases stats and re-applies functions.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("Reset() clears runtime locked/level state.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(3);

        // EXAMPLES
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("💡 EXAMPLES", new Color(1f, 0.8f, 0.2f));
        EditorGUILayout.LabelField("• 'Warrior Strength' (+10% damage per level)", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• 'Fireball' (active ability unlock)", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Tier progression: Tier 1 → Tier 2 node", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• 'Vitality' (+5 max HP per level)", EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(3);

        // NOTES
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("⚠️ IMPORTANT NOTES", new Color(1f, 0.4f, 0.4f));
        EditorGUILayout.LabelField("• Create via: Right-click → Skill Tree → Skill", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Visual position is used by your skill tree editor", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• SkillFunctions run automatically when unlocked/leveled", EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);
    }

    private void DrawSectionHeader(string text, Color color)
    {
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.normal.textColor = color;
        EditorGUILayout.LabelField(text, headerStyle);
        EditorGUILayout.Space(2);
    }

    private void DrawPreviewIcon(Sprite icon)
    {
        if (icon == null) return;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUIStyle previewStyle = new GUIStyle(EditorStyles.boldLabel);
        previewStyle.normal.textColor = new Color(1f, 0.8f, 0.2f);
        EditorGUILayout.LabelField("🖼️ SKILL ICON PREVIEW", previewStyle);

        Rect rect = GUILayoutUtility.GetRect(64, 64, GUILayout.ExpandWidth(false));
        GUI.DrawTexture(rect, icon.texture, ScaleMode.ScaleToFit);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }

    private void DrawPreviewSection(Skill skill)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUIStyle previewStyle = new GUIStyle(EditorStyles.boldLabel);
        previewStyle.normal.textColor = new Color(0.3f, 1f, 0.5f);
        EditorGUILayout.LabelField("🔎 SKILL VALUE PREVIEW", previewStyle);
        EditorGUILayout.Space(3);

        // Show base/stat scaling
        if (skill.MaxLevel > 1)
        {
            for (int level = 1; level <= skill.MaxLevel; level++)
            {
                float scaledValue = skill.Value * level;
                EditorGUILayout.LabelField($"Level {level}: {scaledValue} (Base {skill.Value})", EditorStyles.wordWrappedLabel);
            }
        }
        else
        {
            EditorGUILayout.LabelField($"Value: {skill.Value}", EditorStyles.wordWrappedLabel);
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField($"Tier: {skill.Tier}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Unlock Cost: {skill.UnlockCost}", EditorStyles.miniLabel);

        EditorGUILayout.EndVertical();
    }
}