using UnityEngine;
using UnityEditor;
using Core.Game;

/// <summary>
/// Skill function that unlocks a specific ability
/// Example abilities:
/// - Dash
/// - DoubleJump
/// - Fireball
/// - IceShield
/// - Heal
/// </summary>
[CreateAssetMenu(fileName = "UnlockAbilityFunction", menuName = "Skill Tree/Functions/Unlock Ability")]
public class UnlockAbilityFunction : SkillFunction
{
    [SerializeField] private string _abilityID;

    // Public property for safe access
    public string AbilityID
    {
        get => _abilityID;
        set => _abilityID = value;
    }

    public override void Execute(Skill skill)
    {
        SkillTreeManager.Instance?.UnlockAbility(_abilityID);
        Debug.Log($"[SkillFunction] Unlocked ability: {_abilityID} from skill '{skill.SkillName}'");
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(UnlockAbilityFunction))]
    public class UnlockAbilityFunctionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            UnlockAbilityFunction func = (UnlockAbilityFunction)target;

            // Draw header
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 14;
            titleStyle.normal.textColor = new Color(0.7f, 0.4f, 1f);
            EditorGUILayout.LabelField("⚡ UNLOCK ABILITY FUNCTION", titleStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);

            // Instructions box
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("📋 INSTRUCTIONS", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "This SkillFunction unlocks a specific ability when its parent Skill is unlocked. " +
                "Select the ability from the dropdown list below. You can add more abilities by updating SkillTreeReferences.abilityOptions.",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("WHEN TO USE:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• Unlock active abilities like Dash, Double Jump, Fireball, Ice Shield, or Heal", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("• Attach this function to a Skill node in the skill tree", EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);

            // Dropdown to select ability
            int currentIndex = Mathf.Max(0, System.Array.IndexOf(SkillTreeReferences.abilityOptions, func.AbilityID));
            int newIndex = EditorGUILayout.Popup("Ability", currentIndex, SkillTreeReferences.abilityOptions);

            if (newIndex != currentIndex)
            {
                func.AbilityID = SkillTreeReferences.abilityOptions[newIndex];
                EditorUtility.SetDirty(func);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Selected Ability ID:", func.AbilityID);

            // Preview info
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🔎 PREVIEW", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"This SkillFunction will unlock the '{func.AbilityID}' ability when executed.");
            EditorGUILayout.EndVertical();
        }
    }
#endif
}
