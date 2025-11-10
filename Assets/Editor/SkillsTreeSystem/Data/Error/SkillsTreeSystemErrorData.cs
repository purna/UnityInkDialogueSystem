using UnityEngine;

public class SkillsTreeSystemErrorData {
    private Color _color;

    public Color Color => _color;

    public SkillsTreeSystemErrorData() {
        GenerateRandomColor();
    }

    private void GenerateRandomColor() {
        _color = new Color32(
            (byte)Random.Range(65, 256),
            (byte)Random.Range(50, 176),
            (byte)Random.Range(50, 176),
            255
        );
    }
}
