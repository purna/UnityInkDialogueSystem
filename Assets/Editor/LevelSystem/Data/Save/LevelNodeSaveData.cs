using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelNodeSaveData
{
    [SerializeField] private string _id;
    [SerializeField] private string _name;
    [SerializeField] private string _text;
    [SerializeField] private List<LevelChoiceSaveData> _choices;
    [SerializeField] private string _groupID;
    [SerializeField] private LevelType _levelType;
    [SerializeField] private Vector2 _position;
    
    // Visual data
    [SerializeField] private Sprite _icon;
    [SerializeField] private Sprite _lockedIcon;
    [SerializeField] private Sprite _unlockedIcon;
    [SerializeField] private Sprite _completedIcon;
    [SerializeField] private string _description;
    
    // Level properties
    [SerializeField] private int _tier;
    [SerializeField] private int _levelIndex;
    [SerializeField] private float _completionThreshold;
    [SerializeField] private int _maxAttempts;
    [SerializeField] private LevelSceneType _levelSceneType;
    
    // Scene reference (stored as string path)
    [SerializeField] private string _gameScenePath;
    [SerializeField] private string _gameSceneName;

    public string ID => _id;
    public string Name => _name;
    public string Text => _text;
    public IEnumerable<LevelChoiceSaveData> Choices => _choices;
    public string GroupID => _groupID;
    public LevelType LevelType => _levelType;
    public Vector2 Position => _position;
    
    // Visual data properties
    public Sprite Icon => _icon;
    public Sprite LockedIcon => _lockedIcon;
    public Sprite UnlockedIcon => _unlockedIcon;
    public Sprite CompletedIcon => _completedIcon;
    public string Description => _description;
    
    // Level properties
    public int Tier => _tier;
    public int LevelIndex => _levelIndex;
    public float CompletionThreshold => _completionThreshold;
    public int MaxAttempts => _maxAttempts;
    public LevelSceneType LevelSceneType => _levelSceneType;
    
    // Scene properties
    public string GameScenePath => _gameScenePath;
    public string GameSceneName => _gameSceneName;

    public LevelNodeSaveData(string id, string name, string text, List<LevelChoiceSaveData> choices, 
                            string groupID, LevelType levelType, Vector2 position)
    {
        _id = id;
        _name = name;
        _text = text;
        _choices = choices;
        _groupID = groupID;
        _levelType = levelType;
        _position = position;

        
        // Initialize defaults
        _tier = 0;
        _levelIndex = 0;
        _completionThreshold = 100f;
        _maxAttempts = -1;
        _levelSceneType = LevelSceneType.Normal;
    }

    /// <summary>
    /// Update visual data from node
    /// </summary>
    public void UpdateVisualData(Sprite icon, Sprite lockedIcon, Sprite unlockedIcon, 
                                Sprite completedIcon, string description)
    {
        _icon = icon;
        _lockedIcon = lockedIcon;
        _unlockedIcon = unlockedIcon;
        _completedIcon = completedIcon;
        _description = description;
    }

    /// <summary>
    /// Update level properties from node
    /// </summary>
    public void UpdateLevelProperties(int tier, int levelIndex, float completionThreshold, int maxAttempts)
    {
        _tier = tier;
        _levelIndex = levelIndex;
        _completionThreshold = completionThreshold;
        _maxAttempts = maxAttempts;
    }

    /// <summary>
/// Update scene data from node
/// </summary>
public void UpdateSceneData(SceneField sceneField)
{
    if (sceneField != null)
    {
        _gameSceneName = sceneField.SceneName;
        
        // Try to get the scene path
        #if UNITY_EDITOR
        if (!string.IsNullOrEmpty(sceneField.SceneName))
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:SceneAsset {sceneField.SceneName}");
            if (guids.Length > 0)
            {
                _gameScenePath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            }
        }
        #endif
    }
    else
    {
        _gameScenePath = null;
        _gameSceneName = null;
    }
}


    /// <summary>
    /// Update level scene type from node
    /// </summary>
    public void UpdateLevelSceneType(LevelSceneType levelSceneType)
    {
        _levelSceneType = levelSceneType;
    }

    /// <summary>
    /// Update scene data from node
    /// </summary>
    public void UpdateSceneData(string scenePath, string sceneName)
    {
        _gameScenePath = scenePath;
        _gameSceneName = sceneName;
    }

    /// <summary>
    /// Update all data from Level ScriptableObject
    /// </summary>
    public void UpdateFromLevel(Level level, string assetPath)
    {
        if (level == null) return;

        // Visual data
        _icon = level.Icon;
        _lockedIcon = level.LockedIcon;
        _unlockedIcon = level.UnlockedIcon;
        _completedIcon = level.CompletedIcon;
        _description = level.Description;
        
        // Level properties
        _tier = level.Tier;
        _levelIndex = level.LevelIndex;
        _completionThreshold = level.CompletionThreshold;
        _maxAttempts = level.MaxAttempts;
        _levelSceneType = level.LevelSceneType;
        
        // Scene data
        if (level.GameScene != null)
        {
            _gameSceneName = level.GameSceneName;
            
            // Try to get the scene path
            if (!string.IsNullOrEmpty(level.GameSceneName))
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:SceneAsset {level.GameSceneName}");
                if (guids.Length > 0)
                {
                    _gameScenePath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                }
            }
        }
        else
        {
            _gameScenePath = null;
            _gameSceneName = null;
        }
        
        _position = level.Position;
    }
}