using UnityEngine;


/// <summary>
/// Interface for player controllers to enable/disable player movement
/// Implement this interface in your player controller script
/// </summary>
public interface IPlayerController
{
    /// <summary>
    /// Enable player movement and controls
    /// </summary>
    void EnablePlayer();

    /// <summary>
    /// Disable player movement and controls
    /// </summary>
    void DisablePlayer();

    /// <summary>
    /// Check if player is currently enabled
    /// </summary>
    bool IsPlayerEnabled { get; }
}

// This script is a basic 2D character controller that allows
// the player to run and jump. It uses Unity's new input system,
// which needs to be set up accordingly for directional movement
// and jumping buttons.

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour,  IPlayerController
{
    private bool _isEnabled = true;


    [Header("Movement Params")]
    public float runSpeed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float gravityScale = 20.0f;

    // components attached to player
    private BoxCollider2D coll;
    private Rigidbody2D rb;
    private Animator _animator;


    // other
    private bool isGrounded = false;

    public bool IsPlayerEnabled => _isEnabled;


    private void Awake()
    {
        coll = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        if (_animator != null)
        {
            _animator = GetComponent<Animator>();
        }

        rb.gravityScale = gravityScale;
    }

    private void FixedUpdate()
    {
       if (!_isEnabled) return; // Don't process input when disabled
        
       
       /*
        if (DialogueManager.GetInstance().dialogueIsPlaying)
        {
            return;
        }
        */

        UpdateIsGrounded();

        HandleHorizontalMovement();

        HandleJumping();
    }

    private void UpdateIsGrounded()
    {
        Bounds colliderBounds = coll.bounds;
        float colliderRadius = coll.size.x * 0.4f * Mathf.Abs(transform.localScale.x);
        Vector3 groundCheckPos = colliderBounds.min + new Vector3(colliderBounds.size.x * 0.5f, colliderRadius * 0.9f, 0);
        // Check if player is grounded
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheckPos, colliderRadius);
        // Check if any of the overlapping colliders are not player collider, if so, set isGrounded to true
        this.isGrounded = false;
        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != coll)
                {
                    this.isGrounded = true;
                    break;
                }
            }
        }
    }

    private void HandleHorizontalMovement()
    {
        Vector2 moveDirection = InputManager.GetInstance().GetMoveDirection();
        rb.velocity = new Vector2(moveDirection.x * runSpeed, rb.velocity.y);
    }

    private void HandleJumping()
    {
        bool jumpPressed = InputManager.GetInstance().GetJumpPressed();
        if (isGrounded && jumpPressed)
        {
            isGrounded = false;
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        }
    }


    public void EnablePlayer()
    {
        _isEnabled = true;
        Debug.Log("[PlayerController] Player movement ENABLED");
    }
    
    public void DisablePlayer()
    {
        _isEnabled = false;
        
        // Stop movement
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        
        // Stop animation
        if (_animator != null)
        {
            _animator.SetFloat("Speed", 0f);
        }
        
        Debug.Log("[PlayerController] Player movement DISABLED");
    }

}