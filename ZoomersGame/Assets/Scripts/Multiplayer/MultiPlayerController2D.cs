using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
public class MultiPlayerController2D : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] MultiCharacterController2D controller;
    Rigidbody2D rb;
    PhotonView view;
    public GameObject PlayerCamera;
    public GameObject PlayerButtons;
    public Text PlayerNameText;
    public SpriteRenderer sr;

    private bool moveLeft, moveRight, jump, crouch;
    private int horizontalMove;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
        if (view.IsMine)
        {
            PlayerButtons.SetActive(true);
            PlayerCamera.SetActive(true);
            PlayerNameText.text = PhotonNetwork.NickName;
            PlayerNameText.color = Color.white;
        }
        else
        {
            PlayerNameText.text = view.Owner.NickName;
            PlayerNameText.color = Color.white;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        moveLeft = false;
        moveRight = false;
        crouch = false;
    }

    public void MoveLeft()
    {
        if (view.IsMine)
            moveLeft = !moveLeft;
    }

    public void MoveRight()
    {
        if (view.IsMine)
            moveRight = !moveRight;
    }
    public void Jump()
    {
        if (view.IsMine)
            jump = true;
    }
    public void Crouch()
    {
        if (view.IsMine)
            if (moveLeft || moveRight)
                {
                    crouch = true;
                }
    }

    [PunRPC]
    private void FlipTrue()
    {
        sr.flipX = true;
    }

    [PunRPC]
    private void FlipFalse()
    {
        sr.flipX = false;
    }


    private void Update()
    {
        if (view.IsMine)
        {
            moveLeft = moveLeft || Input.GetButtonDown("Left");
            if (moveLeft)
            {
                horizontalMove = -1;
                view.RPC("FlipTrue", RpcTarget.AllBuffered);
            }
            if (Input.GetButtonUp("Left"))
                moveLeft = false;

            moveRight = moveRight || Input.GetButtonDown("Right");
            if (moveRight)
            {
                horizontalMove = 1;
                view.RPC("FlipFalse", RpcTarget.AllBuffered);
            }
            if (Input.GetButtonUp("Right"))
                moveRight = false;

            if (!moveLeft && !moveRight)
                horizontalMove = 0;

            crouch = crouch || Input.GetButtonDown("Crouch") || Input.GetButtonUp("Crouch");

            animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

            jump = jump || Input.GetButtonDown("Jump");
            if (controller.isGrounded)
            {
                animator.SetBool("IsJumping", false);
                animator.SetBool("IsFalling", false);
            }
            else if (rb.velocity.y < -0.01)
            {
                animator.SetBool("IsJumping", false);
                animator.SetBool("IsFalling", true);
            }
            if (jump)
            {
                animator.SetBool("IsJumping", true);
                animator.SetBool("IsFalling", false);
            }
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
        if (view.IsMine)
        {
            controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump);
            jump = false;
            crouch = false;
        }
    }
}
