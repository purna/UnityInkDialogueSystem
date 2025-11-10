using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class LevelSystemGroup : Group {
    private string _id;
    private readonly Color _defaultBorderColor;
    private readonly float _defaultBorderWidth;
    private string _oldTitle;

    public string OldTitle => _oldTitle;
    public string ID => _id;

    public LevelSystemGroup(string groupTitle, Vector2 position) {
        title = groupTitle;
        _id = Guid.NewGuid().ToString();
        _oldTitle = groupTitle;
        SetPosition(new Rect(position, Vector2.zero));

        _defaultBorderColor = contentContainer.style.borderBottomColor.value;
        _defaultBorderWidth = contentContainer.style.borderBottomWidth.value;
    }

    public void SetErrorStyle(Color color) {
        contentContainer.style.borderBottomColor = color;
        contentContainer.style.borderBottomWidth = 2f;
    }

    public void ResetStyle() {
        contentContainer.style.borderBottomColor = _defaultBorderColor;
        contentContainer.style.borderBottomWidth = _defaultBorderWidth;
    }

    public void UpdateTitle() {
        _oldTitle = title;
    }

    public void SetID(string ID) {
        _id = ID;
    }
}
