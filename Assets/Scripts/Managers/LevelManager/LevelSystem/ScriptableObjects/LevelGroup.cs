using UnityEngine;

/// <summary>
/// Represents a group/category of skills in the skill tree
/// Similar to DialogueGroup in your dialogue system
/// </summary>
[CreateAssetMenu(fileName = "LevelGroup", menuName = "Skill Tree/Group")]
public class LevelGroup : ScriptableObject
{
     [SerializeField] private string _groupName;
     [SerializeField] private Sprite _groupImage;

    [SerializeField, TextArea] private string _description;
    [SerializeField] private int _tierLevel;
    [SerializeField] private Color _groupColor = Color.white;
    [SerializeField] private int _requiredSkillPoints;
    
    public string GroupName => _groupName;

    public Sprite GroupImage => _groupImage;
    public string Description => _description;
    public int TierLevel => _tierLevel;
    public Color GroupColor => _groupColor;
    public int RequiredSkillPoints => _requiredSkillPoints;
    
    /// <summary>
    /// Initialize the group with basic properties
    /// </summary>
    public void Initialize(string groupName, Sprite groupImage , string description = "", int tierLevel = 0)
    {
        _groupName = groupName;
        _groupImage = groupImage;
        _description = description;
        _tierLevel = tierLevel;
        _groupColor = Color.white;
        _requiredSkillPoints = 0;
    }
    
    /// <summary>
    /// Initialize the group with all properties
    /// </summary>
    public void Initialize(string groupName, Sprite groupImage , string description, int tierLevel, Color groupColor, int requiredSkillPoints)
    {
        _groupName = groupName;
        _groupImage = groupImage;
        _description = description;
        _tierLevel = tierLevel;
        _groupColor = groupColor;
        _requiredSkillPoints = requiredSkillPoints;
    }
}