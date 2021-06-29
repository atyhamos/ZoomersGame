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
    [SerializeField] private GameObject CherryPowerButton;
    [SerializeField] private GameObject BoxPowerButton;
    private Rigidbody2D rb;
    private PhotonView view;
    public GameObject PlayerCamera;
    public GameObject LeaderCamera;
    public GameObject PlayerButtons;
    public Text PlayerNameText;
    public SpriteRenderer Sprite;
    public bool hasPowerUp, usingPowerUp;
    public MultiPowerUp previousPowerUp, currentPowerUp;
    private bool moveLeft, moveRight, jump, crouch;
    private int horizontalMove;
    public int checkpointsCrossed = 0;
    public Transform nextCheckpoint;
    public int rank = 0;
    private MultiplayerManager Manager;

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
        Manager = GameObject.Find("MultiplayerManager").GetComponent<MultiplayerManager>();
        Manager.AddPlayer(this);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        nextCheckpoint = GameObject.Find("Checkpoint 1").transform;
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
    public void StopMoving()
    {
        moveLeft = false;
        moveRight = false;
        jump = false;
        crouch = false;
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
            if (hasPowerUp)
                if (currentPowerUp.GetType().ToString() == "MultiCherryPowerUp")
                    CherryPowerButton.SetActive(true);
                else
                    BoxPowerButton.SetActive(true);
            else if (!hasPowerUp)
            {
                CherryPowerButton.SetActive(false);
                BoxPowerButton.SetActive(false);
            }
            if (rank > 1)
            {
                LeaderCamera.SetActive(true);
                PlayerCamera.SetActive(false);
            }
            if (rank == 1)
                PlayerCamera.SetActive(true);
        }
    }

    public void UpdateCheckpoint(Transform checkpoint)
    {
        Debug.Log("Updating next checkpoint...");
        nextCheckpoint = checkpoint;
        checkpointsCrossed++;
        MultiplayerManager.instance.RankRacers();
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
