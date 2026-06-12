using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player movement, jumping, and character animations.
/// Supports horizontal movement, double jump,
/// sprite flipping, and ground detection.
/// </summary>

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float jumpPower = 3f;

    // Current number of jumps performed since leaving the ground.
    private int jumpCount = 0;
    private int maxJumpCount = 2;

    private LayerMask groundLayer;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private float moveInput;

    private void Awake()
    {
        groundLayer = LayerMask.GetMask("Ground");

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void OnDisable()
    {
        moveInput = 0f;

        if (rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (animator != null)
            animator.SetBool("isWalking", false);
    }

    private void Update()
    {
        moveInput = 0f;

        if (Keyboard.current.spaceKey.wasPressedThisFrame && jumpCount < maxJumpCount)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
            jumpCount++;

            animator.SetBool("isJumping", true);
            Invoke("EndJump", 0.8f);
        }

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            moveInput = -1f;
        else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            moveInput = 1f;

        if (animator != null)
            animator.SetBool("isWalking", moveInput != 0f);

        if (spriteRenderer != null)
        {
            if (moveInput < 0) spriteRenderer.flipX = true;
            else if (moveInput > 0) spriteRenderer.flipX = false;
        }

        if (IsGrounded() && rb.linearVelocity.y <= 0f)
        {
            jumpCount = 0;
        }
    }

    private void FixedUpdate()
    {
        if (rb == null) return;
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

    }

    // Checks whether the player is standing on the Ground layer.
    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            rb.position,
            Vector2.down,
            1f,
            groundLayer
        );

        return hit.collider != null;
    }

    private void EndJump()
    {
        animator.SetBool("isJumping", false);
    }
}

