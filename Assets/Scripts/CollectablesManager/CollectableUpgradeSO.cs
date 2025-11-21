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
        //InventoryManager.Instance.UpdateInventory(CurrentItem);

        InventoryManager.Instance.AddItem(this);

        
        GivePowerUp(objectThatCollected);

        if (_playerEffects == null)
            GetReference(objectThatCollected);

        _playerEffects.PlayCollectionEffect(CollectionFlashTime, CollectColor, CollectionClip);
    }

    private void GivePowerUp(GameObject objectThatCollected)
    {
        if (_playerUpgrades == null)
            _playerUpgrades = FinderHelper.GetComponentOnObject<PlayerUpgrades>(objectThatCollected);

        switch (_upgradeToGivePlayer)
        {
            case UpgradeToGivePlayer.Bomb:

                GiveBombPowerUp();
                break;
            case UpgradeToGivePlayer.Invisibility:
                
                GiveInvisibilityPowerUp();
                break;
            case UpgradeToGivePlayer.Staff:
                
                GiveStaffPowerUp();
                break;
            case UpgradeToGivePlayer.Shield:
                
                GiveShieldPowerUp();
                break;
            case UpgradeToGivePlayer.Prayer:
                
                GivePrayerPowerUp();
                break;
        }
    }

    private void GiveBombPowerUp()
    {
        _playerUpgrades.UnlockBomb();
    }

    private void GiveInvisibilityPowerUp()
    {
        _playerUpgrades.UnlockInvisibility();
    }

    private void GiveStaffPowerUp()
    
    {
        _playerUpgrades.UnlockStaff();
    }

    private void GiveShieldPowerUp()
    
    {
        _playerUpgrades.UnlockShield();
    }

    private void GivePrayerPowerUp()
    {
        _playerUpgrades.UnlockPrayer();
    }


}
