using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CollectableCurrencySO))]
public class CollectableCurrencySOEditor : CollectableSOBaseEditor
{
    public override void OnInspectorGUI()
    {
        CollectableCurrencySO currency = (CollectableCurrencySO)target;
        
        // Draw preview icon
        DrawPreviewIcon(currency.ItemIcon, "💰 Currency Preview");
        
        // Draw info box
        DrawInfoBox(
            title: "CURRENCY COLLECTABLE (Coins/Money)",
            purpose: "Gives player money/currency when collected. Currency is managed by CurrencyManager and does NOT add to inventory (just gives currency).",
            whenToUse: new string[]
            {
                "Coins scattered around the level",
                "Enemy drops",
                "Chest rewards",
                "Currency pickups"
            },
            examples: new string[]
            {
                "Small coin: CurrencyAmount = 1",
                "Gold coin: CurrencyAmount = 5",
                "Gem: CurrencyAmount = 10",
                "Diamond: CurrencyAmount = 50"
            },
            whatHappens: "1. Adds currency to CurrencyManager\n2. Plays visual/audio effect (flash, sound)\n3. Destroys the pickup"
        );
        
        // Draw default inspector
        DrawDefaultInspector();
        
        // Show current value preview
        if (currency.CurrencyAmount > 0)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox($"💰 This will give {currency.CurrencyAmount} currency when collected.", MessageType.Info);
        }
    }
}