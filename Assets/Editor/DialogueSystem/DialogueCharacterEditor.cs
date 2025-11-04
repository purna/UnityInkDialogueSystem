#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(DialogueCharacter))]
public class DialogueCharacterEditor : Editor
{
    private SerializedProperty _defaultSprite;
    private SerializedProperty _defaultName;
    private SerializedProperty _emotionSprites;
    private SerializedProperty _portraitAnimatorController;
    private SerializedProperty _emotionAnimationMappings;
    private SerializedProperty _availableAnimationStates;
    private SerializedProperty _defaultLayout;
    private SerializedProperty _voiceInfo;

    private bool showEmotionSprites = true;
    private bool showAnimationMappings = true;
    private bool showAvailableStates = false;

    private void OnEnable()
    {
        _defaultSprite = serializedObject.FindProperty("_defaultSprite");
        _defaultName = serializedObject.FindProperty("_defaultName");
        _emotionSprites = serializedObject.FindProperty("_emotionSprites");
        _portraitAnimatorController = serializedObject.FindProperty("_portraitAnimatorController");
        _emotionAnimationMappings = serializedObject.FindProperty("_emotionAnimationMappings");
        _availableAnimationStates = serializedObject.FindProperty("_availableAnimationStates");
        _defaultLayout = serializedObject.FindProperty("_defaultLayout");
        _voiceInfo = serializedObject.FindProperty("_voiceInfo");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DialogueCharacter character = (DialogueCharacter)target;

        // Basic Info Section
        EditorGUILayout.LabelField("Basic Info", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_defaultSprite);
        EditorGUILayout.PropertyField(_defaultName);
        EditorGUILayout.Space();

        // Animator Controller Section
        EditorGUILayout.LabelField("Animator Controller (Ink System)", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_portraitAnimatorController);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            character.RefreshAnimationStates();
            serializedObject.Update();
        }

        // Refresh Button
        if (GUILayout.Button("Refresh Animation States"))
        {
            character.RefreshAnimationStates();
        }

        // Show available states count
        int stateCount = _availableAnimationStates.arraySize;
        EditorGUILayout.HelpBox($"Found {stateCount} animation states in controller", MessageType.Info);

        EditorGUILayout.Space();

        // Emotion-Animation Mappings Section
        showAnimationMappings = EditorGUILayout.Foldout(showAnimationMappings, $"Emotion to Animation Mappings ({_emotionAnimationMappings.arraySize})", true);
        
        if (showAnimationMappings)
        {
            EditorGUI.indentLevel++;

            // Auto-map button
            if (GUILayout.Button("Auto-Map Emotions to Animations"))
            {
                character.AutoMapEmotionsToAnimations();
                serializedObject.Update();
            }

            EditorGUILayout.Space(5);

            // Display each mapping
            for (int i = 0; i < _emotionAnimationMappings.arraySize; i++)
            {
                SerializedProperty mapping = _emotionAnimationMappings.GetArrayElementAtIndex(i);
                SerializedProperty emotion = mapping.FindPropertyRelative("emotion");
                SerializedProperty animationStateName = mapping.FindPropertyRelative("animationStateName");

                EditorGUILayout.BeginHorizontal();
                
                // Emotion dropdown
                EditorGUILayout.PropertyField(emotion, GUIContent.none, GUILayout.Width(150));

                // Animation state dropdown
                if (_availableAnimationStates.arraySize > 0)
                {
                    // Create dropdown list of available states
                    List<string> stateNames = new List<string>();
                    for (int j = 0; j < _availableAnimationStates.arraySize; j++)
                    {
                        stateNames.Add(_availableAnimationStates.GetArrayElementAtIndex(j).stringValue);
                    }

                    // Find current selection
                    int currentIndex = stateNames.IndexOf(animationStateName.stringValue);
                    if (currentIndex == -1) currentIndex = 0;

                    // Draw dropdown
                    int newIndex = EditorGUILayout.Popup(currentIndex, stateNames.ToArray());
                    if (newIndex >= 0 && newIndex < stateNames.Count)
                    {
                        animationStateName.stringValue = stateNames[newIndex];
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(animationStateName, GUIContent.none);
                }

                // Delete button
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    _emotionAnimationMappings.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            // Add new mapping button
            EditorGUILayout.Space(5);
            if (GUILayout.Button("+ Add Emotion Mapping"))
            {
                _emotionAnimationMappings.arraySize++;
                SerializedProperty newMapping = _emotionAnimationMappings.GetArrayElementAtIndex(_emotionAnimationMappings.arraySize - 1);
                newMapping.FindPropertyRelative("emotion").enumValueIndex = 0;
                newMapping.FindPropertyRelative("animationStateName").stringValue = "";
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Emotion Sprites Section (Graph-Based)
        showEmotionSprites = EditorGUILayout.Foldout(showEmotionSprites, $"Emotion Sprites - Graph-Based ({_emotionSprites.arraySize})", true);
        
        if (showEmotionSprites)
        {
            EditorGUI.indentLevel++;

            // Sync button
            if (_emotionAnimationMappings.arraySize > 0)
            {
                if (GUILayout.Button("Sync with Animation Mappings"))
                {
                    character.SyncEmotionSpritesWithMappings();
                    serializedObject.Update();
                }
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.PropertyField(_emotionSprites, true);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Layout Section
        EditorGUILayout.LabelField("Layout", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_defaultLayout);
        EditorGUILayout.Space();

        // Voice Section
        EditorGUILayout.LabelField("Voice", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_voiceInfo);
        EditorGUILayout.Space();

        // Available States (Debug/Reference)
        showAvailableStates = EditorGUILayout.Foldout(showAvailableStates, "Available Animation States (Reference)", true);
        if (showAvailableStates)
        {
            EditorGUI.indentLevel++;
            GUI.enabled = false;
            EditorGUILayout.PropertyField(_availableAnimationStates, true);
            GUI.enabled = true;
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif