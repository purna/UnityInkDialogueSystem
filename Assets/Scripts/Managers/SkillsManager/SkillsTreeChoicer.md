using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Individual skill node UI component - handles display and interaction
/// Now supports manual skill selection from available skills in the controller
/// </summary>
public class SkillsTreeChoicer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
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
    [SerializeField] private float _hoverScale = 1.1f;
    
    [Header("Manual Skill Assignment")]
    [SerializeField] private SkillsTreeController _controller;
    [Tooltip("Select which skill this node represents")]
    [SerializeField] private int _skillIndex = -1;
    
    [SerializeField] private Skill _skill;
    private Color _originalBorderColor;
    private Vector3 _originalScale;
    private bool _isHovering;
    
    private void Awake()
    {
        _originalScale = transform.localScale;
        
        // Store original border color if border exists
        if (_borderImage != null)
        {
            _originalBorderColor = _borderImage.color;
        }
    }
    
    private void Start()
    {
        // Auto-initialize if controller is assigned and skill index is set
        if (_controller != null && _skillIndex >= 0 && _skill == null)
        {
            AutoAssignSkillFromController();
        }
    }
    
    /// <summary>
    /// Automatically assign skill based on the selected index from controller
    /// </summary>
    private void AutoAssignSkillFromController()
    {
        if (_controller == null)
        {
            Debug.LogWarning($"[SkillsTreeChoicer] {gameObject.name}: No controller assigned!");
            return;
        }
        
        List<Skill> availableSkills = _controller.GetAvailableSkills();
        
        if (availableSkills == null || availableSkills.Count == 0)
        {
            Debug.LogWarning($"[SkillsTreeChoicer] {gameObject.name}: No skills available from controller!");
            return;
        }
        
        if (_skillIndex < 0 || _skillIndex >= availableSkills.Count)
        {
            Debug.LogWarning($"[SkillsTreeChoicer] {gameObject.name}: Skill index {_skillIndex} out of range (0-{availableSkills.Count - 1})");
            return;
        }
        
        _skill = availableSkills[_skillIndex];
        Initialize(_skill, _controller);
    }
    
    /// <summary>
    /// Manually set the skill for this node (for runtime assignment)
    /// </summary>
    public void SetSkill(Skill skill)
    {
        _skill = skill;
        if (_controller != null)
        {
            Initialize(_skill, _controller);
        }
    }
    
    /// <summary>
    /// Get the currently assigned skill
    /// </summary>
    public Skill GetSkill()
    {
        return _skill;
    }
    
    /// <summary>
    /// Set the controller reference
    /// </summary>
    public void SetController(SkillsTreeController controller)
    {
        _controller = controller;
    }
    
    /// <summary>
    /// Get the skill index (used by editor)
    /// </summary>
    public int GetSkillIndex()
    {
        return _skillIndex;
    }
    
    /// <summary>
    /// Set the skill index (used by editor)
    /// </summary>
    public void SetSkillIndex(int index)
    {
        _skillIndex = index;
    }
    
    /// <summary>
    /// Get the controller reference (used by editor)
    /// </summary>
    public SkillsTreeController GetController()
    {
        return _controller;
    }
    
    public void Initialize(Skill skill, SkillsTreeController controller)
    {
        _skill = skill;
        _controller = controller;
        
        if (_skill == null)
        {
            Debug.LogError("[SkillsTreeChoicer] Cannot initialize with null skill!");
            return;
        }

        // Set name (optional - can be hidden in compact view)
        if (_nameText != null)
        {
            _nameText.text = _skill.SkillName;
        }
        
        // Set description (optional - can be hidden in compact view)
        if (_descriptionText != null)
        {
            _descriptionText.text = _skill.Description;
            // Hide by default, show only on hover or in details panel
            _descriptionText.gameObject.SetActive(false);
        }
        
        UpdateDisplay();
    }
    
    public void UpdateDisplay()
    {
        if (_skill == null)
            return;
        
        // Update sprites based on unlock state
        if (_lockedSprite != null)
        {
            _lockedSprite.gameObject.SetActive(!_skill.IsUnlocked);
            
            // Use the locked icon if available
            if (_skill.LockedIcon != null)
                _lockedSprite.sprite = _skill.LockedIcon;
            else if (_skill.Icon != null)
                _lockedSprite.sprite = _skill.Icon;
        }
        
        if (_unlockedSprite != null)
        {
            _unlockedSprite.gameObject.SetActive(_skill.IsUnlocked);
            
            // Use the unlocked icon if available
            if (_skill.UnlockedIcon != null)
                _unlockedSprite.sprite = _skill.UnlockedIcon;
            else if (_skill.Icon != null)
                _unlockedSprite.sprite = _skill.Icon;
        }
        
        // Update level display
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
        {
            _lockedOverlay.SetActive(!_skill.IsUnlocked);
        }
        
        // Update border color based on state
        Color targetColor = GetStateColor();
        _originalBorderColor = targetColor;
        
        if (!_isHovering && _borderImage != null)
        {
            _borderImage.color = targetColor;
        }
        
        // Update background opacity
        if (_backgroundImage != null)
        {
            Color bgColor = _backgroundImage.color;
            bgColor.a = _skill.IsUnlocked ? 1f : 0.5f;
            _backgroundImage.color = bgColor;
        }
    }
    
    private Color GetStateColor()
    {
        if (_skill.IsUnlocked)
        {
            return _unlockedColor;
        }
        else if (_skill.CanUnlock())
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
        if (_borderImage != null)
        {
            _borderImage.color = _hoverColor;
        }
        
        // Scale up
        transform.localScale = _originalScale * _hoverScale;
        
        // Show description if available
        if (_descriptionText != null && _skill != null && !string.IsNullOrEmpty(_skill.Description))
        {
            _descriptionText.gameObject.SetActive(true);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovering = false;
        
        // Restore border color
        if (_borderImage != null)
        {
            _borderImage.color = _originalBorderColor;
        }
        
        // Restore scale
        transform.localScale = _originalScale;
        
        // Hide description
        if (_descriptionText != null)
        {
            _descriptionText.gameObject.SetActive(false);
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_controller != null && _skill != null)
        {
            _controller.ShowSkillDetails(_skill);
            
            // Optional: Play click sound
            // AudioManager.Instance?.PlaySound("SkillNodeClick");
        }
    }
    
    /// <summary>
    /// Pulse animation for when a skill is unlocked
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
}