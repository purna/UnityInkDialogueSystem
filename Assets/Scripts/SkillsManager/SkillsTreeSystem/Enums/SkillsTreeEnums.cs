/// <summary>
/// Types of skills in the skill tree
/// </summary>
public enum SkillType
{
    Passive,      // Passive bonuses (e.g., +10% damage)
    Active,       // Active abilities that can be triggered
    Attribute,    // Stat increases (e.g., +5 Strength)
    Unlock,       // Unlocks new features or mechanics
    Upgrade       // Upgrades existing abilities
}

/// <summary>
/// Types of stat modifiers
/// </summary>
public enum StatType
{
    // Combat Stats
    Health,
    MaxHealth,
    Damage,
    AttackSpeed,
    CriticalChance,
    CriticalDamage,
    Defense,
    Armor,
    
    // Movement Stats
    MovementSpeed,
    JumpHeight,
    DashDistance,
    
    // Resource Stats
    Mana,
    MaxMana,
    ManaRegeneration,
    Stamina,
    MaxStamina,
    StaminaRegeneration,
    
    // Attribute Stats
    Strength,
    Dexterity,
    Intelligence,
    Vitality,
    Luck,
    
    // Special Stats
    ExperienceGain,
    GoldGain,
    ItemDropRate,
    SkillCooldownReduction
}

/// <summary>
/// How the modifier is applied to the stat
/// </summary>
public enum ModifierType
{
    Flat,           // Adds a flat amount (e.g., +10 damage)
    Percentage,     // Adds a percentage (e.g., +15% damage)
    Multiplicative  // Multiplies the value (e.g., x1.5 damage)
}