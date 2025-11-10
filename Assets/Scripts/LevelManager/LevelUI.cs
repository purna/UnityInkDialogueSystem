using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the level UI display
/// </summary>
public class LevelUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject _levelPanel;
    [SerializeField] private Transform _levelNodesContainer;
    
    [Header("Prefabs")]
    [SerializeField] private LevelNodeUI _levelNodePrefab;
    [SerializeField] private GameObject _connectionLinePrefab;
    
    [Header("Level Details Panel")]
    [SerializeField] private GameObject _detailsPanel;
    [SerializeField] private TextMeshProUGUI _levelNameText;
    [SerializeField] private TextMeshProUGUI _levelDescriptionText;
    [SerializeField] private TextMeshProUGUI _levelCostText;
    [SerializeField] private TextMeshProUGUI _levelLevelText;
    [SerializeField] private Image _levelIcon;
    [SerializeField] private Button _unlockButton;
    
    [Header("Level Points Display")]
    [SerializeField] private TextMeshProUGUI _levelPointsText;
    
    [Header("References")]
    [SerializeField] private LevelManager _levelManager;
    
    private Dictionary<Level, LevelNodeUI> _levelNodes = new Dictionary<Level, LevelNodeUI>();
    private Level _selectedLevel;
    
    private void Start()
    {
        if (_levelManager == null)
            _levelManager = LevelManager.Instance;
        
        if (_levelManager != null)
        {
            _levelManager.OnLevelUnlocked += OnLevelUnlocked;
            _levelManager.OnLevelLevelUp += OnLevelLevelUp;
            _levelManager.OnLevelPointsChanged += OnLevelPointsChanged;
        }
        
        if (_unlockButton != null)
            _unlockButton.onClick.AddListener(OnUnlockButtonClicked);
        
        if (_detailsPanel != null)
            _detailsPanel.SetActive(false);
        
        if (_levelPanel != null)
            _levelPanel.SetActive(false);
        
        UpdateLevelPointsDisplay();
    }
    
    private void OnDestroy()
    {
        if (_levelManager != null)
        {
            _levelManager.OnLevelUnlocked -= OnLevelUnlocked;
            _levelManager.OnLevelLevelUp -= OnLevelLevelUp;
            _levelManager.OnLevelPointsChanged -= OnLevelPointsChanged;
        }
    }
    
    public void OpenLevel()
    {
        if (_levelPanel != null)
            _levelPanel.SetActive(true);
        
        if (_levelNodes.Count == 0)
            GenerateLevel();
    }
    
    public void CloseLevel()
    {
        if (_levelPanel != null)
            _levelPanel.SetActive(false);
        
        if (_detailsPanel != null)
            _detailsPanel.SetActive(false);
    }
    
    private void GenerateLevel()
    {
        if (_levelManager == null || _levelManager.LevelContainer == null)
        {
            Debug.LogWarning("[LevelUI] No level level container found!");
            return;
        }
        
        ClearLevel();
        
        var allLevels = _levelManager.LevelContainer.GetAllLevels();
        
        // Create level nodes
        foreach (var level in allLevels)
        {
            CreateLevelNode(level);
        }
        
        // Create connections
        foreach (var level in allLevels)
        {
            CreateConnections(level);
        }
        
        UpdateAllNodes();
    }
    
    private void CreateLevelNode(Level level)
    {
        if (_levelNodePrefab == null || _levelNodesContainer == null)
            return;

        LevelNodeUI nodeUI = Instantiate(_levelNodePrefab, _levelNodesContainer);
        nodeUI.Initialize(level, this);
        nodeUI.GetComponent<RectTransform>().anchoredPosition = level.Position;

        _levelNodes[level] = nodeUI;
    }
    
    private void CreateConnections(Level level)
    {
        if (_connectionLinePrefab == null || !_levelNodes.ContainsKey(level))
            return;

        foreach (var child in level.Children)
        {
            if (child != null && _levelNodes.ContainsKey(child))
            {
                // Create line renderer or UI line between parent and child
                GameObject line = Instantiate(_connectionLinePrefab, _levelNodesContainer);
                line.transform.SetAsFirstSibling(); // Draw lines behind nodes

                // Position line between level and child
                RectTransform lineRect = line.GetComponent<RectTransform>();
                RectTransform levelRect = _levelNodes[level].GetComponent<RectTransform>();
                RectTransform childRect = _levelNodes[child].GetComponent<RectTransform>();

                if (lineRect != null)
                {
                    Vector2 start = levelRect.anchoredPosition;
                    Vector2 end = childRect.anchoredPosition;
                    Vector2 direction = end - start;

                    lineRect.anchoredPosition = (start + end) / 2f;
                    lineRect.sizeDelta = new Vector2(direction.magnitude, lineRect.sizeDelta.y);
                    lineRect.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                }
            }
        }
    }
    
    private void ClearLevel()
    {
        foreach (var node in _levelNodes.Values)
        {
            if (node != null)
                Destroy(node.gameObject);
        }
        
        _levelNodes.Clear();
        
        // Clear connection lines
        foreach (Transform child in _levelNodesContainer)
        {
            Destroy(child.gameObject);
        }
    }
    
    public void ShowLevelDetails(Level level)
    {
        if (level == null || _detailsPanel == null)
            return;

        _selectedLevel = level;
        _detailsPanel.SetActive(true);

        if (_levelNameText != null)
            _levelNameText.text = level.LevelName;

        if (_levelDescriptionText != null)
            _levelDescriptionText.text = level.Description;

        if (_levelCostText != null)
            _levelCostText.text = $"Cost: {level.UnlockCost} SP";

        if (_levelLevelText != null)
            _levelLevelText.text = $"Level: {level.CurrentLevel} / {level.MaxLevel}";

        if (_levelIcon != null)
            _levelIcon.sprite = level.Icon;

        UpdateUnlockButton();
    }
    
    private void UpdateUnlockButton()
    {
        if (_unlockButton == null || _selectedLevel == null)
            return;
        
        bool canUnlock = _selectedLevel.CanUnlock() && 
                        _levelManager.CurrentLevelPoints >= _selectedLevel.UnlockCost;
        
        _unlockButton.interactable = canUnlock;
        
        TextMeshProUGUI buttonText = _unlockButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            if (_selectedLevel.IsUnlocked)
            {
                if (_selectedLevel.CurrentLevel < _selectedLevel.MaxLevel)
                    buttonText.text = "Level Up";
                else
                    buttonText.text = "Max Level";
            }
            else
            {
                buttonText.text = "Unlock";
            }
        }
    }
    
    private void OnUnlockButtonClicked()
    {
        if (_selectedLevel == null || _levelManager == null)
            return;
        
        if (!_selectedLevel.IsUnlocked)
        {
            _levelManager.TryUnlockLevel(_selectedLevel);
        }
        else if (_selectedLevel.CurrentLevel < _selectedLevel.MaxLevel)
        {
            _levelManager.TryLevelUpLevel(_selectedLevel);
        }
    }
    
    private void OnLevelUnlocked(Level level)
    {
        UpdateNodeDisplay(level);
        UpdateAllNodes();

        if (_selectedLevel == level)
            ShowLevelDetails(level);
    }

    private void OnLevelLevelUp(Level level)
    {
        UpdateNodeDisplay(level);

        if (_selectedLevel == level)
            ShowLevelDetails(level);
    }
    
    private void OnLevelPointsChanged(int newAmount)
    {
        UpdateLevelPointsDisplay();
        UpdateUnlockButton();
        UpdateAllNodes();
    }
    
    private void UpdateLevelPointsDisplay()
    {
        if (_levelPointsText != null && _levelManager != null)
        {
            _levelPointsText.text = $"Level Points: {_levelManager.CurrentLevelPoints}";
        }
    }
    
    private void UpdateNodeDisplay(Level level)
    {
        if (_levelNodes.ContainsKey(level))
        {
            _levelNodes[level].UpdateDisplay();
        }
    }
    
    private void UpdateAllNodes()
    {
        foreach (var node in _levelNodes.Values)
        {
            node.UpdateDisplay();
        }
    }
}