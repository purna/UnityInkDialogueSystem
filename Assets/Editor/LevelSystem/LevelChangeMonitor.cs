using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Monitors Level ScriptableObjects for changes and notifies the graph view
/// This enables auto-update of node visuals when Levels are modified in the sidebar
/// </summary>
[InitializeOnLoad]
public static class LevelChangeMonitor
{
    private static Dictionary<int, Level> _trackedLevels = new Dictionary<int, Level>();
    private static Dictionary<Level, LevelData> _lastKnownStates = new Dictionary<Level, LevelData>();
    
    // Store last known state of a Level
    private class LevelData
    {
        public string levelName;
        public string description;
        public Sprite icon;
        public Sprite lockedIcon;
        public Sprite completedIcon;
        public int tier;
        public int levelIndex;
        public float completionThreshold;
        public int maxAttempts;
        
        // Runtime state (for detecting changes during play mode testing)
        public bool isUnlocked;
        public bool isCompleted;
        public int attemptsUsed;
        public float bestScore;
        public int timesCompleted;
        
        public LevelData(Level level)
        {
            levelName = level.LevelName;
            description = level.Description;
            icon = level.Icon;
            lockedIcon = level.LockedIcon;
            completedIcon = level.CompletedIcon;
            tier = level.Tier;
            levelIndex = level.LevelIndex;
            completionThreshold = level.CompletionThreshold;
            maxAttempts = level.MaxAttempts;
            
            // Runtime state
            isUnlocked = level.IsUnlocked;
            isCompleted = level.IsCompleted;
            attemptsUsed = level.AttemptsUsed;
            bestScore = level.BestScore;
            timesCompleted = level.TimesCompleted;
        }
        
        public bool HasChanged(Level level)
        {
            return levelName != level.LevelName ||
                   description != level.Description ||
                   icon != level.Icon ||
                   lockedIcon != level.LockedIcon ||
                   completedIcon != level.CompletedIcon ||
                   tier != level.Tier ||
                   levelIndex != level.LevelIndex ||
                   completionThreshold != level.CompletionThreshold ||
                   maxAttempts != level.MaxAttempts ||
                   isUnlocked != level.IsUnlocked ||
                   isCompleted != level.IsCompleted ||
                   attemptsUsed != level.AttemptsUsed ||
                   bestScore != level.BestScore ||
                   timesCompleted != level.TimesCompleted;
        }
        
        /// <summary>
        /// Check if only runtime state has changed (not design-time properties)
        /// </summary>
        public bool HasRuntimeStateChanged(Level level)
        {
            return isUnlocked != level.IsUnlocked ||
                   isCompleted != level.IsCompleted ||
                   attemptsUsed != level.AttemptsUsed ||
                   bestScore != level.BestScore ||
                   timesCompleted != level.TimesCompleted;
        }
        
        /// <summary>
        /// Check if design-time properties have changed (not runtime state)
        /// </summary>
        public bool HasDesignPropertiesChanged(Level level)
        {
            return levelName != level.LevelName ||
                   description != level.Description ||
                   icon != level.Icon ||
                   lockedIcon != level.LockedIcon ||
                   completedIcon != level.CompletedIcon ||
                   tier != level.Tier ||
                   levelIndex != level.LevelIndex ||
                   completionThreshold != level.CompletionThreshold ||
                   maxAttempts != level.MaxAttempts;
        }
    }
    
    static LevelChangeMonitor()
    {
        // Subscribe to editor update
        EditorApplication.update += OnEditorUpdate;
        
        // Clear tracking when entering/exiting play mode
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
    
    /// <summary>
    /// Track a Level for changes
    /// </summary>
    public static void TrackLevel(Level level)
    {
        if (level == null) return;
        
        int instanceId = level.GetInstanceID();
        if (!_trackedLevels.ContainsKey(instanceId))
        {
            _trackedLevels[instanceId] = level;
            _lastKnownStates[level] = new LevelData(level);
        }
    }
    
    /// <summary>
    /// Stop tracking a Level
    /// </summary>
    public static void UntrackLevel(Level level)
    {
        if (level == null) return;
        
        int instanceId = level.GetInstanceID();
        _trackedLevels.Remove(instanceId);
        _lastKnownStates.Remove(level);
    }
    
    /// <summary>
    /// Clear all tracked Levels
    /// </summary>
    public static void ClearTracking()
    {
        _trackedLevels.Clear();
        _lastKnownStates.Clear();
    }
    
    private static void OnEditorUpdate()
    {
        // Check each tracked Level for changes
        List<Level> changedLevels = new List<Level>();
        
        foreach (var kvp in _trackedLevels)
        {
            Level level = kvp.Value;
            if (level == null)
            {
                changedLevels.Add(null); // Mark for removal
                continue;
            }
            
            if (_lastKnownStates.TryGetValue(level, out LevelData lastState))
            {
                if (lastState.HasChanged(level))
                {
                    // Level has changed!
                    changedLevels.Add(level);
                    
                    // Determine what type of change occurred
                    bool runtimeChanged = lastState.HasRuntimeStateChanged(level);
                    bool designChanged = lastState.HasDesignPropertiesChanged(level);
                    
                    // Update last known state
                    _lastKnownStates[level] = new LevelData(level);
                    
                    // Notify the editor window
                    NotifyLevelChanged(level, runtimeChanged, designChanged);
                }
            }
        }
        
        // Clean up null Levels
        foreach (var level in changedLevels)
        {
            if (level == null)
            {
                // Find and remove the null entry
                int? keyToRemove = null;
                foreach (var kvp in _trackedLevels)
                {
                    if (kvp.Value == null)
                    {
                        keyToRemove = kvp.Key;
                        break;
                    }
                }
                
                if (keyToRemove.HasValue)
                {
                    _trackedLevels.Remove(keyToRemove.Value);
                }
            }
        }
    }
    
    private static void NotifyLevelChanged(Level level, bool runtimeChanged, bool designChanged)
    {
        // Find the open LevelSystemEditorWindow and notify it
        LevelSystemEditorWindow[] windows = Resources.FindObjectsOfTypeAll<LevelSystemEditorWindow>();

        foreach (var window in windows)
        {
            window.OnLevelChanged(level);
        }
    }
    
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.ExitingEditMode:
                // About to enter play mode - keep tracking but mark for refresh
                Debug.Log("[LevelChangeMonitor] Entering play mode - continuing to track levels");
                break;
                
            case PlayModeStateChange.EnteredPlayMode:
                // Now in play mode - update all tracked states
                RefreshAllTrackedStates();
                break;
                
            case PlayModeStateChange.ExitingPlayMode:
                // About to exit play mode
                Debug.Log("[LevelChangeMonitor] Exiting play mode - refreshing level states");
                break;
                
            case PlayModeStateChange.EnteredEditMode:
                // Back in edit mode - refresh all states to reflect runtime changes
                RefreshAllTrackedStates();
                break;
        }
    }
    
    /// <summary>
    /// Refresh the last known state of all tracked levels
    /// Useful after play mode changes or external modifications
    /// </summary>
    public static void RefreshAllTrackedStates()
    {
        foreach (var kvp in _trackedLevels)
        {
            Level level = kvp.Value;
            if (level != null && _lastKnownStates.ContainsKey(level))
            {
                _lastKnownStates[level] = new LevelData(level);
            }
        }
        
        Debug.Log($"[LevelChangeMonitor] Refreshed {_trackedLevels.Count} tracked levels");
    }
    
    /// <summary>
    /// Get tracking statistics for debugging
    /// </summary>
    public static string GetTrackingStats()
    {
        return $"Tracking {_trackedLevels.Count} levels";
    }
}