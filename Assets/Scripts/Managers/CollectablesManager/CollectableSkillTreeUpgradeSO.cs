// ═══════════════════════════════════════════════════════════════════════════
// COLLECTABLE TYPE 4: SKILL TREE UPGRADE (Auto-Unlock)
// ═══════════════════════════════════════════════════════════════════════════
using UnityEngine;
using Core.Game;

/// <summary>
/// Skill Tree Upgrade - Automatically unlocks a specific skill OR gives skill points
/// 
/// PURPOSE:
/// - Instantly grants a specific skill to the player (no spending needed)
/// - Can also give skill points as a bonus
/// - Great for story progression or forced skill unlocks
/// - Bypasses normal skill tree progression
/// 
/// WHEN TO USE:
/// - Tutorial rewards (give first skill automatically)
/// - Story-critical skills (player MUST have this skill)
/// - Boss rewards (instant skill unlock)
/// - Special event rewards
/// - Progression gates (skill needed to continue)
/// 
/// EXAMPLE USES:
/// - "Dash Ability Scroll" - Instantly unlocks Dash skill
/// - "Knowledge Crystal" - Gives 3 skill points + unlocks a skill
/// - "Master's Gift" - Auto-unlocks a powerful skill
/// - "Training Manual" - Gives 5 skill points only
/// 
/// TWO MODES:
/// 
/// MODE 1: AUTO-UNLOCK SKILL
/// - Set Skill To Unlock = drag skill SO
/// - Set Auto Unlock Skill = true
/// - Player gets that skill immediately (if prerequisites met)
/// - Useful for: story skills, tutorial rewards
/// 
/// MODE 2: GRANT SKILL POINTS ONLY
/// - Set Skill Points To Grant = 5
/// - Set Auto Unlock Skill = false
/// - Just gives skill points, no auto-unlock
/// - Useful for: bonus rewards, hidden collectables
/// 
/// MODE 3: BOTH
/// - Give skill points AND auto-unlock a skill
/// - Great for: major milestones, boss rewards
/// 
/// WHAT HAPPENS ON COLLECT:
/// 1. Adds to inventory (for tracking)
/// 2. Gives skill points (if specified)
/// 3. Auto-unlocks skill (if enabled and prerequisites met)
/// 4. Plays visual/audio effect
/// 5. Destroys the pickup
/// 
/// IMPORTANT NOTES:
/// - Auto-unlock respects prerequisites (won't unlock if prereqs not met)
/// - Auto-unlock DOES NOT bypass skill point cost (pays the cost)
/// - If player doesn't have enough points, unlock fails
/// - Use "Skill Name To Unlock" if you don't have direct SO reference
/// </summary>
[CreateAssetMenu(menuName = "Pixelagent/Collectable/Skill Tree Upgrade", fileName = "New Skill Tree Upgrade")]
public class CollectableSkillTreeUpgradeSO : CollectableSOBase
{
    [Header("Skill Integration")]
    [Tooltip("Direct reference to skill to unlock (drag Skill SO here)")]
    [SerializeField] private Skill skillToUnlock;
    
    [Tooltip("Alternative: name of skill to unlock (used if Skill To Unlock is empty)")]
    [SerializeField] private string skillNameToUnlock;
    
    [Tooltip("How many skill points to give (0 = none, can be used with auto-unlock)")]
    [SerializeField] private int skillPointsToGrant = 0;
    
    [Tooltip("If true, automatically unlocks the skill when collected (if prerequisites met)")]
    [SerializeField] private bool autoUnlockSkill = true;

    public override void Collect(GameObject objectThatCollected)
    {
        // Add to inventory (for tracking)
        InventoryManager.Instance.AddItem(this);

        // Grant skill points if specified (can be used alone or with auto-unlock)
        if (skillPointsToGrant > 0 && SkillsTreeManager.Instance != null)
        {
            SkillsTreeManager.Instance.AddSkillPoints(skillPointsToGrant);
            Debug.Log($"Granted {skillPointsToGrant} skill points");
        }

        // Auto-unlock skill if specified
        if (autoUnlockSkill)
        {
            Skill targetSkill = skillToUnlock;
            
            // If no direct reference, try to find by name
            if (targetSkill == null && !string.IsNullOrEmpty(skillNameToUnlock))
            {
                targetSkill = SkillsTreeManager.Instance?.SkillTreeContainer?.GetSkillByName(skillNameToUnlock);
            }
            
            if (targetSkill != null && SkillsTreeManager.Instance != null)
            {
                // Check if skill can be unlocked (prerequisites met)
                if (targetSkill.CanUnlock())
                {
                    // Try to unlock through manager (which handles point costs)
                    if (SkillsTreeManager.Instance.TryUnlockSkill(targetSkill))
                    {
                        Debug.Log($"Auto-unlocked skill: {targetSkill.SkillName}");
                    }
                    else
                    {
                        Debug.LogWarning($"Could not auto-unlock {targetSkill.SkillName} - not enough skill points");
                    }
                }
                else
                {
                    Debug.LogWarning($"Could not auto-unlock {targetSkill.SkillName} - prerequisites not met");
                }
            }
            else
            {
                Debug.LogWarning("No skill specified for auto-unlock");
            }
        }

        // Play collection visual/audio feedback
        if (_playerEffects == null)
            GetReference(objectThatCollected);
        _playerEffects.PlayCollectionEffect(CollectionFlashTime, CollectColor, CollectionClip);
    }
}
