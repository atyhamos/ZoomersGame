using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    private bool moveLeft, moveRight, jump, isGrounded, isLeftWalled, isRightWalled, canDoubleJump;
    private int extraJumpCount = 0;
    [SerializeField]
    private Transform groundCheck;
    [SerializeField]
    private Transform leftWallCheck;
    [SerializeField]
    private Transform rightWallCheck;
    [SerializeField]
    private float moveSpeed = 10f;
    [SerializeField]
    private float jumpSpeed = 500f;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void MoveLeft()
    {
        moveLeft = true;
    }

    public void MoveRight()
    {
        moveRight = true;
    }
    public void Jump()
    {
        jump = true;
    }
    public void StopMoving()
    {
        moveLeft = false;
        moveRight = false;
        jump = false;
    }
    private void FixedUpdate()
    {
        isGrounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));
        isLeftWalled = Physics2D.Linecast(transform.position, leftWallCheck.position, 1 << LayerMask.NameToLayer("Ground"));
        isRightWalled = Physics2D.Linecast(transform.position, rightWallCheck.position, 1 << LayerMask.NameToLayer("Ground"));
        if (isGrounded)
        {
            extraJumpCount = 0;
            if (rb.velocity == Vector2.zero || isLeftWalled || isRightWalled)
                animator.Play("PlayerIdleAnimation");
            if (moveLeft)
            {
                rb.velocity = new Vector2(-moveSpeed, rb.velocity.y);
                animator.Play("PlayerRunAnimation");
                spriteRenderer.flipX = true;
            }
            else if (moveRight)
            {
                rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
                animator.Play("PlayerRunAnimation");
                spriteRenderer.flipX = false;
            }
            else
            {
                animator.Play("PlayerIdleAnimation");
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
            if (jump)
            {
                rb.AddForce(Vector2.up * jumpSpeed);
                animator.Play("PlayerJumpUpAnimation");
                isGrounded = false;
                jump = false;
            }
        }
        // Not Grounded
        if (moveLeft && !isLeftWalled)
        {
            rb.velocity = new Vector2(-moveSpeed, rb.velocity.y);
            spriteRenderer.flipX = true;
        }
        if (moveRight && !isRightWalled)
        {
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
            spriteRenderer.flipX = false;
        }
        if (jump && extraJumpCount < 1)
        {
            rb.AddForce(Vector2.up * jumpSpeed);
            animator.Play("PlayerJumpUpAnimation");
            jump = false;
            extraJumpCount++;
        }
    }
}
