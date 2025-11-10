using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using NodeDirection = UnityEditor.Experimental.GraphView.Direction;

public abstract class SkillsTreeBaseNode : Node
{
    private string _id;
    protected string _skillstreeName;
    protected List<SkillsTreeChoiceSaveData> _choices;
    private string _text;
    
    // Visual elements for displaying skill data
    private Image _iconImage;
    private Label _descriptionLabel;
    private VisualElement _iconContainer;
    
    // Reference to the Skill ScriptableObject (set externally)
    private Skill _skill;
    
    // Skill properties (stored locally for saving)
    private Sprite _icon;
    private Sprite _lockedIcon;
    private Sprite _unlockedIcon;
    private string _description;
    private int _tier;
    private int _unlockCost;
    private float _value;
    private int _maxLevel;

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            UpdateDescriptionDisplay();
        }
    }
    
    public Skill Skill
    {
        get => _skill;
        set
        {
            _skill = value;
            if (_skill != null)
            {
                _icon = _skill.Icon;
                _lockedIcon = _skill.LockedIcon;
                _unlockedIcon = _skill.UnlockedIcon;
                _description = _skill.Description;
                _tier = _skill.Tier;
                _unlockCost = _skill.UnlockCost;
                _value = _skill.Value;
                _maxLevel = _skill.MaxLevel;
                _text = _skill.Description; // Keep text in sync
            }
            UpdateVisualDisplay();
        }
    }
    

    protected abstract SkillsTreeType _type { get; }

    private Color _defaultBackgroundColor;
    protected SkillsTreeSystemGraphView _graphView;
    protected SkillsTreeSystemGroup _group;

    public string SkillsTreeName => _skillstreeName;
    public SkillsTreeSystemGroup Group => _group;
    public string ID => _id;
    public IEnumerable<SkillsTreeChoiceSaveData> Choices => _choices;
    public SkillsTreeType SkillsTreeType => _type;
    
    // Expose skill properties for saving
    public Sprite Icon => _icon;
    public Sprite LockedIcon => _lockedIcon;
    public Sprite UnlockedIcon => _unlockedIcon;
    public string Description => _description;
    public int Tier => _tier;
    public int UnlockCost => _unlockCost;
    public float Value => _value;
    public int MaxLevel => _maxLevel;

    public virtual void Initialize(string nodeName, SkillsTreeSystemGraphView graphView, Vector2 position)
    {
        _id = Guid.NewGuid().ToString();
        _skillstreeName = nodeName;
        _choices = new();
        _text = "SkillsTree text.";
        _description = "";
        _defaultBackgroundColor = new(29f / 255f, 29f / 255f, 30f / 255f);
        _graphView = graphView;
        
        // Initialize default skill properties
        _tier = 0;
        _unlockCost = 1;
        _value = 0f;
        _maxLevel = 1;

        SetPosition(new(position, Vector2.zero));

        mainContainer.AddToClassList("ds-node__main-container");
        extensionContainer.AddToClassList("ds-node__extension-container");
    }

    #region Overrided Methods
    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
        evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());
        base.BuildContextualMenu(evt);
    }
    #endregion

    public virtual void Draw()
    {
        // Title field
        TextField skillstreeNameField = UIElementUtility.CreateTextField(_skillstreeName, onValueChanged: callback =>
        {
            TextField target = callback.target as TextField;
            target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

            if (string.IsNullOrEmpty(target.value))
                target.value = _skillstreeName;

            if (_group == null)
            {
                _graphView.RemoveUngroupedNode(this);
                _skillstreeName = target.value;
                _graphView.AddUngroupedNode(this);
                return;
            }

            SkillsTreeSystemGroup currentGroup = _group;
            _graphView.RemoveGroupedNode(this, _group);
            _skillstreeName = target.value;
            _graphView.AddGroupedNode(this, currentGroup);
        });
        skillstreeNameField.AddClasses(
            "ds-node__text-field",
            "ds-node__text-field__hidden",
            "ds-node__filename-text-field"
        );
        titleContainer.Insert(0, skillstreeNameField);

        // Input port
        Port inputPort = this.CreatePort("SkillsTree Connection", direction: NodeDirection.Input, capacity: Port.Capacity.Multi);
        inputContainer.Add(inputPort);

        // Create visual display container in the main node area
        VisualElement visualContainer = new();
        visualContainer.name = "skill-visual-container";
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
        _iconContainer.name = "skill-icon-container";
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
        _iconImage.name = "skill-icon";
        _iconImage.scaleMode = ScaleMode.ScaleToFit;
        _iconImage.style.width = Length.Percent(100);
        _iconImage.style.height = Length.Percent(100);
        _iconContainer.Add(_iconImage);
        visualContainer.Add(_iconContainer);
        
        // Description container
        VisualElement descriptionContainer = new();
        descriptionContainer.style.flexGrow = 1;
        descriptionContainer.style.flexDirection = FlexDirection.Column;
        descriptionContainer.style.justifyContent = Justify.Center;
        
        // Description label
        _descriptionLabel = new Label();
        _descriptionLabel.name = "skill-description";
        _descriptionLabel.text = _text;
        _descriptionLabel.style.whiteSpace = WhiteSpace.Normal;
        _descriptionLabel.style.fontSize = 11;
        _descriptionLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
        _descriptionLabel.style.maxWidth = 200;
        descriptionContainer.Add(_descriptionLabel);
        
        visualContainer.Add(descriptionContainer);
        
        // Add visual container to main container
        mainContainer.Add(visualContainer);

        // Output ports for choices
        foreach (var choice in _choices)
        {
            Port choicePort = CreateChoicePort(choice);
            choicePort.userData = choice;
            outputContainer.Add(choicePort);
        }

        RefreshExpandedState();
        
        // Initial visual update
        UpdateVisualDisplay();
    }

    public void ChangeGroup(SkillsTreeSystemGroup group)
    {
        _group = group;
    }

    #region Visual Updates
    /// <summary>
    /// Updates the visual display with current skill data
    /// </summary>
    public void UpdateVisualDisplay()
    {
        UpdateIconDisplay(_icon);
        UpdateDescriptionDisplay();
    }
    
    /// <summary>
    /// Updates the icon display
    /// </summary>
    private void UpdateIconDisplay(Sprite icon)
    {
        if (_iconImage == null) return;
        
        if (icon != null)
        {
            _iconImage.sprite = icon;
            _iconImage.style.display = DisplayStyle.Flex;
        }
        else
        {
            _iconImage.sprite = null;
            _iconImage.style.display = DisplayStyle.None;
        }
    }
    
    /// <summary>
    /// Updates the description label
    /// </summary>
    private void UpdateDescriptionDisplay()
    {
        if (_descriptionLabel == null) return;
        
        string displayText = !string.IsNullOrEmpty(_description) ? _description : _text;
        
        // Truncate if too long
        if (displayText.Length > 100)
        {
            displayText = displayText.Substring(0, 97) + "...";
        }
        
        _descriptionLabel.text = displayText;
    }
    
    /// <summary>
    /// Public method to update icon from external sources
    /// </summary>
    public void UpdateIcon(Sprite icon)
    {
        _icon = icon;
        UpdateIconDisplay(icon);
        NotifyDataChanged();
    }
    
    /// <summary>
    /// Public method to update description from external sources
    /// </summary>
    public void UpdateDescription(string description)
    {
        _text = description;
        _description = description;
        UpdateDescriptionDisplay();
        NotifyDataChanged();
    }
    
    /// <summary>
    /// Update skill properties
    /// </summary>
    public void UpdateSkillProperties(int tier, int unlockCost, float value, int maxLevel)
    {
        _tier = tier;
        _unlockCost = unlockCost;
        _value = value;
        _maxLevel = maxLevel;
        NotifyDataChanged();
    }
    
    /// <summary>
    /// Notify the graph that this node's data has changed
    /// </summary>
    private void NotifyDataChanged()
    {
        // The graph view should listen for this and trigger a save
        _graphView?.OnNodeDataChanged(this);
    }
    #endregion

    #region Save/Load
    public void Setup(SkillsTreeNodeSaveData data, List<SkillsTreeChoiceSaveData> choices)
    {
        _id = data.ID;
        _choices = choices;
        _text = data.Text;
        
        // Load skill-specific data
        _icon = data.Icon;
        _lockedIcon = data.LockedIcon;
        _unlockedIcon = data.UnlockedIcon;
        _description = data.Description;
        _tier = data.Tier;
        _unlockCost = data.UnlockCost;
        _value = data.Value;
        _maxLevel = data.MaxLevel;
        
        // Update visual display with loaded data
        UpdateVisualDisplay();
    }
    
    /// <summary>
    /// Get save data from this node
    /// </summary>
    public SkillsTreeNodeSaveData GetSaveData()
    {
        List<SkillsTreeChoiceSaveData> choicesCopy = _choices.Select(c => c.Copy()).ToList();
        
        SkillsTreeNodeSaveData saveData = new SkillsTreeNodeSaveData(
            _id,
            _skillstreeName,
            _text,
            choicesCopy,
            _group?.ID ?? "",
            _type,
            GetPosition().position
        );
        
        // Update with current skill properties
        saveData.UpdateVisualData(_icon, _lockedIcon, _unlockedIcon, _description);
        saveData.UpdateSkillProperties(_tier, _unlockCost, _value, _maxLevel);
        
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

    public SkillsTreeChoiceSaveData GetChoice(int i)
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