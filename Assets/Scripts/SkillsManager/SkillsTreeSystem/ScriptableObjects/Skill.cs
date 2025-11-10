using System.Collections.Generic;
using UnityEngine;

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
    public List<SkillFunction> UnlockFunctions => _unlockFunctions;
    public Vector2 Position => _position;
    public bool IsUnlocked => _isUnlocked;
    public int CurrentLevel => _currentLevel;
    
public void Initialize(string skillName, string description, Sprite icon, 
                      Sprite lockedIcon, Sprite unlockedIcon, // ADD THESE
                      int tier, int unlockCost, SkillType skillType, float value, int maxLevel,
                      List<Skill> prerequisites, List<Skill> children, 
                      List<SkillFunction> unlockFunctions, Vector2 position)
{
    _skillName = skillName;
    _description = description;
    _icon = icon;
    _lockedIcon = lockedIcon;     // ADD THIS
    _unlockedIcon = unlockedIcon; // ADD THIS
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
    
    public void Unlock()
    {
        if (!_isUnlocked)
        {
            _isUnlocked = true;
            _currentLevel = 1;
            ExecuteFunctions();
        }
    }
    
    public void LevelUp()
    {
        if (_isUnlocked && _currentLevel < _maxLevel)
        {
            _currentLevel++;
            ExecuteFunctions();
        }
    }
    
    public void Reset()
    {
        _isUnlocked = false;
        _currentLevel = 0;
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
}

