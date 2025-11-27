using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InputSettingsMenuController : BaseMenuController
{
    [Header("Input Display Settings")]
    [SerializeField] private List<InputDisplayItem> _inputItems = new List<InputDisplayItem>();

    [Header("Scroll Control")]
    [SerializeField] private bool _useSliderInsteadOfScrollbar = true;

    [Header("Slider Settings")]
    [SerializeField] private Color _sliderTrackColor = new Color(0.2f, 0.2f, 0.2f);
    [SerializeField] private Color _sliderHandleColor = Color.white;
    [SerializeField] private Sprite _sliderHandleSprite;
    [SerializeField] private float _sliderHandleHeight = 60f;
    [SerializeField] private bool _showSliderTrackBorder = false;
    [SerializeField] private Color _sliderTrackBorderColor = new Color(1, 1, 1, 0.2f);
    [SerializeField] private float _sliderTrackBorderWidth = 1f;

    [Header("Text Colors")]
    [SerializeField] private Color _itemHeaderColor = Color.white;
    [SerializeField] private Color _itemDescriptionColor = new Color(0.8f, 0.8f, 0.8f);

    private VisualElement _contentArea;
    private ScrollView _scrollView;
    private Slider _customSlider;
    private VisualElement _sliderContainer;
    private bool _isInternalUpdate = false;

    [System.Serializable]
    public class InputDisplayItem
    {
        [Tooltip("Header text for this input section")]
        public string header = "KEYBOARD";
        
        [Tooltip("Description or instructions")]
        [TextArea(2, 5)]
        public string description = "Use arrow keys to move";
        
        [Tooltip("Image showing the input layout")]
        public Sprite layoutImage;
    }

    protected override void OnEnableCustom()
    {
        SetupContentStructure();
        
        if (_useSliderInsteadOfScrollbar)
        {
            SetupCustomSlider();
        }
        
        PopulateContent();
    }

    private void SetupContentStructure()
    {
        if (_contentContainer == null) return;

        // Check if ContentArea already exists in UXML
        _contentArea = _root?.Q("ContentArea");
        
        if (_contentArea == null)
        {
            // Create ContentArea structure programmatically
            _contentArea = new VisualElement();
            _contentArea.name = "ContentArea";
            _contentArea.AddToClassList("content-area");
            _contentArea.style.flexGrow = 1;
            _contentArea.style.flexDirection = FlexDirection.Row;
            _contentArea.style.position = Position.Relative;
            _contentArea.style.minHeight = Length.Percent(50);
            
            // Replace ContentContainer with ContentArea
            var parent = _contentContainer.parent;
            var contentIndex = parent.IndexOf(_contentContainer);
            parent.RemoveAt(contentIndex);
            parent.Insert(contentIndex, _contentArea);
        }

        // Create or find ScrollView
        _scrollView = _contentArea.Q<ScrollView>("ContentScrollView");
        
        if (_scrollView == null)
        {
            _scrollView = new ScrollView();
            _scrollView.name = "ContentScrollView";
            _scrollView.AddToClassList("scroll-view");
            _scrollView.mode = ScrollViewMode.Vertical;
            _scrollView.style.flexGrow = 1;
            _contentArea.Add(_scrollView);
            
            // Create content container inside scroll view
            var innerContent = new VisualElement();
            innerContent.name = "ContentContainer";
            innerContent.AddToClassList("content-container");
            _scrollView.Add(innerContent);
        }

        // Hide scrollbar if using slider
        if (_useSliderInsteadOfScrollbar)
        {
            _scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
        }
    }

    private void SetupCustomSlider()
    {
        if (_scrollView == null || _contentArea == null) return;

        // Check if slider already exists in UXML
        _sliderContainer = _contentArea.Q("SliderContainer");
        
        if (_sliderContainer == null)
        {
            _sliderContainer = new VisualElement();
            _sliderContainer.name = "SliderContainer";
            _sliderContainer.AddToClassList("slider-container");
            _sliderContainer.style.position = Position.Relative;
            _sliderContainer.style.width = 30;
            _sliderContainer.style.flexShrink = 0;
            _sliderContainer.style.marginLeft = 2;
            
            _customSlider = new Slider();
            _customSlider.name = "CustomSlider";
            _customSlider.direction = SliderDirection.Vertical;
            _customSlider.inverted = true;
            _customSlider.lowValue = 0;
            _customSlider.highValue = 100;
            _customSlider.value = 0;
            _customSlider.style.flexGrow = 1;
            _customSlider.style.width = 30;
            
            _sliderContainer.Add(_customSlider);
            _contentArea.Add(_sliderContainer);
        }
        else
        {
            _customSlider = _sliderContainer.Q<Slider>("CustomSlider");
        }
        
        if (_customSlider == null) return;
        
        // Setup Visuals
        var dragger = _customSlider.Q("unity-dragger");
        if (dragger != null)
        {
            dragger.style.height = _sliderHandleHeight;
            dragger.style.backgroundColor = _sliderHandleColor;
            SetBorderRadius(dragger, 8);
            dragger.style.width = 30;
            dragger.style.marginLeft = 0;
            
            if (_sliderHandleSprite != null)
            {
                dragger.style.backgroundImage = new StyleBackground(_sliderHandleSprite);
                dragger.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            }
        }
        
        var dragContainer = _customSlider.Q("unity-drag-container");
        if (dragContainer != null)
        {
            dragContainer.style.left = 0;
            dragContainer.style.width = 30;
            dragContainer.style.marginLeft = 0;
        }
        
        var tracker = _customSlider.Q("unity-tracker");
        if (tracker != null)
        {
            tracker.style.backgroundColor = _sliderTrackColor;
            tracker.style.width = 6;
            tracker.style.marginLeft = 12;
            SetBorderRadius(tracker, 3);
            
            SetBorderWidth(tracker, _showSliderTrackBorder ? _sliderTrackBorderWidth : 0);
            if (_showSliderTrackBorder) SetBorderColor(tracker, _sliderTrackBorderColor);
        }
        
        // Setup Events
        _customSlider.RegisterValueChangedCallback(OnSliderValueChanged);
        _scrollView.verticalScroller.valueChanged += OnNativeScrolled;
        _scrollView.RegisterCallback<GeometryChangedEvent>(UpdateSliderRange);
    }

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
    
    private void OnNativeScrolled(float newValue)
    {
        if (_customSlider == null || _scrollView == null) return;

        float highValue = _scrollView.verticalScroller.highValue;
        if (highValue > 0)
        {
            float percent = Mathf.Clamp01(newValue / highValue);
            
            _isInternalUpdate = true;
            _customSlider.SetValueWithoutNotify(percent * 100f);
            _isInternalUpdate = false;
        }
    }

    private void UpdateSliderRange(GeometryChangedEvent evt)
    {
        if (_scrollView == null || _customSlider == null || _sliderContainer == null) return;
        
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

    private void PopulateContent()
    {
        if (_scrollView == null) return;

        // Get or create the inner content container
        var contentContainer = _scrollView.Q("ContentContainer");
        if (contentContainer == null)
        {
            contentContainer = _scrollView.contentContainer;
        }

        // Clear the content
        contentContainer.Clear();

        foreach (var item in _inputItems)
        {
            var itemElement = CreateInputItem(item);
            contentContainer.Add(itemElement);
        }
    }

    private VisualElement CreateInputItem(InputDisplayItem item)
    {
        // Main container for this input item
        var container = new VisualElement();
        container.AddToClassList("input-item");
        container.style.width = Length.Percent(100);
        container.style.marginBottom = 30;
        container.style.alignItems = Align.Center;

        // Header text
        if (!string.IsNullOrEmpty(item.header))
        {
            var headerLabel = new Label(item.header);
            headerLabel.AddToClassList("input-item-header");
            headerLabel.style.fontSize = 24;
            headerLabel.style.color = _itemHeaderColor;
            headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            headerLabel.style.marginBottom = 10;
            headerLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            container.Add(headerLabel);
        }

        // Description text
        if (!string.IsNullOrEmpty(item.description))
        {
            var descLabel = new Label(item.description);
            descLabel.AddToClassList("input-item-description");
            descLabel.style.fontSize = 14;
            descLabel.style.color = _itemDescriptionColor;
            descLabel.style.marginBottom = 15;
            descLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            descLabel.style.whiteSpace = WhiteSpace.Normal;
            container.Add(descLabel);
        }

        // Image display (100% width)
        if (item.layoutImage != null)
        {
            var imageContainer = new VisualElement();
            imageContainer.AddToClassList("input-item-image");
            imageContainer.style.width = Length.Percent(100);
            imageContainer.style.height = 400;
            imageContainer.style.backgroundImage = new StyleBackground(item.layoutImage);
            imageContainer.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            imageContainer.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);
            container.Add(imageContainer);
        }

        return container;
    }

    protected override void OnResetClicked()
    {
        PlayClickSound();
        
        // Scroll to top
        if (_scrollView != null)
        {
            _scrollView.scrollOffset = Vector2.zero;
        }
        
        Debug.Log("Input Settings Reset - Scrolled to top");
    }

    // Public methods for runtime manipulation
    public void AddInputItem(string header, string description, Sprite image)
    {
        var newItem = new InputDisplayItem
        {
            header = header,
            description = description,
            layoutImage = image
        };
        
        _inputItems.Add(newItem);
        
        if (_scrollView != null)
        {
            var contentContainer = _scrollView.Q("ContentContainer") ?? _scrollView.contentContainer;
            var itemElement = CreateInputItem(newItem);
            contentContainer.Add(itemElement);
        }
    }

    public void ClearInputItems()
    {
        _inputItems.Clear();
        if (_scrollView != null)
        {
            var contentContainer = _scrollView.Q("ContentContainer") ?? _scrollView.contentContainer;
            contentContainer.Clear();
        }
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

    private void OnValidate()
    {
        if (Application.isPlaying && _root != null)
        {
            SetupContentStructure();
            
            if (_useSliderInsteadOfScrollbar)
            {
                SetupCustomSlider();
            }
            
            PopulateContent();
        }
    }
}