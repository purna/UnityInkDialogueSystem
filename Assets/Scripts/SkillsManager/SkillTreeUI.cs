using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the skill tree UI display
/// </summary>
public class SkillTreeUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject _skillTreePanel;
    [SerializeField] private Transform _skillNodesContainer;
    
    [Header("Prefabs")]
    [SerializeField] private SkillNodeUI _skillNodePrefab;
    [SerializeField] private GameObject _connectionLinePrefab;
    
    [Header("Skill Details Panel")]
    [SerializeField] private GameObject _detailsPanel;
    [SerializeField] private TextMeshProUGUI _skillNameText;
    [SerializeField] private TextMeshProUGUI _skillDescriptionText;
    [SerializeField] private TextMeshProUGUI _skillCostText;
    [SerializeField] private TextMeshProUGUI _skillLevelText;
    [SerializeField] private Image _skillIcon;
    [SerializeField] private Button _unlockButton;
    
    [Header("Skill Points Display")]
    [SerializeField] private TextMeshProUGUI _skillPointsText;
    
    [Header("References")]
    [SerializeField] private SkillTreeManager _skillTreeManager;
    
    private Dictionary<Skill, SkillNodeUI> _skillNodes = new Dictionary<Skill, SkillNodeUI>();
    private Skill _selectedSkill;
    
    private void Start()
    {
        if (_skillTreeManager == null)
            _skillTreeManager = SkillTreeManager.Instance;
        
        if (_skillTreeManager != null)
        {
            _skillTreeManager.OnSkillUnlocked += OnSkillUnlocked;
            _skillTreeManager.OnSkillLevelUp += OnSkillLevelUp;
            _skillTreeManager.OnSkillPointsChanged += OnSkillPointsChanged;
        }
        
        if (_unlockButton != null)
            _unlockButton.onClick.AddListener(OnUnlockButtonClicked);
        
        if (_detailsPanel != null)
            _detailsPanel.SetActive(false);
        
        if (_skillTreePanel != null)
            _skillTreePanel.SetActive(false);
        
        UpdateSkillPointsDisplay();
    }
    
    private void OnDestroy()
    {
        if (_skillTreeManager != null)
        {
            _skillTreeManager.OnSkillUnlocked -= OnSkillUnlocked;
            _skillTreeManager.OnSkillLevelUp -= OnSkillLevelUp;
            _skillTreeManager.OnSkillPointsChanged -= OnSkillPointsChanged;
        }
    }
    
    public void OpenSkillTree()
    {
        if (_skillTreePanel != null)
            _skillTreePanel.SetActive(true);
        
        if (_skillNodes.Count == 0)
            GenerateSkillTree();
    }
    
    public void CloseSkillTree()
    {
        if (_skillTreePanel != null)
            _skillTreePanel.SetActive(false);
        
        if (_detailsPanel != null)
            _detailsPanel.SetActive(false);
    }
    
    private void GenerateSkillTree()
    {
        if (_skillTreeManager == null || _skillTreeManager.SkillTreeContainer == null)
        {
            Debug.LogWarning("[SkillTreeUI] No skill tree container found!");
            return;
        }
        
        ClearSkillTree();
        
        var allSkills = _skillTreeManager.SkillTreeContainer.GetAllSkills();
        
        // Create skill nodes
        foreach (var skill in allSkills)
        {
            CreateSkillNode(skill);
        }
        
        // Create connections
        foreach (var skill in allSkills)
        {
            CreateConnections(skill);
        }
        
        UpdateAllNodes();
    }
    
    private void CreateSkillNode(Skill skill)
    {
        if (_skillNodePrefab == null || _skillNodesContainer == null)
            return;

        SkillNodeUI nodeUI = Instantiate(_skillNodePrefab, _skillNodesContainer);
        nodeUI.Initialize(skill, this);
        nodeUI.GetComponent<RectTransform>().anchoredPosition = skill.Position;

        _skillNodes[skill] = nodeUI;
    }
    
    private void CreateConnections(Skill skill)
    {
        if (_connectionLinePrefab == null || !_skillNodes.ContainsKey(skill))
            return;

        foreach (var child in skill.Children)
        {
            if (child != null && _skillNodes.ContainsKey(child))
            {
                // Create line renderer or UI line between parent and child
                GameObject line = Instantiate(_connectionLinePrefab, _skillNodesContainer);
                line.transform.SetAsFirstSibling(); // Draw lines behind nodes

                // Position line between skill and child
                RectTransform lineRect = line.GetComponent<RectTransform>();
                RectTransform skillRect = _skillNodes[skill].GetComponent<RectTransform>();
                RectTransform childRect = _skillNodes[child].GetComponent<RectTransform>();

                if (lineRect != null)
                {
                    Vector2 start = skillRect.anchoredPosition;
                    Vector2 end = childRect.anchoredPosition;
                    Vector2 direction = end - start;

                    lineRect.anchoredPosition = (start + end) / 2f;
                    lineRect.sizeDelta = new Vector2(direction.magnitude, lineRect.sizeDelta.y);
                    lineRect.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                }
            }
        }
    }
    
    private void ClearSkillTree()
    {
        foreach (var node in _skillNodes.Values)
        {
            if (node != null)
                Destroy(node.gameObject);
        }
        
        _skillNodes.Clear();
        
        // Clear connection lines
        foreach (Transform child in _skillNodesContainer)
        {
            Destroy(child.gameObject);
        }
    }
    
    public void ShowSkillDetails(Skill skill)
    {
        if (skill == null || _detailsPanel == null)
            return;

        _selectedSkill = skill;
        _detailsPanel.SetActive(true);

        if (_skillNameText != null)
            _skillNameText.text = skill.SkillName;

        if (_skillDescriptionText != null)
            _skillDescriptionText.text = skill.Description;

        if (_skillCostText != null)
            _skillCostText.text = $"Cost: {skill.UnlockCost} SP";

        if (_skillLevelText != null)
            _skillLevelText.text = $"Level: {skill.CurrentLevel} / {skill.MaxLevel}";

        if (_skillIcon != null)
            _skillIcon.sprite = skill.Icon;

        UpdateUnlockButton();
    }
    
    private void UpdateUnlockButton()
    {
        if (_unlockButton == null || _selectedSkill == null)
            return;
        
        bool canUnlock = _selectedSkill.CanUnlock() && 
                        _skillTreeManager.CurrentSkillPoints >= _selectedSkill.UnlockCost;
        
        _unlockButton.interactable = canUnlock;
        
        TextMeshProUGUI buttonText = _unlockButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            if (_selectedSkill.IsUnlocked)
            {
                if (_selectedSkill.CurrentLevel < _selectedSkill.MaxLevel)
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
        if (_selectedSkill == null || _skillTreeManager == null)
            return;
        
        if (!_selectedSkill.IsUnlocked)
        {
            _skillTreeManager.TryUnlockSkill(_selectedSkill);
        }
        else if (_selectedSkill.CurrentLevel < _selectedSkill.MaxLevel)
        {
            _skillTreeManager.TryLevelUpSkill(_selectedSkill);
        }
    }
    
    private void OnSkillUnlocked(Skill skill)
    {
        UpdateNodeDisplay(skill);
        UpdateAllNodes();

        if (_selectedSkill == skill)
            ShowSkillDetails(skill);
    }

    private void OnSkillLevelUp(Skill skill)
    {
        UpdateNodeDisplay(skill);

        if (_selectedSkill == skill)
            ShowSkillDetails(skill);
    }
    
    private void OnSkillPointsChanged(int newAmount)
    {
        UpdateSkillPointsDisplay();
        UpdateUnlockButton();
        UpdateAllNodes();
    }
    
    private void UpdateSkillPointsDisplay()
    {
        if (_skillPointsText != null && _skillTreeManager != null)
        {
            _skillPointsText.text = $"Skill Points: {_skillTreeManager.CurrentSkillPoints}";
        }
    }
    
    private void UpdateNodeDisplay(Skill skill)
    {
        if (_skillNodes.ContainsKey(skill))
        {
            _skillNodes[skill].UpdateDisplay();
        }
    }
    
    private void UpdateAllNodes()
    {
        foreach (var node in _skillNodes.Values)
        {
            node.UpdateDisplay();
        }
    }
}