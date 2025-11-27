// ============================================================================
// INSTRUCTIONS: Collectable Detection Skill
// ============================================================================
// HOW TO CREATE:
// 1. Right-click in the Project Window
// 2. Select: Create > Skill Tree > Skills > Collectable Detection
// 3. Fill out the settings:
//    - Detection Radius: How far the player can detect collectables
//    - Show On Minimap: If true, detected collectables appear on the minimap
//    - Collectable Layer: LayerMask used to locate collectables
//
// WHEN THIS SKILL IS UNLOCKED:
// ✔ Enables collectable detection through CollectableDetectionSystem
// ✔ Highlights or tracks collectables near the player
// ✔ Can optionally reveal collectables on the minimap
//
// WHEN THIS SKILL IS LOCKED:
// ✘ Detection is disabled
// ✘ Minimap and tracking indicators are removed
//
// USE CASES:
// - Treasure hunter perks
// - Scavenger detection abilities
// - Skills that locate items in a radius
//
// ============================================================================
// CUSTOM EDITOR
// ============================================================================
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SkillCollectableDetection))]
public class SkillCollectableDetectionEditor : Editor
{
    private SerializedProperty detectionRadius;
    private SerializedProperty showOnMinimap;
    private SerializedProperty collectableLayer;

    private void OnEnable()
    {
        detectionRadius = serializedObject.FindProperty("detectionRadius");
        showOnMinimap = serializedObject.FindProperty("showOnMinimap");
        collectableLayer = serializedObject.FindProperty("collectableLayer");
    }

    public override void OnInspectorGUI()
    {
        SkillCollectableDetection skill = (SkillCollectableDetection)target;
        serializedObject.Update();

        DrawPreviewIcon(skill.Icon);

        DrawTitleHeader();
        DrawInfoBox();
        DrawDefaultInspector();
        DrawPreview(skill);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawTitleHeader()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 14;
        titleStyle.normal.textColor = new Color(0.4f, 0.8f, 1f);
        EditorGUILayout.LabelField("🔍 COLLECTABLE DETECTION SKILL", titleStyle);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }

    private void DrawInfoBox()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUIStyle header = new GUIStyle(EditorStyles.boldLabel);
        header.normal.textColor = new Color(1f, 0.8f, 0.2f);
        EditorGUILayout.LabelField("📋 PURPOSE", header);
        EditorGUILayout.LabelField("Highlights nearby collectables and optionally shows them on the minimap.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space(4);

        header.normal.textColor = new Color(0.8f, 0.6f, 0.2f);
        EditorGUILayout.LabelField("✅ WHEN TO USE", header);
        EditorGUILayout.LabelField("• Skills that help players locate items", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Survival, exploration, or RPG games", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space(4);

        header.normal.textColor = new Color(0.6f, 0.6f, 1f);
        EditorGUILayout.LabelField("⚙️ HOW IT WORKS", header);
        EditorGUILayout.LabelField("Enables CollectableDetectionSystem and displays collectables in range.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space(4);

        header.normal.textColor = new Color(1f, 0.4f, 0.4f);
        EditorGUILayout.LabelField("⚠️ IMPORTANT NOTES", header);
        EditorGUILayout.LabelField("• Requires CollectableDetectionSystem in the scene.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Auto-disables when the skill is locked.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(8);
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

    private void DrawPreview(SkillCollectableDetection skill)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUIStyle previewStyle = new GUIStyle(EditorStyles.boldLabel);
        previewStyle.normal.textColor = new Color(0.4f, 1f, 0.6f);
        EditorGUILayout.LabelField("📡 DETECTION PREVIEW", previewStyle);
        EditorGUILayout.Space(3);

        EditorGUILayout.LabelField($"Radius: {skill.DetectionRadius} units", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField(skill.ShowOnMinimap ? "Shown on minimap" : "Not shown on minimap", EditorStyles.wordWrappedLabel);

        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Collectable Layer:", EditorStyles.miniLabel);
        EditorGUILayout.LabelField(collectableLayer.intValue.ToString(), EditorStyles.wordWrappedLabel);

        EditorGUILayout.EndVertical();
    }
}