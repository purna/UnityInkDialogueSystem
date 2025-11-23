using System.Collections; // Required for Coroutines
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsMenuController : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField] private UIDocument _uiDocument;

    [Header("Images")]
    [SerializeField] private Sprite _headerSeparatorImage;
    [SerializeField] private Sprite _footerSeparatorImage;
    [SerializeField] private Sprite _footerLeftDecoration;
    [SerializeField] private Sprite _footerRightDecoration;
    [SerializeField] private Sprite _sliderHandleSprite; 

    [Header("Colors")]
    [SerializeField] private Color _textColor = Color.white;
    [SerializeField] private Color _sliderTrackerColor = new Color(0.2f, 0.2f, 0.2f);
    [SerializeField] private Color _sliderHandleColor = Color.white;
    
    [Space(10)]
    [SerializeField] private Color _closeBtnNormal = new Color(0.145f, 0.388f, 0.922f);
    [SerializeField] private Color _closeBtnHover = new Color(0.231f, 0.510f, 0.965f);
    [SerializeField] private Color _resetBtnNormal = new Color(0.3f, 0.3f, 0.3f);
    [SerializeField] private Color _resetBtnHover = new Color(0.4f, 0.4f, 0.4f);

    [Header("Audio")]
    [SerializeField] private AudioClip _buttonClickSound;
    [SerializeField] private AudioSource _uiAudioSource; 

    // Visual Elements
    private VisualElement _root;
    private Label _headerText;
    private VisualElement _headerSeparator;
    private VisualElement _footerSeparator;
    private VisualElement _footerLeftDeco;
    private VisualElement _footerRightDeco;
    
    private Slider _masterSlider;
    private Slider _musicSlider;
    private Slider _sfxSlider;
    
    private Button _resetButton;
    private Button _closeButton;

    // Default Values
    private const float DEFAULT_MASTER = 1.0f;
    private const float DEFAULT_MUSIC = 0.8f;
    private const float DEFAULT_SFX = 1.0f;

    private void OnEnable()
    {
        if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();
        _root = _uiDocument.rootVisualElement;

        // FIX: Ensure UI is visible again when re-enabled (since we hide it in CloseRoutine)
        if (_root != null) _root.style.display = DisplayStyle.Flex;

        if (_uiAudioSource == null) _uiAudioSource = GetComponent<AudioSource>();

        BindElements();
        SetupCallbacks();
        ApplyVisuals();
    }

    private void BindElements()
    {
        _headerText = _root.Q<Label>("HeaderText");
        _headerSeparator = _root.Q("HeaderSeparator");
        
        _masterSlider = _root.Q<Slider>("MasterSlider");
        _musicSlider = _root.Q<Slider>("MusicSlider");
        _sfxSlider = _root.Q<Slider>("SFXSlider");

        _footerSeparator = _root.Q("FooterSeparator");
        _footerLeftDeco = _root.Q("FooterLeftDecoration");
        _footerRightDeco = _root.Q("FooterRightDecoration");
        
        _resetButton = _root.Q<Button>("ResetButton");
        _closeButton = _root.Q<Button>("CloseButton");
    }

    private void SetupCallbacks()
    {
        // Slider Events
        _masterSlider?.RegisterValueChangedCallback(evt => OnMasterVolumeChanged(evt.newValue));
        _musicSlider?.RegisterValueChangedCallback(evt => OnMusicVolumeChanged(evt.newValue));
        _sfxSlider?.RegisterValueChangedCallback(evt => OnSFXVolumeChanged(evt.newValue));

        // Button Events
        if (_resetButton != null) _resetButton.clicked += OnResetClicked;
        if (_closeButton != null) _closeButton.clicked += OnCloseClicked;

        // Button Styling Events (Hover)
        RegisterButtonHover(_resetButton, _resetBtnNormal, _resetBtnHover);
        RegisterButtonHover(_closeButton, _closeBtnNormal, _closeBtnHover);
    }

    private void ApplyVisuals()
    {
        SetBg(_headerSeparator, _headerSeparatorImage);
        SetBg(_footerSeparator, _footerSeparatorImage);
        SetBg(_footerLeftDeco, _footerLeftDecoration);
        SetBg(_footerRightDeco, _footerRightDecoration);

        if (_headerText != null) _headerText.style.color = _textColor;
        
        var labels = _root.Query<Label>(className: "slider-label").ToList();
        foreach(var label in labels) label.style.color = _textColor;

        if (_resetButton != null) _resetButton.style.backgroundColor = _resetBtnNormal;
        if (_closeButton != null) _closeButton.style.backgroundColor = _closeBtnNormal;

        ApplySliderColors(_masterSlider);
        ApplySliderColors(_musicSlider);
        ApplySliderColors(_sfxSlider);
    }

    private void ApplySliderColors(Slider slider)
    {
        if (slider == null) return;
        
        var tracker = slider.Q("unity-tracker");
        if (tracker != null) tracker.style.backgroundColor = _sliderTrackerColor;

        var dragger = slider.Q("unity-dragger");
        if (dragger != null) 
        {
            dragger.style.backgroundColor = _sliderHandleColor;
            if (_sliderHandleSprite != null)
            {
                dragger.style.backgroundImage = new StyleBackground(_sliderHandleSprite);
                dragger.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            }
        }
    }

    // --- Event Logic ---

    private void OnMasterVolumeChanged(float value)
    {
        // AudioManager.Instance.SetMasterVolume(value);
        Debug.Log($"Master Volume: {value:F2}");
    }

    private void OnMusicVolumeChanged(float value)
    {
        // AudioManager.Instance.SetMusicVolume(value);
        Debug.Log($"Music Volume: {value:F2}");
    }

    private void OnSFXVolumeChanged(float value)
    {
        // AudioManager.Instance.SetSFXVolume(value);
        Debug.Log($"SFX Volume: {value:F2}");
    }

    private void OnResetClicked()
    {
        PlayClickSound();
        _masterSlider.value = DEFAULT_MASTER;
        _musicSlider.value = DEFAULT_MUSIC;
        _sfxSlider.value = DEFAULT_SFX;
        Debug.Log("Settings Reset");
    }

    private void OnCloseClicked()
    {
        StartCoroutine(CloseSequence());
    }

    // FIX: Coroutine to handle sound playback before disabling object
    private IEnumerator CloseSequence()
    {
        PlayClickSound();

        // 1. Hide the visuals immediately so UI feels responsive
        if (_root != null) _root.style.display = DisplayStyle.None;

        // 2. Wait for the sound to finish (use Realtime to ignore pausing)
        float delay = (_buttonClickSound != null) ? _buttonClickSound.length : 0.1f;
        yield return new WaitForSecondsRealtime(delay);

        // 3. Actually disable the GameObject
        Debug.Log("Close Settings");
        gameObject.SetActive(false);
    }

    private void PlayClickSound()
    {
        if (_uiAudioSource != null && _buttonClickSound != null)
        {
            _uiAudioSource.PlayOneShot(_buttonClickSound);
        }
    }

    // --- Helpers ---

    private void SetBg(VisualElement element, Sprite sprite)
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

    private void RegisterButtonHover(Button btn, Color normal, Color hover)
    {
        if (btn == null) return;
        btn.RegisterCallback<MouseEnterEvent>(evt => btn.style.backgroundColor = hover);
        btn.RegisterCallback<MouseLeaveEvent>(evt => btn.style.backgroundColor = normal);
    }

    private void OnValidate()
    {
        if (Application.isPlaying && _root != null)
        {
            ApplyVisuals();
        }
    }
}