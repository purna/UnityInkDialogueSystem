using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CollectableSkillPointsSO))]
public class CollectableSkillPointsSOEditor : CollectableSOBaseEditor
{
    private SerializedProperty skillPointsAmountProp;

    private void OnEnable()
    {
        skillPointsAmountProp = serializedObject.FindProperty("skillPointsAmount");
    }

    public override void OnInspectorGUI()
    {
        CollectableSkillPointsSO skillPoints = (CollectableSkillPointsSO)target;

        
        // Draw preview icon
        DrawPreviewIcon(skillPoints.ItemIcon, "⭐ Skill Points Preview");
        
        // Draw info box
        DrawInfoBox(
            title: "SKILL POINTS COLLECTABLE",
            purpose: "Gives player skill points to spend in the skill tree. Skill points are managed by SkillTreeManager and ARE added to inventory for tracking purposes.",
            whenToUse: new string[]
            {
                "Rewards for completing objectives",
                "Hidden in secret areas",
                "Boss/mini-boss rewards",
                "Quest completion rewards",
                "Level-up rewards"
            },
            examples: new string[]
            {
                "Basic Skill Point: skillPointsAmount = 1",
                "Rare Skill Orb: skillPointsAmount = 3",
                "Epic Knowledge Tome: skillPointsAmount = 5",
                "Legendary Wisdom Crystal: skillPointsAmount = 10"
            },
            whatHappens: "1. Adds skill points to SkillTreeManager (player can spend these)\n2. Adds to inventory (for tracking/statistics)\n3. Plays visual/audio effect\n4. Destroys the pickup"
        );
        
        // Draw default inspector
        DrawDefaultInspector();
        
        // Show current value preview with rarity indication
        serializedObject.Update();
        int amount = skillPointsAmountProp.intValue;
        serializedObject.ApplyModifiedProperties();
        
        if (amount > 0)
        {
            EditorGUILayout.Space(5);
            string rarity = GetRarityLabel(amount);
            string icon = GetRarityIcon(amount);
            EditorGUILayout.HelpBox($"{icon} This will give {amount} skill point{(amount > 1 ? "s" : "")} when collected. ({rarity})", MessageType.Info);
        }
        
        // Show tip about SkillTreeManager
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox("💡 TIP: Check SkillTreeManager.CurrentSkillPoints to see the player's total unspent skill points.", MessageType.None);
    }
    
    private string GetRarityLabel(int amount)
    {
        if (amount >= 10) return "Legendary";
        if (amount >= 5) return "Epic";
        if (amount >= 3) return "Rare";
        return "Common";
    }
    
    private string GetRarityIcon(int amount)
    {
        if (amount >= 10) return "🌟";
        if (amount >= 5) return "⭐⭐⭐";
        if (amount >= 3) return "⭐⭐";
        return "⭐";
    }

  
}