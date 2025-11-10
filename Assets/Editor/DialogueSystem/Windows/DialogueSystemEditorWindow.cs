using System;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class DialogueSystemEditorWindow : EditorWindow {
    private DialogueSystemGraphView _graphView;
    private readonly string _defaultFileName = "DialogueName";
    private static TextField _fileNameField;
    private Button _saveButton;
    private Button _minimapButton;

    [MenuItem("Window/Game Systems/Dialogue Graph")]
    public static void ShowExample() {
        GetWindow<DialogueSystemEditorWindow>("Dialogue Graph");
    }

    /// <summary>
    /// Opens the Dialogue System window with a specific container
    /// </summary>
    public static void OpenWithContainer(DialogueContainer container) {
        DialogueSystemEditorWindow window = GetWindow<DialogueSystemEditorWindow>("Dialogue Graph");
        window.LoadContainer(container);
    }

    private void CreateGUI() {
        AddGraphView();
        AddToolbar();
        AddStyles();
    }

    /// <summary>
    /// Loads a dialogue container into the graph view
    /// </summary>
    public void LoadContainer(DialogueContainer container) {
        if (container == null) {
            Debug.LogWarning("Cannot load null container!");
            return;
        }

        // Clear current graph
        if (_graphView != null) {
            _graphView.ClearGraph();
        }

        // Update filename to match container
        UpdateFileName(container.FileName);

        // Load using the existing save manager system
        DialogueSystemSaveManager.Initialize(_graphView, container.FileName);
        DialogueSystemSaveManager.Load();

        // Update the window title
        titleContent = new GUIContent($"Dialogue Graph - {container.name}");

        Debug.Log($"Loaded container: {container.name}");
    }

    private void AddGraphView() {
        _graphView = new(this);
        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }

    private void AddToolbar() {
        Toolbar toolbar = new();
        toolbar.AddStyleSheets("DialogueSystem/Styles/DSToolbarStyles.uss");

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

        rootVisualElement.Add(toolbar);
    }

    private void ResetGraph() {
        _graphView.ClearGraph();
        UpdateFileName(_defaultFileName);
        titleContent = new GUIContent("Dialogue Graph");
    }

    private void Save() {
        if (string.IsNullOrEmpty(_fileNameField.value)) {
            EditorUtility.DisplayDialog("Invalid file name", "Change it and try again", "Ok");
            return;
        }

        DialogueSystemSaveManager.Initialize(_graphView, _fileNameField.value);
        DialogueSystemSaveManager.Save();
    }

    private void Load() {
        string filePath = EditorUtility.OpenFilePanel("Dialogue Graphs", "Assets/_Project/Editor/DialogueSystem/Graphs", "asset");
        if (string.IsNullOrEmpty(filePath))
            return;

        _graphView.ClearGraph();
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        UpdateFileName(fileName);
        DialogueSystemSaveManager.Initialize(_graphView, fileName);
        DialogueSystemSaveManager.Load();
    }

    private void AddStyles() {
        rootVisualElement.AddStyleSheets("DialogueSystem/Styles/DSVariables.uss");
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
}