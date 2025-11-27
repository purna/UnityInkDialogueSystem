using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class OptionsSettingsMenuController : BaseMenuController
{
    [Header("Content Button Settings")]
    [SerializeField] private List<ContentButtonData> _contentButtons = new List<ContentButtonData>();

    [Header("Button Decoration Images")]
    [SerializeField] private Sprite _contentLeftDecoration;
    [SerializeField] private Sprite _contentRightDecoration;

    [Header("Content Button Colors")]
    [SerializeField] private Color _contentBtnNormal = new Color(0.145f, 0.388f, 0.922f);
    [SerializeField] private Color _contentBtnHover = new Color(0.231f, 0.510f, 0.965f);

    private List<ContentButtonElements> _contentButtonElements = new List<ContentButtonElements>();

    protected override void OnEnableCustom()
    {
        CreateContentButtons();
        SetupContentButtonCallbacks();
        ApplyContentVisuals();
    }

    private void CreateContentButtons()
    {
        if (_contentContainer == null) return;

        _contentContainer.Clear();
        _contentButtonElements.Clear();

        foreach (var buttonData in _contentButtons)
        {
            // Create row container - this centers the button container
            var row = new VisualElement();
            row.AddToClassList("slider-row");
            row.style.alignItems = Align.Center;
            row.style.justifyContent = Justify.Center;

            // Create button container (holds decorations + button)
            var buttonContainer = new VisualElement();
            buttonContainer.AddToClassList("content-button-container");

            // Create left decoration
            var leftDeco = new VisualElement();
            leftDeco.name = "ContentLeftDecoration";
            leftDeco.AddToClassList("content-decoration");
            leftDeco.style.opacity = 0; // Start hidden

            // Create the button
            var button = new Button();
            button.text = buttonData.buttonText;
            button.AddToClassList("content-button");

            // Create right decoration
            var rightDeco = new VisualElement();
            rightDeco.name = "ContentRightDecoration";
            rightDeco.AddToClassList("content-decoration");
            rightDeco.style.opacity = 0; // Start hidden

            // Assemble the structure
            buttonContainer.Add(leftDeco);
            buttonContainer.Add(button);
            buttonContainer.Add(rightDeco);
            row.Add(buttonContainer);
            _contentContainer.Add(row);

            // Store references
            _contentButtonElements.Add(new ContentButtonElements
            {
                button = button,
                buttonContainer = buttonContainer,
                leftDecoration = leftDeco,
                rightDecoration = rightDeco,
                data = buttonData
            });
        }
    }

    private void SetupContentButtonCallbacks()
    {
        foreach (var btnElement in _contentButtonElements)
        {
            if (btnElement.button != null)
            {
                btnElement.button.clicked += () => OnContentButtonClicked(btnElement.data);
                RegisterButtonHover(btnElement.button, _contentBtnNormal, _contentBtnHover);
                
                // Register hover/focus on BOTH button and container for decorations
                RegisterDecorationHover(btnElement);
                RegisterDecorationFocus(btnElement);
            }
        }
    }

    private void ApplyContentVisuals()
    {
        foreach (var btnElement in _contentButtonElements)
        {
            if (btnElement.button != null)
            {
                btnElement.button.style.backgroundColor = _contentBtnNormal;
            }

            // Set decoration images
            SetBg(btnElement.leftDecoration, _contentLeftDecoration);
            SetBg(btnElement.rightDecoration, _contentRightDecoration);
            
            // Ensure decorations start hidden (opacity already set in CreateContentButtons)
            // The CSS transition will handle the smooth fade in/out
        }
    }

    private void OnContentButtonClicked(ContentButtonData data)
    {
        PlayClickSound();
        Debug.Log($"Content Button Clicked: {data.buttonText}");

        // Load scene if specified
        if (!string.IsNullOrEmpty(data.targetScene.SceneName))
        {
            Debug.Log($"Loading scene: {data.targetScene.SceneName}");
            SceneManager.LoadScene(data.targetScene.SceneName);
            return;
        }

        // Show UI Document if specified
        if (data.targetUIDocument != null)
        {
            if (UIDocumentManager.Instance != null)
            {
                UIDocumentManager.Instance.ShowDocument(data.targetUIDocument, true);
            }
            else
            {
                data.targetUIDocument.gameObject.SetActive(true);
            }
            
            if (data.hideCurrentMenu)
            {
                gameObject.SetActive(false);
            }
        }
        else if (string.IsNullOrEmpty(data.targetScene.SceneName))
        {
            Debug.LogWarning($"No target (Scene or UI Document) assigned for button: {data.buttonText}");
        }
    }

    protected override void OnResetClicked()
    {
        PlayClickSound();
        Debug.Log("Settings Reset - Options Menu");
    }

    private void RegisterDecorationHover(ContentButtonElements btnElement)
    {
        if (btnElement.button == null || btnElement.buttonContainer == null) return;

        // Register on the CONTAINER to match the CSS selector .content-button-container:hover
        btnElement.buttonContainer.RegisterCallback<MouseEnterEvent>(evt => 
        {
            if (btnElement.leftDecoration != null)
            {
                btnElement.leftDecoration.style.opacity = 1;
            }
            if (btnElement.rightDecoration != null)
            {
                btnElement.rightDecoration.style.opacity = 1;
            }
        });

        btnElement.buttonContainer.RegisterCallback<MouseLeaveEvent>(evt => 
        {
            // Only hide if button is not focused
            if (!btnElement.button.ClassListContains("unity-button:focus"))
            {
                if (btnElement.leftDecoration != null)
                {
                    btnElement.leftDecoration.style.opacity = 0;
                }
                if (btnElement.rightDecoration != null)
                {
                    btnElement.rightDecoration.style.opacity = 0;
                }
            }
        });
    }

    private void RegisterDecorationFocus(ContentButtonElements btnElement)
    {
        if (btnElement.button == null) return;

        // Show decorations when button receives focus (keyboard/gamepad navigation)
        btnElement.button.RegisterCallback<FocusInEvent>(evt => 
        {
            if (btnElement.leftDecoration != null)
            {
                btnElement.leftDecoration.style.opacity = 1;
            }
            if (btnElement.rightDecoration != null)
            {
                btnElement.rightDecoration.style.opacity = 1;
            }
        });

        // Hide decorations when button loses focus
        btnElement.button.RegisterCallback<FocusOutEvent>(evt => 
        {
            if (btnElement.leftDecoration != null)
            {
                btnElement.leftDecoration.style.opacity = 0;
            }
            if (btnElement.rightDecoration != null)
            {
                btnElement.rightDecoration.style.opacity = 0;
            }
        });
    }

    private void OnValidate()
    {
        if (Application.isPlaying && _root != null)
        {
            CreateContentButtons();
            SetupContentButtonCallbacks();
            ApplyContentVisuals();
        }
    }

    [System.Serializable]
    public class ContentButtonData
    {
        [Tooltip("Text to display on the button")]
        public string buttonText = "BUTTON";
        
        [Tooltip("The UI Document GameObject to open when this button is clicked")]
        public UIDocument targetUIDocument;
        
        [Tooltip("The scene to load when this button is clicked (takes priority over UI Document)")]
        public SceneField targetScene;
        
        [Tooltip("Hide this options menu when opening the target")]
        public bool hideCurrentMenu = true;
    }

    private class ContentButtonElements
    {
        public Button button;
        public VisualElement buttonContainer;
        public VisualElement leftDecoration;
        public VisualElement rightDecoration;
        public ContentButtonData data;
    }
}