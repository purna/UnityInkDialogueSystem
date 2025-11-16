using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public static class AssetsUtility
{
    public static TAsset CreateAsset<TAsset>(string path, string assetName) where TAsset : ScriptableObject
    {
        TAsset asset = LoadAsset<TAsset>(path, assetName);
        if (asset != null)
            return asset;

        asset = ScriptableObject.CreateInstance<TAsset>();
        string fullPath = $"{path}/{assetName}.asset";
        AssetDatabase.CreateAsset(asset, fullPath);
        return asset;
    }

    public static List<TAsset> GetAssets<TAsset>(string path) where TAsset : ScriptableObject
    {
        string[] files = Directory.GetFiles(path, "*.asset", SearchOption.AllDirectories);
        List<TAsset> result = new();

        foreach (string file in files)
        {
            TAsset asset = LoadAsset<TAsset>(file);
            if (asset != null)
                result.Add(asset);
        }

        return result;
    }

    public static TAsset LoadAsset<TAsset>(string path, string assetName) where TAsset : ScriptableObject
    {
        return LoadAsset<TAsset>($"{path}/{assetName}.asset");
    }

    public static TAsset LoadAsset<TAsset>(string path) where TAsset : ScriptableObject
    {
        return AssetDatabase.LoadAssetAtPath<TAsset>(path);
    }

    public static void Save(this Object asset)
    {
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static void RemoveAsset(string path, string assetName)
    {
        string fullPath = $"{path}/{assetName}.asset";
        AssetDatabase.DeleteAsset(fullPath);
    }

    /// <summary>
    /// Renames an asset file
    /// </summary>
    public static bool RenameAsset(string path, string oldName, string newName)
    {
        string fullPath = $"{path}/{oldName}.asset";
        
        if (!File.Exists(fullPath))
        {
            Debug.LogWarning($"Cannot rename asset: {fullPath} does not exist");
            return false;
        }

        string error = AssetDatabase.RenameAsset(fullPath, newName);
        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError($"Failed to rename asset {oldName} to {newName}: {error}");
            return false;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return true;
    }

    /// <summary>
    /// Moves an asset from one path to another
    /// </summary>
    public static bool MoveAsset(string oldPath, string newPath, string assetName)
    {
        string oldFullPath = $"{oldPath}/{assetName}.asset";
        string newFullPath = $"{newPath}/{assetName}.asset";

        if (!File.Exists(oldFullPath))
        {
            Debug.LogWarning($"Cannot move asset: {oldFullPath} does not exist");
            return false;
        }

        string error = AssetDatabase.MoveAsset(oldFullPath, newFullPath);
        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError($"Failed to move asset from {oldFullPath} to {newFullPath}: {error}");
            return false;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return true;
    }
}
#endif