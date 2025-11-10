using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Custom editor to show a dropdown for skill selection
/// </summary>
[CustomEditor(typeof(SkillNode))]
public class SkillNoderEditor : Editor
{
    private SerializedProperty _controllerProp;
    private SerializedProperty _skillIndexProp;
    private SerializedProperty _skillProp;
    
    private void OnEnable()
    {
        _controllerProp = serializedObject.FindProperty("_controller");
        _skillIndexProp = serializedObject.FindProperty("_skillIndex");
        _skillProp = serializedObject.FindProperty("_skill");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // Draw default inspector
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Manual Skill Selection", EditorStyles.boldLabel);
        
        SkillNode choicer = (SkillNode)target;
        SkillsTreeController controller = _controllerProp.objectReferenceValue as SkillsTreeController;
        
        if (controller == null)
        {
            EditorGUILayout.HelpBox("Assign a SkillsTreeController to select skills", MessageType.Info);
        }
        else
        {
            List<Skill> availableSkills = controller.GetAvailableSkills();
            
            if (availableSkills == null || availableSkills.Count == 0)
            {
                EditorGUILayout.HelpBox("No skills available in the controller's SkillsTreeContainer/Group", MessageType.Warning);
            }
            else
            {
                // Create dropdown options
                string[] skillNames = new string[availableSkills.Count + 1];
                skillNames[0] = "-- Select Skill --";
                for (int i = 0; i < availableSkills.Count; i++)
                {
                    skillNames[i + 1] = availableSkills[i] != null ? availableSkills[i].SkillName : $"Skill {i}";
                }
                
                // Show dropdown
                int currentIndex = _skillIndexProp.intValue + 1; // +1 because of "Select Skill" option
                int newIndex = EditorGUILayout.Popup("Select Skill", currentIndex, skillNames);
                
                if (newIndex != currentIndex)
                {
                    _skillIndexProp.intValue = newIndex - 1; // -1 to convert back to actual index
                    
                    // Auto-assign the skill
                    if (newIndex > 0 && newIndex <= availableSkills.Count)
                    {
                        _skillProp.objectReferenceValue = availableSkills[newIndex - 1];
                    }
                    else
                    {
                        _skillProp.objectReferenceValue = null;
                    }
                }
                
                // Show currently selected skill info
                if (_skillIndexProp.intValue >= 0 && _skillIndexProp.intValue < availableSkills.Count)
                {
                    Skill selectedSkill = availableSkills[_skillIndexProp.intValue];
                    if (selectedSkill != null)
                    {
                        EditorGUILayout.Space(5);
                        EditorGUILayout.LabelField("Selected Skill Info:", EditorStyles.miniBoldLabel);
                        EditorGUILayout.LabelField($"Name: {selectedSkill.SkillName}");
                        EditorGUILayout.LabelField($"Description: {selectedSkill.Description}");
                        EditorGUILayout.LabelField($"Cost: {selectedSkill.UnlockCost} SP");
                    }
                }
                
                EditorGUILayout.Space(5);
                if (GUILayout.Button("Refresh Skill Assignment"))
                {
                    if (Application.isPlaying)
                    {
                        choicer.SetSkill(availableSkills[_skillIndexProp.intValue]);
                    }
                    else
                    {
                        EditorUtility.SetDirty(target);
                    }
                }
            }
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}