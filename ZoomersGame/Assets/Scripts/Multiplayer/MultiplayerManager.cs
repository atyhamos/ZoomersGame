using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class MultiplayerManager : MonoBehaviourPunCallbacks
{
    public Transform map1Spawn, map2Spawn;
    [SerializeField] private Text PingText;
    [SerializeField] private GameObject PlayerPrefab, PlayerElement;
    [SerializeField] private GameObject rejoinUI, loseUI, startUI, readyUI, winUI, rulesUI, winnerUI, skinsUI, skinsButton, friendsButton, menuUI, menuButton, countdown;
    [SerializeField] private Transform lobbySpawnLocation;
    [SerializeField] private Text playerCount, players, rules, winnerScreen, debugStuff, hostMessage, nonhostMessage;
    [SerializeField] private GameObject loadingUI, holdingArea;
    [SerializeField] private Text countdownText;
    [SerializeField] private GameObject Checkpoints;
    [SerializeField] private TMP_InputField winsInput;
    public Toggle bgmToggle, soundFXToggle;
    public int winsNeeded = 5, mapIndex = 0;
    public string winnerName;
    public Button startButton;
    public Button readyButton;
    private Player[] playerList;
    private Vector2 spawnPosition, respawnLocation;
    private string roomCode, playersNotReady;
    private MultiplayerController player;
    private int pingUpdate = 1; // 1 second
    public List<MultiplayerController> racersArray, racersScores;
    public static MultiplayerManager instance;
    public bool isRacing;
    private float countdownUntil = 3f;
    public bool inLobby, allPrepared = false;
    public MultiplayerController leadPlayer;
    private bool rulesOpen, skinsOpen, menuOpen;
    private PhotonView view;
    public RuntimeAnimatorController skin;
    private int skinIndex;

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
        if (!player.isHost)
            player.GetMasterData();
        if (!AudioManager.instance.BGMOn())
        {
            bgmToggle.isOn = false;
            AudioManager.instance.ToggleBGM();
        }
        if (!AudioManager.instance.FXOn())
        {
            soundFXToggle.isOn = false;
            AudioManager.instance.ToggleFX();
        }

    }
    public void SpawnPlayer()
    {
        spawnPosition = map1Spawn.position;
        player = PhotonNetwork.Instantiate(PlayerPrefab.name, lobbySpawnLocation.position, Quaternion.identity).gameObject.GetComponent<MultiplayerController>();
        view = GetComponent<PhotonView>();
    }
    public void ToggleBGM()
    {
        AudioManager.instance.ToggleBGM();
    }

    public void ToggleFX()
    {
        AudioManager.instance.ToggleFX();
    }
    private void Update()
    {
        if (PhotonNetwork.NetworkingClient == null || player == null)
        {
            PlayerData.instance.UpdateInMatch(false);
            PlayerData.instance.inMatch = false;
            ClearUI();
            rejoinUI.SetActive(true);
            return;
        }
        debugStuff.text = $"inLobby:{inLobby}\nisRacing:{isRacing}\nAllLoaded:{AllLoaded()}";
        if (Time.time >= pingUpdate)
        {
            pingUpdate = Mathf.FloorToInt(Time.time) + 1;
            Ping();
        }
        if (inLobby)
        {
            countdown.SetActive(false);
            loseUI.SetActive(false);
            player.HideWings();
            if (player.isHost)
            {
                if (PlayerCount() == 1)
                {
                    if (player.PlayerNameText.text == "Amos" || player.PlayerNameText.text == "atyhamos" || player.PlayerNameText.text == "ryan")
                    {
                        hostMessage.text = "Debug Mode";
                        startButton.interactable = true;
                    }
                    else
                    {
                        hostMessage.text = "Online races require at least 2 players";
                        startButton.interactable = false;
                    }
                }
                else if (!AllReady())
                {
                    hostMessage.text = "Players yet to ready: " + playersNotReady;
                    startButton.interactable = false;
                }
                else
                {
                    hostMessage.text = "";
                    startButton.interactable = true;
                }
            }
        }
        else
        {
            // at racing map but race hasnt started
            if (!isRacing && AllAtMap())
            {
                player.DisableButtons();
                countdown.SetActive(true);
                countdownUntil -= 1 * Time.deltaTime;
                countdownText.text = "Race is starting in " + countdownUntil.ToString("0");
                if (countdownUntil <= 0.5)
                    StartCoroutine(GoTask());
            }
        }
        Debug.Log(AllLoaded());
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
        countdown.GetComponentInChildren<Image>().CrossFadeAlpha(0, 1, false);
        yield return new WaitForSeconds(1f);
        countdown.SetActive(false);
        countdownUntil = 3f; // reset
        countdownText.fontSize = 25; // reset
        countdownText.text = "Race is starting in " + countdownUntil.ToString("0"); // reset
    }
    
    public void JoinNonHostMessage()
    {
        nonhostMessage.text = "Press Ready for the host to start the match";
    }

    public void ReadyButton()
    {
        AudioManager.instance.ButtonPress();
        if (player.isReady)
            readyButton.image.color = Color.grey;
        else
            readyButton.image.color = Color.green;
        player.ReadyButton();
        ClearMessage();
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
        loadingUI.SetActive(true);
        player.Load();
        Checkpoints.SetActive(false); // to prevent accidental crossing of checkpoints
        player.transform.position = spawnPosition;
        player.StopMoving();
        player.HideAllButtons();
        startUI.SetActive(false);
        readyUI.SetActive(false);
        skinsOpen = false;
        skinsButton.SetActive(false);
        friendsButton.SetActive(false);
        skinsUI.SetActive(false);
        //while (!AllLoaded())
        //    yield return null;
        yield return new WaitForSeconds(1f);
        loadingUI.SetActive(false);
        inLobby = false;
        AudioManager.instance.Race();
    }
    public void StartButton()
    {
        AudioManager.instance.ButtonPress();
        foreach (MultiplayerController player in racersArray)
        {
            player.StartButton(); //RPC call
        }
    }

    public void RulesButton()
    {
        foreach (MultiplayerController player in racersArray)
        {
            player.StartButton(); //RPC call
        }
    }
    
    public void ShowHideRules()
    {
        winsInput.text = "";
        if (!rulesOpen)
        {
            AudioManager.instance.MenuOpen();
            player.StopMoving();
            player.HideAllButtons();
            rulesUI.SetActive(true);
        }
        else
        {
            AudioManager.instance.MenuClose();
            player.EnableButtons();
            rulesUI.SetActive(false);
        }
        rulesOpen = !rulesOpen;
    }

    public void SubmitRules()
    {
        AudioManager.instance.ButtonPress();
        if (winsInput.text != "")
            UpdateRules(int.Parse(winsInput.text), mapIndex, false);
        else
            UpdateRules(winsNeeded, mapIndex, false);
        player.EnableButtons();
        rulesUI.SetActive(false);
        rulesOpen = !rulesOpen;
        player.UpdateRules(winsNeeded, mapIndex, false);
    }

    public void UpdateRules(int wins, int mapIndex, bool isJustJoined)
    {
        winsNeeded = wins;
        rules.text = $"First to {winsNeeded} wins\n";
        if (mapIndex == 0)
        {
            Debug.Log("Update Industrial city");
            rules.text += "Map: Purple City";
            spawnPosition = map1Spawn.position;
        }
        else
        {
            Debug.Log("Update Grassy Hills");
            rules.text += "Map: Grassy Hills";
            spawnPosition = map2Spawn.position;
        }
        foreach (MultiplayerController player in racersArray)
        {
            if (!player.isHost)
                player.Unready();
        }
        if (isJustJoined)
            JoinNonHostMessage();
        else
            UpdateRulesMessage();
    }

    public void UpdateRulesMessage()
    {
        nonhostMessage.text = "Settings were changed! \nCheck the match settings and ready up!";

    }

    public void ClearMessage()
    {
        nonhostMessage.text = "";
    }

    public bool AllReady()
    {
        string tempplayersNotReady = "";
        foreach (MultiplayerController player in racersArray)
        {
            if (player.isReady || player.isHost)
                continue;
            else
            {
                if (tempplayersNotReady == "")
                {
                    tempplayersNotReady += player.PlayerNameText.text;
                    continue;
                }
                else
                    tempplayersNotReady += ", " + player.PlayerNameText.text;
            }
        }
        playersNotReady = tempplayersNotReady;
        //Debug.Log("all ready!");
        return tempplayersNotReady == "" ? true : false;
    }

    public bool AllAtMap()
    {
        foreach (MultiplayerController player in racersArray)
        {
            if (!player.InLobby())
                continue;
            else
                return false;
        }
        //Debug.Log("all at map!");
        return true;
    }

    public void LoadStart()
    {
        Debug.Log("Loading start menu");
        readyUI.SetActive(false);
        startUI.SetActive(true);
        skinsButton.SetActive(true);
        friendsButton.SetActive(true);
        player.isReady = true;
        rulesOpen = false;
        skinsOpen = false;
    }

    public void LoadReady()
    {
        player.isReady = false;
        startUI.SetActive(false);
        readyUI.SetActive(true);
        skinsButton.SetActive(true);
        friendsButton.SetActive(true);
        skinsOpen = false;
    }

    public void Home()
    {
        AudioManager.instance.ButtonPress();
        AudioManager.instance.Main();
        SceneManager.LoadScene("MainMenu");
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

    public IEnumerator ShowWinner()
    {
        // Game ends here
        AudioManager.instance.GameWin();
        ClearUI();
        winnerScreen.text = $"Our winner!\n{winnerName}";
        winnerUI.SetActive(true);
        Checkpoints.SetActive(false);
        yield return new WaitForSeconds(5f);
        winnerUI.SetActive(false);
        loadingUI.SetActive(true);
        player.transform.position =  lobbySpawnLocation.position;
        while (player.isLoading)
            yield return null;
        loadingUI.SetActive(false);
        inLobby = true;
        readyButton.image.color = Color.grey;
        player.isReady = false;
        player.EnterLobby();
    }

    public void ClearUI()
    {
        loseUI.SetActive(false);
        winUI.SetActive(false);
        readyUI.SetActive(false);
        startUI.SetActive(false);
        rulesUI.SetActive(false);
        winnerUI.SetActive(false);
        loadingUI.SetActive(false);
        skinsButton.SetActive(false);
    }

    public IEnumerator ShowWin()
    {
        winUI.SetActive(true);
        yield return new WaitForSeconds(1f);
        winUI.SetActive(false);
        foreach (MultiplayerController player in racersArray)
        {
            player.StartRace();
        }
    }

    public void ResetRound()
    {
        respawnLocation = new Vector2(Random.Range(leadPlayer.currentCheckpoint.transform.position.x - 1, leadPlayer.currentCheckpoint.transform.position.x + 1),
                player.currentCheckpoint.transform.position.y + 2);
        Checkpoints.SetActive(false);
        player.transform.position = respawnLocation;
        loseUI.SetActive(false);
    }
    
    public void RejoinButton()
    {
        rejoinUI.SetActive(false);
        //GameManager.instance.rejoinCode = roomCode;
        PhotonNetwork.Disconnect();
        CreateAndJoinRoom.instance.LeaveRoom();
        SceneManager.LoadSceneAsync("Splash"); 
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

    public bool InLobby()
    {
        return inLobby;
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
        if (racersArray.Count == 1)
            return;
        playerList = PhotonNetwork.PlayerList;
        PhotonNetwork.SetMasterClient(playerList[0]);
        StartCoroutine(UpdateTask());
        Debug.Log("Player " + otherPlayer.ToString() + " has left. Updating players...");
        playerCount.text = $"Number of players: {PlayerCount()}";
        if (PlayerCount() == 1 && AllAtMap())
        {
            Debug.Log("Win by default");
            player.ShowWinner(player.PlayerNameText.text);
        }

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
        if (racersScores[racersScores.Count - 1].wins >= winsNeeded)
        {
            winnerName = racersScores[racersScores.Count - 1].PlayerNameText.text;
            foreach (MultiplayerController player in racersScores)
            {
                player.ShowWinner(winnerName);
                Debug.Log("Resetting scores...");
                players.text += $"{player.PlayerNameText.text} (0)\n";
            }
        }
        else
        {
            for (int i = racersScores.Count - 1; i >= 0; i--)
            {
                players.text += $"{racersScores[i].PlayerNameText.text} ({racersScores[i].wins})\n";
            }
        }
    }

    public void ShowHideSkinSelection()
    {
        if (skinsOpen)
        {
            AudioManager.instance.MenuOpen();
            skinsUI.SetActive(false);
        }
        else
        {
            AudioManager.instance.MenuClose();
            skinsUI.SetActive(true);
        }
        skinsOpen = !skinsOpen;
    }


    public void ShowHideMenu()
    {
        if (menuOpen)
        {
            AudioManager.instance.MenuOpen();
            menuUI.SetActive(false);
        }
        else
        {
            AudioManager.instance.MenuClose();
            menuUI.SetActive(true);
        }
        menuOpen = !menuOpen;
    }

    public void Click()
    {
        AudioManager.instance.Click();
    }

    public void ChangeSkin()
    {
        player.ChangeSkin(skinIndex);
    }

    public void ChangeFox()
    {
        AudioManager.instance.ButtonPress();
        Debug.Log("Change to Fox");
        player.ChangeSkin(0);
    }

    public void ChangeMask()
    {
        AudioManager.instance.ButtonPress();
        Debug.Log("Change to Mask Dude!");
        player.ChangeSkin(1);
    }

    public void ChangeNinja()
    {
        AudioManager.instance.ButtonPress();
        Debug.Log("Change to Ninja Frog!");
        player.ChangeSkin(2);
    }

    public void ChangePink()
    {
        AudioManager.instance.ButtonPress();
        Debug.Log("Change to Pink Man!");
        player.ChangeSkin(3);
    }

    public void ChangeVirtual()
    {
        AudioManager.instance.ButtonPress();
        Debug.Log("Change to Virtual Guy!");
        player.ChangeSkin(4);
    }

    public void ChangeMap1()
    {
        AudioManager.instance.ButtonPress();
        Debug.Log("Chosen Map 1!");
        mapIndex = 0;
    }

    public void ChangeMap2()
    {
        AudioManager.instance.ButtonPress();
        Debug.Log("Chosen Map 2!");
        mapIndex = 1;
    }

    public void LoadPlayerList(Transform content)
    {
        foreach (Player player in playerList)
        {
            if (player.NickName == PhotonNetwork.NickName)
                continue;
            GameObject playerListElement = Instantiate(PlayerElement, content.transform);
            if (PlayerData.instance.friendNameList.Contains(player.NickName))
                playerListElement.GetComponent<PlayerElement>().NewFriendElement(player.NickName);
            else
                playerListElement.GetComponent<PlayerElement>().NewPlayerElement(player.NickName);
        }    
    }
}
