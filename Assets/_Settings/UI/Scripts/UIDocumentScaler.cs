using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Automatically applies 4K or default USS to all child UIDocuments
/// </summary>
public class UIDocumentManagerScaler : MonoBehaviour
{
    [Header("USS References")]
    [SerializeField] private StyleSheet defaultUSS;  // e.g., SettingsMenu.uss
    [SerializeField] private StyleSheet uss4K;       // e.g., SettingsMenu.4k.uss

    [Header("4K Threshold")]
    [SerializeField] private int minWidth4K = 3840;
    [SerializeField] private int minHeight4K = 2160;

    private void Start()
    {
        
        Debug.Log($"Screen Resolution: {Screen.width}x{Screen.height}");
        Debug.Log($"4K Detected: {Screen.width >= minWidth4K || Screen.height >= minHeight4K}");
        Debug.Log($"Default USS: {(defaultUSS != null ? defaultUSS.name : "NULL")}");
        Debug.Log($"4K USS: {(uss4K != null ? uss4K.name : "NULL")}");
            

        // Detect all UIDocuments under this GameObject
        var uiDocs = GetComponentsInChildren<UIDocument>(true);

        bool is4K = Screen.width >= minWidth4K || Screen.height >= minHeight4K;

        foreach (var doc in uiDocs)
        {
            var root = doc.rootVisualElement;
            if (root == null) continue;

            if (is4K && uss4K != null)
            {
                // Clear previous USS to avoid conflicts
                root.styleSheets.Clear();

                // Add appropriate USS
                root.styleSheets.Add(uss4K);

                // Force UI to rebuild
                root.MarkDirtyRepaint();
                
                Debug.Log($"Applied USS to {doc.name}: {(is4K ? "4K" : "Default")}");
            }


        }
    }
}
