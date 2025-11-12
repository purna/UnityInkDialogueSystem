using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Container that holds all skills in a skill tree
/// </summary>
[CreateAssetMenu(fileName = "SkillsTreeContainer", menuName = "Skill Tree/Container")]
public class SkillsTreeContainer : ScriptableObject
{
    [Header("Skill Tree Details")]
    [SerializeField] private string _treeName;
    [SerializeField, TextArea] private string _description;
    
    [Header("Skill Tree Groups")]
    [SerializeField] private SerializableDictionary<SkillsTreeGroup, List<Skill>> _groups;
    [SerializeField] private List<Skill> _ungroupedSkills;
    
    public string TreeName => _treeName;
    public string Description => _description;
    public SerializableDictionary<SkillsTreeGroup, List<Skill>> Groups => _groups;
    public List<Skill> UngroupedSkills => _ungroupedSkills;
    
    /// <summary>
    /// Initialize the container
    /// </summary>
    public void Initialize(string treeName, string description = "")
    {
        _treeName = treeName;
        _description = description;
        _groups = new SerializableDictionary<SkillsTreeGroup, List<Skill>>();
        _ungroupedSkills = new List<Skill>();
    }
    
    /// <summary>
    /// Add a new group to the container
    /// </summary>
    public void AddGroup(SkillsTreeGroup group)
    {
        if (!_groups.ContainsKey(group))
        {
            _groups.Add(group, new List<Skill>());
        }
    }
    
    /// <summary>
    /// Add a skill to a specific group
    /// </summary>
    public void AddSkillToGroup(SkillsTreeGroup group, Skill skill)
    {
        if (!_groups.ContainsKey(group))
            AddGroup(group);
            
        if (!_groups[group].Contains(skill))
            _groups[group].Add(skill);
    }
    
    /// <summary>
    /// Add an ungrouped skill
    /// </summary>
    public void AddUngroupedSkill(Skill skill)
    {
        if (!_ungroupedSkills.Contains(skill))
            _ungroupedSkills.Add(skill);
    }
    
    /// <summary>
    /// Remove a skill from a group
    /// </summary>
    public void RemoveSkillFromGroup(SkillsTreeGroup group, Skill skill)
    {
        if (_groups.ContainsKey(group))
            _groups[group].Remove(skill);
    }
    
    /// <summary>
    /// Remove an ungrouped skill
    /// </summary>
    public void RemoveUngroupedSkill(Skill skill)
    {
        _ungroupedSkills.Remove(skill);
    }
    
    /// <summary>
    /// Check if container has any groups
    /// </summary>
    public bool HasGroups()
    {
        return _groups.Count > 0;
    }
    
    /// <summary>
    /// Get all group names
    /// </summary>
    public string[] GetGroupsNames()
    {
        return _groups.Keys.Select(group => group.GroupName).ToArray();
    }
    
    /// <summary>
    /// Get skills in a specific group
    /// </summary>
    public List<Skill> GetSkillsInGroup(SkillsTreeGroup group)
    {
        return _groups.ContainsKey(group) ? _groups[group] : new List<Skill>();
    }
    
    /// <summary>
    /// Get skills from a specific group with optional starting skills filter
    /// </summary>
    public List<Skill> GetGroupedSkills(SkillsTreeGroup group, bool startingSkillsOnly = false)
    {
        if (!_groups.ContainsKey(group))
            return new List<Skill>();
        
        var skills = _groups[group];
        
        if (startingSkillsOnly)
        {
            // Starting skills are skills with no prerequisites
            return skills.FindAll(s => s.Prerequisites == null || s.Prerequisites.Count == 0);
        }
        
        return new List<Skill>(skills);
    }
    
    /// <summary>
    /// Get grouped skill names with optional filter
    /// </summary>
    public List<string> GetGroupedSkillNames(SkillsTreeGroup group, bool isOnlyStartingSkills = false)
    {
        List<string> skillNames = new List<string>();
        
        if (!_groups.ContainsKey(group))
            return skillNames;
        
        var skills = GetGroupedSkills(group, isOnlyStartingSkills);
        
        foreach (var skill in skills)
        {
            skillNames.Add(skill.SkillName);
        }
        
        return skillNames;
    }
    
    /// <summary>
    /// Get ungrouped skill names
    /// </summary>
    public List<string> GetUngroupedSkillNames(bool isOnlyStartingSkills = false)
    {
        List<string> skillNames = new List<string>();
        
        var skills = isOnlyStartingSkills 
            ? _ungroupedSkills.FindAll(s => s.Prerequisites == null || s.Prerequisites.Count == 0)
            : _ungroupedSkills;
        
        foreach (var skill in skills)
        {
            skillNames.Add(skill.SkillName);
        }
        
        return skillNames;
    }
    
    /// <summary>
    /// Get all starting skills (skills with no prerequisites) from all groups and ungrouped
    /// </summary>
    public List<Skill> GetStartingSkills()
    {
        List<Skill> startingSkills = new List<Skill>();
        
        // Get starting skills from all groups
        foreach (var group in _groups)
        {
            var groupStartingSkills = group.Value.FindAll(s => s.Prerequisites == null || s.Prerequisites.Count == 0);
            startingSkills.AddRange(groupStartingSkills);
        }
        
        // Get starting skills from ungrouped
        var ungroupedStartingSkills = _ungroupedSkills.FindAll(s => s.Prerequisites == null || s.Prerequisites.Count == 0);
        startingSkills.AddRange(ungroupedStartingSkills);
        
        return startingSkills;
    }
    
    /// <summary>
    /// Get a skill by name (searches both grouped and ungrouped)
    /// </summary>
    public Skill GetSkillByName(string skillName)
    {
        // Search in grouped skills
        foreach (var group in _groups)
        {
            foreach (var skill in group.Value)
            {
                if (skill.SkillName == skillName)
                    return skill;
            }
        }
        
        // Search in ungrouped skills
        foreach (var skill in _ungroupedSkills)
        {
            if (skill.SkillName == skillName)
                return skill;
        }
        
        return null;
    }
    
    /// <summary>
    /// Get a skill from a specific group by name
    /// </summary>
    public Skill GetGroupSkill(string groupName, string skillName)
    {
        var group = _groups.Keys.FirstOrDefault(g => g.GroupName == groupName);
        
        if (group != null && _groups.TryGetValue(group, out var skills))
        {
            return skills.FirstOrDefault(s => s.SkillName == skillName);
        }
        
        return null;
    }
    
    /// <summary>
    /// Get an ungrouped skill by name
    /// </summary>
    public Skill GetUngroupedSkill(string skillName)
    {
        return _ungroupedSkills.FirstOrDefault(s => s.SkillName == skillName);
    }
    
    /// <summary>
    /// Get all skills (both grouped and ungrouped)
    /// </summary>
    public List<Skill> GetAllSkills()
    {
        List<Skill> allSkills = new List<Skill>();
        
        foreach (var group in _groups)
        {
            allSkills.AddRange(group.Value);
        }
        
        allSkills.AddRange(_ungroupedSkills);
        
        return allSkills;
    }
    
    /// <summary>
    /// Get all unlocked skills
    /// </summary>
    public List<Skill> GetUnlockedSkills()
    {
        return GetAllSkills().Where(s => s.IsUnlocked).ToList();
    }
    
    /// <summary>
    /// Get all skills that can be unlocked (prerequisites met but not yet unlocked)
    /// </summary>
    public List<Skill> GetAvailableSkills()
    {
        return GetAllSkills().Where(s => !s.IsUnlocked && s.CanUnlock()).ToList();
    }
    
    /// <summary>
    /// Get skills by tier
    /// </summary>
    public List<Skill> GetSkillsByTier(int tier)
    {
        return GetAllSkills().Where(s => s.Tier == tier).ToList();
    }
    
    /// <summary>
    /// Get skills by type
    /// </summary>
    public List<Skill> GetSkillsByType(SkillType type)
    {
        return GetAllSkills().Where(s => s.SkillType == type).ToList();
    }
    
    /// <summary>
    /// Get total unlock cost for all skills
    /// </summary>
    public int GetTotalUnlockCost()
    {
        return GetAllSkills().Sum(s => s.UnlockCost);
    }
    
    /// <summary>
    /// Get total unlock cost for unlocked skills
    /// </summary>
    public int GetSpentSkillPoints()
    {
        return GetUnlockedSkills().Sum(s => s.UnlockCost * s.CurrentLevel);
    }
    
    /// <summary>
    /// Reset all skills to locked state
    /// </summary>
    public void ResetAllSkills()
    {
        foreach (var skill in GetAllSkills())
        {
            skill.Reset();
        }
    }
    
    /// <summary>
    /// Check if a skill exists in the container
    /// </summary>
    public bool ContainsSkill(Skill skill)
    {
        return GetAllSkills().Contains(skill);
    }
    
    /// <summary>
    /// Check if a skill exists by name
    /// </summary>
    public bool ContainsSkillByName(string skillName)
    {
        return GetSkillByName(skillName) != null;
    }
}