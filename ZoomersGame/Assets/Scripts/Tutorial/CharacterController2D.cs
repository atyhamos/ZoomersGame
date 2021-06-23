using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour
{
	[SerializeField] private float maxSpeed = 400f;
	[SerializeField] private float jumpForce = 400f;							// Amount of force added when the player jumps.
	[Range(0, 1)] [SerializeField] private float slideSpeed = .36f;			// Amount of maxSpeed applied to crouching movement. 1 = 100%
	[SerializeField] private Transform groundCheck;							// A position marking where to check if the player is grounded.
	[SerializeField] private Transform ceilingCheck;                            // A position marking where to check for ceilings
	[SerializeField] private Transform frontCheck;                            // A position marking where to check for front of player
	[SerializeField] private Collider2D crouchDisableCollider;              // A collider that will be disabled when crouching
	[SerializeField] private Collider2D crouchEnableCollider;              // A collider that will be enabled when crouching
	[SerializeField] private LayerMask m_WhatIsGround;
	[SerializeField] private LayerMask m_WhatIsWall;
	const float groundedRadius = .3f; // Radius of the overlap circle to determine if grounded
	public bool isGrounded;            // Whether or not the player is grounded.
	private bool canDoubleJump;
	const float ceilingRadius = .0625f; // Radius of the overlap circle to determine if the player can stand up
	private Rigidbody2D rb;
	private  bool m_FacingRight = true;  // For determining which way the player is currently facing.
	public bool isTouchingFront; // Checking if there is something in front
	private bool wallSliding;
	private bool canWallJump = true;
	[SerializeField] private float wallSlidingSpeed;
	const float frontRadius = .2f; // Radius of the overlap circle to determine if player has wall in front
	private float slideTimer = 0f;
	public UnityEvent OnLandEvent;
	public float timeZeroToMaxSpeed = 1f;
	float accelRatePerSec;
	float forwardVelocity;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	public BoolEvent OnSlideEvent;
	public bool isSliding = false;

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

		if (OnSlideEvent == null)
			OnSlideEvent = new BoolEvent();
	}

    private void Start()
    {
		accelRatePerSec = maxSpeed / timeZeroToMaxSpeed;
		forwardVelocity = 0f;
    }

    private void FixedUpdate()
	{
		bool wasGrounded = isGrounded;
		isGrounded = false;

		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundedRadius, m_WhatIsGround);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject)
			{
				isGrounded = true;
				if (!wasGrounded)
					OnLandEvent.Invoke();
			}
		}
		isTouchingFront = Physics2D.OverlapCircle(frontCheck.position, frontRadius, m_WhatIsWall);
		if (!isTouchingFront)
			canWallJump = true;
		if (isGrounded)
			canDoubleJump = true;
	}


	public void Move(float move, bool crouch, bool jump)
	{
		// SLIDING
		// If not pressing crouch, check to see if the character can stand up
		if (!crouch)
		{
			// If the character has a ceiling preventing them from standing up, keep them sliding
			if (Physics2D.OverlapCircle(ceilingCheck.position, ceilingRadius, m_WhatIsGround))
				crouch = true;
			// No ceiling. Sliding
			else if (isSliding)
			{
				// still sliding
				if (slideTimer < 0.3f)
					slideTimer += Time.deltaTime;

				// finished sliding
				else
				{
					slideTimer = 0;
					isSliding = false;
					crouchDisableCollider.enabled = true;
					crouchEnableCollider.enabled = false;
					OnSlideEvent.Invoke(false);
				}
			}
		}

		// If pressing crouch, only slide when grounded
		if (crouch && isGrounded)
		{
			// Disable one of the colliders when sliding
			if (crouchDisableCollider != null)
            {
				crouchEnableCollider.enabled = true;
				crouchDisableCollider.enabled = false;
            }
			
			// start the slide timer
			slideTimer = 0;
	
			// if not sliding
			if (!isSliding)
			{
				isSliding = true;
				OnSlideEvent.Invoke(true);
			}

			else
            {
				if (slideTimer < 0.3f)
				{
					slideTimer += Time.deltaTime;
				}
				else
				{
					if (Physics2D.OverlapCircle(ceilingCheck.position, ceilingRadius, m_WhatIsGround))
						slideTimer = 0.3f;
					else
					{
						slideTimer = 0;
						isSliding = false;
						crouchDisableCollider.enabled = true;
						crouchEnableCollider.enabled = false;
						OnSlideEvent.Invoke(false);
					}
				}
            }
		}

		// MOVEMENT
		// If the input is moving the player right and the player is facing left...
		if (move > 0 && !m_FacingRight)
		{
			// ... flip the player.
			Flip();
		}
		// Otherwise if the input is moving the player left and the player is facing right...
		else if (move < 0 && m_FacingRight)
		{
			// ... flip the player.
			Flip();
		}

		if (move != 0)
		{
			forwardVelocity += accelRatePerSec * Time.fixedDeltaTime;
			forwardVelocity = Mathf.Min(forwardVelocity, maxSpeed);
			if (!isSliding)
				rb.velocity = new Vector2(move * forwardVelocity, rb.velocity.y);
			else
				rb.velocity = new Vector2(move * forwardVelocity * slideSpeed, rb.velocity.y);
			if (isTouchingFront && !isGrounded)
				wallSliding = true;
			else
				wallSliding = false;
			if (wallSliding)
				rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
		}

		if (move == 0 && isGrounded)
        {
			forwardVelocity = 0;
			rb.velocity = new Vector2(0, rb.velocity.y);
        }

		// JUMPING
		// If the player should jump...
		if (jump && isGrounded && !isSliding)
		{
			// Add a vertical force to the player.
			isGrounded = false;
			rb.velocity = new Vector2(rb.velocity.x, jumpForce);
			canDoubleJump = true;
		}
		else if (jump && canDoubleJump && !isSliding)
		{
			rb.velocity = new Vector2(rb.velocity.x, 0.8f * jumpForce);
			canDoubleJump = false;
		}
		else if (jump && isTouchingFront && canWallJump)
        {
			rb.velocity = new Vector2(rb.velocity.x, 0.8f * jumpForce);
			canWallJump = false;
			canDoubleJump = true;
		}
	}

	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;

		Vector3 nameScale = transform.GetChild(0).transform.GetChild(0).localScale;
		nameScale.x *= -1;
		transform.GetChild(0).transform.GetChild(0).localScale = nameScale;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.tag == "Trampoline")
			rb.velocity = new Vector2(rb.velocity.x, 100f);
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
		if (collision.CompareTag("Finish"))
			GetComponent<Timer>().Finish();
    }
}
