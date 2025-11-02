using UnityEngine;

public class DialogueSystemErrorData {
    private Color _color;

    public Color Color => _color;

    public DialogueSystemErrorData() {
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
