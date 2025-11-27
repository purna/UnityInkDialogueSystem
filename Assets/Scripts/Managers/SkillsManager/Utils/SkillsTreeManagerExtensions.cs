using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Game;


// ============================================================
// EXAMPLE SKILL TREE SETUP
// ============================================================

/*
RECOMMENDED SKILL TREE STRUCTURE:

TIER 1 (Basic Unlocks):
├─ [Explosive Expert] → Unlocks Bomb upgrade
├─ [Defensive Training] → Unlocks Shield upgrade
├─ [Stealth Training] → Unlocks Invisibility upgrade
└─ [Arcane Knowledge] → Unlocks Staff upgrade

TIER 2 (Enhancements):
├─ [Bomb Capacity I] → +2 Bombs (requires Explosive Expert)
├─ [Longer Fuse] → +5s countdown (requires Explosive Expert)
├─ [Reinforced Shield] → +1s shield duration (requires Defensive Training)
└─ [Shadow Step] → +0.5s invisibility (requires Stealth Training)

TIER 3 (Advanced):
├─ [Bomb Capacity II] → +3 more Bombs (requires Bomb Capacity I)
├─ [Master Bomber] → Bombs explode twice (requires Tier 2)
└─ [Aegis Shield] → Shield blocks all damage (requires Reinforced Shield)

WORKFLOW:
1. Player finds "Shield" collectible in world → gets it added to inventory
2. Player can't USE shield yet - it shows as "Locked - Requires Defensive Training"
3. Player unlocks "Defensive Training" skill → Shield becomes usable
4. Player can now equip and use Shield with E key
5. Player unlocks "Reinforced Shield" → Shield duration increases
*/

// ============================================================
// HELPER: Add to SkillsTreeManager
// ============================================================

public static class SkillsTreeManagerExtensions
{
    /// <summary>
    /// Extension method to find a Skill by name via the Manager.
    /// Since the data is held in the Controller/Container, we bridge the connection here.
    /// </summary>
    public static Skill GetSkillByName(this SkillsTreeManager instance, string skillName)
    {
        // Validate input parameters
        if (instance == null)
        {
            Debug.LogError("[SkillsTreeManagerExtensions] SkillsTreeManager instance is null!");
            return null;
        }

        if (string.IsNullOrEmpty(skillName))
        {
            Debug.LogWarning("[SkillsTreeManagerExtensions] Skill name is null or empty!");
            return null;
        }

        // 1. Try to find the active SkillsTreeController in the scene
        // (The Controller holds the reference to the Container/ScriptableObject)
        SkillsTreeController controller = Object.FindObjectOfType<SkillsTreeController>();

        if (controller != null)
        {
            // Use the Controller's existing method which queries the Container
            Skill foundSkill = controller.GetSkillByName(skillName);
            
            if (foundSkill == null)
            {
                Debug.LogWarning($"[SkillsTreeManagerExtensions] Skill '{skillName}' not found in SkillsTreeController.");
            }
            
            return foundSkill;
        }

        // 2. Fallback: If no controller is found (e.g. scene loading issues)
        Debug.LogWarning($"[SkillsTreeManagerExtensions] Could not find 'SkillsTreeController' in scene to look up skill: '{skillName}'. " +
                        "Make sure a SkillsTreeController is present in the scene.");
        return null;
    }
}