using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(LevelMapController))]
public class LevelMapControllerEditor : Editor
{
    private SerializedProperty _worldsProp;
    private SerializedProperty _levelsProp;
    
    // Storage for play mode changes
    private static Dictionary<string, Vector2> _playModeLevelPositions = new Dictionary<string, Vector2>();
    private static Dictionary<int, Vector2> _playModeWorldPositions = new Dictionary<int, Vector2>();
    private static bool _hasPlayModeChanges = false;
    private static bool _autoApplyChanges = true; // Default to auto-apply
    
    // Track previous values for change detection
    private Dictionary<string, Vector2> _previousLevelPositions = new Dictionary<string, Vector2>();
    private Dictionary<int, Vector2> _previousWorldPositions = new Dictionary<int, Vector2>();

    private void OnEnable()
    {
        _worldsProp = serializedObject.FindProperty("_worlds");
        _levelsProp = serializedObject.FindProperty("_levels");
        
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
            
        LevelMapController controller = (LevelMapController)target;
        if (controller == null)
            return;
        
        bool hasChanges = false;
        
        // Get the actual runtime values using reflection
        var worldsField = typeof(LevelMapController).GetField("_worlds", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var levelsField = typeof(LevelMapController).GetField("_levels", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Check world position changes
        if (worldsField != null)
        {
            var worlds = worldsField.GetValue(controller) as List<LevelMapController.WorldData>;
            if (worlds != null)
            {
                foreach (var world in worlds)
                {
                    Vector2 currentPos = new Vector2(world.X, world.Y);
                    if (!_previousWorldPositions.ContainsKey(world.WorldIndex) ||
                        _previousWorldPositions[world.WorldIndex] != currentPos)
                    {
                        _previousWorldPositions[world.WorldIndex] = currentPos;
                        hasChanges = true;
                    }
                }
            }
        }

        // Check level position changes
        if (levelsField != null)
        {
            var levels = levelsField.GetValue(controller) as List<LevelMapController.LevelNode>;
            if (levels != null)
            {
                foreach (var level in levels)
                {
                    Vector2 currentPos = new Vector2(level.X, level.Y);
                    if (!_previousLevelPositions.ContainsKey(level.Id) ||
                        _previousLevelPositions[level.Id] != currentPos)
                    {
                        _previousLevelPositions[level.Id] = currentPos;
                        hasChanges = true;
                    }
                }
            }
        }
        
        // Trigger re-render if positions changed
        if (hasChanges)
        {
            var renderMethod = typeof(LevelMapController).GetMethod("RenderView", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            renderMethod?.Invoke(controller, null);
        }
    }
    
    private void CaptureCurrentPositions()
    {
        if (!Application.isPlaying)
            return;
            
        LevelMapController controller = (LevelMapController)target;
        if (controller == null)
            return;
        
        _previousLevelPositions.Clear();
        _previousWorldPositions.Clear();
        
        var worldsField = typeof(LevelMapController).GetField("_worlds", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var levelsField = typeof(LevelMapController).GetField("_levels", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (worldsField != null)
        {
            var worlds = worldsField.GetValue(controller) as List<LevelMapController.WorldData>;
            if (worlds != null)
            {
                foreach (var world in worlds)
                {
                    _previousWorldPositions[world.WorldIndex] = new Vector2(world.X, world.Y);
                }
            }
        }

        if (levelsField != null)
        {
            var levels = levelsField.GetValue(controller) as List<LevelMapController.LevelNode>;
            if (levels != null)
            {
                foreach (var level in levels)
                {
                    _previousLevelPositions[level.Id] = new Vector2(level.X, level.Y);
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
        LevelMapController controller = (LevelMapController)target;
        
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

    private void CapturePlayModePositions(LevelMapController controller)
    {
        _playModeLevelPositions.Clear();
        _playModeWorldPositions.Clear();
        _hasPlayModeChanges = false;

        // Get the actual runtime values using reflection
        var worldsField = typeof(LevelMapController).GetField("_worlds", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var levelsField = typeof(LevelMapController).GetField("_levels", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (worldsField != null)
        {
            var worlds = worldsField.GetValue(controller) as List<LevelMapController.WorldData>;
            if (worlds != null)
            {
                foreach (var world in worlds)
                {
                    _playModeWorldPositions[world.WorldIndex] = new Vector2(world.X, world.Y);
                    _hasPlayModeChanges = true;
                }
            }
        }

        if (levelsField != null)
        {
            var levels = levelsField.GetValue(controller) as List<LevelMapController.LevelNode>;
            if (levels != null)
            {
                foreach (var level in levels)
                {
                    _playModeLevelPositions[level.Id] = new Vector2(level.X, level.Y);
                    _hasPlayModeChanges = true;
                }
            }
        }

        if (_hasPlayModeChanges)
        {
            Debug.Log($"Captured {_playModeWorldPositions.Count} world positions and {_playModeLevelPositions.Count} level positions from Play Mode");
        }
    }

    private void ApplyPlayModeChanges()
    {
        if (!_hasPlayModeChanges)
            return;

        Undo.RecordObject(target, "Apply Play Mode Position Changes");

        // Apply world positions
        for (int i = 0; i < _worldsProp.arraySize; i++)
        {
            SerializedProperty worldProp = _worldsProp.GetArrayElementAtIndex(i);
            SerializedProperty indexProp = worldProp.FindPropertyRelative("WorldIndex");
            int worldIndex = indexProp.intValue;

            if (_playModeWorldPositions.ContainsKey(worldIndex))
            {
                Vector2 pos = _playModeWorldPositions[worldIndex];
                worldProp.FindPropertyRelative("X").floatValue = pos.x;
                worldProp.FindPropertyRelative("Y").floatValue = pos.y;
            }
        }

        // Apply level positions
        for (int i = 0; i < _levelsProp.arraySize; i++)
        {
            SerializedProperty levelProp = _levelsProp.GetArrayElementAtIndex(i);
            SerializedProperty idProp = levelProp.FindPropertyRelative("Id");
            string levelId = idProp.stringValue;

            if (_playModeLevelPositions.ContainsKey(levelId))
            {
                Vector2 pos = _playModeLevelPositions[levelId];
                levelProp.FindPropertyRelative("X").floatValue = pos.x;
                levelProp.FindPropertyRelative("Y").floatValue = pos.y;
            }
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
        
        Debug.Log("Applied Play Mode position changes to the scene");
        DiscardPlayModeChanges();
    }

    private void DiscardPlayModeChanges()
    {
        _playModeLevelPositions.Clear();
        _playModeWorldPositions.Clear();
        _hasPlayModeChanges = false;
        Repaint();
    }
}