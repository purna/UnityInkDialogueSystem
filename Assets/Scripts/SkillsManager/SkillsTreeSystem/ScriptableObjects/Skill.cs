using System.Collections.Generic;
using UnityEngine;
using Core.Game;


// ═══════════════════════════════════════════════════════════════════════════
// SKILL SCRIPTABLE OBJECT — INSTRUCTIONS
// ═══════════════════════════════════════════════════════════════════════════
// PURPOSE:
// - Defines a single skill node in the Skill Tree.
// - Handles prerequisites, child links, icons, unlocking, leveling, and runtime values.
//
// WHEN TO USE:
// - Creating passive skills (e.g., +10% Damage, +5 Max Health)
// - Creating active ability unlocks
// - Creating skill progression nodes in a skill tree editor
// - Boss unlock nodes / branch-connectors
//
// EXAMPLE USES:
// - Health Upgrade: Value = 10, MaxLevel = 5
// - Fireball Unlock: MaxLevel = 1, SkillType = Active
// - Ranger Tier Progression Node: Tier = 2, prerequisites link Tier 1
//
// WHAT HAPPENS AT RUNTIME:
// 1. Skill starts locked until prerequisites are met.
// 2. Unlock() sets the skill to unlocked and executes all SkillFunctions.
// 3. LevelUp() increases skill level and reapplies functions.
// 4. Reset() clears state for save/load systems.
//
// HOW TO CREATE A NEW SKILL:
// 1. Right‑click in Project Window → Create → Skill Tree → Skill
// 2. Fill out:
// • Identity (Name, Description, Icons)
// • Properties (Tier, Cost, Type, Value, Max Level)
// • Prerequisites & Children
// • Unlock Functions
// • Visual Position (for skill tree editors)
// 3. Test unlocking behavior inside your skill tree UI.


/// <summary>
/// Represents a single skill node in the skill tree
/// </summary>
[CreateAssetMenu(fileName = "Skill", menuName = "Skill Tree/Skill")]
public class Skill : ScriptableObject
{
    [Header("Skill Identity")]
    [SerializeField] private string _skillName;
    [SerializeField, TextArea] private string _description;
    [SerializeField] private Sprite _icon;
    [SerializeField] private Sprite _lockedIcon;
    [SerializeField] private Sprite _unlockedIcon;
    
    [Header("Skill Properties")]
    [SerializeField] private int _tier;
    [SerializeField] private int _unlockCost;
    [SerializeField] private SkillType _skillType;
    
    [Header("Skill Values")]
    [SerializeField] private float _value;
    [SerializeField] private int _maxLevel = 1;
    
    [Header("Prerequisites")]
    [SerializeField] private List<Skill> _prerequisites;
    
    [Header("Children")]
    [SerializeField] private List<Skill> _children;
    
    [Header("Unlock Functions")]
    [SerializeField] private List<SkillFunction> _unlockFunctions;
    
    [Header("Visual")]
    [SerializeField] private Vector2 _position;
    
    [Header("Collectable Requirements")]
    [SerializeField] private bool _requiresSpecialKey = false;
    [SerializeField] private string _requiredKeyName; // ItemName of the CollectableSkillTreeKeySO

    [Header("Unlock Requirements")]
    [SerializeField] private bool requiresSpecialKey = false;
    [SerializeField] private string requiredKeyName; // Name of the CollectableSkillTreeKeySO ItemName
    
    public bool RequiresSpecialKey => requiresSpecialKey;
    public string RequiredKeyName => requiredKeyName;
    
    // Runtime state
    private bool _isUnlocked;
    private int _currentLevel;
    
    // Properties
    public string SkillName => _skillName;
    public string Description => _description;
    public Sprite Icon => _icon;
    public Sprite LockedIcon => _lockedIcon;
    public Sprite UnlockedIcon => _unlockedIcon;
    public int Tier => _tier;
    public int UnlockCost => _unlockCost;
    public SkillType SkillType => _skillType;
    public float Value => _value;
    public int MaxLevel => _maxLevel;
    public List<Skill> Prerequisites => _prerequisites;
    public List<Skill> Children => _children;
    public List<SkillFunction> UnlockFunctions
    {
        get => _unlockFunctions;
        protected set => _unlockFunctions = value;
    }
    public Vector2 Position => _position;
    public bool IsUnlocked => _isUnlocked;
    public int CurrentLevel => _currentLevel;
    
    public void Initialize(string skillName, string description, Sprite icon, 
                          Sprite lockedIcon, Sprite unlockedIcon,
                          int tier, int unlockCost, SkillType skillType, float value, int maxLevel,
                          List<Skill> prerequisites, List<Skill> children, 
                          List<SkillFunction> unlockFunctions, Vector2 position)
    {
        _skillName = skillName;
        _description = description;
        _icon = icon;
        _lockedIcon = lockedIcon;
        _unlockedIcon = unlockedIcon;
        _tier = tier;
        _unlockCost = unlockCost;
        _skillType = skillType;
        _value = value;
        _maxLevel = maxLevel;
        _prerequisites = prerequisites ?? new List<Skill>();
        _children = children ?? new List<Skill>();
        _unlockFunctions = unlockFunctions ?? new List<SkillFunction>();
        _position = position;
        _isUnlocked = false;
        _currentLevel = 0;
    }
    
    public bool CanUnlock()
    {
        if (_isUnlocked && _currentLevel >= _maxLevel)
            return false;
            
        // Check if all prerequisites are unlocked
        foreach (var prereq in _prerequisites)
        {
            if (prereq != null && !prereq.IsUnlocked)
                return false;
        }
        
        return true;
    }
    
    public virtual void Unlock()
    {
        if (!_isUnlocked)
        {
            _isUnlocked = true;
            _currentLevel = 1;
            OnUnlock();
            ExecuteFunctions();
        }
    }
    
    public virtual void LevelUp()
    {
        if (_isUnlocked && _currentLevel < _maxLevel)
        {
            _currentLevel++;
            OnUnlock();
            ExecuteFunctions();
        }
    }
    
    public virtual void Reset()
    {
        _isUnlocked = false;
        _currentLevel = 0;
        OnLock();
    }
    
    private void ExecuteFunctions()
    {
        foreach (var function in _unlockFunctions)
        {
            if (function != null)
            {
                function.Execute(this);
            }
        }
    }
    /// <summary>
    /// Called when the skill is unlocked (virtual for override)
    /// </summary>
    protected virtual void OnUnlock()
    {
        // Default implementation - can be overridden by derived classes
    }

    /// <summary>
    /// Called when the skill is locked/reset (virtual for override)
    /// </summary>
    protected virtual void OnLock()
    {
        // Default implementation - can be overridden by derived classes
    }
    
    /// <summary>
    /// Gets the skill value scaled by current level
    /// </summary>
    public float GetScaledValue()
    {
        return _value * _currentLevel;
    }

    /// <summary>
    /// Gets the base skill value without level scaling
    /// </summary>
    public float GetBaseValue()
    {
        return _value;
    }
    
    public void UpdateName(string newName)
    {
        _skillName = newName;
    }

    public void UpdateDescription(string newDescription)
    {
        _description = newDescription;
    }

    public void UpdatePosition(Vector2 newPosition)
    {
        _position = newPosition;
    }

    // ===== COLLECTABLE INTEGRATION METHODS =====
    
    /// <summary>
    /// Check if player has enough skill points to unlock/level up
    /// </summary>
    public bool HasEnoughSkillPoints()
    {
        if (SkillTreeManager.Instance == null)
            return false;

        return SkillTreeManager.Instance.CurrentSkillPoints >= _unlockCost;
    }

    /// <summary>
    /// Check if player has the required key in inventory
    /// </summary>
    public bool HasRequiredKey()
    {
        if (!_requiresSpecialKey || string.IsNullOrEmpty(_requiredKeyName))
            return true; // No key required

        if (InventoryManager.Instance == null)
            return false;

        // Check if player has the key in their inventory by ItemName
        return InventoryManager.Instance.HasItem(_requiredKeyName);
    }

    /// <summary>
    /// Check if this skill can be afforded (both points and keys)
    /// </summary>
    public bool CanAfford()
    {
        return HasEnoughSkillPoints() && HasRequiredKey();
    }

    /// <summary>
    /// Attempt to unlock this skill through SkillTreeManager
    /// </summary>
    public bool TryUnlockWithManager()
    {
        if (!CanUnlock())
        {
            Debug.LogWarning($"Cannot unlock {_skillName} - prerequisites not met");
            return false;
        }

        if (!CanAfford())
        {
            if (!HasEnoughSkillPoints())
                Debug.LogWarning($"Not enough skill points for {_skillName}");
            else if (!HasRequiredKey())
                Debug.LogWarning($"Missing required key '{_requiredKeyName}' for {_skillName}");
            return false;
        }

        // Use SkillTreeManager to unlock (handles point deduction)
        if (SkillTreeManager.Instance != null)
        {
            return SkillTreeManager.Instance.TryUnlockSkill(this);
        }

        return false;
    }

    /// <summary>
    /// Get the actual cost for this skill
    /// </summary>
    public int GetActualCost()
    {
        return _unlockCost;
    }
}