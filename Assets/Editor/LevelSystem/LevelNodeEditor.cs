using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Custom editor to show a dropdown for Level selection
/// </summary>
[CustomEditor(typeof(LevelNode))]
public class LevelNodeEditor : Editor
{
    private SerializedProperty _controllerProp;
    private SerializedProperty _levelIndexProp;
    private SerializedProperty _levelProp;
    
    private void OnEnable()
    {
        _controllerProp = serializedObject.FindProperty("_controller");
        _levelIndexProp = serializedObject.FindProperty("_levelIndex");
        _levelProp = serializedObject.FindProperty("_level");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // Draw default inspector
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        DrawSectionHeader("Manual Level Selection");
        
        LevelNode node = (LevelNode)target;
        LevelController controller = _controllerProp.objectReferenceValue as LevelController;
        
        if (controller == null)
        {
            EditorGUILayout.HelpBox("Assign a LevelController to select levels", MessageType.Info);
        }
        else
        {
            List<Level> availableLevels = controller.GetAvailableLevels();
            
            if (availableLevels == null || availableLevels.Count == 0)
            {
                EditorGUILayout.HelpBox("No levels available in the controller's LevelContainer/Group", MessageType.Warning);
            }
            else
            {
                // Create dropdown options
                string[] levelNames = new string[availableLevels.Count + 1];
                levelNames[0] = "-- Select Level --";
                for (int i = 0; i < availableLevels.Count; i++)
                {
                    Level level = availableLevels[i];
                    if (level != null)
                    {
                        string statusIcon = level.IsCompleted ? "✓" : level.IsUnlocked ? "○" : "🔒";
                        levelNames[i + 1] = $"{statusIcon} {level.LevelName}";
                    }
                    else
                    {
                        levelNames[i + 1] = $"Level {i}";
                    }
                }
                
                // Show dropdown
                int currentIndex = _levelIndexProp.intValue + 1;
                int newIndex = EditorGUILayout.Popup("Select Level", currentIndex, levelNames);
                
                if (newIndex != currentIndex)
                {
                    _levelIndexProp.intValue = newIndex - 1;
                    
                    // Auto-assign the level
                    if (newIndex > 0 && newIndex <= availableLevels.Count)
                    {
                        _levelProp.objectReferenceValue = availableLevels[newIndex - 1];
                    }
                    else
                    {
                        _levelProp.objectReferenceValue = null;
                    }
                }
                
                // Show currently selected level info
                if (_levelIndexProp.intValue >= 0 && _levelIndexProp.intValue < availableLevels.Count)
                {
                    Level selectedLevel = availableLevels[_levelIndexProp.intValue];
                    if (selectedLevel != null)
                    {
                        DrawLevelInfo(selectedLevel);
                    }
                }
                
                EditorGUILayout.Space(5);
                
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Refresh Level Assignment"))
                {
                    if (Application.isPlaying)
                    {
                        if (_levelIndexProp.intValue >= 0 && _levelIndexProp.intValue < availableLevels.Count)
                        {
                            node.SetLevel(availableLevels[_levelIndexProp.intValue]);
                        }
                    }
                    else
                    {
                        EditorUtility.SetDirty(target);
                    }
                }
                
                if (GUILayout.Button("Preview"))
                {
                    if (_levelIndexProp.intValue >= 0 && _levelIndexProp.intValue < availableLevels.Count)
                    {
                        Selection.activeObject = availableLevels[_levelIndexProp.intValue];
                        EditorGUIUtility.PingObject(availableLevels[_levelIndexProp.intValue]);
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawLevelInfo(Level level)
    {
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("Selected Level Info:", EditorStyles.miniBoldLabel);
        
        // Basic info
        EditorGUILayout.BeginHorizontal();
        
        // Icon preview
        if (level.Icon != null)
        {
            Rect iconRect = GUILayoutUtility.GetRect(32, 32, GUILayout.Width(32), GUILayout.Height(32));
            GUI.DrawTexture(iconRect, level.Icon.texture, ScaleMode.ScaleToFit);
            GUILayout.Space(5);
        }
        
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Name:", level.LevelName, EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Type:", level.LevelSceneType.ToString());
        EditorGUILayout.LabelField("Tier:", level.Tier.ToString());
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(3);
        
        // Description
        if (!string.IsNullOrEmpty(level.Description))
        {
            EditorGUILayout.LabelField("Description:", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField(level.Description, EditorStyles.wordWrappedMiniLabel);
        }
        
        EditorGUILayout.Space(3);
        
        // Prerequisites and connections
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Prerequisites: {level.Prerequisites.Count}", GUILayout.Width(120));
        EditorGUILayout.LabelField($"Next Levels: {level.Children.Count}");
        EditorGUILayout.EndHorizontal();
        
        // Runtime status
        if (Application.isPlaying)
        {
            EditorGUILayout.Space(3);
            DrawSectionDivider();
            
            EditorGUILayout.LabelField("Runtime Status:", EditorStyles.miniBoldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            // Unlocked status
            string unlockedStatus = level.IsUnlocked ? "✓ Unlocked" : "🔒 Locked";
            GUIStyle unlockedStyle = level.IsUnlocked ? EditorStyles.boldLabel : EditorStyles.label;
            EditorGUILayout.LabelField("Unlock:", unlockedStatus, unlockedStyle);
            
            // Completed status
            string completedStatus = level.IsCompleted ? "✓ Completed" : "○ Not Completed";
            GUIStyle completedStyle = level.IsCompleted ? EditorStyles.boldLabel : EditorStyles.label;
            EditorGUILayout.LabelField("Complete:", completedStatus, completedStyle);
            
            EditorGUILayout.EndHorizontal();
            
            // Can unlock
            if (!level.IsUnlocked)
            {
                bool canUnlock = level.CanUnlock();
                string canUnlockText = canUnlock ? "✓ Can Unlock" : "✗ Cannot Unlock Yet";
                EditorGUILayout.LabelField("Status:", canUnlockText);
            }
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawSectionHeader(string title)
    {
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 12;
        EditorGUILayout.LabelField(title, headerStyle);
        DrawSectionDivider();
    }
    
    private void DrawSectionDivider()
    {
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        rect.height = 1;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
    }
}