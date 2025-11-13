using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main controller for the skill tree UI - handles instantiation and display
/// Supports both automatic generation and manual node assignment
/// </summary>
public class SkillsTreeController : MonoBehaviour
{
    [Header("Setup Mode")]
    [SerializeField] private SkillTreeSetupMode _setupMode = SkillTreeSetupMode.AutoGenerate;
    
    [Header("SkillsTree Data")]
    [SerializeField] private SkillsTreeContainer skillstreeContainer;
    [SerializeField] private SkillsTreeGroup skillstreeGroup;
    [SerializeField] private Skill skillstree;
    private bool _isSkillTreeOpen = false;

    public bool IsSkillTreeOpen => _isSkillTreeOpen;
    
    [Header("Manual Node Assignment")]
    [SerializeField] private List<SkillNodeMapping> _manualSkillNodes = new List<SkillNodeMapping>();
    
    [Header("SkillsTree Selection (Auto Mode)")]
    [SerializeField] private bool groupedSkillsTrees = false;
    [SerializeField] private bool startingSkillsTreesOnly = false;
    [SerializeField] private int selectedSkillsTreeGroupIndex = 0;
    [SerializeField] private int selectedSkillsTreeIndex = 0;
    
    [Header("System References")]
    [SerializeField] private SkillTreeManager skillstreeManager;
    [SerializeField] private GameObject skillstreeUI;
    
    [Header("Auto Start Settings")]
    [SerializeField] private bool startSkillsTreeOnStart = false;
    [SerializeField] private float startDelay = 0f;
    
    [Header("UI Setup (Auto Generate Mode)")]
    [SerializeField] private Transform _skillNodesParent;
    [SerializeField] private GameObject _skillNodePrefab;
    [SerializeField] private GridLayoutGroup _gridLayout;
    [SerializeField] private bool _useGridLayout = false;
    [SerializeField] private bool _useSkillPositions = true;
    
    [Header("Line Rendering")]
    [SerializeField] private bool _autoGenerateLines = true;
    [SerializeField] private GameObject _connectionLinePrefab;
    [SerializeField] private Transform _linesParent;
    [SerializeField] private Color _lockedLineColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    [SerializeField] private Color _unlockedLineColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    [SerializeField] private float _lineWidth = 3f;

    [Header("Tooltip Panel")]
    [SerializeField] private SkillTooltip _skillTooltip;
    
    [Header("Details Panel")]
    [SerializeField] private GameObject _detailsPanel;
    [SerializeField] private SkillTooltip _detailsPanelTooltip;

    [Header("Details Panel UI (Legacy - Optional)")]
    [SerializeField] private TextMeshProUGUI _detailsTitle;
    [SerializeField] private TextMeshProUGUI _detailsDescription;
    [SerializeField] private Image _detailsIcon;
    [SerializeField] private TextMeshProUGUI _detailsCost;
    [SerializeField] private TextMeshProUGUI _detailsLevel;
    [SerializeField] private Button _unlockButton;
    [SerializeField] private TextMeshProUGUI _unlockButtonText;
    [SerializeField] private TextMeshProUGUI _skillPointsText;
    
    [Header("Close Button")]
    [SerializeField] private Button _closeButton;
    [SerializeField] private bool _allowCloseWithKey = true;
    [SerializeField] private KeyCode _closeKey = KeyCode.Escape;
    
    [Header("Player Control")]
    [SerializeField] private GameObject _playerObject;
    [SerializeField] private bool _disablePlayerWhenOpen = true;
    
    private IPlayerController _playerController;
    
    private Dictionary<Skill, SkillNode> _skillNodes = new Dictionary<Skill, SkillNode>();
    private Dictionary<Skill, List<LineRenderer>> _connectionLines = new Dictionary<Skill, List<LineRenderer>>();
    private List<SkillNode> _instantiatedNodes = new List<SkillNode>();
    private SkillNode _currentlySelectedNode;
    private Skill _selectedSkill;
    
    private void Start()
    {
        if (_detailsPanel != null)
            _detailsPanel.SetActive(false);
        
        if (_unlockButton != null)
            _unlockButton.onClick.AddListener(OnUnlockButtonClicked);
        
        if (_closeButton != null)
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
        
        if (skillstreeManager == null)
            skillstreeManager = SkillTreeManager.Instance;

        if (skillstreeManager != null)
        {
            skillstreeManager.OnSkillUnlocked += OnSkillUnlocked;
            skillstreeManager.OnSkillLevelUp += OnSkillLevelUp;
            skillstreeManager.OnSkillPointsChanged += OnSkillPointsChanged;
        }
        
        if (_skillTooltip == null)
        {
            _skillTooltip = GetComponent<SkillTooltip>();
        }
        
        if (_skillTooltip != null)
        {
            _skillTooltip.gameObject.SetActive(false);
        }
        
        // Find player controller
        if (_playerObject != null && _disablePlayerWhenOpen)
        {
            _playerController = _playerObject.GetComponent<IPlayerController>();
            if (_playerController == null)
            {
                Debug.LogWarning("[SkillsTreeController] Player object doesn't implement IPlayerController interface!");
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
                    Debug.LogWarning("[SkillsTreeController] Player doesn't implement IPlayerController interface!");
                }
            }
        }
        
        // FIXED: Only initialize if startSkillsTreeOnStart is TRUE
        if (startSkillsTreeOnStart)
        {
            if (startDelay > 0)
                Invoke(nameof(InitializeSkillsTree), startDelay);
            else
                InitializeSkillsTree();
        }
        else
        {
            // IMPORTANT: Hide the UI if we're not auto-starting
            if (skillstreeUI != null)
                skillstreeUI.SetActive(false);
                
            Debug.Log("[SkillsTreeController] Skill tree UI initialized but hidden. Waiting for trigger.");
        }
        
        UpdateSkillPointsDisplay();
    }
    
    private void Update()
    {
        // Handle close key input when skill tree is open
        if (_isSkillTreeOpen && _allowCloseWithKey)
        {
            if (Input.GetKeyDown(_closeKey))
            {
                HideSkillTree();
            }
        }
    }
    
    private void OnDestroy()
    {
        if (skillstreeManager != null)
        {
            skillstreeManager.OnSkillUnlocked -= OnSkillUnlocked;
            skillstreeManager.OnSkillLevelUp -= OnSkillLevelUp;
            skillstreeManager.OnSkillPointsChanged -= OnSkillPointsChanged;
        }
    }
    
    private void InitializeSkillsTree()
    {
        if (skillstreeUI != null)
            skillstreeUI.SetActive(true);
        
        InitializeSkillTree();
    }
    
    private void InitializeSkillTree()
    {
        switch (_setupMode)
        {
            case SkillTreeSetupMode.AutoGenerate:
                GenerateSkillTree();
                break;
            case SkillTreeSetupMode.ManualAssignment:
                SetupManualSkillNodes();
                break;
        }
    }
    
    #region Public Skill Access Methods
    
    public List<Skill> GetAvailableSkills()
    {
        if (skillstreeContainer == null)
        {
            Debug.LogWarning("[SkillsTreeController] No SkillsTreeContainer assigned!");
            return new List<Skill>();
        }
        
        List<Skill> skills = GetSkillsToGenerate();
        return skills ?? new List<Skill>();
    }
    
    public Skill GetSkillByName(string skillName)
    {
        List<Skill> availableSkills = GetAvailableSkills();
        return availableSkills.Find(s => s.SkillName == skillName);
    }
    
    public void RegisterManualNode(Skill skill, SkillNode node)
    {
        if (skill != null && node != null)
        {
            _skillNodes[skill] = node;
            RegisterSkillNode(node);
        }
    }
    
    #endregion
    
    #region Manual Setup Mode
    
    private void SetupManualSkillNodes()
    {
        _skillNodes.Clear();
        ClearRegisteredNodes();
        
        SkillNode[] allNodes = FindObjectsOfType<SkillNode>();
        
        foreach (var node in allNodes)
        {
            Skill skill = node.GetSkill();
            if (skill != null)
            {
                node.SetController(this);
                _skillNodes[skill] = node;
                RegisterSkillNode(node);
            }
        }
        
        if (_manualSkillNodes != null && _manualSkillNodes.Count > 0)
        {
            foreach (var mapping in _manualSkillNodes)
            {
                if (mapping.skill == null || mapping.nodeGameObject == null)
                    continue;
                
                SkillNode nodeComponent = mapping.nodeGameObject.GetComponent<SkillNode>();
                if (nodeComponent == null)
                    nodeComponent = mapping.nodeGameObject.AddComponent<SkillNode>();
                
                nodeComponent.SetController(this);
                nodeComponent.SetSkill(mapping.skill);
                
                _skillNodes[mapping.skill] = nodeComponent;
                RegisterSkillNode(nodeComponent);
            }
        }
        
        foreach (var skill in _skillNodes.Keys)
        {
            if (_autoGenerateLines)
                CreateConnectionLines(skill);
        }
        
        UpdateAllNodeStates();
        
        Debug.Log($"[SkillsTreeController] Initialized {_skillNodes.Count} manual skill nodes");
    }
    
    #endregion
    
    #region Auto Generate Mode
    
    private void GenerateSkillTree()
    {
        if (skillstreeContainer == null)
        {
            Debug.LogError("[SkillsTreeController] No skill tree container assigned!");
            return;
        }
        
        ClearSkillTree();
        
        List<Skill> skillsToGenerate = GetSkillsToGenerate();
        
        if (skillsToGenerate == null || skillsToGenerate.Count == 0)
        {
            Debug.LogWarning("[SkillsTreeController] No skills to generate!");
            return;
        }
        
        Debug.Log($"[SkillsTreeController] Generating {skillsToGenerate.Count} skill nodes...");
        
        foreach (var skill in skillsToGenerate)
        {
            CreateSkillNode(skill);
        }
        
        if (_autoGenerateLines)
        {
            foreach (var skill in skillsToGenerate)
            {
                CreateConnectionLines(skill);
            }
        }
        
        UpdateAllNodeStates();
        
        Debug.Log($"[SkillsTreeController] Successfully generated {_skillNodes.Count} skill nodes");
    }
    
    private List<Skill> GetSkillsToGenerate()
    {
        if (groupedSkillsTrees && skillstreeGroup != null)
        {
            return skillstreeContainer.GetGroupedSkills(skillstreeGroup, startingSkillsTreesOnly);
        }
        else if (skillstree != null)
        {
            List<Skill> allSkills = new List<Skill>();
            CollectSkillTreeRecursive(skillstree, allSkills);
            return allSkills;
        }
        else
        {
            if (startingSkillsTreesOnly)
                return skillstreeContainer.GetStartingSkills();
            else
                return skillstreeContainer.GetAllSkills();
        }
    }
    
    private void CollectSkillTreeRecursive(Skill skill, List<Skill> collection)
    {
        if (skill == null || collection.Contains(skill))
            return;
        
        collection.Add(skill);
        
        foreach (var child in skill.Children)
        {
            CollectSkillTreeRecursive(child, collection);
        }
    }
    
    private void CreateSkillNode(Skill skill)
    {
        if (_skillNodePrefab == null || _skillNodesParent == null)
        {
            Debug.LogError("[SkillsTreeController] Skill node prefab or parent not assigned!");
            return;
        }
        
        GameObject nodeObj = Instantiate(_skillNodePrefab, _skillNodesParent);
        nodeObj.name = $"SkillNode_{skill.SkillName}";
        
        SkillNode nodeComponent = nodeObj.GetComponent<SkillNode>();
        
        if (nodeComponent == null)
            nodeComponent = nodeObj.AddComponent<SkillNode>();
        
        nodeComponent.Initialize(skill, this);
        
        if (!_useGridLayout && _useSkillPositions)
        {
            RectTransform rectTransform = nodeObj.GetComponent<RectTransform>();
            if (rectTransform != null)
                rectTransform.anchoredPosition = skill.Position;
        }
        
        _skillNodes[skill] = nodeComponent;
        RegisterSkillNode(nodeComponent);
        
        Debug.Log($"[SkillsTreeController] Created node for skill: {skill.SkillName}");
    }
    
    #endregion
    
    #region Connection Lines
    
    private void CreateConnectionLines(Skill skill)
    {
        if (!_skillNodes.ContainsKey(skill))
            return;
        
        List<LineRenderer> lines = new List<LineRenderer>();
        
        foreach (var childSkill in skill.Children)
        {
            if (childSkill == null || !_skillNodes.ContainsKey(childSkill))
                continue;
            
            LineRenderer line = CreateLine(skill, childSkill);
            if (line != null)
                lines.Add(line);
        }
        
        if (lines.Count > 0)
            _connectionLines[skill] = lines;
    }
    
    private LineRenderer CreateLine(Skill fromSkill, Skill toSkill)
    {
        Transform linesContainer = _linesParent != null ? _linesParent : _skillNodesParent;
        
        GameObject lineObj;
        if (_connectionLinePrefab != null)
            lineObj = Instantiate(_connectionLinePrefab, linesContainer);
        else
        {
            lineObj = new GameObject($"Line_{fromSkill.SkillName}_to_{toSkill.SkillName}");
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
        
        Vector3 startPos = _skillNodes[fromSkill].transform.position;
        Vector3 endPos = _skillNodes[toSkill].transform.position;
        
        line.SetPosition(0, startPos);
        line.SetPosition(1, endPos);
        
        UpdateLineColor(line, fromSkill);
        
        return line;
    }
    
    private void UpdateLineColor(LineRenderer line, Skill skill)
    {
        if (line == null)
            return;
        
        Color color = skill.IsUnlocked ? _unlockedLineColor : _lockedLineColor;
        line.startColor = color;
        line.endColor = color;
    }

    #endregion

    #region Cleanup

    private void ClearSkillTree()
    {
        if (_setupMode == SkillTreeSetupMode.AutoGenerate)
        {
            foreach (var node in _skillNodes.Values)
            {
                if (node != null)
                    Destroy(node.gameObject);
            }
        }

        _skillNodes.Clear();
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
    
    public void ShowSkillDetails(Skill skill)
    {
        if (skill == null)
        {
            Debug.LogWarning("[SkillsTreeController] Cannot show details for null skill!");
            return;
        }

        Debug.Log($"[SkillsTreeController] ShowSkillDetails() called for: {skill.SkillName}");
        
        _selectedSkill = skill;

        if (skillstreeUI != null && !skillstreeUI.activeSelf)
        {
            skillstreeUI.SetActive(true);
        }

        if (_detailsPanel != null)
        {
            _detailsPanel.SetActive(true);
            Debug.Log("[SkillsTreeController] ✓ Details panel activated");
        }
        else
        {
            Debug.LogWarning("[SkillsTreeController] Details panel reference is not set!");
            return;
        }
        
        UpdateNodeSelectionStates(skill);
        UpdateDetailsUI(skill);
        UpdateUnlockButton();
    }
    
    private void UpdateNodeSelectionStates(Skill selectedSkill)
    {
        foreach (var node in _instantiatedNodes)
        {
            if (node != null)
            {
                bool isSelected = node.GetSkill() == selectedSkill;
                node.SetSelected(isSelected);
                
                if (isSelected)
                    _currentlySelectedNode = node;
            }
        }
    }

    private void UpdateDetailsUI(Skill skill)
    {
        if (_detailsPanelTooltip != null)
        {
            _detailsPanelTooltip.SetSkillData(skill, this);
        }
        else
        {
            if (_detailsTitle != null)
                _detailsTitle.text = skill.SkillName;
            
            if (_detailsDescription != null)
                _detailsDescription.text = skill.Description;
            
            if (_detailsIcon != null && skill.Icon != null)
                _detailsIcon.sprite = skill.Icon;
            
            if (_detailsCost != null)
                _detailsCost.text = $"Cost: {skill.UnlockCost} SP";
            
            if (_detailsLevel != null)
                _detailsLevel.text = $"Level: {skill.CurrentLevel} / {skill.MaxLevel}";
        }
    }

    public void HideSkillDetails()
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
        _selectedSkill = null;
    }

    public void RegisterSkillNode(SkillNode node)
    {
        if (node != null && !_instantiatedNodes.Contains(node))
            _instantiatedNodes.Add(node);
    }
    
    public void UnregisterSkillNode(SkillNode node)
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
        if (_selectedSkill != null)
            ShowSkillDetails(_selectedSkill);
    }

    private void UpdateUnlockButton()
    {
        if (_unlockButton == null || _selectedSkill == null)
            return;
        
        if (skillstreeManager == null)
        {
            _unlockButton.interactable = false;
            return;
        }
        
        bool canAfford = skillstreeManager.CurrentSkillPoints >= _selectedSkill.UnlockCost;
        bool canUnlock = _selectedSkill.CanUnlock();
        
        if (!_selectedSkill.IsUnlocked)
        {
            _unlockButton.interactable = canAfford && canUnlock;
            if (_unlockButtonText != null)
                _unlockButtonText.text = canAfford && canUnlock ? "Unlock" : "Locked";
        }
        else if (_selectedSkill.CurrentLevel < _selectedSkill.MaxLevel)
        {
            _unlockButton.interactable = canAfford;
            if (_unlockButtonText != null)
                _unlockButtonText.text = canAfford ? "Level Up" : "Level Up";
        }
        else
        {
            _unlockButton.interactable = false;
            if (_unlockButtonText != null)
                _unlockButtonText.text = "Max Level";
        }
    }

    private void OnUnlockButtonClicked()
    {
        if (_selectedSkill == null || skillstreeManager == null)
            return;

        bool success = false;

        if (!_selectedSkill.IsUnlocked)
        {
            success = skillstreeManager.TryUnlockSkill(_selectedSkill);
        }
        else if (_selectedSkill.CurrentLevel < _selectedSkill.MaxLevel)
        {
            success = skillstreeManager.TryLevelUpSkill(_selectedSkill);
        }

        if (success)
        {
            ShowSkillDetails(_selectedSkill);
        }
    }
    
    private void OnCloseButtonClicked()
    {
        HideSkillTree();
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnSkillUnlocked(Skill skill)
    {
        UpdateNodeState(skill);
        UpdateConnectionLines(skill);
        
        if (_selectedSkill == skill)
            ShowSkillDetails(skill);
            
        if (_skillNodes.ContainsKey(skill))
        {
            _skillNodes[skill].PlayUnlockAnimation();
        }
    }
    
    private void OnSkillLevelUp(Skill skill)
    {
        UpdateNodeState(skill);
        
        if (_selectedSkill == skill)
            ShowSkillDetails(skill);
    }
    
    private void OnSkillPointsChanged(int newAmount)
    {
        UpdateSkillPointsDisplay();
        
        if (_selectedSkill != null)
            UpdateUnlockButton();
        
        UpdateAllNodeStates();
    }
    
    #endregion
    
    #region Update Methods
    
    private void UpdateSkillPointsDisplay()
    {
        if (_skillPointsText != null && skillstreeManager != null)
        {
            _skillPointsText.text = $"Skill Points: {skillstreeManager.CurrentSkillPoints}";
        }
    }
    
    private void UpdateNodeState(Skill skill)
    {
        if (_skillNodes.ContainsKey(skill))
        {
            _skillNodes[skill].UpdateDisplay();
        }
    }
    
    private void UpdateConnectionLines(Skill skill)
    {
        if (_connectionLines.ContainsKey(skill))
        {
            foreach (var line in _connectionLines[skill])
            {
                UpdateLineColor(line, skill);
            }
        }
    }
    
    private void UpdateAllNodeStates()
    {
        foreach (var node in _skillNodes.Values)
        {
            if (node != null)
                node.UpdateDisplay();
        }
    }
    
    #endregion
    
    #region Public Methods
    
    public void RefreshSkillTree()
    {
        InitializeSkillTree();
    }
    
    public void SetSkillTreeContainer(SkillsTreeContainer container)
    {
        skillstreeContainer = container;
        InitializeSkillTree();
    }
    
    public void AddManualSkillNode(Skill skill, GameObject nodeGameObject)
    {
        if (skill == null || nodeGameObject == null)
        {
            Debug.LogWarning("[SkillsTreeController] Cannot add null skill or GameObject!");
            return;
        }
        
        _manualSkillNodes.Add(new SkillNodeMapping { skill = skill, nodeGameObject = nodeGameObject });
        
        if (_skillNodes != null && _setupMode == SkillTreeSetupMode.ManualAssignment)
        {
            SkillNode nodeComponent = nodeGameObject.GetComponent<SkillNode>();
            if (nodeComponent == null)
                nodeComponent = nodeGameObject.AddComponent<SkillNode>();
            
            nodeComponent.Initialize(skill, this);
            _skillNodes[skill] = nodeComponent;
            RegisterSkillNode(nodeComponent);
            
            if (_autoGenerateLines)
                CreateConnectionLines(skill);
        }
    }

    public GameObject GetSkillNodeGameObject(Skill skill)
    {
        if (_skillNodes.ContainsKey(skill))
            return _skillNodes[skill].gameObject;
        return null;
    }

    public void ShowSkillTree()
    {
        Debug.Log("[SkillsTreeController] ShowSkillTree() called");
        
        if (skillstreeUI != null)
        {
            skillstreeUI.SetActive(true);
            _isSkillTreeOpen = true;
            Debug.Log("[SkillsTreeController] ✓ Main skill tree UI activated");
        }
        else
        {
            Debug.LogWarning("[SkillsTreeController] skillstreeUI is not assigned!");
        }
        
        if (_detailsPanel != null)
        {
            _detailsPanel.SetActive(false);
            Debug.Log("[SkillsTreeController] Details panel hidden (will show on skill selection)");
        }
        
        if (_skillNodes == null || _skillNodes.Count == 0)
        {
            Debug.Log("[SkillsTreeController] Initializing skill tree nodes...");
            InitializeSkillTree();
        }
        
        UpdateAllNodeStates();
        UpdateSkillPointsDisplay();
        
        // Disable player movement
        if (_disablePlayerWhenOpen && _playerController != null)
        {
            _playerController.DisablePlayer();
            Debug.Log("[SkillsTreeController] Player movement disabled");
        }
    }

    public void HideSkillTree()
    {
        if (skillstreeUI != null)
        {
            skillstreeUI.SetActive(false);
            _isSkillTreeOpen = false;
            Debug.Log("[SkillsTreeController] Skill tree UI hidden");
        }
        
        if (_detailsPanel != null)
        {
            _detailsPanel.SetActive(false);
        }
        
        if (_currentlySelectedNode != null)
        {
            _currentlySelectedNode.SetSelected(false);
            _currentlySelectedNode = null;
        }
        
        _selectedSkill = null;
        
        // Re-enable player movement
        if (_disablePlayerWhenOpen && _playerController != null)
        {
            _playerController.EnablePlayer();
            Debug.Log("[SkillsTreeController] Player movement enabled");
        }
    }

    public void ToggleSkillTree()
    {
        if (skillstreeUI != null)
        {
            if (skillstreeUI.activeSelf)
                HideSkillTree();
            else
                ShowSkillTree();
        }
    }

    public void SetSkillGroup(SkillsTreeGroup group)
    {
        skillstreeGroup = group;
        InitializeSkillTree();
        Debug.Log($"[SkillsTreeController] Filtered to group: {group?.GroupName ?? "None"}");
    }

    public void FilterByGroup(string groupName)
    {
        if (skillstreeContainer == null)
        {
            Debug.LogWarning("[SkillsTreeController] No container assigned to filter by group!");
            return;
        }
        
        foreach (var kvp in skillstreeContainer.Groups)
        {
            if (kvp.Key.GroupName == groupName)
            {
                SetSkillGroup(kvp.Key);
                return;
            }
        }
        
        Debug.LogWarning($"[SkillsTreeController] Could not find group with name: {groupName}");
    }

    public SkillsTreeGroup GetSkillGroup(string groupName)
    {
        if (skillstreeContainer == null)
            return null;
        
        foreach (var kvp in skillstreeContainer.Groups)
        {
            if (kvp.Key.GroupName == groupName)
                return kvp.Key;
        }
        
        return null;
    }

    public void ClearGroupFilter()
    {
        skillstreeGroup = null;
        InitializeSkillTree();
        Debug.Log("[SkillsTreeController] Group filter cleared - showing all skills");
    }

    #endregion
}

public enum SkillTreeSetupMode
{
    AutoGenerate,
    ManualAssignment
}

[Serializable]
public class SkillNodeMapping
{
    [Tooltip("The skill data")]
    public Skill skill;
    
    [Tooltip("The GameObject in the scene that represents this skill node")]
    public GameObject nodeGameObject;
}