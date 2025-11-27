using System;
using UnityEngine;

[Serializable]
public class SkillsTreeChoiceData {
    [SerializeField] private string _text;
    [SerializeField] private Skill _nextSkillsTree;

    public string Text => _text;
    public Skill NextSkillsTree => _nextSkillsTree;

    public SkillsTreeChoiceData(string text) {
        _text = text;
    }

    public void SetNextSkillsTree(Skill nextSkillsTree) {
        _nextSkillsTree = nextSkillsTree;
    }
}
