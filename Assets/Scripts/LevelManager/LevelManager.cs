using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the level  system, including unlocking levels and managing level points
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level")]
    [SerializeField] private LevelContainer _levelContainer;

    [Header("Level Points")]
    [SerializeField] private int _currentLevelPoints = 0;
    [SerializeField] private int _totalLevelPointsEarned = 0;

    [Header("Player Stats")]
    [SerializeField] private Dictionary<StatType, float> _statModifiers = new Dictionary<StatType, float>();

    [Header("Unlocked Abilities")]
    [SerializeField] private List<string> _unlockedAbilities = new List<string>();

    // Events
    public event Action<Level> OnLevelUnlocked;
    public event Action<Level> OnLevelCompleted;
    public event Action<Level> OnLevelLevelUp;
    public event Action<int> OnLevelPointsChanged;
    public event Action<int, int> OnProgressChanged;
    public event Action<StatType, float> OnStatModified;
    public event Action<string> OnAbilityUnlocked;
    public event Action<string, string, object> OnCustomEvent;

    public int CurrentLevelPoints => _currentLevelPoints;
    public int TotalLevelPointsEarned => _totalLevelPointsEarned;
    public LevelContainer LevelContainer => _levelContainer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeStatModifiers();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeStatModifiers()
    {
        foreach (StatType stat in Enum.GetValues(typeof(StatType)))
        {
            _statModifiers[stat] = 0f;
        }
    }

    public void SetLevelContainer(LevelContainer container)
    {
        _levelContainer = container;
    }

    public bool TryUnlockLevel(Level level)
    {
        if (level == null)
        {
            Debug.LogWarning("[LevelManager] Cannot unlock null level");
            return false;
        }

        if (level.IsUnlocked)
        {
            Debug.LogWarning($"[LevelManager] Level '{level.LevelName}' is already unlocked");
            return false;
        }

        if (!level.CanUnlock())
        {
            Debug.LogWarning($"[LevelManager] Cannot unlock '{level.LevelName}' - prerequisites not met");
            return false;
        }

        if (_currentLevelPoints < level.UnlockCost)
        {
            Debug.LogWarning($"[LevelManager] Not enough level points to unlock '{level.LevelName}' (Need: {level.UnlockCost}, Have: {_currentLevelPoints})");
            return false;
        }

        // Spend points and unlock
        _currentLevelPoints -= level.UnlockCost;
        level.Unlock();

        Debug.Log($"[LevelManager] Unlocked level: {level.LevelName}");

        OnLevelUnlocked?.Invoke(level);
        OnLevelPointsChanged?.Invoke(_currentLevelPoints);

        return true;
    }

    public bool TryLevelUpLevel(Level level)
    {
        // Levels don't have leveling up - they are completed or not
        // This method is not applicable for Level system
        Debug.LogWarning($"[LevelManager] TryLevelUpLevel is not supported for Level '{level?.LevelName ?? "null"}'");
        return false;
    }

    public void AddLevelPoints(int amount)
    {
        _currentLevelPoints += amount;
        _totalLevelPointsEarned += amount;

        Debug.Log($"[LevelManager] Added {amount} level points. Total: {_currentLevelPoints}");

        OnLevelPointsChanged?.Invoke(_currentLevelPoints);
    }

    public void ModifyStat(StatType statType, ModifierType modifierType, float value)
    {
        if (!_statModifiers.ContainsKey(statType))
            _statModifiers[statType] = 0f;

        _statModifiers[statType] += value;

        Debug.Log($"[LevelManager] Modified {statType} by {value} ({modifierType})");

        OnStatModified?.Invoke(statType, _statModifiers[statType]);
    }

    public float GetStatModifier(StatType statType)
    {
        return _statModifiers.ContainsKey(statType) ? _statModifiers[statType] : 0f;
    }

    public void UnlockAbility(string abilityID)
    {
        if (!_unlockedAbilities.Contains(abilityID))
        {
            _unlockedAbilities.Add(abilityID);
            Debug.Log($"[LevelManager] Unlocked ability: {abilityID}");
            OnAbilityUnlocked?.Invoke(abilityID);
        }
    }

    public bool IsAbilityUnlocked(string abilityID)
    {
        return _unlockedAbilities.Contains(abilityID);
    }

    public void TriggerCustomEvent(string eventName, string eventParameter, object level)
    {
        Debug.Log($"[LevelManager] Custom event triggered: {eventName} with parameter: {eventParameter}");
        OnCustomEvent?.Invoke(eventName, eventParameter, level);
    }


    public void ResetLevel()
    {
        if (_levelContainer != null)
        {
            _levelContainer.ResetAllLevels();
        }

        // Refund all spent points
        _currentLevelPoints = _totalLevelPointsEarned;

        // Clear stat modifiers
        InitializeStatModifiers();

        // Clear unlocked abilities
        _unlockedAbilities.Clear();

        Debug.Log("[LevelManager] Level reset");

        OnLevelPointsChanged?.Invoke(_currentLevelPoints);
    }

    public List<Level> GetUnlockedLevels()
    {
        if (_levelContainer != null)
            return _levelContainer.GetUnlockedLevels();
        return new List<Level>();
    }

    public List<Level> GetAvailableLevels()
    {
        if (_levelContainer != null)
            return _levelContainer.GetUnlockedLevels(); // Assuming GetAvailableLevels is GetUnlockedLevels
        return new List<Level>();
    }

    public int GetCompletedLevelsCount()
    {
        if (_levelContainer != null)
            return _levelContainer.GetCompletedLevels().Count;
        return 0;
    }

    public int GetTotalLevelsCount()
    {
        if (_levelContainer != null)
            return _levelContainer.GetAllLevels().Count;
        return 0;
    }

    public void LoadLevel(Level level)
    {
        if (level == null)
        {
            Debug.LogWarning("[LevelManager] Cannot load null level");
            return;
        }

        Debug.Log($"[LevelManager] Loading level: {level.LevelName}");
        // TODO: Implement actual level loading logic based on LevelSceneType
        // For example: UnityEngine.SceneManagement.SceneManager.LoadScene(level.LevelName);
    }

    public void CompleteLevel(Level level, float score)
    {
        if (level == null)
        {
            Debug.LogWarning("[LevelManager] Cannot complete null level");
            return;
        }

        bool wasCompleted = level.IsCompleted;
        level.CompleteLevel(score);

        if (!wasCompleted && level.IsCompleted)
        {
            OnLevelCompleted?.Invoke(level);
            OnProgressChanged?.Invoke(GetCompletedLevelsCount(), GetTotalLevelsCount());
        }
    }

}