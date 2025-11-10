using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Monitors Skill ScriptableObjects for changes and notifies the graph view
/// This enables auto-update of node visuals when skills are modified in the sidebar
/// </summary>
[InitializeOnLoad]
public static class SkillChangeMonitor
{
    private static Dictionary<int, Skill> _trackedSkills = new Dictionary<int, Skill>();
    private static Dictionary<Skill, SkillData> _lastKnownStates = new Dictionary<Skill, SkillData>();
    
    // Store last known state of a skill
    private class SkillData
    {
        public string skillName;
        public string description;
        public Sprite icon;
        public int tier;
        public int unlockCost;
        public float value;
        public int maxLevel;
        
        public SkillData(Skill skill)
        {
            skillName = skill.SkillName;
            description = skill.Description;
            icon = skill.Icon;
            tier = skill.Tier;
            unlockCost = skill.UnlockCost;
            value = skill.Value;
            maxLevel = skill.MaxLevel;
        }
        
        public bool HasChanged(Skill skill)
        {
            return skillName != skill.SkillName ||
                   description != skill.Description ||
                   icon != skill.Icon ||
                   tier != skill.Tier ||
                   unlockCost != skill.UnlockCost ||
                   value != skill.Value ||
                   maxLevel != skill.MaxLevel;
        }
    }
    
    static SkillChangeMonitor()
    {
        // Subscribe to editor update
        EditorApplication.update += OnEditorUpdate;
    }
    
    /// <summary>
    /// Track a skill for changes
    /// </summary>
    public static void TrackSkill(Skill skill)
    {
        if (skill == null) return;
        
        int instanceId = skill.GetInstanceID();
        if (!_trackedSkills.ContainsKey(instanceId))
        {
            _trackedSkills[instanceId] = skill;
            _lastKnownStates[skill] = new SkillData(skill);
        }
    }
    
    /// <summary>
    /// Stop tracking a skill
    /// </summary>
    public static void UntrackSkill(Skill skill)
    {
        if (skill == null) return;
        
        int instanceId = skill.GetInstanceID();
        _trackedSkills.Remove(instanceId);
        _lastKnownStates.Remove(skill);
    }
    
    /// <summary>
    /// Clear all tracked skills
    /// </summary>
    public static void ClearTracking()
    {
        _trackedSkills.Clear();
        _lastKnownStates.Clear();
    }
    
    private static void OnEditorUpdate()
    {
        // Check each tracked skill for changes
        List<Skill> changedSkills = new List<Skill>();
        
        foreach (var kvp in _trackedSkills)
        {
            Skill skill = kvp.Value;
            if (skill == null)
            {
                changedSkills.Add(null); // Mark for removal
                continue;
            }
            
            if (_lastKnownStates.TryGetValue(skill, out SkillData lastState))
            {
                if (lastState.HasChanged(skill))
                {
                    // Skill has changed!
                    changedSkills.Add(skill);
                    
                    // Update last known state
                    _lastKnownStates[skill] = new SkillData(skill);
                    
                    // Notify the editor window
                    NotifySkillChanged(skill);
                }
            }
        }
        
        // Clean up null skills
        foreach (var skill in changedSkills)
        {
            if (skill == null)
            {
                // Find and remove the null entry
                int? keyToRemove = null;
                foreach (var kvp in _trackedSkills)
                {
                    if (kvp.Value == null)
                    {
                        keyToRemove = kvp.Key;
                        break;
                    }
                }
                
                if (keyToRemove.HasValue)
                {
                    _trackedSkills.Remove(keyToRemove.Value);
                }
            }
        }
    }
    
    private static void NotifySkillChanged(Skill skill)
    {
        // Find the open SkillsTreeSystemEditorWindow and notify it
        SkillsTreeSystemEditorWindow[] windows = Resources.FindObjectsOfTypeAll<SkillsTreeSystemEditorWindow>();
        
        foreach (var window in windows)
        {
            window.OnSkillChanged(skill);
        }
    }
}