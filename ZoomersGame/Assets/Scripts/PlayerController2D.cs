using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    private bool moveLeft, moveRight, jump, isGrounded, canDoubleJump;
    private static int jumpCount = 0;
    [SerializeField]
    private Transform groundCheck;
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
    public void DoubleJump()
    {
        if (!isGrounded && jumpCount == 1)
        {
            canDoubleJump = true;
            jumpCount = 0;
        }
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
            if (rb.velocity == Vector2.zero && isGrounded) {
                animator.Play("PlayerIdleAnimation");
            }
            if (moveLeft)
            {
                rb.velocity = new Vector2(-moveSpeed, rb.velocity.y);
                if (isGrounded)
                    animator.Play("PlayerRunAnimation");
                spriteRenderer.flipX = true;
            }
            else if (moveRight)
            {
                rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
                if (isGrounded)
                    animator.Play("PlayerRunAnimation");
                spriteRenderer.flipX = false;
            }
            else
            {
                if (isGrounded)
                    animator.Play("PlayerIdleAnimation");
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
            if (jump)
            {
                if (isGrounded)
                {
                    rb.AddForce(Vector2.up * jumpSpeed);
                    animator.Play("PlayerJumpUpAnimation");
                    isGrounded = false;
                    jump = false;
                    jumpCount = 1;
                    return;
                }
                if (canDoubleJump)
                {
                    rb.AddForce(Vector2.up * jumpSpeed);
                    animator.Play("PlayerJumpUpAnimation");
                    isGrounded = false;
                    jump = false;
                    canDoubleJump = false;
                }
            }
        }
    }
