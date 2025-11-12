using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a single level node in the level progression tree
/// Compatible with both graph editor and level unlock manager
/// </summary>
[CreateAssetMenu(fileName = "Level", menuName = "Level System/Level")]
public class Level : ScriptableObject
{
    [Header("Level Identity")]
    [Tooltip("Unique name for this level (used for identification and display)")]
    [SerializeField] private string _levelName;

    [Tooltip("Detailed description of the level (displayed in UI menus)")]
    [SerializeField, TextArea] private string _description;
    
    
    [Tooltip("Main icon representing this level")]
    [SerializeField] private Sprite _icon;
    
    [Tooltip("Icon shown when the level is locked")]
    [SerializeField] private Sprite _lockedIcon;
    
    [Tooltip("Icon shown when the level is unlocked but not completed")]
    [SerializeField] private Sprite _unlockedIcon;
    
    [Tooltip("Icon shown when the level has been completed")]
    [SerializeField] private Sprite _completedIcon;
    
    [Header("Level Properties")]
    [Tooltip("Tier/difficulty ranking of this level (used for grouping and progression)")]
    [SerializeField] private int _tier;
    
    [Tooltip("Sequential order of this level in the progression tree")]
    [SerializeField] private int _levelIndex;
    
    [Tooltip("Category or type of level (gameplay mode, difficulty type, etc.)")]
    [SerializeField] private LevelSceneType _levelSceneType;
    
    [Tooltip("The Unity scene that will be loaded when this level is played")]
    [SerializeField] private SceneField _gameScene;
    
    [Header("Completion Requirements")]
    [Tooltip("Minimum score required to complete this level (0 = no requirement)")]
    [SerializeField] private float _completionThreshold = 100f;
    
    [Tooltip("Maximum number of attempts allowed (-1 = unlimited attempts)")]
    [SerializeField] private int _maxAttempts = -1;
    
    [Header("Prerequisites & Connections")]
    [Tooltip("Levels that must be completed before this level can be unlocked")]
    [SerializeField] private List<Level> _prerequisiteLevels = new List<Level>();
    
    [Tooltip("Levels that become available after completing this level")]
    [SerializeField] private List<Level> _nextLevels = new List<Level>();

    [Header("Visual (Graph Editor)")]
    [Tooltip("Position of this level node in the graph editor (for visual organization)")]
    [SerializeField] private Vector2 _position;
    
    // Runtime state
    private bool _isUnlocked;
    private bool _isCompleted;
    private int _attemptsUsed;
    private float _bestScore;
    private int _timesCompleted;
    
    #region Properties
    
    public string LevelName => _levelName;
    public string Description => _description;
    public Sprite Icon => _icon;
    public Sprite LockedIcon => _lockedIcon;
    public Sprite UnlockedIcon => _unlockedIcon;
    public Sprite CompletedIcon => _completedIcon;
    public int Tier => _tier;
    public int LevelIndex => _levelIndex;
    public LevelSceneType LevelSceneType => _levelSceneType;
    public SceneField GameScene => _gameScene;
    public string GameSceneName => _gameScene?.SceneName ?? string.Empty;
    public float CompletionThreshold => _completionThreshold;
    public int MaxAttempts => _maxAttempts;
    public Vector2 Position => _position;
    
    // Runtime state properties
    public bool IsUnlocked => _isUnlocked;
    public bool IsCompleted => _isCompleted;
    public int AttemptsUsed => _attemptsUsed;
    public float BestScore => _bestScore;
    public int TimesCompleted => _timesCompleted;
    
    // Graph connections (compatible with both systems)
    public List<Level> Prerequisites => _prerequisiteLevels;
    public List<Level> Children => _nextLevels;
    public List<Level> PrerequisiteLevels => _prerequisiteLevels;
    public List<Level> NextLevels => _nextLevels;
    
    // Compatibility properties for controller system
    public int MaxLevel => 1; // Levels don't have "levels" - they're binary complete/incomplete
    public int CurrentLevel => _isCompleted ? 1 : 0;
    public float Value => _bestScore;
    public int UnlockCost => 0; // No currency cost - prerequisite-based only
    
    #endregion
    
    #region Initialization
    
    public void Initialize(string levelName, string description, 
                          Sprite icon, Sprite lockedIcon, Sprite unlockedIcon, Sprite completedIcon,
                          int tier, LevelSceneType levelSceneType, int levelIndex, float completionThreshold, int maxAttempts,
                          List<Level> prerequisiteLevels, List<Level> nextLevels, 
                          Vector2 position)
    {
        _levelName = levelName;
        _description = description;
        _icon = icon;
        _lockedIcon = lockedIcon;
        _unlockedIcon = unlockedIcon;
        _completedIcon = completedIcon;
        _tier = tier;
        _levelSceneType = levelSceneType;
        _levelIndex = levelIndex;
        _completionThreshold = completionThreshold;
        _maxAttempts = maxAttempts;
        _prerequisiteLevels = prerequisiteLevels ?? new List<Level>();
        _nextLevels = nextLevels ?? new List<Level>();
        _position = position;
        
        ResetRuntimeState();
    }
    
    private void ResetRuntimeState()
    {
        _isUnlocked = false;
        _isCompleted = false;
        _attemptsUsed = 0;
        _bestScore = 0f;
        _timesCompleted = 0;
    }
    
    #endregion
    
    #region Unlock & Completion Logic
    
    /// <summary>
    /// Check if this level can be unlocked (prerequisites met)
    /// </summary>
    public bool CanUnlock()
    {
        if (_isUnlocked)
            return false;
            
        // Starting levels (no prerequisites) can always be unlocked
        if (_prerequisiteLevels == null || _prerequisiteLevels.Count == 0)
            return true;
            
        // Check if all prerequisite levels are completed
        foreach (var prereq in _prerequisiteLevels)
        {
            if (prereq != null && !prereq.IsCompleted)
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Check if level can be played (unlocked and attempts remaining)
    /// </summary>
    public bool CanPlay()
    {
        if (!_isUnlocked)
            return false;
            
        if (_isCompleted)
            return true; // Can replay completed levels
            
        if (_maxAttempts > 0 && _attemptsUsed >= _maxAttempts)
            return false;
            
        return true;
    }
    
    /// <summary>
    /// Unlock this level
    /// </summary>
    public void Unlock()
    {
        if (!_isUnlocked && CanUnlock())
        {
            _isUnlocked = true;
            Debug.Log($"[Level] Unlocked: {_levelName}");
        }
    }
    
    /// <summary>
    /// Force unlock (for testing or special cases)
    /// </summary>
    public void ForceUnlock()
    {
        _isUnlocked = true;
        Debug.Log($"[Level] Force Unlocked: {_levelName}");
    }
    
    /// <summary>
    /// Register a level attempt
    /// </summary>
    public void StartAttempt()
    {
        if (_isUnlocked)
        {
            _attemptsUsed++;
            Debug.Log($"[Level] {_levelName} - Attempt {_attemptsUsed}/{(_maxAttempts > 0 ? _maxAttempts.ToString() : "∞")}");
        }
    }
    
    /// <summary>
    /// Complete the level with a score
    /// </summary>
    public void CompleteLevel(float score)
    {
        if (!_isUnlocked)
        {
            Debug.LogWarning($"[Level] Cannot complete {_levelName} - not unlocked!");
            return;
        }
        
        _timesCompleted++;
        
        // Update best score
        if (score > _bestScore)
        {
            _bestScore = score;
            Debug.Log($"[Level] {_levelName} - New best score: {score:F2}");
        }
        
        // Mark as completed if threshold is met
        if (!_isCompleted && score >= _completionThreshold)
        {
            _isCompleted = true;
            Debug.Log($"[Level] ✓ COMPLETED: {_levelName} with score {score:F2} (threshold: {_completionThreshold})");
            
            // Auto-unlock next levels
            UnlockNextLevels();
        }
        else if (!_isCompleted)
        {
            Debug.Log($"[Level] {_levelName} finished but not completed. Score: {score:F2}/{_completionThreshold}");
        }
    }
    
    /// <summary>
    /// Automatically unlock next levels when this one is completed
    /// </summary>
    private void UnlockNextLevels()
    {
        foreach (var nextLevel in _nextLevels)
        {
            if (nextLevel != null && nextLevel.CanUnlock())
            {
                nextLevel.Unlock();
            }
        }
    }
    
    #endregion
    
    #region Progress & Stats
    
    /// <summary>
    /// Get completion percentage (0-100)
    /// </summary>
    public float GetCompletionPercentage()
    {
        if (_completionThreshold <= 0)
            return 0f;
            
        return Mathf.Clamp01(_bestScore / _completionThreshold) * 100f;
    }
    
    /// <summary>
    /// Get remaining attempts (-1 if unlimited)
    /// </summary>
    public int GetRemainingAttempts()
    {
        if (_maxAttempts < 0)
            return -1;
            
        return Mathf.Max(0, _maxAttempts - _attemptsUsed);
    }
    
    /// <summary>
    /// Get scaled value (for compatibility with skill tree-style systems)
    /// </summary>
    public float GetScaledValue()
    {
        return _bestScore;
    }
    
    /// <summary>
    /// Check if this is a starting level (no prerequisites)
    /// </summary>
    public bool IsStartingLevel()
    {
        return _prerequisiteLevels == null || _prerequisiteLevels.Count == 0;
    }
    
    /// <summary>
    /// Check if the level has a valid scene assigned
    /// </summary>
    public bool HasValidScene()
    {
        return _gameScene != null && !string.IsNullOrEmpty(_gameScene.SceneName);
    }
    
    #endregion
    
    #region Editor & Save/Load
    
    /// <summary>
    /// Reset level progress (for new game or testing)
    /// </summary>
    public void Reset()
    {
        ResetRuntimeState();
        Debug.Log($"[Level] Reset: {_levelName}");
    }
    
    /// <summary>
    /// Update level name (editor only)
    /// </summary>
    public void UpdateName(string newName)
    {
        _levelName = newName;
    }

    /// <summary>
    /// Update description (editor only)
    /// </summary>
    public void UpdateDescription(string newDescription)
    {
        _description = newDescription;
    }

    /// <summary>
    /// Update position in graph (editor only)
    /// </summary>
    public void UpdatePosition(Vector2 newPosition)
    {
        _position = newPosition;
    }

    /// <summary>
    /// Update game scene (editor only)
    /// </summary>
    public void UpdateGameScene(SceneField newScene)
    {
        _gameScene = newScene;
    }
    
    // <summary>
/// Update the level's icon (editor only)
/// </summary>
public void UpdateIcon(Sprite icon)
{
    _icon = icon;
}



    // <summary>
    /// Update the level's icon (editor only)
    /// </summary>
    public void UpdateLevelSceneType(LevelSceneType levelSceneType)
    {
        _levelSceneType = levelSceneType;
    }

    /// <summary>
    /// Update the level's locked icon (editor only)
    /// </summary>
    public void UpdateLockedIcon(Sprite lockedIcon)
    {
        _lockedIcon = lockedIcon;
    }

    public void UpdateVisualData(
        Sprite icon,
        Sprite lockedIcon,
        Sprite unlockedIcon,
        Sprite completedIcon
    )
    {
        _icon = icon;
        _lockedIcon = lockedIcon;
        _unlockedIcon = unlockedIcon;
        _completedIcon = completedIcon;
    }
    

    public void UpdateLevelProperties(
        int tier,
        int levelIndex,
        float completionThreshold,
        int maxAttempts
    )
    {
        _tier = tier;
        _levelIndex = levelIndex;
        _completionThreshold = completionThreshold;
        _maxAttempts = maxAttempts;
    }
        

/// <summary>
/// Update the level's unlocked icon (editor only)
/// </summary>
public void UpdateUnlockedIcon(Sprite unlockedIcon)
{
    _unlockedIcon = unlockedIcon;
}

/// <summary>
/// Update the level's completed icon (editor only)
/// </summary>
public void UpdateCompletedIcon(Sprite completedIcon)
{
    _completedIcon = completedIcon;
}

/// <summary>
/// Update the level's tier (editor only)
/// </summary>
public void UpdateTier(int tier)
{
    _tier = tier;
}

/// <summary>
/// Update the level's index (editor only)
/// </summary>
public void UpdateLevelIndex(int levelIndex)
{
    _levelIndex = levelIndex;
}

/// <summary>
/// Update the level's completion threshold (editor only)
/// </summary>
public void UpdateCompletionThreshold(float threshold)
{
    _completionThreshold = threshold;
}

/// <summary>
/// Update the level's max attempts (editor only)
/// </summary>
public void UpdateMaxAttempts(int maxAttempts)
{
    _maxAttempts = maxAttempts;
}


    /// <summary>
    /// Update visual icons (editor only)
    /// </summary>
    public void UpdateIcons(Sprite icon, Sprite lockedIcon, Sprite unlockedIcon, Sprite completedIcon)
    {
        _icon = icon;
        _lockedIcon = lockedIcon;
        _unlockedIcon = unlockedIcon;
        _completedIcon = completedIcon;
    }
    
    /// <summary>
    /// Update level properties (editor only)
    /// </summary>
    public void UpdateProperties(int tier, int levelIndex, float completionThreshold, int maxAttempts)
    {
        _tier = tier;
        _levelIndex = levelIndex;
        _completionThreshold = completionThreshold;
        _maxAttempts = maxAttempts;
    }
    
    /// <summary>
    /// Add prerequisite level
    /// </summary>
    public void AddPrerequisite(Level prerequisite)
    {
        if (prerequisite != null && !_prerequisiteLevels.Contains(prerequisite))
        {
            _prerequisiteLevels.Add(prerequisite);
        }
    }
    
    /// <summary>
    /// Remove prerequisite level
    /// </summary>
    public void RemovePrerequisite(Level prerequisite)
    {
        _prerequisiteLevels.Remove(prerequisite);
    }
    
    /// <summary>
    /// Add next level connection
    /// </summary>
    public void AddNextLevel(Level nextLevel)
    {
        if (nextLevel != null && !_nextLevels.Contains(nextLevel))
        {
            _nextLevels.Add(nextLevel);
        }
    }
    
    /// <summary>
    /// Remove next level connection
    /// </summary>
    public void RemoveNextLevel(Level nextLevel)
    {
        _nextLevels.Remove(nextLevel);
    }
    
    /// <summary>
    /// Get save data for serialization
    /// </summary>
    public LevelSaveData GetSaveData()
    {
        return new LevelSaveData
        {
            levelName = _levelName,
            isUnlocked = _isUnlocked,
            isCompleted = _isCompleted,
            attemptsUsed = _attemptsUsed,
            bestScore = _bestScore,
            timesCompleted = _timesCompleted
        };
    }
    
    /// <summary>
    /// Load from save data
    /// </summary>
    public void LoadFromSaveData(LevelSaveData saveData)
    {
        if (saveData.levelName != _levelName)
        {
            Debug.LogWarning($"[Level] Save data name mismatch: {saveData.levelName} vs {_levelName}");
        }
        
        _isUnlocked = saveData.isUnlocked;
        _isCompleted = saveData.isCompleted;
        _attemptsUsed = saveData.attemptsUsed;
        _bestScore = saveData.bestScore;
        _timesCompleted = saveData.timesCompleted;
    }
    
    #endregion
    
    #region Scene Loading
    
    /// <summary>
    /// Load the game scene associated with this level
    /// </summary>
    public void LoadScene()
    {
        if (!HasValidScene())
        {
            Debug.LogError($"[Level] Cannot load scene for {_levelName} - no scene assigned!");
            return;
        }
        
        if (!CanPlay())
        {
            Debug.LogWarning($"[Level] Cannot play {_levelName} - level is locked or no attempts remaining!");
            return;
        }
        
        Debug.Log($"[Level] Loading scene: {_gameScene.SceneName} for level {_levelName}");
        UnityEngine.SceneManagement.SceneManager.LoadScene(_gameScene.SceneName);
    }
    
    /// <summary>
    /// Load the game scene asynchronously
    /// </summary>
    public void LoadSceneAsync(System.Action<UnityEngine.AsyncOperation> onComplete = null)
    {
        if (!HasValidScene())
        {
            Debug.LogError($"[Level] Cannot load scene for {_levelName} - no scene assigned!");
            return;
        }
        
        if (!CanPlay())
        {
            Debug.LogWarning($"[Level] Cannot play {_levelName} - level is locked or no attempts remaining!");
            return;
        }
        
        Debug.Log($"[Level] Loading scene async: {_gameScene.SceneName} for level {_levelName}");
        var asyncOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_gameScene.SceneName);
        
        if (onComplete != null && asyncOp != null)
        {
            asyncOp.completed += onComplete;
        }
    }

    #endregion

    #region Debug

    public override string ToString()
    {
        string status = _isCompleted ? "✓" : _isUnlocked ? "○" : "🔒";
        string sceneName = HasValidScene() ? $" [{_gameScene.SceneName}]" : " [No Scene]";
        return $"{status} {_levelName} (T{_tier}){sceneName} - Score: {_bestScore:F0}/{_completionThreshold}";
    }
    
    #endregion
    
    /// <summary>
    /// Save the asset
    /// </summary>
    public void Save()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
    #endif
}
}