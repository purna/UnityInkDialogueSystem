using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Individual level node UI component - handles display and interaction
/// Now supports manual level selection from available levels in the controller
/// Enhanced with click feedback, hover effects, and tooltip integration
/// </summary>
public class LevelNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private Image _lockedSprite;
    [SerializeField] private Image _unlockedSprite;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _borderImage;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private GameObject _lockedOverlay;

    [Header("Visual Settings")]
    [SerializeField] private Color _unlockedColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color _availableColor = new Color(1f, 0.8f, 0f);
    [SerializeField] private Color _lockedColor = new Color(0.4f, 0.4f, 0.4f);
    [SerializeField] private Color _hoverColor = Color.white;
    [SerializeField] private Color _selectedColor = new Color(0f, 0.7f, 1f); // Cyan for selection
    [SerializeField] private float _hoverScale = 1.1f;
    [SerializeField] private float _clickScale = 0.95f;
    [SerializeField] private float _clickDuration = 0.15f;

    [Header("Click Feedback")]
    [SerializeField] private Sprite _clickedSprite;
    [SerializeField] private Color _clickedBackgroundColor = new Color(1f, 1f, 1f, 0.3f);
    
    [Header("Hover Tooltip")]
    [SerializeField] private GameObject _hoverTooltipPrefab;
    [SerializeField] private Vector2 _tooltipOffset = new Vector2(10f, 10f);
    [SerializeField] private bool _showHoverTooltip = true;

    [Header("Manual Level Assignment")]
    [SerializeField] private LevelController _controller;
    [Tooltip("Select which level this node represents")]
    [SerializeField] private int _levelIndex = -1;

    [SerializeField] private Level _level;
    private Color _originalBorderColor;
    private Color _originalBackgroundColor;
    private Vector3 _originalScale;
    private bool _isHovering;
    private bool _isSelected;
    private GameObject _hoverTooltipInstance;
    private Sprite _originalSprite;

    private void Awake()
    {
        _originalScale = transform.localScale;

        // Store original colors
        if (_borderImage != null)
        {
            _originalBorderColor = _borderImage.color;
        }
        
        if (_backgroundImage != null)
        {
            _originalBackgroundColor = _backgroundImage.color;
        }
        
        // Store original sprite
        if (_unlockedSprite != null)
        {
            _originalSprite = _unlockedSprite.sprite;
        }
        
        // Set up button listener
        if (_button == null)
        {
            _button = GetComponent<Button>();
        }
        
        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogWarning($"[LevelNode] {gameObject.name}: No Button component found! Add a Button component to make this node clickable.");
        }
    }

    private void Start()
    {
        // Auto-initialize if controller is assigned and level index is set
        if (_controller != null && _levelIndex >= 0 && _level == null)
        {
            AutoAssignLevelFromController();
        }
    }

    /// <summary>
    /// Automatically assign level based on the selected index from controller
    /// </summary>
    private void AutoAssignLevelFromController()
    {
        if (_controller == null)
        {
            Debug.LogWarning($"[LevelNode] {gameObject.name}: No controller assigned!");
            return;
        }

        List<Level> availableLevels = _controller.GetAvailableLevels();

        if (availableLevels == null || availableLevels.Count == 0)
        {
            Debug.LogWarning($"[LevelNode] {gameObject.name}: No levels available from controller!");
            return;
        }

        if (_levelIndex < 0 || _levelIndex >= availableLevels.Count)
        {
            Debug.LogWarning($"[LevelNode] {gameObject.name}: Level index {_levelIndex} out of range (0-{availableLevels.Count - 1})");
            return;
        }

        _level = availableLevels[_levelIndex];
        Initialize(_level, _controller);
    }

    /// <summary>
    /// Manually set the level for this node (for runtime assignment)
    /// </summary>
    public void SetLevel(Level level)
    {
        _level = level;
        if (_controller != null)
        {
            Initialize(_level, _controller);
        }
    }

    /// <summary>
    /// Get the currently assigned level
    /// </summary>
    public Level GetLevel()
    {
        return _level;
    }

    /// <summary>
    /// Set the controller reference
    /// </summary>
    public void SetController(LevelController controller)
    {
        _controller = controller;
    }

    /// <summary>
    /// Get the level index (used by editor)
    /// </summary>
    public int GetLevelIndex()
    {
        return _levelIndex;
    }

    /// <summary>
    /// Set the level index (used by editor)
    /// </summary>
    public void SetLevelIndex(int index)
    {
        _levelIndex = index;
    }

    /// <summary>
    /// Get the controller reference (used by editor)
    /// </summary>
    public LevelController GetController()
    {
        return _controller;
    }

    public void Initialize(Level level, LevelController controller)
    {
        _level = level;
        _controller = controller;

        if (_level == null)
        {
            Debug.LogError("[LevelNode] Cannot initialize with null level!");
            return;
        }

        // Set name (optional - can be hidden in compact view)
        if (_nameText != null)
        {
            _nameText.text = _level.LevelName;
        }

        // Set description (optional - can be hidden in compact view)
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

        // Update sprites based on unlock state
        if (_lockedSprite != null)
        {
            _lockedSprite.gameObject.SetActive(!_level.IsUnlocked);

            if (_level.LockedIcon != null)
                _lockedSprite.sprite = _level.LockedIcon;
            else if (_level.Icon != null)
                _lockedSprite.sprite = _level.Icon;
        }

        if (_unlockedSprite != null)
        {
            _unlockedSprite.gameObject.SetActive(_level.IsUnlocked);

            if (_level.UnlockedIcon != null)
            {
                _unlockedSprite.sprite = _level.UnlockedIcon;
                _originalSprite = _level.UnlockedIcon;
            }
            else if (_level.Icon != null)
            {
                _unlockedSprite.sprite = _level.Icon;
                _originalSprite = _level.Icon;
            }
        }

        // Update level display
        if (_levelText != null)
        {
            if (_level.IsUnlocked && _level.MaxLevel > 1)
            {
                _levelText.text = $"{_level.CurrentLevel}/{_level.MaxLevel}";
                _levelText.gameObject.SetActive(true);
            }
            else
            {
                _levelText.gameObject.SetActive(false);
            }
        }

        // Update locked overlay
        if (_lockedOverlay != null)
        {
            _lockedOverlay.SetActive(!_level.IsUnlocked);
        }

        // Update border color based on state
        Color targetColor = GetStateColor();
        _originalBorderColor = targetColor;

        if (!_isHovering && !_isSelected && _borderImage != null)
        {
            _borderImage.color = targetColor;
        }

        // Update background opacity
        if (_backgroundImage != null && !_isSelected)
        {
            Color bgColor = _originalBackgroundColor;
            bgColor.a = _level.IsUnlocked ? 1f : 0.5f;
            _backgroundImage.color = bgColor;
        }
    }

    private Color GetStateColor()
    {
        if (_level.IsUnlocked)
        {
            return _unlockedColor;
        }
        else if (_level.CanUnlock())
        {
            return _availableColor;
        }
        else
        {
            return _lockedColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovering = true;

        // Highlight border
        if (_borderImage != null && !_isSelected)
        {
            _borderImage.color = _hoverColor;
        }

        // Scale up
        if (!_isSelected)
        {
            transform.localScale = _originalScale * _hoverScale;
        }

        // Show description if available
        if (_descriptionText != null && _level != null && !string.IsNullOrEmpty(_level.Description))
        {
            _descriptionText.gameObject.SetActive(true);
        }

        // Show hover tooltip
        if (_showHoverTooltip && _level != null)
        {
            ShowHoverTooltip();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovering = false;

        // Restore border color
        if (_borderImage != null && !_isSelected)
        {
            _borderImage.color = _originalBorderColor;
        }

        // Restore scale
        if (!_isSelected)
        {
            transform.localScale = _originalScale;
        }

        // Hide description
        if (_descriptionText != null)
        {
            _descriptionText.gameObject.SetActive(false);
        }

        // Hide hover tooltip
        HideHoverTooltip();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        HandleClick();
    }
    
    /// <summary>
    /// Handle button click (called by Button component)
    /// </summary>
    private void OnButtonClick()
    {
        HandleClick();
    }
    
    /// <summary>
    /// Unified click handling logic
    /// </summary>
    private void HandleClick()
    {
        if (_controller != null && _level != null)
        {
            // Set selected state
            SetSelected(true);
            
            // Play click animation
            StartCoroutine(ClickFeedbackCoroutine());
            
            // Show level details in the main details panel
            _controller.ShowLevelDetails(_level);

            // Optional: Play click sound
            // AudioManager.Instance?.PlaySound("LevelNodeClick");
            
            Debug.Log($"[LevelNode] Clicked on level: {_level.LevelName}");
        }
        else
        {
            Debug.LogWarning($"[LevelNode] Cannot handle click - Controller: {_controller != null}, Level: {_level != null}");
        }
    }

    /// <summary>
    /// Set the selected state of this node
    /// </summary>
    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        
        if (_isSelected)
        {
            // Apply selected visual state
            if (_borderImage != null)
            {
                _borderImage.color = _selectedColor;
            }
            
            if (_backgroundImage != null)
            {
                Color selectedBg = _selectedColor;
                selectedBg.a = 0.3f;
                _backgroundImage.color = selectedBg;
            }
        }
        else
        {
            // Restore normal state
            if (_borderImage != null)
            {
                _borderImage.color = _isHovering ? _hoverColor : _originalBorderColor;
            }
            
            if (_backgroundImage != null)
            {
                _backgroundImage.color = _originalBackgroundColor;
            }
            
            // Restore original sprite
            if (_unlockedSprite != null && _originalSprite != null)
            {
                _unlockedSprite.sprite = _originalSprite;
            }
        }
    }

    /// <summary>
    /// Click feedback animation with color change and sprite swap
    /// </summary>
    private System.Collections.IEnumerator ClickFeedbackCoroutine()
    {
        float halfDuration = _clickDuration / 2f;
        float elapsed = 0f;
        
        // Store original values
        Color originalBgColor = _backgroundImage != null ? _backgroundImage.color : Color.white;
        Sprite displaySprite = _unlockedSprite != null ? _unlockedSprite.sprite : null;
        
        // Scale down and change visuals
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            
            // Scale animation
            transform.localScale = Vector3.Lerp(_originalScale * _hoverScale, _originalScale * _clickScale, t);
            
            // Background color animation
            if (_backgroundImage != null)
            {
                _backgroundImage.color = Color.Lerp(originalBgColor, _clickedBackgroundColor, t);
            }
            
            yield return null;
        }
        
        // Swap sprite at peak of animation
        if (_clickedSprite != null && _unlockedSprite != null && _level.IsUnlocked)
        {
            _unlockedSprite.sprite = _clickedSprite;
        }
        
        elapsed = 0f;
        
        // Scale back up and restore visuals
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            
            // Scale animation
            Vector3 targetScale = _isSelected ? _originalScale : (_originalScale * _hoverScale);
            transform.localScale = Vector3.Lerp(_originalScale * _clickScale, targetScale, t);
            
            // Background color animation
            if (_backgroundImage != null)
            {
                Color targetColor = _isSelected ? Color.Lerp(_selectedColor, Color.white, 0.7f) : originalBgColor;
                _backgroundImage.color = Color.Lerp(_clickedBackgroundColor, targetColor, t);
            }
            
            yield return null;
        }
        
        // Restore sprite after animation (unless selected)
        if (!_isSelected && _unlockedSprite != null && displaySprite != null)
        {
            _unlockedSprite.sprite = displaySprite;
        }
        
        // Ensure final state is correct
        if (!_isHovering && !_isSelected)
        {
            transform.localScale = _originalScale;
        }
    }

    /// <summary>
    /// Show hover tooltip at mouse position
    /// </summary>
    private void ShowHoverTooltip()
    {
        if (_hoverTooltipPrefab != null && _hoverTooltipInstance == null)
        {
            // Instantiate tooltip
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                _hoverTooltipInstance = Instantiate(_hoverTooltipPrefab, canvas.transform);
                
                // Position at mouse with offset
                RectTransform tooltipRect = _hoverTooltipInstance.GetComponent<RectTransform>();
                if (tooltipRect != null)
                {
                    Vector2 mousePos = Input.mousePosition;
                    tooltipRect.position = mousePos + _tooltipOffset;
                }
                
      
            }
        }
    }

    /// <summary>
    /// Hide and destroy hover tooltip
    /// </summary>
    private void HideHoverTooltip()
    {
        if (_hoverTooltipInstance != null)
        {
            Destroy(_hoverTooltipInstance);
            _hoverTooltipInstance = null;
        }
    }

    /// <summary>
    /// Pulse animation for when a level is unlocked
    /// </summary>
    public void PlayUnlockAnimation()
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine(UnlockPulseCoroutine());
    }

    private System.Collections.IEnumerator UnlockPulseCoroutine()
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 targetScale = _originalScale * 1.3f;

        // Scale up
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(_originalScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;

        // Scale back down
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(targetScale, _originalScale, t);
            yield return null;
        }

        transform.localScale = _originalScale;
    }

    private System.Collections.IEnumerator CompletionPulseCoroutine()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 originalScale = _originalScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.3f;
            transform.localScale = originalScale * scale;
            yield return null;
        }

        transform.localScale = originalScale;
    }

    /// <summary>
    /// Pulse animation for when a level is completed
    /// </summary>
    public void PlayCompletionAnimation()
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine(CompletionPulseCoroutine());
    }

    /// <summary>
    /// Shake animation for when unlock fails
    /// </summary>
    public void PlayFailAnimation()
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine(ShakeCoroutine());
    }

    private System.Collections.IEnumerator ShakeCoroutine()
    {
        Vector3 originalPos = transform.localPosition;
        float duration = 0.3f;
        float elapsed = 0f;
        float magnitude = 10f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = originalPos.x + Random.Range(-magnitude, magnitude);
            float y = originalPos.y + Random.Range(-magnitude, magnitude);
            transform.localPosition = new Vector3(x, y, originalPos.z);
            yield return null;
        }

        transform.localPosition = originalPos;
    }
    
    private void OnDestroy()
    {
        // Clean up hover tooltip if it exists
        HideHoverTooltip();
    }
}