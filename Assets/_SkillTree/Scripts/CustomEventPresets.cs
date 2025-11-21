using UnityEngine;
using Core.Game;

/// <summary>
/// Predefined SkillFunction presets for common custom events
/// </summary>
public static class CustomEventPresets
{
    public static CustomEventFunction OnEnemyKilled()
    {
        var func = ScriptableObject.CreateInstance<CustomEventFunction>();
        func.name = "OnEnemyKilled Event";
        func.EventName = "OnEnemyKilled";
        func.EventParameter = "Give100XP";
        func.Description = "Triggered when the player kills an enemy. Example: awards 100 XP or triggers a kill streak effect.";
        return func;
    }

    public static CustomEventFunction OnItemCollected()
    {
        var func = ScriptableObject.CreateInstance<CustomEventFunction>();
        func.name = "OnItemCollected Event";
        func.EventName = "OnItemCollected";
        func.EventParameter = "SpeedBoost_10s";
        func.Description = "Triggered when the player collects an item. Example: grants a temporary 10-second speed boost.";
        return func;
    }

    public static CustomEventFunction OnLevelUp()
    {
        var func = ScriptableObject.CreateInstance<CustomEventFunction>();
        func.name = "OnLevelUp Event";
        func.EventName = "OnLevelUp";
        func.EventParameter = "Grant5SkillPoints";
        func.Description = "Triggered when the player levels up. Example: grants 5 skill points automatically.";
        return func;
    }

    public static CustomEventFunction OnBossDefeated()
    {
        var func = ScriptableObject.CreateInstance<CustomEventFunction>();
        func.name = "OnBossDefeated Event";
        func.EventName = "OnBossDefeated";
        func.EventParameter = "DropRareLootBonus";
        func.Description = "Triggered when a boss is defeated. Example: grants a rare loot bonus or unlocks a special ability.";
        return func;
    }
}
