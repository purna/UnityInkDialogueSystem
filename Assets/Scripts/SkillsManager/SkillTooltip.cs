using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

/// <summary>
/// Tooltip component that displays detailed skill information
/// </summary>
public class SkillTooltip : MonoBehaviour
{
    [Header("Tooltip UI Elements")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _statsText;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Image _backgroundImage;
    
    [Header("Prerequisite Display")]
    [SerializeField] private Transform _prerequisitesContainer;
    [SerializeField] private GameObject _prerequisiteItemPrefab;
    
    [Header("Visual Settings")]
    [SerializeField] private Color _unlockedColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color _lockedColor = new Color(0.8f, 0.2f, 0.2f);
    [SerializeField] private Color _availableColor = new Color(1f, 0.8f, 0f);
    
    private Skill _skill;
    private SkillsTreeController _controller;
    
    /// <summary>
    /// Set the skill data to display in the tooltip
    /// </summary>
    public void SetSkillData(Skill skill, SkillsTreeController controller)
    {
        _skill = skill;
        _controller = controller;
        
        if (_skill == null)
        {
            Debug.LogError("[SkillTooltip] Cannot set null skill data!");
            return;
        }
        
        UpdateTooltipDisplay();
    }
    
    private void UpdateTooltipDisplay()
    {
        // Set title
        if (_titleText != null)
        {
            _titleText.text = _skill.SkillName;
        }
        
        // Set description
        if (_descriptionText != null)
        {
            _descriptionText.text = _skill.Description;
        }
        
        // Set icon
        if (_iconImage != null)
        {
            if (_skill.IsUnlocked && _skill.UnlockedIcon != null)
                _iconImage.sprite = _skill.UnlockedIcon;
            else if (!_skill.IsUnlocked && _skill.LockedIcon != null)
                _iconImage.sprite = _skill.LockedIcon;
            else if (_skill.Icon != null)
                _iconImage.sprite = _skill.Icon;
            
            _iconImage.gameObject.SetActive(_iconImage.sprite != null);
        }
        
        // Set stats
        if (_statsText != null)
        {
            StringBuilder statsBuilder = new StringBuilder();
            
            statsBuilder.AppendLine($"<b>Tier:</b> {_skill.Tier}");
            statsBuilder.AppendLine($"<b>Type:</b> {_skill.SkillType}");
            
            if (_skill.MaxLevel > 1)
            {
                statsBuilder.AppendLine($"<b>Max Level:</b> {_skill.MaxLevel}");
            }
            
            if (_skill.Value != 0)
            {
                if (_skill.IsUnlocked)
                {
                    statsBuilder.AppendLine($"<b>Value:</b> {_skill.GetScaledValue():F1} ({_skill.GetBaseValue():F1} × {_skill.CurrentLevel})");
                }
                else
                {
                    statsBuilder.AppendLine($"<b>Value:</b> {_skill.GetBaseValue():F1}");
                }
            }
            
            _statsText.text = statsBuilder.ToString();
        }
        
        // Set cost
        if (_costText != null)
        {
            if (_skill.IsUnlocked && _skill.CurrentLevel < _skill.MaxLevel)
            {
                _costText.text = $"Level Up Cost: {_skill.UnlockCost} SP";
            }
            else if (!_skill.IsUnlocked)
            {
                _costText.text = $"Unlock Cost: {_skill.UnlockCost} SP";
            }
            else
            {
                _costText.text = "Max Level Reached";
            }
        }
        
        // Set level
        if (_levelText != null)
        {
            if (_skill.IsUnlocked)
            {
                _levelText.text = $"Level: {_skill.CurrentLevel} / {_skill.MaxLevel}";
                _levelText.gameObject.SetActive(true);
            }
            else
            {
                _levelText.gameObject.SetActive(false);
            }
        }
        
        // Set status
        if (_statusText != null)
        {
            Color statusColor;
            string statusMessage;
            
            if (_skill.IsUnlocked)
            {
                statusColor = _unlockedColor;
                statusMessage = _skill.CurrentLevel >= _skill.MaxLevel ? "✓ MAXED" : "✓ UNLOCKED";
            }
            else if (_skill.CanUnlock())
            {
                statusColor = _availableColor;
                statusMessage = "AVAILABLE";
            }
            else
            {
                statusColor = _lockedColor;
                statusMessage = "✗ LOCKED";
            }
            
            _statusText.text = statusMessage;
            _statusText.color = statusColor;
        }
        
        // Display prerequisites
        DisplayPrerequisites();
        
        // Update background color based on status
        if (_backgroundImage != null)
        {
            Color bgColor = _backgroundImage.color;
            
            if (_skill.IsUnlocked)
                bgColor = new Color(_unlockedColor.r, _unlockedColor.g, _unlockedColor.b, 0.9f);
            else if (_skill.CanUnlock())
                bgColor = new Color(_availableColor.r, _availableColor.g, _availableColor.b, 0.9f);
            else
                bgColor = new Color(_lockedColor.r, _lockedColor.g, _lockedColor.b, 0.9f);
            
            _backgroundImage.color = bgColor;
        }
    }
    
    private void DisplayPrerequisites()
    {
        if (_prerequisitesContainer == null)
            return;
        
        // Clear existing prerequisite items
        foreach (Transform child in _prerequisitesContainer)
        {
            Destroy(child.gameObject);
        }
        
        // If no prerequisites, hide container
        if (_skill.Prerequisites == null || _skill.Prerequisites.Count == 0)
        {
            _prerequisitesContainer.gameObject.SetActive(false);
            return;
        }
        
        _prerequisitesContainer.gameObject.SetActive(true);
        
        // Create prerequisite items
        foreach (var prereq in _skill.Prerequisites)
        {
            if (prereq == null)
                continue;
            
            GameObject prereqItem;
            
            if (_prerequisiteItemPrefab != null)
            {
                prereqItem = Instantiate(_prerequisiteItemPrefab, _prerequisitesContainer);
            }
            else
            {
                // Create simple text item
                prereqItem = new GameObject("PrerequisiteItem");
                prereqItem.transform.SetParent(_prerequisitesContainer);
                var text = prereqItem.AddComponent<TextMeshProUGUI>();
                text.fontSize = 12;
            }
            
            // Set prerequisite text
            TextMeshProUGUI prereqText = prereqItem.GetComponent<TextMeshProUGUI>();
            if (prereqText != null)
            {
                string checkmark = prereq.IsUnlocked ? "✓" : "✗";
                Color textColor = prereq.IsUnlocked ? _unlockedColor : _lockedColor;
                
                prereqText.text = $"{checkmark} {prereq.SkillName}";
                prereqText.color = textColor;
            }
            
            // Set prerequisite icon if available
            Image prereqIcon = prereqItem.GetComponentInChildren<Image>();
            if (prereqIcon != null && prereq.Icon != null)
            {
                prereqIcon.sprite = prereq.IsUnlocked ? 
                    (prereq.UnlockedIcon ?? prereq.Icon) : 
                    (prereq.LockedIcon ?? prereq.Icon);
            }
        }
    }
    
    /// <summary>
    /// Refresh the tooltip display (useful if skill state changes while tooltip is visible)
    /// </summary>
    public void RefreshDisplay()
    {
        if (_skill != null)
        {
            UpdateTooltipDisplay();
        }
    }
}