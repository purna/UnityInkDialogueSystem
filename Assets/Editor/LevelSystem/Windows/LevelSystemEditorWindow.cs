using System;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class LevelSystemEditorWindow : EditorWindow {
    private LevelSystemGraphView _graphView;
    private readonly string _defaultFileName = "LevelName";
    private static TextField _fileNameField;
    private Button _saveButton;
    private Button _minimapButton;

    private Button _inspectorButton;


    private VisualElement _mainContainer;

    [MenuItem("Window/Game Systems/Level Graph")]
    public static void ShowExample() {
        GetWindow<LevelSystemEditorWindow>("Level Graph");
    }

    /// <summary>
    /// Opens the Level System window with a specific container
    /// </summary>
    public static void OpenWithContainer(LevelContainer container) {
        LevelSystemEditorWindow window = GetWindow<LevelSystemEditorWindow>("Level Graph");
        window.LoadContainer(container);
    }

    private void CreateGUI() {
        // Main container to hold graph and sidebar
        _mainContainer = new VisualElement();
        _mainContainer.style.flexDirection = FlexDirection.Row;
        _mainContainer.style.flexGrow = 1;
        
        AddGraphView();
        AddToolbar();
        AddSidebar();
        AddStyles();
    }

    /// <summary>
    /// Loads a Level container into the graph view
    /// </summary>
    public void LoadContainer(LevelContainer container) {
        if (container == null) {
            Debug.LogWarning("Cannot load null container!");
            return;
        }

        // Clear current graph
        if (_graphView != null) {
            _graphView.ClearGraph();
        }

        // Update filename to match container
        UpdateFileName(container.name);

        // Load using the existing save manager system
        LevelSystemSaveManager.Initialize(_graphView, container.name);
        LevelSystemSaveManager.Load();

        // Update the window title
        titleContent = new GUIContent($"Level Graph - {container.name}");

        Debug.Log($"Loaded container: {container.name}");
    }

    private void AddGraphView() {
        _graphView = new(this);
        _graphView.style.flexGrow = 1;
        _mainContainer.Add(_graphView);
        rootVisualElement.Add(_mainContainer);
    }
    
    private void AddSidebar() {
        VisualElement sidebar = _graphView.Sidebar.CreateSidebar();
        _mainContainer.Add(sidebar);
    }

    private void AddToolbar() {
        Toolbar toolbar = new();
        toolbar.AddStyleSheets("LevelSystem/Styles/DSToolbarStyles.uss");

        _fileNameField = UIElementUtility.CreateTextField(_defaultFileName, "File Name: ", callback => {
            _fileNameField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
        });
        toolbar.Add(_fileNameField);

        _saveButton = UIElementUtility.CreateButton("Save", Save);
        toolbar.Add(_saveButton);

        Button loadButton = UIElementUtility.CreateButton("Load", Load);
        toolbar.Add(loadButton);

        Button clearButton = UIElementUtility.CreateButton("Clear", _graphView.ClearGraph);
        toolbar.Add(clearButton);

        Button resetButton = UIElementUtility.CreateButton("Reset", ResetGraph);
        toolbar.Add(resetButton);

        _minimapButton = UIElementUtility.CreateButton("Minimap", ChangeMinimapState);
        toolbar.Add(_minimapButton);

         _inspectorButton = UIElementUtility.CreateButton("Inspector", ToggleInspector);
        toolbar.Add(_inspectorButton);
    

        rootVisualElement.Insert(0, toolbar);
    }

    private void ResetGraph() {
        _graphView.ClearGraph();
        UpdateFileName(_defaultFileName);
        titleContent = new GUIContent("Level Graph");
    }

    private void Save() {
        if (string.IsNullOrEmpty(_fileNameField.value)) {
            EditorUtility.DisplayDialog("Invalid file name", "Change it and try again", "Ok");
            return;
        }

        LevelSystemSaveManager.Initialize(_graphView, _fileNameField.value);
        LevelSystemSaveManager.Save();
    }

    private void Load() {
        string filePath = EditorUtility.OpenFilePanel("Level Graphs", "Assets/_Project/Editor/LevelSystem/Graphs", "asset");
        if (string.IsNullOrEmpty(filePath))
            return;

        _graphView.ClearGraph();
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        UpdateFileName(fileName);
        LevelSystemSaveManager.Initialize(_graphView, fileName);
        LevelSystemSaveManager.Load();
    }

    private void AddStyles() {
        rootVisualElement.AddStyleSheets("LevelSystem/Styles/DSVariables.uss");
    }

    public void ChangeSaveButtonState(bool newState) {
        _saveButton.SetEnabled(newState);
    }

    private void ChangeMinimapState() {
        _graphView.ChangeMinimapState();
        _minimapButton.ToggleInClassList("ds-toolbar__button__selected");
    }

    public static void UpdateFileName(string fileName) {
        _fileNameField.value = fileName;
    }

    /// <summary>
    /// Called when a Level ScriptableObject is changed in the inspector
    /// </summary>
    public void OnLevelChanged(Level changedLevel)
    {
        if (_graphView != null)
        {
            _graphView.OnLevelChanged(changedLevel);
        }
    }
    
    private void ToggleInspector()
{
    if (_graphView?.Sidebar != null)
    {
        if (_graphView.Sidebar.IsVisible)
        {
            _graphView.Sidebar.Hide();
            _inspectorButton.RemoveFromClassList("ds-toolbar__button__selected");
        }
        else
        {
            // Show the sidebar but don't select a node
            // (it will remain empty until a node is clicked)
            _inspectorButton.AddToClassList("ds-toolbar__button__selected");
        }
    }
}
}