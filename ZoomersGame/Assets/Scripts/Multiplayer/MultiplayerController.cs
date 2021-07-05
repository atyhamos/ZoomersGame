using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Cinemachine;
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
    public GameObject PlayerButtons, PlaceholderButtons;
    public Text PlayerNameText;
    public SpriteRenderer Sprite;
    public bool usingPowerUp;
    public MultiPowerUp previousPowerUp, currentPowerUp;
    public bool moveLeft, moveRight, jump, crouch;
    private int horizontalMove;
    public int checkpointsCrossed = 0;
    public Checkpoint currentCheckpoint;
    public int rank = 0, wins = 0;
    public MultiplayerManager Manager;
    public bool isReady, isHost, isLoading, lostRound;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
        if (view.IsMine)
        {
            isHost = PhotonNetwork.IsMasterClient;
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
        currentCheckpoint = null;
        moveLeft = false;
        moveRight = false;
        crouch = false;
        if (view.IsMine)
        {
            if (isHost)
            {
                isReady = true;
                Manager.LoadStart();
            }
            else
            {
                isReady = false;
                Manager.LoadReady();
            }
        }
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

    public void DisableButtons()
    {
        if (view.IsMine)
        {
            PlayerButtons.SetActive(false);
            PlaceholderButtons.SetActive(true);
        }
    }
    public void EnableButtons()
    {
        if (view.IsMine)
        {
            PlayerButtons.SetActive(true);
            PlaceholderButtons.SetActive(false);
        }
    }

    public void HideAllButtons()
    {
        if (view.IsMine)
        {
            PlayerButtons.SetActive(false);
            PlaceholderButtons.SetActive(false);
        }
    }


    public void ConsumePower()
    {
        if (HasPowerUp())
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
        rb.velocity = Vector2.zero;
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

    [PunRPC]
    private void Ready()
    {
        isReady = !isReady;
    }

    [PunRPC]
    private void Loading()
    {
        isLoading = true;
    }

    public void Load()
    {
        view.RPC("Loading", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void FinishLoading()
    {
        isLoading = false;
    }
    public void ReadyButton()
    {
        view.RPC("Ready", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void StartRound()
    {
        Manager.StartCoroutine("StartTask");
        view.RPC("FlipFalse", RpcTarget.AllBuffered);
    }

    public void StartButton()
    {
        view.RPC("StartRound", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void Win()
    {
        wins++;
        Debug.Log("You now have " + wins + " wins");
        Manager.StartCoroutine("Win");
    }

    public void WinRound()
    {
        DisableButtons();
        StopMoving();
        view.RPC("Win", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void Respawn(int racerId)
    {
        isLoading = true;
        rank = 1;
        checkpointsCrossed = 0;
        Manager.leadPlayer = Manager.racersArray[racerId];
        this.currentCheckpoint = Manager.leadPlayer.currentCheckpoint;
        Manager.ResetRound();
        StopMoving();
        lostRound = false;
        isLoading = false;
    }

    public bool HasPowerUp()
    {
        return currentPowerUp != null;
    }

    public void ResetRound(int racerId)
    {

        if (Manager.racersArray[racerId].currentCheckpoint.flipSprite)
            view.RPC("FlipTrue", RpcTarget.AllBuffered);
        else
            view.RPC("FlipFalse", RpcTarget.AllBuffered);

        view.RPC("Respawn", RpcTarget.AllBuffered, racerId);
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
            if (HasPowerUp())
                if (currentPowerUp.GetType().ToString() == "MultiCherryPowerUp")
                    CherryPowerButton.SetActive(true);
                else
                    BoxPowerButton.SetActive(true);
            else if (!HasPowerUp())
            {
                CherryPowerButton.SetActive(false);
                BoxPowerButton.SetActive(false);
            }
            if (Manager.isRacing)
            {
                if (!lostRound && rank > 1) // Not leader and still in race
                {
                    view.RPC("OtherCameras", RpcTarget.AllBuffered);
                    if (MultiBoundsCheck.instance.WithinBounds(transform))
                    {
                        Debug.Log("Within bounds!");
                    }
                    else
                    {
                        Debug.Log("Not in bounds");
                        lostRound = true;
                        StopMoving();
                        HideAllButtons();
                        Manager.StartCoroutine("Lose");
                    }
                }
                if (rank == 1) // Leader
                {
                    view.RPC("LeadCamera", RpcTarget.AllBuffered);
                }
            }
        }
    }

    [PunRPC]
    private void OtherCameras()
    {
        LeaderCamera.SetActive(true);
        MultiBoundsCheck.instance.UpdateBounds();
        MultiBoundsCheck.instance.UpdateSize(LeaderCamera.GetComponent<CinemachineVirtualCamera>(), Manager.leadPlayer.currentCheckpoint.orthographicSize);
        PlayerCamera.SetActive(false);
    }

    [PunRPC]
    private void LeadCamera()
    {
        PlayerCamera.SetActive(true);
        MultiBoundsCheck.instance.UpdateBounds();
        MultiBoundsCheck.instance.UpdateSize(PlayerCamera.GetComponent<CinemachineVirtualCamera>(), currentCheckpoint.orthographicSize);
    }

    public void CrossCheckpoint(Checkpoint checkpoint)
    {
        Debug.Log("Updating next checkpoint...");
        currentCheckpoint = checkpoint;
        view.RPC("RankRacers", RpcTarget.AllBuffered);
    }

    public void UpdateLeadPlayer()
    {
        Manager.leadPlayer = Manager.racersArray[Manager.racersArray.Count - 1];
    }

    [PunRPC]
    private void RankRacers()
    {
        checkpointsCrossed++;
        Manager.RankRacers();
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
            controller.Move(horizontalMove, crouch, jump, view);
            jump = false;
            crouch = false;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("LoadingArea"))
        {
            view.RPC("FinishLoading", RpcTarget.AllBuffered);
        }
    }
}
