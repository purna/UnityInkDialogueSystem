using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillTreeController : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;

    [Header("Text Settings")]
    [SerializeField] private string _headerTitleText = "Skill Tree System";
    [SerializeField] private string _instructionText = "Select a skill to view details";

    [System.Serializable]
    public class SkillTreeTheme
    {
        [Header("Node Base")]
        public Color NodeBackground = new Color(0.06f, 0.09f, 0.16f);
        public Color NodeBorderLocked = new Color(0.2f, 0.25f, 0.33f);

        [Header("Node States")]
        public Color StateAvailable = Color.white;
        public Color StateUnlocked = new Color(0.91f, 0.7f, 0.03f);
        public Color StateSelected = new Color(0.23f, 0.51f, 0.96f);

        [Header("Icons")]
        public Color IconLocked = new Color(0.5f, 0.5f, 0.5f);
        public Color IconAvailable = Color.white;
        public Color IconUnlocked = new Color(0.91f, 0.7f, 0.03f);

        [Header("Connections")]
        public Color LineInactive = new Color(0.11f, 0.16f, 0.23f);
        public Color LineActive = new Color(0.98f, 0.75f, 0.14f);

        [Header("UI Elements")]
        public Color CostBadgeBackground = new Color(0.14f, 0.38f, 0.92f);
        public Color CostBadgeText = Color.white;
        public Color TextGreen = new Color(0.13f, 0.77f, 0.36f);
        public Color TextGrey = new Color(0.58f, 0.64f, 0.72f);
    }

    [Header("Visual Settings")]
    [SerializeField] private SkillTreeTheme _theme;

    [System.Serializable]
    public class SkillNodeData
    {
        public string Id;
        public string Name;
        [TextArea] public string Description;
        public Sprite Icon;
        
        [Header("Position %")]
        [Range(0, 100)] public float X;
        [Range(0, 100)] public float Y;
        
        public int Cost;
        public List<string> Parents;
    }

    [Header("Configuration")]
    [SerializeField] private List<SkillNodeData> _skills = new List<SkillNodeData>();

    // State
    private List<string> _unlocked = new List<string>();
    private int _points = 10;
    private string _selectedId = null;

    // UI Elements
    private VisualElement _nodesContainer;
    private VisualElement _connectionsLayer;
    private VisualElement _sidebar;
    private VisualElement _sidebarBackdrop;
    private Button _sidebarCloseButton;
    private Label _pointsLabel;
    private Label _headerTitleLabel;
    private Label _instructionLabel;
    
    // Sidebar Elements
    private VisualElement _sidebarIcon;
    private Label _sidebarName;
    private Label _sidebarStatus;
    private Label _sidebarDesc;
    private Label _sidebarCost;
    private Button _unlockButton;

    // Constants
    private const float NODE_SIZE = 56f;
    private const float NODE_ICON_SIZE = 32f;
    private const float BADGE_SIZE = 16f;
    private const float BADGE_OFFSET = -5f;
    private const int BADGE_FONT_SIZE = 9;
    private const float BORDER_WIDTH_NORMAL = 3f;
    private const float BORDER_WIDTH_SELECTED = 4f;
    private const float CONNECTION_LINE_WIDTH = 3f;

    private void OnEnable()
    {
        if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();
        var root = _uiDocument.rootVisualElement;

        BindUIElements(root);
        SetupEventHandlers();

        if (_unlocked.Count == 0) _unlocked.Add("root");

        // Reset selection state
        _selectedId = null;
        HideSidebar();

        // Wait for layout to build tree
        root.schedule.Execute(() => {
            BuildTree();
            UpdateUI();
        });
    }

    private void BindUIElements(VisualElement root)
    {
        // Bind Main Containers
        _nodesContainer = root.Q("NodesContainer");
        _connectionsLayer = root.Q("ConnectionsLayer");
        
        // Bind Labels
        _pointsLabel = root.Q<Label>("PointsDisplay");
        _headerTitleLabel = root.Q<Label>("HeaderTitle");
        _instructionLabel = root.Q<Label>("InstructionLabel");
        
        // Bind Sidebar
        _sidebar = root.Q("Sidebar");
        _sidebarBackdrop = root.Q("SidebarBackdrop");
        _sidebarCloseButton = root.Q<Button>("SidebarCloseButton");
        _sidebarIcon = root.Q("SidebarIcon");
        _sidebarName = root.Q<Label>("SkillName");
        _sidebarStatus = root.Q<Label>("SkillStatus");
        _sidebarDesc = root.Q<Label>("SkillDescription");
        _sidebarCost = root.Q<Label>("SkillCost");
        _unlockButton = root.Q<Button>("UnlockButton");

        // Set text content
        if (_headerTitleLabel != null) _headerTitleLabel.text = _headerTitleText;
        if (_instructionLabel != null) _instructionLabel.text = _instructionText;
    }

    private void SetupEventHandlers()
    {
        if (_unlockButton != null) _unlockButton.clicked += OnUnlockClicked;
        if (_connectionsLayer != null) _connectionsLayer.generateVisualContent += OnGenerateConnections;
        
        // Add close button handler
        if (_sidebarCloseButton != null) _sidebarCloseButton.clicked += HideSidebar;
        
        // Add backdrop click handler - using MouseDownEvent for better reliability
        if (_sidebarBackdrop != null)
        {
            _sidebarBackdrop.RegisterCallback<MouseDownEvent>(evt => {
                HideSidebar();
                evt.StopPropagation();
            });
        }
    }

    private void HideSidebar()
    {
        _selectedId = null;
        if (_sidebar != null) _sidebar.style.display = DisplayStyle.None;
        if (_sidebarBackdrop != null) _sidebarBackdrop.style.display = DisplayStyle.None;
        
        // Refresh nodes to remove selection highlight
        UpdateUI();
    }

    private void BuildTree()
    {
        _nodesContainer.Clear();

        foreach (var skill in _skills)
        {
            var btn = CreateSkillButton(skill);
            _nodesContainer.Add(btn);
        }
    }

    private Button CreateSkillButton(SkillNodeData skill)
    {
        var btn = new Button();
        btn.AddToClassList("skill-node");
        btn.name = skill.Id;
        
        ApplyButtonStyles(btn, skill);
        
        var nodeContent = CreateNodeContent();
        btn.Add(nodeContent);

        var icon = CreateNodeIcon(skill);
        nodeContent.Add(icon);

        if (skill.Cost > 0)
        {
            var badge = CreateCostBadge(skill.Cost);
            btn.Add(badge);
        }

        btn.clicked += () => SelectSkill(skill.Id);
        
        return btn;
    }

    private void ApplyButtonStyles(Button btn, SkillNodeData skill)
    {
        btn.style.position = Position.Absolute;
        btn.style.left = Length.Percent(skill.X);
        btn.style.top = Length.Percent(skill.Y);
        btn.style.width = NODE_SIZE;
        btn.style.height = NODE_SIZE;
        btn.style.marginLeft = -NODE_SIZE / 2;
        btn.style.marginTop = -NODE_SIZE / 2;
        
        // Reset default button styles
        btn.style.backgroundColor = Color.clear;
        SetBorderWidth(btn, 0);

    }

    private VisualElement CreateNodeContent()
    {
        var nodeContent = new VisualElement();
        nodeContent.name = "NodeContent";
        nodeContent.style.width = Length.Percent(100);
        nodeContent.style.height = Length.Percent(100);
        nodeContent.style.overflow = Overflow.Hidden;
        nodeContent.style.backgroundColor = _theme.NodeBackground;
        
        SetBorderWidth(nodeContent, BORDER_WIDTH_NORMAL);
        SetBorderRadius(nodeContent, NODE_SIZE / 2);
        
        nodeContent.style.justifyContent = Justify.Center;
        nodeContent.style.alignItems = Align.Center;
        
        return nodeContent;
    }

    private VisualElement CreateNodeIcon(SkillNodeData skill)
    {
        var icon = new VisualElement();
        icon.AddToClassList("skill-node-icon");
        icon.style.width = NODE_ICON_SIZE;
        icon.style.height = NODE_ICON_SIZE;
        
        if (skill.Icon != null)
        {
            icon.style.backgroundImage = new StyleBackground(skill.Icon);
        }
        
        return icon;
    }

    private Label CreateCostBadge(int cost)
    {
        var badge = new Label(cost.ToString());
        badge.style.position = Position.Absolute;
        badge.style.top = BADGE_OFFSET;
        badge.style.right = BADGE_OFFSET;
        badge.style.width = BADGE_SIZE;
        badge.style.height = BADGE_SIZE;
        badge.style.backgroundColor = _theme.CostBadgeBackground;
        badge.style.color = _theme.CostBadgeText;
        
        SetBorderRadius(badge, BADGE_SIZE / 2);
        
        badge.style.unityTextAlign = TextAnchor.MiddleCenter;
        badge.style.fontSize = BADGE_FONT_SIZE;
        badge.style.unityFontStyleAndWeight = FontStyle.Bold;
        
        return badge;
    }

    private void UpdateUI()
    {
        if (_pointsLabel != null) _pointsLabel.text = $"{_points} PTS";

        foreach (var skill in _skills)
        {
            UpdateSkillNode(skill);
        }
        
        _connectionsLayer.MarkDirtyRepaint();
    }

    private void UpdateSkillNode(SkillNodeData skill)
    {
        var btn = _nodesContainer.Q<Button>(skill.Id);
        if (btn == null) return;

        var nodeContent = btn.Q<VisualElement>("NodeContent");
        var icon = btn.Q<VisualElement>(className: "skill-node-icon");
        
        string status = GetStatus(skill);
        Color borderColor = GetBorderColor(status, skill.Id == _selectedId);
        Color iconColor = GetIconColor(status);

        if (nodeContent != null)
        {
            float borderWidth = skill.Id == _selectedId ? BORDER_WIDTH_SELECTED : BORDER_WIDTH_NORMAL;
            SetBorderWidth(nodeContent, borderWidth);
            SetBorderColor(nodeContent, borderColor);
        }

        if (icon != null)
        {
            icon.style.unityBackgroundImageTintColor = iconColor;
        }
    }

    private Color GetBorderColor(string status, bool isSelected)
    {
        if (isSelected) return _theme.StateSelected;
        if (status == "unlocked") return _theme.StateUnlocked;
        if (status == "available") return _theme.StateAvailable;
        return _theme.NodeBorderLocked;
    }

    private Color GetIconColor(string status)
    {
        if (status == "unlocked") return _theme.IconUnlocked;
        if (status == "available") return _theme.IconAvailable;
        return _theme.IconLocked;
    }

    private void SetBorderWidth(VisualElement element, float width)
    {
        element.style.borderLeftWidth = width;
        element.style.borderRightWidth = width;
        element.style.borderTopWidth = width;
        element.style.borderBottomWidth = width;
    }

    private void SetBorderColor(VisualElement element, Color color)
    {
        element.style.borderTopColor = color;
        element.style.borderBottomColor = color;
        element.style.borderLeftColor = color;
        element.style.borderRightColor = color;
    }

    private void SetBorderRadius(VisualElement element, float radius)
    {
        element.style.borderTopLeftRadius = radius;
        element.style.borderTopRightRadius = radius;
        element.style.borderBottomLeftRadius = radius;
        element.style.borderBottomRightRadius = radius;
    }

    private void OnGenerateConnections(MeshGenerationContext mgc)
    {
        var painter = mgc.painter2D;
        painter.lineWidth = CONNECTION_LINE_WIDTH;
        painter.lineCap = LineCap.Round;

        float width = _connectionsLayer.contentRect.width;
        float height = _connectionsLayer.contentRect.height;

        // Safety check if layout hasn't happened yet
        if (float.IsNaN(width) || width <= 1) return;

        foreach (var skill in _skills)
        {
            if (skill.Parents == null) continue;
            
            foreach (var parentId in skill.Parents)
            {
                DrawConnection(painter, skill, parentId, width, height);
            }
        }
    }

    private void DrawConnection(Painter2D painter, SkillNodeData skill, string parentId, float width, float height)
    {
        var parent = _skills.Find(s => s.Id == parentId);
        if (parent == null) return;

        Vector2 start = new Vector2((parent.X / 100f) * width, (parent.Y / 100f) * height);
        Vector2 end = new Vector2((skill.X / 100f) * width, (skill.Y / 100f) * height);

        bool isPathActive = _unlocked.Contains(skill.Id) && _unlocked.Contains(parentId);
        
        painter.strokeColor = isPathActive ? _theme.LineActive : _theme.LineInactive;
        painter.BeginPath();
        painter.MoveTo(start);
        painter.LineTo(end);
        painter.Stroke();
    }

    private string GetStatus(SkillNodeData skill)
    {
        if (_unlocked.Contains(skill.Id)) return "unlocked";
        if (skill.Parents == null) skill.Parents = new List<string>();

        if (skill.Id == "ultimate")
        {
            bool hasParent = skill.Parents.Any(p => _unlocked.Contains(p));
            if (hasParent && _points >= skill.Cost) return "available";
            if (hasParent) return "locked-points";
            return "locked-path";
        }

        bool parentsUnlocked = skill.Parents.Count == 0 || skill.Parents.All(p => _unlocked.Contains(p));
        if (!parentsUnlocked) return "locked-path";
        if (_points < skill.Cost) return "locked-points";
        return "available";
    }

    private void SelectSkill(string id)
    {
        _selectedId = id;
        UpdateInspector();
        UpdateUI();
    }

    private void OnUnlockClicked()
    {
        var skill = _skills.Find(s => s.Id == _selectedId);
        if (skill != null && GetStatus(skill) == "available")
        {
            _points -= skill.Cost;
            _unlocked.Add(skill.Id);
            UpdateUI();
            UpdateInspector();
        }
    }

    private void UpdateInspector()
    {
        if (_selectedId == null)
        {
            HideSidebar();
            return;
        }

        if (_sidebar != null) _sidebar.style.display = DisplayStyle.Flex;
        if (_sidebarBackdrop != null) _sidebarBackdrop.style.display = DisplayStyle.Flex;
        
        var skill = _skills.Find(s => s.Id == _selectedId);
        if (skill == null) return;

        UpdateSidebarContent(skill);
    }

    private void UpdateSidebarContent(SkillNodeData skill)
    {
        string status = GetStatus(skill);

        if (_sidebarName != null) _sidebarName.text = skill.Name;
        if (_sidebarDesc != null) _sidebarDesc.text = skill.Description;
        if (_sidebarCost != null) _sidebarCost.text = $"{skill.Cost} Points";

        UpdateSidebarIcon(skill, status);
        UpdateSidebarStatus(status);
        UpdateUnlockButton(status);
    }

    private void UpdateSidebarIcon(SkillNodeData skill, string status)
    {
        if (_sidebarIcon == null) return;

        _sidebarIcon.style.backgroundImage = skill.Icon != null 
            ? new StyleBackground(skill.Icon) 
            : null;
            
        _sidebarIcon.style.unityBackgroundImageTintColor = status == "unlocked" 
            ? _theme.IconUnlocked 
            : Color.white;
    }

    private void UpdateSidebarStatus(string status)
    {
        if (_sidebarStatus == null) return;

        _sidebarStatus.text = status.ToUpper();
        _sidebarStatus.style.color = status == "unlocked" 
            ? _theme.TextGreen 
            : _theme.TextGrey;
    }

    private void UpdateUnlockButton(string status)
    {
        if (_unlockButton == null) return;

        _unlockButton.SetEnabled(status == "available");
        _unlockButton.text = status == "unlocked" ? "Mastered" : "Unlock Skill";
    }


    #if UNITY_EDITOR
    [ContextMenu("Auto-Populate Skills")]
    private void PopulateDefaultData()
    {
        UnityEditor.Undo.RecordObject(this, "Populate Skills");
        _skills = new List<SkillNodeData>();
        
        // Helper to find sprites safely
        Sprite TryFindSprite(string id) => Resources.Load<Sprite>($"SkillIcons/{id}");

        _skills.Add(new SkillNodeData { Id = "root", Name = "Novice", Description = "The beginning.", X = 50, Y = 10, Cost = 0, Icon = TryFindSprite("root"), Parents = new List<string>() });
        _skills.Add(new SkillNodeData { Id = "warrior_1", Name = "Strength", Description = "+10% Phys Dmg", X = 20, Y = 35, Cost = 1, Icon = TryFindSprite("warrior_1"), Parents = new List<string> { "root" } });
        _skills.Add(new SkillNodeData { Id = "warrior_2", Name = "Iron Skin", Description = "-15% Dmg Taken", X = 10, Y = 60, Cost = 2, Icon = TryFindSprite("warrior_2"), Parents = new List<string> { "warrior_1" } });
        _skills.Add(new SkillNodeData { Id = "warrior_3", Name = "Execute", Description = "Crit Low HP", X = 30, Y = 60, Cost = 3, Icon = TryFindSprite("warrior_3"), Parents = new List<string> { "warrior_1" } });
        _skills.Add(new SkillNodeData { Id = "rogue_1", Name = "Agility", Description = "+Speed", X = 50, Y = 35, Cost = 1, Icon = TryFindSprite("rogue_1"), Parents = new List<string> { "root" } });
        _skills.Add(new SkillNodeData { Id = "rogue_2", Name = "Precision", Description = "+20% Crit", X = 40, Y = 60, Cost = 2, Icon = TryFindSprite("rogue_2"), Parents = new List<string> { "rogue_1" } });
        _skills.Add(new SkillNodeData { Id = "rogue_3", Name = "Shadow", Description = "Invisibility", X = 60, Y = 60, Cost = 3, Icon = TryFindSprite("rogue_3"), Parents = new List<string> { "rogue_1" } });
        _skills.Add(new SkillNodeData { Id = "mage_1", Name = "Intellect", Description = "+Mana", X = 80, Y = 35, Cost = 1, Icon = TryFindSprite("mage_1"), Parents = new List<string> { "root" } });
        _skills.Add(new SkillNodeData { Id = "mage_2", Name = "Fireball", Description = "Boom.", X = 70, Y = 60, Cost = 2, Icon = TryFindSprite("mage_2"), Parents = new List<string> { "mage_1" } });
        _skills.Add(new SkillNodeData { Id = "mage_3", Name = "Frost", Description = "Freeze.", X = 90, Y = 60, Cost = 3, Icon = TryFindSprite("mage_3"), Parents = new List<string> { "mage_1" } });
        _skills.Add(new SkillNodeData { Id = "ultimate", Name = "Heroic Will", Description = "The ultimate power.", X = 50, Y = 85, Cost = 5, Icon = TryFindSprite("ultimate"), Parents = new List<string> { "warrior_3", "rogue_3", "mage_3" } });
        
        // Also ensure theme is set to prevent null errors
        ResetThemeDefaults();
        
        Debug.Log("Skills populated!");
    }

      [ContextMenu("Reset Theme Colors")]
    private void ResetThemeDefaults()
    {
        UnityEditor.Undo.RecordObject(this, "Reset Theme Colors");

        if (_theme == null) _theme = new SkillTreeTheme();
        _theme.NodeBackground = new Color(0.06f, 0.09f, 0.16f);
        _theme.NodeBorderLocked = new Color(0.2f, 0.25f, 0.33f);
        _theme.StateAvailable = Color.white;
        _theme.StateUnlocked = new Color(0.91f, 0.7f, 0.03f);
        _theme.StateSelected = new Color(0.23f, 0.51f, 0.96f);
        _theme.IconLocked = new Color(0.5f, 0.5f, 0.5f);
        _theme.IconAvailable = Color.white;
        _theme.IconUnlocked = new Color(0.91f, 0.7f, 0.03f);
        _theme.LineInactive = new Color(0.11f, 0.16f, 0.23f);
        _theme.LineActive = new Color(0.98f, 0.75f, 0.14f);
        _theme.CostBadgeBackground = new Color(0.14f, 0.38f, 0.92f);
        _theme.CostBadgeText = Color.white;
        _theme.TextGreen = new Color(0.13f, 0.77f, 0.36f);
        _theme.TextGrey = new Color(0.58f, 0.64f, 0.72f);
    }
#endif
}