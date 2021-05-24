using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    private bool moveLeft, moveRight, jump, crouch, isGrounded, isLeftWalled, isRightWalled, isSliding;
    private int extraJumpCount, slideCount = 0;
    [SerializeField]
    private BoxCollider2D playerCollider;
    [SerializeField]
    private BoxCollider2D crouchCollider;
    [SerializeField]
    private Transform groundCheck;
    [SerializeField]
    private Transform leftWallCheck;
    [SerializeField]
    private Transform rightWallCheck;
    [SerializeField]
    private float moveSpeed = 500f;
    [SerializeField]
    private float jumpSpeed = 500f;
    [SerializeField]
    private float maxSpeed = 500f;
    private float slideTimer = 0f;
    private float maxSlideTime = 0.8f;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        moveLeft = false;
        moveRight = false;
        crouch = false;
        isSliding = false;
    }

    public void MoveLeft()
    {
        moveLeft = !moveLeft;
    }

    public void MoveRight()
    {
        moveRight = !moveRight;
    }
    public void Jump()
    {
        jump = true;
    }
    public void Crouch()
    {
        if (moveLeft || moveRight)
        {
            crouch = true;
            slideCount = 0;
        }
    }

    private void Animate()
    {
        if (rb.velocity == Vector2.zero || !moveLeft && !moveRight)
            animator.Play("PlayerIdleAnimation");
        else if (isSliding)
            animator.Play("PlayerCrouchAnimation");
        else if (moveLeft && !isLeftWalled)
        {
            animator.Play("PlayerRunAnimation");
            spriteRenderer.flipX = true;
        }
        else if (moveRight && !isRightWalled)
        {
            animator.Play("PlayerRunAnimation");
            spriteRenderer.flipX = false;
        }
        if (jump)
        {
            animator.Play("PlayerJumpUpAnimation");
            jump = false;
        }
    
    }
    private void Slide()
    {
        if (isSliding)
        {
            if (slideTimer < maxSlideTime)
            {
                slideTimer += Time.deltaTime;
            }
            else
            {
                isSliding = false;
                crouch = false;
                slideTimer = 0;
                playerCollider.enabled = true;
                crouchCollider.enabled = false;
                return;
            }
        }
        if (slideCount == 0)
        {
            isSliding = true;
            animator.Play("PlayerCrouchAnimation");
            rb.velocity = new Vector2(0.75f * rb.velocity.x, rb.velocity.y);
            playerCollider.enabled = false;
            crouchCollider.enabled = true;
            slideCount++; 
        }
    }
    private void FixedUpdate()
    {
        isGrounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));
        isLeftWalled = Physics2D.Linecast(transform.position, leftWallCheck.position, 1 << LayerMask.NameToLayer("Ground"));
        isRightWalled = Physics2D.Linecast(transform.position, rightWallCheck.position, 1 << LayerMask.NameToLayer("Ground"));
        if (isGrounded)
        {
            extraJumpCount = 0;
            if (moveLeft && !isLeftWalled)
            {
                // If going right, pressing left will stop momentum
                if (rb.velocity.x > 0)
                    rb.velocity = new Vector2(0, rb.velocity.y);
                if (crouch)
                    Slide();
                else if (rb.velocity.x > -maxSpeed)
                    rb.AddForce(Vector2.left * moveSpeed * Time.deltaTime);
                 
            }
            else if (moveRight && !isRightWalled)
            {
                // If going left, pressing right will stop momentum
                if (rb.velocity.x < 0)
                    rb.velocity = new Vector2(0, rb.velocity.y);
                if (crouch)
                    Slide();
                if (rb.velocity.x < maxSpeed)             
                    rb.AddForce(Vector2.right * moveSpeed * Time.deltaTime);
            }
            else
                rb.velocity = new Vector2(0, rb.velocity.y);
            if (jump)
                rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
            Animate();
        }
        // Not Grounded
        else if (moveLeft && !isLeftWalled)
        {
            if (rb.velocity.x > 0)
                rb.velocity = new Vector2(0, rb.velocity.y);
            if (rb.velocity.x > -maxSpeed)
                rb.AddForce(Vector2.left * moveSpeed * Time.deltaTime);
            spriteRenderer.flipX = true;
        }
        else if (moveRight && !isRightWalled)
        {
            if (rb.velocity.x < 0)
                rb.velocity = new Vector2(0, rb.velocity.y);
            if (rb.velocity.x < maxSpeed)
                rb.AddForce(Vector2.right * moveSpeed * Time.deltaTime);
            spriteRenderer.flipX = false;
        }
        // Can double jump
        if (jump && extraJumpCount < 1)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0.75f * jumpSpeed);
            animator.Play("PlayerJumpUpAnimation");
            jump = false;
            extraJumpCount++;
        }
        // Cannot jump more than once extra
        if (jump && extraJumpCount >= 1)
            jump = false;       
    }
}
