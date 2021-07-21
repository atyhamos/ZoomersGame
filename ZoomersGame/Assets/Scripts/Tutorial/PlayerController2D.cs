using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerController2D : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] CharacterController2D controller;
    Rigidbody2D rb;
    public GameObject PlayerCamera, PlayerButtons, nameBackground, wings;
    public Text PlayerNameText;
    public bool hasPowerUp, usingPowerUp;
    public PowerUp previousPowerUp, currentPowerUp;
    public bool moveLeft, moveRight, jump, crouch;
    private int horizontalMove;
    public bool jumpButtonDown;

    void Start()
    {
        PlayerNameText.text = PhotonNetwork.NickName;
        rb = GetComponent<Rigidbody2D>();
        nameBackground.transform.localScale = new Vector3(PlayerNameText.text.Length / 1.5f, 2.5f, 0f);
        moveLeft = false;
        moveRight = false;
        crouch = false;
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
        jumpButtonDown = true;
    }

    public void Float()
    {
        jumpButtonDown = false;
    }
    public void Crouch()
    {
        if (moveLeft || moveRight)
            crouch = true;
    }

    public void StopMoving()
    {
        moveLeft = false;
        moveRight = false;
        jump = false;
        crouch = false;
    }
    public void ConsumePower()
    {
        if (hasPowerUp)
        {
            if (usingPowerUp)
            {
                Debug.Log("Cancelling previous power... Powers do not stack!");
                previousPowerUp.Cancel();
            }
            currentPowerUp.Consume();
            previousPowerUp = currentPowerUp;
            currentPowerUp = null;
        }
    }
    public void Home()
    {
        AudioManager.instance.ButtonPress();
        AudioManager.instance.Main();
        GameManager.instance.ChangeScene(3);
    }

   

    private void Update()
    {
        moveLeft = moveLeft || Input.GetButtonDown("Left");
        if (moveLeft)
            horizontalMove = -1;    
        
        if (Input.GetButtonUp("Left"))
            moveLeft = false;

        moveRight = moveRight || Input.GetButtonDown("Right");
        if (moveRight)
            horizontalMove = 1;
    
        if (Input.GetButtonUp("Right"))
            moveRight = false;

        if (!moveLeft && !moveRight)
            horizontalMove = 0;

        crouch = crouch || Input.GetButtonDown("Crouch") || Input.GetButtonUp("Crouch");

        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        jump = jump || Input.GetButtonDown("Jump");

        // debugging weird jumping animation while grounded
        if (controller.isGrounded)
        {
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", false);
        }
        else if (!controller.isGrounded && rb.velocity.y < -0.01)
        {
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", true);
        }
        // cannot jump while sliding
        if (jump && !controller.isSliding)
        {
            animator.SetBool("IsJumping", true);
            animator.SetBool("IsFalling", false);
        }

        if (Input.GetButtonDown("PowerUp"))
            ConsumePower();

    }

    public void OnLanding()
    {
        animator.SetBool("IsFalling", false);
    }

    public void OnSlide(bool isSliding)
    {
        animator.SetBool("IsSliding", isSliding);
    }

    private void FixedUpdate()
    {
        controller.Move(horizontalMove, crouch, jump);
        jump = false;
        crouch = false;
    }
}
