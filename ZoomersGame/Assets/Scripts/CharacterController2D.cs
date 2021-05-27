using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour
{
	[SerializeField] private float moveForce = 400f;
	[SerializeField] private float maxSpeed = 400f;
	[SerializeField] private float jumpForce = 400f;							// Amount of force added when the player jumps.
	[Range(0, 1)] [SerializeField] private float slideSpeed = .36f;			// Amount of maxSpeed applied to crouching movement. 1 = 100%
	[SerializeField] private Transform groundCheck;							// A position marking where to check if the player is grounded.
	[SerializeField] private Transform ceilingCheck;							// A position marking where to check for ceilings
	[SerializeField] private Collider2D crouchDisableCollider;              // A collider that will be disabled when crouching
	[SerializeField] private LayerMask m_WhatIsGround;
	const float groundedRadius = .2f; // Radius of the overlap circle to determine if grounded
	public bool isGrounded;            // Whether or not the player is grounded.
	private bool canDoubleJump;
	const float ceilingRadius = .0625f; // Radius of the overlap circle to determine if the player can stand up
	private Rigidbody2D rb;
	private bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private float slideTimer = 0f;
	public UnityEvent OnLandEvent;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	public BoolEvent OnSlideEvent;
	private bool isSliding = false;

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

		if (OnSlideEvent == null)
			OnSlideEvent = new BoolEvent();
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
	}


	public void Move(float move, bool crouch, bool jump)
	{
		
		// If not pressing crouch, check to see if the character can stand up
		if (!crouch)
		{
			// If the character has a ceiling preventing them from standing up, keep them sliding
			if (Physics2D.OverlapCircle(ceilingCheck.position, ceilingRadius, m_WhatIsGround))
				crouch = true;
			else if (isSliding)
			{
				if (slideTimer < 0.8f)
					slideTimer += Time.deltaTime;
				else
				{
					slideTimer = 0;
					isSliding = false;
					crouchDisableCollider.enabled = true;
					OnSlideEvent.Invoke(false);
				}
			}
		}

		// If pressing crouch
		if (crouch)
		{
			// Disable one of the colliders when sliding
			if (isGrounded && crouchDisableCollider != null)
				crouchDisableCollider.enabled = false;
			slideTimer = 0;
			if (!isSliding)
			{
				isSliding = true;
				OnSlideEvent.Invoke(true);
			}

			else
            {
				if (slideTimer < 0.8f)
                {
					slideTimer += Time.deltaTime;
				}
				else
				{
					slideTimer = 0;
					isSliding = false;
					crouchDisableCollider.enabled = true;
					OnSlideEvent.Invoke(false);
				}
				// Reduce the speed by the slideSpeed multiplier
				move *= slideSpeed;
            }
		}
		

		if (rb.velocity.magnitude < maxSpeed)
			rb.AddForce(Vector2.right * move * moveForce);
		if (move == 0 && isGrounded)
			rb.velocity = new Vector2(0, rb.velocity.y);
		// If the input is moving the player right and the player is facing left...
		if (move > 0 && !m_FacingRight)
		{
			// ... flip the player.
			Flip();
			rb.velocity = new Vector2(0, rb.velocity.y);
		}
		// Otherwise if the input is moving the player left and the player is facing right...
		else if (move < 0 && m_FacingRight)
		{
			// ... flip the player.
			Flip();
			rb.velocity = new Vector2(0, rb.velocity.y);
		}

		// If the player should jump...
		if (jump && isGrounded)
		{
			// Add a vertical force to the player.
			isGrounded = false;
			rb.velocity = new Vector2(rb.velocity.x, 0.8f * jumpForce);
			canDoubleJump = true;
		}
		else if (jump && canDoubleJump)
		{
			rb.velocity = new Vector2(rb.velocity.x, 0.8f * jumpForce);
			canDoubleJump = false;
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

		Vector3 nameScale = transform.GetChild(2).transform.GetChild(0).localScale;
		nameScale.x *= -1;
		transform.GetChild(2).transform.GetChild(0).localScale = nameScale;
	}
}
