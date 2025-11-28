using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ToggleSettingsMenuController : BaseMenuController
{
    [Header("Toggle Settings")]
    [SerializeField] private List<ToggleSettingData> _toggleSettings = new List<ToggleSettingData>();

    [Header("Toggle Decoration Images")]
    [SerializeField] private Sprite _toggleLeftDecoration;
    [SerializeField] private Sprite _toggleRightDecoration;

    [Header("Toggle Colors")]
    [SerializeField] private Color _toggleOnColor = new Color(0.2f, 0.8f, 0.2f);  // Green
    [SerializeField] private Color _toggleOffColor = new Color(0.8f, 0.2f, 0.2f);  // Red
    [SerializeField] private Color _toggleHoverOnColor = new Color(0.3f, 0.9f, 0.3f);  // Lighter Green
    [SerializeField] private Color _toggleHoverOffColor = new Color(0.9f, 0.3f, 0.3f);  // Lighter Red
    [SerializeField] private Color _labelColor = new Color(0.78f, 0.78f, 0.78f);  // Light gray
    [SerializeField] private Color _toggleTextColor = Color.white;

    private List<ToggleSettingElements> _toggleElements = new List<ToggleSettingElements>();

    // Events for each setting type
    public event Action<bool> OnFullScreenChanged;
    public event Action<bool> OnParticleEffectsChanged;
    public event Action<bool> OnRumbleChanged;
    public event Action<bool> OnNativeControllerChanged;
    public event Action<bool> OnShowAchievementsChanged;
    public event Action<string> OnLanguageChanged;

    protected override void OnEnableCustom()
    {
        CreateToggleSettings();
        SetupToggleCallbacks();
        ApplyToggleVisuals();
        LoadSettings();
    }

    private void CreateToggleSettings()
    {
        if (_contentContainer == null) return;

        _contentContainer.Clear();
        _toggleElements.Clear();

        foreach (var toggleData in _toggleSettings)
        {
            // Create row container
            var row = new VisualElement();
            row.AddToClassList("toggle-row");

            // Create toggle container (holds decorations + toggle)
            var toggleContainer = new VisualElement();
            toggleContainer.AddToClassList("toggle-container");

            // Create left decoration
            var leftDeco = new VisualElement();
            leftDeco.name = "ToggleLeftDecoration";
            leftDeco.AddToClassList("toggle-decoration");
            leftDeco.style.opacity = 0;

            // Create the toggle button wrapper (now matches content-button width)
            var toggleButton = new Button();
            toggleButton.AddToClassList("toggle-switch");
            
            // Create the slider background
            var sliderBg = new VisualElement();
            sliderBg.name = "SliderBackground";
            sliderBg.AddToClassList("slider-background");
            
            // Create the slider knob (the moving circle)
            var sliderKnob = new VisualElement();
            sliderKnob.name = "SliderKnob";
            sliderKnob.AddToClassList("slider-knob");
            
            // Create label inside toggle (stays centered)
            var label = new Label(toggleData.settingLabel);
            label.AddToClassList("toggle-label-inside");
            label.style.color = _toggleTextColor;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.fontSize = 18;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.position = Position.Absolute;
            label.style.width = Length.Percent(100);
            label.style.height = Length.Percent(100);
            label.pickingMode = PickingMode.Ignore; // Allow clicks to pass through to button
            
            sliderBg.Add(sliderKnob);
            toggleButton.Add(sliderBg);
            toggleButton.Add(label); // Add label on top

            // Create right decoration
            var rightDeco = new VisualElement();
            rightDeco.name = "ToggleRightDecoration";
            rightDeco.AddToClassList("toggle-decoration");
            rightDeco.style.opacity = 0;

            // Assemble structure: [Decoration] [Toggle] [Decoration]
            toggleContainer.Add(leftDeco);
            toggleContainer.Add(toggleButton);
            toggleContainer.Add(rightDeco);
            
            row.Add(toggleContainer);
            _contentContainer.Add(row);

            // Store references
            _toggleElements.Add(new ToggleSettingElements
            {
                toggleButton = toggleButton,
                sliderBackground = sliderBg,
                sliderKnob = sliderKnob,
                label = label,
                toggleContainer = toggleContainer,
                leftDecoration = leftDeco,
                rightDecoration = rightDeco,
                data = toggleData,
                isOn = toggleData.defaultValue
            });
        }
    }

    private void SetupToggleCallbacks()
    {
        foreach (var toggleElement in _toggleElements)
        {
            if (toggleElement.toggleButton != null)
            {
                toggleElement.toggleButton.clicked += () => OnToggleClicked(toggleElement);
                
                // Register hover with color changes and sounds
                RegisterToggleHover(toggleElement);
                
                // Register hover/focus for decorations
                RegisterDecorationHover(toggleElement);
                RegisterDecorationFocus(toggleElement);
            }
        }
    }

    private void RegisterToggleHover(ToggleSettingElements toggleElement)
    {
        if (toggleElement.toggleButton == null) return;

        Color normalColor = toggleElement.isOn ? _toggleOnColor : _toggleOffColor;
        Color hoverColor = toggleElement.isOn ? _toggleHoverOnColor : _toggleHoverOffColor;

        // Mouse enter - play hover sound and change color
        toggleElement.toggleButton.RegisterCallback<MouseEnterEvent>(evt => 
        {
            PlayHoverSound();
            Color currentHoverColor = toggleElement.isOn ? _toggleHoverOnColor : _toggleHoverOffColor;
            if (toggleElement.sliderBackground != null)
            {
                toggleElement.sliderBackground.style.backgroundColor = currentHoverColor;
            }
        });

        // Mouse leave - revert to normal color
        toggleElement.toggleButton.RegisterCallback<MouseLeaveEvent>(evt => 
        {
            Color currentNormalColor = toggleElement.isOn ? _toggleOnColor : _toggleOffColor;
            if (toggleElement.sliderBackground != null)
            {
                toggleElement.sliderBackground.style.backgroundColor = currentNormalColor;
            }
        });
    }

    private void ApplyToggleVisuals()
    {
        foreach (var toggleElement in _toggleElements)
        {
            // Set decoration images
            SetBg(toggleElement.leftDecoration, _toggleLeftDecoration);
            SetBg(toggleElement.rightDecoration, _toggleRightDecoration);

            // Set initial slider background to OFF color
            if (toggleElement.sliderBackground != null)
            {
                toggleElement.sliderBackground.style.backgroundColor = _toggleOffColor;
            }

            // Apply initial toggle state
            UpdateToggleVisual(toggleElement);
        }
    }

    private void OnToggleClicked(ToggleSettingElements toggleElement)
    {
        PlayClickSound();
        
        // Toggle the state
        toggleElement.isOn = !toggleElement.isOn;
        
        // Update visual
        UpdateToggleVisual(toggleElement);
        
        // Invoke appropriate callback
        InvokeSettingCallback(toggleElement);
        
        // Save setting
        SaveSetting(toggleElement);
        
        Debug.Log($"Toggle '{toggleElement.data.settingLabel}' changed to: {toggleElement.isOn}");
    }

    private void UpdateToggleVisual(ToggleSettingElements toggleElement)
    {
        if (toggleElement.sliderBackground == null || toggleElement.sliderKnob == null) return;

        // Update background color
        Color bgColor = toggleElement.isOn ? _toggleOnColor : _toggleOffColor;
        toggleElement.sliderBackground.style.backgroundColor = bgColor;
        
        // Get the width of the toggle switch and knob to calculate proper translation
        // The knob should move to the far right (toggle width - knob width) when ON
        // You may need to adjust these values based on your actual toggle-switch and slider-knob sizes
        // For example, if toggle is 300px wide and knob is 40px wide, translateX should be 260px when ON
        float knobSize = 38f; // Adjust based on your slider-knob width in USS
        float toggleWidth = 300f; // Adjust based on your toggle-switch width in USS
        float maxTranslateX = toggleWidth - knobSize - 0f; // -10 for some padding from the edge
        
        // Animate knob position (translate to far right when ON, far left when OFF)
        float translateX = toggleElement.isOn ? maxTranslateX : 0f;
        toggleElement.sliderKnob.style.translate = new Translate(translateX, 2f);
        
        // Update label text to show ON/OFF state if enabled (label stays centered)
        if (toggleElement.label != null)
        {
            if (toggleElement.data.showStateInLabel)
            {
                string stateText = toggleElement.isOn ? "ON" : "OFF";
                toggleElement.label.text = $"{toggleElement.data.settingLabel}: {stateText}";
            }
            else
            {
                toggleElement.label.text = toggleElement.data.settingLabel;
            }
        }
    }

    private void InvokeSettingCallback(ToggleSettingElements toggleElement)
    {
        switch (toggleElement.data.settingType)
        {
            case SettingType.FullScreen:
                OnFullScreenChanged?.Invoke(toggleElement.isOn);
                Screen.fullScreen = toggleElement.isOn;
                break;
                
            case SettingType.ParticleEffects:
                OnParticleEffectsChanged?.Invoke(toggleElement.isOn);
                break;
                
            case SettingType.Rumble:
                OnRumbleChanged?.Invoke(toggleElement.isOn);
                break;
                
            case SettingType.NativeControllerInput:
                OnNativeControllerChanged?.Invoke(toggleElement.isOn);
                break;
                
            case SettingType.ShowAchievements:
                OnShowAchievementsChanged?.Invoke(toggleElement.isOn);
                break;
                
            case SettingType.Custom:
                toggleElement.data.onValueChanged?.Invoke(toggleElement.isOn);
                break;
        }
    }

    private void SaveSetting(ToggleSettingElements toggleElement)
    {
        string key = $"Setting_{toggleElement.data.settingType}_{toggleElement.data.settingLabel}";
        PlayerPrefs.SetInt(key, toggleElement.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        foreach (var toggleElement in _toggleElements)
        {
            string key = $"Setting_{toggleElement.data.settingType}_{toggleElement.data.settingLabel}";
            
            if (PlayerPrefs.HasKey(key))
            {
                toggleElement.isOn = PlayerPrefs.GetInt(key) == 1;
            }
            else
            {
                toggleElement.isOn = toggleElement.data.defaultValue;
            }
            
            UpdateToggleVisual(toggleElement);
            InvokeSettingCallback(toggleElement);
        }
    }

    protected override void OnResetClicked()
    {
        PlayClickSound();
        Debug.Log("Resetting all toggle settings to defaults");
        
        foreach (var toggleElement in _toggleElements)
        {
            toggleElement.isOn = toggleElement.data.defaultValue;
            UpdateToggleVisual(toggleElement);
            InvokeSettingCallback(toggleElement);
            SaveSetting(toggleElement);
        }
    }

    private void RegisterDecorationHover(ToggleSettingElements toggleElement)
    {
        if (toggleElement.toggleButton == null || toggleElement.toggleContainer == null) return;

        toggleElement.toggleContainer.RegisterCallback<MouseEnterEvent>(evt => 
        {
            if (toggleElement.leftDecoration != null)
            {
                toggleElement.leftDecoration.style.opacity = 1;
            }
            if (toggleElement.rightDecoration != null)
            {
                toggleElement.rightDecoration.style.opacity = 1;
            }
        });

        toggleElement.toggleContainer.RegisterCallback<MouseLeaveEvent>(evt => 
        {
            if (!toggleElement.toggleButton.ClassListContains("unity-button:focus"))
            {
                if (toggleElement.leftDecoration != null)
                {
                    toggleElement.leftDecoration.style.opacity = 0;
                }
                if (toggleElement.rightDecoration != null)
                {
                    toggleElement.rightDecoration.style.opacity = 0;
                }
            }
        });
    }

    private void RegisterDecorationFocus(ToggleSettingElements toggleElement)
    {
        if (toggleElement.toggleButton == null) return;

        toggleElement.toggleButton.RegisterCallback<FocusInEvent>(evt => 
        {
            if (toggleElement.leftDecoration != null)
            {
                toggleElement.leftDecoration.style.opacity = 1;
            }
            if (toggleElement.rightDecoration != null)
            {
                toggleElement.rightDecoration.style.opacity = 1;
            }
        });

        toggleElement.toggleButton.RegisterCallback<FocusOutEvent>(evt => 
        {
            if (toggleElement.leftDecoration != null)
            {
                toggleElement.leftDecoration.style.opacity = 0;
            }
            if (toggleElement.rightDecoration != null)
            {
                toggleElement.rightDecoration.style.opacity = 0;
            }
        });
    }

    // Public method to get current value of a setting
    public bool GetSettingValue(SettingType settingType)
    {
        var element = _toggleElements.Find(e => e.data.settingType == settingType);
        return element?.isOn ?? false;
    }

    // Public method to set a setting value programmatically
    public void SetSettingValue(SettingType settingType, bool value)
    {
        var element = _toggleElements.Find(e => e.data.settingType == settingType);
        if (element != null)
        {
            element.isOn = value;
            UpdateToggleVisual(element);
            SaveSetting(element);
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying && _root != null)
        {
            CreateToggleSettings();
            SetupToggleCallbacks();
            ApplyToggleVisuals();
        }
    }

    [System.Serializable]
    public enum SettingType
    {
        FullScreen,
        ParticleEffects,
        Rumble,
        NativeControllerInput,
        ShowAchievements,
        Custom
    }

    [System.Serializable]
    public class ToggleSettingData
    {
        [Tooltip("Label displayed inside the toggle")]
        public string settingLabel = "Setting";
        
        [Tooltip("Type of setting this toggle controls")]
        public SettingType settingType = SettingType.Custom;
        
        [Tooltip("Default value for this setting")]
        public bool defaultValue = true;
        
        [Tooltip("Show ON/OFF state in the label text")]
        public bool showStateInLabel = true;
        
        [Tooltip("Custom callback for Custom setting type")]
        public Action<bool> onValueChanged;
    }

    private class ToggleSettingElements
    {
        public Button toggleButton;
        public VisualElement sliderBackground;
        public VisualElement sliderKnob;
        public Label label;
        public VisualElement toggleContainer;
        public VisualElement leftDecoration;
        public VisualElement rightDecoration;
        public ToggleSettingData data;
        public bool isOn;
    }
}