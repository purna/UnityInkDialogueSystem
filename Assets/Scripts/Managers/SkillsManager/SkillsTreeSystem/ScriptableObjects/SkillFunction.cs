using UnityEngine;
using Core.Game;

// ═══════════════════════════════════════════════════════════════════════════
// SKILL FUNCTION — INSTRUCTIONS
// ═══════════════════════════════════════════════════════════════════════════
// PURPOSE:
// - A SkillFunction runs automatically when a Skill is unlocked or leveled.
// - Used to apply gameplay effects: stat boosts, abilities, events, etc.
//
// WHEN TO USE:
// - You want a skill to modify stats (damage, health, speed, etc.)
// - You want a skill to unlock an ability by ID
// - You want a skill to trigger a custom gameplay event
//
// EXAMPLES:
// - StatModifierFunction → +10% damage, +5 max HP, +speed
// - UnlockAbilityFunction → unlocks a player ability like Dash, Double Jump
// - CustomEventFunction → triggers script-defined events for quests or systems
//
// WHAT HAPPENS AT RUNTIME:
// 1. Player unlocks a Skill
// 2. All SkillFunctions attached to the Skill are executed
// 3. Each function applies gameplay effects using SkillTreeManager
//
// HOW TO CREATE:
// 1. Right-click → Skill Tree → Functions → (choose type)
// 2. Fill out the name, description, and parameters
// 3. Assign to a Skill in the Skill's inspector


/// <summary>
/// Base class for skill functions that execute when a skill is unlocked
/// </summary>
public abstract class SkillFunction : ScriptableObject
{
    [SerializeField] private string _functionName;
    [SerializeField, TextArea] private string _description;
    

     public string FunctionName
    {
        get => _functionName;
        set => _functionName = value;
    }


    public string Description
    {
        get => _description;
        set => _description = value;
    }

    
    /// <summary>
    /// Execute the function for a Skill
    /// </summary>
    public abstract void Execute(Skill skill);
}
