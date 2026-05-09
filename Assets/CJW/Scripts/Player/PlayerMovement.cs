using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float jumpPower = 3f;
    private int jumpCount = 0;
    [SerializeField] private int maxJumpCount = 2;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private float moveInput;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
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

        if (IsGrounded())
        {
            jumpCount = 0;
        }
    }

    private void FixedUpdate()
    {
        if (rb == null) return;
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);



        /*Debug.DrawRay(rb.position, Vector3.down, new Color(0, 1, 0));
        RaycastHit2D rayHit = Physics2D.Raycast(rb.positon, Vector3.down, 1);

        if (rayHit.collider != null)
        {
            Debug.Log(rayHit.collider.name);
        }*/
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            rb.position,
            Vector2.down,
            0.6f
        );

        return hit.collider != null;
    }

    private void EndJump()
    {
        animator.SetBool("isJumping", false);
    }
}

