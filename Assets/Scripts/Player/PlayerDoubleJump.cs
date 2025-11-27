using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


// ==================== PLAYER DOUBLE JUMP ====================
public class PlayerDoubleJump : MonoBehaviour, IPlayerUpgrade
{
    public string UpgradeName => "DoubleJump";
    public bool IsActive { get; set; }

    [Header("Double Jump Configuration")]
    [SerializeField] private PlayerUpgrades playerUpgrades;
    [SerializeField] private Rigidbody2D playerRigidbody;

    [Header("Double Jump Settings")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private int maxDoubleJumps = 5;

    private int currentDoubleJumpCount;
    private bool hasDoubleJumped = false;

    private void Start()
    {
        currentDoubleJumpCount = maxDoubleJumps;
        
        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponent<Rigidbody2D>();
        }
    }

    private void Update()
    {
        if (!IsActive) return;

        if (Keyboard.current.eKey.wasPressedThisFrame && currentDoubleJumpCount > 0)
        {
            PerformDoubleJump();
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

    private void PerformDoubleJump()
    {
        if (playerRigidbody != null)
        {
            // Reset vertical velocity before applying jump force
            playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x, 0);
            playerRigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        currentDoubleJumpCount--;
        Debug.Log($"Double jump activated! Remaining jumps: {currentDoubleJumpCount}");

        if (currentDoubleJumpCount <= 0)
        {
            playerUpgrades.LockUpgrade(UpgradeName);
        }
    }

    public void ResetDoubleJump()
    {
        hasDoubleJumped = false;
    }
}