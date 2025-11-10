using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// UI component for displaying a level node with completion tracking
/// </summary>
public class LevelNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI Components")]
    [SerializeField] private Image _lockedIcon;
    [SerializeField] private Image _unlockedIcon;
    [SerializeField] private Image _completedIcon;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _borderImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _progressText;
    [SerializeField] private GameObject _lockedOverlay;
    [SerializeField] private GameObject _completedBadge;
    [SerializeField] private Image _progressBar;
    
    [Header("State Colors")]
    [SerializeField] private Color _completedColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    [SerializeField] private Color _unlockedColor = new Color(0.2f, 0.5f, 0.8f, 1f);
    [SerializeField] private Color _lockedColor = Color.gray;
    [SerializeField] private Color _hoverColor = Color.white;
    
    [Header("Animation")]
    [SerializeField] private float _hoverScale = 1.1f;
    [SerializeField] private float _animationSpeed = 5f;
    
    private Level _level;
    private LevelUI _levelUI;
    private Color _originalColor;
    private bool _isHovering;
    private Vector3 _targetScale = Vector3.one;

    public void Initialize(Level level, LevelUI levelUI)
    {
        _level = level;
        _levelUI = levelUI;

        if (_level == null)
            return;

        // Set name
        if (_nameText != null)
            _nameText.text = _level.LevelName;
        
        // Set description (hide by default)
        if (_descriptionText != null)
        {
            _descriptionText.text = _level.Description;
            _descriptionText.gameObject.SetActive(false);
        }

        UpdateDisplay();
    }
    
    public void UpdateDisplay()
    {
        if (_level == null)
            return;
        
        // Determine current state
        LevelState state = GetLevelState();
        
        // Update icon visibility
        UpdateIconVisibility(state);
        
        // Update progress display
        UpdateProgressDisplay();
        
        // Update colors
        UpdateColors(state);
        
        // Update overlays
        UpdateOverlays(state);
        
        // Update progress bar
        UpdateProgressBar();
    }
    
    private LevelState GetLevelState()
    {
        if (_level.IsCompleted)
            return LevelState.Completed;
        else if (_level.IsUnlocked)
            return LevelState.Unlocked;
        else
            return LevelState.Locked;
    }
    
    private void UpdateIconVisibility(LevelState state)
    {
        if (_lockedIcon != null)
            _lockedIcon.gameObject.SetActive(state == LevelState.Locked);
        
        if (_unlockedIcon != null)
            _unlockedIcon.gameObject.SetActive(state == LevelState.Unlocked);
        
        if (_completedIcon != null)
            _completedIcon.gameObject.SetActive(state == LevelState.Completed);
        
        // Set appropriate sprite
        switch (state)
        {
            case LevelState.Locked:
                if (_lockedIcon != null && _level.LockedIcon != null)
                    _lockedIcon.sprite = _level.LockedIcon;
                break;
            case LevelState.Unlocked:
                if (_unlockedIcon != null && _level.Icon != null)
                    _unlockedIcon.sprite = _level.Icon;
                break;
            case LevelState.Completed:
                if (_completedIcon != null && _level.CompletedIcon != null)
                    _completedIcon.sprite = _level.CompletedIcon;
                else if (_completedIcon != null && _level.Icon != null)
                    _completedIcon.sprite = _level.Icon;
                break;
        }
    }
    
    private void UpdateProgressDisplay()
    {
        if (_progressText == null)
            return;
        
        if (_level.IsCompleted)
        {
            _progressText.text = $"✓ Completed ({_level.TimesCompleted}x)";
            _progressText.gameObject.SetActive(true);
        }
        else if (_level.IsUnlocked)
        {
            // Show attempts or progress
            if (_level.MaxAttempts > 0)
            {
                int remaining = _level.GetRemainingAttempts();
                _progressText.text = $"Attempts: {remaining}/{_level.MaxAttempts}";
            }
            else
            {
                float completion = _level.GetCompletionPercentage();
                if (completion > 0)
                    _progressText.text = $"Best: {completion:F0}%";
                else
                    _progressText.text = "Not Started";
            }
            _progressText.gameObject.SetActive(true);
        }
        else
        {
            _progressText.gameObject.SetActive(false);
        }
    }
    
    private void UpdateColors(LevelState state)
    {
        Color targetColor;

        switch (state)
        {
            case LevelState.Completed:
                targetColor = _completedColor;
                break;
            case LevelState.Unlocked:
                targetColor = _unlockedColor;
                break;
            default:
                targetColor = _lockedColor;
                break;
        }

        _originalColor = targetColor;

        if (!_isHovering && _borderImage != null)
        {
            _borderImage.color = targetColor;
        }

        // Update background alpha
        if (_backgroundImage != null)
        {
            Color bgColor = _backgroundImage.color;
            bgColor.a = state == LevelState.Locked ? 0.6f : 1f;
            _backgroundImage.color = bgColor;
        }
    }
    
    private void UpdateOverlays(LevelState state)
    {
        if (_lockedOverlay != null)
            _lockedOverlay.SetActive(state == LevelState.Locked);
        
        if (_completedBadge != null)
            _completedBadge.SetActive(state == LevelState.Completed);
    }
    
    private void UpdateProgressBar()
    {
        if (_progressBar == null)
            return;
        
        if (!_level.IsUnlocked || _level.IsCompleted)
        {
            _progressBar.gameObject.SetActive(false);
            return;
        }
        
        _progressBar.gameObject.SetActive(true);
        float completion = _level.GetCompletionPercentage() / 100f;
        _progressBar.fillAmount = completion;
        
        // Color the progress bar
        _progressBar.color = Color.Lerp(Color.red, Color.green, completion);
    }
    
    private void Update()
    {
        // Smooth scale animation
        if (transform.localScale != _targetScale)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale, 
                _targetScale, 
                Time.deltaTime * _animationSpeed
            );
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
        _targetScale = Vector3.one * _hoverScale;
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
        _targetScale = Vector3.one;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_levelUI != null && _level != null)
        {
            _levelUI.ShowLevelDetails(_level);
        }
    }
    
    /// <summary>
    /// Play completion animation
    /// </summary>
    public void PlayCompletionAnimation()
    {
        // Simple pulse animation
        StartCoroutine(CompletionPulseCoroutine());
    }
    
    private System.Collections.IEnumerator CompletionPulseCoroutine()
    {
        Vector3 originalScale = transform.localScale;
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.3f;
            transform.localScale = originalScale * scale;
            yield return null;
        }
        
        transform.localScale = originalScale;
        UpdateDisplay();
    }
    
    public Level GetLevel()
    {
        return _level;
    }
}

/// <summary>
/// Enum representing the visual state of a level
/// </summary>
public enum LevelState
{
    Locked,
    Unlocked,
    Completed
}