using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a single skill node in the skill tree (Updated from your dialogue system)
/// </summary>
[CreateAssetMenu(fileName = "SkillsTree", menuName = "Skill Tree/Skills Tree Node")]
public class SkillsTree : ScriptableObject
{
    [Header("Skill Identity")]
    [SerializeField] private string _name;
    [SerializeField, TextArea] private string _description;
    [SerializeField] private Sprite _icon;
    
    [Header("Skill Properties")]
    [SerializeField] private int _tier;
    [SerializeField] private int _unlockCost;
    [SerializeField] private SkillsType _type;
    [SerializeField] private bool _isStartingSkill;
    
    [Header("Skill Values")]
    [SerializeField] private float _value;
    [SerializeField] private int _maxLevel = 1;
    
    [Header("Prerequisites & Children")]
    [SerializeField] private List<SkillsTree> _prerequisites;
    [SerializeField] private List<SkillsTree> _children;
    
    [Header("Unlock Functions")]
    [SerializeField] private List<SkillFunction> _unlockFunctions;
    [SerializeField] private ExternalFunctionType _externalFunctionType;
    [SerializeField] private string _functionParameter;
    
    [Header("Visual Position")]
    [SerializeField] private Vector2 _position;
    
    // Runtime state (not serialized - resets between sessions)
    private bool _isUnlocked;
    private int _currentLevel;
    
    // Properties
    public string Name => _name;
    public string Description => _description;
    public Sprite Icon => _icon;
    public int Tier => _tier;
    public int UnlockCost => _unlockCost;
    public SkillsType Type => _type;
    public bool IsStartingSkill => _isStartingSkill;
    public float Value => _value;
    public int MaxLevel => _maxLevel;
    public List<SkillsTree> Prerequisites => _prerequisites;
    public List<SkillsTree> Children => _children;
    public List<SkillFunction> UnlockFunctions => _unlockFunctions;
    public ExternalFunctionType ExternalFunctionType => _externalFunctionType;
    public string FunctionParameter => _functionParameter;
    public string CustomFunctionName => _functionParameter; // Backward compatibility
    public Vector2 Position => _position;
    public bool IsUnlocked => _isUnlocked;
    public int CurrentLevel => _currentLevel;
    
    /// <summary>
    /// Initialize the skill with all basic properties
    /// </summary>
    public void Initialize(string name, string description, Sprite icon, int tier, 
                          int unlockCost, SkillsType type, bool isStartingSkill,
                          float value, int maxLevel, Vector2 position)
    {
        _name = name;
        _description = description;
        _icon = icon;
        _tier = tier;
        _unlockCost = unlockCost;
        _type = type;
        _isStartingSkill = isStartingSkill;
        _value = value;
        _maxLevel = maxLevel;
        _position = position;
        
        _prerequisites = new List<SkillsTree>();
        _children = new List<SkillsTree>();
        _unlockFunctions = new List<SkillFunction>();
        
        _isUnlocked = false;
        _currentLevel = 0;
    }
    
    /// <summary>
    /// Initialize with prerequisites and children
    /// </summary>
    public void InitializeWithConnections(string name, string description, Sprite icon, int tier,
                                         int unlockCost, SkillsType type, bool isStartingSkill,
                                         float value, int maxLevel, Vector2 position,
                                         List<SkillsTree> prerequisites, List<SkillsTree> children)
    {
        Initialize(name, description, icon, tier, unlockCost, type, isStartingSkill, value, maxLevel, position);
        
        _prerequisites = prerequisites ?? new List<SkillsTree>();
        _children = children ?? new List<SkillsTree>();
    }
    
    /// <summary>
    /// Initialize with functions
    /// </summary>
    public void InitializeWithFunctions(string name, string description, Sprite icon, int tier,
                                       int unlockCost, SkillsType type, bool isStartingSkill,
                                       float value, int maxLevel, Vector2 position,
                                       List<SkillFunction> unlockFunctions,
                                       ExternalFunctionType externalFunctionType, string functionParameter)
    {
        Initialize(name, description, icon, tier, unlockCost, type, isStartingSkill, value, maxLevel, position);
        
        _unlockFunctions = unlockFunctions ?? new List<SkillFunction>();
        _externalFunctionType = externalFunctionType;
        _functionParameter = functionParameter;
    }
    
    /// <summary>
    /// Add a prerequisite skill
    /// </summary>
    public void AddPrerequisite(SkillsTree prerequisite)
    {
        if (prerequisite != null && !_prerequisites.Contains(prerequisite))
        {
            _prerequisites.Add(prerequisite);
        }
    }
    
    /// <summary>
    /// Add a child skill
    /// </summary>
    public void AddChild(SkillsTree child)
    {
        if (child != null && !_children.Contains(child))
        {
            _children.Add(child);
            // Automatically add this as a prerequisite to the child
            child.AddPrerequisite(this);
        }
    }
    
    /// <summary>
    /// Check if this skill can be unlocked
    /// </summary>
    public bool CanUnlock()
    {
        // If already at max level, can't unlock more
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
    
    /// <summary>
    /// Unlock this skill
    /// </summary>
    public void Unlock()
    {
        if (!_isUnlocked)
        {
            _isUnlocked = true;
            _currentLevel = 1;
            ExecuteFunctions();
            
            Debug.Log($"[SkillsTree] Unlocked: {_name}");
        }
    }
    
    /// <summary>
    /// Level up this skill
    /// </summary>
    public void LevelUp()
    {
        if (_isUnlocked && _currentLevel < _maxLevel)
        {
            _currentLevel++;
            ExecuteFunctions();
            
            Debug.Log($"[SkillsTree] Leveled up: {_name} to level {_currentLevel}");
        }
    }
    
    /// <summary>
    /// Reset this skill to locked state
    /// </summary>
    public void Reset()
    {
        _isUnlocked = false;
        _currentLevel = 0;
    }
    
    /// <summary>
    /// Execute all unlock functions
    /// </summary>
    private void ExecuteFunctions()
    {
        foreach (var function in _unlockFunctions)
        {
            if (function != null)
            {
                function.Execute(this);
            }
        }
        /*
        // Execute external function if set
        if (_externalFunctionType != ExternalFunctionType.None)
        {
            ExecuteExternalFunction();
        }
        */
    }
    
    /// <summary>
    /// Execute external function (custom implementation)
    /// </summary>
    private void ExecuteExternalFunction()
    {
        // This can be expanded based on your ExternalFunctionType enum
        Debug.Log($"[SkillsTree] Executing external function: {_externalFunctionType} with parameter: {_functionParameter}");
        
        // You can add custom logic here or trigger events
        SkillTreeManager.Instance?.TriggerCustomEvent(_externalFunctionType.ToString(), _functionParameter, (object)this);
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
    
    /// <summary>
    /// Get all skills that are locked behind this one
    /// </summary>
    public List<SkillsTree> GetLockedChildren()
    {
        List<SkillsTree> lockedChildren = new List<SkillsTree>();
        
        foreach (var child in _children)
        {
            if (child != null && !child.IsUnlocked)
            {
                lockedChildren.Add(child);
            }
        }
        
        return lockedChildren;
    }
    
    /// <summary>
    /// Check if all prerequisites are met
    /// </summary>
    public bool ArePrerequisitesMet()
    {
        foreach (var prereq in _prerequisites)
        {
            if (prereq != null && !prereq.IsUnlocked)
                return false;
        }
        
        return true;
    }
}

