using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Container for organizing levels into progression trees
/// </summary>
[CreateAssetMenu(fileName = "LevelContainer", menuName = "Level System/Level Container")]
public class LevelContainer : ScriptableObject
{
    /// <summary>
    /// Initialize the container with a name
    /// </summary>
    public void Initialize(string name)
    {
        this.name = name;
    }
    [Header("Level Organization")]
    [SerializeField] private List<Level> _allLevels = new List<Level>();
    [SerializeField] private List<Level> _startingLevels = new List<Level>();
    
    [Header("Level Groups")]
    [SerializeField] private Dictionary<LevelGroup, List<Level>> _groups = new Dictionary<LevelGroup, List<Level>>();

    public Dictionary<LevelGroup, List<Level>> Groups => _groups;

    public string LevelName => name;

    public bool HasGroups() => _groups.Count > 0;

    public string[] GetGroupsNames() => _groups.Keys.Select(g => g.GroupName).ToArray();

    public List<string> GetGroupedLevelNames(LevelGroup group, bool startingOnly)
    {
        if (!_groups.ContainsKey(group))
            return new List<string>();

        List<string> levelNames = new List<string>();
        foreach (var level in _groups[group])
        {
            if (startingOnly && (level.PrerequisiteLevels != null && level.PrerequisiteLevels.Count > 0))
                continue;
            levelNames.Add(level.LevelName);
        }
        return levelNames;
    }

    public List<string> GetUngroupedLevelNames(bool startingOnly)
    {
        List<string> levelNames = new List<string>();
        foreach (var level in _allLevels)
        {
            if (_groups.Values.Any(list => list.Contains(level)))
                continue;
            if (startingOnly && (level.PrerequisiteLevels != null && level.PrerequisiteLevels.Count > 0))
                continue;
            levelNames.Add(level.LevelName);
        }
        return levelNames;
    }

    /// <summary>
    /// Get all levels in the container
    /// </summary>
    public List<Level> GetAllLevels()
    {
        return new List<Level>(_allLevels);
    }

    /// <summary>
    /// Get only starting levels (no prerequisites)
    /// </summary>
    public List<Level> GetStartingLevels()
    {
        return new List<Level>(_startingLevels);
    }

    /// <summary>
    /// Get all unlocked levels
    /// </summary>
    public List<Level> GetUnlockedLevels()
    {
        return _allLevels.Where(l => l.IsUnlocked).ToList();
    }

    /// <summary>
    /// Get all completed levels
    /// </summary>
    public List<Level> GetCompletedLevels()
    {
        return _allLevels.Where(l => l.IsCompleted).ToList();
    }

    /// <summary>
    /// Get levels from a specific group
    /// </summary>
    public List<Level> GetGroupedLevels(LevelGroup group, bool startingOnly = false)
    {
        if (!_groups.ContainsKey(group))
            return new List<Level>();

        if (startingOnly)
        {
            return _groups[group].Where(l =>
                l.PrerequisiteLevels == null || l.PrerequisiteLevels.Count == 0
            ).ToList();
        }

        return new List<Level>(_groups[group]);
    }

    /// <summary>
    /// Get a level from a specific group by name
    /// </summary>
    public Level GetGroupLevel(string groupTitle, string levelName)
    {
        LevelGroup group = Groups.Keys.FirstOrDefault(g => g.GroupName == groupTitle);
        if (group != null && Groups.ContainsKey(group))
        {
            return Groups[group].FirstOrDefault(l => l.LevelName == levelName);
        }
        return null;
    }

    /// <summary>
    /// Get levels by tier
    /// </summary>
    public List<Level> GetLevelsByTier(int tier)
    {
        return _allLevels.Where(l => l.Tier == tier).ToList();
    }

    /// <summary>
    /// Get level by name
    /// </summary>
    public Level GetLevelByName(string name)
    {
        return _allLevels.FirstOrDefault(l => l.LevelName == name);
    }

    /// <summary>
    /// Get an ungrouped level by name
    /// </summary>
    public Level GetUngroupedLevel(string levelName)
    {
        return _allLevels.FirstOrDefault(l => l.LevelName == levelName && !Groups.Values.Any(list => list.Contains(l)));
    }

    /// <summary>
    /// Check if all levels are completed
    /// </summary>
    public bool AreAllLevelsCompleted()
    {
        return _allLevels.All(l => l.IsCompleted);
    }

    /// <summary>
    /// Get completion statistics
    /// </summary>
    public (int total, int completed, int unlocked) GetCompletionStats()
    {
        int total = _allLevels.Count;
        int completed = _allLevels.Count(l => l.IsCompleted);
        int unlocked = _allLevels.Count(l => l.IsUnlocked);

        return (total, completed, unlocked);
    }

    /// <summary>
    /// Reset all levels in the container
    /// </summary>
    public void ResetAllLevels()
    {
        foreach (var level in _allLevels)
        {
            level.Reset();
        }
        Debug.Log($"[LevelContainer] Reset {_allLevels.Count} levels");
    }

    /// <summary>
    /// Add a level to the container
    /// </summary>
    public void AddLevel(Level level)
    {
        if (level != null && !_allLevels.Contains(level))
        {
            _allLevels.Add(level);
            
            // Auto-add to starting levels if no prerequisites
            if (level.PrerequisiteLevels == null || level.PrerequisiteLevels.Count == 0)
            {
                if (!_startingLevels.Contains(level))
                    _startingLevels.Add(level);
            }
        }
    }

    /// <summary>
    /// Remove a level from the container
    /// </summary>
    public void RemoveLevel(Level level)
    {
        _allLevels.Remove(level);
        _startingLevels.Remove(level);
    }

    /// <summary>
    /// Add a group to the container
    /// </summary>
    public void AddGroup(LevelGroup group)
    {
        if (group != null && !_groups.ContainsKey(group))
        {
            _groups[group] = new List<Level>();
        }
    }

    /// <summary>
    /// Add a level to a group
    /// </summary>
    public void AddLevelToGroup(Level level, LevelGroup group)
    {
        if (level == null || group == null)
            return;

        if (!_groups.ContainsKey(group))
        {
            _groups[group] = new List<Level>();
        }

        if (!_groups[group].Contains(level))
        {
            _groups[group].Add(level);
        }
    }

    /// <summary>
    /// Get the next uncompleted level
    /// </summary>
    public Level GetNextUncompletedLevel()
    {
        // Get unlocked but not completed levels
        var availableLevels = _allLevels.Where(l => l.IsUnlocked && !l.IsCompleted).ToList();
        
        if (availableLevels.Count == 0)
            return null;

        // Return the one with lowest tier/index
        return availableLevels.OrderBy(l => l.Tier).ThenBy(l => l.LevelIndex).FirstOrDefault();
    }

    /// <summary>
    /// Validate the level tree (check for circular dependencies, etc.)
    /// </summary>
    public bool ValidateLevelTree(out string errorMessage)
    {
        errorMessage = "";

        foreach (var level in _allLevels)
        {
            if (HasCircularDependency(level, new HashSet<Level>()))
            {
                errorMessage = $"Circular dependency detected for level: {level.LevelName}";
                return false;
            }
        }

        return true;
    }

    private bool HasCircularDependency(Level level, HashSet<Level> visited)
    {
        if (visited.Contains(level))
            return true;

        visited.Add(level);

        foreach (var prereq in level.PrerequisiteLevels)
        {
            if (prereq != null && HasCircularDependency(prereq, new HashSet<Level>(visited)))
                return true;
        }

        return false;
    }
}