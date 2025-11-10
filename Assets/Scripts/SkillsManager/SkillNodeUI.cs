using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Enhanced UI component for a single skill node with locked/unlocked icon support
/// </summary>
public class SkillNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI Components")]
    [SerializeField] private Image _lockedIcon;
    [SerializeField] private Image _unlockedIcon;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _borderImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private GameObject _lockedOverlay;
    
    [Header("Colors")]
    [SerializeField] private Color _unlockedColor = Color.green;
    [SerializeField] private Color _availableColor = Color.yellow;
    [SerializeField] private Color _lockedColor = Color.gray;
    [SerializeField] private Color _hoverColor = Color.white;
    
    private Skill _skill;
    private SkillTreeUI _skillTreeUI;
    private Color _originalColor;
    private bool _isHovering;

    public void Initialize(Skill skill, SkillTreeUI skillTreeUI)
    {
        _skill = skill;
        _skillTreeUI = skillTreeUI;

        if (_skill == null)
            return;

        // Set name and description
        if (_nameText != null)
            _nameText.text = _skill.SkillName;
        
        if (_descriptionText != null)
        {
            _descriptionText.text = _skill.Description;
            // Hide description by default, show on hover
            _descriptionText.gameObject.SetActive(false);
        }

        // Set locked icon
        if (_lockedIcon != null && _skill.LockedIcon != null)
            _lockedIcon.sprite = _skill.LockedIcon;
        else if (_lockedIcon != null && _skill.Icon != null)
            _lockedIcon.sprite = _skill.Icon;
        
        // Set unlocked icon
        if (_unlockedIcon != null && _skill.UnlockedIcon != null)
            _unlockedIcon.sprite = _skill.UnlockedIcon;
        else if (_unlockedIcon != null && _skill.Icon != null)
            _unlockedIcon.sprite = _skill.Icon;

        UpdateDisplay();
    }
    
    public void UpdateDisplay()
    {
        if (_skill == null)
            return;
        
        // Show/hide appropriate icon based on unlock state
        if (_lockedIcon != null)
            _lockedIcon.gameObject.SetActive(!_skill.IsUnlocked);
        
        if (_unlockedIcon != null)
            _unlockedIcon.gameObject.SetActive(_skill.IsUnlocked);
        
        // Update level text
        if (_levelText != null)
        {
            if (_skill.IsUnlocked && _skill.MaxLevel > 1)
            {
                _levelText.text = $"{_skill.CurrentLevel}/{_skill.MaxLevel}";
                _levelText.gameObject.SetActive(true);
            }
            else
            {
                _levelText.gameObject.SetActive(false);
            }
        }

        // Update locked overlay
        if (_lockedOverlay != null)
            _lockedOverlay.SetActive(!_skill.IsUnlocked);

        // Update colors based on state
        Color targetColor;

        if (_skill.IsUnlocked)
        {
            targetColor = _unlockedColor;
        }
        else if (_skill.CanUnlock())
        {
            targetColor = _availableColor;
        }
        else
        {
            targetColor = _lockedColor;
        }

        _originalColor = targetColor;

        if (!_isHovering)
        {
            if (_borderImage != null)
                _borderImage.color = targetColor;
        }

        // Update icon transparency
        if (_lockedIcon != null)
        {
            Color iconColor = _lockedIcon.color;
            iconColor.a = _skill.IsUnlocked ? 0.3f : 1f;
            _lockedIcon.color = iconColor;
        }
        
        if (_unlockedIcon != null)
        {
            Color iconColor = _unlockedIcon.color;
            iconColor.a = _skill.IsUnlocked ? 1f : 0.3f;
            _unlockedIcon.color = iconColor;
        }
        
        // Update background
        if (_backgroundImage != null)
        {
            Color bgColor = _backgroundImage.color;
            bgColor.a = _skill.IsUnlocked ? 1f : 0.6f;
            _backgroundImage.color = bgColor;
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovering = true;
        
        if (_borderImage != null)
            _borderImage.color = _hoverColor;
        
        // Show description on hover
        if (_descriptionText != null)
            _descriptionText.gameObject.SetActive(true);
        
        // Scale up effect
        transform.localScale = Vector3.one * 1.1f;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovering = false;
        
        if (_borderImage != null)
            _borderImage.color = _originalColor;
        
        // Hide description
        if (_descriptionText != null)
            _descriptionText.gameObject.SetActive(false);
        
        // Restore scale
        transform.localScale = Vector3.one;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_skillTreeUI != null && _skill != null)
        {
            _skillTreeUI.ShowSkillDetails(_skill);
        }
    }
    
    public Skill GetSkill()
    {
        return _skill;
    }
}