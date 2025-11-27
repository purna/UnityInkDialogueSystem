using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Game
{
/// <summary>
/// Manages the skill tree system, including unlocking skills and managing skill points
/// </summary>
public class SkillsTreeManager : MonoBehaviour
{
    public static SkillsTreeManager Instance { get; private set; }

    [Header("Skill Tree")]
    [SerializeField] private SkillsTreeContainer _skillTreeContainer;

    [Header("Skill Points")]
    [SerializeField] private int _currentSkillPoints = 0;
    [SerializeField] private int _totalSkillPointsEarned = 0;

    [Header("Player Stats")]
    [SerializeField] private Dictionary<StatType, float> _statModifiers = new Dictionary<StatType, float>();

    [Header("Unlocked Abilities")]
    [SerializeField] private List<string> _unlockedAbilities = new List<string>();

    // Events
    public event Action<Skill> OnSkillUnlocked;
    public event Action<Skill> OnSkillLevelUp;
    public event Action<int> OnSkillPointsChanged;
    public event Action<StatType, float> OnStatModified;
    public event Action<string> OnAbilityUnlocked;
    public event Action<string, string, object> OnCustomEvent;

    public int CurrentSkillPoints => _currentSkillPoints;
    public int TotalSkillPointsEarned => _totalSkillPointsEarned;
    public SkillsTreeContainer SkillTreeContainer => _skillTreeContainer;

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

    public void SetSkillTreeContainer(SkillsTreeContainer container)
    {
        _skillTreeContainer = container;
    }

    public bool TryUnlockSkill(Skill skill)
    {
        if (skill == null)
        {
            Debug.LogWarning("[SkillTreeManager] Cannot unlock null skill");
            return false;
        }

        if (skill.IsUnlocked)
        {
            Debug.LogWarning($"[SkillTreeManager] Skill '{skill.SkillName}' is already unlocked");
            return false;
        }

        if (!skill.CanUnlock())
        {
            Debug.LogWarning($"[SkillTreeManager] Cannot unlock '{skill.SkillName}' - prerequisites not met");
            return false;
        }

        if (_currentSkillPoints < skill.UnlockCost)
        {
            Debug.LogWarning($"[SkillTreeManager] Not enough skill points to unlock '{skill.SkillName}' (Need: {skill.UnlockCost}, Have: {_currentSkillPoints})");
            return false;
        }

        // Spend points and unlock
        _currentSkillPoints -= skill.UnlockCost;
        skill.Unlock();

        Debug.Log($"[SkillTreeManager] Unlocked skill: {skill.SkillName}");

        OnSkillUnlocked?.Invoke(skill);
        OnSkillPointsChanged?.Invoke(_currentSkillPoints);

        return true;
    }

    public bool TryLevelUpSkill(Skill skill)
    {
        if (skill == null || !skill.IsUnlocked)
            return false;

        if (skill.CurrentLevel >= skill.MaxLevel)
        {
            Debug.LogWarning($"[SkillTreeManager] Skill '{skill.SkillName}' is already at max level");
            return false;
        }

        if (_currentSkillPoints < skill.UnlockCost)
        {
            Debug.LogWarning($"[SkillTreeManager] Not enough skill points to level up '{skill.SkillName}'");
            return false;
        }

        _currentSkillPoints -= skill.UnlockCost;
        skill.LevelUp();

        Debug.Log($"[SkillTreeManager] Leveled up skill: {skill.SkillName} to level {skill.CurrentLevel}");

        OnSkillLevelUp?.Invoke(skill);
        OnSkillPointsChanged?.Invoke(_currentSkillPoints);

        return true;
    }

    public void AddSkillPoints(int amount)
    {
        _currentSkillPoints += amount;
        _totalSkillPointsEarned += amount;

        Debug.Log($"[SkillTreeManager] Added {amount} skill points. Total: {_currentSkillPoints}");

        OnSkillPointsChanged?.Invoke(_currentSkillPoints);
    }

    public void ModifyStat(StatType statType, ModifierType modifierType, float value)
    {
        if (!_statModifiers.ContainsKey(statType))
            _statModifiers[statType] = 0f;

        _statModifiers[statType] += value;

        Debug.Log($"[SkillTreeManager] Modified {statType} by {value} ({modifierType})");

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
            Debug.Log($"[SkillTreeManager] Unlocked ability: {abilityID}");
            OnAbilityUnlocked?.Invoke(abilityID);
        }
    }

    public bool IsAbilityUnlocked(string abilityID)
    {
        return _unlockedAbilities.Contains(abilityID);
    }

    public void TriggerCustomEvent(string eventName, string eventParameter, object skill)
    {
        Debug.Log($"[SkillTreeManager] Custom event triggered: {eventName} with parameter: {eventParameter}");
        OnCustomEvent?.Invoke(eventName, eventParameter, skill);
    }


    public void ResetSkillTree()
    {
        if (_skillTreeContainer != null)
        {
            _skillTreeContainer.ResetAllSkills();
        }

        // Refund all spent points
        _currentSkillPoints = _totalSkillPointsEarned;

        // Clear stat modifiers
        InitializeStatModifiers();

        // Clear unlocked abilities
        _unlockedAbilities.Clear();

        Debug.Log("[SkillTreeManager] Skill tree reset");

        OnSkillPointsChanged?.Invoke(_currentSkillPoints);
    }

    public List<Skill> GetUnlockedSkills()
    {
        if (_skillTreeContainer != null)
            return _skillTreeContainer.GetUnlockedSkills();
        return new List<Skill>();
    }

    public List<Skill> GetAvailableSkills()
    {
        if (_skillTreeContainer != null)
            return _skillTreeContainer.GetAvailableSkills();
        return new List<Skill>();
    }
}
} // End namespace Core.Game