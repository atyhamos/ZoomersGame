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
    public GameObject PlayerCamera;
    public GameObject PlayerButtons;
    public Text PlayerNameText;

    private bool moveLeft, moveRight, jump, crouch;
    private int horizontalMove;

    void Start()
    {
        PlayerNameText.text = PhotonNetwork.NickName;
        rb = GetComponent<Rigidbody2D>();
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
    }
    public void Crouch()
    {
        if (moveLeft || moveRight)
            crouch = true;
    }
    public void Home()
    {
        GameManager.instance.ChangeScene(2);
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
