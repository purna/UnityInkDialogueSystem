using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class QuitSettingsMenuController : BaseMenuController
{
    [Header("Quit Confirmation Settings")]
    [SerializeField] private string _confirmationMessage = "Are you sure you want to quit?";
    [SerializeField] private float _messageTextSize = 20f;
    [SerializeField] private Color _messageTextColor = Color.white;

    [Header("Quit Behavior")]
    [SerializeField] private bool _loadQuitScene = false;
    [SerializeField] private SceneField _quitScene;
    [SerializeField] private float _quitDelay = 0.5f;

    [Header("Button Settings")]
    [SerializeField] private string _yesButtonText = "YES";
    [SerializeField] private string _noButtonText = "NO";

    [Header("Button Decoration Images")]
    [SerializeField] private Sprite _contentLeftDecoration;
    [SerializeField] private Sprite _contentRightDecoration;

    [Header("Content Button Colors")]
    [SerializeField] private Color _yesBtnNormal = new Color(0.8f, 0.2f, 0.2f);  // Red for Yes
    [SerializeField] private Color _yesBtnHover = new Color(0.9f, 0.3f, 0.3f);
    [SerializeField] private Color _noBtnNormal = new Color(0.145f, 0.388f, 0.922f);  // Blue for No
    [SerializeField] private Color _noBtnHover = new Color(0.231f, 0.510f, 0.965f);

    private ContentButtonElements _yesButtonElement;
    private ContentButtonElements _noButtonElement;
    private Label _messageLabel;

    protected override void OnEnableCustom()
    {
        CreateConfirmationUI();
        SetupButtonCallbacks();
        ApplyVisuals();
    }

    private void CreateConfirmationUI()
    {
        if (_contentContainer == null) return;

        _contentContainer.Clear();

        // Create message label
        _messageLabel = new Label(_confirmationMessage);
        _messageLabel.AddToClassList("quit-message");
        _messageLabel.style.fontSize = _messageTextSize;
        _messageLabel.style.color = _messageTextColor;
        _messageLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        _messageLabel.style.whiteSpace = WhiteSpace.Normal;
        _messageLabel.style.marginBottom = 40;
        _messageLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        _contentContainer.Add(_messageLabel);

        // Create button container to hold both buttons
        var buttonsContainer = new VisualElement();
        buttonsContainer.style.flexDirection = FlexDirection.Column;
        buttonsContainer.style.justifyContent = Justify.Center;
        buttonsContainer.style.alignItems = Align.Center;
        buttonsContainer.style.width = Length.Percent(100);

        // Create YES button (with decorations)
        _yesButtonElement = CreateConfirmButton(_yesButtonText, true);
        buttonsContainer.Add(_yesButtonElement.buttonContainer);

        // Add spacing between buttons
        var spacer = new VisualElement();
        spacer.style.height = 20;
        buttonsContainer.Add(spacer);

        // Create NO button (with decorations)
        _noButtonElement = CreateConfirmButton(_noButtonText, false);
        buttonsContainer.Add(_noButtonElement.buttonContainer);

        _contentContainer.Add(buttonsContainer);
    }

    private ContentButtonElements CreateConfirmButton(string text, bool isYesButton)
    {
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
        button.text = text;
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

        return new ContentButtonElements
        {
            button = button,
            buttonContainer = buttonContainer,
            leftDecoration = leftDeco,
            rightDecoration = rightDeco
        };
    }

    private void SetupButtonCallbacks()
    {
        // YES button
        if (_yesButtonElement?.button != null)
        {
            _yesButtonElement.button.clicked += OnYesClicked;
            RegisterButtonHover(_yesButtonElement.button, _yesBtnNormal, _yesBtnHover);
            RegisterDecorationHover(_yesButtonElement);
            RegisterDecorationFocus(_yesButtonElement);
        }

        // NO button
        if (_noButtonElement?.button != null)
        {
            _noButtonElement.button.clicked += OnNoClicked;
            RegisterButtonHover(_noButtonElement.button, _noBtnNormal, _noBtnHover);
            RegisterDecorationHover(_noButtonElement);
            RegisterDecorationFocus(_noButtonElement);
        }
    }

    private void ApplyVisuals()
    {
        // YES button visuals
        if (_yesButtonElement?.button != null)
        {
            _yesButtonElement.button.style.backgroundColor = _yesBtnNormal;
        }
        SetBg(_yesButtonElement?.leftDecoration, _contentLeftDecoration);
        SetBg(_yesButtonElement?.rightDecoration, _contentRightDecoration);

        // NO button visuals
        if (_noButtonElement?.button != null)
        {
            _noButtonElement.button.style.backgroundColor = _noBtnNormal;
        }
        SetBg(_noButtonElement?.leftDecoration, _contentLeftDecoration);
        SetBg(_noButtonElement?.rightDecoration, _contentRightDecoration);
    }

    private void OnYesClicked()
    {
        PlayClickSound();
        Debug.Log("Quit Confirmed - Exiting application");
        StartCoroutine(QuitSequence());
    }

    private void OnNoClicked()
    {
        PlayClickSound();
        Debug.Log("Quit Cancelled - Returning to previous menu");
        StartCoroutine(BackSequence());
    }

    private IEnumerator QuitSequence()
    {
        // Play close animation if enabled
        if (_enableAnimations)
        {
            bool animationComplete = false;
            PlayCloseAnimation(() => animationComplete = true);
            yield return new WaitUntil(() => animationComplete);
        }
        else
        {
            if (_root != null) _root.style.display = DisplayStyle.None;
        }

        // Wait for quit delay
        yield return new WaitForSecondsRealtime(_quitDelay);

        // Load quit scene or quit application
        if (_loadQuitScene && !string.IsNullOrEmpty(_quitScene.SceneName))
        {
            Debug.Log($"Loading quit scene: {_quitScene.SceneName}");
            SceneManager.LoadScene(_quitScene.SceneName);
        }
        else
        {
            Debug.Log("Quitting application...");
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }

    protected override void OnResetClicked()
    {
        PlayClickSound();
        // Reset doesn't do anything special for quit menu
        Debug.Log("Reset clicked on Quit Menu");
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

    // Public method to update the confirmation message at runtime
    public void SetConfirmationMessage(string message)
    {
        _confirmationMessage = message;
        if (_messageLabel != null)
        {
            _messageLabel.text = message;
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying && _root != null)
        {
            CreateConfirmationUI();
            SetupButtonCallbacks();
            ApplyVisuals();
        }
    }

    private class ContentButtonElements
    {
        public Button button;
        public VisualElement buttonContainer;
        public VisualElement leftDecoration;
        public VisualElement rightDecoration;
    }
}