using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tooltip component for displaying level information on hover
/// </summary>
public class LevelTooltip : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private TextMeshProUGUI _tierText;
    [SerializeField] private TextMeshProUGUI _prerequisitesText;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private GameObject _completedBadge;
    [SerializeField] private GameObject _lockedBadge;
    
    [Header("Visual Settings")]
    [SerializeField] private Color _completedColor = new Color(1f, 0.84f, 0f);
    [SerializeField] private Color _unlockedColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color _lockedColor = new Color(0.8f, 0.2f, 0.2f);
    
    [Header("Layout Settings")]
    [SerializeField] private float _maxWidth = 300f;
    [SerializeField] private bool _followMouse = true;
    [SerializeField] private Vector2 _offset = new Vector2(10f, -10f);
    
    private RectTransform _rectTransform;
    private Canvas _canvas;
    private Level _currentLevel;
    
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
    }
    
    private void Update()
    {
        if (_followMouse)
        {
            UpdatePosition();
        }
    }
    
    /// <summary>
    /// Set the level to display in the tooltip
    /// </summary>
    public void SetLevel(Level level)
    {
        if (level == null)
        {
            Debug.LogWarning("[LevelTooltip] Cannot set null level!");
            return;
        }
        
        _currentLevel = level;
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
        
        // Force layout rebuild
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
    }
    
    private void UpdatePosition()
    {
        if (_canvas == null || _rectTransform == null)
            return;
        
        Vector2 mousePosition = Input.mousePosition;
        
        // Convert mouse position to canvas space
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            mousePosition,
            _canvas.worldCamera,
            out Vector2 localPoint
        );
        
        // Apply offset
        localPoint += _offset;
        
        // Clamp to canvas bounds
        RectTransform canvasRect = _canvas.transform as RectTransform;
        Vector2 canvasSize = canvasRect.rect.size;
        Vector2 tooltipSize = _rectTransform.rect.size;
        
        // Clamp X
        float minX = -canvasSize.x / 2 + tooltipSize.x / 2;
        float maxX = canvasSize.x / 2 - tooltipSize.x / 2;
        localPoint.x = Mathf.Clamp(localPoint.x, minX, maxX);
        
        // Clamp Y
        float minY = -canvasSize.y / 2 + tooltipSize.y / 2;
        float maxY = canvasSize.y / 2 - tooltipSize.y / 2;
        localPoint.y = Mathf.Clamp(localPoint.y, minY, maxY);
        
        _rectTransform.localPosition = localPoint;
    }
    
    /// <summary>
    /// Set whether the tooltip follows the mouse
    /// </summary>
    public void SetFollowMouse(bool follow)
    {
        _followMouse = follow;
    }
    
    /// <summary>
    /// Set the tooltip offset from mouse position
    /// </summary>
    public void SetOffset(Vector2 offset)
    {
        _offset = offset;
    }
}