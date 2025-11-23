using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// ============================================
// CustomScrollViewController.cs
// ============================================

public class CustomScrollViewController : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField] private UIDocument _uiDocument;

    [Header("Header Settings")]
    [SerializeField] private string _headerText = "Scroll Menu";
    [SerializeField] private Sprite _headerSeparator;

    [Header("Footer Settings")]
    [SerializeField] private string _footerText = "Close";
    [SerializeField] private bool _showFooterSeparator = true;
    [SerializeField] private Sprite _footerLeftDecoration;
    [SerializeField] private Sprite _footerRightDecoration;

    [Header("Content Settings")]
    [SerializeField] private LayoutMode _layoutMode = LayoutMode.TwoColumn;
    [SerializeField] private List<ScrollItemData> _items = new List<ScrollItemData>();
    [SerializeField] private bool _useSliderInsteadOfScrollbar = false;

    [Header("Scrollbar Styling")]
    [SerializeField] private Sprite _scrollbarHandle;
    [SerializeField] private Sprite _scrollUpArrow;
    [SerializeField] private Sprite _scrollDownArrow;
    [SerializeField] private Color _scrollbarTrackColor = new Color(0.2f, 0.2f, 0.2f);
    [SerializeField] private Color _scrollbarHandleColor = Color.white;
    [SerializeField] private bool _showScrollbarHandleBackground = true;
    [SerializeField] private bool _showScrollbarHandleBorder = true;
    [SerializeField] private Color _scrollbarHandleBackgroundColor = new Color(0, 0, 0, 0);
    [SerializeField] private Color _scrollbarHandleBorderColor = new Color(1, 1, 1, 0.2f);
    [SerializeField] private float _scrollbarHandleBorderWidth = 1f;
    [SerializeField] private bool _showScrollbarTrackBorder = false;
    [SerializeField] private Color _scrollbarTrackBorderColor = new Color(1, 1, 1, 0.2f);
    [SerializeField] private float _scrollbarTrackBorderWidth = 1f;

    [Header("Slider Settings (Alternative to Scrollbar)")]
    [SerializeField] private Color _sliderTrackColor = new Color(0.2f, 0.2f, 0.2f);
    [SerializeField] private Color _sliderHandleColor = Color.white;
    [SerializeField] private Sprite _sliderHandleSprite;
    [SerializeField] private float _sliderHandleHeight = 60f;
    [SerializeField] private bool _showSliderTrackBorder = false;
    [SerializeField] private Color _sliderTrackBorderColor = new Color(1, 1, 1, 0.2f);
    [SerializeField] private float _sliderTrackBorderWidth = 1f;

    [Header("Color Theme")]
    [SerializeField] private Color _headerTextColor = Color.white;
    [SerializeField] private Color _footerButtonTextColor = Color.white;
    [SerializeField] private Color _footerButtonBackgroundColor = new Color(0.145f, 0.388f, 0.922f); 
    [SerializeField] private Color _footerButtonHoverColor = new Color(0.231f, 0.510f, 0.965f); 
    [SerializeField] private Color _footerButtonActiveColor = new Color(0.118f, 0.251f, 0.686f); 

    public enum LayoutMode
    {
        SingleColumn,
        TwoColumn
    }

    [System.Serializable]
    public class ScrollItemData
    {
        [TextArea(3, 10)] public string text;
        public Sprite image;
    }

    // UI References
    private VisualElement _root;
    private Label _headerLabel;
    private VisualElement _headerSeparatorImage;
    private ScrollView _scrollView;
    private VisualElement _contentContainer;
    private Button _footerButton;
    private VisualElement _footerSeparatorImage;
    private VisualElement _footerLeftDecorationImage;
    private VisualElement _footerRightDecorationImage;
    
    // Slider control
    private Slider _customSlider;
    private VisualElement _sliderContainer;

    // Navigation State
    private int _focusedItemIndex = -1;
    private bool _isInternalUpdate = false; // Prevents infinite loop between slider and scrollview

    // Store default theme colors for reset
    private static readonly Color DefaultHeaderTextColor = Color.white;
    private static readonly Color DefaultFooterButtonTextColor = Color.white;
    private static readonly Color DefaultFooterButtonBackgroundColor = new Color(0.145f, 0.388f, 0.922f);
    private static readonly Color DefaultFooterButtonHoverColor = new Color(0.231f, 0.510f, 0.965f);
    private static readonly Color DefaultFooterButtonActiveColor = new Color(0.118f, 0.251f, 0.686f);

    private void OnEnable()
    {
        if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();
        _root = _uiDocument.rootVisualElement;

        BindElements();
        ApplySettings();
        PopulateContent();
        SetupEventHandlers();
        
        if (_useSliderInsteadOfScrollbar)
        {
            SetupCustomSlider();
        }
    }

    private void BindElements()
    {
        _headerLabel = _root.Q<Label>("HeaderText");
        _headerSeparatorImage = _root.Q("HeaderSeparator");
        _scrollView = _root.Q<ScrollView>("ContentScrollView");
        _contentContainer = _root.Q("ContentContainer");
        _footerButton = _root.Q<Button>("FooterButton");
        _footerSeparatorImage = _root.Q("FooterSeparator");
        _footerLeftDecorationImage = _root.Q("FooterLeftDecoration");
        _footerRightDecorationImage = _root.Q("FooterRightDecoration");
        
        _sliderContainer = _root.Q("SliderContainer");
        _customSlider = _root.Q<Slider>("CustomSlider");
    }

    private void ApplySettings()
    {
        if (_headerLabel != null)
        {
            _headerLabel.text = _headerText;
            _headerLabel.style.color = _headerTextColor;
        }

        if (_headerSeparatorImage != null)
        {
            if (_headerSeparator != null)
                _headerSeparatorImage.style.backgroundImage = new StyleBackground(_headerSeparator);
            else
                _headerSeparatorImage.style.display = DisplayStyle.None;
        }

        if (_footerButton != null)
        {
            _footerButton.text = _footerText;
            ApplyFooterButtonColors();
        }

        if (_footerSeparatorImage != null)
        {
            _footerSeparatorImage.style.display = _showFooterSeparator ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if (_footerLeftDecorationImage != null)
        {
            if (_footerLeftDecoration != null)
                _footerLeftDecorationImage.style.backgroundImage = new StyleBackground(_footerLeftDecoration);
            else
                _footerLeftDecorationImage.style.display = DisplayStyle.None;
        }

        if (_footerRightDecorationImage != null)
        {
            if (_footerRightDecoration != null)
                _footerRightDecorationImage.style.backgroundImage = new StyleBackground(_footerRightDecoration);
            else
                _footerRightDecorationImage.style.display = DisplayStyle.None;
        }

        if (!_useSliderInsteadOfScrollbar)
        {
            ApplyScrollbarStyling();
        }
        else
        {
            if (_scrollView != null)
            {
                _scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            }
        }
    }

    private void ApplyScrollbarStyling()
    {
        if (_scrollView == null) return;
        var verticalScroller = _scrollView.verticalScroller;
        if (verticalScroller == null) return;

        var track = verticalScroller.Q("unity-tracker");
        if (track != null)
        {
            track.style.backgroundColor = _scrollbarTrackColor;
            SetBorderWidth(track, _showScrollbarTrackBorder ? _scrollbarTrackBorderWidth : 0);
            if (_showScrollbarTrackBorder) SetBorderColor(track, _scrollbarTrackBorderColor);
        }

        var dragger = verticalScroller.Q("unity-dragger");
        if (dragger != null)
        {
            dragger.style.unityBackgroundImageTintColor = _scrollbarHandleColor;
            dragger.style.backgroundColor = _showScrollbarHandleBackground ? _scrollbarHandleBackgroundColor : new Color(0,0,0,0);
            SetBorderWidth(dragger, _showScrollbarHandleBorder ? _scrollbarHandleBorderWidth : 0);
            if (_showScrollbarHandleBorder) SetBorderColor(dragger, _scrollbarHandleBorderColor);
            
            if (_scrollbarHandle != null)
                dragger.style.backgroundImage = new StyleBackground(_scrollbarHandle);
        }

        var upButton = verticalScroller.Q("unity-low-button");
        if (upButton != null && _scrollUpArrow != null)
        {
            upButton.style.backgroundImage = new StyleBackground(_scrollUpArrow);
            upButton.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
        }

        var downButton = verticalScroller.Q("unity-high-button");
        if (downButton != null && _scrollDownArrow != null)
        {
            downButton.style.backgroundImage = new StyleBackground(_scrollDownArrow);
            downButton.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
        }
    }

    private void ApplyFooterButtonColors()
    {
        if (_footerButton == null) return;

        _footerButton.style.color = _footerButtonTextColor;
        _footerButton.style.backgroundColor = _footerButtonBackgroundColor;

        _footerButton.UnregisterCallback<MouseEnterEvent>(OnFooterButtonMouseEnter);
        _footerButton.UnregisterCallback<MouseLeaveEvent>(OnFooterButtonMouseLeave);
        _footerButton.UnregisterCallback<MouseDownEvent>(OnFooterButtonMouseDown);
        _footerButton.UnregisterCallback<MouseUpEvent>(OnFooterButtonMouseUp);

        _footerButton.RegisterCallback<MouseEnterEvent>(OnFooterButtonMouseEnter);
        _footerButton.RegisterCallback<MouseLeaveEvent>(OnFooterButtonMouseLeave);
        _footerButton.RegisterCallback<MouseDownEvent>(OnFooterButtonMouseDown);
        _footerButton.RegisterCallback<MouseUpEvent>(OnFooterButtonMouseUp);
    }

    private void OnFooterButtonMouseEnter(MouseEnterEvent evt) { if (_footerButton != null) _footerButton.style.backgroundColor = _footerButtonHoverColor; }
    private void OnFooterButtonMouseLeave(MouseLeaveEvent evt) { if (_footerButton != null) _footerButton.style.backgroundColor = _footerButtonBackgroundColor; }
    private void OnFooterButtonMouseDown(MouseDownEvent evt) { if (_footerButton != null) _footerButton.style.backgroundColor = _footerButtonActiveColor; }
    private void OnFooterButtonMouseUp(MouseUpEvent evt) { if (_footerButton != null) _footerButton.style.backgroundColor = _footerButtonHoverColor; }

    private void SetBorderWidth(VisualElement element, float width)
    {
        element.style.borderLeftWidth = width;
        element.style.borderRightWidth = width;
        element.style.borderTopWidth = width;
        element.style.borderBottomWidth = width;
    }

    private void SetBorderColor(VisualElement element, Color color)
    {
        element.style.borderTopColor = color;
        element.style.borderBottomColor = color;
        element.style.borderLeftColor = color;
        element.style.borderRightColor = color;
    }

    private void SetBorderRadius(VisualElement element, float radius)
    {
        element.style.borderTopLeftRadius = radius;
        element.style.borderTopRightRadius = radius;
        element.style.borderBottomLeftRadius = radius;
        element.style.borderBottomRightRadius = radius;
    }

    private void SetupCustomSlider()
    {
        if (_scrollView == null) return;

        if (_sliderContainer == null)
        {
            _sliderContainer = new VisualElement();
            _sliderContainer.name = "SliderContainer"; 
            _sliderContainer.style.position = Position.Absolute;
            _sliderContainer.style.right = 2; 
            _sliderContainer.style.top = 10;
            _sliderContainer.style.bottom = 10;
            
            _customSlider = new Slider();
            _customSlider.name = "CustomSlider"; 
            _customSlider.direction = SliderDirection.Vertical;
            _customSlider.inverted = true; 
            _customSlider.lowValue = 0;
            _customSlider.highValue = 100;
            _customSlider.value = 0;
            _customSlider.style.flexGrow = 1;
            
            _sliderContainer.Add(_customSlider);
            _scrollView.parent.Add(_sliderContainer);
        }
        
        // --- Setup Visuals ---
        var dragger = _customSlider.Q("unity-dragger");
        if (dragger != null)
        {
            dragger.style.height = _sliderHandleHeight;
            dragger.style.backgroundColor = _sliderHandleColor;
            SetBorderRadius(dragger, 8);
            
            if (_sliderHandleSprite != null)
            {
                dragger.style.backgroundImage = new StyleBackground(_sliderHandleSprite);
                dragger.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            }
        }
        
        var dragContainer = _customSlider.Q("unity-drag-container");
        if (dragContainer != null) dragContainer.style.left = 0; 
        
        var tracker = _customSlider.Q("unity-tracker");
        if (tracker != null)
        {
            tracker.style.backgroundColor = _sliderTrackColor;
            SetBorderRadius(tracker, 3);
            
            SetBorderWidth(tracker, _showSliderTrackBorder ? _sliderTrackBorderWidth : 0);
            if (_showSliderTrackBorder) SetBorderColor(tracker, _sliderTrackBorderColor);
        }
        
        // --- EVENTS ---
        
        // 1. Slider -> Content (User drags slider)
        _customSlider.RegisterValueChangedCallback(OnSliderValueChanged);
        
        // 2. Content -> Slider (Trackpad, MouseWheel, Arrow Keys)
        // FIXED: Added <float> to solve CS0411 error
        _scrollView.verticalScroller.valueChanged += OnNativeScrolled;


        // 3. Resize detection
        _scrollView.RegisterCallback<GeometryChangedEvent>(UpdateSliderRange);
    }

    // Called when user drags custom slider
    private void OnSliderValueChanged(ChangeEvent<float> evt)
    {
        if (_scrollView == null || _isInternalUpdate) return;
        
        float maxScroll = _scrollView.contentContainer.layout.height - _scrollView.layout.height;
        if (maxScroll > 0)
        {
            float normalizedValue = evt.newValue / 100f;
            _scrollView.scrollOffset = new Vector2(0, normalizedValue * maxScroll);
        }
    }
    
    // Called when content moves (via arrow keys, mousewheel, trackpad)
    private void OnNativeScrolled(float newValue)
    {
        if (_customSlider == null || _scrollView == null) return;

        float highValue = _scrollView.verticalScroller.highValue;
        if (highValue > 0)
        {
            // Calculate 0-1 percentage
            // FIXED: Use 'newValue' directly instead of 'evt.newValue'
            float percent = Mathf.Clamp01(newValue / highValue);
            
            // Set flag to prevent loop
            _isInternalUpdate = true;
            _customSlider.SetValueWithoutNotify(percent * 100f);
            _isInternalUpdate = false;
        }
    }
    private void UpdateSliderRange(GeometryChangedEvent evt)
    {
        if (_scrollView == null || _customSlider == null) return;
        
        float contentHeight = _scrollView.contentContainer.layout.height;
        float viewportHeight = _scrollView.layout.height;
        
        if (contentHeight > viewportHeight)
        {
            _customSlider.SetEnabled(true);
            _sliderContainer.style.display = DisplayStyle.Flex;
        }
        else
        {
            _customSlider.SetEnabled(false);
            _sliderContainer.style.display = DisplayStyle.None;
        }
    }

    private void PopulateContent()
    {
        if (_contentContainer == null) return;

        _contentContainer.Clear();
        _focusedItemIndex = -1; 

        for (int i = 0; i < _items.Count; i++)
        {
            VisualElement itemElement = CreateContentItem(_items[i], i);
            _contentContainer.Add(itemElement);
        }
    }

    private VisualElement CreateContentItem(ScrollItemData item, int index)
    {
        var container = new VisualElement();
        container.AddToClassList("content-item");
        container.focusable = true;
        
        container.RegisterCallback<ClickEvent>(evt => 
        {
            _focusedItemIndex = index;
            container.Focus();
        });

        if (_layoutMode == LayoutMode.TwoColumn && item.image != null)
        {
            container.style.flexDirection = FlexDirection.Row;

            var imageContainer = new VisualElement();
            imageContainer.AddToClassList("item-image-container");
            imageContainer.style.backgroundImage = new StyleBackground(item.image);
            imageContainer.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            container.Add(imageContainer);

            var textLabel = CreateTextLabel(item.text);
            textLabel.style.flexGrow = 1;
            container.Add(textLabel);
        }
        else
        {
            var textLabel = CreateTextLabel(item.text);
            container.Add(textLabel);
        }

        return container;
    }

    private Label CreateTextLabel(string text)
    {
        var label = new Label();
        label.AddToClassList("item-text");
        label.text = text;
        label.enableRichText = true; 
        return label;
    }

    private void SetupEventHandlers()
    {
        if (_footerButton != null)
            _footerButton.clicked += OnFooterButtonClicked;
            
        if (_scrollView != null)
        {
            _scrollView.focusable = true;
            // TrickleDown is essential for single-click navigation response
            _scrollView.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }
    }

    private void OnKeyDown(KeyDownEvent evt)
    {
        if (_contentContainer == null || _contentContainer.childCount == 0) return;

        if (evt.keyCode == KeyCode.DownArrow)
        {
            NavigateSelection(1);
            evt.StopPropagation(); // Stop propagation immediately
        }
        else if (evt.keyCode == KeyCode.UpArrow)
        {
            NavigateSelection(-1);
            evt.StopPropagation();
        }
    }

    private void NavigateSelection(int direction)
    {
        // If we haven't started selecting yet
        if (_focusedItemIndex == -1)
        {
            _focusedItemIndex = (direction > 0) ? 0 : _contentContainer.childCount - 1;
        }
        else
        {
            _focusedItemIndex += direction;
        }

        // Clamp values
        if (_focusedItemIndex < 0) _focusedItemIndex = 0;
        if (_focusedItemIndex >= _contentContainer.childCount) _focusedItemIndex = _contentContainer.childCount - 1;

        var element = _contentContainer[_focusedItemIndex];
        
        // Focus and Scroll
        element.Focus();
        _scrollView.ScrollTo(element); 
    }

    private void OnFooterButtonClicked()
    {
        Debug.Log($"Footer button '{_footerText}' clicked!");
        gameObject.SetActive(false);
    }

    public void AddItem(string text, Sprite image = null)
    {
        var newItem = new ScrollItemData { text = text, image = image };
        _items.Add(newItem);
        
        if (_contentContainer != null)
        {
            var itemElement = CreateContentItem(newItem, _items.Count - 1);
            _contentContainer.Add(itemElement);
        }
    }

    public void ClearItems()
    {
        _items.Clear();
        if (_contentContainer != null)
            _contentContainer.Clear();
        _focusedItemIndex = -1;
    }

    public void SetLayoutMode(LayoutMode mode)
    {
        _layoutMode = mode;
        PopulateContent();
    }

    public void SetHeaderTextColor(Color color)
    {
        _headerTextColor = color;
        if (_headerLabel != null) _headerLabel.style.color = _headerTextColor;
    }

    public void SetFooterButtonColors(Color textColor, Color backgroundColor, Color hoverColor, Color activeColor)
    {
        _footerButtonTextColor = textColor;
        _footerButtonBackgroundColor = backgroundColor;
        _footerButtonHoverColor = hoverColor;
        _footerButtonActiveColor = activeColor;
        ApplyFooterButtonColors();
    }

    public void ResetColorTheme()
    {
        _headerTextColor = DefaultHeaderTextColor;
        _footerButtonTextColor = DefaultFooterButtonTextColor;
        _footerButtonBackgroundColor = DefaultFooterButtonBackgroundColor;
        _footerButtonHoverColor = DefaultFooterButtonHoverColor;
        _footerButtonActiveColor = DefaultFooterButtonActiveColor;
        
        if (_headerLabel != null) _headerLabel.style.color = _headerTextColor;
        ApplyFooterButtonColors();
    }

    public void ToggleSliderMode(bool useSlider)
    {
        _useSliderInsteadOfScrollbar = useSlider;
        
        if (_scrollView != null)
        {
            if (useSlider)
            {
                _scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
                if (_sliderContainer == null) SetupCustomSlider();
                else _sliderContainer.style.display = DisplayStyle.Flex;
            }
            else
            {
                _scrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;
                if (_sliderContainer != null) _sliderContainer.style.display = DisplayStyle.None;
            }
        }
    }

    [ContextMenu("Reset Color Theme")]
    private void ResetColorThemeContextMenu()
    {
        ResetColorTheme();
    }

    [ContextMenu("Test - Add Sample Items")]
    private void TestAddSampleItems()
    {
        _items.Clear();
        
        _items.Add(new ScrollItemData { text = "<b>Bold Item 1</b>\nThis is a description with <i>italic text</i>." });
        _items.Add(new ScrollItemData { text = "<color=#FF0000>Red text</color> and <color=#00FF00>green text</color>." });
        _items.Add(new ScrollItemData { text = "Regular text item without formatting." });
        
        for (int i = 4; i <= 10; i++)
        {
            _items.Add(new ScrollItemData { text = $"<size=16><b>Item {i}</b></size>\nSample content for scrolling test." });
        }
        
        if (Application.isPlaying) PopulateContent();
    }
}