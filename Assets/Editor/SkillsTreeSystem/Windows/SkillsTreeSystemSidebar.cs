using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

/// <summary>
/// Sidebar for displaying and editing selected skill node properties
/// </summary>
public class SkillsTreeSystemSidebar
{
    private SkillsTreeBaseNode _selectedNode;
    private Skill _selectedSkill;
    private ScrollView _scrollView;
    private VisualElement _contentContainer;
    private VisualElement _rootElement;
    
    private const float SIDEBAR_WIDTH = 300f;
    
    // UI Elements for skill properties
    private TextField _skillNameField;
    private TextField _descriptionField;
    private ObjectField _iconField;
    private ObjectField _lockedIconField;   
    private ObjectField _unlockedIconField; 
    private IntegerField _tierField;
    private IntegerField _unlockCostField;
    private EnumField _skillTypeField;
    private FloatField _valueField;
    private IntegerField _maxLevelField;
    private Vector2Field _positionField;
    
    // Unlock Functions UI
    private VisualElement _unlockFunctionsContainer;
    private Label _unlockFunctionsCountLabel;
    
    public float Width => SIDEBAR_WIDTH;
    public bool IsVisible => _rootElement != null && _rootElement.style.display == DisplayStyle.Flex;
    
    public VisualElement CreateSidebar()
    {
        _rootElement = new VisualElement();
        _rootElement.name = "skillstree-sidebar";
        _rootElement.style.width = SIDEBAR_WIDTH;
        _rootElement.style.backgroundColor = new Color(0.22f, 0.22f, 0.22f);
        _rootElement.style.borderLeftWidth = 1;
        _rootElement.style.borderLeftColor = new Color(0.1f, 0.1f, 0.1f);
        _rootElement.style.display = DisplayStyle.None;
        
        // Header
        Label header = new Label("Skill Properties");
        header.style.fontSize = 16;
        header.style.unityFontStyleAndWeight = FontStyle.Bold;
        SetPadding(header, 10);
        header.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);
        header.style.borderBottomWidth = 1;
        header.style.borderBottomColor = new Color(0.1f, 0.1f, 0.1f);
        _rootElement.Add(header);
        
        // Scroll view for content
        _scrollView = new ScrollView(ScrollViewMode.Vertical);
        _scrollView.style.flexGrow = 1;
        _rootElement.Add(_scrollView);
        
        _contentContainer = new VisualElement();
        SetPadding(_contentContainer, 10);
        _scrollView.Add(_contentContainer);
        
        BuildPropertyFields();
        
        return _rootElement;
    }
    
    private void BuildPropertyFields()
    {
        _contentContainer.Clear();
        
        // Skill Identity Section
        AddSectionHeader("Identity");
        
        _skillNameField = new TextField("Skill Name");
        _skillNameField.tooltip = "The unique name of this skill. This will be displayed in the UI and used to identify the skill.";
        _skillNameField.RegisterValueChangedCallback(OnSkillNameChanged);
        _contentContainer.Add(_skillNameField);
        
        _descriptionField = new TextField("Description");
        _descriptionField.multiline = true;
        _descriptionField.style.minHeight = 60;
        _descriptionField.tooltip = "A detailed description of what this skill does. This will be shown to players when they view the skill.";
        _descriptionField.RegisterValueChangedCallback(OnDescriptionChanged);
        _contentContainer.Add(_descriptionField);
        
        _iconField = new ObjectField("Icon");
        _iconField.objectType = typeof(Sprite);
        _iconField.tooltip = "The visual icon for this skill. Should be a Sprite asset that represents the skill's theme or effect.";
        _iconField.RegisterValueChangedCallback(OnIconChanged);
        _contentContainer.Add(_iconField);

        _lockedIconField = new ObjectField("Locked Icon");
        _lockedIconField.objectType = typeof(Sprite);
        _lockedIconField.tooltip = "The icon displayed when this skill is locked/unavailable.";
        _lockedIconField.RegisterValueChangedCallback(OnLockedIconChanged);
        _contentContainer.Add(_lockedIconField);

        _unlockedIconField = new ObjectField("Unlocked Icon");
        _unlockedIconField.objectType = typeof(Sprite);
        _unlockedIconField.tooltip = "The icon displayed when this skill is unlocked/available.";
        _unlockedIconField.RegisterValueChangedCallback(OnUnlockedIconChanged);
        _contentContainer.Add(_unlockedIconField);
        
        AddSpace();
        
        // Skill Properties Section
        AddSectionHeader("Properties");
        
        _tierField = new IntegerField("Tier");
        _tierField.tooltip = "The tier or level of this skill in the skill tree. Higher tiers typically require more prerequisites.\nExample: Tier 1 = Basic, Tier 2 = Intermediate, Tier 3 = Advanced";
        _tierField.RegisterValueChangedCallback(OnTierChanged);
        _contentContainer.Add(_tierField);
        
        _unlockCostField = new IntegerField("Unlock Cost");
        _unlockCostField.tooltip = "The number of skill points required to unlock this skill. Must be at least 0.\nExample: 1 for basic skills, 3-5 for advanced skills";
        _unlockCostField.RegisterValueChangedCallback(OnUnlockCostChanged);
        _contentContainer.Add(_unlockCostField);
        
        _skillTypeField = new EnumField("Skill Type", SkillType.Passive);
        _skillTypeField.tooltip = "The type of skill:\n• Passive: Always active bonuses (e.g., +10% damage)\n• Active: Abilities that must be triggered\n• Attribute: Stat increases (e.g., +5 Strength)\n• Unlock: Unlocks new features or mechanics\n• Upgrade: Improves existing abilities";
        _skillTypeField.RegisterValueChangedCallback(OnSkillTypeChanged);
        _contentContainer.Add(_skillTypeField);
        
        AddSpace();
        
        // Skill Values Section
        AddSectionHeader("Values");
        
        _valueField = new FloatField("Value");
        _valueField.tooltip = "The base numerical value of this skill's effect. This is multiplied by the skill's current level.\nExample: Value of 10 at level 3 = 30 total effect";
        _valueField.RegisterValueChangedCallback(OnValueChanged);
        _contentContainer.Add(_valueField);
        
        _maxLevelField = new IntegerField("Max Level");
        _maxLevelField.tooltip = "The maximum level this skill can reach. Each level requires spending additional skill points.\nMust be at least 1. Most skills have 1-5 levels.";
        _maxLevelField.RegisterValueChangedCallback(OnMaxLevelChanged);
        _contentContainer.Add(_maxLevelField);

        AddSpace();

        // Unlock Functions Section
        AddSectionHeader("Unlock Functions");
        
        _unlockFunctionsCountLabel = new Label("Functions: 0");
        _unlockFunctionsCountLabel.style.paddingLeft = 5;
        _unlockFunctionsCountLabel.style.paddingBottom = 5;
        _unlockFunctionsCountLabel.style.fontSize = 11;
        _unlockFunctionsCountLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
        _contentContainer.Add(_unlockFunctionsCountLabel);
        
        _unlockFunctionsContainer = new VisualElement();
        _unlockFunctionsContainer.name = "unlock-functions-container";
        _unlockFunctionsContainer.style.maxWidth = SIDEBAR_WIDTH - 30; // Prevent content overflow
        SetPadding(_unlockFunctionsContainer, 5, 5, 5, 5);
        _unlockFunctionsContainer.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f, 0.5f);
        SetBorderRadius(_unlockFunctionsContainer, 4);
        SetBorderWidth(_unlockFunctionsContainer, 1);
        SetBorderColor(_unlockFunctionsContainer, new Color(0.1f, 0.1f, 0.1f));
        _contentContainer.Add(_unlockFunctionsContainer);
        
        // Add button for new functions
        Button addFunctionButton = new Button(() => OnAddUnlockFunction());
        addFunctionButton.text = "+ Add Function";
        addFunctionButton.style.marginTop = 5;
        addFunctionButton.style.backgroundColor = new Color(0.3f, 0.5f, 0.3f);
        _contentContainer.Add(addFunctionButton);
        
        AddSpace();
        
        // Position Section
        AddSectionHeader("Position");
        
        _positionField = new Vector2Field("Node Position");
        _positionField.tooltip = "The X and Y position of this skill node in the graph editor. You can also drag the node directly in the graph.";
        _positionField.RegisterValueChangedCallback(OnPositionChanged);
        _contentContainer.Add(_positionField);
        
        AddSpace();
        
        // Info Section
        AddSectionHeader("Connections");
        
        Label prerequisitesLabel = new Label("Prerequisites: 0");
        prerequisitesLabel.name = "prerequisites-label";
        SetPadding(prerequisitesLabel, 5, 5, 5, 5);
        prerequisitesLabel.tooltip = "The number of skills that must be unlocked before this skill becomes available.\nConnect input ports from other skills to create prerequisites.";
        _contentContainer.Add(prerequisitesLabel);
        
        Label childrenLabel = new Label("Children: 0");
        childrenLabel.name = "children-label";
        SetPadding(childrenLabel, 5, 5, 5, 5);
        childrenLabel.tooltip = "The number of skills that require this skill as a prerequisite.\nConnect output ports to other skills to create child relationships.";
        _contentContainer.Add(childrenLabel);
        
        AddSpace();
        
        // Add help text at the bottom
        AddHelpSection();
    }
    
    private void BuildUnlockFunctionsList()
    {
        _unlockFunctionsContainer.Clear();
        
        if (_selectedSkill == null)
            return;
        
        List<SkillFunction> functions = _selectedSkill.UnlockFunctions;
        
        if (functions == null || functions.Count == 0)
        {
            Label emptyLabel = new Label("No unlock functions assigned");
            emptyLabel.style.paddingLeft = 5;
            emptyLabel.style.paddingTop = 10;
            emptyLabel.style.paddingBottom = 10;
            emptyLabel.style.fontSize = 11;
            emptyLabel.style.color = new Color(0.6f, 0.6f, 0.6f);
            emptyLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            _unlockFunctionsContainer.Add(emptyLabel);
            return;
        }
        
        // Display each function
        for (int i = 0; i < functions.Count; i++)
        {
            int index = i; // Capture for closure
            SkillFunction function = functions[i];
            
            VisualElement functionRow = new VisualElement();
            functionRow.style.flexDirection = FlexDirection.Row;
            functionRow.style.paddingTop = 3;
            functionRow.style.paddingBottom = 3;
            functionRow.style.paddingRight = 5;
            
            ObjectField functionField = new ObjectField();
            functionField.objectType = typeof(SkillFunction);
            functionField.value = function;
            functionField.style.flexGrow = 1;
            functionField.RegisterValueChangedCallback(evt => OnUnlockFunctionChanged(index, evt.newValue as SkillFunction));
            functionRow.Add(functionField);
            
            Button removeButton = new Button(() => OnRemoveUnlockFunction(index));
            removeButton.text = "X";
            removeButton.style.width = 25;
            removeButton.style.backgroundColor = new Color(0.6f, 0.2f, 0.2f);
            removeButton.style.marginLeft = 5;
            functionRow.Add(removeButton);
            
            _unlockFunctionsContainer.Add(functionRow);
        }
    }
    
    private void OnAddUnlockFunction()
    {
        if (_selectedSkill == null) return;
        
        SerializedObject so = new SerializedObject(_selectedSkill);
        SerializedProperty functionsProperty = so.FindProperty("_unlockFunctions");
        
        functionsProperty.arraySize++;
        so.ApplyModifiedProperties();
        
        // Update node's unlock functions list
        if (_selectedNode != null)
        {
            _selectedNode.UpdateUnlockFunctions(_selectedSkill.UnlockFunctions);
        }
        
        EditorUtility.SetDirty(_selectedSkill);
        RefreshUnlockFunctionsList();
    }
    
    private void OnRemoveUnlockFunction(int index)
    {
        if (_selectedSkill == null) return;
        
        SerializedObject so = new SerializedObject(_selectedSkill);
        SerializedProperty functionsProperty = so.FindProperty("_unlockFunctions");
        
        if (index >= 0 && index < functionsProperty.arraySize)
        {
            functionsProperty.DeleteArrayElementAtIndex(index);
            so.ApplyModifiedProperties();
            
            // Update node's unlock functions list
            if (_selectedNode != null)
            {
                _selectedNode.UpdateUnlockFunctions(_selectedSkill.UnlockFunctions);
            }
            
            EditorUtility.SetDirty(_selectedSkill);
            RefreshUnlockFunctionsList();
        }
    }
    
    private void OnUnlockFunctionChanged(int index, SkillFunction newFunction)
    {
        if (_selectedSkill == null) return;
        
        SerializedObject so = new SerializedObject(_selectedSkill);
        SerializedProperty functionsProperty = so.FindProperty("_unlockFunctions");
        
        if (index >= 0 && index < functionsProperty.arraySize)
        {
            functionsProperty.GetArrayElementAtIndex(index).objectReferenceValue = newFunction;
            so.ApplyModifiedProperties();
            
            // Update node's unlock functions list
            if (_selectedNode != null)
            {
                _selectedNode.UpdateUnlockFunctions(_selectedSkill.UnlockFunctions);
            }
            
            EditorUtility.SetDirty(_selectedSkill);
            RefreshUnlockFunctionsList();
        }
    }
    
    private void RefreshUnlockFunctionsList()
    {
        if (_selectedSkill == null) return;
        
        int count = _selectedSkill.UnlockFunctions?.Count ?? 0;
        _unlockFunctionsCountLabel.text = $"Functions: {count}";
        
        BuildUnlockFunctionsList();
    }
    
    private void AddHelpSection()
    {
        AddSectionHeader("Quick Tips");
        
        Label helpText = new Label(
            "• Hover over any field to see its tooltip\n" +
            "• Drag nodes in the graph to reposition\n" +
            "• Connect output ports to create skill chains\n" +
            "• Add multiple unlock functions per skill\n" +
            "• Use Ctrl+S to save your changes\n" +
            "• Right-click nodes for more options"
        );
        SetPadding(helpText, 5, 5, 5, 5);
        helpText.style.fontSize = 11;
        helpText.style.color = new Color(0.7f, 0.7f, 0.7f);
        helpText.style.whiteSpace = WhiteSpace.Normal;
        _contentContainer.Add(helpText);
    }
    
    private void AddSectionHeader(string title)
    {
        Label header = new Label(title);
        header.style.fontSize = 14;
        header.style.unityFontStyleAndWeight = FontStyle.Bold;
        header.style.paddingTop = 5;
        header.style.paddingBottom = 5;
        header.style.color = new Color(0.8f, 0.8f, 0.8f);
        _contentContainer.Add(header);
    }
    
    private void AddSpace()
    {
        VisualElement space = new VisualElement();
        space.style.height = 10;
        _contentContainer.Add(space);
    }
    
    public void ShowNode(SkillsTreeBaseNode node, Skill skill)
    {
        _selectedNode = node;
        _selectedSkill = skill;
        
        if (_rootElement != null)
        {
            _rootElement.style.display = DisplayStyle.Flex;
            UpdateFields();
        }
    }
    
    public void Hide()
    {
        _selectedNode = null;
        _selectedSkill = null;
        
        if (_rootElement != null)
        {
            _rootElement.style.display = DisplayStyle.None;
        }
    }
    
    private void UpdateFields()
    {
        if (_selectedNode == null || _selectedSkill == null)
            return;
        
        // Update all fields with current values
        _skillNameField.SetValueWithoutNotify(_selectedSkill.SkillName);
        _descriptionField.SetValueWithoutNotify(_selectedSkill.Description);
        _iconField.SetValueWithoutNotify(_selectedSkill.Icon);
        _lockedIconField.SetValueWithoutNotify(_selectedSkill.LockedIcon);    
        _unlockedIconField.SetValueWithoutNotify(_selectedSkill.UnlockedIcon); 
        _tierField.SetValueWithoutNotify(_selectedSkill.Tier);
        _unlockCostField.SetValueWithoutNotify(_selectedSkill.UnlockCost);
        _skillTypeField.SetValueWithoutNotify(_selectedSkill.SkillType);
        _valueField.SetValueWithoutNotify(_selectedSkill.Value);
        _maxLevelField.SetValueWithoutNotify(_selectedSkill.MaxLevel);
        _positionField.SetValueWithoutNotify(_selectedNode.GetPosition().position);
        
        // Update connection counts
        Label prerequisitesLabel = _contentContainer.Q<Label>("prerequisites-label");
        if (prerequisitesLabel != null)
            prerequisitesLabel.text = $"Prerequisites: {_selectedSkill.Prerequisites.Count}";
        
        Label childrenLabel = _contentContainer.Q<Label>("children-label");
        if (childrenLabel != null)
            childrenLabel.text = $"Children: {_selectedSkill.Children.Count}";

        // Update unlock functions
        RefreshUnlockFunctionsList();
    }
    
    /// <summary>
    /// Refreshes the sidebar if a skill is currently selected (for external changes)
    /// </summary>
    public void RefreshIfSelected(Skill skill)
    {
        if (_selectedSkill == skill && _rootElement.style.display == DisplayStyle.Flex)
        {
            UpdateFields();
            
            // Also update the node's visual display
            if (_selectedNode != null)
            {
                _selectedNode.UpdateVisualDisplay();
            }
        }
    }
    
    // Value change callbacks with validation
    private void OnSkillNameChanged(ChangeEvent<string> evt)
    {
        if (_selectedSkill == null) return;
        
        string newValue = evt.newValue.Trim();
        if (string.IsNullOrEmpty(newValue))
        {
            Debug.LogWarning("Skill name cannot be empty!");
            _skillNameField.SetValueWithoutNotify(_selectedSkill.SkillName);
            return;
        }
        
        SerializedObject so = new SerializedObject(_selectedSkill);
        so.FindProperty("_skillName").stringValue = newValue;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(_selectedSkill);
    }
    
    private void OnDescriptionChanged(ChangeEvent<string> evt)
    {
        if (_selectedSkill == null || _selectedNode == null) return;
        
        SerializedObject so = new SerializedObject(_selectedSkill);
        so.FindProperty("_description").stringValue = evt.newValue;
        so.ApplyModifiedProperties();
        
        // Update the node's text and visual display
        _selectedNode.Text = evt.newValue;
        _selectedNode.UpdateDescription(evt.newValue);
        
        EditorUtility.SetDirty(_selectedSkill);
    }

    private void OnIconChanged(ChangeEvent<Object> evt)
    {
        if (_selectedSkill == null || _selectedNode == null) return;

        SerializedObject so = new SerializedObject(_selectedSkill);
        so.FindProperty("_icon").objectReferenceValue = evt.newValue;
        so.ApplyModifiedProperties();

        // Update the node's icon display
        Sprite newIcon = evt.newValue as Sprite;
        _selectedNode.UpdateIcon(newIcon);

        EditorUtility.SetDirty(_selectedSkill);
    }
    
    private void OnLockedIconChanged(ChangeEvent<Object> evt)
    {
        if (_selectedSkill == null) return;
        
        SerializedObject so = new SerializedObject(_selectedSkill);
        so.FindProperty("_lockedIcon").objectReferenceValue = evt.newValue;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(_selectedSkill);
    }

    private void OnUnlockedIconChanged(ChangeEvent<Object> evt)
    {
        if (_selectedSkill == null) return;
        
        SerializedObject so = new SerializedObject(_selectedSkill);
        so.FindProperty("_unlockedIcon").objectReferenceValue = evt.newValue;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(_selectedSkill);
    }
    
    private void OnTierChanged(ChangeEvent<int> evt)
    {
        if (_selectedSkill == null) return;
        
        int newValue = Mathf.Max(0, evt.newValue);
        
        SerializedObject so = new SerializedObject(_selectedSkill);
        so.FindProperty("_tier").intValue = newValue;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(_selectedSkill);
        
        if (newValue != evt.newValue)
        {
            _tierField.SetValueWithoutNotify(newValue);
        }
    }
    
    private void OnUnlockCostChanged(ChangeEvent<int> evt)
    {
        if (_selectedSkill == null) return;
        
        int newValue = Mathf.Max(0, evt.newValue);
        
        SerializedObject so = new SerializedObject(_selectedSkill);
        so.FindProperty("_unlockCost").intValue = newValue;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(_selectedSkill);
        
        if (newValue != evt.newValue)
        {
            _unlockCostField.SetValueWithoutNotify(newValue);
            Debug.Log("Unlock cost must be at least 0. Value clamped.");
        }
    }
    
    private void OnSkillTypeChanged(ChangeEvent<System.Enum> evt)
    {
        if (_selectedSkill == null) return;
        
        SerializedObject so = new SerializedObject(_selectedSkill);
        so.FindProperty("_skillType").enumValueIndex = (int)(SkillType)evt.newValue;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(_selectedSkill);
    }
    
    private void OnValueChanged(ChangeEvent<float> evt)
    {
        if (_selectedSkill == null) return;
        
        SerializedObject so = new SerializedObject(_selectedSkill);
        so.FindProperty("_value").floatValue = evt.newValue;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(_selectedSkill);
    }
    
    private void OnMaxLevelChanged(ChangeEvent<int> evt)
    {
        if (_selectedSkill == null) return;
        
        int newValue = Mathf.Max(1, evt.newValue);
        
        SerializedObject so = new SerializedObject(_selectedSkill);
        so.FindProperty("_maxLevel").intValue = newValue;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(_selectedSkill);
        
        if (newValue != evt.newValue)
        {
            _maxLevelField.SetValueWithoutNotify(newValue);
            Debug.Log("Max level must be at least 1. Value clamped.");
        }
    }
    
    private void OnPositionChanged(ChangeEvent<Vector2> evt)
    {
        if (_selectedSkill == null || _selectedNode == null) return;
        
        // Update node position
        Rect currentPos = _selectedNode.GetPosition();
        _selectedNode.SetPosition(new Rect(evt.newValue, currentPos.size));
        
        // Update skill position
        SerializedObject so = new SerializedObject(_selectedSkill);
        so.FindProperty("_position").vector2Value = evt.newValue;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(_selectedSkill);
    }

    // Helper Methods for Styling
    
    private void SetPadding(VisualElement element, int top, int right, int bottom, int left)
    {
        element.style.paddingTop = top;
        element.style.paddingRight = right;
        element.style.paddingBottom = bottom;
        element.style.paddingLeft = left;
    }
    
    private void SetPadding(VisualElement element, int all)
    {
        SetPadding(element, all, all, all, all);
    }

    private void SetBorderRadius(VisualElement element, int radius)
    {
        element.style.borderTopLeftRadius = radius;
        element.style.borderTopRightRadius = radius;
        element.style.borderBottomLeftRadius = radius;
        element.style.borderBottomRightRadius = radius;
    }
    
    private void SetBorderWidth(VisualElement element, int width)
    {
        element.style.borderTopWidth = width;
        element.style.borderRightWidth = width;
        element.style.borderBottomWidth = width;
        element.style.borderLeftWidth = width;
    }
    
    private void SetBorderColor(VisualElement element, Color color)
    {
        element.style.borderTopColor = color;
        element.style.borderRightColor = color;
        element.style.borderBottomColor = color;
        element.style.borderLeftColor = color;
    }
    
    private void SetMargin(VisualElement element, int top, int right, int bottom, int left)
    {
        element.style.marginTop = top;
        element.style.marginRight = right;
        element.style.marginBottom = bottom;
        element.style.marginLeft = left;
    }
}