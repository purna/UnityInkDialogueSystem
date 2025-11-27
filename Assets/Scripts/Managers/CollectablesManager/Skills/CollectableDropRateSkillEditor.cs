using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CollectableDropRateSkill))]
public class CollectableDropRateSkillEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CollectableDropRateSkill dropRateSkill = (CollectableDropRateSkill)target;
        
        // Draw preview icon
        DrawPreviewIcon(dropRateSkill.Icon, "🔑 Skill Tree Key Preview");

        // Draw header
        DrawTitleHeader();
        
        // Draw info box
        DrawInfoBox(dropRateSkill);
        
        // Draw default inspector
        DrawDefaultInspector();
        
        // Show preview calculations
        DrawPreviewSection(dropRateSkill);
    }

       private void DrawTitleHeader()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 14;
        titleStyle.normal.textColor = new Color(1f, 0.8f, 0.2f); // Gold color
        EditorGUILayout.LabelField("🍀 COLLECTABLE DROP RATE SKILL", titleStyle);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
    
    private void DrawInfoBox(CollectableDropRateSkill dropRateSkill)
    {
  
        
        // PURPOSE section
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("📋 PURPOSE", new Color(0.4f, 0.8f, 0.4f));
        EditorGUILayout.LabelField("Passive skill that increases item drop rates and currency drops. Always active once unlocked and scales with skill level.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(3);
        
        // WHEN TO USE section
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("✅ WHEN TO USE", new Color(0.8f, 0.6f, 0.2f));
        EditorGUILayout.LabelField("• Create a 'loot luck' skill in your skill tree", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Use this skill type instead of base Skill type", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Player unlocks it like any other skill", EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(3);
        
        // HOW IT WORKS section
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("⚙️ HOW IT WORKS", new Color(0.6f, 0.6f, 1f));
        EditorGUILayout.LabelField("When unlocked, multiplies drop rates:", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Level 1 = 1.5x drops (50% more)", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Level 2 = 3.0x drops (if max level = 2)", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Affects currency and/or items based on settings", EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(3);
        
        // INTEGRATION section
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("🔧 INTEGRATION", new Color(0.8f, 0.4f, 0.8f));
        EditorGUILayout.LabelField("Your loot system should check:", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("CollectableDropRateSkill.GetScaledDropRate()", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField("Multiply drop chances by this value", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("Example: base 10% chance → 1.5x = 15% chance", EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(3);
        
        // EXAMPLE SETUP section
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("💡 EXAMPLE SETUP", new Color(1f, 0.8f, 0.2f));
        EditorGUILayout.LabelField("• Name: \"Fortune's Favor\"", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Drop Rate Multiplier: 1.5 (50% increase)", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Max Level: 3", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Each level increases drops by 50% more", EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(3);
        
        // IMPORTANT NOTES section
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("⚠️ IMPORTANT NOTES", new Color(1f, 0.4f, 0.4f));
        EditorGUILayout.LabelField("• This is a SKILL, not a COLLECTABLE", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Create via: Right-click → Pixelagent → Collectable → Collectable Drop Rate", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Place it in your skill tree like any other skill", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("• Effect multiplies per level (Level 2 = 2x the multiplier)", EditorStyles.wordWrappedLabel);
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
    
    private void DrawPreviewSection(CollectableDropRateSkill dropRateSkill)
    {
        SerializedProperty multiplierProp = serializedObject.FindProperty("dropRateMultiplier");
        SerializedProperty maxLevelProp = serializedObject.FindProperty("maxLevel");
        SerializedProperty affectsCurrencyProp = serializedObject.FindProperty("affectsCurrency");
        SerializedProperty affectsItemsProp = serializedObject.FindProperty("affectsItems");
        
        if (multiplierProp != null && maxLevelProp != null)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            GUIStyle previewStyle = new GUIStyle(EditorStyles.boldLabel);
            previewStyle.normal.textColor = new Color(1f, 0.8f, 0.2f);
            EditorGUILayout.LabelField("🍀 DROP RATE PREVIEW", previewStyle);
            EditorGUILayout.Space(3);
            
            float multiplier = multiplierProp.floatValue;
            int maxLevel = maxLevelProp.intValue;
            
            // Show what each level does
            for (int level = 1; level <= maxLevel; level++)
            {
                float scaledRate = multiplier * level;
                float percentIncrease = (scaledRate - 1f) * 100f;
                EditorGUILayout.LabelField($"Level {level}: {scaledRate:F1}x drops (+{percentIncrease:F0}% increase)", EditorStyles.wordWrappedLabel);
            }
            
            EditorGUILayout.Space(3);
            
            // Show what it affects
            string affects = "";
            if (affectsCurrencyProp != null && affectsCurrencyProp.boolValue && 
                affectsItemsProp != null && affectsItemsProp.boolValue)
            {
                affects = "Affects: Currency AND Items";
            }
            else if (affectsCurrencyProp != null && affectsCurrencyProp.boolValue)
            {
                affects = "Affects: Currency ONLY";
            }
            else if (affectsItemsProp != null && affectsItemsProp.boolValue)
            {
                affects = "Affects: Items ONLY";
            }
            else
            {
                affects = "⚠️ Warning: Doesn't affect anything!";
            }
            
            EditorGUILayout.LabelField(affects, EditorStyles.wordWrappedLabel);
            
            EditorGUILayout.Space(3);
            
            // Example calculation
            EditorGUILayout.LabelField("Example: 10% base drop chance at max level:", EditorStyles.miniLabel);
            float maxScaledRate = multiplier * maxLevel;
            float exampleChance = 10f * maxScaledRate;
            EditorGUILayout.LabelField($"10% × {maxScaledRate:F1} = {exampleChance:F1}% drop chance", EditorStyles.miniLabel);
            
            EditorGUILayout.EndVertical();
        }
    }
}