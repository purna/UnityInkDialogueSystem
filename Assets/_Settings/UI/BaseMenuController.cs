using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Base class for menu controllers with common functionality and animations
/// </summary>
public abstract class BaseMenuController : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField] protected UIDocument _uiDocument;

    [Header("Common Images")]
    [SerializeField] protected Sprite _headerSeparatorImage;
    [SerializeField] protected Sprite _footerSeparatorImage;
    [SerializeField] protected Sprite _footerLeftDecoration;
    [SerializeField] protected Sprite _footerRightDecoration;

    [Header("Common Colors")]
    [SerializeField] protected Color _textColor = Color.white;
    [SerializeField] protected Color _footerBtnNormal = new Color(0.145f, 0.388f, 0.922f);
    [SerializeField] protected Color _footerBtnHover = new Color(0.231f, 0.510f, 0.965f);

    [Header("Footer Button Visibility")]
    [SerializeField] protected bool _showBackButton = true;
    [SerializeField] protected bool _showResetButton = true;
    [SerializeField] protected bool _showCloseButton = true;

    [Header("Animation Settings")]
    [SerializeField] protected bool _enableAnimations = true;
    [SerializeField] protected float _fadeInDuration = 0.3f;
    [SerializeField] protected float _fadeOutDuration = 0.2f;
    [SerializeField] protected AnimationCurve _fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] protected AnimationCurve _fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] protected bool _scaleAnimation = true;
    [SerializeField] protected float _scaleFrom = 0.95f;
    [SerializeField] protected float _scaleTo = 1.0f;

    [Header("Audio")]
    [SerializeField] protected AudioClip _buttonClickSound;
    [SerializeField] protected AudioSource _uiAudioSource;

    // Common Visual Elements
    protected VisualElement _root;
    protected Label _headerText;
    protected VisualElement _headerSeparator;
    protected VisualElement _contentContainer;
    protected VisualElement _footerSeparator;
    protected VisualElement _footerLeftDeco;
    protected VisualElement _footerRightDeco;
    
    protected Button _resetButton;
    protected Button _closeButton;
    protected Button _backButton;

    private Coroutine _currentAnimation;

    protected virtual void OnEnable()
    {
        if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();
        _root = _uiDocument.rootVisualElement;

        if (_uiAudioSource == null) _uiAudioSource = GetComponent<AudioSource>();

        BindCommonElements();
        SetupCommonCallbacks();
        ApplyCommonVisuals();
        
        // Call derived class specific setup
        OnEnableCustom();

        // Play opening animation
        if (_enableAnimations)
        {
            PlayOpenAnimation();
        }
        else
        {
            if (_root != null) _root.style.display = DisplayStyle.Flex;
        }
    }

    protected virtual void BindCommonElements()
    {
        _headerText = _root.Q<Label>("HeaderText");
        _headerSeparator = _root.Q("HeaderSeparator");
        _contentContainer = _root.Q("ContentContainer");
        
        _footerSeparator = _root.Q("FooterSeparator");
        _footerLeftDeco = _root.Q("FooterLeftDecoration");
        _footerRightDeco = _root.Q("FooterRightDecoration");
        
        _resetButton = _root.Q<Button>("ResetButton");
        _closeButton = _root.Q<Button>("CloseButton");
        _backButton = _root.Q<Button>("BackButton");
    }

    protected virtual void SetupCommonCallbacks()
    {
        if (_resetButton != null) 
        {
            _resetButton.clicked += OnResetClicked;
            RegisterButtonHover(_resetButton, _footerBtnNormal, _footerBtnHover);
            _resetButton.style.display = _showResetButton ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        if (_closeButton != null) 
        {
            _closeButton.clicked += OnCloseClicked;
            RegisterButtonHover(_closeButton, _footerBtnNormal, _footerBtnHover);
            _closeButton.style.display = _showCloseButton ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        if (_backButton != null) 
        {
            _backButton.clicked += OnBackClicked;
            RegisterButtonHover(_backButton, _footerBtnNormal, _footerBtnHover);
            _backButton.style.display = _showBackButton ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    protected virtual void ApplyCommonVisuals()
    {
        SetBg(_headerSeparator, _headerSeparatorImage);
        SetBg(_footerSeparator, _footerSeparatorImage);
        SetBg(_footerLeftDeco, _footerLeftDecoration);
        SetBg(_footerRightDeco, _footerRightDecoration);

        if (_headerText != null) _headerText.style.color = _textColor;
        
        if (_resetButton != null) 
        {
            _resetButton.style.backgroundColor = _footerBtnNormal;
            _resetButton.style.display = _showResetButton ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        if (_closeButton != null) 
        {
            _closeButton.style.backgroundColor = _footerBtnNormal;
            _closeButton.style.display = _showCloseButton ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        if (_backButton != null) 
        {
            _backButton.style.backgroundColor = _footerBtnNormal;
            _backButton.style.display = _showBackButton ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    // Animation Methods
    protected virtual void PlayOpenAnimation()
    {
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
        }
        _currentAnimation = StartCoroutine(AnimateOpen());
    }

    protected virtual void PlayCloseAnimation(System.Action onComplete = null)
    {
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
        }
        _currentAnimation = StartCoroutine(AnimateClose(onComplete));
    }

    private IEnumerator AnimateOpen()
    {
        if (_root == null) yield break;

        // Make visible but transparent
        _root.style.display = DisplayStyle.Flex;
        _root.style.opacity = 0;
        
        if (_scaleAnimation)
        {
            _root.style.scale = new Scale(new Vector3(_scaleFrom, _scaleFrom, 1));
        }

        float elapsedTime = 0f;

        while (elapsedTime < _fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / _fadeInDuration);
            float curveValue = _fadeInCurve.Evaluate(t);

            // Animate opacity
            _root.style.opacity = curveValue;

            // Animate scale
            if (_scaleAnimation)
            {
                float scale = Mathf.Lerp(_scaleFrom, _scaleTo, curveValue);
                _root.style.scale = new Scale(new Vector3(scale, scale, 1));
            }

            yield return null;
        }

        // Ensure final state
        _root.style.opacity = 1;
        if (_scaleAnimation)
        {
            _root.style.scale = new Scale(new Vector3(_scaleTo, _scaleTo, 1));
        }

        _currentAnimation = null;
    }

    private IEnumerator AnimateClose(System.Action onComplete)
    {
        if (_root == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        float elapsedTime = 0f;
        float startOpacity = _root.resolvedStyle.opacity;

        while (elapsedTime < _fadeOutDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / _fadeOutDuration);
            float curveValue = _fadeOutCurve.Evaluate(t);

            // Animate opacity (fade out)
            _root.style.opacity = startOpacity * (1f - curveValue);

            // Animate scale (optional)
            if (_scaleAnimation)
            {
                float scale = Mathf.Lerp(_scaleTo, _scaleFrom, curveValue);
                _root.style.scale = new Scale(new Vector3(scale, scale, 1));
            }

            yield return null;
        }

        // Final state
        _root.style.opacity = 0;
        _root.style.display = DisplayStyle.None;

        _currentAnimation = null;
        onComplete?.Invoke();
    }

    // Abstract/Virtual methods for derived classes
    protected abstract void OnEnableCustom();
    protected abstract void OnResetClicked();

    protected virtual void OnCloseClicked()
    {
        StartCoroutine(CloseSequence());
    }

    protected virtual void OnBackClicked()
    {
        StartCoroutine(BackSequence());
    }

    protected virtual IEnumerator CloseSequence()
    {
        PlayClickSound();

        if (_enableAnimations)
        {
            // Wait for animation to complete
            bool animationComplete = false;
            PlayCloseAnimation(() => animationComplete = true);

            yield return new WaitUntil(() => animationComplete);
        }
        else
        {
            if (_root != null) _root.style.display = DisplayStyle.None;
        }

        float delay = (_buttonClickSound != null) ? _buttonClickSound.length : 0.1f;
        yield return new WaitForSecondsRealtime(delay);
        
        gameObject.SetActive(false);
    }

    protected virtual IEnumerator BackSequence()
    {
        PlayClickSound();

        if (_enableAnimations)
        {
            // Wait for animation to complete
            bool animationComplete = false;
            PlayCloseAnimation(() => animationComplete = true);

            yield return new WaitUntil(() => animationComplete);
        }
        else
        {
            if (_root != null) _root.style.display = DisplayStyle.None;
        }

        float delay = (_buttonClickSound != null) ? _buttonClickSound.length : 0.1f;
        yield return new WaitForSecondsRealtime(delay);
        
        if (UIDocumentManager.Instance != null)
        {
            UIDocumentManager.Instance.ShowPreviousDocument();
        }
        
        gameObject.SetActive(false);
    }

    // Helper Methods
    protected void PlayClickSound()
    {
        if (_uiAudioSource != null && _buttonClickSound != null)
        {
            _uiAudioSource.PlayOneShot(_buttonClickSound);
        }
    }

    protected void SetBg(VisualElement element, Sprite sprite)
    {
        if (element == null) return;
        if (sprite != null)
        {
            element.style.backgroundImage = new StyleBackground(sprite);
            element.style.display = DisplayStyle.Flex;
        }
        else
        {
            element.style.display = DisplayStyle.None;
        }
    }

    protected void RegisterButtonHover(Button btn, Color normal, Color hover)
    {
        if (btn == null) return;
        btn.RegisterCallback<MouseEnterEvent>(evt => btn.style.backgroundColor = hover);
        btn.RegisterCallback<MouseLeaveEvent>(evt => btn.style.backgroundColor = normal);
    }

    // Public methods for external control
    public void CloseWithAnimation()
    {
        StartCoroutine(CloseSequence());
    }

    public void OpenWithAnimation()
    {
        gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
            _currentAnimation = null;
        }
    }
}