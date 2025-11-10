using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using NodeDirection = UnityEditor.Experimental.GraphView.Direction;

public abstract class LevelBaseNode : Node
{
    private string _id;
    protected string _levelName;
    protected List<LevelChoiceSaveData> _choices;
    private string _text;
    
    // Visual elements for displaying Level data
    private Image _iconImage;
    private Label _descriptionLabel;
    private VisualElement _iconContainer;
    private Label _statusLabel;
    
    // Reference to the Level ScriptableObject
    private Level _level;
    
    // Level properties (cached from ScriptableObject)
    private Sprite _icon;
    private Sprite _lockedIcon;
    private Sprite _unlockedIcon;
    private Sprite _completedIcon;
    private string _description;
    private int _tier;
    private int _levelIndex;
    private float _completionThreshold;
    private int _maxAttempts;

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            UpdateDescriptionDisplay();
        }
    }
    
    public Level Level
    {
        get => _level;
        set
        {
            _level = value;
            if (_level != null)
            {
                SyncFromLevel();
            }
            UpdateVisualDisplay();
        }
    }
    
    protected abstract LevelType _type { get; }

    private Color _defaultBackgroundColor;
    protected LevelSystemGraphView _graphView;
    protected LevelSystemGroup _group;

    public string LevelName => _levelName;
    public LevelSystemGroup Group => _group;
    public string ID => _id;
    public IEnumerable<LevelChoiceSaveData> Choices => _choices;
    public LevelType LevelType => _type;
    
    // Expose Level properties for saving
    public Sprite Icon => _icon;
    public Sprite LockedIcon => _lockedIcon;
    public Sprite UnlockedIcon => _unlockedIcon;
    public Sprite CompletedIcon => _completedIcon;
    public string Description => _description;
    public int Tier => _tier;
    public int LevelIndex => _levelIndex;
    public float CompletionThreshold => _completionThreshold;
    public int MaxAttempts => _maxAttempts;

    public virtual void Initialize(string nodeName, LevelSystemGraphView graphView, Vector2 position)
    {
        _id = Guid.NewGuid().ToString();
        _levelName = nodeName;
        _choices = new();
        _text = "Level description.";
        _description = "";
        _defaultBackgroundColor = new(29f / 255f, 29f / 255f, 30f / 255f);
        _graphView = graphView;
        
        // Initialize default Level properties
        _tier = 0;
        _levelIndex = 0;
        _completionThreshold = 100f;
        _maxAttempts = -1; // Unlimited

        SetPosition(new(position, Vector2.zero));

        mainContainer.AddToClassList("ds-node__main-container");
        extensionContainer.AddToClassList("ds-node__extension-container");
    }

    #region Overrided Methods
    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
        evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());
        
        if (_level != null)
        {
            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Sync from Level Asset", actionEvent => SyncFromLevel());
            evt.menu.AppendAction("Apply to Level Asset", actionEvent => ApplyToLevel());
        }
        
        base.BuildContextualMenu(evt);
    }
    #endregion

    public virtual void Draw()
    {
        // Title field
        TextField levelNameField = UIElementUtility.CreateTextField(_levelName, onValueChanged: callback =>
        {
            TextField target = callback.target as TextField;
            target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

            if (string.IsNullOrEmpty(target.value))
                target.value = _levelName;

            if (_group == null)
            {
                _graphView.RemoveUngroupedNode(this);
                _levelName = target.value;
                _graphView.AddUngroupedNode(this);
                
                // Update Level asset if exists
                if (_level != null)
                {
                    _level.UpdateName(target.value);
                }
                return;
            }

            LevelSystemGroup currentGroup = _group;
            _graphView.RemoveGroupedNode(this, _group);
            _levelName = target.value;
            _graphView.AddGroupedNode(this, currentGroup);
            
            // Update Level asset if exists
            if (_level != null)
            {
                _level.UpdateName(target.value);
            }
        });
        levelNameField.AddClasses(
            "ds-node__text-field",
            "ds-node__text-field__hidden",
            "ds-node__filename-text-field"
        );
        titleContainer.Insert(0, levelNameField);

        // Input port (prerequisite levels)
        Port inputPort = this.CreatePort("Prerequisites", direction: NodeDirection.Input, capacity: Port.Capacity.Multi);
        inputContainer.Add(inputPort);

        // Create visual display container
        VisualElement visualContainer = new();
        visualContainer.name = "level-visual-container";
        visualContainer.style.flexDirection = FlexDirection.Row;
        visualContainer.style.paddingTop = 5;
        visualContainer.style.paddingBottom = 5;
        visualContainer.style.paddingLeft = 10;
        visualContainer.style.paddingRight = 10;
        visualContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        visualContainer.style.borderTopWidth = 1;
        visualContainer.style.borderTopColor = new Color(0.1f, 0.1f, 0.1f);
        visualContainer.style.borderBottomWidth = 1;
        visualContainer.style.borderBottomColor = new Color(0.1f, 0.1f, 0.1f);
        
        // Icon container
        _iconContainer = new VisualElement();
        _iconContainer.name = "level-icon-container";
        _iconContainer.style.width = 48;
        _iconContainer.style.height = 48;
        _iconContainer.style.marginRight = 10;
        _iconContainer.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
        _iconContainer.style.borderTopLeftRadius = 4;
        _iconContainer.style.borderTopRightRadius = 4;
        _iconContainer.style.borderBottomLeftRadius = 4;
        _iconContainer.style.borderBottomRightRadius = 4;
        _iconContainer.style.borderTopWidth = 1;
        _iconContainer.style.borderBottomWidth = 1;
        _iconContainer.style.borderLeftWidth = 1;
        _iconContainer.style.borderRightWidth = 1;
        _iconContainer.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f);
        _iconContainer.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f);
        _iconContainer.style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f);
        _iconContainer.style.borderRightColor = new Color(0.3f, 0.3f, 0.3f);
        
        // Icon image
        _iconImage = new Image();
        _iconImage.name = "level-icon";
        _iconImage.scaleMode = ScaleMode.ScaleToFit;
        _iconImage.style.width = Length.Percent(100);
        _iconImage.style.height = Length.Percent(100);
        _iconContainer.Add(_iconImage);
        visualContainer.Add(_iconContainer);
        
        // Info container (description + status)
        VisualElement infoContainer = new();
        infoContainer.style.flexGrow = 1;
        infoContainer.style.flexDirection = FlexDirection.Column;
        infoContainer.style.justifyContent = Justify.Center;
        
        // Description label
        _descriptionLabel = new Label();
        _descriptionLabel.name = "level-description";
        _descriptionLabel.text = _text;
        _descriptionLabel.style.whiteSpace = WhiteSpace.Normal;
        _descriptionLabel.style.fontSize = 11;
        _descriptionLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
        _descriptionLabel.style.maxWidth = 200;
        _descriptionLabel.style.marginBottom = 3;
        infoContainer.Add(_descriptionLabel);
        
        // Status label
        _statusLabel = new Label();
        _statusLabel.name = "level-status";
        _statusLabel.text = GetStatusText();
        _statusLabel.style.fontSize = 9;
        _statusLabel.style.color = new Color(0.6f, 0.6f, 0.6f);
        infoContainer.Add(_statusLabel);
        
        visualContainer.Add(infoContainer);
        mainContainer.Add(visualContainer);

        // Output ports for next levels
        foreach (var choice in _choices)
        {
            Port choicePort = CreateChoicePort(choice);
            choicePort.userData = choice;
            outputContainer.Add(choicePort);
        }

        RefreshExpandedState();
        UpdateVisualDisplay();
    }

    public void ChangeGroup(LevelSystemGroup group)
    {
        _group = group;
    }

    #region Level Synchronization
    
    /// <summary>
    /// Sync node data FROM the Level ScriptableObject
    /// </summary>
    private void SyncFromLevel()
    {
        if (_level == null) return;
        
        _levelName = _level.LevelName;
        _icon = _level.Icon;
        _lockedIcon = _level.LockedIcon;
        _unlockedIcon = _level.UnlockedIcon;
        _completedIcon = _level.CompletedIcon;
        _description = _level.Description;
        _tier = _level.Tier;
        _levelIndex = _level.LevelIndex;
        _completionThreshold = _level.CompletionThreshold;
        _maxAttempts = _level.MaxAttempts;
        _text = _level.Description;
        
        UpdateVisualDisplay();
        Debug.Log($"[LevelBaseNode] Synced from Level asset: {_levelName}");
    }
    
    /// <summary>
    /// Apply node data TO the Level ScriptableObject
    /// </summary>
    private void ApplyToLevel()
    {
        if (_level == null) return;
        
        _level.UpdateName(_levelName);
        _level.UpdateDescription(_description);
        _level.UpdateIcons(_icon, _lockedIcon, _unlockedIcon, _completedIcon);
        _level.UpdateProperties(_tier, _levelIndex, _completionThreshold, _maxAttempts);
        
        UnityEditor.EditorUtility.SetDirty(_level);
        Debug.Log($"[LevelBaseNode] Applied to Level asset: {_levelName}");
    }
    
    #endregion

    #region Visual Updates
    
    public void UpdateVisualDisplay()
    {
        UpdateIconDisplay(_icon);
        UpdateDescriptionDisplay();
        UpdateStatusDisplay();
    }
    
    private void UpdateIconDisplay(Sprite icon)
    {
        if (_iconImage == null) return;
        
        Sprite displayIcon = icon;
        
        // Show appropriate icon based on runtime state if level is assigned
        if (_level != null)
        {
            if (_level.IsCompleted && _completedIcon != null)
                displayIcon = _completedIcon;
            else if (_level.IsUnlocked && _unlockedIcon != null)
                displayIcon = _unlockedIcon;
            else if (!_level.IsUnlocked && _lockedIcon != null)
                displayIcon = _lockedIcon;
        }
        
        if (displayIcon != null)
        {
            _iconImage.sprite = displayIcon;
            _iconImage.style.display = DisplayStyle.Flex;
        }
        else
        {
            _iconImage.sprite = null;
            _iconImage.style.display = DisplayStyle.None;
        }
        
        // Update icon container border color
        if (_iconContainer != null && _level != null)
        {
            Color borderColor;
            if (_level.IsCompleted)
                borderColor = new Color(1f, 0.84f, 0f); // Gold
            else if (_level.IsUnlocked)
                borderColor = new Color(0.2f, 0.8f, 0.2f); // Green
            else
                borderColor = new Color(0.3f, 0.3f, 0.3f); // Gray
            
            _iconContainer.style.borderTopColor = borderColor;
            _iconContainer.style.borderBottomColor = borderColor;
            _iconContainer.style.borderLeftColor = borderColor;
            _iconContainer.style.borderRightColor = borderColor;
        }
    }
    
    private void UpdateDescriptionDisplay()
    {
        if (_descriptionLabel == null) return;
        
        string displayText = !string.IsNullOrEmpty(_description) ? _description : _text;
        
        if (displayText.Length > 100)
        {
            displayText = displayText.Substring(0, 97) + "...";
        }
        
        _descriptionLabel.text = displayText;
    }
    
    private void UpdateStatusDisplay()
    {
        if (_statusLabel == null) return;
        _statusLabel.text = GetStatusText();
    }
    
    private string GetStatusText()
    {
        string status = $"Tier {_tier} | Level {_levelIndex}";
        
        if (_completionThreshold > 0)
        {
            status += $" | Goal: {_completionThreshold:F0}";
        }
        
        if (_maxAttempts > 0)
        {
            status += $" | Max: {_maxAttempts}";
        }
        else if (_maxAttempts < 0)
        {
            status += " | Unlimited";
        }
        
        // Add runtime status if level is assigned and playing
        if (_level != null && UnityEngine.Application.isPlaying)
        {
            if (_level.IsCompleted)
            {
                status += $" | ✓ Completed ({_level.TimesCompleted}x)";
                if (_level.BestScore > 0)
                {
                    status += $" | Best: {_level.BestScore:F0}";
                }
            }
            else if (_level.IsUnlocked)
            {
                if (_level.AttemptsUsed > 0)
                {
                    status += $" | Attempts: {_level.AttemptsUsed}";
                    if (_maxAttempts > 0)
                    {
                        status += $"/{_maxAttempts}";
                    }
                }
                if (_level.BestScore > 0)
                {
                    status += $" | Best: {_level.BestScore:F0}";
                }
            }
            else
            {
                status += " | 🔒 Locked";
            }
        }
        
        return status;
    }
    
    public void UpdateIcon(Sprite icon)
    {
        _icon = icon;
        UpdateIconDisplay(icon);
        NotifyDataChanged();
    }
    
    public void UpdateDescription(string description)
    {
        _text = description;
        _description = description;
        UpdateDescriptionDisplay();
        NotifyDataChanged();
    }
    
    public void UpdateLevelProperties(int tier, int levelIndex, float completionThreshold, int maxAttempts)
    {
        _tier = tier;
        _levelIndex = levelIndex;
        _completionThreshold = completionThreshold;
        _maxAttempts = maxAttempts;
        UpdateStatusDisplay();
        NotifyDataChanged();
    }
    
    private void NotifyDataChanged()
    {
        _graphView?.OnNodeDataChanged(this);
    }
    
    #endregion

    #region Save/Load
    
    public void Setup(LevelNodeSaveData data, List<LevelChoiceSaveData> choices)
    {
        _id = data.ID;
        _choices = choices;
        _text = data.Text;
        
        // Load Level-specific data
        _icon = data.Icon;
        _lockedIcon = data.LockedIcon;
        _unlockedIcon = data.UnlockedIcon;
        _completedIcon = data.CompletedIcon;
        _description = data.Description;
        _tier = data.Tier;
        _levelIndex = data.LevelIndex;
        _completionThreshold = data.CompletionThreshold;
        _maxAttempts = data.MaxAttempts;
        
        UpdateVisualDisplay();
    }
    
    public LevelNodeSaveData GetSaveData()
    {
        List<LevelChoiceSaveData> choicesCopy = _choices.Select(c => c.Copy()).ToList();
        
        LevelNodeSaveData saveData = new LevelNodeSaveData(
            _id,
            _levelName,
            _text,
            choicesCopy,
            _group?.ID ?? "",
            _type,
            GetPosition().position
        );
        
        saveData.UpdateVisualData(_icon, _lockedIcon, _unlockedIcon, _completedIcon, _description);
        saveData.UpdateLevelProperties(_tier, _levelIndex, _completionThreshold, _maxAttempts);
        
        return saveData;
    }
    
    #endregion

    #region Utility
    
    public void DisconnectAllPorts()
    {
        DisconnectInputPorts();
        DisconnectOutputPorts();
    }

    private void DisconnectInputPorts()
    {
        DisconnectPorts(inputContainer);
    }

    private void DisconnectOutputPorts()
    {
        DisconnectPorts(outputContainer);
    }

    public LevelChoiceSaveData GetChoice(int i)
    {
        return _choices[i];
    }

    private void DisconnectPorts(VisualElement container)
    {
        foreach (var element in container.Children())
            if (element is Port port)
                if (port.connected)
                    _graphView.DeleteElements(port.connections);
    }

    public void SetErrorStyle(Color color)
    {
        mainContainer.style.backgroundColor = color;
    }

    public void ResetStyle()
    {
        mainContainer.style.backgroundColor = _defaultBackgroundColor;
    }

    public bool IsStartingNode()
    {
        Port port = inputContainer.Children().First() as Port;
        return !port.connected;
    }
    
    #endregion

    protected abstract Port CreateChoicePort(object userData);
}