using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// ==================== PLAYER DASH ====================
public class PlayerDash : MonoBehaviour, IPlayerUpgrade
{
    public string UpgradeName => "Dash";
    public bool IsActive { get; set; }

    [Header("Dash Configuration")]
    [SerializeField] private PlayerUpgrades playerUpgrades;
    [SerializeField] private Rigidbody2D playerRigidbody;

    [Header("Dash Settings")]
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private int maxDashes = 3;

    private int currentDashCount;
    private bool canDash = true;
    private bool isDashing = false;

    private void Start()
    {
        currentDashCount = maxDashes;
        
        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponent<Rigidbody2D>();
        }
    }

    private void Update()
    {
        if (!IsActive) return;

        if (Keyboard.current.eKey.wasPressedThisFrame && canDash && currentDashCount > 0)
        {
            StartCoroutine(PerformDash());
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
        isDashing = false;
    }

    private IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;
        currentDashCount--;

        // Get dash direction based on player facing direction or input
        float dashDirection = transform.localScale.x > 0 ? 1f : -1f;
        
        if (playerRigidbody != null)
        {
            playerRigidbody.velocity = new Vector2(dashDirection * dashForce, 0);
        }

        Debug.Log($"Dash activated! Remaining dashes: {currentDashCount}");

        yield return new WaitForSeconds(dashDuration);
        
        isDashing = false;

        if (currentDashCount <= 0)
        {
            playerUpgrades.LockUpgrade(UpgradeName);
            yield break;
        }

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public bool IsDashing => isDashing;
}
