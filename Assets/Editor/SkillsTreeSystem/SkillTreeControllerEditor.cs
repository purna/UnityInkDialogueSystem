using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SkillTreeController))]
public class SkillTreeControllerEditor : Editor
{
    private SerializedProperty _skillsProp;
    
    // Storage for play mode changes
    private static Dictionary<string, Vector2> _playModeSkillPositions = new Dictionary<string, Vector2>();
    private static bool _hasPlayModeChanges = false;
    private static bool _autoApplyChanges = true; // Default to auto-apply
    
    // Track previous values for change detection
    private Dictionary<string, Vector2> _previousSkillPositions = new Dictionary<string, Vector2>();

    private void OnEnable()
    {
        _skillsProp = serializedObject.FindProperty("_skills");
        
        // Subscribe to play mode state changes
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        
        // Initialize tracking
        CaptureCurrentPositions();
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }
    
    private void CheckForPositionChanges()
    {
        if (!Application.isPlaying)
            return;
            
        SkillTreeController controller = (SkillTreeController)target;
        if (controller == null)
            return;
        
        bool hasChanges = false;
        
        // Get the actual runtime values using reflection
        var skillsField = typeof(SkillTreeController).GetField("_skills", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Check skill position changes
        if (skillsField != null)
        {
            var skills = skillsField.GetValue(controller) as List<SkillTreeController.SkillNodeData>;
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    Vector2 currentPos = new Vector2(skill.X, skill.Y);
                    if (!_previousSkillPositions.ContainsKey(skill.Id) ||
                        _previousSkillPositions[skill.Id] != currentPos)
                    {
                        _previousSkillPositions[skill.Id] = currentPos;
                        hasChanges = true;
                    }
                }
            }
        }
        
        // Trigger re-render if positions changed
        if (hasChanges)
        {
            // Rebuild the entire tree to update node positions
            var buildTreeMethod = typeof(SkillTreeController).GetMethod("BuildTree", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            buildTreeMethod?.Invoke(controller, null);
            
            // Update UI styling
            var updateUIMethod = typeof(SkillTreeController).GetMethod("UpdateUI", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            updateUIMethod?.Invoke(controller, null);
        }
    }
    
    private void CaptureCurrentPositions()
    {
        if (!Application.isPlaying)
            return;
            
        SkillTreeController controller = (SkillTreeController)target;
        if (controller == null)
            return;
        
        _previousSkillPositions.Clear();
        
        var skillsField = typeof(SkillTreeController).GetField("_skills", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (skillsField != null)
        {
            var skills = skillsField.GetValue(controller) as List<SkillTreeController.SkillNodeData>;
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    _previousSkillPositions[skill.Id] = new Vector2(skill.X, skill.Y);
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Show checkbox for auto-apply (always visible)
        EditorGUILayout.BeginHorizontal();
        _autoApplyChanges = EditorGUILayout.ToggleLeft(
            "Auto-Apply Play Mode Position Changes", 
            _autoApplyChanges, 
            EditorStyles.boldLabel
        );
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);

        // Show warning if in play mode
        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "PLAY MODE EDITING ENABLED\n" +
                "Position changes will be " + (_autoApplyChanges ? "automatically applied" : "held for manual approval") + " when exiting Play Mode.",
                MessageType.Info
            );
        }

        // Show manual apply button if we have saved changes and auto-apply is off
        if (!Application.isPlaying && _hasPlayModeChanges && !_autoApplyChanges)
        {
            EditorGUILayout.HelpBox(
                "Play Mode changes detected! Click below to apply them.",
                MessageType.Warning
            );
            
            if (GUILayout.Button("Apply Play Mode Position Changes", GUILayout.Height(30)))
            {
                ApplyPlayModeChanges();
            }
            
            if (GUILayout.Button("Discard Play Mode Changes"))
            {
                DiscardPlayModeChanges();
            }
            
            EditorGUILayout.Space();
        }

        // Check for changes before drawing
        EditorGUI.BeginChangeCheck();
        
        // Draw default inspector
        DrawDefaultInspector();
        
        // If something changed in play mode, update the view
        if (EditorGUI.EndChangeCheck() && Application.isPlaying)
        {
            CheckForPositionChanges();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        SkillTreeController controller = (SkillTreeController)target;
        
        switch (state)
        {
            case PlayModeStateChange.ExitingPlayMode:
                // Capture all position changes before exiting play mode
                CapturePlayModePositions(controller);
                break;
                
            case PlayModeStateChange.EnteredEditMode:
                // Auto-apply if checkbox is enabled
                if (_hasPlayModeChanges && _autoApplyChanges)
                {
                    ApplyPlayModeChanges();
                }
                else if (_hasPlayModeChanges)
                {
                    // Just show the prompt
                    Repaint();
                }
                break;
        }
    }

    private void CapturePlayModePositions(SkillTreeController controller)
    {
        _playModeSkillPositions.Clear();
        _hasPlayModeChanges = false;

        // Get the actual runtime values using reflection
        var skillsField = typeof(SkillTreeController).GetField("_skills", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (skillsField != null)
        {
            var skills = skillsField.GetValue(controller) as List<SkillTreeController.SkillNodeData>;
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    _playModeSkillPositions[skill.Id] = new Vector2(skill.X, skill.Y);
                    _hasPlayModeChanges = true;
                }
            }
        }

        if (_hasPlayModeChanges)
        {
            Debug.Log($"Captured {_playModeSkillPositions.Count} skill positions from Play Mode");
        }
    }

    private void ApplyPlayModeChanges()
    {
        if (!_hasPlayModeChanges)
            return;

        Undo.RecordObject(target, "Apply Play Mode Position Changes");

        // Apply skill positions
        for (int i = 0; i < _skillsProp.arraySize; i++)
        {
            SerializedProperty skillProp = _skillsProp.GetArrayElementAtIndex(i);
            SerializedProperty idProp = skillProp.FindPropertyRelative("Id");
            string skillId = idProp.stringValue;

            if (_playModeSkillPositions.ContainsKey(skillId))
            {
                Vector2 pos = _playModeSkillPositions[skillId];
                skillProp.FindPropertyRelative("X").floatValue = pos.x;
                skillProp.FindPropertyRelative("Y").floatValue = pos.y;
            }
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
        
        Debug.Log("Applied Play Mode position changes to the scene");
        DiscardPlayModeChanges();
    }

    private void DiscardPlayModeChanges()
    {
        _playModeSkillPositions.Clear();
        _hasPlayModeChanges = false;
        Repaint();
    }
}