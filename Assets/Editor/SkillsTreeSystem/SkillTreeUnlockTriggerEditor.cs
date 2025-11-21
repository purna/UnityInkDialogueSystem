using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal;

[CustomEditor(typeof(SkillTreeUnlockTrigger))]
public class SkillTreeUnlockTriggerEditor : Editor
{
    // Base class properties
    private SerializedProperty visualCueProp;
    private SerializedProperty emoteAnimatorProp;
    private SerializedProperty triggerOnEnterProp;
    private SerializedProperty requiresInputProp;
    private SerializedProperty interactKeyProp;
    private SerializedProperty canTriggerMultipleTimesProp;
    private SerializedProperty interactPromptProp;
    private SerializedProperty promptTextComponentProp;
    private SerializedProperty promptTextProp;
    private SerializedProperty fadeInDurationProp;
    private SerializedProperty displayDurationProp;
    private SerializedProperty fadeOutDurationProp;
    private SerializedProperty loopPromptProp;
    private SerializedProperty promptLeftOffsetProp;
    private SerializedProperty promptRightOffsetProp;
    private SerializedProperty promptVerticalOffsetProp;
    private SerializedProperty continuousPositionUpdateProp;
    private SerializedProperty showDebugLogsProp;
    private SerializedProperty playEmoteOnTriggerProp;

    // Derived class properties
    private SerializedProperty skillTreeManagerProp;
    private SerializedProperty skillsTreeControllerProp;
    private SerializedProperty abilitiesToUnlockProp;
    private SerializedProperty skillsToUnlockProp;
    private SerializedProperty respectSkillRequirementsProp;
    private SerializedProperty skillPointsRewardProp;
    private SerializedProperty addToCurrentOnlyProp;
    private SerializedProperty collectableRewardsProp;
    private SerializedProperty rewardSpawnOffsetProp;

    // Visual feedback properties
    private SerializedProperty spriteRendererProp;
    private SerializedProperty unlockParticlesProp;
    private SerializedProperty unlockSoundProp;

    // Locked prompt properties
    private SerializedProperty lockedPromptObjectProp;
    private SerializedProperty lockedPromptTextProp;
    private SerializedProperty lockedPromptDisplayTimeProp;

    private ReorderableList abilitiesList;
    private ReorderableList skillsList;
    private ReorderableList collectableRewardsList;
    private bool showPromptSettings = false;
    private bool showPositioningSettings = false;
    private bool showLockedPromptSettings = false;

    private void OnEnable()
    {
        // Base class properties
        visualCueProp = serializedObject.FindProperty("visualCue");
        emoteAnimatorProp = serializedObject.FindProperty("emoteAnimator");
        triggerOnEnterProp = serializedObject.FindProperty("triggerOnEnter");
        requiresInputProp = serializedObject.FindProperty("requiresInput");
        interactKeyProp = serializedObject.FindProperty("interactKey");
        canTriggerMultipleTimesProp = serializedObject.FindProperty("canTriggerMultipleTimes");
        interactPromptProp = serializedObject.FindProperty("interactPrompt");
        promptTextComponentProp = serializedObject.FindProperty("promptTextComponent");
        promptTextProp = serializedObject.FindProperty("promptText");
        fadeInDurationProp = serializedObject.FindProperty("fadeInDuration");
        displayDurationProp = serializedObject.FindProperty("displayDuration");
        fadeOutDurationProp = serializedObject.FindProperty("fadeOutDuration");
        loopPromptProp = serializedObject.FindProperty("loopPrompt");
        promptLeftOffsetProp = serializedObject.FindProperty("promptLeftOffset");
        promptRightOffsetProp = serializedObject.FindProperty("promptRightOffset");
        promptVerticalOffsetProp = serializedObject.FindProperty("promptVerticalOffset");
        continuousPositionUpdateProp = serializedObject.FindProperty("continuousPositionUpdate");
        showDebugLogsProp = serializedObject.FindProperty("showDebugLogs");
        playEmoteOnTriggerProp = serializedObject.FindProperty("playEmoteOnTrigger");

        // Derived class properties
        skillTreeManagerProp = serializedObject.FindProperty("skillTreeManager");
        skillsTreeControllerProp = serializedObject.FindProperty("skillsTreeController");
        abilitiesToUnlockProp = serializedObject.FindProperty("abilitiesToUnlock");
        skillsToUnlockProp = serializedObject.FindProperty("skillsToUnlock");
        respectSkillRequirementsProp = serializedObject.FindProperty("respectSkillRequirements");
        skillPointsRewardProp = serializedObject.FindProperty("skillPointsReward");
        addToCurrentOnlyProp = serializedObject.FindProperty("addToCurrentOnly");
        collectableRewardsProp = serializedObject.FindProperty("collectableRewards");
        rewardSpawnOffsetProp = serializedObject.FindProperty("rewardSpawnOffset");

        // Visual feedback properties
        spriteRendererProp = serializedObject.FindProperty("spriteRenderer");
        unlockParticlesProp = serializedObject.FindProperty("unlockParticles");
        unlockSoundProp = serializedObject.FindProperty("unlockSound");

        // Locked prompt properties
        lockedPromptObjectProp = serializedObject.FindProperty("lockedPromptObject");
        lockedPromptTextProp = serializedObject.FindProperty("lockedPromptText");
        lockedPromptDisplayTimeProp = serializedObject.FindProperty("lockedPromptDisplayTime");

        // Setup Abilities List
        abilitiesList = new ReorderableList(serializedObject, abilitiesToUnlockProp, true, true, true, true);
        abilitiesList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Abilities to Unlock (IDs)");
        };
        abilitiesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = abilitiesList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                element,
                GUIContent.none
            );
        };

        // Setup Skills List
        skillsList = new ReorderableList(serializedObject, skillsToUnlockProp, true, true, true, true);
        skillsList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Skills to Unlock");
        };
        skillsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = skillsList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                element,
                GUIContent.none
            );
        };

        // Setup Collectable Rewards List
        collectableRewardsList = new ReorderableList(serializedObject, collectableRewardsProp, true, true, true, true);
        collectableRewardsList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Collectable Rewards (Prefabs)");
        };
        collectableRewardsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = collectableRewardsList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                element,
                GUIContent.none
            );
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        SkillTreeUnlockTrigger trigger = (SkillTreeUnlockTrigger)target;

        // Configuration Warning Box at top
        DrawConfigurationWarning();

        EditorGUILayout.Space(5);

        // Visual Feedback Section
        DrawVisualFeedbackSection();
        EditorGUILayout.Space();

        // Emote Animator
        EditorGUILayout.LabelField("Emote Animator", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(emoteAnimatorProp, new GUIContent("Animator (Optional)"));
        EditorGUILayout.PropertyField(playEmoteOnTriggerProp, new GUIContent("Play Emote On Trigger"));
        EditorGUILayout.Space();

        // System References
        DrawSystemReferences();
        EditorGUILayout.Space();

        // Unlock Settings (Main Section)
        DrawUnlockSettings();
        EditorGUILayout.Space();

        // Collectable Rewards Section
        DrawCollectableRewardsSection();
        EditorGUILayout.Space();

        // Trigger Settings
        DrawTriggerSettings();
        EditorGUILayout.Space();

        // UI Prompt Section (only if requires input)
        if (requiresInputProp.boolValue && !triggerOnEnterProp.boolValue)
        {
            DrawUIPromptSettings();
            EditorGUILayout.Space();
        }

        // Locked Prompt Settings (always show)
        DrawLockedPromptSettings();
        EditorGUILayout.Space();

        // Feedback Settings
        DrawFeedbackSettings();
        EditorGUILayout.Space();

        // Utility Buttons
        DrawUtilityButtons();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawConfigurationWarning()
    {
        bool triggerOnEnter = triggerOnEnterProp.boolValue;
        bool requiresInput = requiresInputProp.boolValue;
        bool respectRequirements = respectSkillRequirementsProp.boolValue;

        if (!triggerOnEnter && !requiresInput)
        {
            EditorGUILayout.HelpBox(
                "⚠️ WARNING: Both 'Trigger On Enter' and 'Requires Input' are FALSE!\n" +
                "This trigger will NEVER activate. Please enable one of them.",
                MessageType.Error);
        }
        else if (respectRequirements && skillsToUnlockProp.arraySize > 0)
        {
            EditorGUILayout.HelpBox(
                "🔒 PREREQUISITES MODE: Will check if skills have their prerequisites unlocked.\n" +
                "Shows locked prompt with missing prerequisite name if requirements aren't met.",
                MessageType.Info);
        }
        else if (triggerOnEnter)
        {
            EditorGUILayout.HelpBox(
                "✓ AUTO TRIGGER MODE: Unlocks will be applied immediately when player enters.",
                MessageType.Info);
        }
        else if (requiresInput)
        {
            KeyCode key = (KeyCode)interactKeyProp.enumValueIndex;
            EditorGUILayout.HelpBox(
                $"✓ INPUT MODE: Player must press '{key}' to claim rewards.",
                MessageType.Info);
        }
    }

    private void DrawVisualFeedbackSection()
    {
        EditorGUILayout.LabelField("Visual Feedback", EditorStyles.boldLabel);
        
        EditorGUILayout.PropertyField(spriteRendererProp, new GUIContent(
            "Sprite Renderer",
            "Will automatically display locked/unlocked icons from the first skill"));
        
        if (spriteRendererProp.objectReferenceValue != null && skillsToUnlockProp.arraySize > 0)
        {
            SerializedProperty firstSkill = skillsToUnlockProp.GetArrayElementAtIndex(0);
            if (firstSkill.objectReferenceValue != null)
            {
                Skill skill = firstSkill.objectReferenceValue as Skill;
                EditorGUILayout.HelpBox(
                    $"💡 Using icons from: {skill.SkillName}\n" +
                    $"Locked Icon: {(skill.LockedIcon != null ? skill.LockedIcon.name : "None")}\n" +
                    $"Unlocked Icon: {(skill.UnlockedIcon != null ? skill.UnlockedIcon.name : "None")}",
                    MessageType.Info);
            }
        }
        
        EditorGUILayout.PropertyField(unlockParticlesProp, new GUIContent(
            "Unlock Particles",
            "Particle effect to play when unlocked"));
        
        EditorGUILayout.PropertyField(unlockSoundProp, new GUIContent(
            "Unlock Sound",
            "Audio clip to play when unlocked"));
        
        EditorGUILayout.PropertyField(visualCueProp, new GUIContent(
            "Additional Visual Cue",
            "Optional extra GameObject for visual indication"));
    }

    private void DrawSystemReferences()
    {
        EditorGUILayout.LabelField("System References", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(skillTreeManagerProp, new GUIContent(
            "Skill Tree Manager",
            "Will auto-find if not assigned"));
        EditorGUILayout.PropertyField(skillsTreeControllerProp, new GUIContent(
            "Skills Tree Controller",
            "Will auto-find if not assigned"));
    }

    private void DrawUnlockSettings()
    {
        EditorGUILayout.LabelField("Unlock Settings", EditorStyles.boldLabel);
        
        // Summary box
        int abilityCount = abilitiesToUnlockProp.arraySize;
        int skillCount = skillsToUnlockProp.arraySize;
        int pointsReward = skillPointsRewardProp.intValue;
        
        if (abilityCount > 0 || skillCount > 0 || pointsReward > 0)
        {
            string summary = "This trigger will:\n";
            if (abilityCount > 0)
                summary += $"• Unlock {abilityCount} ability/abilities\n";
            if (skillCount > 0)
                summary += $"• Unlock {skillCount} skill(s)\n";
            if (pointsReward > 0)
                summary += $"• Award {pointsReward} skill points";
            
            EditorGUILayout.HelpBox(summary, MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("No skill/ability rewards configured. Add abilities, skills, or skill points below.", MessageType.Warning);
        }
        
        EditorGUILayout.Space(5);
        
        // Abilities List
        EditorGUILayout.LabelField("Abilities", EditorStyles.miniBoldLabel);
        abilitiesList.DoLayoutList();
        
        if (abilityCount > 0)
        {
            EditorGUILayout.HelpBox(
                "💡 TIP: Ability IDs are strings (e.g., 'double_jump', 'dash', 'wall_climb')\n" +
                "These are added to the UnlockedAbilities list in SkillTreeManager.",
                MessageType.None);
        }
        
        EditorGUILayout.Space(5);
        
        // Skills List
        EditorGUILayout.LabelField("Skills", EditorStyles.miniBoldLabel);
        skillsList.DoLayoutList();
        
        if (skillCount > 0)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(respectSkillRequirementsProp, new GUIContent(
                "Respect Requirements",
                "If TRUE, checks skill prerequisites. If FALSE, force unlocks."));
            EditorGUI.indentLevel--;
            
            if (respectSkillRequirementsProp.boolValue)
            {
                EditorGUILayout.HelpBox(
                    "Requirements Mode: Skills will only unlock if their prerequisites are met.\n" +
                    "Will show locked prompt with missing prerequisite name if not met.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Force Unlock Mode: Skills will unlock regardless of prerequisites (useful for story rewards).",
                    MessageType.Warning);
            }
        }
        
        EditorGUILayout.Space(5);
        
        // Skill Points Reward
        EditorGUILayout.LabelField("Skill Points", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(skillPointsRewardProp, new GUIContent(
            "Points to Award",
            "Skill points given to the player when triggered"));
        
        if (skillPointsRewardProp.intValue > 0)
        {
            EditorGUILayout.PropertyField(addToCurrentOnlyProp, new GUIContent(
                "Add to Current Only",
                "If TRUE: only adds to spendable points. If FALSE: adds to both current and total earned."));
            
            if (addToCurrentOnlyProp.boolValue)
            {
                EditorGUILayout.HelpBox(
                    "⚠️ Current Only Mode: Points added will be spendable but won't increase lifetime total.\n" +
                    "Use for: Refunds, temporary bonuses, or point transfers.",
                    MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Normal Mode: Points will be added to both current (spendable) and total earned.\n" +
                    "Use for: Normal rewards, quest completions, achievements.",
                    MessageType.Info);
            }
        }
        EditorGUI.indentLevel--;
    }

    private void DrawCollectableRewardsSection()
    {
        EditorGUILayout.LabelField("Collectable Rewards", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "💰 Spawn collectable prefabs when unlocked (coins, items, health, etc.)\n" +
            "Works just like SkillGatedChest rewards!",
            MessageType.None);
        
        collectableRewardsList.DoLayoutList();
        
        if (collectableRewardsProp.arraySize > 0)
        {
            EditorGUILayout.PropertyField(rewardSpawnOffsetProp, new GUIContent(
                "Spawn Offset",
                "Position offset for spawning rewards (relative to trigger)"));
            
            EditorGUILayout.HelpBox(
                $"✓ Will spawn {collectableRewardsProp.arraySize} collectable(s) when unlocked",
                MessageType.Info);
        }
    }

    private void DrawTriggerSettings()
    {
        EditorGUILayout.LabelField("Trigger Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.PropertyField(triggerOnEnterProp, new GUIContent(
            "Trigger On Enter",
            "If TRUE, applies unlocks immediately when player enters (if prerequisites met)"));
        
        EditorGUILayout.PropertyField(requiresInputProp, new GUIContent(
            "Requires Input",
            "If TRUE, requires key press to apply unlocks"));
        
        if (requiresInputProp.boolValue && !triggerOnEnterProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(interactKeyProp, new GUIContent("Interact Key"));
            EditorGUI.indentLevel--;
        }
        else if (requiresInputProp.boolValue && triggerOnEnterProp.boolValue)
        {
            EditorGUILayout.HelpBox(
                "Note: 'Requires Input' is ignored because 'Trigger On Enter' is enabled.",
                MessageType.Warning);
        }
        
        EditorGUILayout.PropertyField(canTriggerMultipleTimesProp, new GUIContent(
            "Can Trigger Multiple Times",
            "If FALSE, trigger only works once. If TRUE, can be triggered repeatedly."));
        
        if (canTriggerMultipleTimesProp.boolValue)
        {
            EditorGUILayout.HelpBox(
                "⚠️ Repeatable Trigger: Player can gain rewards multiple times by re-entering.\n" +
                "Useful for: Skill point fountains, training areas, or resource nodes.",
                MessageType.Warning);
        }
    }

    private void DrawUIPromptSettings()
    {
        showPromptSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showPromptSettings, "UI Prompt Settings (Success)");
        
        if (showPromptSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(interactPromptProp, new GUIContent("Prompt GameObject"));
            EditorGUILayout.PropertyField(promptTextComponentProp, new GUIContent("Text Component"));
            EditorGUILayout.PropertyField(promptTextProp, new GUIContent("Prompt Message"));
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Fade Animation", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(fadeInDurationProp, new GUIContent("Fade In Duration"));
            EditorGUILayout.PropertyField(displayDurationProp, new GUIContent("Display Duration"));
            EditorGUILayout.PropertyField(fadeOutDurationProp, new GUIContent("Fade Out Duration"));
            EditorGUILayout.PropertyField(loopPromptProp, new GUIContent("Loop Animation"));
            
            EditorGUILayout.Space(5);
            showPositioningSettings = EditorGUILayout.Foldout(showPositioningSettings, "Dynamic Positioning", true);
            if (showPositioningSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(promptLeftOffsetProp, new GUIContent("Left Offset"));
                EditorGUILayout.PropertyField(promptRightOffsetProp, new GUIContent("Right Offset"));
                EditorGUILayout.PropertyField(promptVerticalOffsetProp, new GUIContent("Vertical Offset"));
                EditorGUILayout.PropertyField(continuousPositionUpdateProp, new GUIContent(
                    "Continuous Update",
                    "Update position every frame (for moving triggers)"));
                EditorGUI.indentLevel--;
            }
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawLockedPromptSettings()
    {
        showLockedPromptSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showLockedPromptSettings, "Locked Prompt Settings (When Blocked)");
        
        if (showLockedPromptSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.HelpBox(
                "This prompt shows when player tries to interact but prerequisites aren't met.\n" +
                "Example: 'Requires Dash to be unlocked first!' (when skill prerequisites not met)",
                MessageType.Info);
            
            EditorGUILayout.PropertyField(lockedPromptObjectProp, new GUIContent(
                "Locked Prompt Object",
                "UI GameObject to show when locked"));
            
            EditorGUILayout.PropertyField(lockedPromptTextProp, new GUIContent(
                "Locked Prompt Text",
                "TextMeshProUGUI component for the message"));
            
            EditorGUILayout.PropertyField(lockedPromptDisplayTimeProp, new GUIContent(
                "Display Time",
                "How long to show the locked message"));
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawFeedbackSettings()
    {
        EditorGUILayout.LabelField("Feedback Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(showDebugLogsProp, new GUIContent(
            "Show Debug Logs",
            "Print detailed logs to console for debugging"));
    }

    private void DrawUtilityButtons()
    {
        EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);
        
        SkillTreeUnlockTrigger trigger = (SkillTreeUnlockTrigger)target;
        
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
            
            // Show current state
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Runtime Info", EditorStyles.boldLabel);
            
            GUIStyle statusStyle = new GUIStyle(EditorStyles.label);
            statusStyle.normal.textColor = trigger.IsPlayerInRange ? Color.green : Color.gray;
            EditorGUILayout.LabelField($"Player In Range: {trigger.IsPlayerInRange}", statusStyle);
            
            GUIStyle unlockedStyle = new GUIStyle(EditorStyles.label);
            bool isUnlocked = trigger.HasBeenUnlocked();
            unlockedStyle.normal.textColor = isUnlocked ? Color.green : Color.yellow;
            EditorGUILayout.LabelField($"Has Been Unlocked: {isUnlocked}", unlockedStyle);
            
            GUIStyle triggeredStyle = new GUIStyle(EditorStyles.label);
            triggeredStyle.normal.textColor = trigger.HasTriggered ? Color.yellow : Color.green;
            EditorGUILayout.LabelField($"Has Triggered: {trigger.HasTriggered}", triggeredStyle);
            
            // Show what will be unlocked
            List<string> abilities = trigger.GetAbilitiesToUnlock();
            if (abilities.Count > 0)
            {
                EditorGUILayout.LabelField($"Abilities Queue: {abilities.Count}");
            }
            
            List<Skill> skills = trigger.GetSkillsToUnlock();
            if (skills.Count > 0)
            {
                EditorGUILayout.LabelField($"Skills Queue: {skills.Count}");
            }
            
            EditorGUILayout.EndVertical();
        }
        else
        {
            // Editor Setup
            EditorGUILayout.HelpBox(
                "✓ Trigger is ready! Make sure to:\n" +
                "• Add a 2D Collider with 'Is Trigger' enabled\n" +
                "• Set the collider size to your desired trigger area\n" +
                "• Tag your player GameObject as 'Player'\n" +
                "• Assign at least one Skill to see locked/unlocked visuals",
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
                    newCol.size = new Vector2(2f, 2f);
                    EditorUtility.SetDirty(trigger);
                }
                
                if (GUILayout.Button("Add Circle Collider 2D", GUILayout.Height(25)))
                {
                    CircleCollider2D newCol = trigger.gameObject.AddComponent<CircleCollider2D>();
                    newCol.isTrigger = true;
                    newCol.radius = 1f;
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
            
            // Quick Add Buttons
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Quick Add", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Add Ability Slot"))
            {
                abilitiesToUnlockProp.arraySize++;
                serializedObject.ApplyModifiedProperties();
            }
            
            if (GUILayout.Button("+ Add Skill Slot"))
            {
                skillsToUnlockProp.arraySize++;
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Add Reward Slot"))
            {
                collectableRewardsProp.arraySize++;
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}