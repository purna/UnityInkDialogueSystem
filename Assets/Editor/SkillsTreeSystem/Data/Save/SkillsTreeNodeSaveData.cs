using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SkillsTreeNodeSaveData
{
    [SerializeField] private string _ID;
    [SerializeField] private string _name;
    [SerializeField] private string _text;
    [SerializeField] private List<SkillsTreeChoiceSaveData> _choices;
    [SerializeField] private string _groupID;
    [SerializeField] private SkillsTreeType _skillstreeType;
    [SerializeField] private Vector2 _position;
    
    // Skill-specific data
    [Header("Skill Properties")]
    [SerializeField] private string _skillAssetPath; // Path to the Skill ScriptableObject
    [SerializeField] private Sprite _icon;
    [SerializeField] private Sprite _lockedIcon;
    [SerializeField] private Sprite _unlockedIcon;
    [SerializeField] private string _description;
    [SerializeField] private int _tier;
    [SerializeField] private int _unlockCost;
    [SerializeField] private float _value;
    [SerializeField] private int _maxLevel;
    
    // Prerequisites and Children (stored as IDs)
    [SerializeField] private List<string> _prerequisiteIDs;
    [SerializeField] private List<string> _childIDs;
    
    // Variable modification/condition fields (if still needed)
    [SerializeField] private string _variableName;
    [SerializeField] private VariableDataType _variableType;
    [SerializeField] private ModificationType _modificationType;
    [SerializeField] private ConditionType _conditionType;
    
    // Properties
    public string ID => _ID;
    public string Name => _name;
    public string Text => _text;
    public IEnumerable<SkillsTreeChoiceSaveData> Choices => _choices;
    public string GroupID => _groupID;
    public SkillsTreeType SkillsTreeType => _skillstreeType;
    public Vector2 Position => _position;
    
    // Skill properties
    public string SkillAssetPath => _skillAssetPath;
    public Sprite Icon => _icon;
    public Sprite LockedIcon => _lockedIcon;
    public Sprite UnlockedIcon => _unlockedIcon;
    public string Description => _description;
    public int Tier => _tier;
    public int UnlockCost => _unlockCost;
    public float Value => _value;
    public int MaxLevel => _maxLevel;
    public List<string> PrerequisiteIDs => _prerequisiteIDs;
    public List<string> ChildIDs => _childIDs;
    
    // Constructor
    public SkillsTreeNodeSaveData(string id, string name,  string text, List<SkillsTreeChoiceSaveData> choices, 
                                 string groupID, SkillsTreeType skillstreeType, Vector2 position)
    {
        _ID = id;
        _name = name;
        _text = text;
        _choices = choices;
        _groupID = groupID;
        _skillstreeType = skillstreeType;
        _position = position;
        
        // Initialize skill-specific data
        _skillAssetPath = "";
        _icon = null;
        _description = "";
        _tier = 0;
        _unlockCost = 1;
        _value = 0f;
        _maxLevel = 1;
        _prerequisiteIDs = new List<string>();
        _childIDs = new List<string>();
    }
    
    /// <summary>
    /// Update skill data from a Skill ScriptableObject
    /// </summary>
    public void UpdateFromSkill(Skill skill, string assetPath = "")
    {
        if (skill == null) return;
        
        _skillAssetPath = assetPath;
        _icon = skill.Icon;
        _description = skill.Description;
        _tier = skill.Tier;
        _unlockCost = skill.UnlockCost;
        _value = skill.Value;
        _maxLevel = skill.MaxLevel;
        
        // Note: Prerequisites and children would need to be set separately
        // since we're storing them as IDs
    }
    
    /// <summary>
    /// Update skill data from a SkillsTree ScriptableObject
    /// </summary>
    public void UpdateFromSkillsTree(Skill skillsTree, string assetPath = "")
    {
        if (skillsTree == null) return;
        
        _skillAssetPath = assetPath;
        _icon = skillsTree.Icon;
        _lockedIcon = skillsTree.LockedIcon;     
        _unlockedIcon = skillsTree.UnlockedIcon; 
        _description = skillsTree.Description;
        _tier = skillsTree.Tier;
        _unlockCost = skillsTree.UnlockCost;
        _value = skillsTree.Value;
        _maxLevel = skillsTree.MaxLevel;
        
        // Update prerequisites and children IDs
        _prerequisiteIDs.Clear();
        foreach (var prereq in skillsTree.Prerequisites)
        {
            if (prereq != null)
            {
                // You'll need to implement a way to get the node ID from a SkillsTree reference
                // For now, we'll store the name as a temporary solution
                _prerequisiteIDs.Add(prereq.name);
            }
        }
        
        _childIDs.Clear();
        foreach (var child in skillsTree.Children)
        {
            if (child != null)
            {
                _childIDs.Add(child.name);
            }
        }
    }
    
    /// <summary>
    /// Update visual properties (icon and description)
    /// </summary>
    public void UpdateVisualData(Sprite icon, Sprite lockedIcon, Sprite unlockedIcon, string description)
    {
        _icon = icon;
        _lockedIcon = lockedIcon;
        _unlockedIcon = unlockedIcon;
        _description = description;
        _text = description; // Keep text in sync
    }
    
    /// <summary>
    /// Update skill properties
    /// </summary>
    public void UpdateSkillProperties(int tier, int unlockCost, float value, int maxLevel)
    {
        _tier = tier;
        _unlockCost = unlockCost;
        _value = value;
        _maxLevel = maxLevel;
    }
    
    /// <summary>
    /// Add a prerequisite ID
    /// </summary>
    public void AddPrerequisiteID(string prerequisiteID)
    {
        if (!_prerequisiteIDs.Contains(prerequisiteID))
        {
            _prerequisiteIDs.Add(prerequisiteID);
        }
    }
    
    /// <summary>
    /// Add a child ID
    /// </summary>
    public void AddChildID(string childID)
    {
        if (!_childIDs.Contains(childID))
        {
            _childIDs.Add(childID);
        }
    }
    
    /// <summary>
    /// Remove a prerequisite ID
    /// </summary>
    public void RemovePrerequisiteID(string prerequisiteID)
    {
        _prerequisiteIDs.Remove(prerequisiteID);
    }
    
    /// <summary>
    /// Remove a child ID
    /// </summary>
    public void RemoveChildID(string childID)
    {
        _childIDs.Remove(childID);
    }
    
    /// <summary>
    /// Clear all prerequisites
    /// </summary>
    public void ClearPrerequisites()
    {
        _prerequisiteIDs.Clear();
    }
    
    /// <summary>
    /// Clear all children
    /// </summary>
    public void ClearChildren()
    {
        _childIDs.Clear();
    }
}