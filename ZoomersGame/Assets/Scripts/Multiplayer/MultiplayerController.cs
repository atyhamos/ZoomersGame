using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
public class MultiplayerController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] MultiCharacterController controller;
    private Rigidbody2D rb;
    private PhotonView view;
    public GameObject PlayerCamera;
    public GameObject PlayerButtons;
    public Text PlayerNameText;
    public SpriteRenderer Sprite;

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
            PlayerNameText.color = Color.cyan;
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
                crouch = true;
    }
    public void Home()
    {
        GameManager.instance.ChangeScene(2);
        PhotonNetwork.Destroy(view);
        PhotonNetwork.LeaveRoom();
    }

    [PunRPC]
    private void FlipTrue()
    {
        Sprite.flipX = true;
    }

    [PunRPC]
    private void FlipFalse()
    {
        Sprite.flipX = false;
    }

    private void Update()
    {
        if (view.IsMine)
        {
        //    // debugging weird jumping animation while grounded
        //    if (controller.isGrounded)
        //    {
        //        animator.SetBool("IsJumping", false);
        //        animator.SetBool("IsFalling", false);
        //    }
            
            moveLeft = moveLeft || Input.GetButtonDown("Left");
            if (moveLeft)
            {
                view.RPC("FlipTrue", RpcTarget.AllBuffered);
                horizontalMove = -1;
            }

            if (Input.GetButtonUp("Left"))
                moveLeft = false;

            moveRight = moveRight || Input.GetButtonDown("Right");
            if (moveRight)
            {
                view.RPC("FlipFalse", RpcTarget.AllBuffered);
                horizontalMove = 1;
            }

            if (Input.GetButtonUp("Right"))
                moveRight = false;

            if (!moveLeft && !moveRight)
                horizontalMove = 0;

            crouch = crouch || Input.GetButtonDown("Crouch") || Input.GetButtonUp("Crouch");

            animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

            jump = jump || Input.GetButtonDown("Jump");

            // cannot jump while sliding
        //    if (jump && !controller.isSliding)
        //    {
        //        animator.SetBool("IsJumping", true);
        //        animator.SetBool("IsFalling", false);
        //    }
            if (!controller.isGrounded && rb.velocity.y < -0.01)
            {
                animator.SetBool("IsJumping", false);
                animator.SetBool("IsFalling", true);
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
            controller.Move(horizontalMove, crouch, jump);
            jump = false;
            crouch = false;
        }
    }
}
