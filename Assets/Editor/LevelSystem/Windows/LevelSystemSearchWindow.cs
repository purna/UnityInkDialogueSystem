using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class LevelSystemSearchWindow : ScriptableObject, ISearchWindowProvider {
    private LevelSystemGraphView _graphView;
    private Texture2D _indentationIcon;

    public void Initialize(LevelSystemGraphView graphView) {
        _graphView = graphView;

        _indentationIcon = new(1, 1);
        _indentationIcon.SetPixel(0, 0, Color.clear);
        _indentationIcon.Apply();
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
        List<SearchTreeEntry> searchTreeEntries = new() {
            new SearchTreeGroupEntry(new GUIContent("Create Element")),
            new SearchTreeGroupEntry(new GUIContent("Level Node"), 1),
            new SearchTreeEntry(new GUIContent("Single Choice", _indentationIcon)){
                level = 2,
                userData = LevelType.SingleChoice
            },
            new SearchTreeEntry(new GUIContent("Multiple Choice", _indentationIcon)){
                level = 2,
                userData = LevelType.MultipleChoice
            },
            new SearchTreeGroupEntry(new GUIContent("Level Group"), 1),
            new SearchTreeEntry(new GUIContent("Single Group", _indentationIcon)){
                level = 2,
                userData = new Group()
            },
        };

        return searchTreeEntries;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context) {
        switch (SearchTreeEntry.userData) {
            case LevelType.SingleChoice:
                _graphView.CreateNode(
                    "LevelName",
                    LevelType.SingleChoice,
                    _graphView.GetLocalMousePosition(context.screenMousePosition, true)
                );
                break;
            case LevelType.MultipleChoice:
                _graphView.CreateNode(
                    "LevelName",
                    LevelType.MultipleChoice,
                    _graphView.GetLocalMousePosition(context.screenMousePosition, true)
                );
                break;
            case Group _:
                _graphView.CreateGroup("Level group", context.screenMousePosition);
                break;
            default:
                return false;
        }

        return true;
    }
}
