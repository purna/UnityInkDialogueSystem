using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Game;

[CreateAssetMenu(menuName = "Pixelagent/Collectable/Player Upgrade", fileName = "New Player Upgrade")]
public class CollectableUpgradeSO : CollectableSOBase
{
    private PlayerUpgrades _playerUpgrades;

    public enum UpgradeToGivePlayer
    {
        Bomb,
        Invisibility,
        Shield,
        Staff,
        Prayer
    }

    [Header("Collectable Stats")]
    [SerializeField] private UpgradeToGivePlayer _upgradeToGivePlayer;

    private string currentItem = "";

    public string CurrentItem { get; private set; }

    public override void Collect(GameObject objectThatCollected)
    {
        // Update inventory
        InventoryManager.Instance.AddItem(this);

        // Give the upgrade to the player
        GivePowerUp(objectThatCollected);

        // Play collection effects
        if (_playerEffects == null)
            GetReference(objectThatCollected);

        _playerEffects.PlayCollectionEffect(CollectionFlashTime, CollectColor, CollectionClip);
    }

    private void GivePowerUp(GameObject objectThatCollected)
    {
        // Get PlayerUpgrades reference if we don't have it
        if (_playerUpgrades == null)
            _playerUpgrades = FinderHelper.GetComponentOnObject<PlayerUpgrades>(objectThatCollected);

        // Simply unlock the upgrade by name - much cleaner!
        string upgradeName = _upgradeToGivePlayer.ToString();
        bool success = _playerUpgrades.UnlockUpgrade(upgradeName);

        if (success)
        {
            Debug.Log($"Player collected and unlocked: {upgradeName}");
        }
        else
        {
            Debug.LogWarning($"Failed to unlock upgrade: {upgradeName}");
        }
    }
}