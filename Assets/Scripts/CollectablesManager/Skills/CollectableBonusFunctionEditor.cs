using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CollectableBonusFunction))]
public class CollectableBonusFunctionEditor : Editor
{
    private SerializedProperty bonusTypeProp;
    private SerializedProperty bonusAmountProp;

    private void OnEnable()
    {
        bonusTypeProp = serializedObject.FindProperty("bonusType");
        bonusAmountProp = serializedObject.FindProperty("bonusAmount");
    }

    public override void OnInspectorGUI()
    {
        CollectableBonusFunction bonusFunction = (CollectableBonusFunction)target;
        
   
        // Draw header
        DrawTitleHeader();
        
        // Draw info box
        DrawInfoBox();
        
        // Draw bonus type details
        DrawBonusTypeDetails(bonusFunction);
        
        // Draw default inspector
        DrawDefaultInspector();
        
        // Show configuration preview
        DrawConfigurationPreview(bonusFunction);
        
        // Show integration notes
        DrawIntegrationNotes(bonusFunction);
    }
    
    private void DrawTitleHeader()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 14;
        titleStyle.normal.textColor = new Color(1f, 0.8f, 0.2f); // Gold color
        EditorGUILayout.LabelField("💎 COLLECTABLE BONUS FUNCTION", titleStyle);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
    
    private void DrawInfoBox()
    {
        // PURPOSE section
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("📋 PURPOSE", new Color(0.4f, 0.8f, 0.4f));
        
        EditorGUILayout.LabelField("Enhances player's ability to gain and find collectables. Provides different bonus types for diverse build strategies and scales with skill values for progression.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space(5);
        
        // WHEN TO USE section
        DrawSectionHeader("✅ WHEN TO USE", new Color(0.8f, 0.6f, 0.2f));
        string[] whenToUse = new string[]
        {
            "• Passive bonuses in skill trees",
            "• Temporary power-ups or buffs",
            "• Character trait/perk systems",
            "• Equipment bonuses (treasure hunter gear)",
            "• Level progression rewards"
        };
        foreach (string use in whenToUse)
        {
            EditorGUILayout.LabelField(use, EditorStyles.wordWrappedLabel);
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);
    }

        private void DrawSectionHeader(string text, Color color)
    {
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.normal.textColor = color;
        EditorGUILayout.LabelField(text, headerStyle);
        EditorGUILayout.Space(2);
    }
    
    private void DrawBonusTypeDetails(CollectableBonusFunction bonusFunction)
    {
        serializedObject.Update();
        CollectableBonusFunction.CollectableBonusType currentType = 
            (CollectableBonusFunction.CollectableBonusType)bonusTypeProp.enumValueIndex;
        serializedObject.ApplyModifiedProperties();
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSectionHeader("📚 BONUS TYPE REFERENCE", new Color(0.4f, 0.8f, 0.4f));

        EditorGUILayout.Space(3);
        
        // Currency Multiplier
        DrawBonusTypeInfo(
            "💰 CURRENCY MULTIPLIER",
            "Multiplies currency gained from pickups",
            "1.5x = 50% more coins, 2.0x = double coins",
            "Economy builds, farming strategies",
            currentType == CollectableBonusFunction.CollectableBonusType.CurrencyMultiplier
        );
        
        // Drop Rate Increase
        DrawBonusTypeInfo(
            "📦 DROP RATE INCREASE",
            "Increases chance of enemies/objects dropping collectables",
            "1.25 = 25% increase, 1.5 = 50% increase",
            "Loot-focused builds, completionists",
            currentType == CollectableBonusFunction.CollectableBonusType.DropRateIncrease
        );
        
        // Detection Radius
        DrawBonusTypeInfo(
            "🔍 DETECTION RADIUS",
            "Expands range for detecting nearby collectables",
            "15 = 15 unit radius, 30 = 30 unit radius",
            "Explorer builds, treasure hunters",
            currentType == CollectableBonusFunction.CollectableBonusType.DetectionRadius
        );
        
        // Collection Speed
        DrawBonusTypeInfo(
            "⚡ COLLECTION SPEED",
            "Increases how quickly collectables are picked up",
            "1.5x = 50% faster, 2.0x = double speed",
            "Fast-paced gameplay, speedrunners",
            currentType == CollectableBonusFunction.CollectableBonusType.CollectionSpeed
        );
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);
    }
    
    private void DrawBonusTypeInfo(string title, string description, string example, string bestFor, bool isSelected)
    {
        if (isSelected)
        {
            GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.LabelField("✓ " + title, EditorStyles.boldLabel);
        }
        else
        {
            EditorGUILayout.LabelField(title, EditorStyles.miniBoldLabel);
        }
        
        EditorGUILayout.LabelField($"  • {description}", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField($"  • Example: {example}", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField($"  • Best for: {bestFor}", EditorStyles.wordWrappedLabel);
        
        if (isSelected)
        {
            EditorGUILayout.EndVertical();
        }
        
        EditorGUILayout.Space(3);
    }
    
    private void DrawConfigurationPreview(CollectableBonusFunction bonusFunction)
    {
        serializedObject.Update();
        CollectableBonusFunction.CollectableBonusType type = 
            (CollectableBonusFunction.CollectableBonusType)bonusTypeProp.enumValueIndex;
        float amount = bonusAmountProp.floatValue;
        serializedObject.ApplyModifiedProperties();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("📊 CURRENT CONFIGURATION", EditorStyles.boldLabel);
        EditorGUILayout.Space(3);
        
        string icon = GetBonusIcon(type);
        string typeName = GetBonusTypeName(type);
        string valueDescription = GetValueDescription(type, amount);
        string effectiveness = GetEffectivenessRating(type, amount);
        
        EditorGUILayout.LabelField($"{icon} Type: {typeName}");
        EditorGUILayout.LabelField($"📈 Value: {amount} ({valueDescription})");
        EditorGUILayout.Space(3);
        
        MessageType messageType = GetMessageType(type, amount);
        EditorGUILayout.HelpBox($"Effectiveness: {effectiveness}", messageType);
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawIntegrationNotes(CollectableBonusFunction bonusFunction)
    {
        serializedObject.Update();
        CollectableBonusFunction.CollectableBonusType type = 
            (CollectableBonusFunction.CollectableBonusType)bonusTypeProp.enumValueIndex;
        serializedObject.ApplyModifiedProperties();
        
        EditorGUILayout.Space(5);
        
        string requirements = GetRequirementsForType(type);
        string status = GetImplementationStatus(type);
        
        EditorGUILayout.HelpBox($"⚙️ INTEGRATION:\n" +
                                $"{requirements}\n\n" +
                                $"Status: {status}", 
                                MessageType.None);
        
        // Scaling info
        EditorGUILayout.Space(3);
        EditorGUILayout.HelpBox("📊 SCALING: Uses skill.GetScaledValue() for progression. " +
                                "Value scales with skill level for smooth progression curves.", 
                                MessageType.Info);
    }
    
    private string GetBonusIcon(CollectableBonusFunction.CollectableBonusType type)
    {
        switch (type)
        {
            case CollectableBonusFunction.CollectableBonusType.CurrencyMultiplier: return "💰";
            case CollectableBonusFunction.CollectableBonusType.DropRateIncrease: return "📦";
            case CollectableBonusFunction.CollectableBonusType.DetectionRadius: return "🔍";
            case CollectableBonusFunction.CollectableBonusType.CollectionSpeed: return "⚡";
            default: return "💎";
        }
    }
    
    private string GetBonusTypeName(CollectableBonusFunction.CollectableBonusType type)
    {
        switch (type)
        {
            case CollectableBonusFunction.CollectableBonusType.CurrencyMultiplier: return "Currency Multiplier";
            case CollectableBonusFunction.CollectableBonusType.DropRateIncrease: return "Drop Rate Increase";
            case CollectableBonusFunction.CollectableBonusType.DetectionRadius: return "Detection Radius";
            case CollectableBonusFunction.CollectableBonusType.CollectionSpeed: return "Collection Speed";
            default: return "Unknown";
        }
    }
    
    private string GetValueDescription(CollectableBonusFunction.CollectableBonusType type, float amount)
    {
        switch (type)
        {
            case CollectableBonusFunction.CollectableBonusType.CurrencyMultiplier:
                return amount >= 2f ? "Double or more" : $"{(amount - 1f) * 100f:F0}% bonus";
            case CollectableBonusFunction.CollectableBonusType.DropRateIncrease:
                return $"{(amount - 1f) * 100f:F0}% increase";
            case CollectableBonusFunction.CollectableBonusType.DetectionRadius:
                return amount >= 30f ? "Very long range" : amount >= 20f ? "Long range" : amount >= 10f ? "Medium range" : "Short range";
            case CollectableBonusFunction.CollectableBonusType.CollectionSpeed:
                return amount >= 2f ? "Double speed or more" : $"{(amount - 1f) * 100f:F0}% faster";
            default:
                return "N/A";
        }
    }
    
    private string GetEffectivenessRating(CollectableBonusFunction.CollectableBonusType type, float amount)
    {
        switch (type)
        {
            case CollectableBonusFunction.CollectableBonusType.CurrencyMultiplier:
                if (amount >= 2.5f) return "⭐⭐⭐ Excellent (+150% or more)";
                if (amount >= 2f) return "⭐⭐⭐ Excellent (Double coins)";
                if (amount >= 1.5f) return "⭐⭐ Good (+50% bonus)";
                return "⭐ Basic (Small bonus)";
                
            case CollectableBonusFunction.CollectableBonusType.DropRateIncrease:
                if (amount >= 2f) return "⭐⭐⭐ Excellent (Double drop rate)";
                if (amount >= 1.5f) return "⭐⭐ Good (+50% drops)";
                if (amount >= 1.25f) return "⭐⭐ Good (+25% drops)";
                return "⭐ Basic (Minor increase)";
                
            case CollectableBonusFunction.CollectableBonusType.DetectionRadius:
                if (amount >= 30f) return "⭐⭐⭐ Excellent (Very wide range)";
                if (amount >= 20f) return "⭐⭐ Good (Long range)";
                if (amount >= 10f) return "⭐⭐ Good (Medium range)";
                return "⭐ Basic (Short range)";
                
            case CollectableBonusFunction.CollectableBonusType.CollectionSpeed:
                if (amount >= 3f) return "⭐⭐⭐ Excellent (Triple speed)";
                if (amount >= 2f) return "⭐⭐⭐ Excellent (Double speed)";
                if (amount >= 1.5f) return "⭐⭐ Good (+50% faster)";
                return "⭐ Basic (Slight boost)";
                
            default:
                return "Unknown";
        }
    }
    
    private MessageType GetMessageType(CollectableBonusFunction.CollectableBonusType type, float amount)
    {
        // Return Info for good values, Warning for weak values
        switch (type)
        {
            case CollectableBonusFunction.CollectableBonusType.CurrencyMultiplier:
            case CollectableBonusFunction.CollectableBonusType.DropRateIncrease:
            case CollectableBonusFunction.CollectableBonusType.CollectionSpeed:
                return amount >= 1.5f ? MessageType.Info : MessageType.Warning;
            case CollectableBonusFunction.CollectableBonusType.DetectionRadius:
                return amount >= 15f ? MessageType.Info : MessageType.Warning;
            default:
                return MessageType.None;
        }
    }
    
    private string GetRequirementsForType(CollectableBonusFunction.CollectableBonusType type)
    {
        switch (type)
        {
            case CollectableBonusFunction.CollectableBonusType.CurrencyMultiplier:
                return "• Requires: CurrencyManager.ApplyCurrencyMultiplier()\n• Attach to Skill object as SkillFunction";
            case CollectableBonusFunction.CollectableBonusType.DropRateIncrease:
                return "• Requires: DropManager.ApplyDropRateMultiplier()\n• Attach to Skill object as SkillFunction";
            case CollectableBonusFunction.CollectableBonusType.DetectionRadius:
                return "• Requires: CollectableDetectionSystem singleton in scene\n• Fully implemented and ready to use";
            case CollectableBonusFunction.CollectableBonusType.CollectionSpeed:
                return "• Requires: CollectionManager.ApplySpeedMultiplier()\n• Attach to Skill object as SkillFunction";
            default:
                return "• Unknown requirements";
        }
    }
    
    private string GetImplementationStatus(CollectableBonusFunction.CollectableBonusType type)
    {
        switch (type)
        {
            case CollectableBonusFunction.CollectableBonusType.DetectionRadius:
                return "✅ Fully Implemented";
            case CollectableBonusFunction.CollectableBonusType.CurrencyMultiplier:
            case CollectableBonusFunction.CollectableBonusType.DropRateIncrease:
            case CollectableBonusFunction.CollectableBonusType.CollectionSpeed:
                return "⚠️ Needs Implementation (TODO in code)";
            default:
                return "❓ Unknown";
        }
    }

       
}