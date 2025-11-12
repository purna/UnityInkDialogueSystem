using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Container for organizing levels into progression trees
/// </summary>
[CreateAssetMenu(fileName = "LevelContainer", menuName = "Level System/Level Container")]
public class LevelContainer : ScriptableObject
{
    [Header("Level Details")]
    [SerializeField] private string _levelName;
    [SerializeField, TextArea] private string _description;
    
    [Header("Level Groups")]
    [SerializeField] private SerializableDictionary<LevelGroup, List<Level>> _groups;
    [SerializeField] private List<Level> _ungroupedLevels;
    
    public string LevelName => _levelName;
    public string Description => _description;
    public SerializableDictionary<LevelGroup, List<Level>> Groups => _groups;
    public List<Level> UngroupedLevels => _ungroupedLevels;
    
    /// <summary>
    /// Initialize the container with a name
    /// </summary>
    public void Initialize(string levelName, string description = "")
    {
        _levelName = levelName;
        _description = description;
        _groups = new SerializableDictionary<LevelGroup, List<Level>>();
        _ungroupedLevels = new List<Level>();
    }

    /// <summary>
    /// Add a new group to the container
    /// </summary>
    public void AddGroup(LevelGroup group)
    {
        if (!_groups.ContainsKey(group))
        {
            _groups.Add(group, new List<Level>());
        }
    }

    /// <summary>
    /// Add a level to a specific group
    /// </summary>
    public void AddLevelToGroup(LevelGroup group, Level level)
    {
        if (!_groups.ContainsKey(group))
            AddGroup(group);

        if (!_groups[group].Contains(level))
            _groups[group].Add(level);
    }

    /// <summary>
    /// Add an ungrouped level
    /// </summary>
    public void AddUngroupedLevel(Level level)
    {
        if (!_ungroupedLevels.Contains(level))
            _ungroupedLevels.Add(level);
    }
    
    /// <summary>
    /// Remove a level from a group
    /// </summary>
    public void RemoveLevelFromGroup(LevelGroup group, Level level)
    {
        if (_groups.ContainsKey(group))
            _groups[group].Remove(level);
    }
    
    /// <summary>
    /// Remove an ungrouped level
    /// </summary>
    public void RemoveUngroupedLevel(Level level)
    {
        _ungroupedLevels.Remove(level);
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
    /// Get levels in a specific group
    /// </summary>
    public List<Level> GetLevelsInGroup(LevelGroup group)
    {
        return _groups.ContainsKey(group) ? _groups[group] : new List<Level>();
    }

    /// <summary>
    /// Get levels from a specific group with optional starting levels filter
    /// </summary>
    public List<Level> GetGroupedLevels(LevelGroup group, bool startingLevelsOnly = false)
    {
        if (!_groups.ContainsKey(group))
            return new List<Level>();

        var levels = _groups[group];

        if (startingLevelsOnly)
        {
            // Starting levels are levels with no prerequisites
            return levels.FindAll(l => l.PrerequisiteLevels == null || l.PrerequisiteLevels.Count == 0);
        }

        return new List<Level>(levels);
    }

    /// <summary>
    /// Get grouped level names with optional filter
    /// </summary>
    public List<string> GetGroupedLevelNames(LevelGroup group, bool isOnlyStartingLevels = false)
    {
        List<string> levelNames = new List<string>();

        if (!_groups.ContainsKey(group))
            return levelNames;

        var levels = GetGroupedLevels(group, isOnlyStartingLevels);

        foreach (var level in levels)
        {
            levelNames.Add(level.LevelName);
        }

        return levelNames;
    }

    /// <summary>
    /// Get ungrouped level names
    /// </summary>
    public List<string> GetUngroupedLevelNames(bool isOnlyStartingLevels = false)
    {
        List<string> levelNames = new List<string>();

        var levels = isOnlyStartingLevels
            ? _ungroupedLevels.FindAll(l => l.PrerequisiteLevels == null || l.PrerequisiteLevels.Count == 0)
            : _ungroupedLevels;

        foreach (var level in levels)
        {
            levelNames.Add(level.LevelName);
        }

        return levelNames;
    }

    /// <summary>
    /// Get all starting levels (levels with no prerequisites) from all groups and ungrouped
    /// </summary>
    public List<Level> GetStartingLevels()
    {
        List<Level> startingLevels = new List<Level>();

        // Get starting levels from all groups
        foreach (var group in _groups)
        {
            var groupStartingLevels = group.Value.FindAll(l => l.PrerequisiteLevels == null || l.PrerequisiteLevels.Count == 0);
            startingLevels.AddRange(groupStartingLevels);
        }

        // Get starting levels from ungrouped
        var ungroupedStartingLevels = _ungroupedLevels.FindAll(l => l.PrerequisiteLevels == null || l.PrerequisiteLevels.Count == 0);
        startingLevels.AddRange(ungroupedStartingLevels);

        return startingLevels;
    }

    /// <summary>
    /// Get a level by name (searches both grouped and ungrouped)
    /// </summary>
    public Level GetLevelByName(string levelName)
    {
        // Search in grouped levels
        foreach (var group in _groups)
        {
            foreach (var level in group.Value)
            {
                if (level.LevelName == levelName)
                    return level;
            }
        }

        // Search in ungrouped levels
        foreach (var level in _ungroupedLevels)
        {
            if (level.LevelName == levelName)
                return level;
        }

        return null;
    }

    /// <summary>
    /// Get a level from a specific group by name
    /// </summary>
    public Level GetGroupLevel(string groupName, string levelName)
    {
        var group = _groups.Keys.FirstOrDefault(g => g.GroupName == groupName);

        if (group != null && _groups.TryGetValue(group, out var levels))
        {
            return levels.FirstOrDefault(l => l.LevelName == levelName);
        }

        return null;
    }

    /// <summary>
    /// Get an ungrouped level by name
    /// </summary>
    public Level GetUngroupedLevel(string levelName)
    {
        return _ungroupedLevels.FirstOrDefault(l => l.LevelName == levelName);
    }

    /// <summary>
    /// Get all levels (both grouped and ungrouped)
    /// </summary>
    public List<Level> GetAllLevels()
    {
        List<Level> allLevels = new List<Level>();

        foreach (var group in _groups)
        {
            allLevels.AddRange(group.Value);
        }

        allLevels.AddRange(_ungroupedLevels);

        return allLevels;
    }

    /// <summary>
    /// Get all unlocked levels
    /// </summary>
    public List<Level> GetUnlockedLevels()
    {
        return GetAllLevels().Where(l => l.IsUnlocked).ToList();
    }

    /// <summary>
    /// Get all completed levels
    /// </summary>
    public List<Level> GetCompletedLevels()
    {
        return GetAllLevels().Where(l => l.IsCompleted).ToList();
    }

    /// <summary>
    /// Get all levels that can be unlocked (prerequisites met but not yet unlocked)
    /// </summary>
    public List<Level> GetAvailableLevels()
    {
        return GetAllLevels().Where(l => !l.IsUnlocked && l.CanUnlock()).ToList();
    }

    /// <summary>
    /// Get levels by tier
    /// </summary>
    public List<Level> GetLevelsByTier(int tier)
    {
        return GetAllLevels().Where(l => l.Tier == tier).ToList();
    }

    /// <summary>
    /// Check if all levels are completed
    /// </summary>
    public bool AreAllLevelsCompleted()
    {
        return GetAllLevels().All(l => l.IsCompleted);
    }

    /// <summary>
    /// Get completion statistics
    /// </summary>
    public (int total, int completed, int unlocked) GetCompletionStats()
    {
        var allLevels = GetAllLevels();
        int total = allLevels.Count;
        int completed = allLevels.Count(l => l.IsCompleted);
        int unlocked = allLevels.Count(l => l.IsUnlocked);

        return (total, completed, unlocked);
    }

    /// <summary>
    /// Reset all levels in the container
    /// </summary>
    public void ResetAllLevels()
    {
        foreach (var level in GetAllLevels())
        {
            level.Reset();
        }
        Debug.Log($"[LevelContainer] Reset {GetAllLevels().Count} levels");
    }

    /// <summary>
    /// Get the next uncompleted level
    /// </summary>
    public Level GetNextUncompletedLevel()
    {
        // Get unlocked but not completed levels
        var availableLevels = GetAllLevels().Where(l => l.IsUnlocked && !l.IsCompleted).ToList();

        if (availableLevels.Count == 0)
            return null;

        // Return the one with lowest tier/index
        return availableLevels.OrderBy(l => l.Tier).ThenBy(l => l.LevelIndex).FirstOrDefault();
    }

    /// <summary>
    /// Check if a level exists in the container
    /// </summary>
    public bool ContainsLevel(Level level)
    {
        return GetAllLevels().Contains(level);
    }

    /// <summary>
    /// Check if a level exists by name
    /// </summary>
    public bool ContainsLevelByName(string levelName)
    {
        return GetLevelByName(levelName) != null;
    }

    /// <summary>
    /// Validate the level tree (check for circular dependencies, etc.)
    /// </summary>
    public bool ValidateLevelTree(out string errorMessage)
    {
        errorMessage = "";

        foreach (var level in GetAllLevels())
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

        if (level.PrerequisiteLevels != null)
        {
            foreach (var prereq in level.PrerequisiteLevels)
            {
                if (prereq != null && HasCircularDependency(prereq, new HashSet<Level>(visited)))
                    return true;
            }
        }

        return false;
    }


    /// <summary>
/// Validate and repair the container by re-scanning all level assets
/// Call this if levels are missing from the container
/// </summary>
public void ValidateAndRepairContainer(string graphFolderPath)
{
    Debug.Log($"[LevelContainer] Starting validation and repair for {_levelName}");
    
    int addedCount = 0;
    
    // Clear existing lists (we'll rebuild them)
    _groups.Clear();
    _ungroupedLevels.Clear();
    
    #if UNITY_EDITOR
    // Re-scan global levels
    string globalPath = $"{graphFolderPath}/Global/Level";
    if (System.IO.Directory.Exists(globalPath))
    {
        string[] globalLevelGuids = UnityEditor.AssetDatabase.FindAssets("t:Level", new[] { globalPath });
        foreach (string guid in globalLevelGuids)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            Level level = UnityEditor.AssetDatabase.LoadAssetAtPath<Level>(assetPath);
            if (level != null)
            {
                AddUngroupedLevel(level);
                addedCount++;
                Debug.Log($"[LevelContainer] Added ungrouped level: {level.LevelName}");
            }
        }
    }
    
    // Re-scan grouped levels
    string groupsPath = $"{graphFolderPath}/Groups";
    if (System.IO.Directory.Exists(groupsPath))
    {
        string[] groupDirectories = System.IO.Directory.GetDirectories(groupsPath);
        foreach (string groupDir in groupDirectories)
        {
            string groupName = System.IO.Path.GetFileName(groupDir);
            string levelPath = $"{groupDir}/Level";
            
            if (System.IO.Directory.Exists(levelPath))
            {
                // Find or create the LevelGroup
                string groupAssetPath = $"{groupDir}/{groupName}.asset";
                LevelGroup levelGroup = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelGroup>(groupAssetPath);
                
                if (levelGroup != null)
                {
                    AddGroup(levelGroup);
                    
                    // Find all levels in this group
                    string[] levelGuids = UnityEditor.AssetDatabase.FindAssets("t:Level", new[] { levelPath });
                    foreach (string guid in levelGuids)
                    {
                        string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                        Level level = UnityEditor.AssetDatabase.LoadAssetAtPath<Level>(assetPath);
                        if (level != null)
                        {
                            AddLevelToGroup(levelGroup, level);
                            addedCount++;
                            Debug.Log($"[LevelContainer] Added level to group '{groupName}': {level.LevelName}");
                        }
                    }
                }
            }
        }
    }
    
    UnityEditor.EditorUtility.SetDirty(this);
    UnityEditor.AssetDatabase.SaveAssets();
    #endif
    
    Debug.Log($"[LevelContainer] Validation complete. Added {addedCount} levels.");
}

/// <summary>
/// Check if the container is empty or missing levels
/// </summary>
public bool NeedsRepair()
{
    return (_groups.Count == 0 && _ungroupedLevels.Count == 0) || 
           (_groups.Any(g => g.Value.Count == 0));
}

/// <summary>
/// Get a detailed status report of the container
/// </summary>
public string GetStatusReport()
{
    var sb = new System.Text.StringBuilder();
    sb.AppendLine($"=== LevelContainer: {_levelName} ===");
    sb.AppendLine($"Total Groups: {_groups.Count}");
    sb.AppendLine($"Ungrouped Levels: {_ungroupedLevels.Count}");
    sb.AppendLine();
    
    foreach (var group in _groups)
    {
        sb.AppendLine($"Group '{group.Key.GroupName}': {group.Value.Count} levels");
        foreach (var level in group.Value)
        {
            sb.AppendLine($"  - {level.LevelName}");
        }
    }
    
    if (_ungroupedLevels.Count > 0)
    {
        sb.AppendLine("Ungrouped Levels:");
        foreach (var level in _ungroupedLevels)
        {
            sb.AppendLine($"  - {level.LevelName}");
        }
    }
    
    return sb.ToString();
}
}