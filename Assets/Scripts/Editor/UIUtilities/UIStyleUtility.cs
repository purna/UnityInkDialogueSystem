using UnityEditor;
using UnityEngine.UIElements;

public static class UIStyleUtility {
    public static VisualElement AddStyleSheets(this VisualElement element, params string[] styleSheetNames) {
        foreach (var styleSheetName in styleSheetNames) {
            StyleSheet styleSheet = EditorGUIUtility.Load(styleSheetName) as StyleSheet;
            if (styleSheet == null)
                continue;

            element.styleSheets.Add(styleSheet);
        }

        return element;
    }

    public static VisualElement AddClasses(this VisualElement element, params string[] classNames) {
        foreach (var className in classNames)
            element.AddToClassList(className);
        return element;
    }
}
