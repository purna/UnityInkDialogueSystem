using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// ==================== PLAYER BOMB ====================
public class PlayerBomb : MonoBehaviour, IPlayerUpgrade
{
    public string UpgradeName => "Bomb";
    public bool IsActive { get; set; }

    [Header("Bomb Configuration")]
    [SerializeField] private GameObject bombObject;
    [SerializeField] private Transform bombSpawnPosition;
    [SerializeField] private PlayerUpgrades playerUpgrades;
    
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
        
        GameObject countdownObj = new GameObject("BombCountdown");
        countdownObj.transform.SetParent(this.transform);
        bombCountdown = countdownObj.AddComponent<PlayerBombCountdown>();
        bombCountdown.Initialize(countdownTime, OnCountdownFinished);
    }

    private void Update()
    {
        if (!IsActive) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (bombCountdown.IsCountdownActive)
            {
                TryDropBomb();
            }
            else
            {
                bombCountdown.StartCountdown();
                TryDropBomb();
            }
        }
        
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            playerUpgrades.LockUpgrade(UpgradeName);
        }
    }

    public void Activate()
    {
        IsActive = true;
        enabled = true;
    }

    public void Deactivate()
    {
        IsActive = false;
        enabled = false;
    }

    private void TryDropBomb()
    {
        if (currentBombCount > 0 && canDropBomb && !bombCountdown.IsCountdownFinished)
        {
            SpawnBomb();
            currentBombCount--;
            StartCoroutine(BombDropCooldown());
            
            if (currentBombCount <= 0)
            {
                OnBombsExhausted();
            }
        }
    }

    private void SpawnBomb()
    {
        if (bombObject != null && bombSpawnPosition != null)
        {
            Instantiate(bombObject, bombSpawnPosition.position, bombSpawnPosition.rotation);
            Debug.Log($"Bomb dropped! Remaining bombs: {currentBombCount - 1}");
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
        Debug.Log("Bomb countdown finished!");
        playerUpgrades.LockUpgrade(UpgradeName);
    }

    private void OnBombsExhausted()
    {
        Debug.Log("All bombs used!");
        playerUpgrades.LockUpgrade(UpgradeName);
    }

    public void SetBombObject(GameObject collectedObject)
    {
        bombObject = collectedObject;
    }
}
