using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GenericCollectableTrigger))]
public class GenericCollectableTriggerEditor : Editor
{
    private SerializedProperty collectableTypeProp;
    private SerializedProperty checkIfAlreadyUnlockedProp;
    private SerializedProperty playerUpgradesProp;
    private SerializedProperty oneTimeUseProp;
    private SerializedProperty destroyOnTriggerProp;
    private SerializedProperty visualCueProp;
    private SerializedProperty animatorProp;
    private SerializedProperty animationTriggerNameProp;
    private SerializedProperty collectSoundProp;
    private SerializedProperty audioSourceProp;
    private SerializedProperty showDebugLogsProp;

    private void OnEnable()
    {
        collectableTypeProp = serializedObject.FindProperty("collectableType");
        checkIfAlreadyUnlockedProp = serializedObject.FindProperty("checkIfAlreadyUnlocked");
        playerUpgradesProp = serializedObject.FindProperty("playerUpgrades");
        oneTimeUseProp = serializedObject.FindProperty("oneTimeUse");
        destroyOnTriggerProp = serializedObject.FindProperty("destroyOnTrigger");
        visualCueProp = serializedObject.FindProperty("visualCue");
        animatorProp = serializedObject.FindProperty("animator");
        animationTriggerNameProp = serializedObject.FindProperty("animationTriggerName");
        collectSoundProp = serializedObject.FindProperty("collectSound");
        audioSourceProp = serializedObject.FindProperty("audioSource");
        showDebugLogsProp = serializedObject.FindProperty("showDebugLogs");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        GenericCollectableTrigger trigger = (GenericCollectableTrigger)target;

        // Info Box
        DrawInfoBox();
        EditorGUILayout.Space(5);

        // Collectable Settings
        DrawCollectableSettings();
        EditorGUILayout.Space();

        // References
        DrawReferences();
        EditorGUILayout.Space();

        // Trigger Behavior
        DrawTriggerBehavior();
        EditorGUILayout.Space();

        // Visual Feedback
        DrawVisualFeedback();
        EditorGUILayout.Space();

        // Audio
        DrawAudio();
        EditorGUILayout.Space();

        // Debug
        DrawDebugSettings();
        EditorGUILayout.Space();

        // Utility Buttons
        DrawUtilityButtons();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawInfoBox()
    {
        CollectableType type = (CollectableType)collectableTypeProp.enumValueIndex;
        bool oneTimeUse = oneTimeUseProp.boolValue;
        
        string message = $"✓ {type} Collectable Trigger";
        if (oneTimeUse)
            message += " (One-time use)";
        else
            message += " (Repeatable)";
        
        EditorGUILayout.HelpBox(message, MessageType.Info);
    }

    private void DrawCollectableSettings()
    {
        EditorGUILayout.LabelField("Collectable Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.PropertyField(collectableTypeProp, new GUIContent(
            "Collectable Type",
            "Type of upgrade/collectable this trigger will unlock"));
        
        EditorGUILayout.PropertyField(checkIfAlreadyUnlockedProp, new GUIContent(
            "Check If Already Unlocked",
            "Skip triggering if the player already has this upgrade"));
        
        EditorGUILayout.EndVertical();
        
        // Show description for selected type
        CollectableType type = (CollectableType)collectableTypeProp.enumValueIndex;
        string description = GetCollectableDescription(type);
        if (!string.IsNullOrEmpty(description))
        {
            EditorGUILayout.Space(3);
            EditorGUILayout.HelpBox(description, MessageType.None);
        }
    }

    private void DrawReferences()
    {
        EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(playerUpgradesProp, new GUIContent(
            "Player Upgrades",
            "Will auto-find if not assigned"));
        
        if (playerUpgradesProp.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox(
                "PlayerUpgrades not assigned. Will attempt to auto-find at runtime.",
                MessageType.Warning);
        }
    }

    private void DrawTriggerBehavior()
    {
        EditorGUILayout.LabelField("Trigger Behavior", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.PropertyField(oneTimeUseProp, new GUIContent(
            "One Time Use",
            "If TRUE, trigger only works once. If FALSE, can trigger multiple times."));
        
        EditorGUILayout.PropertyField(destroyOnTriggerProp, new GUIContent(
            "Destroy On Trigger",
            "Destroy this GameObject after triggering"));
        
        EditorGUILayout.EndVertical();
        
        if (!oneTimeUseProp.boolValue)
        {
            EditorGUILayout.HelpBox(
                "⚠️ Repeatable Mode: Player can collect this upgrade multiple times.",
                MessageType.Warning);
        }
    }

    private void DrawVisualFeedback()
    {
        EditorGUILayout.LabelField("Visual Feedback", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.PropertyField(visualCueProp, new GUIContent(
            "Visual Cue",
            "GameObject to show/hide as visual indicator"));
        
        EditorGUILayout.PropertyField(animatorProp, new GUIContent(
            "Animator",
            "Animator to trigger collection animation"));
        
        if (animatorProp.objectReferenceValue != null)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(animationTriggerNameProp, new GUIContent(
                "Animation Trigger",
                "Name of the animation trigger parameter"));
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndVertical();
    }

    private void DrawAudio()
    {
        EditorGUILayout.LabelField("Audio", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.PropertyField(collectSoundProp, new GUIContent(
            "Collect Sound",
            "Audio clip to play when collected"));
        
        EditorGUILayout.PropertyField(audioSourceProp, new GUIContent(
            "Audio Source",
            "Will auto-add if not assigned"));
        
        EditorGUILayout.EndVertical();
    }

    private void DrawDebugSettings()
    {
        EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(showDebugLogsProp, new GUIContent(
            "Show Debug Logs",
            "Print logs to console for debugging"));
    }

    private void DrawUtilityButtons()
    {
        EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);
        
        GenericCollectableTrigger trigger = (GenericCollectableTrigger)target;
        
        if (Application.isPlaying)
        {
            // Runtime Controls
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Manual Trigger", GUILayout.Height(30)))
            {
                trigger.ManualTrigger();
            }
            
            if (GUILayout.Button("Reset Trigger", GUILayout.Height(30)))
            {
                trigger.ResetTrigger();
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        else
        {
            // Editor Setup
            EditorGUILayout.HelpBox(
                "✓ Trigger is ready! Make sure to:\n" +
                "• Add a 2D Collider with 'Is Trigger' enabled\n" +
                "• Tag your player GameObject as 'Player'",
                MessageType.Info);
            
            // Check for collider
            Collider2D col = trigger.GetComponent<Collider2D>();
            if (col == null)
            {
                EditorGUILayout.HelpBox(
                    "⚠️ Missing Collider2D component! This trigger won't detect the player.",
                    MessageType.Warning);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Box Collider 2D", GUILayout.Height(25)))
                {
                    BoxCollider2D newCol = trigger.gameObject.AddComponent<BoxCollider2D>();
                    newCol.isTrigger = true;
                    newCol.size = new Vector2(1f, 1f);
                    EditorUtility.SetDirty(trigger);
                }
                
                if (GUILayout.Button("Add Circle Collider 2D", GUILayout.Height(25)))
                {
                    CircleCollider2D newCol = trigger.gameObject.AddComponent<CircleCollider2D>();
                    newCol.isTrigger = true;
                    newCol.radius = 0.5f;
                    EditorUtility.SetDirty(trigger);
                }
                EditorGUILayout.EndHorizontal();
            }
            else if (!col.isTrigger)
            {
                EditorGUILayout.HelpBox(
                    "⚠️ Collider2D is not set as trigger! Enable 'Is Trigger' in the collider settings.",
                    MessageType.Warning);
                
                if (GUILayout.Button("Set as Trigger", GUILayout.Height(25)))
                {
                    col.isTrigger = true;
                    EditorUtility.SetDirty(trigger);
                }
            }
            else
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Setup Complete!", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Collider Type: {col.GetType().Name}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("✓ Is Trigger: Enabled", EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
            }
        }
    }

    private string GetCollectableDescription(CollectableType type)
    {
        switch (type)
        {
            case CollectableType.Bomb:
                return "💣 Bomb: Explosive attack ability";
            
            case CollectableType.Invisibility:
                return "👻 Invisibility: Temporary stealth ability";
            
            case CollectableType.Shield:
                return "🛡️ Shield: Protective barrier ability";
            
            case CollectableType.Staff:
                return "🪄 Staff: Magic staff weapon upgrade";
            
            default:
                return "";
        }
    }
}