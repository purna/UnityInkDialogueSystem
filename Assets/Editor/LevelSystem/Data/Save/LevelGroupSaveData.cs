using System;
using UnityEngine;

[Serializable]
public class LevelGroupSaveData {
    [SerializeField] private string _ID;
    [SerializeField] private string _name;
    [SerializeField] private Vector2 _position;

    public string ID => _ID;
    public string Name => _name;
    public Vector2 Position => _position;

    public LevelGroupSaveData(string ID, string name, Vector2 position) {
        _ID = ID;
        _name = name;
        _position = position;
    }
}
