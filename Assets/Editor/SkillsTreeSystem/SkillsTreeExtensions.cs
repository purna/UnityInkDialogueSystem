using UnityEditor;
using UnityEngine;

/// <summary>
/// Extension methods and helpers for the Skills Tree system
/// </summary>
public static class SkillsTreeExtensions
{
    /// <summary>
    /// Extension method to save a ScriptableObject
    /// </summary>
    public static void Save(this ScriptableObject obj)
    {
        if (obj == null) return;
        
        EditorUtility.SetDirty(obj);
        AssetDatabase.SaveAssetIfDirty(obj);
    }
    
    /// <summary>
    /// Get icon from Skill (works with both Skill and SkillsTree types)
    /// </summary>
    public static Sprite GetIcon(this Skill skill)
    {
        return skill != null ? skill.Icon : null;
    }
    
    /// <summary>
    /// Update node visual from its skill reference
    /// </summary>
    public static void RefreshFromSkill(this SkillsTreeBaseNode node)
    {
        if (node == null || node.Skill == null) return;
        
        node.UpdateIcon(node.Skill.Icon);
        node.UpdateDescription(node.Skill.Description);
        node.UpdateSkillProperties(
            node.Skill.Tier,
            node.Skill.UnlockCost,
            node.Skill.Value,
            node.Skill.MaxLevel
        );
    }
}