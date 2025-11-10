using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using NodeDirection = UnityEditor.Experimental.GraphView.Direction;

public static class UIElementUtility {
    public static TextField CreateTextField(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null) {
        TextField textField = new() {
            value = value,
            label = label,
        };

        if (onValueChanged != null)
            textField.RegisterValueChangedCallback(onValueChanged);
        return textField;
    }

    public static TextField CreateTextArea(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null) {
        TextField textArea = CreateTextField(value, label, onValueChanged);
        textArea.multiline = true;
        return textArea;
    }

    public static Foldout CreateFoldout(string title, bool collapsed = false) {
        Foldout foldout = new() {
            text = title,
            value = !collapsed
        };

        return foldout;
    }

    public static Button CreateButton(string text, Action onClick = null) {
        Button button = new(onClick) {
            text = text,
        };
        return button;
    }


        // Extension method for DialogueBaseNode
    public static Port CreatePort(this DialogueBaseNode node, string portName = "", Orientation orientation = Orientation.Horizontal, NodeDirection direction = NodeDirection.Output, Port.Capacity capacity = Port.Capacity.Single) {
        Port port = node.InstantiatePort(orientation, direction, capacity, typeof(object));
        port.portName = portName;
        return port;
    }

    // Extension method for SkillTreeBaseNode
    public static Port CreatePort(this SkillsTreeBaseNode node, string portName = "", Orientation orientation = Orientation.Horizontal, NodeDirection direction = NodeDirection.Output, Port.Capacity capacity = Port.Capacity.Single)
    {
        Port port = node.InstantiatePort(orientation, direction, capacity, typeof(object));
        port.portName = portName;
        return port;
    }
    

      // Extension method for LevelBaseNode
    public static Port CreatePort(this LevelBaseNode node, string portName = "", Orientation orientation = Orientation.Horizontal, NodeDirection direction = NodeDirection.Output, Port.Capacity capacity = Port.Capacity.Single)
    {
        Port port = node.InstantiatePort(orientation, direction, capacity, typeof(object));
        port.portName = portName;
        return port;
    }
    

    public static ObjectField CreateObjectField(string title, Type type, UnityEngine.Object value = null, EventCallback<ChangeEvent<UnityEngine.Object>> onValueChanged = null) {
        ObjectField objectField = new() {
            objectType = type,
            label = title,
            value = value,
        };

        if (onValueChanged != null)
            objectField.RegisterValueChangedCallback(onValueChanged);
        return objectField;
    }

    public static EnumField CreateEnumField(string title, Enum type, EventCallback<ChangeEvent<Enum>> onValueChanged = null) {
        EnumField enumField = new(title, type);
        if (onValueChanged != null)
            enumField.RegisterValueChangedCallback(onValueChanged);
        return enumField;
    }

    public static Toggle CreateToggle(string label, bool value = false, EventCallback<ChangeEvent<bool>> onValueChanged = null) {
        Toggle toggle = new() {
            label = label,
            value = value
        };

        if (onValueChanged != null)
            toggle.RegisterValueChangedCallback(onValueChanged);
        return toggle;
    }

    public static Toggle CreateToggle(bool value, string label = null, EventCallback<ChangeEvent<bool>> onValueChanged = null) {
        Toggle toggle = new() {
            label = label,
            value = value
        };

        if (onValueChanged != null)
            toggle.RegisterValueChangedCallback(onValueChanged);
        return toggle;
    }

    public static IntegerField CreateIntegerField(int value = 0, string label = null, EventCallback<ChangeEvent<int>> onValueChanged = null) {
        IntegerField integerField = new() {
            label = label,
            value = value
        };

        if (onValueChanged != null)
            integerField.RegisterValueChangedCallback(onValueChanged);
        return integerField;
    }

    public static FloatField CreateFloatField(float value = 0f, string label = null, EventCallback<ChangeEvent<float>> onValueChanged = null) {
        FloatField floatField = new() {
            label = label,
            value = value
        };

        if (onValueChanged != null)
            floatField.RegisterValueChangedCallback(onValueChanged);
        return floatField;
    }
}