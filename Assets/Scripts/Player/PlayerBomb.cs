using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBomb : MonoBehaviour
{
    [Header("Bomb Configuration")]
    [SerializeField] private GameObject bombObject;
    [SerializeField] private Transform bombSpawnPosition;
    [SerializeField] private PlayerUpgrades playerUpgrades;
    [SerializeField] public bool IsActive = false;
    
    [Header("Bomb Settings")]
    [SerializeField] private int maxBombs = 10;
    [SerializeField] private float bombDropDelay = 0.5f;
    [SerializeField] private float countdownTime = 30f;
    
    private int currentBombCount;
    private bool canDropBomb = true;
    private PlayerBombCountdown bombCountdown;

    private void Start()
    {
        currentBombCount = maxBombs;
        
        // Create and initialize the countdown system
        GameObject countdownObj = new GameObject("BombCountdown");
        countdownObj.transform.SetParent(this.transform);
        bombCountdown = countdownObj.AddComponent<PlayerBombCountdown>();
        bombCountdown.Initialize(countdownTime, OnCountdownFinished);
    }

    private void Update()
    {
        if (playerUpgrades.BombUpgradeUnlocked == true)
        {
            // Check if the E key was pressed this frame
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                if (bombCountdown.IsCountdownActive)
                {
                    TryDropBomb();
                }
                else
                {
                    // Start countdown on first bomb drop
                    bombCountdown.StartCountdown();
                    TryDropBomb();
                }
            }
            
            // Check if the F key was pressed this frame
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                playerUpgrades.LockBomb();
            }
        }
    }

    private void TryDropBomb()
    {
        // Check if we can drop a bomb (have bombs left, not on cooldown, countdown not finished)
        if (currentBombCount > 0 && canDropBomb && !bombCountdown.IsCountdownFinished)
        {
            SpawnBomb();
            currentBombCount--;
            
            // Start cooldown
            StartCoroutine(BombDropCooldown());
            
            // Check if we've used all bombs
            if (currentBombCount <= 0)
            {
                OnBombsExhausted();
            }
        }
    }

    private void SpawnBomb()
    {
        // Instantiate the bomb prefab at the spawn position
        if (bombObject != null && bombSpawnPosition != null)
        {
            Instantiate(bombObject, bombSpawnPosition.position, bombSpawnPosition.rotation);
            Debug.Log($"Bomb dropped! Remaining bombs: {currentBombCount - 1}");
        }
        else
        {
            Debug.LogWarning("Bomb prefab or spawn position is not assigned.");
        }
    }

    private IEnumerator BombDropCooldown()
    {
        canDropBomb = false;
        yield return new WaitForSeconds(bombDropDelay);
        canDropBomb = true;
    }

    private void OnCountdownFinished()
    {
        Debug.Log("Bomb countdown finished! No more bombs can be dropped.");
        playerUpgrades.LockBomb();
    }

    private void OnBombsExhausted()
    {
        Debug.Log("All bombs used! No more bombs can be dropped.");
        playerUpgrades.LockBomb();
    }

    public void SetBombObject(GameObject collectedObject)
    {
        bombObject = collectedObject;
    }

    // Public methods to get current status
    public int GetRemainingBombs()
    {
        return currentBombCount;
    }

    public float GetRemainingTime()
    {
        return bombCountdown != null ? bombCountdown.GetRemainingTime() : 0f;
    }

    public bool CanDropBombs()
    {
        return currentBombCount > 0 && !bombCountdown.IsCountdownFinished && canDropBomb;
    }
}
