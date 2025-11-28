using UnityEngine;
using UnityEngine.UIElements;

public class AudioSettingsMenuController : BaseMenuController
{
    [Header("Slider Settings")]
    [SerializeField] private Sprite _sliderHandleSprite;
    [SerializeField] private Color _sliderTrackerColor = new Color(0.2f, 0.2f, 0.2f);
    [SerializeField] private Color _sliderHandleColor = Color.white;

    private Slider _masterSlider;
    private Slider _musicSlider;
    private Slider _sfxSlider;

    private const float DEFAULT_MASTER = 1.0f;
    private const float DEFAULT_MUSIC = 0.8f;
    private const float DEFAULT_SFX = 1.0f;

    protected override void OnEnableCustom()
    {
        BindSliders();
        SetupSliderCallbacks();
        ApplySliderVisuals();
    }

    private void BindSliders()
    {
        _masterSlider = _root.Q<Slider>("MasterSlider");
        _musicSlider = _root.Q<Slider>("MusicSlider");
        _sfxSlider = _root.Q<Slider>("SFXSlider");
    }

    private void SetupSliderCallbacks()
    {
        _masterSlider?.RegisterValueChangedCallback(evt => OnMasterVolumeChanged(evt.newValue));
        _musicSlider?.RegisterValueChangedCallback(evt => OnMusicVolumeChanged(evt.newValue));
        _sfxSlider?.RegisterValueChangedCallback(evt => OnSFXVolumeChanged(evt.newValue));
    }

    private void ApplySliderVisuals()
    {
        var labels = _root.Query<Label>(className: "slider-label").ToList();
        foreach(var label in labels) label.style.color = _textColor;

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

    protected override void OnResetClicked()
    {
        PlayClickSound();
        if (_masterSlider != null) _masterSlider.value = DEFAULT_MASTER;
        if (_musicSlider != null) _musicSlider.value = DEFAULT_MUSIC;
        if (_sfxSlider != null) _sfxSlider.value = DEFAULT_SFX;
        Debug.Log("Audio Settings Reset");
    }

    private void OnValidate()
    {
        if (Application.isPlaying && _root != null)
        {
            ApplySliderVisuals();
        }
    }
}