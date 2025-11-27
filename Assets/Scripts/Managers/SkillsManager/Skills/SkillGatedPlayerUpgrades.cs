using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Game;



// ============================================================
// APPROACH 3: Skill Tree gates Upgrade access
// ============================================================

/// <summary>
/// Modified PlayerUpgrades that checks skill requirements
/// </summary>
public class SkillGatedPlayerUpgrades : MonoBehaviour
{

    private Dictionary<string, string> upgradeSkillRequirements = new Dictionary<string, string>()
    {
        { "Bomb", "Explosive Expert" },      // Must unlock "Explosive Expert" skill first
        { "Shield", "Defensive Training" },   // Must unlock "Defensive Training" skill first
        { "Staff", "Arcane Knowledge" },      // Must unlock "Arcane Knowledge" skill first
    };
    
    public bool CanUseUpgrade(string upgradeName)
    {
        if (!upgradeSkillRequirements.ContainsKey(upgradeName))
            return true; // No skill requirement
        
        string requiredSkill = upgradeSkillRequirements[upgradeName];
        
        // Check if player has unlocked the required skill
        if (SkillsTreeManager.Instance != null)
        {
            Skill skill = SkillsTreeManager.Instance.GetSkillByName(requiredSkill);
            if (skill != null && !skill.IsUnlocked)
            {
                Debug.Log($"Cannot use {upgradeName} - must unlock '{requiredSkill}' skill first!");
                return false;
            }
        }
        
        return true;
    }
}