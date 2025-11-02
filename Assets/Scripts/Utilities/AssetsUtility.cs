using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public static class AssetsUtility {
    public static TAsset CreateAsset<TAsset>(string path, string assetName) where TAsset : ScriptableObject {
        TAsset asset = LoadAsset<TAsset>(path, assetName);
        if (asset != null)
            return asset;

        asset = ScriptableObject.CreateInstance<TAsset>();
        AssetDatabase.CreateAsset(asset, $"{path}/{assetName}.asset");
        return asset;
    }

    public static List<TAsset> GetAssets<TAsset>(string path) where TAsset : ScriptableObject {
        string[] files = Directory.GetFiles(path, "*.asset", SearchOption.AllDirectories);
        List<TAsset> result = new();

        foreach (string file in files) {
            TAsset asset = LoadAsset<TAsset>(file);
            if (asset != null)
                result.Add(asset);
        }

        return result;
    }

    public static TAsset LoadAsset<TAsset>(string path, string assetName) where TAsset : ScriptableObject {
        return LoadAsset<TAsset>($"{path}/{assetName}.asset");
    }

    public static TAsset LoadAsset<TAsset>(string path) where TAsset : ScriptableObject {
        return AssetDatabase.LoadAssetAtPath<TAsset>(path);
    }

    public static void Save(this Object asset) {
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static void RemoveAsset(string path, string asset) {
        AssetDatabase.DeleteAsset($"{path}/{asset}.asset");
    }
}
#endif