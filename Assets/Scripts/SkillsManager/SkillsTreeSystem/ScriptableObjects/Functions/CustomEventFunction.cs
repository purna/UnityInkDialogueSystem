using UnityEngine;
using UnityEditor;
using Core.Game;

/// <summary>
/// Skill function that triggers a custom event
/// </summary>
/// Example events:
/// - OnEnemyKilled
/// - OnItemCollected
/// - OnLevelUp
/// - OnBossDefeated
[CreateAssetMenu(fileName = "CustomEventFunction", menuName = "Skill Tree/Functions/Custom Event")]
public class CustomEventFunction : SkillFunction
{
    [SerializeField] private string _eventName;
    [SerializeField] private string _eventParameter;
    [SerializeField] private string _eventParameter2;

    public string EventName
    {
        get => _eventName;
        set => _eventName = value;
    }

    public string EventParameter
    {
        get => _eventParameter;
        set => _eventParameter = value;
    }

    public string EventParameter2
    {
        get => _eventParameter2;
        set => _eventParameter2 = value;
    }


    public override void Execute(Skill skill)
    {
        SkillTreeManager.Instance?.TriggerCustomEvent(_eventName, _eventParameter, skill);
        Debug.Log($"[SkillFunction] Triggered event: {_eventName} from skill '{skill.SkillName}'");
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CustomEventFunction))]
    public class CustomEventFunctionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            CustomEventFunction func = (CustomEventFunction)target;

            // Draw header
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 14;
            titleStyle.normal.textColor = new Color(0.9f, 0.5f, 0.1f); // orange
            EditorGUILayout.LabelField("⚡ CUSTOM EVENT FUNCTION", titleStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);

            // Instructions box
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("📋 INSTRUCTIONS", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "This SkillFunction triggers a predefined event when its parent Skill is unlocked. " +
                "Select an event from the dropdown and provide an optional parameter if required.",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("WHEN TO USE:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• Trigger gameplay systems on skill unlock", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("• Progress quests or objectives", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("• Activate visual/audio effects via events", EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);

            // Event dropdown
            int currentIndex = Mathf.Max(0, System.Array.IndexOf(SkillTreeReferences.eventOptions, func._eventName));
            int newIndex = EditorGUILayout.Popup("Event Name", currentIndex, SkillTreeReferences.eventOptions);

            if (newIndex != currentIndex)
            {
                func._eventName = SkillTreeReferences.eventOptions[newIndex];
                EditorUtility.SetDirty(func);
            }

            // Event parameter
            func._eventParameter = EditorGUILayout.TextField("Event Parameter", func._eventParameter);

            // Preview box
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🔎 PREVIEW", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"• Event: {func._eventName}");
            EditorGUILayout.LabelField($"• Parameter: {func._eventParameter}");
            EditorGUILayout.EndVertical();
        }
    }
#endif
}
