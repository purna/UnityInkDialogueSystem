using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

/// <summary>
/// Sidebar for displaying and editing selected Level node properties
/// </summary>
public class LevelSystemSidebar
{
    private LevelBaseNode _selectedNode;
    private Level _selectedLevel;
    private ScrollView _scrollView;
    private VisualElement _contentContainer;
    private VisualElement _rootElement;
    
    private const float SIDEBAR_WIDTH = 300f;
    
    // UI Elements for Level properties
    private TextField _levelNameField;
    private TextField _descriptionField;
    private ObjectField _iconField;
    private ObjectField _lockedIconField;
    private ObjectField _unlockedIconField;
    private ObjectField _completedIconField;
    private IntegerField _tierField;
    private IntegerField _levelIndexField;
    private EnumField _levelTypeField;
    private ObjectField _gameSceneField;
    private FloatField _completionThresholdField;
    private IntegerField _maxAttemptsField;
    private Vector2Field _positionField;
    
    // Runtime state display (read-only)
    private Label _statusLabel;
    private Label _attemptsLabel;
    private Label _bestScoreLabel;
    private Label _completionLabel;
    
    public float Width => SIDEBAR_WIDTH;
    public bool IsVisible => _rootElement != null && _rootElement.style.display == DisplayStyle.Flex;
    
    public VisualElement CreateSidebar()
    {
        _rootElement = new VisualElement();
        _rootElement.name = "level-sidebar";
        _rootElement.style.width = SIDEBAR_WIDTH;
        _rootElement.style.backgroundColor = new Color(0.22f, 0.22f, 0.22f);
        _rootElement.style.borderLeftWidth = 1;
        _rootElement.style.borderLeftColor = new Color(0.1f, 0.1f, 0.1f);
        _rootElement.style.display = DisplayStyle.None;
        
        // Header
        Label header = new Label("Level Properties");
        header.style.fontSize = 16;
        header.style.unityFontStyleAndWeight = FontStyle.Bold;
        header.style.paddingTop = 10;
        header.style.paddingBottom = 10;
        header.style.paddingLeft = 10;
        header.style.paddingRight = 10;
        header.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);
        header.style.borderBottomWidth = 1;
        header.style.borderBottomColor = new Color(0.1f, 0.1f, 0.1f);
        _rootElement.Add(header);
        
        // Scroll view for content
        _scrollView = new ScrollView(ScrollViewMode.Vertical);
        _scrollView.style.flexGrow = 1;
        _rootElement.Add(_scrollView);
        
        _contentContainer = new VisualElement();
        _contentContainer.style.paddingTop = 10;
        _contentContainer.style.paddingBottom = 10;
        _contentContainer.style.paddingLeft = 10;
        _contentContainer.style.paddingRight = 10;
        _scrollView.Add(_contentContainer);
        
        BuildPropertyFields();
        
        return _rootElement;
    }
    
    private void BuildPropertyFields()
    {
        _contentContainer.Clear();
        
        // Level Identity Section
        AddSectionHeader("Identity");
        
        _levelNameField = new TextField("Level Name");
        _levelNameField.tooltip = "The unique name of this level. This will be displayed in the UI and used to identify the level.";
        _levelNameField.RegisterValueChangedCallback(OnLevelNameChanged);
        _contentContainer.Add(_levelNameField);
        
        _descriptionField = new TextField("Description");
        _descriptionField.multiline = true;
        _descriptionField.style.minHeight = 60;
        _descriptionField.tooltip = "A detailed description of this level. This will be shown to players when they view the level.";
        _descriptionField.RegisterValueChangedCallback(OnDescriptionChanged);
        _contentContainer.Add(_descriptionField);
        
        _iconField = new ObjectField("Icon");
        _iconField.objectType = typeof(Sprite);
        _iconField.tooltip = "The visual icon for this level. Should be a Sprite asset that represents the level's theme.";
        _iconField.RegisterValueChangedCallback(OnIconChanged);
        _contentContainer.Add(_iconField);

        _lockedIconField = new ObjectField("Locked Icon");
        _lockedIconField.objectType = typeof(Sprite);
        _lockedIconField.tooltip = "The icon displayed when this level is locked/unavailable.";
        _lockedIconField.RegisterValueChangedCallback(OnLockedIconChanged);
        _contentContainer.Add(_lockedIconField);

        _unlockedIconField = new ObjectField("Unlocked Icon");
        _unlockedIconField.objectType = typeof(Sprite);
        _unlockedIconField.tooltip = "The icon displayed when this level is unlocked but not yet completed.";
        _unlockedIconField.RegisterValueChangedCallback(OnUnlockedIconChanged);
        _contentContainer.Add(_unlockedIconField);

        _completedIconField = new ObjectField("Completed Icon");
        _completedIconField.objectType = typeof(Sprite);
        _completedIconField.tooltip = "The icon displayed when this level has been completed.";
        _completedIconField.RegisterValueChangedCallback(OnCompletedIconChanged);
        _contentContainer.Add(_completedIconField);
        
        AddSpace();
        
        // Level Properties Section
        AddSectionHeader("Properties");
        
        _tierField = new IntegerField("Tier");
        _tierField.tooltip = "The tier or difficulty level of this level in the progression tree. Higher tiers typically require more prerequisites.\nExample: Tier 1 = Beginner, Tier 2 = Intermediate, Tier 3 = Advanced";
        _tierField.RegisterValueChangedCallback(OnTierChanged);
        _contentContainer.Add(_tierField);
        
        _levelIndexField = new IntegerField("Level Index");
        _levelIndexField.tooltip = "The sequential order of this level in its tier. Used for organizing linear progressions.\nExample: Level 1, Level 2, Level 3, etc.";
        _levelIndexField.RegisterValueChangedCallback(OnLevelIndexChanged);
        _contentContainer.Add(_levelIndexField);
        
        _levelTypeField = new EnumField("Level Type", LevelSceneType.Normal);
        _levelTypeField.tooltip = "The type or category of this level. Used for filtering and organization.";
        _levelTypeField.RegisterValueChangedCallback(OnLevelTypeChanged);
        _contentContainer.Add(_levelTypeField);

        // NOTE: SceneField requires custom PropertyDrawer, so we'll use a workaround
        // We'll create a custom field that mimics SceneField behavior
        _gameSceneField = new ObjectField("Game Scene");
        _gameSceneField.objectType = typeof(SceneAsset);
        _gameSceneField.tooltip = "The Unity scene that will be loaded when this level is played. Drag a scene asset from your project here.";
        _gameSceneField.RegisterValueChangedCallback(OnGameSceneChanged);
        _contentContainer.Add(_gameSceneField);
        
        AddSpace();
        
        // Completion Requirements Section
        AddSectionHeader("Completion Requirements");
        
        _completionThresholdField = new FloatField("Completion Threshold");
        _completionThresholdField.tooltip = "The score or objective count required to complete this level.\nExample: 100 points, 50 enemies defeated, etc.";
        _completionThresholdField.RegisterValueChangedCallback(OnCompletionThresholdChanged);
        _contentContainer.Add(_completionThresholdField);
        
        _maxAttemptsField = new IntegerField("Max Attempts");
        _maxAttemptsField.tooltip = "The maximum number of times this level can be attempted. Set to -1 for unlimited attempts.\nExample: 3 attempts for challenge levels, -1 for practice levels";
        _maxAttemptsField.RegisterValueChangedCallback(OnMaxAttemptsChanged);
        _contentContainer.Add(_maxAttemptsField);

        AddSpace();
        
        // Runtime State Section (Read-Only)
        AddSectionHeader("Runtime State (Read-Only)");
        
        _statusLabel = new Label("Status: Not Started");
        _statusLabel.name = "status-label";
        _statusLabel.style.paddingLeft = 5;
        _statusLabel.style.paddingTop = 3;
        _statusLabel.style.paddingBottom = 3;
        _statusLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
        _contentContainer.Add(_statusLabel);
        
        _attemptsLabel = new Label("Attempts Used: 0");
        _attemptsLabel.name = "attempts-label";
        _attemptsLabel.style.paddingLeft = 5;
        _attemptsLabel.style.paddingTop = 3;
        _attemptsLabel.style.paddingBottom = 3;
        _attemptsLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
        _contentContainer.Add(_attemptsLabel);
        
        _bestScoreLabel = new Label("Best Score: 0");
        _bestScoreLabel.name = "best-score-label";
        _bestScoreLabel.style.paddingLeft = 5;
        _bestScoreLabel.style.paddingTop = 3;
        _bestScoreLabel.style.paddingBottom = 3;
        _bestScoreLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
        _contentContainer.Add(_bestScoreLabel);
        
        _completionLabel = new Label("Times Completed: 0");
        _completionLabel.name = "completion-label";
        _completionLabel.style.paddingLeft = 5;
        _completionLabel.style.paddingTop = 3;
        _completionLabel.style.paddingBottom = 3;
        _completionLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
        _contentContainer.Add(_completionLabel);

        AddSpace();
        
        // Position Section
        AddSectionHeader("Position");
        
        _positionField = new Vector2Field("Node Position");
        _positionField.tooltip = "The X and Y position of this level node in the graph editor. You can also drag the node directly in the graph.";
        _positionField.RegisterValueChangedCallback(OnPositionChanged);
        _contentContainer.Add(_positionField);
        
        AddSpace();
        
        // Connections Section
        AddSectionHeader("Connections");
        
        Label prerequisitesLabel = new Label("Prerequisites: 0");
        prerequisitesLabel.name = "prerequisites-label";
        prerequisitesLabel.style.paddingLeft = 5;
        prerequisitesLabel.style.paddingTop = 5;
        prerequisitesLabel.style.paddingBottom = 5;
        prerequisitesLabel.tooltip = "The number of levels that must be completed before this level becomes available.\nConnect input ports from other levels to create prerequisites.";
        _contentContainer.Add(prerequisitesLabel);
        
        Label nextLevelsLabel = new Label("Next Levels: 0");
        nextLevelsLabel.name = "next-levels-label";
        nextLevelsLabel.style.paddingLeft = 5;
        nextLevelsLabel.style.paddingTop = 5;
        nextLevelsLabel.style.paddingBottom = 5;
        nextLevelsLabel.tooltip = "The number of levels that become available after completing this level.\nConnect output ports to other levels to create progression paths.";
        _contentContainer.Add(nextLevelsLabel);
        
        AddSpace();
        
        // Add help text at the bottom
        AddHelpSection();
    }
    
    private void AddHelpSection()
    {
        AddSectionHeader("Quick Tips");
        
        Label helpText = new Label(
            "• Hover over any field to see its tooltip\n" +
            "• Drag nodes in the graph to reposition\n" +
            "• Connect output ports to create level progression\n" +
            "• Set Max Attempts to -1 for unlimited tries\n" +
            "• Use Tier and Index to organize difficulty\n" +
            "• Use Ctrl+S to save your changes\n" +
            "• Right-click nodes for more options"
        );
        helpText.style.paddingLeft = 5;
        helpText.style.paddingTop = 5;
        helpText.style.paddingBottom = 5;
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
    
    public void ShowNode(LevelBaseNode node, Level level)
    {
        _selectedNode = node;
        _selectedLevel = level;
        
        if (_rootElement != null)
        {
            _rootElement.style.display = DisplayStyle.Flex;
            UpdateFields();
        }
    }
    
    public void Hide()
    {
        _selectedNode = null;
        _selectedLevel = null;
        
        if (_rootElement != null)
        {
            _rootElement.style.display = DisplayStyle.None;
        }
    }
    
    private void UpdateFields()
    {
        if (_selectedNode == null || _selectedLevel == null)
            return;
        
        // Update all fields with current values
        _levelNameField.SetValueWithoutNotify(_selectedLevel.LevelName);
        _descriptionField.SetValueWithoutNotify(_selectedLevel.Description);
        _iconField.SetValueWithoutNotify(_selectedLevel.Icon);
        _lockedIconField.SetValueWithoutNotify(_selectedLevel.LockedIcon);
        _unlockedIconField.SetValueWithoutNotify(_selectedLevel.UnlockedIcon);
        _completedIconField.SetValueWithoutNotify(_selectedLevel.CompletedIcon);
        _tierField.SetValueWithoutNotify(_selectedLevel.Tier);
        _levelIndexField.SetValueWithoutNotify(_selectedLevel.LevelIndex);
        _levelTypeField.SetValueWithoutNotify(_selectedLevel.LevelSceneType);
        
        // Set game scene field - convert SceneField to SceneAsset
        if (_selectedLevel.GameScene != null && !string.IsNullOrEmpty(_selectedLevel.GameSceneName))
        {
            string scenePath = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByName(_selectedLevel.GameSceneName).path;
            if (string.IsNullOrEmpty(scenePath))
            {
                // Try to find the scene asset by name
                string[] guids = AssetDatabase.FindAssets($"t:SceneAsset {_selectedLevel.GameSceneName}");
                if (guids.Length > 0)
                {
                    scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                }
            }
            
            if (!string.IsNullOrEmpty(scenePath))
            {
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                _gameSceneField.SetValueWithoutNotify(sceneAsset);
            }
            else
            {
                _gameSceneField.SetValueWithoutNotify(null);
            }
        }
        else
        {
            _gameSceneField.SetValueWithoutNotify(null);
        }
        
        _completionThresholdField.SetValueWithoutNotify(_selectedLevel.CompletionThreshold);
        _maxAttemptsField.SetValueWithoutNotify(_selectedLevel.MaxAttempts);
        _positionField.SetValueWithoutNotify(_selectedNode.GetPosition().position);
        
        // Update runtime state labels
        UpdateRuntimeStateLabels();
        
        // Update connection counts
        Label prerequisitesLabel = _contentContainer.Q<Label>("prerequisites-label");
        if (prerequisitesLabel != null)
            prerequisitesLabel.text = $"Prerequisites: {_selectedLevel.PrerequisiteLevels.Count}";
        
        Label nextLevelsLabel = _contentContainer.Q<Label>("next-levels-label");
        if (nextLevelsLabel != null)
            nextLevelsLabel.text = $"Next Levels: {_selectedLevel.NextLevels.Count}";
    }
    
    private void UpdateRuntimeStateLabels()
    {
        if (_selectedLevel == null)
            return;
        
        // Status
        if (_statusLabel != null)
        {
            string status;
            Color statusColor;
            
            if (_selectedLevel.IsCompleted)
            {
                status = "Status: ✓ Completed";
                statusColor = new Color(0.2f, 0.8f, 0.2f);
            }
            else if (_selectedLevel.IsUnlocked)
            {
                status = "Status: Unlocked";
                statusColor = new Color(0.2f, 0.5f, 0.8f);
            }
            else
            {
                status = "Status: 🔒 Locked";
                statusColor = new Color(0.6f, 0.6f, 0.6f);
            }
            
            _statusLabel.text = status;
            _statusLabel.style.color = statusColor;
        }
        
        // Attempts
        if (_attemptsLabel != null)
        {
            if (_selectedLevel.MaxAttempts > 0)
            {
                int remaining = _selectedLevel.GetRemainingAttempts();
                _attemptsLabel.text = $"Attempts Used: {_selectedLevel.AttemptsUsed}/{_selectedLevel.MaxAttempts} ({remaining} remaining)";
            }
            else
            {
                _attemptsLabel.text = $"Attempts Used: {_selectedLevel.AttemptsUsed} (Unlimited)";
            }
        }
        
        // Best Score
        if (_bestScoreLabel != null)
        {
            float percentage = _selectedLevel.GetCompletionPercentage();
            _bestScoreLabel.text = $"Best Score: {_selectedLevel.BestScore:F1} ({percentage:F0}%)";
        }
        
        // Completion count
        if (_completionLabel != null)
        {
            _completionLabel.text = $"Times Completed: {_selectedLevel.TimesCompleted}";
        }
    }
    
    /// <summary>
    /// Refreshes the sidebar if a level is currently selected (for external changes)
    /// </summary>
    public void RefreshIfSelected(Level level)
    {
        if (_selectedLevel == level && _rootElement.style.display == DisplayStyle.Flex)
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
    private void OnLevelNameChanged(ChangeEvent<string> evt)
    {
        if (_selectedLevel == null) return;
        
        string newValue = evt.newValue.Trim();
        if (string.IsNullOrEmpty(newValue))
        {
            Debug.LogWarning("Level name cannot be empty!");
            _levelNameField.SetValueWithoutNotify(_selectedLevel.LevelName);
            return;
        }
        
        SerializedObject so = new SerializedObject(_selectedLevel);
        so.FindProperty("_levelName").stringValue = newValue;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(_selectedLevel);
    }
    
    private void OnDescriptionChanged(ChangeEvent<string> evt)
    {
        if (_selectedLevel == null || _selectedNode == null) return;
        
        SerializedObject so = new SerializedObject(_selectedLevel);
        so.FindProperty("_description").stringValue = evt.newValue;
        so.ApplyModifiedProperties();
        
        // Update the node's text and visual display
        _selectedNode.Text = evt.newValue;
        _selectedNode.UpdateDescription(evt.newValue);
        
        EditorUtility.SetDirty(_selectedLevel);
    }

    private void OnIconChanged(ChangeEvent<Object> evt)
    {
        if (_selectedLevel == null || _selectedNode == null) return;

        SerializedObject so = new SerializedObject(_selectedLevel);
        so.FindProperty("_icon").objectReferenceValue = evt.newValue;
        so.ApplyModifiedProperties();

        // Update the node's icon display
        Sprite newIcon = evt.newValue as Sprite;
        _selectedNode.UpdateIcon(newIcon);

        EditorUtility.SetDirty(_selectedLevel);
    }
    
    private void OnLockedIconChanged(ChangeEvent<Object> evt)
    {
        if (_selectedLevel == null) return;
        
        SerializedObject so = new SerializedObject(_selectedLevel);
        so.FindProperty("_lockedIcon").objectReferenceValue = evt.newValue;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(_selectedLevel);
        
        // Refresh node display
        if (_selectedNode != null)
            _selectedNode.UpdateVisualDisplay();
    }

    private void OnUnlockedIconChanged(ChangeEvent<Object> evt)
    {
        if (_selectedLevel == null) return;
        
        SerializedObject so = new SerializedObject(_selectedLevel);
        so.FindProperty("_unlockedIcon").objectReferenceValue = evt.newValue;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(_selectedLevel);
        
        // Refresh node display
        if (_selectedNode != null)
            _selectedNode.UpdateVisualDisplay();
    }

    private void OnCompletedIconChanged(ChangeEvent<Object> evt)
    {
        if (_selectedLevel == null) return;
        
        SerializedObject so = new SerializedObject(_selectedLevel);
        so.FindProperty("_completedIcon").objectReferenceValue = evt.newValue;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(_selectedLevel);
        
        // Refresh node display
        if (_selectedNode != null)
            _selectedNode.UpdateVisualDisplay();
    }
    
    private void OnTierChanged(ChangeEvent<int> evt)
    {
        if (_selectedLevel == null || _selectedNode == null) return;
        
        int newValue = Mathf.Max(0, evt.newValue);
        
        SerializedObject so = new SerializedObject(_selectedLevel);
        so.FindProperty("_tier").intValue = newValue;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(_selectedLevel);
        
        // Update node display
        _selectedNode.UpdateLevelProperties(newValue, _selectedLevel.LevelIndex, 
            _selectedLevel.CompletionThreshold, _selectedLevel.MaxAttempts);
        
        if (newValue != evt.newValue)
        {
            _tierField.SetValueWithoutNotify(newValue);
        }
    }
    
    private void OnLevelIndexChanged(ChangeEvent<int> evt)
    {
        if (_selectedLevel == null || _selectedNode == null) return;
        
        int newValue = Mathf.Max(0, evt.newValue);
        
        SerializedObject so = new SerializedObject(_selectedLevel);
        so.FindProperty("_levelIndex").intValue = newValue;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(_selectedLevel);
        
        // Update node display
        _selectedNode.UpdateLevelProperties(_selectedLevel.Tier, newValue, 
            _selectedLevel.CompletionThreshold, _selectedLevel.MaxAttempts);
        
        if (newValue != evt.newValue)
        {
            _levelIndexField.SetValueWithoutNotify(newValue);
        }
    }
    
    private void OnLevelTypeChanged(ChangeEvent<System.Enum> evt)
    {
        if (_selectedLevel == null) return;
        
        SerializedObject so = new SerializedObject(_selectedLevel);
        so.FindProperty("_levelSceneType").enumValueIndex = (int)(LevelSceneType)evt.newValue;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(_selectedLevel);
    }

    private void OnGameSceneChanged(ChangeEvent<Object> evt)
    {
        if (_selectedLevel == null) return;

        SceneAsset sceneAsset = evt.newValue as SceneAsset;
        
        // We need to update the SceneField in the Level
        // Since SceneField is a custom class, we need to use SerializedProperty
        SerializedObject so = new SerializedObject(_selectedLevel);
        SerializedProperty sceneFieldProp = so.FindProperty("_gameScene");
        
        if (sceneFieldProp != null)
        {
            SerializedProperty sceneAssetProp = sceneFieldProp.FindPropertyRelative("m_SceneAsset");
            SerializedProperty sceneNameProp = sceneFieldProp.FindPropertyRelative("m_SceneName");
            
            if (sceneAssetProp != null && sceneNameProp != null)
            {
                sceneAssetProp.objectReferenceValue = sceneAsset;
                sceneNameProp.stringValue = sceneAsset != null ? sceneAsset.name : "";
            }
        }
        
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(_selectedLevel);
        
        // Update node display if needed
        if (_selectedNode != null)
        {
            _selectedNode.UpdateVisualDisplay();
        }
    }
    
    private void OnCompletionThresholdChanged(ChangeEvent<float> evt)
    {
        if (_selectedLevel == null || _selectedNode == null) return;
        
        float newValue = Mathf.Max(0f, evt.newValue);
        
        SerializedObject so = new SerializedObject(_selectedLevel);
        so.FindProperty("_completionThreshold").floatValue = newValue;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(_selectedLevel);
        
        // Update node display
        _selectedNode.UpdateLevelProperties(_selectedLevel.Tier, _selectedLevel.LevelIndex, 
            newValue, _selectedLevel.MaxAttempts);
        
        if (newValue != evt.newValue)
        {
            _completionThresholdField.SetValueWithoutNotify(newValue);
        }
    }
    
    private void OnMaxAttemptsChanged(ChangeEvent<int> evt)
    {
        if (_selectedLevel == null || _selectedNode == null) return;
        
        // Allow -1 for unlimited, otherwise minimum of 1
        int newValue = evt.newValue;
        if (newValue < -1)
            newValue = -1;
        else if (newValue == 0)
            newValue = 1;
        
        SerializedObject so = new SerializedObject(_selectedLevel);
        so.FindProperty("_maxAttempts").intValue = newValue;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(_selectedLevel);
        
        // Update node display
        _selectedNode.UpdateLevelProperties(_selectedLevel.Tier, _selectedLevel.LevelIndex, 
            _selectedLevel.CompletionThreshold, newValue);
        
        if (newValue != evt.newValue)
        {
            _maxAttemptsField.SetValueWithoutNotify(newValue);
            Debug.Log("Max attempts must be -1 (unlimited) or >= 1. Value adjusted.");
        }
    }
    
    private void OnPositionChanged(ChangeEvent<Vector2> evt)
    {
        if (_selectedLevel == null || _selectedNode == null) return;
        
        // Update node position
        Rect currentPos = _selectedNode.GetPosition();
        _selectedNode.SetPosition(new Rect(evt.newValue, currentPos.size));
        
        // Update level position
        SerializedObject so = new SerializedObject(_selectedLevel);
        so.FindProperty("_position").vector2Value = evt.newValue;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(_selectedLevel);
    }
}