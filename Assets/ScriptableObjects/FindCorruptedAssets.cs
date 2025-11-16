#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class FindCorruptedAssets : EditorWindow
{
    [MenuItem("Tools/Find Corrupted Assets")]
    static void FindCorrupted()
    {
        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            try
            {
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (obj == null)
                {
                    Debug.LogError($"Failed to load: {path}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading {path}: {e.Message}");
            }
        }
        
        Debug.Log("Scan complete!");
    }
}
#endif