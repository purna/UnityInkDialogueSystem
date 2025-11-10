using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

[CustomEditor(typeof(LevelNode))]
public class LevelSystemInspector : Editor {
    private SerializedProperty _levelsManager;
    private SerializedProperty _levelsContainer;
    private SerializedProperty _levelsGroup;
    private SerializedProperty _levels;

    private SerializedProperty _isGroupedLevels;
    private SerializedProperty _isStartingLevels;

    private SerializedProperty _selectedLevelsGroupIndex;
    private SerializedProperty _selectedLevelsIndex;

    private void OnEnable() {
        // Use the actual field names from LevelsChoicer class
        _levelsContainer = serializedObject.FindProperty("LevelsContainer");
        _levelsManager = serializedObject.FindProperty("LevelsManager");
        _levelsGroup = serializedObject.FindProperty("LevelsGroup");
        _levels = serializedObject.FindProperty("Levels");

        _isGroupedLevels = serializedObject.FindProperty("isGroupedLevels");
        _isStartingLevels = serializedObject.FindProperty("isStartingLevels");

        _selectedLevelsGroupIndex = serializedObject.FindProperty("selectedLevelsGroupIndex");
        _selectedLevelsIndex = serializedObject.FindProperty("selectedLevelsIndex");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        // Check if property exists before drawing
        if (_levelsManager != null)
        {
            _levelsManager.DrawPropertyField();
        }

        DrawLevelContainerArea();
        LevelContainer levelsContainer = _levelsContainer?.objectReferenceValue as LevelContainer;
        if (levelsContainer == null) {
            StopDrawing("Select Level Container to continue.");
            return;
        }

        DrawFiltersArea();

        List<string> levelNames;
        string levelFolderPath = $"Assets/_Project/ScriptableObjects/Levels/{levelsContainer.name}";
        string levelInfoMessage;

        if (_isGroupedLevels.boolValue) {
            if (!levelsContainer.HasGroups()) {
                StopDrawing("There are no Level groups in this container.");
                return;
            }

            DrawLevelGroupArea(levelsContainer);

            LevelGroup levelGroup = _levelsGroup?.objectReferenceValue as LevelGroup;
            if (levelGroup == null)
            {
                StopDrawing("Please select a Level Group.");
                return;
            }
            
            levelNames = levelsContainer.GetGroupedLevelNames(levelGroup, _isStartingLevels.boolValue);
            levelFolderPath += $"/Groups/{levelGroup.GroupName}/Levels";
            levelInfoMessage = "There are no " + (_isStartingLevels.boolValue ? "Starting " : "") + "Level in this Level Group";
        } else {
            levelNames = levelsContainer.GetUngroupedLevelNames(_isStartingLevels.boolValue);
            levelFolderPath += $"/Global/Levels";
            levelInfoMessage = "There are no " + (_isStartingLevels.boolValue ? "Starting " : "") + "Ungrouped Level in this Level Container";
        }

        if (levelNames.Count == 0) {
            StopDrawing(levelInfoMessage);
            return;
        }

        DrawLevelArea(levelNames, levelFolderPath);

        serializedObject.ApplyModifiedProperties();
    }

    #region Draw
    private void DrawLevelContainerArea() {
        InspectorUtility.DrawHeader("Level Container");
        
        if (_levelsContainer != null)
        {
            _levelsContainer.DrawPropertyField();
        }
        else
        {
            EditorGUILayout.HelpBox("Level Container property not found!", MessageType.Error);
        }
        
        InspectorUtility.DrawSpace();
    }

    private void DrawFiltersArea() {
        InspectorUtility.DrawHeader("Filters");
        
        if (_isGroupedLevels != null)
            _isGroupedLevels.DrawPropertyField();
            
        if (_isStartingLevels != null)
            _isStartingLevels.DrawPropertyField();
            
        InspectorUtility.DrawSpace();
    }

    private void DrawLevelGroupArea(LevelContainer levelContainer) {
        InspectorUtility.DrawHeader("Level Group");
        string[] groupNames = levelContainer.GetGroupsNames();

        if (groupNames.Length == 0)
        {
            EditorGUILayout.HelpBox("No groups found in container.", MessageType.Warning);
            return;
        }

        int oldGroupIndex = _selectedLevelsGroupIndex.intValue;
        LevelGroup oldGroup = _levelsGroup?.objectReferenceValue as LevelGroup;
        bool isOldGroupNull = oldGroup == null;
        string oldGroupName = isOldGroupNull ? string.Empty : oldGroup.GroupName;
        UpdatePropertyIndex(groupNames, _selectedLevelsGroupIndex, oldGroupIndex, isOldGroupNull, oldGroupName);

        _selectedLevelsGroupIndex.intValue = InspectorUtility.DrawPopup("Level Group", _selectedLevelsGroupIndex, groupNames);

        string selectedLevelGroupName = groupNames[_selectedLevelsGroupIndex.intValue];
        LevelGroup selectedLevelGroup = AssetsUtility.LoadAsset<LevelGroup>(
            $"Assets/_Project/ScriptableObjects/Levels/{levelContainer.LevelName}/Groups/{selectedLevelGroupName}", 
            selectedLevelGroupName
        );

        _levelsGroup.objectReferenceValue = selectedLevelGroup;
        
        if (_levelsGroup != null)
        {
            InspectorUtility.DrawDisabledField(() => _levelsGroup.DrawPropertyField());
        }
        
        InspectorUtility.DrawSpace();
    }

    private void DrawLevelArea(List<string> LevelsNames, string LevelsFolderPath) {
        InspectorUtility.DrawHeader("Level");

        int oldLevelIndex = _selectedLevelsIndex.intValue;
        Level oldLevels = _levels?.objectReferenceValue as Level;
        bool isOldLevelNull = oldLevels == null;
        string oldLevelName = isOldLevelNull ? string.Empty : oldLevels.LevelName;
        UpdatePropertyIndex(LevelsNames.ToArray(), _selectedLevelsIndex, oldLevelIndex, isOldLevelNull, oldLevelName);

        _selectedLevelsIndex.intValue = InspectorUtility.DrawPopup("Level", _selectedLevelsIndex, LevelsNames.ToArray());

        string LevelsName = LevelsNames[_selectedLevelsIndex.intValue];
        Level Levels = AssetsUtility.LoadAsset<Level>(LevelsFolderPath, LevelsName);
        
        if (_levels != null)
        {
            _levels.objectReferenceValue = Levels;
            InspectorUtility.DrawDisabledField(() => _levels.DrawPropertyField());
        }
    }

    private void StopDrawing(string reason) {
        InspectorUtility.DrawHelpBox(reason);
        InspectorUtility.DrawSpace();
        InspectorUtility.DrawHelpBox("You need to select a Level for this component to work properly.", MessageType.Warning);
        serializedObject.ApplyModifiedProperties();
    }
    #endregion

    #region IndexMethods
    private void UpdatePropertyIndex(string[] names, SerializedProperty property, int oldPropertyIndex, bool isOldNameNull, string oldName) {
        if (property == null || names == null || names.Length == 0)
        {
            return;
        }
        
        if (isOldNameNull) {
            property.intValue = 0;
            return;
        }

        if (oldPropertyIndex < names.Length && oldName == names[oldPropertyIndex])
            return;

        if (names.Contains(oldName))
            property.intValue = Array.IndexOf(names, oldName);
        else
            property.intValue = 0;
    }
    #endregion
}