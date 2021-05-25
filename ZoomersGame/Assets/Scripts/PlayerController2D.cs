using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] CharacterController2D controller;
    Rigidbody2D rb;
    private bool moveLeft, moveRight, jump, crouch;
    private int horizontalMove;

    void Start()
    {
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
        {
            crouch = true;
        }
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
        if (jump)
            animator.SetBool("IsJumping", true);
        if (rb.velocity.y < -0.01)
            animator.SetBool("IsFalling", true);
        else
            animator.SetBool("IsFalling", false);
    }

    public void OnLanding()
    {
        animator.SetBool("IsJumping", false);
        animator.Play("PlayerIdleAnimation");
    }

    public void OnSlide(bool isSliding)
    {
        animator.SetBool("IsSliding", isSliding);
    }

    private void FixedUpdate()
    {
        controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump);
        jump = false;
        crouch = false;
    }
}
