// ═══════════════════════════════════════════════════════════════════════════
// COLLECTABLE TYPE 3: SKILL TREE KEY
// ═══════════════════════════════════════════════════════════════════════════
using UnityEngine;
using Core.Game;

/// <summary>
/// Skill Tree Key - Unlocks gated skill branches or specific skills
/// 
/// PURPOSE:
/// - Acts as a "gate" for advanced skill branches
/// - Player must find the key before they can unlock certain skills
/// - Stays in inventory permanently (not consumed)
/// - Can reference a specific skill group OR just be checked by name
/// 
/// WHEN TO USE:
/// - Lock advanced/powerful skills behind exploration
/// - Gate end-game abilities
/// - Reward for completing dungeons/areas
/// - Quest rewards for major storylines
/// - Secret collectables
/// 
/// EXAMPLE USES:
/// - "Combat Manual" - Unlocks Combat skill branch
/// - "Ancient Tome" - Unlocks Magic skill branch
/// - "Master's License" - Required for tier 3 skills
/// - "Detection Blueprint" - Unlocks radar/detection skills
/// 
/// HOW IT WORKS:
/// 1. Player collects key → Goes to inventory
/// 2. Player opens skill tree → Sees locked skill with key icon
/// 3. If player has key in inventory → Can unlock that skill
/// 4. Key stays in inventory (not consumed when skill unlocks)
/// 
/// SETUP OPTIONS:
/// Option A: Set Target Group - Unlocks entire skill branch
/// Option B: Set Skill Tree Group Name - Skills check for this name
/// Option C: Leave both blank - Skills check for ItemName
/// 
/// IN SKILLS:
/// - Set "Requires Special Key" = true
/// - Set "Required Key Name" = "Combat Manual" (matches this ItemName)
/// 
/// WHAT HAPPENS ON COLLECT:
/// 1. Adds key to inventory (permanent)
/// 2. Shows message about what was unlocked
/// 3. Plays visual/audio effect
/// 4. Destroys the pickup
/// 5. Key can now be checked by skills in skill tree
/// </summary>
[CreateAssetMenu(menuName = "Pixelagent/Collectable/Skill Tree Key", fileName = "New Skill Tree Key")]
public class CollectableSkillTreeKeySO : CollectableSOBase
{
    [Header("Skill Tree Key Stats")]
    [Tooltip("Name of the skill branch this unlocks (optional - can use ItemName instead)")]
    [SerializeField] private string skillTreeGroupName; // Which skill branch to unlock
    
    [Tooltip("Reference to SkillsTreeGroup this unlocks (optional - more direct connection)")]
    [SerializeField] private SkillsTreeGroup targetGroup; // Reference to the group
    
    public string SkillTreeGroupName => skillTreeGroupName;
    public SkillsTreeGroup TargetGroup => targetGroup;

    public override void Collect(GameObject objectThatCollected)
    {
        // Add to inventory - the key stays in inventory for later use
        // Skills will check: InventoryManager.Instance.HasItem("Combat Manual")
        InventoryManager.Instance.AddItem(this);
        
        // Optionally show a message that branch is now available
        if (targetGroup != null)
        {
            Debug.Log($"Acquired key to unlock: {targetGroup.GroupName} skill branch!");
        }
        else if (!string.IsNullOrEmpty(skillTreeGroupName))
        {
            Debug.Log($"Acquired key to unlock: {skillTreeGroupName} skill branch!");
        }
        else
        {
            Debug.Log($"Acquired key: {ItemName}");
        }

        // Play collection visual/audio feedback
        if (_playerEffects == null)
            GetReference(objectThatCollected);
        _playerEffects.PlayCollectionEffect(CollectionFlashTime, CollectColor, CollectionClip);
    }
}