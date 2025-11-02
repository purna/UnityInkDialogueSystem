using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DialogueSystemSearchWindow : ScriptableObject, ISearchWindowProvider {
    private DialogueSystemGraphView _graphView;
    private Texture2D _indentationIcon;

    public void Initialize(DialogueSystemGraphView graphView) {
        _graphView = graphView;

        _indentationIcon = new(1, 1);
        _indentationIcon.SetPixel(0, 0, Color.clear);
        _indentationIcon.Apply();
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
        List<SearchTreeEntry> searchTreeEntries = new() {
            new SearchTreeGroupEntry(new GUIContent("Create Element")),
            new SearchTreeGroupEntry(new GUIContent("Dialogue Node"), 1),
            new SearchTreeEntry(new GUIContent("Single Choice", _indentationIcon)){
                level = 2,
                userData = DialogueType.SingleChoice
            },
            new SearchTreeEntry(new GUIContent("Multiple Choice", _indentationIcon)){
                level = 2,
                userData = DialogueType.MultipleChoice
            },
            new SearchTreeGroupEntry(new GUIContent("Dialogue Group"), 1),
            new SearchTreeEntry(new GUIContent("Single Group", _indentationIcon)){
                level = 2,
                userData = new Group()
            },
        };

        return searchTreeEntries;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context) {
        switch (SearchTreeEntry.userData) {
            case DialogueType.SingleChoice:
                _graphView.CreateNode(
                    "DialogueName",
                    DialogueType.SingleChoice,
                    _graphView.GetLocalMousePosition(context.screenMousePosition, true)
                );
                break;
            case DialogueType.MultipleChoice:
                _graphView.CreateNode(
                    "DialogueName",
                    DialogueType.MultipleChoice,
                    _graphView.GetLocalMousePosition(context.screenMousePosition, true)
                );
                break;
            case Group _:
                _graphView.CreateGroup("Dialogue group", context.screenMousePosition);
                break;
            default:
                return false;
        }

        return true;
    }
}
