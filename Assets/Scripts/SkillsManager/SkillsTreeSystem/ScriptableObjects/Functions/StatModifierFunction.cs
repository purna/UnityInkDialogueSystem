using UnityEngine;
using UnityEditor;
using Core.Game;

/// <summary>
/// Function that modifies a player stat
/// </summary>
/// Example stats:
/// - Health, MaxHealth, Damage, AttackSpeed
/// - MovementSpeed, JumpHeight, DashDistance
/// - Strength, Dexterity, Intelligence, Vitality, Luck
/// - GoldGain, ItemDropRate, SkillCooldownReduction
[CreateAssetMenu(fileName = "StatModifierFunction", menuName = "Skill Tree/Functions/Stat Modifier")]
public class StatModifierFunction : SkillFunction
{
    [SerializeField] private StatType _statType;
    [SerializeField] private ModifierType _modifierType;
    [SerializeField] private bool _useSkillValue = true;
    [SerializeField] private float _customValue;

    public StatType Stat => _statType;
    public ModifierType Modifier => _modifierType;
    public bool UseSkillValue => _useSkillValue;
    public float CustomValue => _customValue;
    
    public override void Execute(Skill skill)
    {
        float value = _useSkillValue ? skill.GetScaledValue() : _customValue;
        SkillTreeManager.Instance?.ModifyStat(_statType, _modifierType, value);
        Debug.Log($"[SkillFunction] Modified {_statType} by {value} ({_modifierType}) for skill '{skill.SkillName}'");
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(StatModifierFunction))]
    public class StatModifierFunctionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            StatModifierFunction func = (StatModifierFunction)target;

            // Draw header
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 14;
            titleStyle.normal.textColor = new Color(0.4f, 0.8f, 1f); // blue
            EditorGUILayout.LabelField("⚡ STAT MODIFIER FUNCTION", titleStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);

            // Instructions box
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("📋 INSTRUCTIONS", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "This SkillFunction modifies a player stat when its parent Skill is unlocked. " +
                "Choose a stat and a modifier type below, and decide whether to use the skill's scaled value or a custom value.",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("WHEN TO USE:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• Increase damage, health, or defense", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("• Modify movement or resource stats", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("• Apply flat, percentage, or multiplicative boosts", EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);

            // StatType dropdown
            func._statType = (StatType)EditorGUILayout.EnumPopup("Stat Type", func.Stat);
            func._modifierType = (ModifierType)EditorGUILayout.EnumPopup("Modifier Type", func.Modifier);
            func._useSkillValue = EditorGUILayout.Toggle("Use Skill Value", func.UseSkillValue);
            if (!func.UseSkillValue)
            {
                func._customValue = EditorGUILayout.FloatField("Custom Value", func.CustomValue);
            }

            EditorUtility.SetDirty(func);

            // Preview box
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🔎 PREVIEW", EditorStyles.boldLabel);
            string valueText = func.UseSkillValue ? "Skill's scaled value" : func.CustomValue.ToString();
            EditorGUILayout.LabelField($"• Stat: {func.Stat}");
            EditorGUILayout.LabelField($"• Modifier: {func.Modifier}");
            EditorGUILayout.LabelField($"• Value applied: {valueText}");
            EditorGUILayout.EndVertical();
        }
    }
#endif
}
