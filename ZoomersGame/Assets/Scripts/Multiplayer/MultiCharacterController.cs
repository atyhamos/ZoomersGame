using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public class MultiCharacterController : MonoBehaviour
{
	public float maxSpeed = 400f;
	[SerializeField] private float jumpForce = 400f;                            // Amount of force added when the player jumps.
	[Range(0, 1)] [SerializeField] private float slideSpeed = .36f;         // Amount of maxSpeed applied to crouching movement. 1 = 100%
	[SerializeField] private Transform groundCheck;                         // A position marking where to check if the player is grounded.
	[SerializeField] private Transform ceilingCheck;                            // A position marking where to check for ceilings
	[SerializeField] private Transform frontCheck;                            // A position marking where to check for front of 
	[SerializeField] private Collider2D standingCollider;              // A collider that will be disabled when crouching
	[SerializeField] private Collider2D crouchCollider;              // A collider that will be enabled when crouching
	[SerializeField] private LayerMask m_WhatIsGround;
	[SerializeField] private LayerMask m_WhatIsWall;
	[SerializeField] private Animator anim;
	const float groundedRadius = .3f; // Radius of the overlap circle to determine if grounded
	public bool isGrounded;            // Whether or not the player is grounded.
	public bool canDoubleJump;
	const float ceilingRadius = .0625f; // Radius of the overlap circle to determine if the player can stand up
	public Rigidbody2D rb;
	private bool m_FacingRight = true;  // For determining which way the player is currently facing.
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
	public Transform trapPlacement;

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

	[PunRPC]
	private void DisableCollider()
	{
		standingCollider.enabled = false;
		crouchCollider.enabled = true;
	}

	[PunRPC]
	private void EnableCollider()
    {
		standingCollider.enabled = true;
		crouchCollider.enabled = false;
	}
	public void Move(float move, bool crouch, bool jump, PhotonView view)
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
				view.RPC("DisableCollider", RpcTarget.AllBuffered);
				// still sliding
				if (slideTimer < 0.3f)
					slideTimer += Time.deltaTime;

				// finished sliding
				else
				{
					view.RPC("EnableCollider", RpcTarget.AllBuffered);
					slideTimer = 0;
					isSliding = false;
					standingCollider.enabled = true;
					crouchCollider.enabled = false;
					OnSlideEvent.Invoke(false);
				}
			}
		}

		// If pressing crouch, only slide when grounded
		if (crouch && isGrounded)
		{
			// Disable one of the colliders when sliding
			if (standingCollider != null)
			{
				view.RPC("DisableCollider", RpcTarget.AllBuffered);
				crouchCollider.enabled = true;
				standingCollider.enabled = false;
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
						view.RPC("EnableCollider", RpcTarget.AllBuffered);
						slideTimer = 0;
						isSliding = false;
						standingCollider.enabled = true;
						crouchCollider.enabled = false;
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
			// rb.velocity = new Vector2(0, rb.velocity.y);
		}
		// Otherwise if the input is moving the player left and the player is facing right...
		else if (move < 0 && m_FacingRight)
		{
			// ... flip the player.
			Flip();
			// rb.velocity = new Vector2(0, rb.velocity.y);
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
			AudioManager.instance.Play("Jump");
			anim.SetBool("IsJumping", true);
			anim.SetBool("IsFalling", false);
			isGrounded = false;
			rb.velocity = new Vector2(rb.velocity.x, jumpForce);
			canDoubleJump = true;
		}
		else if (jump && canDoubleJump && !isSliding)
		{
			AudioManager.instance.Play("Jump");
			anim.SetBool("IsJumping", true);
			anim.SetBool("IsFalling", false);
			rb.velocity = new Vector2(rb.velocity.x, 0.8f * jumpForce);
			canDoubleJump = false;
		}
		else if (jump && isTouchingFront && canWallJump)
		{
			AudioManager.instance.Play("Jump");
			anim.SetBool("IsJumping", true);
			anim.SetBool("IsFalling", false);
			rb.velocity = new Vector2(rb.velocity.x, 0.8f * jumpForce);
			canWallJump = false;
			canDoubleJump = true;
		}
	}

	private void Flip()
    {
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;
		
		// Flip the Front check position
		Vector3 frontCheckPosition = frontCheck.localPosition;
		frontCheckPosition.x *= -1;
		frontCheck.localPosition = frontCheckPosition;

		Vector3 trapCheckPlacement = trapPlacement.localPosition;
		trapCheckPlacement.x *= -1;
		trapPlacement.localPosition = trapCheckPlacement;
	}
	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.tag == "Trampoline")
        {
			anim.SetBool("IsJumping", true);
			anim.SetBool("IsFalling", false);
			rb.velocity = new Vector2(rb.velocity.x, 100f);
        }
		if (collision.gameObject.tag == "PowerUp")
		{
			Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
			Debug.Log($"Mass: {rb.mass}\nLinear Drag: {rb.drag}\nAngular Drag: {rb.angularDrag}\nGravity: {rb.gravityScale}");
		}

	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		MultiplayerController player = GetComponent<MultiplayerController>();
		if (collision.CompareTag("Finish"))
		{
			player.StopMoving();
			GetComponent<Timer>().Finish();
		}

		if (collision.CompareTag("PowerUp"))
		{
			if (!player.HasPowerUp())
			{
				MultiPowerUp power = collision.gameObject.GetComponent<MultiPowerUp>();
				AudioManager.instance.Play("PowerUp");
				power.Pickup(this, player);
				player.currentPowerUp = power;
			}
		}

		if (collision.CompareTag("Random"))
		{
			if (!player.HasPowerUp())
			{
				MultiPowerUp power = collision.GetComponent<MultiRandomPowerUp>().GetRandomPower();
				AudioManager.instance.Play("PowerUp");
				power.Pickup(this, player);
				player.currentPowerUp = power;
			}
		}

		if (collision.CompareTag("Checkpoint"))
        {
			if (player.currentCheckpoint == null
				&& collision.GetComponent<Checkpoint>() == GameObject.Find("Checkpoint 0").GetComponent<Checkpoint>())
            {
				Debug.Log("Crossed first checkpoint!");
				player.currentCheckpoint = collision.GetComponent<Checkpoint>();
				player.CrossCheckpoint(player.currentCheckpoint, true);
				return;
			}
			else if (player.currentCheckpoint.Next() == collision.GetComponent<Checkpoint>()) // Correct checkpoint
            {
				Debug.Log("Crossed correct checkpoint!");
				player.currentCheckpoint = player.currentCheckpoint.Next();
				player.CrossCheckpoint(player.currentCheckpoint, true);
				return;
            }
			else if (player.currentCheckpoint.Previous() == collision.GetComponent<Checkpoint>()) // Backward checkpoint
			{
				Debug.Log("You're going backwards! (Crossed a previous checkpoint)");
				player.currentCheckpoint = player.currentCheckpoint.Previous();
				player.CrossCheckpoint(player.currentCheckpoint, false);
				return;
			}
		}
	}
}
