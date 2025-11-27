using UnityEngine;
using UnityEditor;
using Core.Game;


/// <summary>
/// Example of how Skills can interact with Upgrades without merging the systems
/// Skills can unlock, enhance, or modify upgrades
/// </summary>

// ============================================================
// APPROACH 1: Skills unlock Upgrade abilities
// ============================================================

/// <summary>
/// Skill Function that unlocks an upgrade permanently
/// Use this for: "Unlock Bomb Ability" skill node
/// </summary>
[CreateAssetMenu(menuName = "Skill Tree/Functions/Unlock Upgrade")]
public class SkillFunctionUnlockUpgrade : SkillFunction
{
    [SerializeField] private string upgradeName; // "Bomb", "Shield", etc.
    
    public override void Execute(Skill skill)
    {
        PlayerUpgrades playerUpgrades = FindObjectOfType<PlayerUpgrades>();
        if (playerUpgrades != null)
        {
            playerUpgrades.UnlockUpgrade(upgradeName);
            Debug.Log($"Skill '{skill.SkillName}' unlocked upgrade: {upgradeName}");
        }
    }
}

