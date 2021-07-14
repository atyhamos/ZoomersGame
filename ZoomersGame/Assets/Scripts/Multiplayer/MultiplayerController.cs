using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Cinemachine;
using UnityEngine.EventSystems;
public class MultiplayerController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] MultiCharacterController controller;
    [SerializeField] private GameObject CherryPowerButton;
    [SerializeField] private GameObject BoxPowerButton;
    public GameObject LeftRightButtons;
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
    public bool isReady, isHost, isLoading, lostRound, hasWon = false;
    public List<RuntimeAnimatorController> skins;

    private void Awake()
    {
        Manager = GameObject.Find("MultiplayerManager").GetComponent<MultiplayerManager>();
        rb = GetComponent<Rigidbody2D>();
        view = GetComponent<PhotonView>();
        if (view.IsMine)
        {
            if (PhotonNetwork.IsMasterClient)
                BecomeHost();
            PlayerNameText.text = PhotonNetwork.NickName;
            PlayerNameText.color = Color.cyan;
        }
        else
        {
            PlayerNameText.text = view.Owner.NickName;
            PlayerNameText.color = Color.white;
        }
        Manager.AddPlayer(this);
    }

    void Start()
    {
        PlayerCamera.SetActive(false);
        if (view.IsMine)
        {
            EnableButtons();
            PlayerCamera.SetActive(true);
            if (isHost)
            {
                isReady = true;
                Manager.LoadStart();
            }
            else
            {
                Debug.Log("Setting ready to false!");
                isReady = false;
                Manager.LoadReady();
            }
        }
        currentCheckpoint = null;
        moveLeft = false;
        moveRight = false;
        crouch = false;
        wins = 0;
        checkpointsCrossed = 0;
        rank = 0;
        LeaderCamera = null;
        lostRound = false;
        isLoading = false;
        currentPowerUp = null;
        hasWon = false;
    }

    [PunRPC]
    private void BecomeHostRPC()
    {
        isHost = true;
    }

    public void BecomeHost()
    {
        view.RPC("BecomeHostRPC", RpcTarget.AllBuffered);
        if (Manager.InLobby())
            Manager.LoadStart();
    }

    public void MoveLeftDown()
    {
        if (view.IsMine)
            moveLeft = true;
    }

    public void MoveRightDown()
    {
        if (view.IsMine)
            moveRight = true;
    }

    public void MoveLeftUp()
    {
        if (view.IsMine)
            moveLeft = false;
    }

    public void MoveRightUp()
    {
        if (view.IsMine)
            moveRight = false;
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

    [PunRPC]
    private void EnterLobbyRPC()
    {
        PhotonNetwork.OpCleanRpcBuffer(view);
        Manager.leadPlayer = null;
        Manager.winnerName = null;
        Manager.isRacing = false;
        PlayerCamera.SetActive(true);
        MultiBoundsCheck.instance.UpdateSize(PlayerCamera.GetComponent<CinemachineVirtualCamera>(), 17);
        Start();
    }
    public void EnterLobby()
    {
        view.RPC("EnterLobbyRPC", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void UpdateRulesRPC(int winsNeeded, bool isJustJoined)
    {
        Manager.UpdateRules(winsNeeded, isJustJoined);
    }

    public void UpdateRules(int winsNeeded, bool isJustJoined)
    {
        view.RPC("UpdateRulesRPC", RpcTarget.AllBuffered, winsNeeded, isJustJoined);
    }

    [PunRPC]
    private void UnreadyRPC()
    {
        isReady = false;
        Manager.readyButton.image.color = Color.grey;
    }
    public void Unready()
    {
        view.RPC("UnreadyRPC", RpcTarget.AllBuffered);
    }

    public void DisableButtons()
    {
        if (view.IsMine)
        {
            var pointers = LeftRightButtons.GetComponentsInChildren<IPointerDownHandler>();
            if (pointers != null)
            {
                foreach (var p in pointers)
                {
                    var mb = p as MonoBehaviour;
                    if (mb != null)
                        mb.enabled = false;
                }
            }
            PlayerButtons.SetActive(false);
            PlaceholderButtons.SetActive(true);
        }
    }
    public void EnableButtons()
    {
        if (view.IsMine)
        {
            var pointers = LeftRightButtons.GetComponentsInChildren<IPointerDownHandler>(true);
            if (pointers != null)
            {
                foreach (var p in pointers)
                {
                    var mb = p as MonoBehaviour;
                    if (mb != null)
                        mb.enabled = true;
                }
            }
            PlayerButtons.SetActive(true);
            PlaceholderButtons.SetActive(false);
        }
    }

    public void HideAllButtons()
    {
        if (view.IsMine)
        {
            var pointers = LeftRightButtons.GetComponentsInChildren<IPointerDownHandler>();
            if (pointers != null)
            {
                foreach (var p in pointers)
                {
                    var mb = p as MonoBehaviour;
                    if (mb != null)
                        mb.enabled = false;
                }
            }
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
        rb.velocity = new Vector2(0, -1000);
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
        Debug.Log("Finished Loading!!");
        isLoading = false;
    }
    public void ReadyButton()
    {
        view.RPC("Ready", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void StartRound()
    {
        if (!isHost)
            isReady = false;
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
        if (hasWon)
            return;
        else
        {
            wins++;
            Debug.Log("You won! You now have " + wins + " wins");
            hasWon = true;
            Manager.UpdatePlayerScores(); // this is where checking if the match ends
            if (view.IsMine && wins < Manager.winsNeeded)
                Manager.StartCoroutine("ShowWin");
        }
    }

    public void WinRound()
    {
        StopMoving();
        DisableButtons();
        view.RPC("Win", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void Respawn(int racerId)
    {
        rank = 1;
        checkpointsCrossed = 0;
        Manager.leadPlayer = Manager.racersArray[racerId];
        this.currentCheckpoint = Manager.leadPlayer.currentCheckpoint;
        Manager.ResetRound();
        StopMoving();
        lostRound = false;
        hasWon = false;
    }

    [PunRPC]
    private void EndGame(string winnerName)
    {
        Manager.winnerName = winnerName;
        Manager.StartCoroutine("ShowWinner");
        isLoading = true;
    }

    public void ShowWinner(string winnerName)
    {
        view.RPC("EndGame", RpcTarget.AllBuffered, winnerName);
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

    [PunRPC]
    private void GetMasterDataRPC()
    {
        Manager.UpdatePlayerScores();
        Manager.JoinNonHostMessage();
        UpdateRules(Manager.winsNeeded, true);
    }

    

    public void GetMasterData()
    {
        Debug.Log("Getting data from Master Client");
        view.RPC("GetMasterDataRPC", RpcTarget.MasterClient);
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
                        Lose();
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
            if (!isHost && PhotonNetwork.IsMasterClient)
            {
                Debug.Log("Becoming host...");
                BecomeHost();
            }
        }
    }

    [PunRPC]
    private void LoseRPC()
    {
        lostRound = true;
    }

    public void Lose()
    {
        view.RPC("LoseRPC", RpcTarget.AllBuffered);
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

    [PunRPC]
    private void StartRaceRPC()
    {
        Manager.isRacing = false;
    }

    public void StartRace()
    {
        view.RPC("StartRaceRPC", RpcTarget.AllBuffered);
    }

    public bool InLobby()
    {
        return Manager.inLobby;
    }


    public void CrossCheckpoint(Checkpoint checkpoint, bool correct)
    {
        Debug.Log("Updating next checkpoint...");
        currentCheckpoint = checkpoint;
        if (correct)
            checkpointsCrossed++; 
        else
            checkpointsCrossed--;
        view.RPC("RankRacers", RpcTarget.AllBuffered);
    }

    public void UpdateLeadPlayer()
    {
        Manager.leadPlayer = Manager.racersArray[Manager.racersArray.Count - 1];
    }

    [PunRPC]
    private void RankRacers()
    {
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

    [PunRPC]
    private void ChangeSkinRPC(int skinIndex)
    {
        GetComponent<Animator>().runtimeAnimatorController = skins[skinIndex];
    }
    public void ChangeSkin(int skinIndex)
    {
        view.RPC("ChangeSkinRPC", RpcTarget.AllBuffered, skinIndex);
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
