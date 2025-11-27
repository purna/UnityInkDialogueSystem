


// ═══════════════════════════════════════════════════════════════════════════
// COLLECTABLE TYPE 1: CURRENCY (Coins/Money)
// ═══════════════════════════════════════════════════════════════════════════
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Game;

/// <summary>
/// Standard currency collectable (coins, gold, gems, etc.)
/// 
/// PURPOSE:
/// - Gives player money/currency when collected
/// - Currency is managed by CurrencyManager
/// - Does NOT add to inventory (just gives currency)
/// 
/// WHEN TO USE:
/// - Coins scattered around the level
/// - Enemy drops
/// - Chest rewards
/// - Currency pickups
/// 
/// EXAMPLE USES:
/// - Small coin: CurrencyAmount = 1
/// - Gold coin: CurrencyAmount = 5
/// - Gem: CurrencyAmount = 10
/// 
/// WHAT HAPPENS ON COLLECT:
/// 1. Adds currency to CurrencyManager
/// 2. Plays visual/audio effect (flash, sound)
/// 3. Destroys the pickup
/// </summary>
[CreateAssetMenu(menuName = "Pixelagent/Collectable/Currency", fileName = "New Coin Collectable")]
public class CollectableCurrencySO : CollectableSOBase
{
    [Header("Collectable Stats")]
    [Tooltip("Amount of currency this collectable gives (1 = small coin, 5 = gold coin, etc.)")]
    public int CurrencyAmount = 1;

    public override void Collect(GameObject objectThatCollected)
    {
        // Give currency to the player
        CurrencyManager.Instance.IncrementCurrency(CurrencyAmount);

        // Play collection visual/audio feedback
        if (_playerEffects == null)
            GetReference(objectThatCollected);
        _playerEffects.PlayCollectionEffect(CollectionFlashTime, CollectColor, CollectionClip);
    }
}