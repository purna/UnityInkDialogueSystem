using System.IO;
using UnityEditor;

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
