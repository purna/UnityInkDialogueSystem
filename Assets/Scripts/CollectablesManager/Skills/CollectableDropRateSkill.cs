
// ═══════════════════════════════════════════════════════════════════════════
// SPECIAL SKILL TYPE: COLLECTABLE DROP RATE SKILL
// ═══════════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// Collectable Drop Rate Skill - Passive skill that increases item drop rates
/// 
/// PURPOSE:
/// - Increases chances of getting rare collectables
/// - Increases currency drops
/// - Passive skill (always active once unlocked)
/// - Can scale with skill level
/// 
/// WHEN TO USE:
/// - Create a skill in your skill tree for "loot luck"
/// - Use this skill type instead of base Skill type
/// - Player unlocks it like any other skill
/// 
/// HOW IT WORKS:
/// - When unlocked, multiplies drop rates
/// - Level 1 = 1.5x drops (50% more)
/// - Level 2 = 3.0x drops (if max level = 2 and drop rate = 1.5 per level)
/// - Affects currency and/or items based on settings
/// 
/// INTEGRATION:
/// - Your loot system should check: CollectableDropRateSkill.GetScaledDropRate()
/// - Multiply drop chances by this value
/// - Example: if base chance is 10%, with 1.5x = 15% chance
/// 
/// EXAMPLE SETUP:
/// - Create this skill in skill tree
/// - Name: "Fortune's Favor"
/// - Drop Rate Multiplier: 1.5 (50% increase)
/// - Max Level: 3
/// - Each level increases drops by 50% more
/// 
/// WHAT HAPPENS ON UNLOCK:
/// 1. Skill becomes active
/// 2. All future collectables drop more frequently
/// 3. Currency drops increase
/// 4. Message shown to player
/// 
/// NOTE: This is a SKILL, not a COLLECTABLE
/// - Create it as: Right-click → Skill Tree → Skills → Collectable Drop Rate
/// - Place it in your skill tree like any other skill
/// </summary>
[CreateAssetMenu(fileName = "CollectableDropRateSkill", menuName = "Pixelagent/Collectable/Collectable Drop Rate")]
public class CollectableDropRateSkill : Skill
{
    [Header("Drop Rate Settings")]
    [Tooltip("Multiplier for drop rates (1.5 = 50% more drops, 2.0 = 100% more drops)")]
    [SerializeField] private float dropRateMultiplier = 1.5f;
    
    [Tooltip("Does this affect currency drops?")]
    [SerializeField] private bool affectsCurrency = true;
    
    [Tooltip("Does this affect item drops?")]
    [SerializeField] private bool affectsItems = true;

    public float DropRateMultiplier => dropRateMultiplier;

    public new void Unlock()
    {
        base.Unlock();
        
        // You can implement drop rate modifications here
        // For example, modify CurrencyManager or your loot system
        Debug.Log($"Collectable drop rate increased by {(dropRateMultiplier - 1f) * 100}%");
    }

    public new void Reset()
    {
        base.Reset();
        Debug.Log("Collectable drop rate reset to normal");
    }

    /// <summary>
    /// Get the total drop rate multiplier based on current level
    /// Example: Level 1 = 1.5x, Level 2 = 3.0x (if dropRateMultiplier = 1.5)
    /// </summary>
    public float GetScaledDropRate()
    {
        return IsUnlocked ? dropRateMultiplier * CurrentLevel : 1f;
    }
}
