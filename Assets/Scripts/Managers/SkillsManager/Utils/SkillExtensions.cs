using UnityEngine;
using Core.Game;

namespace Core.Game
{
    // Extension methods for Skill class - you can add these directly to Skill.cs
    public static class SkillExtensions
{
    /// <summary>
    /// Check if player has enough skill points to unlock/level up
    /// </summary>
    public static bool HasEnoughSkillPoints(this Skill skill)
    {
        if (SkillsTreeManager.Instance == null)
            return false;

        return SkillsTreeManager.Instance.CurrentSkillPoints >= skill.UnlockCost;
    }

    /// <summary>
    /// Check if player has the required key in inventory
    /// </summary>
    public static bool HasRequiredKey(this Skill skill)
    {
        if (!skill.RequiresSpecialKey || string.IsNullOrEmpty(skill.RequiredKeyName))
            return true; // No key required

        if (InventoryManager.Instance == null)
            return false;

        // Check if player has the key in their inventory by ItemName
        return InventoryManager.Instance.HasItem(skill.RequiredKeyName);
    }

    /// <summary>
    /// Check if this skill can be afforded (both points and keys)
    /// </summary>
    public static bool CanAfford(this Skill skill)
    {
        return skill.HasEnoughSkillPoints() && skill.HasRequiredKey();
    }

    /// <summary>
    /// Attempt to unlock this skill through SkillTreeManager
    /// </summary>
    public static bool TryUnlockWithManager(this Skill skill)
    {
        if (!skill.CanUnlock())
        {
            Debug.LogWarning($"Cannot unlock {skill.SkillName} - prerequisites not met");
            return false;
        }

        if (!skill.CanAfford())
        {
            if (!skill.HasEnoughSkillPoints())
                Debug.LogWarning($"Not enough skill points for {skill.SkillName}");
            else if (!skill.HasRequiredKey())
                Debug.LogWarning($"Missing required key '{skill.RequiredKeyName}' for {skill.SkillName}");
            return false;
        }

        // Use SkillTreeManager to unlock (handles point deduction)
        if (SkillsTreeManager.Instance != null)
        {
            return SkillsTreeManager.Instance.TryUnlockSkill(skill);
        }

        return false;
    }

    /// <summary>
    /// Get the actual cost for this skill
    /// </summary>
    public static int GetActualCost(this Skill skill)
    {
        return skill.UnlockCost;
    }
}
} // End namespace Core.Game