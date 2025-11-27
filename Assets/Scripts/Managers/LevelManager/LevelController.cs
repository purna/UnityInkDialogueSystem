using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main controller for the level unlock system - handles level progression and unlock states
/// Supports both automatic generation and manual node assignment
/// </summary>
public class LevelController : MonoBehaviour
{
    [Header("Setup Mode")]
    [SerializeField] private LevelSetupMode _setupMode = LevelSetupMode.AutoGenerate;
    
    [Header("Level Data")]
    [SerializeField] private LevelContainer levelContainer;
    [SerializeField] private LevelGroup levelGroup;
    [SerializeField] private Level startingLevel;
    private bool _isLevelOpen = false;

    public bool IsLevelOpen => _isLevelOpen;
    
    [Header("Manual Node Assignment")]
    [SerializeField] private List<LevelNodeMapping> _manualLevelNodes = new List<LevelNodeMapping>();
    
    [Header("Level Selection (Auto Mode)")]
    [SerializeField] private bool groupedLevels = false;
    [SerializeField] private bool startingLevelsOnly = true;
    [SerializeField] private int selectedLevelGroupIndex = 0;
    [SerializeField] private int selectedLevelIndex = 0;
    
    [Header("System References")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameObject levelUI;
    
    [Header("Auto Start Settings")]
    [SerializeField] private bool initializeOnStart = false;
    [SerializeField] private float startDelay = 0f;
    
    [Header("UI Setup (Auto Generate Mode)")]
    [SerializeField] private Transform _levelNodesParent;
    [SerializeField] private GameObject _levelNodePrefab;
    [SerializeField] private GridLayoutGroup _gridLayout;
    [SerializeField] private bool _useGridLayout = false;
    [SerializeField] private bool _useLevelPositions = true;
    
    [Header("Line Rendering")]
    [SerializeField] private bool _autoGenerateLines = true;
    [SerializeField] private GameObject _connectionLinePrefab;
    [SerializeField] private Transform _linesParent;
    [SerializeField] private Color _lockedLineColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    [SerializeField] private Color _unlockedLineColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    [SerializeField] private Color _completedLineColor = new Color(1f, 0.84f, 0f, 1f);
    [SerializeField] private float _lineWidth = 3f;

    [Header("Tooltip")]
    [SerializeField] private LevelTooltip _levelTooltip;
 
    [Header("Details Panel")]
    [SerializeField] private GameObject _detailsPanel;
    [SerializeField] private LevelTooltip _detailsPanelTooltip;

    [Header("Details Panel UI (Legacy - Optional)")]
    [SerializeField] private TextMeshProUGUI _detailsTitle;
    [SerializeField] private TextMeshProUGUI _detailsDescription;
    [SerializeField] private Image _detailsIcon;
    [SerializeField] private TextMeshProUGUI _detailsStatus;
    [SerializeField] private Button _playButton;
    [SerializeField] private TextMeshProUGUI _playButtonText;
    
    [Header("Progress Display")]
    [SerializeField] private TextMeshProUGUI _progressText;
    [SerializeField] private Slider _progressBar;

    [Header("Close Button")]
    [SerializeField] private Button _closeButton;
    [SerializeField] private bool _allowCloseWithKey = true;
    [SerializeField] private KeyCode _closeKey = KeyCode.Escape;
    
    [Header("Player Control")]
    [SerializeField] private GameObject _playerObject;
    [SerializeField] private bool _disablePlayerWhenOpen = true;
    
    private IPlayerController _playerController;
    
    private Dictionary<Level, LevelNode> _levelNodes = new Dictionary<Level, LevelNode>();
    private Dictionary<Level, List<LineRenderer>> _connectionLines = new Dictionary<Level, List<LineRenderer>>();
    private List<LevelNode> _instantiatedNodes = new List<LevelNode>();
    private LevelNode _currentlySelectedNode;
    private Level _selectedLevel;
    
    private void Start()
    {
        if (_detailsPanel != null)
            _detailsPanel.SetActive(false);
        
        if (_playButton != null)
            _playButton.onClick.AddListener(OnPlayButtonClicked);

        if (_closeButton != null)
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
        
        if (levelManager == null)
            levelManager = LevelManager.Instance;

        if (levelManager != null)
        {
            levelManager.OnLevelUnlocked += OnLevelUnlocked;
            levelManager.OnLevelCompleted += OnLevelCompleted;
            levelManager.OnProgressChanged += OnProgressChanged;
        }

        if (_levelTooltip == null)
        {
            _levelTooltip = GetComponent<LevelTooltip>();
        }
        
        if (_levelTooltip != null)
        {
            _levelTooltip.gameObject.SetActive(false);
        }

        // Find player controller
        if (_playerObject != null && _disablePlayerWhenOpen)
        {
            _playerController = _playerObject.GetComponent<IPlayerController>();
            if (_playerController == null)
            {
                Debug.LogWarning("[LevelController] Player object doesn't implement IPlayerController interface!");
            }
        }
        else if (_disablePlayerWhenOpen)
        {
            // Try to find player by tag
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerObject = player;
                _playerController = player.GetComponent<IPlayerController>();
                if (_playerController == null)
                {
                    Debug.LogWarning("[LevelController] Player doesn't implement IPlayerController interface!");
                }
            }
        }
        
        // FIXED: Only initialize if initializeOnStart is TRUE
        if (initializeOnStart)
        {
            if (startDelay > 0)
                Invoke(nameof(InitializeLevel), startDelay);
            else
                InitializeLevel();
        }
        else
        {
            // IMPORTANT: Hide the UI if we're not auto-starting
            if (levelUI != null)
                levelUI.SetActive(false);
                
            Debug.Log("[LevelController] Level UI initialized but hidden. Waiting for trigger.");
        }
        
        UpdateProgressDisplay();
    }

    private void Update()
    {
        // Handle close key input when level UI is open
        if (_isLevelOpen && _allowCloseWithKey)
        {
            if (Input.GetKeyDown(_closeKey))
            {
                HideLevel();
            }
        }
    }
    
    private void OnDestroy()
    {
        if (levelManager != null)
        {
            levelManager.OnLevelUnlocked -= OnLevelUnlocked;
            levelManager.OnLevelCompleted -= OnLevelCompleted;
            levelManager.OnProgressChanged -= OnProgressChanged;
        }
    }
    
    private void InitializeLevel()
    {
        if (levelUI != null)
            levelUI.SetActive(true);
        
        if (_setupMode == LevelSetupMode.AutoGenerate)
        {
            GenerateLevel();
        }
        else
        {
            SetupManualLevelNodes();
        }
    }
    
    #region Public Level Access Methods
    
    public List<Level> GetAvailableLevels()
    {
        if (levelContainer == null)
        {
            Debug.LogWarning("[LevelController] No LevelContainer assigned!");
            return new List<Level>();
        }
        
        List<Level> levels = GetLevelsToGenerate();
        return levels ?? new List<Level>();
    }
    
    public Level GetLevelByName(string levelName)
    {
        List<Level> availableLevels = GetAvailableLevels();
        return availableLevels.Find(l => l.LevelName == levelName);
    }
    
    public void RegisterManualNode(Level level, LevelNode node)
    {
        if (level != null && node != null)
        {
            _levelNodes[level] = node;
            RegisterLevelNode(node);
        }
    }
    
    #endregion
    
    #region Manual Setup Mode
    
    private void SetupManualLevelNodes()
    {
        _levelNodes.Clear();
        ClearRegisteredNodes();
        
        LevelNode[] allNodes = FindObjectsOfType<LevelNode>();
        
        foreach (var node in allNodes)
        {
            Level level = node.GetLevel();
            if (level != null)
            {
                node.SetController(this);
                _levelNodes[level] = node;
                RegisterLevelNode(node);
            }
        }
        
        if (_manualLevelNodes != null && _manualLevelNodes.Count > 0)
        {
            foreach (var mapping in _manualLevelNodes)
            {
                if (mapping.level == null || mapping.nodeGameObject == null)
                    continue;
                
                LevelNode nodeComponent = mapping.nodeGameObject.GetComponent<LevelNode>();
                if (nodeComponent == null)
                    nodeComponent = mapping.nodeGameObject.AddComponent<LevelNode>();
                
                nodeComponent.SetController(this);
                nodeComponent.SetLevel(mapping.level);
                
                _levelNodes[mapping.level] = nodeComponent;
                RegisterLevelNode(nodeComponent);
            }
        }
        
        foreach (var level in _levelNodes.Keys)
        {
            if (_autoGenerateLines)
                CreateConnectionLines(level);
        }
        
        UpdateAllNodeStates();
        
        Debug.Log($"[LevelController] Initialized {_levelNodes.Count} manual level nodes");
    }
    
    #endregion
    
    #region Auto Generate Mode
    
    private void GenerateLevel()
    {
        if (levelContainer == null)
        {
            Debug.LogError("[LevelController] No level container assigned!");
            return;
        }
        
        ClearLevel();
        
        List<Level> levelsToGenerate = GetLevelsToGenerate();
        
        if (levelsToGenerate == null || levelsToGenerate.Count == 0)
        {
            Debug.LogWarning("[LevelController] No levels to generate!");
            return;
        }
        
        Debug.Log($"[LevelController] Generating {levelsToGenerate.Count} level nodes...");
        
        foreach (var level in levelsToGenerate)
        {
            CreateLevelNode(level);
        }
        
        if (_autoGenerateLines)
        {
            foreach (var level in levelsToGenerate)
            {
                CreateConnectionLines(level);
            }
        }
        
        UpdateAllNodeStates();
        
        Debug.Log($"[LevelController] Successfully generated {_levelNodes.Count} level nodes");
    }
    
    private List<Level> GetLevelsToGenerate()
    {
        if (groupedLevels && levelGroup != null)
        {
            // Change this line to NOT filter by startingLevelsOnly
            return levelContainer.GetGroupedLevels(levelGroup, false); // Always show all grouped levels
        }
        else if (startingLevel != null)
        {
            List<Level> allLevels = new List<Level>();
            CollectLevelRecursive(startingLevel, allLevels);
            return allLevels;
        }
        else
        {
            // Always return ALL levels, not just starting levels
            return levelContainer.GetAllLevels();
        }
    }
    
    private void CollectLevelRecursive(Level level, List<Level> collection)
    {
        if (level == null || collection.Contains(level))
            return;
        
        collection.Add(level);
        
        foreach (var child in level.Children)
        {
            CollectLevelRecursive(child, collection);
        }
    }
    
    private void CreateLevelNode(Level level)
    {
        if (_levelNodePrefab == null || _levelNodesParent == null)
        {
            Debug.LogError("[LevelController] Level node prefab or parent not assigned!");
            return;
        }
        
        GameObject nodeObj = Instantiate(_levelNodePrefab, _levelNodesParent);
        nodeObj.name = $"LevelNode_{level.LevelName}";
        
        LevelNode nodeComponent = nodeObj.GetComponent<LevelNode>();
        
        if (nodeComponent == null)
            nodeComponent = nodeObj.AddComponent<LevelNode>();
        
        nodeComponent.Initialize(level, this);
        
        if (!_useGridLayout && _useLevelPositions)
        {
            RectTransform rectTransform = nodeObj.GetComponent<RectTransform>();
            if (rectTransform != null)
                rectTransform.anchoredPosition = level.Position;
        }
        
        _levelNodes[level] = nodeComponent;
        RegisterLevelNode(nodeComponent);
        
        Debug.Log($"[LevelController] Created node for level: {level.LevelName}");
    }
    
    #endregion
    
    #region Connection Lines
    
    private void CreateConnectionLines(Level level)
    {
        if (!_levelNodes.ContainsKey(level))
            return;
        
        List<LineRenderer> lines = new List<LineRenderer>();
        
        foreach (var childLevel in level.Children)
        {
            if (childLevel == null || !_levelNodes.ContainsKey(childLevel))
                continue;
            
            LineRenderer line = CreateLine(level, childLevel);
            if (line != null)
                lines.Add(line);
        }
        
        if (lines.Count > 0)
            _connectionLines[level] = lines;
    }
    
    private LineRenderer CreateLine(Level fromLevel, Level toLevel)
    {
        Transform linesContainer = _linesParent != null ? _linesParent : _levelNodesParent;
        
        GameObject lineObj;
        if (_connectionLinePrefab != null)
            lineObj = Instantiate(_connectionLinePrefab, linesContainer);
        else
        {
            lineObj = new GameObject($"Line_{fromLevel.LevelName}_to_{toLevel.LevelName}");
            lineObj.transform.SetParent(linesContainer);
        }
        
        lineObj.transform.SetAsFirstSibling();
        
        LineRenderer line = lineObj.GetComponent<LineRenderer>();
        if (line == null)
            line = lineObj.AddComponent<LineRenderer>();
        
        line.positionCount = 2;
        line.startWidth = _lineWidth;
        line.endWidth = _lineWidth;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.sortingOrder = -1;
        
        Vector3 startPos = _levelNodes[fromLevel].transform.position;
        Vector3 endPos = _levelNodes[toLevel].transform.position;
        
        line.SetPosition(0, startPos);
        line.SetPosition(1, endPos);
        
        UpdateLineColor(line, fromLevel);
        
        return line;
    }
    
    private void UpdateLineColor(LineRenderer line, Level level)
    {
        if (line == null)
            return;
        
        Color color;
        if (level.IsCompleted)
            color = _completedLineColor;
        else if (level.IsUnlocked)
            color = _unlockedLineColor;
        else
            color = _lockedLineColor;
            
        line.startColor = color;
        line.endColor = color;
    }

    #endregion

    #region Cleanup

    private void ClearLevel()
    {
        if (_setupMode == LevelSetupMode.AutoGenerate)
        {
            foreach (var node in _levelNodes.Values)
            {
                if (node != null)
                    Destroy(node.gameObject);
            }
        }

        _levelNodes.Clear();
        ClearRegisteredNodes();

        foreach (var lines in _connectionLines.Values)
        {
            foreach (var line in lines)
            {
                if (line != null)
                    Destroy(line.gameObject);
            }
        }
        _connectionLines.Clear();
    }
    
    #endregion
    
    #region Details Panel
    
    public void ShowLevelDetails(Level level)
    {
        if (level == null)
        {
            Debug.LogWarning("[LevelController] Cannot show details for null level!");
            return;
        }

        Debug.Log($"[LevelController] ShowLevelDetails() called for: {level.LevelName}");

        _selectedLevel = level;

        if (levelUI != null && !levelUI.activeSelf)
        {
            levelUI.SetActive(true);
        }

        if (_detailsPanel != null)
        {
            _detailsPanel.SetActive(true);
            Debug.Log("[LevelController] ✓ Details panel activated");
        }
        else
        {
            Debug.LogWarning("[LevelController] Details panel reference is not set!");
            return;
        }
        
        UpdateNodeSelectionStates(level);
        UpdateDetailsUI(level);
        UpdatePlayButton();
    }

    private void UpdateNodeSelectionStates(Level selectedLevel)
    {
        foreach (var node in _instantiatedNodes)
        {
            if (node != null)
            {
                bool isSelected = node.GetLevel() == selectedLevel;
                node.SetSelected(isSelected);
                
                if (isSelected)
                    _currentlySelectedNode = node;
            }
        }
    }
    
    private void UpdateDetailsUI(Level level)
    {
        if (_detailsPanelTooltip != null)
        {
            _detailsPanelTooltip.SetLevel(level);
        }
        else
        {
            if (_detailsTitle != null)
                _detailsTitle.text = level.LevelName;
            
            if (_detailsDescription != null)
                _detailsDescription.text = level.Description;
            
            if (_detailsIcon != null)
            {
                if (level.IsCompleted && level.CompletedIcon != null)
                    _detailsIcon.sprite = level.CompletedIcon;
                else if (level.IsUnlocked && level.UnlockedIcon != null)
                    _detailsIcon.sprite = level.UnlockedIcon;
                else if (!level.IsUnlocked && level.LockedIcon != null)
                    _detailsIcon.sprite = level.LockedIcon;
                else if (level.Icon != null)
                    _detailsIcon.sprite = level.Icon;
            }
            
            if (_detailsStatus != null)
            {
                string status = level.IsCompleted ? "✓ Completed" : 
                               level.IsUnlocked ? "Unlocked - Ready to Play" : 
                               "🔒 Locked";
                _detailsStatus.text = status;
            }
        }
    }

    public void HideLevelDetails()
    {
        if (_detailsPanel != null)
        {
            _detailsPanel.SetActive(false);
        }

        foreach (var node in _instantiatedNodes)
        {
            if (node != null)
                node.SetSelected(false);
        }

        _currentlySelectedNode = null;
        _selectedLevel = null;
    }

    public void RegisterLevelNode(LevelNode node)
    {
        if (node != null && !_instantiatedNodes.Contains(node))
            _instantiatedNodes.Add(node);
    }
    
    public void UnregisterLevelNode(LevelNode node)
    {
        if (node != null && _instantiatedNodes.Contains(node))
            _instantiatedNodes.Remove(node);
    }
    
    public void ClearRegisteredNodes()
    {
        _instantiatedNodes.Clear();
        _currentlySelectedNode = null;
    }

    public void RefreshDetailsPanel()
    {
        if (_selectedLevel != null)
            ShowLevelDetails(_selectedLevel);
    }

    private void UpdatePlayButton()
    {
        if (_playButton == null || _selectedLevel == null)
            return;
        
        bool canPlay = _selectedLevel.IsUnlocked && !_selectedLevel.IsCompleted;
        
        if (_selectedLevel.IsCompleted)
        {
            _playButton.interactable = true;
            if (_playButtonText != null)
                _playButtonText.text = "Replay";
        }
        else if (_selectedLevel.IsUnlocked)
        {
            _playButton.interactable = true;
            if (_playButtonText != null)
                _playButtonText.text = "Play Level";
        }
        else
        {
            _playButton.interactable = false;
            if (_playButtonText != null)
                _playButtonText.text = "Locked";
        }
    }

    private void OnPlayButtonClicked()
    {
        if (_selectedLevel == null || levelManager == null)
            return;

        if (_selectedLevel.IsUnlocked)
        {
            levelManager.LoadLevel(_selectedLevel);
            Debug.Log($"[LevelController] Loading level: {_selectedLevel.LevelName}");
        }
    }

    private void OnCloseButtonClicked()
    {
        HideLevel();
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnLevelUnlocked(Level level)
    {
        UpdateNodeState(level);
        UpdateConnectionLines(level);
        
        if (_selectedLevel == level)
            ShowLevelDetails(level);
            
        if (_levelNodes.ContainsKey(level))
            _levelNodes[level].PlayUnlockAnimation();
            
        UpdateProgressDisplay();
    }
    
    private void OnLevelCompleted(Level level)
    {
        UpdateNodeState(level);
        UpdateConnectionLines(level);
        
        if (_selectedLevel == level)
            ShowLevelDetails(level);
            
        if (_levelNodes.ContainsKey(level))
            _levelNodes[level].PlayCompletionAnimation();
            
        UpdateProgressDisplay();
    }
    
    private void OnProgressChanged(int completed, int total)
    {
        UpdateProgressDisplay();
        
        if (_selectedLevel != null)
            UpdatePlayButton();
        
        UpdateAllNodeStates();
    }
    
    #endregion
    
    #region Update Methods
    
    private void UpdateProgressDisplay()
    {
        if (levelManager == null)
            return;
            
        int completed = levelManager.GetCompletedLevelsCount();
        int total = levelManager.GetTotalLevelsCount();
        float percentage = total > 0 ? (float)completed / total * 100f : 0f;
        
        if (_progressText != null)
            _progressText.text = $"Progress: {completed}/{total} ({percentage:F0}%)";
        
        if (_progressBar != null)
        {
            _progressBar.maxValue = total;
            _progressBar.value = completed;
        }
    }
    
    private void UpdateNodeState(Level level)
    {
        if (_levelNodes.ContainsKey(level))
            _levelNodes[level].UpdateDisplay();
    }
    
    private void UpdateConnectionLines(Level level)
    {
        if (_connectionLines.ContainsKey(level))
        {
            foreach (var line in _connectionLines[level])
                UpdateLineColor(line, level);
        }
    }
    
    private void UpdateAllNodeStates()
    {
        foreach (var node in _levelNodes.Values)
            node.UpdateDisplay();
        
        foreach (var levelLines in _connectionLines)
        {
            foreach (var line in levelLines.Value)
                UpdateLineColor(line, levelLines.Key);
        }
    }
    
    #endregion
    
    #region Public Methods
    
    public void RefreshLevel()
    {
        InitializeLevel();
    }
    
    public void SetLevelContainer(LevelContainer container)
    {
        levelContainer = container;
        InitializeLevel();
    }
    
    public void AddManualLevelNode(Level level, GameObject nodeGameObject)
    {
        if (level == null || nodeGameObject == null)
        {
            Debug.LogWarning("[LevelController] Cannot add null level or GameObject!");
            return;
        }
        
        _manualLevelNodes.Add(new LevelNodeMapping { level = level, nodeGameObject = nodeGameObject });
        
        if (_levelNodes != null && _setupMode == LevelSetupMode.ManualAssignment)
        {
            LevelNode nodeComponent = nodeGameObject.GetComponent<LevelNode>();
            if (nodeComponent == null)
                nodeComponent = nodeGameObject.AddComponent<LevelNode>();
            
            nodeComponent.Initialize(level, this);
            _levelNodes[level] = nodeComponent;
            RegisterLevelNode(nodeComponent);
            
            if (_autoGenerateLines)
                CreateConnectionLines(level);
        }
    }

    public GameObject GetLevelNodeGameObject(Level level)
    {
        if (_levelNodes.ContainsKey(level))
            return _levelNodes[level].gameObject;
        return null;
    }

    public void ShowLevel()
    {
        Debug.Log("[LevelController] ShowLevel() called");
        
        if (levelUI != null)
        {
            levelUI.SetActive(true);
            _isLevelOpen = true;
            Debug.Log("[LevelController] ✓ Main level UI activated");
        }
        else
        {
            Debug.LogWarning("[LevelController] levelUI is not assigned!");
        }

        if (_detailsPanel != null)
        {
            _detailsPanel.SetActive(false);
            Debug.Log("[LevelController] Details panel hidden (will show on level selection)");
        }

        if (_levelNodes == null || _levelNodes.Count == 0)
        {
            Debug.Log("[LevelController] Initializing level nodes...");
            InitializeLevel();
        }

        UpdateAllNodeStates();
        UpdateProgressDisplay();

        // Disable player movement
        if (_disablePlayerWhenOpen && _playerController != null)
        {
            _playerController.DisablePlayer();
            Debug.Log("[LevelController] Player movement disabled");
        }
    }

    public void HideLevel()
    {
        if (levelUI != null)
        {
            levelUI.SetActive(false);
            _isLevelOpen = false;
            Debug.Log("[LevelController] Level UI hidden");
        }

        if (_detailsPanel != null)
            _detailsPanel.SetActive(false);

        if (_currentlySelectedNode != null)
        {
            _currentlySelectedNode.SetSelected(false);
            _currentlySelectedNode = null;
        }

        _selectedLevel = null;

        // Re-enable player movement
        if (_disablePlayerWhenOpen && _playerController != null)
        {
            _playerController.EnablePlayer();
            Debug.Log("[LevelController] Player movement enabled");
        }
    }

    public void ToggleLevel()
    {
        if (levelUI != null)
        {
            if (levelUI.activeSelf)
                HideLevel();
            else
                ShowLevel();
        }
    }

    public void SetLevelGroup(LevelGroup group)
    {
        levelGroup = group;
        InitializeLevel();
        Debug.Log($"[LevelController] Filtered to group: {group?.GroupName ?? "None"}");
    }

    public void FilterByGroup(string groupName)
    {
        if (levelContainer == null)
        {
            Debug.LogWarning("[LevelController] No container assigned to filter by group!");
            return;
        }
        
        foreach (var kvp in levelContainer.Groups)
        {
            if (kvp.Key.GroupName == groupName)
            {
                SetLevelGroup(kvp.Key);
                return;
            }
        }
        
        Debug.LogWarning($"[LevelController] Could not find group with name: {groupName}");
    }

    public LevelGroup GetLevelGroup(string groupName)
    {
        if (levelContainer == null)
            return null;
        
        foreach (var kvp in levelContainer.Groups)
        {
            if (kvp.Key.GroupName == groupName)
                return kvp.Key;
        }
        
        return null;
    }

    public void ClearGroupFilter()
    {
        levelGroup = null;
        InitializeLevel();
        Debug.Log("[LevelController] Group filter cleared");
    }

    #endregion

     #region Tooltip Methods

    // Call this method from your LevelNode when hovering
    public void ShowTooltipForLevel(Level level)
    {
        if (_levelTooltip != null && level != null)
        {
            _levelTooltip.gameObject.SetActive(true);
            _levelTooltip.SetLevel(level);
        }
    }

    public void HideTooltip()
    {
        if (_levelTooltip != null)
        {
            _levelTooltip.gameObject.SetActive(false);
        }
    }
    
    // Then in your LevelNode, call these methods:
    // - OnPointerEnter: levelController.ShowTooltipForLevel(level);
    // - OnPointerExit: levelController.HideTooltip();
    
    #endregion
}

public enum LevelSetupMode
{
    AutoGenerate,
    ManualAssignment
}

[Serializable]
public class LevelNodeMapping
{
    [Tooltip("The level data")]
    public Level level;
    
    [Tooltip("The GameObject representing this level node")]
    public GameObject nodeGameObject;
}