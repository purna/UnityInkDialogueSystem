using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using UnityEngine;
using TMPro;

/// <summary>
/// *SUMMARY:
/// The PlayerUpgrades script manages the unlocking of player abilities in a Unity game. It allows the player to gain new upgrades, such as bombs and invisibility, which are initially disabled. The script includes:
/// Upgrade Tracking: Stores whether the bomb and invisibility upgrades are unlocked.
/// Component Management: Retrieves PlayerBomb and PlayerInvisibility components and disables them initially.
/// Upgrade Unlocking: Enables the respective components when the upgrades are unlocked.
/// </summary>
/// *EXAMPLE USAGE:
/*
using UnityEngine;

public class GameProgress : MonoBehaviour
{
    public PlayerUpgrades playerUpgrades;

    /// Example function to unlock bomb upgrade after defeating a boss
    public void DefeatBoss()
    {
        /// Unlock the bomb ability when the boss is defeated
        playerUpgrades.UnlockBomb();
        Debug.Log("Bomb upgrade unlocked!");
    }

    /// Example function to unlock invisibility upgrade after reaching a checkpoint
    public void ReachCheckpoint()
    {
        /// Unlock the invisibility ability when the player reaches a checkpoint
        playerUpgrades.UnlockInvisibility();
        Debug.Log("Invisibility upgrade unlocked!");
    }
}

*/
public class PlayerUpgrades : MonoBehaviour
{
    public bool BombUpgradeUnlocked { get; private set; }
    public bool InvisibilityUpgradeUnlocked { get; private set; }
    public bool ShieldUpgradeUnlocked { get; private set; }
    public bool StaffUpgradeUnlocked { get; private set; }
    public bool PrayerUpgradeUnlocked { get; private set; }

    private PlayerBomb _playerBomb;
    private PlayerInvisibility _playerInvisibility;
    private PlayerShield _playerShield;
    private PlayerStaff _playerStaff;
    private PlayerPrayer _playerPrayer;

    [SerializeField] TMP_Text UIText;

    private string inventryText;

    private void Awake()
    {
        _playerBomb = GetComponent<PlayerBomb>();
        _playerInvisibility = GetComponent<PlayerInvisibility>();
        _playerShield = GetComponent<PlayerShield>();
        _playerStaff = GetComponent<PlayerStaff>();
        _playerPrayer = GetComponent<PlayerPrayer>();

        _playerBomb.enabled = false;
        _playerInvisibility.enabled = false;
        _playerShield.enabled = false;
        _playerStaff.enabled = false;
        _playerPrayer.enabled = false;
    }

    private void Update()
    {

        if (BombUpgradeUnlocked == true)
        {
            inventryText = "Bomb";
        } 
        else if (InvisibilityUpgradeUnlocked == true) 
        {
            inventryText = "Invisibility";
        } 
        else if (ShieldUpgradeUnlocked == true) 
        {
            inventryText = "Shield";
        } 
         else if (StaffUpgradeUnlocked == true) 
        {
            inventryText = "Staff";
        }
         else if (PrayerUpgradeUnlocked == true) 
        {
            inventryText = "Prayer";
        }
        else 
        {
            inventryText = "";
        }

       // UIText.text = "Inventory " + inventryText;


    }

    public void UnlockUpgrade(string upgradeName)
    {
        switch (upgradeName)
        {
            case "Bomb":
                UnlockBomb();
                break;
            case "Invisibility":
                UnlockInvisibility();
                break;
            case "Shield":
                UnlockShield();
                break;
            case "Staff":
                UnlockStaff();
                break;
            case "Prayer":
                UnlockPrayer();
                break;
            default:
                Debug.LogWarning($"Upgrade '{upgradeName}' not found.");
                break;
        }
    }

    public void UnlockBomb()
    {
        BombUpgradeUnlocked = true;
        _playerBomb.enabled = true;
        _playerBomb.IsActive = true;
        //Lock over upgrades
        LockPrayer();
        LockInvisibility();
        LockShield();
        LockStaff();

    }
    public void UnlockInvisibility()
    {
        InvisibilityUpgradeUnlocked = true;
        _playerInvisibility.enabled = true;
        _playerInvisibility.IsActive = true;
        //Lock over upgrades
        LockBomb();
        LockPrayer();
        LockShield();
        LockStaff();
        

    }
    public void UnlockShield()
    {
        ShieldUpgradeUnlocked = true;
        _playerShield.enabled = true;
        _playerShield.IsActive = true;
        //Lock over upgrades
        LockBomb();
        LockPrayer();
        LockInvisibility();
        LockStaff();
    }
    public void UnlockStaff()
    {
        StaffUpgradeUnlocked = true;
        _playerStaff.enabled = true;
        _playerStaff.IsActive = true;
        //Lock over upgrades
        LockBomb();
        LockPrayer();
        LockInvisibility();
        LockShield();  
    }

      public void UnlockPrayer()
    {
        PrayerUpgradeUnlocked = true;
        _playerPrayer.enabled = true;
        _playerPrayer.IsActive = true;
        //Lock over upgrades
        LockBomb();
        LockInvisibility();
        LockShield(); 
        LockStaff(); 
    }

     public void LockBomb()
    {
        BombUpgradeUnlocked = false;
        _playerBomb.enabled = false;
        _playerBomb.IsActive = false;
    }
    public void LockInvisibility()
    {
        InvisibilityUpgradeUnlocked = false;
        _playerInvisibility.enabled = false;
        _playerInvisibility.IsActive = false;
        
    }
    public void LockShield()
    {
        ShieldUpgradeUnlocked = false;
        _playerShield.enabled = false;
        _playerShield.IsActive = false;
        
    }
    public void LockStaff()
    {
        StaffUpgradeUnlocked = false;
        _playerStaff.enabled = false;
        _playerStaff.IsActive = false;
        
    }
      public void LockPrayer()
    {
        PrayerUpgradeUnlocked = false;
        _playerPrayer.enabled = false;
        _playerPrayer.IsActive = false;
        
    }
}
