using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simplified tooltip component for prefab-based tooltips
/// This is a lightweight alternative to LevelTooltip that can be used on prefabs
/// Automatically populates fields when SetLevel is called
/// </summary>
public class SimpleLevelTooltip : MonoBehaviour
{
    [Header("UI References - Auto-Find")]
    [Tooltip("Leave empty to auto-find by name")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private TextMeshProUGUI _tierText;
    [SerializeField] private TextMeshProUGUI _prerequisitesText;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private GameObject _completedBadge;
    [SerializeField] private GameObject _lockedBadge;
    
    [Header("Auto-Find Settings")]
    [SerializeField] private bool _autoFindComponents = true;
    
    [Header("Visual Settings")]
    [SerializeField] private Color _completedColor = new Color(1f, 0.84f, 0f);
    [SerializeField] private Color _unlockedColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color _lockedColor = new Color(0.8f, 0.2f, 0.2f);
    
    private Level _currentLevel;
    private bool _componentsFound = false;

    private void Awake()
    {
        if (_autoFindComponents)
        {
            AutoFindComponents();
        }
    }

    /// <summary>
    /// Automatically find common child components by name
    /// </summary>
    private void AutoFindComponents()
    {
        // Find text components
        if (_titleText == null)
            _titleText = FindChildComponent<TextMeshProUGUI>("Title", "TitleText", "Name");
        
        if (_descriptionText == null)
            _descriptionText = FindChildComponent<TextMeshProUGUI>("Description", "DescriptionText");
        
        if (_statusText == null)
            _statusText = FindChildComponent<TextMeshProUGUI>("Status", "StatusText");
        
        if (_tierText == null)
            _tierText = FindChildComponent<TextMeshProUGUI>("Tier", "TierText", "Level");
        
        if (_prerequisitesText == null)
            _prerequisitesText = FindChildComponent<TextMeshProUGUI>("Prerequisites", "PrerequisitesText", "Prereqs");
        
        // Find image components
        if (_iconImage == null)
            _iconImage = FindChildComponent<Image>("Icon", "IconImage");
        
        if (_backgroundImage == null)
            _backgroundImage = FindChildComponent<Image>("Background", "BG");
        
        // Find badge objects
        if (_completedBadge == null)
            _completedBadge = FindChildObject("CompletedBadge", "Completed", "CheckMark");
        
        if (_lockedBadge == null)
            _lockedBadge = FindChildObject("LockedBadge", "Locked", "Lock");
        
        _componentsFound = true;
    }

    /// <summary>
    /// Find a child component by trying multiple possible names
    /// </summary>
    private T FindChildComponent<T>(params string[] possibleNames) where T : Component
    {
        foreach (string name in possibleNames)
        {
            Transform child = transform.Find(name);
            if (child != null)
            {
                T component = child.GetComponent<T>();
                if (component != null)
                    return component;
            }
        }
        
        // If not found by name, try getting first child with component
        T[] components = GetComponentsInChildren<T>(true);
        if (components.Length > 0)
            return components[0];
        
        return null;
    }

    /// <summary>
    /// Find a child GameObject by trying multiple possible names
    /// </summary>
    private GameObject FindChildObject(params string[] possibleNames)
    {
        foreach (string name in possibleNames)
        {
            Transform child = transform.Find(name);
            if (child != null)
                return child.gameObject;
        }
        return null;
    }

    /// <summary>
    /// Set the level to display in the tooltip
    /// </summary>
    public void SetLevel(Level level)
    {
        if (level == null)
        {
            Debug.LogWarning("[SimpleLevelTooltip] Cannot set null level!");
            return;
        }
        
        _currentLevel = level;
        
        // Make sure components are found
        if (!_componentsFound && _autoFindComponents)
        {
            AutoFindComponents();
        }
        
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (_currentLevel == null)
            return;
        
        // Title
        if (_titleText != null)
        {
            _titleText.text = _currentLevel.LevelName;
        }
        
        // Description
        if (_descriptionText != null)
        {
            if (!string.IsNullOrEmpty(_currentLevel.Description))
            {
                _descriptionText.text = _currentLevel.Description;
                _descriptionText.gameObject.SetActive(true);
            }
            else
            {
                _descriptionText.gameObject.SetActive(false);
            }
        }
        
        // Status
        if (_statusText != null)
        {
            string status;
            Color statusColor;
            
            if (_currentLevel.IsCompleted)
            {
                status = "✓ Completed";
                statusColor = _completedColor;
            }
            else if (_currentLevel.IsUnlocked)
            {
                status = "Unlocked - Ready to Play";
                statusColor = _unlockedColor;
            }
            else if (_currentLevel.CanUnlock())
            {
                status = "Available to Unlock";
                statusColor = _unlockedColor;
            }
            else
            {
                status = "🔒 Locked";
                statusColor = _lockedColor;
            }
            
            _statusText.text = status;
            _statusText.color = statusColor;
        }
        
        // Tier
        if (_tierText != null)
        {
            _tierText.text = $"Tier {_currentLevel.Tier} - {_currentLevel.LevelSceneType}";
        }
        
        // Prerequisites
        if (_prerequisitesText != null)
        {
            if (_currentLevel.Prerequisites.Count > 0)
            {
                string prereqText = "Prerequisites:\n";
                foreach (var prereq in _currentLevel.Prerequisites)
                {
                    if (prereq != null)
                    {
                        string prereqStatus = prereq.IsCompleted ? "✓" : "✗";
                        prereqText += $"{prereqStatus} {prereq.LevelName}\n";
                    }
                }
                _prerequisitesText.text = prereqText.TrimEnd('\n');
                _prerequisitesText.gameObject.SetActive(true);
            }
            else
            {
                _prerequisitesText.gameObject.SetActive(false);
            }
        }
        
        // Icon
        if (_iconImage != null)
        {
            Sprite iconToUse = null;
            
            if (_currentLevel.IsCompleted && _currentLevel.CompletedIcon != null)
            {
                iconToUse = _currentLevel.CompletedIcon;
            }
            else if (_currentLevel.IsUnlocked && _currentLevel.UnlockedIcon != null)
            {
                iconToUse = _currentLevel.UnlockedIcon;
            }
            else if (!_currentLevel.IsUnlocked && _currentLevel.LockedIcon != null)
            {
                iconToUse = _currentLevel.LockedIcon;
            }
            else if (_currentLevel.Icon != null)
            {
                iconToUse = _currentLevel.Icon;
            }
            
            if (iconToUse != null)
            {
                _iconImage.sprite = iconToUse;
                _iconImage.gameObject.SetActive(true);
            }
            else
            {
                _iconImage.gameObject.SetActive(false);
            }
        }
        
        // Badges
        if (_completedBadge != null)
        {
            _completedBadge.SetActive(_currentLevel.IsCompleted);
        }
        
        if (_lockedBadge != null)
        {
            _lockedBadge.SetActive(!_currentLevel.IsUnlocked);
        }
        
        // Background color tint
        if (_backgroundImage != null)
        {
            Color bgColor = _backgroundImage.color;
            
            if (_currentLevel.IsCompleted)
            {
                bgColor = Color.Lerp(bgColor, _completedColor, 0.2f);
            }
            else if (_currentLevel.IsUnlocked)
            {
                bgColor = Color.Lerp(bgColor, _unlockedColor, 0.1f);
            }
            else
            {
                bgColor = Color.Lerp(bgColor, _lockedColor, 0.1f);
            }
            
            _backgroundImage.color = bgColor;
        }
    }
}