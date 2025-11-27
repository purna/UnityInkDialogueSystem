using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// ============================================================
// APPROACH 2: Skills enhance existing Upgrades
// ============================================================

/// <summary>
/// Enhanced PlayerBomb that can be modified by skills
/// Example: Skill adds +2 bombs, or +5 seconds to timer
/// </summary>
public class EnhancedPlayerBomb : MonoBehaviour, IPlayerUpgrade
{
    public string UpgradeName => "Bomb";
    public bool IsActive { get; set; }

    [Header("Base Stats")]
    [SerializeField] private int baseBombCount = 10;
    [SerializeField] private float baseCountdownTime = 30f;
    
    [Header("Skill Modifiers")]
    private int bonusBombs = 0;
    private float bonusTime = 0f;
    
    // These get called by SkillFunctions
    public void AddBonusBombs(int amount)
    {
        bonusBombs += amount;
        Debug.Log($"Bomb upgrade enhanced! +{amount} bombs (Total bonus: {bonusBombs})");
    }
    
    public void AddBonusTime(float seconds)
    {
        bonusTime += seconds;
        Debug.Log($"Bomb timer enhanced! +{seconds}s (Total bonus: {bonusTime}s)");
    }
    
    public int GetMaxBombs() => baseBombCount + bonusBombs;
    public float GetCountdownTime() => baseCountdownTime + bonusTime;

    
    // Fulfilling "Activate"
    public void Activate() 
    { 
        IsActive = true; 
        enabled = true; // Enable this script
        Debug.Log("Bomb ability unlocked!");
    }

    // ulfilling "Deactivate"
    public void Deactivate() 
    { 
        IsActive = false; 
        enabled = false; // Disable this script
    }
}