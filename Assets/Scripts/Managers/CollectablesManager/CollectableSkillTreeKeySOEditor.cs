using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CollectableSkillTreeKeySO))]
public class CollectableSkillTreeKeySOEditor : CollectableSOBaseEditor
{
    private SerializedProperty skillTreeGroupNameProp;
    private SerializedProperty targetGroupProp;

    private void OnEnable()
    {
        skillTreeGroupNameProp = serializedObject.FindProperty("skillTreeGroupName");
        targetGroupProp = serializedObject.FindProperty("targetGroup");
    }

    public override void OnInspectorGUI()
    {
        CollectableSkillTreeKeySO key = (CollectableSkillTreeKeySO)target;
        
        // Draw preview icon
        DrawPreviewIcon(key.ItemIcon, "🔑 Skill Tree Key Preview");
        
        // Draw info box
        DrawInfoBox(
            title: "SKILL TREE KEY COLLECTABLE",
            purpose: "Unlocks gated skill branches or specific skills. Acts as a gate for advanced abilities. Player must find the key before they can unlock certain skills. Stays in inventory permanently (not consumed).",
            whenToUse: new string[]
            {
                "Lock advanced/powerful skills behind exploration",
                "Gate end-game abilities",
                "Reward for completing dungeons/areas",
                "Quest rewards for major storylines",
                "Secret collectables in hidden locations"
            },
            examples: new string[]
            {
                "\"Combat Manual\" - Unlocks Combat skill branch",
                "\"Ancient Tome\" - Unlocks Magic skill branch",
                "\"Master's License\" - Required for tier 3 skills",
                "\"Detection Blueprint\" - Unlocks radar/detection skills"
            },
            whatHappens: "1. Adds key to inventory (permanent)\n2. Shows message about what was unlocked\n3. Plays visual/audio effect\n4. Destroys the pickup\n5. Key can now be checked by skills in skill tree"
        );
        
        // Draw setup options box
        DrawSetupOptionsBox();
        
        // Draw default inspector
        DrawDefaultInspector();
        
        // Show configuration preview
        DrawConfigurationPreview(key);
        
        // Show usage instructions
        DrawUsageInstructions(key);
    }
    
    private void DrawSetupOptionsBox()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("⚙️ SETUP OPTIONS", EditorStyles.boldLabel);
        EditorGUILayout.Space(3);
        
        EditorGUILayout.LabelField("Option A: Set Target Group", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField("  • Direct reference to SkillsTreeGroup", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("  • Most reliable and type-safe method", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space(3);
        
        EditorGUILayout.LabelField("Option B: Set Skill Tree Group Name", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField("  • String name matching skill group", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("  • Good for dynamic systems", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space(3);
        
        EditorGUILayout.LabelField("Option C: Leave Both Blank", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField("  • Skills check for ItemName directly", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("  • Flexible for custom implementations", EditorStyles.wordWrappedLabel);
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawConfigurationPreview(CollectableSkillTreeKeySO key)
    {
        serializedObject.Update();
        SkillsTreeGroup targetGroup = targetGroupProp.objectReferenceValue as SkillsTreeGroup;
        string groupName = skillTreeGroupNameProp.stringValue;
        serializedObject.ApplyModifiedProperties();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("📊 CURRENT CONFIGURATION", EditorStyles.boldLabel);
        EditorGUILayout.Space(3);
        
        // Key name
        EditorGUILayout.LabelField($"🔑 Key Name: {(string.IsNullOrEmpty(key.ItemName) ? "(Not Set)" : key.ItemName)}");
        
        // Configuration type
        string configType = GetConfigurationType(targetGroup, groupName);
        string configIcon = GetConfigurationIcon(targetGroup, groupName);
        EditorGUILayout.LabelField($"{configIcon} Configuration: {configType}");
        
        // What it unlocks
        string unlocksWhat = GetUnlocksDescription(targetGroup, groupName, key.ItemName);
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Unlocks:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(unlocksWhat, EditorStyles.wordWrappedLabel);
        
        // Validation
        EditorGUILayout.Space(3);
        ValidateConfiguration(targetGroup, groupName, key.ItemName);
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawUsageInstructions(CollectableSkillTreeKeySO key)
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("📖 HOW TO USE IN SKILLS", EditorStyles.boldLabel);
        EditorGUILayout.Space(3);
        
        EditorGUILayout.LabelField("1. Open your Skill ScriptableObject", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("2. Set \"Requires Special Key\" = true", EditorStyles.wordWrappedLabel);
        
        serializedObject.Update();
        SkillsTreeGroup targetGroup = targetGroupProp.objectReferenceValue as SkillsTreeGroup;
        string groupName = skillTreeGroupNameProp.stringValue;
        serializedObject.ApplyModifiedProperties();
        
        string keyNameToUse = GetKeyNameForSkills(targetGroup, groupName, key.ItemName);
        EditorGUILayout.LabelField($"3. Set \"Required Key Name\" = \"{keyNameToUse}\"", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space(3);
        
        EditorGUILayout.LabelField("The skill will check:", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField($"  InventoryManager.Instance.HasItem(\"{keyNameToUse}\")", EditorStyles.wordWrappedLabel);
        
        EditorGUILayout.EndVertical();
        
        // Important note
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox("🔒 KEY BEHAVIOR: Keys are NOT consumed when unlocking skills. They stay in inventory permanently and can unlock multiple skills if configured that way.", MessageType.Info);
    }
    
    private string GetConfigurationType(SkillsTreeGroup targetGroup, string groupName)
    {
        if (targetGroup != null)
            return "Option A: Direct Group Reference";
        if (!string.IsNullOrEmpty(groupName))
            return "Option B: Group Name String";
        return "Option C: Item Name Only";
    }
    
    private string GetConfigurationIcon(SkillsTreeGroup targetGroup, string groupName)
    {
        if (targetGroup != null)
            return "✅";
        if (!string.IsNullOrEmpty(groupName))
            return "✅";
        return "⚠️";
    }
    
    private string GetUnlocksDescription(SkillsTreeGroup targetGroup, string groupName, string itemName)
    {
        if (targetGroup != null)
            return $"  • Skill branch: {targetGroup.GroupName}\n  • All skills in this group can reference this key";
        if (!string.IsNullOrEmpty(groupName))
            return $"  • Skill branch: {groupName}\n  • Skills check for this group name";
        if (!string.IsNullOrEmpty(itemName))
            return $"  • Any skill checking for key: \"{itemName}\"";
        return "  • Nothing configured yet (set Target Group, Group Name, or Item Name)";
    }
    
    private string GetKeyNameForSkills(SkillsTreeGroup targetGroup, string groupName, string itemName)
    {
        if (targetGroup != null)
            return targetGroup.GroupName;
        if (!string.IsNullOrEmpty(groupName))
            return groupName;
        if (!string.IsNullOrEmpty(itemName))
            return itemName;
        return "(Not Set)";
    }
    
    private void ValidateConfiguration(SkillsTreeGroup targetGroup, string groupName, string itemName)
    {
        // Check if properly configured
        bool hasTargetGroup = targetGroup != null;
        bool hasGroupName = !string.IsNullOrEmpty(groupName);
        bool hasItemName = !string.IsNullOrEmpty(itemName);
        
        if (hasTargetGroup)
        {
            EditorGUILayout.HelpBox("✅ Valid: Using direct group reference (recommended)", MessageType.Info);
        }
        else if (hasGroupName)
        {
            EditorGUILayout.HelpBox("✅ Valid: Using group name string", MessageType.Info);
        }
        else if (hasItemName)
        {
            EditorGUILayout.HelpBox("⚠️ Valid but generic: Using ItemName only. Skills must check for this exact name.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox("❌ Configuration Incomplete: Set at least ItemName, or preferably Target Group or Group Name", MessageType.Error);
        }
        
        // Warn about redundant configuration
        if (hasTargetGroup && hasGroupName)
        {
            EditorGUILayout.HelpBox("ℹ️ Note: Both Target Group and Group Name are set. Target Group takes priority.", MessageType.None);
        }
    }
}