using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CollectableDetectionSkill))]
public class CollectableDetectionSkillEditor : Editor
{
    private SerializedProperty detectionRadiusProp;
    private SerializedProperty showOnMinimapProp;
    private SerializedProperty maxLevelProp;

    private void OnEnable()
    {
        detectionRadiusProp = serializedObject.FindProperty("detectionRadius");
        showOnMinimapProp = serializedObject.FindProperty("showOnMinimap");
        maxLevelProp        = serializedObject.FindProperty("maxLevel");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        CollectableDetectionSkill skill = (CollectableDetectionSkill)target;

        // Draw preview icon
        DrawPreviewIcon(skill.Icon, "🔑 Skill Tree Key Preview");

        // Header
        DrawTitleHeader();

        // Info sections
        DrawInfoBox(skill);

        // Default inspector
        DrawDefaultInspector();

        // Preview section (like DropRateSkill)
        DrawPreviewSection();

        serializedObject.ApplyModifiedProperties();
    }

    // ---------------------------------------------------------
    // HEADER
    // ---------------------------------------------------------
    private void DrawTitleHeader()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 14;
        titleStyle.normal.textColor = new Color(1f, 0.8f, 0.2f); 
        EditorGUILayout.LabelField("🔍 COLLECTABLE DETECTION SKILL", titleStyle);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }

    // ---------------------------------------------------------
    // INFO BOXES (matching style of DropRateSkillEditor)
    // ---------------------------------------------------------
    private void DrawInfoBox(CollectableDetectionSkill skill)
    {
        // PURPOSE
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("📋 PURPOSE", new Color(0.4f, 0.8f, 0.4f));
        EditorGUILayout.LabelField(
            "Passive skill that increases the player's ability to detect collectables. "
          + "Higher levels increase detection radius. Optionally reveals collectables on the minimap.",
            EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(3);

        // WHEN TO USE
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("✅ WHEN TO USE", new Color(0.8f, 0.6f, 0.2f));
        EditorGUILayout.LabelField("• Exploration / scavenger focused skill trees", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• To reveal hidden collectables or currency pickups", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• To guide players toward optional side objectives", EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(3);

        // HOW IT WORKS
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("⚙️ HOW IT WORKS", new Color(0.6f, 0.6f, 1f));
        EditorGUILayout.LabelField("When unlocked, activates detection:", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Level 1 = base radius", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Level 2+ = radius increases proportionally", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Optional minimap icons for collectables", EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(3);

        // INTEGRATION
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("🔧 INTEGRATION", new Color(0.8f, 0.4f, 0.8f));
        EditorGUILayout.LabelField("Your detection system should check:", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("CollectableDetectionSkill.GetCurrentRadius()", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Use radius for AOE queries", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Enable/disable minimap markers", EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(3);

        // EXAMPLE SETUP
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("💡 EXAMPLE SETUP", new Color(1f, 0.8f, 0.2f));
        EditorGUILayout.LabelField("• Name: \"Treasure Sense\"", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Base Radius: 10 units", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Max Level: 3", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Minimap Tracking: Optional", EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(3);

        // IMPORTANT NOTES
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("⚠️ IMPORTANT NOTES", new Color(1f, 0.4f, 0.4f));
        EditorGUILayout.LabelField("• This is a SKILL, not a DETECTION SYSTEM", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Your scene must contain a CollectableDetectionSystem", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Radius scales with level", EditorStyles.wordWrappedLabel);
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

    // ---------------------------------------------------------
    // PREVIEW (similar to DropRateSkill)
    // ---------------------------------------------------------
    private void DrawPreviewSection()
    {
        if (detectionRadiusProp == null || maxLevelProp == null)
            return;

        EditorGUILayout.Space(5);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        GUIStyle previewStyle = new GUIStyle(EditorStyles.boldLabel);
        previewStyle.normal.textColor = new Color(1f, 0.8f, 0.2f);
        EditorGUILayout.LabelField("🔍 DETECTION PREVIEW", previewStyle);
        EditorGUILayout.Space(3);

        float baseRadius = detectionRadiusProp.floatValue;
        int maxLevel = maxLevelProp.intValue;

        // Level breakdown
        for (int level = 1; level <= maxLevel; level++)
        {
            float radius = baseRadius * level;
            EditorGUILayout.LabelField(
                $"Level {level}: {radius:F1} units detection range",
                EditorStyles.wordWrappedLabel);
        }

        EditorGUILayout.Space(5);

        // Minimap status
        bool minimap = showOnMinimapProp.boolValue;
        string minimapLabel = minimap ? "🗺️ Minimap Icons: ENABLED" : "🗺️ Minimap Icons: Disabled";
        EditorGUILayout.LabelField(minimapLabel, EditorStyles.wordWrappedLabel);

        EditorGUILayout.Space(5);

        // Example calculation
        EditorGUILayout.LabelField("Example radius at max level:", EditorStyles.miniLabel);
        float finalRadius = baseRadius * maxLevel;
        EditorGUILayout.LabelField($"{baseRadius} × {maxLevel} = {finalRadius} units", EditorStyles.miniLabel);

        EditorGUILayout.EndVertical();
    }
}
