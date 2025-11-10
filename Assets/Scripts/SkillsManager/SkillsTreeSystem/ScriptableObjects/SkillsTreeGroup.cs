using UnityEngine;

/// <summary>
/// Represents a group/category of skills in the skill tree
/// Similar to DialogueGroup in your dialogue system
/// </summary>
[CreateAssetMenu(fileName = "SkillsTreeGroup", menuName = "Skill Tree/Group")]
public class SkillsTreeGroup : ScriptableObject
{
    [SerializeField] private string _groupName;
    [SerializeField, TextArea] private string _description;
    [SerializeField] private int _tierLevel;
    [SerializeField] private Color _groupColor = Color.white;
    [SerializeField] private int _requiredSkillPoints;
    
    public string GroupName => _groupName;
    public string Description => _description;
    public int TierLevel => _tierLevel;
    public Color GroupColor => _groupColor;
    public int RequiredSkillPoints => _requiredSkillPoints;
    
    /// <summary>
    /// Initialize the group with basic properties
    /// </summary>
    public void Initialize(string groupName, string description = "", int tierLevel = 0)
    {
        _groupName = groupName;
        _description = description;
        _tierLevel = tierLevel;
        _groupColor = Color.white;
        _requiredSkillPoints = 0;
    }
    
    /// <summary>
    /// Initialize the group with all properties
    /// </summary>
    public void Initialize(string groupName, string description, int tierLevel, Color groupColor, int requiredSkillPoints)
    {
        _groupName = groupName;
        _description = description;
        _tierLevel = tierLevel;
        _groupColor = groupColor;
        _requiredSkillPoints = requiredSkillPoints;
    }
}