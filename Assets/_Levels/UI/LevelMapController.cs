using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelMapController : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;

    [Header("UI Text & Icons")]
    [SerializeField] private string _campaignButtonText = "World Map";
    [SerializeField] private Sprite _campaignButtonIcon; 
    [SerializeField] private string _instructionText = "Click a World to enter. Click 'World Map' to go back.";

    // --- Theme ---
    [System.Serializable]
    public class LevelMapTheme
    {
        [Header("Nodes")]
        public Color NodeLocked = new Color(0.2f, 0.2f, 0.2f);
        public Color NodeAvailable = new Color(0.0f, 0.5f, 0.9f);
        public Color NodeCompleted = new Color(0.1f, 0.7f, 0.1f);
        public Color WorldBackground = new Color(0.1f, 0.1f, 0.2f);
        public Color WorldBorder = new Color(1f, 0.8f, 0.2f);
        
        [Header("Secrets")]
        public Color SecretLocked = new Color(0.2f, 0.0f, 0.2f);
        public Color SecretAvailable = new Color(0.6f, 0.1f, 0.8f);
        public Color SecretCompleted = new Color(0.4f, 0.0f, 0.6f);

        [Header("Lines")]
        public Color PathLocked = new Color(0.2f, 0.2f, 0.2f);
        public Color PathUnlocked = new Color(0.7f, 0.7f, 0.7f);
        
        [Header("Stars")]
        public Color StarGold = new Color(1f, 0.84f, 0f);
        public Color StarEmpty = new Color(0.3f, 0.3f, 0.3f);
    }
    [SerializeField] private LevelMapTheme _theme;

    // --- Data Structures ---

    [System.Serializable]
    public class WorldData
    {
        public int WorldIndex; 
        public string Name;
        public string Description;
        [Range(0, 100)] public float X;
        [Range(0, 100)] public float Y;
        public Sprite Icon;
        public bool Unlocked;
        public List<int> ConnectedToWorlds;
    }

    [System.Serializable]
    public class LevelNode
    {
        public string Id;
        public int WorldIndex;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;
        public bool IsSecret;
        [Range(0, 100)] public float X;
        [Range(0, 100)] public float Y;
        public List<string> Prerequisites; 
        public bool Unlocked;
        public int StarsEarned; 
    }

    [Header("Data Configuration")]
    [SerializeField] private List<WorldData> _worlds = new List<WorldData>();
    [SerializeField] private List<LevelNode> _levels = new List<LevelNode>();

    // --- Constants ---
    private const float WORLD_NODE_SIZE = 80f;
    private const float LEVEL_NODE_SIZE = 55f;
    private const float SECRET_NODE_SIZE = 45f;
    private const float WORLD_ICON_SIZE = 50f;
    private const float WORLD_BORDER_WIDTH = 6f;
    private const float LEVEL_BORDER_WIDTH_NORMAL = 3f;
    private const float LEVEL_BORDER_WIDTH_SELECTED = 5f;
    private const int WORLD_NAME_FONT_SIZE = 16;
    private const float WORLD_NAME_OFFSET = -30f;
    private const float WORLD_NAME_WIDTH = 150f;
    private const int LEVEL_LABEL_FONT_SIZE = 16;
    private const float CONNECTION_LINE_WIDTH = 5f;
    private const float WORLD_CONNECTION_ALPHA = 0.5f;

    // --- State ---
    private enum ViewMode { WorldSelect, LevelSelect }
    private ViewMode _currentMode = ViewMode.WorldSelect;
    private int _activeWorldIndex = -1;
    private string _selectedLevelId;

    // UI References
    private VisualElement _nodesContainer;
    private VisualElement _connectionsLayer;
    private VisualElement _sidebar;
    private Label _totalStarsLabel;
    
    // Header
    private Button _homeLink;
    private VisualElement _homeIcon;
    private Label _homeText;
    private Label _breadcrumbSeparator;
    private Label _currentViewLabel;
    private VisualElement _currentWorldIcon;
    private Label _instructionLabel;

    // Sidebar Elements
    private VisualElement _sidebarIcon;
    private Label _sidebarName;
    private Label _sidebarStatus;
    private Label _sidebarDesc;
    private Button _playButton;
    private VisualElement[] _sidebarStars;

    private VisualElement _sidebarBackdrop;
    private Button _sidebarCloseButton;

    private void OnEnable()
    {
        if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();
        var root = _uiDocument.rootVisualElement;

        BindUIElements(root);
        SetupEventHandlers();
        ApplyInitialSettings();

        GoToWorldSelect();
    }

private void BindUIElements(VisualElement root)
{
    _nodesContainer = root.Q("NodesContainer");
    _connectionsLayer = root.Q("ConnectionsLayer");
    _totalStarsLabel = root.Q<Label>("TotalStars");
    
    // Header
    _homeLink = root.Q<Button>("HomeLink");
    _homeIcon = root.Q("HomeIcon");
    _homeText = root.Q<Label>("HomeText");
    _breadcrumbSeparator = root.Q<Label>("BreadcrumbSeparator");
    _currentViewLabel = root.Q<Label>("CurrentViewLabel");
    _currentWorldIcon = root.Q("CurrentWorldIcon");
    _instructionLabel = root.Q<Label>("InstructionLabel");

    // Sidebar
    _sidebar = root.Q("Sidebar");
    _sidebarBackdrop = root.Q("SidebarBackdrop");
    _sidebarCloseButton = root.Q<Button>("SidebarCloseButton");
    _sidebarIcon = root.Q("SidebarIcon");
    _sidebarName = root.Q<Label>("LevelName");
    _sidebarStatus = root.Q<Label>("LevelStatus");
    _sidebarDesc = root.Q<Label>("LevelDesc");
    _playButton = root.Q<Button>("PlayButton");
    _sidebarStars = new VisualElement[] { root.Q("Star1"), root.Q("Star2"), root.Q("Star3") };
}

private void SetupEventHandlers()
{
    if (_homeLink != null) _homeLink.clicked += GoToWorldSelect;
    if (_playButton != null) _playButton.clicked += OnPlayClicked;
    if (_connectionsLayer != null) _connectionsLayer.generateVisualContent += OnGenerateConnections;
    
    // Add close button handler
    if (_sidebarCloseButton != null) _sidebarCloseButton.clicked += HideSidebar;
    
    // Add backdrop click handler (click outside to close)
    if (_sidebarBackdrop != null) _sidebarBackdrop.RegisterCallback<ClickEvent>(OnBackdropClicked);
}

private void OnBackdropClicked(ClickEvent evt)
{
    HideSidebar();
}


    private void ApplyInitialSettings()
    {
        if (_homeText != null) _homeText.text = _campaignButtonText;
        if (_instructionLabel != null) _instructionLabel.text = _instructionText;
        if (_homeIcon != null && _campaignButtonIcon != null)
            _homeIcon.style.backgroundImage = new StyleBackground(_campaignButtonIcon);

        // ADDED: Ensure the close button has the X character
        if (_sidebarCloseButton != null) _sidebarCloseButton.text = "×"; 
    }
    // --- STYLE HELPERS ---

    private void SetBorderWidth(VisualElement element, float width)
    {
        element.style.borderTopWidth = width;
        element.style.borderBottomWidth = width;
        element.style.borderLeftWidth = width;
        element.style.borderRightWidth = width;
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

    // --- NAVIGATION ---

    private void GoToWorldSelect()
    {
        _currentMode = ViewMode.WorldSelect;
        _activeWorldIndex = -1;
        _selectedLevelId = null;

        UpdateBreadcrumb(null);
        HideSidebar();
        RenderView();
    }

    private void GoToLevelSelect(int worldIndex)
    {
        _currentMode = ViewMode.LevelSelect;
        _activeWorldIndex = worldIndex;
        _selectedLevelId = null;

        var world = _worlds.Find(w => w.WorldIndex == worldIndex);
        UpdateBreadcrumb(world);
        HideSidebar();
        RenderView();
    }

    private void UpdateBreadcrumb(WorldData world)
    {
        if (_homeLink == null) return;

        bool isWorldSelect = world == null;
        
        // Home link styling
        _homeLink.style.backgroundColor = isWorldSelect ? new Color(0.25f, 0.25f, 0.35f) : Color.clear;
        _homeLink.pickingMode = isWorldSelect ? PickingMode.Ignore : PickingMode.Position;
        
        if (_homeText != null)
            _homeText.style.color = isWorldSelect ? Color.white : new Color(0.7f, 0.7f, 0.7f);

        // Breadcrumb elements visibility
        if (_breadcrumbSeparator != null)
            _breadcrumbSeparator.style.display = isWorldSelect ? DisplayStyle.None : DisplayStyle.Flex;
            
        if (_currentViewLabel != null)
        {
            _currentViewLabel.style.display = isWorldSelect ? DisplayStyle.None : DisplayStyle.Flex;
            if (!isWorldSelect)
                _currentViewLabel.text = world.Name;
        }

        if (_currentWorldIcon != null)
        {
            _currentWorldIcon.style.display = isWorldSelect ? DisplayStyle.None : DisplayStyle.Flex;
            if (!isWorldSelect)
            {
                _currentWorldIcon.style.backgroundImage = world.Icon != null 
                    ? new StyleBackground(world.Icon) 
                    : null;
            }
        }
    }

private void HideSidebar()
{
    if (_sidebar != null) _sidebar.style.display = DisplayStyle.None;
    if (_sidebarBackdrop != null) _sidebarBackdrop.style.display = DisplayStyle.None;
}

    // --- RENDER LOGIC ---

    private void RenderView()
    {
        _nodesContainer.Clear();
        _connectionsLayer.MarkDirtyRepaint();
        _nodesContainer.MarkDirtyRepaint();
        UpdateTotalStars();

        if (_currentMode == ViewMode.WorldSelect)
            RenderWorldNodes();
        else
            RenderLevelNodes();
    }

    private void RenderWorldNodes()
    {
        foreach (var world in _worlds)
        {
            var btn = CreateWorldNode(world);
            _nodesContainer.Add(btn);
        }
    }

private Button CreateWorldNode(WorldData world)
    {
        var btn = CreateBaseNode(world.X, world.Y, WORLD_NODE_SIZE);
        var content = btn.Q<VisualElement>("Content");
        
        SetBorderWidth(content, WORLD_BORDER_WIDTH);
        SetBorderColor(content, _theme.WorldBorder);
        content.style.backgroundColor = _theme.WorldBackground;

        if (world.Icon != null)
        {
            var icon = CreateIcon(world.Icon, WORLD_ICON_SIZE);
            content.Add(icon);
        }

        var label = CreateWorldLabel(world.Name);
        btn.Add(label);

        // --- HOVER EVENTS (Always Active) ---
        btn.RegisterCallback<MouseEnterEvent>(evt => label.style.opacity = 1);
        btn.RegisterCallback<MouseLeaveEvent>(evt => label.style.opacity = 0);

        // --- LOCK LOGIC ---
        if (world.Unlocked)
        {
            // Fully opaque and clickable
            btn.style.opacity = 1f; 
            btn.clicked += () => GoToLevelSelect(world.WorldIndex);
            
            // Optional: Add a pointer cursor only if unlocked
            btn.style.cursor = new StyleCursor(StyleKeyword.Auto); // Or Link
        }
        else
        {
            // 33% transparent (0.66 alpha)
            btn.style.opacity = 0.66f;
            
            // We do NOT add the btn.clicked listener here, making it unclickable.
            // We also don't use SetEnabled(false) because that would stop the hover events.
        }
        
        return btn;
    }

    private Label CreateWorldLabel(string name)
    {
        var label = new Label(name);
        label.style.position = Position.Absolute;
        label.style.bottom = WORLD_NAME_OFFSET;
        
        // --- LAYOUT & CENTERING ---
        label.style.width = StyleKeyword.Auto; // Allow auto width to fit text + padding
        label.style.maxWidth = 200f;           // Max width cap
        label.style.left = Length.Percent(50);
        label.style.translate = new Translate(Length.Percent(-50), 0);
        
        // --- TEXT STYLING ---
        label.style.fontSize = WORLD_NAME_FONT_SIZE;
        label.style.color = Color.white;
        label.style.unityFontStyleAndWeight = FontStyle.Bold;
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        
        // --- BOX STYLING (Rectangle Background) ---
        label.style.backgroundColor = _theme.WorldBackground;
        
        SetBorderWidth(label, 1);
        SetBorderColor(label, _theme.WorldBorder);
        SetBorderRadius(label, 6);


        label.style.paddingTop = 4;
        label.style.paddingBottom = 4;
        label.style.paddingLeft = 10;
        label.style.paddingRight = 10;
        
        // --- ANIMATION ---
        label.style.opacity = 0; 
        label.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("opacity") };
        label.style.transitionDuration = new List<TimeValue> { new TimeValue(0.2f, TimeUnit.Second) };

        return label;
    }


    private void RenderLevelNodes()
    {
        var visibleLevels = _levels.Where(l => l.WorldIndex == _activeWorldIndex).ToList();

        foreach (var level in visibleLevels)
        {
            var btn = CreateLevelNode(level, visibleLevels);
            _nodesContainer.Add(btn);
        }
    }

    private Button CreateLevelNode(LevelNode level, List<LevelNode> visibleLevels)
    {
        float size = level.IsSecret ? SECRET_NODE_SIZE : LEVEL_NODE_SIZE;
        var btn = CreateBaseNode(level.X, level.Y, size);
        var content = btn.Q<VisualElement>("Content");
        
        ApplyLevelNodeStyling(content, level, size);
        AddLevelNodeContent(content, level, visibleLevels, size);

        btn.clicked += () => SelectLevel(level.Id);
        
        return btn;
    }

    private void ApplyLevelNodeStyling(VisualElement content, LevelNode level, float size)
    {
        bool isSelected = level.Id == _selectedLevelId;
        Color borderColor = isSelected ? Color.yellow : Color.white;
        float borderWidth = isSelected ? LEVEL_BORDER_WIDTH_SELECTED : LEVEL_BORDER_WIDTH_NORMAL;
        
        SetBorderWidth(content, borderWidth);
        SetBorderColor(content, borderColor);
        
        Color bgColor = GetLevelNodeColor(level);
        content.style.backgroundColor = bgColor;
    }

    private Color GetLevelNodeColor(LevelNode level)
    {
        if (level.IsSecret)
        {
            if (!level.Unlocked) return _theme.SecretLocked;
            return level.StarsEarned > 0 ? _theme.SecretCompleted : _theme.SecretAvailable;
        }
        else
        {
            if (!level.Unlocked) return _theme.NodeLocked;
            return level.StarsEarned > 0 ? _theme.NodeCompleted : _theme.NodeAvailable;
        }
    }

    private void AddLevelNodeContent(VisualElement content, LevelNode level, List<LevelNode> visibleLevels, float size)
    {
        if (level.Icon != null)
        {
            var icon = CreateIcon(level.Icon, size * 0.6f);
            content.Add(icon);
        }
        else
        {
            var label = CreateLevelLabel(level, visibleLevels);
            content.Add(label);
        }
    }

    private Label CreateLevelLabel(LevelNode level, List<LevelNode> visibleLevels)
    {
        string text = level.IsSecret ? "?" : (visibleLevels.IndexOf(level) + 1).ToString();
        var label = new Label(text);
        label.style.fontSize = LEVEL_LABEL_FONT_SIZE;
        label.style.unityFontStyleAndWeight = FontStyle.Bold;
        label.style.color = Color.white;
        return label;
    }

    private VisualElement CreateIcon(Sprite sprite, float size)
    {
        var icon = new VisualElement();
        icon.style.width = size;
        icon.style.height = size;
        icon.style.backgroundImage = new StyleBackground(sprite);
        return icon;
    }

    private Button CreateBaseNode(float x, float y, float size)
    {
        var btn = new Button();
        btn.style.position = Position.Absolute;
        btn.style.left = Length.Percent(x);
        btn.style.top = Length.Percent(y);
        btn.style.width = size;
        btn.style.height = size;
        btn.style.marginLeft = -size / 2;
        btn.style.marginTop = -size / 2;
        btn.style.backgroundColor = Color.clear;
        SetBorderWidth(btn, 0f);
        btn.style.overflow = Overflow.Visible;
        
        // Prevent Layout Squash
        btn.style.minWidth = size;
        btn.style.minHeight = size;
        btn.style.flexShrink = 0;

        var content = CreateNodeContent(size);
        btn.Add(content);
        
        return btn;
    }

    private VisualElement CreateNodeContent(float size)
    {
        var content = new VisualElement();
        content.name = "Content";
        content.style.width = Length.Percent(100);
        content.style.height = Length.Percent(100);
        SetBorderRadius(content, size / 2);
        content.style.justifyContent = Justify.Center;
        content.style.alignItems = Align.Center;
        content.style.overflow = Overflow.Hidden;
        return content;
    }

    // --- CONNECTIONS ---

    private void OnGenerateConnections(MeshGenerationContext mgc)
    {
        var painter = mgc.painter2D;
        painter.lineWidth = CONNECTION_LINE_WIDTH;
        painter.lineCap = LineCap.Round;
        
        float w = _connectionsLayer.contentRect.width;
        float h = _connectionsLayer.contentRect.height;

        if (_currentMode == ViewMode.WorldSelect)
            DrawWorldConnections(painter, w, h);
        else
            DrawLevelConnections(painter, w, h);
    }

    private void DrawWorldConnections(Painter2D painter, float width, float height)
    {
        foreach (var world in _worlds)
        {
            if (world.ConnectedToWorlds == null) continue;
            
            foreach (var targetIdx in world.ConnectedToWorlds)
            {
                var target = _worlds.Find(wd => wd.WorldIndex == targetIdx);
                if (target == null) continue;
                
                Vector2 start = new Vector2((world.X / 100f) * width, (world.Y / 100f) * height);
                Vector2 end = new Vector2((target.X / 100f) * width, (target.Y / 100f) * height);
                
                painter.strokeColor = new Color(0.5f, 0.5f, 0.5f, WORLD_CONNECTION_ALPHA);
                DrawLine(painter, start, end);
            }
        }
    }

    private void DrawLevelConnections(Painter2D painter, float width, float height)
    {
        var visibleLevels = _levels.Where(l => l.WorldIndex == _activeWorldIndex).ToList();
        
        foreach (var level in visibleLevels)
        {
            if (level.Prerequisites == null) continue;
            
            foreach (var preId in level.Prerequisites)
            {
                var parent = visibleLevels.Find(x => x.Id == preId);
                if (parent == null) continue;

                Vector2 start = new Vector2((parent.X / 100f) * width, (parent.Y / 100f) * height);
                Vector2 end = new Vector2((level.X / 100f) * width, (level.Y / 100f) * height);

                bool unlocked = level.Unlocked && parent.Unlocked;
                painter.strokeColor = unlocked ? _theme.PathUnlocked : _theme.PathLocked;
                
                DrawLine(painter, start, end);
            }
        }
    }

    private void DrawLine(Painter2D painter, Vector2 start, Vector2 end)
    {
        painter.BeginPath();
        painter.MoveTo(start);
        painter.LineTo(end);
        painter.Stroke();
    }

    // --- SELECTION & SIDEBAR ---

    private void SelectLevel(string id)
    {
        _selectedLevelId = id;
        RenderLevelNodes();
        UpdateSidebar();
    }

private void UpdateSidebarInfo(LevelNode level)
{
    if (_sidebarName != null) _sidebarName.text = level.DisplayName;
    if (_sidebarDesc != null) _sidebarDesc.text = level.Description;
    
    if (_sidebarIcon != null)
    {
        _sidebarIcon.style.backgroundImage = level.Icon != null 
            ? new StyleBackground(level.Icon) 
            : null;
    }
}

  private void UpdateSidebar()
{
    var level = _levels.Find(x => x.Id == _selectedLevelId);
    if (level == null)
    {
        HideSidebar();
        return;
    }

    _sidebar.style.display = DisplayStyle.Flex;
    _sidebarBackdrop.style.display = DisplayStyle.Flex; // Show backdrop
    
    UpdateSidebarInfo(level);
    UpdateSidebarStatus(level);
    UpdateSidebarStars(level);
}

    private void UpdateSidebarStatus(LevelNode level)
    {
        if (_sidebarStatus != null)
        {
            _sidebarStatus.text = level.Unlocked 
                ? (level.StarsEarned > 0 ? "COMPLETED" : "READY") 
                : "LOCKED";
        }

        if (_playButton != null)
        {
            _playButton.SetEnabled(level.Unlocked);
            _playButton.text = level.Unlocked ? "PLAY" : "LOCKED";
        }
    }

    private void UpdateSidebarStars(LevelNode level)
    {
        for (int i = 0; i < 3; i++)
        {
            if (_sidebarStars[i] != null)
            {
                _sidebarStars[i].style.backgroundColor = i < level.StarsEarned 
                    ? _theme.StarGold 
                    : _theme.StarEmpty;
            }
        }
    }

    private void UpdateTotalStars()
    {
        int total = _levels.Sum(l => l.StarsEarned);
        if (_totalStarsLabel != null) _totalStarsLabel.text = $"{total} Stars";
    }

    // --- GAMEPLAY ---

    private void OnPlayClicked()
    {
        var level = _levels.Find(x => x.Id == _selectedLevelId);
        if (level == null || !level.Unlocked) return;

        Debug.Log($"Playing {level.DisplayName}");
        
        // Simulate gameplay result
        level.StarsEarned = Random.Range(1, 4);
        
        // Unlock dependent levels
        UnlockDependentLevels(level);
        
        RenderView();
        UpdateSidebar();
    }

    private void UnlockDependentLevels(LevelNode completedLevel)
    {
        foreach (var nextLevel in _levels)
        {
            if (nextLevel.Prerequisites != null && nextLevel.Prerequisites.Contains(completedLevel.Id))
            {
                nextLevel.Unlocked = true;
            }
        }
    }

    // --- EDITOR UTILITIES ---

    [ContextMenu("Auto-Populate Full Campaign")]
    private void AutoPopulate()
    {
        UnityEditor.Undo.RecordObject(this, "Populate Full Campaign");
        _worlds.Clear();
        _levels.Clear();

        PopulateWorlds();
        PopulateLevels();

        Debug.Log("Populated 3 Worlds and Levels.");
        RenderView();
    }

    private void PopulateWorlds()
    {
        _worlds.Add(new WorldData 
        { 
            WorldIndex = 1, 
            Name = "Green Fields", 
            X = 20, 
            Y = 50, 
            Unlocked = true, 
            ConnectedToWorlds = new List<int> { 2 } 
        });
        
        _worlds.Add(new WorldData 
        { 
            WorldIndex = 2, 
            Name = "Dark Forest", 
            X = 50, 
            Y = 50, 
            Unlocked = true, 
            ConnectedToWorlds = new List<int> { 3 } 
        });
        
        _worlds.Add(new WorldData 
        { 
            WorldIndex = 3, 
            Name = "Lava Core", 
            X = 80, 
            Y = 50, 
            Unlocked = true 
        });
    }

    private void PopulateLevels()
    {
        // World 1
        _levels.Add(new LevelNode 
        { 
            Id = "1-1", 
            WorldIndex = 1, 
            DisplayName = "1-1 Start", 
            X = 20, 
            Y = 80, 
            Unlocked = true, 
            Prerequisites = new List<string>() 
        });
        
        _levels.Add(new LevelNode 
        { 
            Id = "1-2", 
            WorldIndex = 1, 
            DisplayName = "1-2 Path", 
            X = 40, 
            Y = 60, 
            Prerequisites = new List<string> { "1-1" } 
        });
        
        _levels.Add(new LevelNode 
        { 
            Id = "1-3", 
            WorldIndex = 1, 
            DisplayName = "1-3 Boss", 
            X = 60, 
            Y = 80, 
            Prerequisites = new List<string> { "1-2" } 
        });

        // World 2
        _levels.Add(new LevelNode 
        { 
            Id = "2-1", 
            WorldIndex = 2, 
            DisplayName = "2-1 Trees", 
            X = 50, 
            Y = 20, 
            Unlocked = true, 
            Prerequisites = new List<string>() 
        });
        
        _levels.Add(new LevelNode 
        { 
            Id = "2-2", 
            WorldIndex = 2, 
            DisplayName = "2-2 Wolf", 
            X = 50, 
            Y = 50, 
            Prerequisites = new List<string> { "2-1" } 
        });
        
        _levels.Add(new LevelNode 
        { 
            Id = "2-3", 
            WorldIndex = 2, 
            DisplayName = "2-3 Bear", 
            X = 50, 
            Y = 80, 
            Prerequisites = new List<string> { "2-2" } 
        });
        
        _levels.Add(new LevelNode 
        { 
            Id = "2-S", 
            WorldIndex = 2, 
            DisplayName = "Secret Cave", 
            IsSecret = true, 
            X = 80, 
            Y = 50, 
            Prerequisites = new List<string> { "2-2" } 
        });
    }
}