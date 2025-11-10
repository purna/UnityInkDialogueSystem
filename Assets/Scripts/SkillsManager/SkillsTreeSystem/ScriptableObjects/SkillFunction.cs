using UnityEngine;

/// <summary>
/// Base class for skill functions that execute when a skill is unlocked
/// </summary>
public abstract class SkillFunction : ScriptableObject
{
    [SerializeField] private string _functionName;
    [SerializeField, TextArea] private string _description;
    
    public string FunctionName => _functionName;
    public string Description => _description;
    
    /// <summary>
    /// Execute the function for a Skill
    /// </summary>
    public abstract void Execute(Skill skill);
}

/// <summary>
/// Function that modifies a player stat
/// </summary>
[CreateAssetMenu(fileName = "StatModifierFunction", menuName = "Skill Tree/Functions/Stat Modifier")]
public class StatModifierFunction : SkillFunction
{
    [SerializeField] private StatType _statType;
    [SerializeField] private ModifierType _modifierType;
    [SerializeField] private bool _useSkillValue = true;
    [SerializeField] private float _customValue;
    
    public override void Execute(Skill skill)
    {
        float value = _useSkillValue ? skill.GetScaledValue() : _customValue;
        SkillTreeManager.Instance?.ModifyStat(_statType, _modifierType, value);
        Debug.Log($"[SkillFunction] Modified {_statType} by {value} ({_modifierType}) for skill '{skill.SkillName}'");
    }
}

/// <summary>
/// Function that unlocks a new ability
/// </summary>
[CreateAssetMenu(fileName = "UnlockAbilityFunction", menuName = "Skill Tree/Functions/Unlock Ability")]
public class UnlockAbilityFunction : SkillFunction
{
    [SerializeField] private string _abilityID;
    
    public override void Execute(Skill skill)
    {
        SkillTreeManager.Instance?.UnlockAbility(_abilityID);
        Debug.Log($"[SkillFunction] Unlocked ability: {_abilityID} from skill '{skill.SkillName}'");
    }
}

/// <summary>
/// Function that triggers a custom event
/// </summary>
[CreateAssetMenu(fileName = "CustomEventFunction", menuName = "Skill Tree/Functions/Custom Event")]
public class CustomEventFunction : SkillFunction
{
    [SerializeField] private string _eventName;
    [SerializeField] private string _eventParameter;
    
    public override void Execute(Skill skill)
    {
        SkillTreeManager.Instance?.TriggerCustomEvent(_eventName, _eventParameter, skill);
        Debug.Log($"[SkillFunction] Triggered event: {_eventName} from skill '{skill.SkillName}'");
    }
}