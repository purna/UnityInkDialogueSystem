using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(CollectableUpgradeSO))]
public class CollectableUpgradeSOEditor : CollectableSOBaseEditor
{
    private SerializedProperty upgradeTypeProp;

    private void OnEnable()
    {
        upgradeTypeProp = serializedObject.FindProperty("_upgradeToGivePlayer");
    }

    public override void OnInspectorGUI()
    {
        CollectableUpgradeSO upgrade = (CollectableUpgradeSO)target;
        
        // Draw preview icon
        DrawPreviewIcon(upgrade.ItemIcon, "⚡ Upgrade Preview");
        
        // Draw info box
        DrawInfoBox(
            title: "PLAYER UPGRADE COLLECTABLE",
            purpose: "Unlocks new abilities and power-ups for the player. When collected, these items permanently unlock new gameplay mechanics and are added to inventory for tracking purposes.",
            whenToUse: new string[]
            {
                "Boss rewards for completing levels",
                "Hidden power-ups in secret areas",
                "Quest completion rewards",
                "Rare pickups in challenging locations",
                "Tutorial rewards for teaching mechanics"
            },
            examples: new string[]
            {
                "Bomb: Unlocks explosive abilities",
                "Invisibility: Allows player to become invisible",
                "Shield: Provides protective barriers",
                "Staff: Enables magical attacks",
                "Prayer: Grants healing/blessing abilities"
            },
            whatHappens: "1. Unlocks ability in PlayerUpgrades system\n2. Adds to inventory (for tracking/statistics)\n3. Plays visual/audio effect\n4. Destroys the pickup"
        );
        
        // Draw default inspector
        DrawDefaultInspector();
        
        // Show upgrade description using serialized property
        serializedObject.Update();
        int upgradeIndex = upgradeTypeProp.enumValueIndex;
        serializedObject.ApplyModifiedProperties();
        
        string upgradeDescription = GetUpgradeDescription(upgradeIndex);
        if (!string.IsNullOrEmpty(upgradeDescription))
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox($"⚡ This upgrade unlocks: {upgradeDescription}", MessageType.Info);
        }
    }
    
    private string GetUpgradeDescription(int upgradeIndex)
    {
        // Match the enum order from CollectableUpgradeSO.UpgradeToGivePlayer
        switch (upgradeIndex)
        {
            case 0: // Bomb
                return "Explosive abilities for combat";
            case 1: // Invisibility
                return "Ability to become invisible to enemies";
            case 2: // Shield
                return "Protective barriers for defense";
            case 3: // Staff
                return "Magical attack abilities";
            case 4: // Prayer
                return "Healing and blessing abilities";
            default:
                return "Unknown upgrade type";
        }
    }
}

