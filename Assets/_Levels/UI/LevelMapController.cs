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
        public Color StarGold = new Color(1f, 0.84f, 0f); // <--- FIXED: Added StarGold
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
        
        public bool Unlocked; // <--- FIXED: Added Unlocked
        
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

    private void OnEnable()
    {
        if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();
        var root = _uiDocument.rootVisualElement;

        _nodesContainer = root.Q("NodesContainer");
        _connectionsLayer = root.Q("ConnectionsLayer");
        _totalStarsLabel = root.Q<Label>("TotalStars");
        
        _homeLink = root.Q<Button>("HomeLink");
        _homeIcon = root.Q("HomeIcon");
        _homeText = root.Q<Label>("HomeText");
        _breadcrumbSeparator = root.Q<Label>("BreadcrumbSeparator");
        _currentViewLabel = root.Q<Label>("CurrentViewLabel");
        _currentWorldIcon = root.Q("CurrentWorldIcon"); 
        _instructionLabel = root.Q<Label>("InstructionLabel");

        _sidebar = root.Q("Sidebar");
        _sidebarIcon = root.Q("SidebarIcon");
        _sidebarName = root.Q<Label>("LevelName");
        _sidebarStatus = root.Q<Label>("LevelStatus");
        _sidebarDesc = root.Q<Label>("LevelDesc");
        _playButton = root.Q<Button>("PlayButton");
        _sidebarStars = new VisualElement[] { root.Q("Star1"), root.Q("Star2"), root.Q("Star3") };

        if (_homeText != null) _homeText.text = _campaignButtonText;
        if (_instructionLabel != null) _instructionLabel.text = _instructionText;
        if (_homeIcon != null && _campaignButtonIcon != null) 
            _homeIcon.style.backgroundImage = new StyleBackground(_campaignButtonIcon);

        if (_homeLink != null) _homeLink.clicked += GoToWorldSelect;
        if (_playButton != null) _playButton.clicked += OnPlayClicked;
        if (_connectionsLayer != null) _connectionsLayer.generateVisualContent += OnGenerateConnections;

        GoToWorldSelect();
    }

    private void SetBorderWidth(VisualElement element, float width)
    {
        element.style.borderTopWidth = width;
        element.style.borderBottomWidth = width;
        element.style.borderLeftWidth = width;
        element.style.borderRightWidth = width;
    }

    // --- NAVIGATION ---

    private void GoToWorldSelect()
    {
        _currentMode = ViewMode.WorldSelect;
        _activeWorldIndex = -1;
        _selectedLevelId = null;

        if (_homeLink != null)
        {
            _homeLink.style.backgroundColor = new Color(0.25f, 0.25f, 0.35f); 
            _homeLink.pickingMode = PickingMode.Ignore; 
            _homeText.style.color = Color.white;
            
            if(_breadcrumbSeparator != null) _breadcrumbSeparator.style.display = DisplayStyle.None;
            if(_currentViewLabel != null) _currentViewLabel.style.display = DisplayStyle.None;
            if(_currentWorldIcon != null) _currentWorldIcon.style.display = DisplayStyle.None;
        }

        if (_sidebar != null) _sidebar.style.display = DisplayStyle.None;
        
        RenderView();
    }

    private void GoToLevelSelect(int worldIndex)
    {
        _currentMode = ViewMode.LevelSelect;
        _activeWorldIndex = worldIndex;
        _selectedLevelId = null;

        var world = _worlds.Find(w => w.WorldIndex == worldIndex);
        string wName = world != null ? world.Name : $"World {worldIndex}";

        if (_homeLink != null)
        {
            _homeLink.style.backgroundColor = Color.clear;
            _homeLink.pickingMode = PickingMode.Position;
            _homeText.style.color = new Color(0.7f, 0.7f, 0.7f); 

            if(_breadcrumbSeparator != null) _breadcrumbSeparator.style.display = DisplayStyle.Flex;
            if(_currentViewLabel != null)
            {
                _currentViewLabel.style.display = DisplayStyle.Flex;
                _currentViewLabel.text = wName;
            }

            if (_currentWorldIcon != null)
            {
                _currentWorldIcon.style.display = DisplayStyle.Flex;
                if(world != null && world.Icon != null)
                    _currentWorldIcon.style.backgroundImage = new StyleBackground(world.Icon);
                else
                    _currentWorldIcon.style.backgroundImage = null;
            }
        }

        if (_sidebar != null) _sidebar.style.display = DisplayStyle.None;

        RenderView();
    }

    // --- RENDER LOGIC ---

    private void RenderView()
    {
        _nodesContainer.Clear();
        _connectionsLayer.MarkDirtyRepaint(); 
        _nodesContainer.MarkDirtyRepaint();
        UpdateTotalStars();

        if (_currentMode == ViewMode.WorldSelect) RenderWorldNodes();
        else RenderLevelNodes();
    }

    private void RenderWorldNodes()
    {
        foreach (var world in _worlds)
        {
            var btn = CreateBaseNode(world.X, world.Y, 80); 
            var content = btn.Q<VisualElement>("Content");
            
            SetBorderWidth(content, 6f);
            content.style.borderTopColor = _theme.WorldBorder; content.style.borderBottomColor = _theme.WorldBorder;
            content.style.borderLeftColor = _theme.WorldBorder; content.style.borderRightColor = _theme.WorldBorder;
            content.style.backgroundColor = _theme.WorldBackground;

            if (world.Icon != null)
            {
                var icon = new VisualElement();
                icon.style.width = 50; icon.style.height = 50;
                icon.style.backgroundImage = new StyleBackground(world.Icon);
                content.Add(icon);
            }

            var lbl = new Label(world.Name);
            lbl.style.position = Position.Absolute;
            lbl.style.bottom = -30;
            lbl.style.fontSize = 16;
            lbl.style.color = Color.white;
            lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            lbl.style.width = 150;
            lbl.style.unityTextAlign = TextAnchor.MiddleCenter;
            btn.Add(lbl);

            btn.clicked += () => GoToLevelSelect(world.WorldIndex);
            _nodesContainer.Add(btn);
        }
    }

    private void RenderLevelNodes()
    {
        var visibleLevels = _levels.Where(l => l.WorldIndex == _activeWorldIndex).ToList();

        foreach (var level in visibleLevels)
        {
            float size = level.IsSecret ? 45f : 55f;
            var btn = CreateBaseNode(level.X, level.Y, size);
            var content = btn.Q<VisualElement>("Content");
            
            Color borderColor = (level.Id == _selectedLevelId) ? Color.yellow : Color.white;
            content.style.borderTopColor = borderColor; content.style.borderBottomColor = borderColor;
            content.style.borderLeftColor = borderColor; content.style.borderRightColor = borderColor;
            
            SetBorderWidth(content, (level.Id == _selectedLevelId) ? 5f : 3f);

            if (level.IsSecret)
            {
                if(!level.Unlocked) content.style.backgroundColor = _theme.SecretLocked;
                else content.style.backgroundColor = (level.StarsEarned > 0) ? _theme.SecretCompleted : _theme.SecretAvailable;
            }
            else
            {
                if(!level.Unlocked) content.style.backgroundColor = _theme.NodeLocked;
                else content.style.backgroundColor = (level.StarsEarned > 0) ? _theme.NodeCompleted : _theme.NodeAvailable;
            }

            if (level.Icon != null)
            {
                var icon = new VisualElement();
                icon.style.width = size * 0.6f; icon.style.height = size * 0.6f;
                icon.style.backgroundImage = new StyleBackground(level.Icon);
                content.Add(icon);
            }
            else
            {
                var lbl = new Label(level.IsSecret ? "?" : (visibleLevels.IndexOf(level) + 1).ToString());
                lbl.style.fontSize = 16;
                lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
                lbl.style.color = Color.white;
                content.Add(lbl);
            }

            btn.clicked += () => SelectLevel(level.Id);
            _nodesContainer.Add(btn);
        }
    }

    private Button CreateBaseNode(float x, float y, float size)
    {
        var btn = new Button();
        btn.style.position = Position.Absolute;
        btn.style.left = Length.Percent(x);
        btn.style.top = Length.Percent(y);
        btn.style.width = size; btn.style.height = size;
        btn.style.marginLeft = -size / 2; btn.style.marginTop = -size / 2;
        btn.style.backgroundColor = Color.clear;
        SetBorderWidth(btn, 0f);
        btn.style.overflow = Overflow.Visible; 
        
        // Prevent Layout Squash
        btn.style.minWidth = size; btn.style.minHeight = size;
        btn.style.flexShrink = 0;

        var content = new VisualElement();
        content.name = "Content";
        content.style.width = Length.Percent(100);
        content.style.height = Length.Percent(100);
        content.style.borderTopLeftRadius = size / 2; content.style.borderTopRightRadius = size / 2;
        content.style.borderBottomLeftRadius = size / 2; content.style.borderBottomRightRadius = size / 2;
        content.style.justifyContent = Justify.Center; 
        content.style.alignItems = Align.Center;
        content.style.overflow = Overflow.Hidden; 
        
        btn.Add(content);
        return btn;
    }

    private void OnGenerateConnections(MeshGenerationContext mgc)
    {
        var painter = mgc.painter2D;
        painter.lineWidth = 5f;
        painter.lineCap = LineCap.Round;
        
        float w = _connectionsLayer.contentRect.width;
        float h = _connectionsLayer.contentRect.height;

        if (_currentMode == ViewMode.WorldSelect)
        {
            foreach(var world in _worlds)
            {
                if(world.ConnectedToWorlds == null) continue;
                foreach(var targetIdx in world.ConnectedToWorlds)
                {
                    var target = _worlds.Find(wd => wd.WorldIndex == targetIdx);
                    if(target == null) continue;
                     
                    painter.strokeColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    painter.BeginPath();
                    painter.MoveTo(new Vector2((world.X/100f)*w, (world.Y/100f)*h));
                    painter.LineTo(new Vector2((target.X/100f)*w, (target.Y/100f)*h));
                    painter.Stroke();
                }
            }
        }
        else
        {
            var visibleLevels = _levels.Where(l => l.WorldIndex == _activeWorldIndex).ToList();
            foreach (var level in visibleLevels)
            {
                if (level.Prerequisites == null) continue;
                foreach (var preId in level.Prerequisites)
                {
                    var parent = visibleLevels.Find(x => x.Id == preId);
                    if (parent == null) continue;

                    Vector2 start = new Vector2((parent.X / 100f) * w, (parent.Y / 100f) * h);
                    Vector2 end = new Vector2((level.X / 100f) * w, (level.Y / 100f) * h);

                    bool unlocked = level.Unlocked && parent.Unlocked;
                    painter.strokeColor = unlocked ? _theme.PathUnlocked : _theme.PathLocked;
                    
                    painter.BeginPath(); painter.MoveTo(start); painter.LineTo(end); painter.Stroke();
                }
            }
        }
    }

    private void SelectLevel(string id)
    {
        _selectedLevelId = id;
        RenderLevelNodes(); 
        UpdateSidebar();
    }

    private void UpdateSidebar()
    {
        var level = _levels.Find(x => x.Id == _selectedLevelId);
        if (level == null) { _sidebar.style.display = DisplayStyle.None; return; }

        _sidebar.style.display = DisplayStyle.Flex;
        _sidebarName.text = level.DisplayName;
        _sidebarDesc.text = level.Description;

        if (_sidebarIcon != null)
            _sidebarIcon.style.backgroundImage = (level.Icon != null) ? new StyleBackground(level.Icon) : null;

        if (level.Unlocked)
        {
            _sidebarStatus.text = (level.StarsEarned > 0) ? "COMPLETED" : "READY";
            _playButton.SetEnabled(true);
            _playButton.text = "PLAY";
        }
        else
        {
            _sidebarStatus.text = "LOCKED";
            _playButton.SetEnabled(false);
            _playButton.text = "LOCKED";
        }

        for (int i = 0; i < 3; i++)
        {
            if (_sidebarStars[i] != null)
                _sidebarStars[i].style.backgroundColor = (i < level.StarsEarned) ? _theme.StarGold : _theme.StarEmpty;
        }
    }

    private void UpdateTotalStars()
    {
        int total = _levels.Sum(l => l.StarsEarned);
        if (_totalStarsLabel != null) _totalStarsLabel.text = $"{total} Stars";
    }

    private void OnPlayClicked()
    {
        var level = _levels.Find(x => x.Id == _selectedLevelId);
        if (level != null && level.Unlocked)
        {
            Debug.Log($"Playing {level.DisplayName}");
            level.StarsEarned = Random.Range(1, 4);
            foreach(var next in _levels)
                if(next.Prerequisites.Contains(level.Id)) next.Unlocked = true;
            RenderView();
            UpdateSidebar();
        }
    }

    [ContextMenu("Auto-Populate Full Campaign")]
    private void AutoPopulate()
    {
        UnityEditor.Undo.RecordObject(this, "Populate Full Campaign");
        _worlds.Clear(); _levels.Clear();

        _worlds.Add(new WorldData { WorldIndex = 1, Name = "Green Fields", X = 20, Y = 50, Unlocked = true, ConnectedToWorlds = new List<int>{2} });
        _worlds.Add(new WorldData { WorldIndex = 2, Name = "Dark Forest", X = 50, Y = 50, Unlocked = true, ConnectedToWorlds = new List<int>{3} });
        _worlds.Add(new WorldData { WorldIndex = 3, Name = "Lava Core", X = 80, Y = 50, Unlocked = true });

        _levels.Add(new LevelNode { Id = "1-1", WorldIndex = 1, DisplayName = "1-1 Start", X = 20, Y = 80, Unlocked = true, Prerequisites = new List<string>() });
        _levels.Add(new LevelNode { Id = "1-2", WorldIndex = 1, DisplayName = "1-2 Path", X = 40, Y = 60, Prerequisites = new List<string>{"1-1"} });
        _levels.Add(new LevelNode { Id = "1-3", WorldIndex = 1, DisplayName = "1-3 Boss", X = 60, Y = 80, Prerequisites = new List<string>{"1-2"} });
        _levels.Add(new LevelNode { Id = "2-1", WorldIndex = 2, DisplayName = "2-1 Trees", X = 50, Y = 20, Unlocked = true, Prerequisites = new List<string>() });
        _levels.Add(new LevelNode { Id = "2-2", WorldIndex = 2, DisplayName = "2-2 Wolf", X = 50, Y = 50, Prerequisites = new List<string>{"2-1"} });
        _levels.Add(new LevelNode { Id = "2-3", WorldIndex = 2, DisplayName = "2-3 Bear", X = 50, Y = 80, Prerequisites = new List<string>{"2-2"} });
        _levels.Add(new LevelNode { Id = "2-S", WorldIndex = 2, DisplayName = "Secret Cave", IsSecret = true, X = 80, Y = 50, Prerequisites = new List<string>{"2-2"} });

        Debug.Log("Populated 3 Worlds and Levels.");
        RenderView();
    }
}