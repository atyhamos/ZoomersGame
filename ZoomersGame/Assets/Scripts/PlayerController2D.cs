using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    private bool moveLeft, moveRight, jump, isGrounded, isLeftWalled, isRightWalled;
    private int extraJumpCount = 0;
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

    private void Animate()
    {
        if (rb.velocity == Vector2.zero || !moveLeft && !moveRight)
            animator.Play("PlayerIdleAnimation");
        if (moveLeft && !isLeftWalled)
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
                if (rb.velocity.x > 0)
                    rb.velocity = new Vector2(0, rb.velocity.y);
                if (rb.velocity.x > -maxSpeed)
                    rb.AddForce(Vector2.left * moveSpeed * Time.deltaTime);
            }
            else if (moveRight && !isRightWalled)
            {
                if (rb.velocity.x < 0)
                    rb.velocity = new Vector2(0, rb.velocity.y);
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
