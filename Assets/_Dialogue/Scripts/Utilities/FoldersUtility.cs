using System.IO;
using UnityEditor;
using UnityEngine;

public static class FoldersUtility {
    public static void CreateFolder(string path, string folderName) {
        string fullPath = $"{path}/{folderName}";
        if (Directory.Exists(fullPath))
            return;

        Directory.CreateDirectory(fullPath);
    }

    public static void DeleteFolder(string path) {
        if (!Directory.Exists(path))
            return;

        Directory.Delete(path);
    }

    /// <summary>
    /// Renames a folder in the Unity project
    /// </summary>
    public static bool RenameEditorFolder(string fullPath, string newName)
    {
        if (!AssetDatabase.IsValidFolder(fullPath))
        {
            Debug.LogWarning($"Cannot rename folder: {fullPath} does not exist");
            return false;
        }

        string error = AssetDatabase.RenameAsset(fullPath, newName);
        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError($"Failed to rename folder {fullPath} to {newName}: {error}");
            return false;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return true;
    }

    /// <summary>
    /// Gets the folder name from a full path
    /// </summary>
    public static string GetFolderName(string fullPath)
    {
        return System.IO.Path.GetFileName(fullPath);
    }

    /// <summary>
    /// Gets the parent path from a full path
    /// </summary>
    public static string GetParentPath(string fullPath)
    {
        return System.IO.Path.GetDirectoryName(fullPath).Replace("\\", "/");
    }


#if UNITY_EDITOR
    public static void CreateEditorFolder(string path, string folderName) {
        if (AssetDatabase.IsValidFolder($"{path}/{folderName}"))
            return;

        AssetDatabase.CreateFolder(path, folderName);
    }

    public static void DeleteEditorFolder(string path) {
        if (!AssetDatabase.IsValidFolder(path))
            return;

        FileUtil.DeleteFileOrDirectory($"{path}.meta");
        FileUtil.DeleteFileOrDirectory(path);
    }
#endif
}
