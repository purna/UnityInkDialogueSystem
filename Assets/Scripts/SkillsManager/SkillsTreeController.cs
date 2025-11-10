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

    // Optional: Legacy UI elements (use if NOT using SkillTooltip)
    [Header("Details Panel UI (Legacy - Optional)")]
    [SerializeField] private TextMeshProUGUI _detailsTitle;
    [SerializeField] private TextMeshProUGUI _detailsDescription;
    [SerializeField] private Image _detailsIcon;
    [SerializeField] private TextMeshProUGUI _detailsCost;
    [SerializeField] private TextMeshProUGUI _detailsLevel;
    [SerializeField] private Button _unlockButton;
    [SerializeField] private TextMeshProUGUI _unlockButtonText;
    
    [Header("Skill Points Display")]
    [SerializeField] private TextMeshProUGUI _skillPointsText;
    
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
        
        // Use the reference if provided, otherwise try to find instance
        if (skillstreeManager == null)
            skillstreeManager = SkillTreeManager.Instance;

        // Subscribe to skill tree manager events
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
        
        // Hide the tooltip by default
        if (_skillTooltip != null)
        {
            _skillTooltip.gameObject.SetActive(false);
        }
        
        if (startSkillsTreeOnStart)
        {
            if (startDelay > 0)
                Invoke(nameof(InitializeSkillsTree), startDelay);
            else
                InitializeSkillsTree();
        }
        else
        {
            InitializeSkillTree();
        }
        
        UpdateSkillPointsDisplay();
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
    
    /// <summary>
    /// Get all available skills from the current container/group configuration
    /// This is used by SkillNode to populate the skill selection dropdown
    /// </summary>
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
    
    /// <summary>
    /// Get a skill by its name
    /// </summary>
    public Skill GetSkillByName(string skillName)
    {
        List<Skill> availableSkills = GetAvailableSkills();
        return availableSkills.Find(s => s.SkillName == skillName);
    }
    
    /// <summary>
    /// Register a manually assigned node
    /// </summary>
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
        
        // First, find all SkillNode components in the scene
        SkillNode[] allChoicers = FindObjectsOfType<SkillNode>();
        
        foreach (var choicer in allChoicers)
        {
            Skill skill = choicer.GetSkill();
            if (skill != null)
            {
                // Ensure controller is set
                choicer.SetController(this);
                
                // Register the node
                _skillNodes[skill] = choicer;
                RegisterSkillNode(choicer);
            }
        }
        
        // Also process the manual mappings list if it exists
        if (_manualSkillNodes != null && _manualSkillNodes.Count > 0)
        {
            foreach (var mapping in _manualSkillNodes)
            {
                if (mapping.skill == null || mapping.nodeGameObject == null)
                {
                    Debug.LogWarning($"[SkillsTreeController] Skipping invalid mapping");
                    continue;
                }
                
                // Get or add the SkillNode component
                SkillNode nodeComponent = mapping.nodeGameObject.GetComponent<SkillNode>();
                if (nodeComponent == null)
                {
                    nodeComponent = mapping.nodeGameObject.AddComponent<SkillNode>();
                }
                
                // Initialize the node with skill data
                nodeComponent.SetController(this);
                nodeComponent.SetSkill(mapping.skill);
                
                // Store the mapping
                _skillNodes[mapping.skill] = nodeComponent;
                RegisterSkillNode(nodeComponent);
            }
        }
        
        // Generate connection lines if enabled
        foreach (var skill in _skillNodes.Keys)
        {
            if (_autoGenerateLines)
            {
                CreateConnectionLines(skill);
            }
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
        
        // First pass: Create all skill nodes
        foreach (var skill in skillsToGenerate)
        {
            CreateSkillNode(skill);
        }
        
        // Second pass: Create connections after all nodes exist
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
            // Get skills from the selected group
            return skillstreeContainer.GetGroupedSkills(skillstreeGroup, startingSkillsTreesOnly);
        }
        else if (skillstree != null)
        {
            // Single skill tree - get all related skills
            List<Skill> allSkills = new List<Skill>();
            CollectSkillTreeRecursive(skillstree, allSkills);
            return allSkills;
        }
        else
        {
            // Get all skills from container
            if (startingSkillsTreesOnly)
            {
                return skillstreeContainer.GetStartingSkills();
            }
            else
            {
                return skillstreeContainer.GetAllSkills();
            }
        }
    }
    
    /// <summary>
    /// Recursively collect all skills in a skill tree (including children)
    /// </summary>
    private void CollectSkillTreeRecursive(Skill skill, List<Skill> collection)
    {
        if (skill == null || collection.Contains(skill))
            return;
        
        collection.Add(skill);
        
        // Collect all children
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
        {
            nodeComponent = nodeObj.AddComponent<SkillNode>();
        }
        
        // Initialize the node with the skill data
        nodeComponent.Initialize(skill, this);
        
        // Position the node based on settings
        if (!_useGridLayout && _useSkillPositions)
        {
            // Use the skill's saved position
            RectTransform rectTransform = nodeObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = skill.Position;
            }
        }
        // If using grid layout, let Unity's GridLayoutGroup handle positioning
        
        _skillNodes[skill] = nodeComponent;
        RegisterSkillNode(nodeComponent); // IMPORTANT: Register the node
        
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
            {
                lines.Add(line);
            }
        }
        
        if (lines.Count > 0)
        {
            _connectionLines[skill] = lines;
        }
    }
    
    private LineRenderer CreateLine(Skill fromSkill, Skill toSkill)
    {
        Transform linesContainer = _linesParent != null ? _linesParent : _skillNodesParent;
        
        GameObject lineObj;
        if (_connectionLinePrefab != null)
        {
            lineObj = Instantiate(_connectionLinePrefab, linesContainer);
        }
        else
        {
            lineObj = new GameObject($"Line_{fromSkill.SkillName}_to_{toSkill.SkillName}");
            lineObj.transform.SetParent(linesContainer);
        }
        
        // Ensure line is drawn behind nodes
        lineObj.transform.SetAsFirstSibling();
        
        LineRenderer line = lineObj.GetComponent<LineRenderer>();
        if (line == null)
        {
            line = lineObj.AddComponent<LineRenderer>();
        }
        
        // Configure line renderer
        line.positionCount = 2;
        line.startWidth = _lineWidth;
        line.endWidth = _lineWidth;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.sortingOrder = -1;
        
        // Set positions
        Vector3 startPos = _skillNodes[fromSkill].transform.position;
        Vector3 endPos = _skillNodes[toSkill].transform.position;
        
        line.SetPosition(0, startPos);
        line.SetPosition(1, endPos);
        
        // Set color based on unlock state
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
        // Only destroy nodes if in auto-generate mode
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
    
    /// <summary>
    /// Show skill details in the details panel
    /// Called when a skill node is clicked
    /// </summary>
    public void ShowSkillDetails(Skill skill)
    {
        if (skill == null)
        {
            Debug.LogWarning("[SkillsTreeController] Cannot show details for null skill!");
            return;
        }

        _selectedSkill = skill;

        // Activate the details panel
        if (_detailsPanel != null)
        {
            _detailsPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[SkillsTreeController] Details panel reference is not set!");
            return;
        }
        
        // Update node selection states - IMPORTANT!
        UpdateNodeSelectionStates(skill);
        
        // Method 1: Use SkillTooltip component (RECOMMENDED)
        if (_detailsPanelTooltip == null)
        {
            _detailsPanelTooltip = _detailsPanel.GetComponent<SkillTooltip>();
            
            if (_detailsPanelTooltip == null)
            {
                _detailsPanelTooltip = _detailsPanel.GetComponentInChildren<SkillTooltip>();
            }
        }
        
        if (_detailsPanelTooltip != null)
        {
            _detailsPanelTooltip.SetSkillData(skill, this);
        }
        
        // Method 2: Use legacy UI elements (OPTIONAL - only if SkillTooltip not used)
        if (_detailsPanelTooltip == null)
        {
            UpdateLegacyDetailsUI(skill);
        }
        
        UpdateUnlockButton();
    }
    
    /// <summary>
    /// Update legacy details UI elements (if not using SkillTooltip)
    /// </summary>
    private void UpdateLegacyDetailsUI(Skill skill)
    {
        if (_detailsTitle != null)
            _detailsTitle.text = skill.SkillName;
        
        if (_detailsDescription != null)
            _detailsDescription.text = skill.Description;
        
        if (_detailsIcon != null)
        {
            if (skill.IsUnlocked && skill.UnlockedIcon != null)
                _detailsIcon.sprite = skill.UnlockedIcon;
            else if (!skill.IsUnlocked && skill.LockedIcon != null)
                _detailsIcon.sprite = skill.LockedIcon;
            else if (skill.Icon != null)
                _detailsIcon.sprite = skill.Icon;
        }
        
        if (_detailsCost != null)
            _detailsCost.text = $"Cost: {skill.UnlockCost} SP";
        
        if (_detailsLevel != null)
            _detailsLevel.text = $"Level: {skill.CurrentLevel} / {skill.MaxLevel}";
    }

    /// <summary>
    /// Update selection states for all nodes
    /// </summary>
    private void UpdateNodeSelectionStates(Skill selectedSkill)
    {
        foreach (var node in _instantiatedNodes)
        {
            if (node != null)
            {
                bool isSelected = node.GetSkill() == selectedSkill;
                node.SetSelected(isSelected);
                
                if (isSelected)
                {
                    _currentlySelectedNode = node;
                }
            }
        }
    }

    /// <summary>
    /// Hide the details panel
    /// </summary>
    public void HideSkillDetails()
    {
        if (_detailsPanel != null)
        {
            _detailsPanel.SetActive(false);
        }

        // Deselect all nodes
        foreach (var node in _instantiatedNodes)
        {
            if (node != null)
            {
                node.SetSelected(false);
            }
        }

        _currentlySelectedNode = null;
        _selectedSkill = null;
    }

    /// <summary>
    /// Register a skill node (call this when instantiating nodes)
    /// </summary>
    public void RegisterSkillNode(SkillNode node)
    {
        if (node != null && !_instantiatedNodes.Contains(node))
        {
            _instantiatedNodes.Add(node);
        }
    }
    
    /// <summary>
    /// Unregister a skill node (call this when destroying nodes)
    /// </summary>
    public void UnregisterSkillNode(SkillNode node)
    {
        if (node != null && _instantiatedNodes.Contains(node))
        {
            _instantiatedNodes.Remove(node);
        }
    }
    
    /// <summary>
    /// Clear all registered nodes (call this when regenerating the tree)
    /// </summary>
    public void ClearRegisteredNodes()
    {
        _instantiatedNodes.Clear();
        _currentlySelectedNode = null;
    }

    /// <summary>
    /// Refresh the details panel if a skill is currently selected
    /// Useful when skill state changes (e.g., after unlock)
    /// </summary>
    public void RefreshDetailsPanel()
    {
        if (_selectedSkill != null)
        {
            ShowSkillDetails(_selectedSkill);
        }
    }

    private void UpdateUnlockButton()
    {
        if (_unlockButton == null || _selectedSkill == null)
            return;
        
        bool canAfford = skillstreeManager != null && 
                        skillstreeManager.CurrentSkillPoints >= _selectedSkill.UnlockCost;
        bool canUnlock = _selectedSkill.CanUnlock() && canAfford;
        
        if (_selectedSkill.IsUnlocked)
        {
            if (_selectedSkill.CurrentLevel < _selectedSkill.MaxLevel)
            {
                _unlockButton.interactable = canAfford;
                if (_unlockButtonText != null)
                    _unlockButtonText.text = "Level Up";
            }
            else
            {
                _unlockButton.interactable = false;
                if (_unlockButtonText != null)
                    _unlockButtonText.text = "Max Level";
            }
        }
        else
        {
            _unlockButton.interactable = canUnlock;
            if (_unlockButtonText != null)
                _unlockButtonText.text = canUnlock ? "Unlock" : "Locked";
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
            // Refresh the details panel
            ShowSkillDetails(_selectedSkill);
        }
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnSkillUnlocked(Skill skill)
    {
        UpdateNodeState(skill);
        UpdateConnectionLines(skill);
        
        if (_selectedSkill == skill)
            ShowSkillDetails(skill);
            
        // Play unlock animation if node exists
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
            node.UpdateDisplay();
        }
        
        foreach (var skillLines in _connectionLines)
        {
            foreach (var line in skillLines.Value)
            {
                UpdateLineColor(line, skillLines.Key);
            }
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
    
    /// <summary>
    /// Manually add a skill node mapping at runtime
    /// </summary>
    public void AddManualSkillNode(Skill skill, GameObject nodeGameObject)
    {
        if (skill == null || nodeGameObject == null)
        {
            Debug.LogWarning("[SkillsTreeController] Cannot add null skill or GameObject!");
            return;
        }
        
        // Add to manual nodes list
        _manualSkillNodes.Add(new SkillNodeMapping { skill = skill, nodeGameObject = nodeGameObject });
        
        // If already initialized, set it up immediately
        if (_skillNodes != null && _setupMode == SkillTreeSetupMode.ManualAssignment)
        {
            SkillNode nodeComponent = nodeGameObject.GetComponent<SkillNode>();
            if (nodeComponent == null)
            {
                nodeComponent = nodeGameObject.AddComponent<SkillNode>();
            }
            
            nodeComponent.Initialize(skill, this);
            _skillNodes[skill] = nodeComponent;
            RegisterSkillNode(nodeComponent);
            
            if (_autoGenerateLines)
            {
                CreateConnectionLines(skill);
            }
        }
    }

    /// <summary>
    /// Get the node GameObject for a specific skill
    /// </summary>
    public GameObject GetSkillNodeGameObject(Skill skill)
    {
        if (_skillNodes.ContainsKey(skill))
        {
            return _skillNodes[skill].gameObject;
        }
        return null;
    }

    #endregion

    #region Public UI Control Methods

    /// <summary>
    /// Show/Open the skill tree UI
    /// </summary>
    public void ShowSkillTree()
    {
        if (skillstreeUI != null)
        {
            skillstreeUI.SetActive(true);
            Debug.Log("[SkillsTreeController] Skill tree UI opened");
        }
        else
        {
            Debug.LogWarning("[SkillsTreeController] Skill tree UI reference not set!");
        }

        // Refresh the display
        UpdateAllNodeStates();
        UpdateSkillPointsDisplay();
    }


    /// <summary>
    /// Hide/Close the skill tree UI
    /// </summary>
    public void HideSkillTree()
    {
        if (skillstreeUI != null)
        {
            skillstreeUI.SetActive(false);
            Debug.Log("[SkillsTreeController] Skill tree UI closed");
        }

        // Also hide the details panel
        HideSkillDetails();
    }

/// <summary>
/// Toggle the skill tree UI visibility
/// </summary>
public void ToggleSkillTree()
{
    if (skillstreeUI != null)
    {
        if (skillstreeUI.activeSelf)
        {
            HideSkillTree();
        }
        else
        {
            ShowSkillTree();
        }
    }
}

/// <summary>
/// Set the skill group to display (filters the tree to only show skills from this group)
/// </summary>
public void SetSkillGroup(SkillsTreeGroup group)
{
    skillstreeGroup = group;
    
    // Regenerate the skill tree with the new group
    InitializeSkillTree();
    
    Debug.Log($"[SkillsTreeController] Filtered to group: {group?.GroupName ?? "None"}");
}

/// <summary>
/// Filter the skill tree by group name
/// </summary>
public void FilterByGroup(string groupName)
{
    if (skillstreeContainer == null)
    {
        Debug.LogWarning("[SkillsTreeController] No container assigned to filter by group!");
        return;
    }
    
    // Find the group by name
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

/// <summary>
/// Get a skill group by name from the current container
/// </summary>
public SkillsTreeGroup GetSkillGroup(string groupName)
{
    if (skillstreeContainer == null)
    {
        Debug.LogWarning("[SkillsTreeController] No container assigned!");
        return null;
    }
    
    foreach (var kvp in skillstreeContainer.Groups)
    {
        if (kvp.Key.GroupName == groupName)
        {
            return kvp.Key;
        }
    }
    
    return null;
}

/// <summary>
/// Clear any group filter and show all skills
/// </summary>
public void ClearGroupFilter()
{
    skillstreeGroup = null;
    InitializeSkillTree();
    Debug.Log("[SkillsTreeController] Group filter cleared - showing all skills");
}

#endregion

}

/// <summary>
/// Setup mode for the skill tree
/// </summary>
public enum SkillTreeSetupMode
{
    AutoGenerate,       // Automatically generate nodes from prefab
    ManualAssignment    // Use manually assigned GameObjects
}

/// <summary>
/// Mapping between a Skill and its UI GameObject
/// </summary>
[Serializable]
public class SkillNodeMapping
{
    [Tooltip("The skill data")]
    public Skill skill;
    
    [Tooltip("The GameObject in the scene that represents this skill node")]
    public GameObject nodeGameObject;
}