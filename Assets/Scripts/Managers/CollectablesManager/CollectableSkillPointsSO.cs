

// ═══════════════════════════════════════════════════════════════════════════
// COLLECTABLE TYPE 2: SKILL POINTS
// ═══════════════════════════════════════════════════════════════════════════
using UnityEngine;
using Core.Game;

/// <summary>
/// Skill Points - Currency for unlocking skills in the skill tree
/// 
/// PURPOSE:
/// - Gives player skill points to spend in the skill tree
/// - Skill points are managed by SkillTreeManager
/// - DOES add to inventory for tracking purposes
/// 
/// WHEN TO USE:
/// - As rewards for completing objectives
/// - Hidden in secret areas
/// - Boss/mini-boss rewards
/// - Quest completion rewards
/// - Level-up rewards
/// 
/// EXAMPLE USES:
/// - Basic Skill Point: skillPointsAmount = 1
/// - Rare Skill Orb: skillPointsAmount = 3
/// - Epic Knowledge Tome: skillPointsAmount = 5
/// 
/// WHAT HAPPENS ON COLLECT:
/// 1. Adds skill points to SkillTreeManager (player can spend these)
/// 2. Adds to inventory (for tracking/statistics)
/// 3. Plays visual/audio effect
/// 4. Destroys the pickup
/// 
/// IMPORTANT:
/// - Unlike currency, these are tracked in inventory
/// - Player spends these to unlock skills
/// - Check SkillTreeManager.CurrentSkillPoints to see total
/// </summary>
[CreateAssetMenu(menuName = "Pixelagent/Collectable/Skill Points", fileName = "New Skill Points Collectable")]
public class CollectableSkillPointsSO : CollectableSOBase
{
    [Header("Skill Points Stats")]
    [Tooltip("How many skill points this collectable gives (1 = normal, 3 = rare, 5 = epic)")]
    [SerializeField] private int skillPointsAmount = 1;

    public override void Collect(GameObject objectThatCollected)
    {
        // Add skill points via SkillTreeManager (player can spend these in skill tree)
        if (SkillsTreeManager.Instance != null)
        {
            SkillsTreeManager.Instance.AddSkillPoints(skillPointsAmount);
        }

        // Add to inventory for tracking (shows in inventory, statistics, etc.)
        InventoryManager.Instance.AddItem(this);

        // Play collection visual/audio feedback
        if (_playerEffects == null)
            GetReference(objectThatCollected);
        _playerEffects.PlayCollectionEffect(CollectionFlashTime, CollectColor, CollectionClip);
    }
}
