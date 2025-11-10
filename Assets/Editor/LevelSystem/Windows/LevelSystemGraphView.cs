using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelSystemGraphView : GraphView {
    private readonly LevelSystemEditorWindow _editorWindow;
    private LevelSystemSearchWindow _searhWindow;
    private MiniMap _miniMap;
    private LevelSystemSidebar _sidebar;
    private LevelBaseNode _selectedNode;

    private readonly SerializableDictionary<string, LevelNodeErrorData> _ungroupedNodes;
    private readonly SerializableDictionary<Group, SerializableDictionary<string, LevelNodeErrorData>> _groupedNodes;
    private readonly SerializableDictionary<string, LevelGroupErrorData> _groups;
    private Dictionary<string, Level> _nodeLevelMapping;

    private int _repeatedNamesCount;
    
    public LevelSystemSidebar Sidebar => _sidebar;

    public LevelSystemGraphView(LevelSystemEditorWindow editor) {
        _editorWindow = editor;
        _ungroupedNodes = new();
        _groupedNodes = new();
        _groups = new();
        _nodeLevelMapping = new Dictionary<string, Level>();
        
        _sidebar = new LevelSystemSidebar();

        AddSearchWindow();
        AddManipulators();
        AddGridBackground();
        AddMinimap();

        OnElementsDeleted();
        OnGroupElementAdded();
        OnGroupElementRemoved();
        OnGroupRenamed();
        OnGraphViewChanged();
        
        // Register callback for node selection
        RegisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);

        AddStyles();
        AddMinimapStyles();
    }

    #region Overrided Methods
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
        List<Port> compatiblePorts = new();
        ports.ForEach(port => {
            if (startPort == port)
                return;

            if (startPort.node == port.node)
                return;

            if (startPort.direction == port.direction)
                return;

            compatiblePorts.Add(port);
        });
        return compatiblePorts;
    }
    #endregion

    #region Manipulators
    private void AddManipulators() {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        this.AddManipulator(CreateNodeContextualMenu("Add Node (Single Choice)", LevelType.SingleChoice));
        this.AddManipulator(CreateNodeContextualMenu("Add Node (Multiple Choice)", LevelType.MultipleChoice));
        this.AddManipulator(CreateGroupContextualMenu());
    }

    private IManipulator CreateNodeContextualMenu(string actionName, LevelType type) {
        ContextualMenuManipulator contextualManipulator = new(
            menuEvent => menuEvent.menu.AppendAction(actionName, actionEvent => CreateNode("LevelName", type, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
        );
        return contextualManipulator;
    }

    private IManipulator CreateGroupContextualMenu() {
        ContextualMenuManipulator contextualManipulator = new(
            menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => CreateGroup("Level group", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
        );
        return contextualManipulator;
    }
    #endregion

    #region Stylizing
    private void AddStyles()
    {

        this.AddStyleSheets(
            "LevelSystem/Styles/DSGraphViewStyles.uss",
            "LevelSystem/Styles/DSNodeStyles.uss"
        );

      
    }
    
    

    private void AddGridBackground() {
        GridBackground gridBackground = new();
        gridBackground.StretchToParentSize();
        Insert(0, gridBackground);
    }

    private void AddMinimapStyles() {
        StyleColor backgroundColor = new(new Color32(29, 29, 29, 255));
        StyleColor borderColor = new(new Color32(51, 51, 51, 255));
        _miniMap.style.backgroundColor = backgroundColor;
        _miniMap.style.borderTopColor = borderColor;
        _miniMap.style.borderRightColor = borderColor;
        _miniMap.style.borderBottomColor = borderColor;
        _miniMap.style.borderLeftColor = borderColor;
    }
    #endregion

    #region Callbacks
    private void OnElementsDeleted() {
        deleteSelection = (operationName, askUser) => {
            List<LevelBaseNode> nodesToDelete = new();
            List<LevelSystemGroup> groupsToDelete = new();
            List<Edge> edgesToDelete = new();

            foreach (var element in selection) {
                if (element is LevelBaseNode node)
                    nodesToDelete.Add(node);

                if (element is LevelSystemGroup group)
                    groupsToDelete.Add(group);

                if (element is Edge edge)
                    edgesToDelete.Add(edge);
            }

            foreach (var group in groupsToDelete) {
                List<LevelBaseNode> groupNodes = new();
                foreach (var groupElement in group.containedElements)
                    if (groupElement is LevelBaseNode node)
                        groupNodes.Add(node);

                group.RemoveElements(groupNodes);
                RemoveGroup(group);
                RemoveElement(group);
            }

            DeleteElements(edgesToDelete);

            foreach (var node in nodesToDelete) {
                // If this is the selected node, deselect it
                if (node == _selectedNode) {
                    DeselectNode();
                }
                
                node.Group?.RemoveElement(node);
                RemoveUngroupedNode(node);
                node.DisconnectAllPorts();
                RemoveElement(node);
                
                // Remove from Level mapping
                if (_nodeLevelMapping.ContainsKey(node.ID)) {
                    _nodeLevelMapping.Remove(node.ID);
                }
            }

        };
    }

    private void OnGroupElementAdded() {
        elementsAddedToGroup = (group, elements) => {
            foreach (var element in elements) {
                if (element is not LevelBaseNode)
                    continue;

                LevelBaseNode node = (LevelBaseNode)element;
                RemoveUngroupedNode(node);
                AddGroupedNode(node, group as LevelSystemGroup);
            }
        };
    }

    private void OnGroupElementRemoved() {
        elementsRemovedFromGroup = (group, elements) => {
            foreach (var element in elements) {
                if (element is not LevelBaseNode)
                    continue;

                LevelBaseNode node = (LevelBaseNode)element;
                RemoveGroupedNode(node, group);
                AddUngroupedNode(node);
            }
        };
    }

    private void OnGroupRenamed() {
        groupTitleChanged = (group, newTitle) => {
            LevelSystemGroup LevelGroup = (LevelSystemGroup)group;
            newTitle = newTitle.RemoveWhitespaces().RemoveSpecialCharacters();

            if (!string.IsNullOrEmpty(newTitle))
                LevelGroup.title = newTitle;

            RemoveGroup(LevelGroup);
            LevelGroup.UpdateTitle();
            AddGroup(LevelGroup);
        };
    }

    private void OnGraphViewChanged() {
        graphViewChanged = (changes) => {
            foreach (var edge in changes.edgesToCreate ?? new()) {
                LevelBaseNode nextNode = edge.input.node as LevelBaseNode;
                LevelChoiceSaveData choiceData = edge.output.userData as LevelChoiceSaveData;
                choiceData.SetNode(nextNode);
            }

            foreach (var element in changes.elementsToRemove ?? new()) {
                if (element is Edge edge) {
                    LevelChoiceSaveData choiceData = edge.output.userData as LevelChoiceSaveData;
                    choiceData.ResetNode();
                }
            }

            return changes;
        };
    }
    
    private void OnMouseDown(MouseDownEvent evt) {
        // Only process left mouse button clicks
        if (evt.button != 0)
            return;
        
        // Check if we clicked on a node
        LevelBaseNode clickedNode = null;
        
        // Get the target element
        if (evt.target is VisualElement targetElement) {
            // Walk up the visual tree to find if we're clicking on a node
            VisualElement current = targetElement;
            while (current != null) {
                if (current is LevelBaseNode node) {
                    clickedNode = node;
                    break;
                }
                current = current.parent;
            }
        }
        
        if (clickedNode != null && clickedNode != _selectedNode) {
            SelectNode(clickedNode);
        } else if (clickedNode == null && evt.target == this) {
            // Clicked on empty space in the graph
            DeselectNode();
        }
    }
    #endregion

    #region ElementsCreation
    public LevelBaseNode CreateNode(string nodeName, LevelType type, Vector2 globalPosition, bool isDraw = true) {
        LevelBaseNode node;
        if (type == LevelType.SingleChoice)
            node = new LevelSingleChoiceNode();
        else
            node = new LevelMultipleChoiceNode();

        node.Initialize(nodeName, this, globalPosition);
        if (isDraw)
            node.Draw();

        AddUngroupedNode(node);
        AddElement(node);

        return node;
    }

    public LevelSystemGroup CreateGroup(string name, Vector2 position) {
        LevelSystemGroup group = new(name, position);
        AddGroup(group);
        AddElement(group);

        foreach (var element in selection)
            if (element is LevelBaseNode node)
                group.AddElement(node);
        return group;
    }

    private void AddSearchWindow() {
        if (_searhWindow != null)
            return;

        _searhWindow = ScriptableObject.CreateInstance<LevelSystemSearchWindow>();
        _searhWindow.Initialize(this);


        nodeCreationRequest = context => SearchWindow.Open(
            new SearchWindowContext(context.screenMousePosition),
            _searhWindow
        );
    }

    private void AddMinimap() {
        _miniMap = new() {
            anchored = true,
        };

        _miniMap.SetPosition(new(15, 15, 200, 180));
        _miniMap.visible = false;
        Add(_miniMap);
    }
    #endregion

    #region Utilities
    public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearhWindow = false) {
        Vector2 worldMousePosition = mousePosition;
        if (isSearhWindow)
            worldMousePosition -= _editorWindow.position.position;

        Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);
        return localMousePosition;
    }

    public void ClearGraph() {
        graphElements.ForEach(graphElement => RemoveElement(graphElement));
        _groupedNodes.Clear();
        _groups.Clear();
        _ungroupedNodes.Clear();
        _nodeLevelMapping.Clear();
        _repeatedNamesCount = 0;
        
        // Hide sidebar when clearing graph
        if (_sidebar != null)
            _sidebar.Hide();
        
        _selectedNode = null;
    }

    public void ChangeMinimapState() {
        _miniMap.visible = !_miniMap.visible;
    }
    
    /// <summary>
    /// Registers a mapping between a node ID and its Level ScriptableObject
    /// </summary>
    public void RegisterNodeLevelMapping(string nodeId, Level Level) {
        if (Level == null) {
            Debug.LogWarning($"Attempted to register null Level for node {nodeId}");
            return;
        }
        
        _nodeLevelMapping[nodeId] = Level;
        
        // If this node is currently selected, update the sidebar
        if (_selectedNode != null && _selectedNode.ID == nodeId) {
            _sidebar.ShowNode(_selectedNode, Level);
        }
    }
    
    /// <summary>
    /// Selects a node and displays its properties in the sidebar
    /// </summary>
    private void SelectNode(LevelBaseNode node) {
        // Deselect previous node
        if (_selectedNode != null) {
            _selectedNode.ResetStyle();
        }
        
        _selectedNode = node;
        
        // Highlight selected node with a subtle blue tint
        _selectedNode.SetErrorStyle(new Color(0.3f, 0.5f, 0.8f, 0.3f));
        
        // Show sidebar with node data
        if (_nodeLevelMapping.TryGetValue(node.ID, out Level Level)) {
            _sidebar.ShowNode(node, Level);
        } else {
            // THIS IS THE FIX:
            // This node is new and hasn't been saved yet.
            // Hide the sidebar, as there is no Level SO to display.
            _sidebar.Hide();
            Debug.LogWarning($"No Level mapping found for node {node.LevelName} (ID: {node.ID})");
        }
    }
    
    /// <summary>
    /// Deselects the currently selected node and hides the sidebar
    /// </summary>
    private void DeselectNode() {
        if (_selectedNode != null) {
            _selectedNode.ResetStyle();
            _selectedNode = null;
        }

        _sidebar.Hide();
    }

    /// <summary>
    /// Called when a Level ScriptableObject is changed
    /// </summary>
    public void OnLevelChanged(Level changedLevel) {
        if (_selectedNode != null && _nodeLevelMapping.TryGetValue(_selectedNode.ID, out Level Level) && Level == changedLevel) {
            _sidebar.ShowNode(_selectedNode, changedLevel);
        }
    }

    /// <summary>
    /// Called when a node's data has changed (e.g., text, properties)
    /// Updates the sidebar if the changed node is currently selected
    /// </summary>
    public void OnNodeDataChanged(LevelBaseNode node) {
        if (_selectedNode == node && _nodeLevelMapping.TryGetValue(node.ID, out Level Level)) {
            _sidebar.ShowNode(node, Level);
        }
    }
    #endregion

    #region Repeated Elements
    public void AddUngroupedNode(LevelBaseNode node) {
        string nodeName = node.LevelName.ToLower();
        if (!_ungroupedNodes.ContainsKey(nodeName))
            _ungroupedNodes[nodeName] = new();

        _ungroupedNodes[nodeName].AddNode(node);
        if (_ungroupedNodes[nodeName].IsError)
            ChangeRepeatedNamesCount(1);
    }

    public void RemoveUngroupedNode(LevelBaseNode node) {
        string nodeName = node.LevelName.ToLower();
        _ungroupedNodes[nodeName].RemoveNode(node);

        if (_ungroupedNodes[nodeName].IsEmpty())
            _ungroupedNodes.Remove(nodeName);
        else
            ChangeRepeatedNamesCount(-1);
    }

    public void AddGroupedNode(LevelBaseNode node, LevelSystemGroup group) {
        string nodeName = node.LevelName.ToLower();
        node.ChangeGroup(group);

        if (!_groupedNodes.ContainsKey(group))
            _groupedNodes[group] = new();

        if (!_groupedNodes[group].ContainsKey(nodeName))
            _groupedNodes[group][nodeName] = new();

        _groupedNodes[group][nodeName].AddNode(node);
        if (_groupedNodes[group][nodeName].IsError)
            ChangeRepeatedNamesCount(1);
    }

    public void RemoveGroupedNode(LevelBaseNode node, Group group) {
        string nodeName = node.LevelName.ToLower();
        node.ChangeGroup(null);
        _groupedNodes[group][nodeName].RemoveNode(node);

        if (_groupedNodes[group][nodeName].IsEmpty())
            _groupedNodes[group].Remove(nodeName);
        else
            ChangeRepeatedNamesCount(-1);

        if (_groupedNodes[group].Count == 0)
            _groupedNodes.Remove(group);
    }

    private void AddGroup(LevelSystemGroup group) {
        string groupName = group.OldTitle.ToLower();
        if (!_groups.ContainsKey(groupName))
            _groups[groupName] = new();

        _groups[groupName].AddGroup(group);
        if (_groups[groupName].IsError)
            ChangeRepeatedNamesCount(1);
    }

    public void RemoveGroup(LevelSystemGroup group) {
        string groupName = group.OldTitle.ToLower();
        _groups[groupName].RemoveGroup(group);

        if (_groups[groupName].IsEmpty())
            _groups.Remove(groupName);
        else
            ChangeRepeatedNamesCount(-1);
    }

    private void ChangeRepeatedNamesCount(int value) {
        _repeatedNamesCount += value;
        _editorWindow.ChangeSaveButtonState(_repeatedNamesCount == 0);
    }
    #endregion
}