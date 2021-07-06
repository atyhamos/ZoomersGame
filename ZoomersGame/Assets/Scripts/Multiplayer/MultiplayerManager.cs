using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class MultiplayerManager : MonoBehaviourPunCallbacks
{
    public float minX, maxX, minY, maxY;
    [SerializeField] private Text PingText;
    [SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private GameObject rejoinUI, loseUI, startUI, readyUI, winUI;
    [SerializeField] private Transform spawnLocation;
    [SerializeField] private Text playerCount, players;
    [SerializeField] private GameObject loadingUI;
    [SerializeField] private GameObject playerNumUI;
    [SerializeField] private Text countdownText;
    [SerializeField] private GameObject Checkpoints;
    public Button startButton;
    public Button readyButton;
    private Player[] playerList;
    private Vector2 randomPosition, respawnLocation;
    private string roomCode;
    private MultiplayerController player;
    private int pingUpdate = 1; // 1 second
    public List<MultiplayerController> racersArray, racersScores;
    public static MultiplayerManager instance;
    public bool isRacing;
    private float countdownUntil = 3f;
    private bool inLobby, allPrepared = false;
    public MultiplayerController leadPlayer;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        SpawnPlayer();
        roomCode = PhotonNetwork.CurrentRoom.Name;
        GameManager.instance.rejoinCode = "";
        startButton.image.color = Color.green;
        readyButton.image.color = Color.grey;
        inLobby = true;
        isRacing = false;
    }
    public void SpawnPlayer()
    {
        randomPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        player = PhotonNetwork.Instantiate(PlayerPrefab.name, spawnLocation.position, Quaternion.identity).gameObject.GetComponent<MultiplayerController>();
    }

    private void Update()
    {
        if (PhotonNetwork.NetworkingClient == null || player == null)
        {
            rejoinUI.SetActive(true);
            return;
        }
        if (Time.time >= pingUpdate)
        {
            pingUpdate = Mathf.FloorToInt(Time.time) + 1;
            Ping();
        }
        if (inLobby)
        {
            if (AllReady())
                startButton.interactable = true;
            else
                startButton.interactable = false;
        }
        else
        {
            // at racing map but race hasnt started
            if (!isRacing && AllLoaded())
            {
                player.DisableButtons();
                countdownText.gameObject.SetActive(true);
                countdownUntil -= 1 * Time.deltaTime;
                countdownText.text = "Race is starting in " + countdownUntil.ToString("0");
                if (countdownUntil <= 0.5)
                    StartCoroutine(GoTask());
            }
        }
        //Debug.Log(AllLoaded());


    }

    private IEnumerator GoTask()
    {
        isRacing = true;
        Checkpoints.SetActive(true);
        foreach (MultiplayerController player in racersArray)
        {
            player.isLoading = false;
            player.EnableButtons();
        }
        countdownText.fontSize = 50;
        countdownText.text = "GO!";
        yield return new WaitForSeconds(2f);
        countdownText.CrossFadeAlpha(0, 1, false);
        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
        countdownUntil = 3f; // reset
        countdownText.fontSize = 25; // reset
        countdownText.text = "Race is starting in " + countdownUntil.ToString("0"); // reset
    }

    public void ReadyButton()
    {
        if (player.isReady)
            readyButton.image.color = Color.grey;
        else
            readyButton.image.color = Color.green;
        player.ReadyButton();
    }

    private bool AllLoaded()
    {
        foreach (MultiplayerController player in racersArray)
        {
            if (player.isLoading)
                return false;
        }
        return true;
    }
  
    public IEnumerator StartTask()
    {
        player.Load();
        player.transform.position = randomPosition;
        player.StopMoving();
        player.HideAllButtons();
        Checkpoints.SetActive(false); // to prevent accidental crossing of checkpoints
        startUI.SetActive(false);
        readyUI.SetActive(false);
        playerNumUI.SetActive(false);
        loadingUI.SetActive(true);
        while (player.isLoading)
            yield return null;
        loadingUI.SetActive(false);
        inLobby = false;
    }
    public void StartButton()
    {
        foreach (MultiplayerController player in racersArray)
        {
            player.StartButton(); //RPC call
        }
    }

    public bool AllReady()
    {
        foreach (MultiplayerController player in racersArray)
        {
            if (player.isReady)
                continue;
            else
                return false;
        }
        //Debug.Log("all ready!");
        return true;
    }

    public void LoadStart()
    {
        startUI.SetActive(true);
    }

    public void LoadReady()
    {
        readyUI.SetActive(true);
    }

    public void Home()
    {
        SceneManager.LoadScene("Loading");
        CreateAndJoinRoom.instance.LeaveRoom();
    }

    public IEnumerator Lose()
    {
        Debug.Log("lose");
        loseUI.SetActive(true);
        yield return new WaitForSeconds(0.5f); // need time to update the loss
        int racerId = LastRacerLeft();
        if (racerId != -1)
        {
            // 1 racer 
            leadPlayer = racersArray[racerId];
            leadPlayer.WinRound();
            yield return new WaitForSeconds(0.5f);
            foreach (MultiplayerController player in racersArray)
            {
                player.ResetRound(racerId);
            }
        }
    }



    public IEnumerator ShowWin()
    {
        winUI.SetActive(true);
        yield return new WaitForSeconds(1f);
        winUI.SetActive(false);
    }

    public void ResetRound()
    {
        respawnLocation = new Vector2(Random.Range(player.currentCheckpoint.transform.position.x - 1, player.currentCheckpoint.transform.position.x + 1),
                player.currentCheckpoint.transform.position.y + 2);
        Checkpoints.SetActive(false);
        player.transform.position = respawnLocation;
        loseUI.SetActive(false);
        isRacing = false;
        Checkpoints.SetActive(true);
    }
    
    public void RejoinButton()
    {
        rejoinUI.SetActive(false);
        GameManager.instance.rejoinCode = roomCode;
        SceneManager.LoadSceneAsync("Loading");
        CreateAndJoinRoom.instance.LeaveRoom();
    }

    public void RankRacers()
    {
        racersArray.Sort((p1, p2) => p1.checkpointsCrossed.CompareTo(p2.checkpointsCrossed));
        Debug.Log(racersArray.Count);
        GameObject leaderCamera = racersArray[racersArray.Count - 1].PlayerCamera;
        MultiplayerController leader = racersArray[racersArray.Count - 1];
        for (int i = 0; i < racersArray.Count; i++)
        {
            racersArray[i].rank = racersArray.Count - i;
            racersArray[i].UpdateLeadPlayer();
            racersArray[i].LeaderCamera = leaderCamera;
            Debug.Log("Updated rank to " + i);
        }
        Debug.Log("Ranked racers!");
    }

    public int LastRacerLeft()
    {
        int roundLosses = racersArray.Count - 1;
        int playerId = -1;
        for (int i = 0; i < racersArray.Count; i++)
        {
            if (racersArray[i].lostRound)
                roundLosses--;
            else
                playerId = i;
        }
        return roundLosses == 0 ? playerId : -1;
    }

    private void Ping()
    {
        PingText.text = "Ping: " + PhotonNetwork.GetPing();
    }

    public void AddPlayer(MultiplayerController player)
    {
        racersArray.Add(player);
        racersScores.Add(player);
        playerList = PhotonNetwork.PlayerList;
        Debug.Log("Added player");
        Debug.Log(PlayerCount());
        playerCount.text = $"Number of players: {PlayerCount()}";
        players.text += $"{player.PlayerNameText.text} ({player.wins})\n";
    }

    public int PlayerCount()
    {
        if (playerList.Length != racersArray.Count)
            UpdateRacers();
        return playerList.Length;
    }

    public void UpdateRacers()
    {
        List<MultiplayerController> tempRacersArray = new List<MultiplayerController>();
        racersScores = new List<MultiplayerController>();
        foreach (MultiplayerController item in racersArray)
        {
            if (item == null)
                continue;
            else
            {
                tempRacersArray.Add(item);
                racersScores.Add(item);
            }    
        }
        racersArray = tempRacersArray;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        playerList = PhotonNetwork.PlayerList;
        StartCoroutine(UpdateTask());
        Debug.Log("Player " + otherPlayer.ToString() + " has left. Updating players...");
        playerCount.text = $"Number of players: {PlayerCount()}";
    }
    
    private IEnumerator UpdateTask()
    {
        yield return new WaitForSeconds(1f);
        UpdateRacers();
        UpdatePlayerScores();
    }

    public void UpdatePlayerScores()
    {
        players.text = "Players: (wins)\n";
        racersScores.Sort((p1, p2) => p1.wins.CompareTo(p2.wins));
        for (int i = racersScores.Count - 1; i >= 0; i--)
        {
            players.text += $"{racersScores[i].PlayerNameText.text} ({racersScores[i].wins})\n";
        }
    }
}
