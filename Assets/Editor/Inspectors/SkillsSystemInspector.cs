using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

[CustomEditor(typeof(SkillNode))]
public class SkillsSystemInspector : Editor {
    private SerializedProperty _skillsTreeManager;
    private SerializedProperty _skillsTreeContainer;
    private SerializedProperty _skillsTreeGroup;
    private SerializedProperty _skillsTree;

    private SerializedProperty _isGroupedSkillsTrees;
    private SerializedProperty _isStartingSkillsTrees;

    private SerializedProperty _selectedSkillsTreeGroupIndex;
    private SerializedProperty _selectedSkillsTreeIndex;

    private void OnEnable() {
        // Use the actual field names from SkillsTreeChoicer class
        _skillsTreeContainer = serializedObject.FindProperty("skillsTreeContainer");
        _skillsTreeManager = serializedObject.FindProperty("skillsTreeManager");
        _skillsTreeGroup = serializedObject.FindProperty("skillsTreeGroup");
        _skillsTree = serializedObject.FindProperty("skillsTree");

        _isGroupedSkillsTrees = serializedObject.FindProperty("isGroupedSkillsTrees");
        _isStartingSkillsTrees = serializedObject.FindProperty("isStartingSkillsTrees");

        _selectedSkillsTreeGroupIndex = serializedObject.FindProperty("selectedSkillsTreeGroupIndex");
        _selectedSkillsTreeIndex = serializedObject.FindProperty("selectedSkillsTreeIndex");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        // Check if property exists before drawing
        if (_skillsTreeManager != null)
        {
            _skillsTreeManager.DrawPropertyField();
        }

        DrawSkillsContainerArea();
        SkillsTreeContainer skillsTreeContainer = _skillsTreeContainer?.objectReferenceValue as SkillsTreeContainer;
        if (skillsTreeContainer == null) {
            StopDrawing("Select Skills Container to continue.");
            return;
        }

        DrawFiltersArea();

        List<string> skillsNames;
        string skillsFolderPath = $"Assets/_Project/ScriptableObjects/SkillsTree/{skillsTreeContainer.TreeName}";
        string skillsInfoMessage;

        if (_isGroupedSkillsTrees.boolValue) {
            if (!skillsTreeContainer.HasGroups()) {
                StopDrawing("There are no skills groups in this container.");
                return;
            }

            DrawSkillsGroupArea(skillsTreeContainer);

            SkillsTreeGroup skillsTreeGroup = _skillsTreeGroup?.objectReferenceValue as SkillsTreeGroup;
            if (skillsTreeGroup == null)
            {
                StopDrawing("Please select a Skills Group.");
                return;
            }
            
            skillsNames = skillsTreeContainer.GetGroupedSkillNames(skillsTreeGroup, _isStartingSkillsTrees.boolValue);
            skillsFolderPath += $"/Groups/{skillsTreeGroup.GroupName}/SkillsTree";
            skillsInfoMessage = "There are no " + (_isStartingSkillsTrees.boolValue ? "Starting " : "") + "Skills in this Skills Group";
        } else {
            skillsNames = skillsTreeContainer.GetUngroupedSkillNames(_isStartingSkillsTrees.boolValue);
            skillsFolderPath += $"/Global/SkillsTree";
            skillsInfoMessage = "There are no " + (_isStartingSkillsTrees.boolValue ? "Starting " : "") + "Ungrouped Skills in this Skills Container";
        }

        if (skillsNames.Count == 0) {
            StopDrawing(skillsInfoMessage);
            return;
        }

        DrawSkillsArea(skillsNames, skillsFolderPath);

        serializedObject.ApplyModifiedProperties();
    }

    #region Draw
    private void DrawSkillsContainerArea() {
        InspectorUtility.DrawHeader("Skills Container");
        
        if (_skillsTreeContainer != null)
        {
            _skillsTreeContainer.DrawPropertyField();
        }
        else
        {
            EditorGUILayout.HelpBox("Skills Container property not found!", MessageType.Error);
        }
        
        InspectorUtility.DrawSpace();
    }

    private void DrawFiltersArea() {
        InspectorUtility.DrawHeader("Filters");
        
        if (_isGroupedSkillsTrees != null)
            _isGroupedSkillsTrees.DrawPropertyField();
            
        if (_isStartingSkillsTrees != null)
            _isStartingSkillsTrees.DrawPropertyField();
            
        InspectorUtility.DrawSpace();
    }

    private void DrawSkillsGroupArea(SkillsTreeContainer skillsContainer) {
        InspectorUtility.DrawHeader("Skills Group");
        string[] groupNames = skillsContainer.GetGroupsNames();

        if (groupNames.Length == 0)
        {
            EditorGUILayout.HelpBox("No groups found in container.", MessageType.Warning);
            return;
        }

        int oldGroupIndex = _selectedSkillsTreeGroupIndex.intValue;
        SkillsTreeGroup oldGroup = _skillsTreeGroup?.objectReferenceValue as SkillsTreeGroup;
        bool isOldGroupNull = oldGroup == null;
        string oldGroupName = isOldGroupNull ? string.Empty : oldGroup.GroupName;
        UpdatePropertyIndex(groupNames, _selectedSkillsTreeGroupIndex, oldGroupIndex, isOldGroupNull, oldGroupName);

        _selectedSkillsTreeGroupIndex.intValue = InspectorUtility.DrawPopup("Skills Group", _selectedSkillsTreeGroupIndex, groupNames);

        string selectedSkillsGroupName = groupNames[_selectedSkillsTreeGroupIndex.intValue];
        SkillsTreeGroup selectedSkillsGroup = AssetsUtility.LoadAsset<SkillsTreeGroup>(
            $"Assets/_Project/ScriptableObjects/SkillsTree/{skillsContainer.TreeName}/Groups/{selectedSkillsGroupName}", 
            selectedSkillsGroupName
        );

        _skillsTreeGroup.objectReferenceValue = selectedSkillsGroup;
        
        if (_skillsTreeGroup != null)
        {
            InspectorUtility.DrawDisabledField(() => _skillsTreeGroup.DrawPropertyField());
        }
        
        InspectorUtility.DrawSpace();
    }

    private void DrawSkillsArea(List<string> skillsTreeNames, string skillsTreeFolderPath) {
        InspectorUtility.DrawHeader("Skills");

        int oldSkillsIndex = _selectedSkillsTreeIndex.intValue;
        Skill oldSkillsTree = _skillsTree?.objectReferenceValue as Skill;
        bool isOldSkillsNull = oldSkillsTree == null;
        string oldSkillsName = isOldSkillsNull ? string.Empty : oldSkillsTree.SkillName;
        UpdatePropertyIndex(skillsTreeNames.ToArray(), _selectedSkillsTreeIndex, oldSkillsIndex, isOldSkillsNull, oldSkillsName);

        _selectedSkillsTreeIndex.intValue = InspectorUtility.DrawPopup("Skills", _selectedSkillsTreeIndex, skillsTreeNames.ToArray());

        string skillsTreeName = skillsTreeNames[_selectedSkillsTreeIndex.intValue];
        Skill skillsTree = AssetsUtility.LoadAsset<Skill>(skillsTreeFolderPath, skillsTreeName);
        
        if (_skillsTree != null)
        {
            _skillsTree.objectReferenceValue = skillsTree;
            InspectorUtility.DrawDisabledField(() => _skillsTree.DrawPropertyField());
        }
    }

    private void StopDrawing(string reason) {
        InspectorUtility.DrawHelpBox(reason);
        InspectorUtility.DrawSpace();
        InspectorUtility.DrawHelpBox("You need to select a Skill for this component to work properly.", MessageType.Warning);
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