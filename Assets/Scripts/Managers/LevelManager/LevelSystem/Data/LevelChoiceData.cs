using System;
using UnityEngine;

[Serializable]
public class LevelChoiceData {
    [SerializeField] private string _text;
    [SerializeField] private Level _nextLevel;

    public string Text => _text;
    public Level NextLevel => _nextLevel;

    public LevelChoiceData(string text) {
        _text = text;
    }

    public void SetNextLevel(Level nextLevel) {
        _nextLevel = nextLevel;
    }
}
