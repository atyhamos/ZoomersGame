using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class MenuControl : MonoBehaviourPunCallbacks
{
    public static MenuControl instance;
    [SerializeField] private Text welcomeMessage;
    [SerializeField] private Text playerStats;
    [SerializeField] private Text leaderBoard;
    private string userName;
    private bool loggingOut;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        if (FirebaseManager.instance == null)
            userName = FirebaseAutoLogin.instance.user.DisplayName;
        else
            userName = FirebaseManager.instance.user.DisplayName;

        if (userName == null)
            welcomeMessage.text = "Welcome to Zoomers!";
        else
            welcomeMessage.text = $"Welcome, {userName}!";

        PhotonNetwork.NickName = userName;
        welcomeMessage.text += $" You are connected to Asia server.";
    }

    private void Start()
    {
        playerStats.text = PlayerData.instance.bestTime;
        leaderBoard.text = PlayerData.instance.leaderName + $" ({PlayerData.instance.leaderTime})";
        loggingOut = false;
    }
    private void Update()
    {
        playerStats.text = PlayerData.instance.bestTime;
        leaderBoard.text = PlayerData.instance.leaderName + $" ({PlayerData.instance.leaderTime})";
    }

    public void Tutorial()
    {
        AudioManager.instance.ButtonPress();
        GameManager.instance.ChangeScene(4);
    }

    public void Singleplayer()
    {
        AudioManager.instance.ButtonPress();
        GameManager.instance.ChangeScene(5);
    }

    public void LogOut()
    {
        AudioManager.instance.ButtonPress();
        PlayerData.instance.StartCoroutine("UpdateStatus", false);
        if (FirebaseManager.instance == null)
            FirebaseAutoLogin.instance.auth.SignOut();
        else
            FirebaseManager.instance.auth.SignOut();

        PhotonNetwork.Disconnect();
        Destroy(PlayerData.instance);
        GameManager.instance.ChangeScene(1);
        loggingOut = true;
    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log(userName + " has signed out");
        PlayerData.instance.UpdateInMatch(false);
        if (loggingOut)
            return;
        GameManager.instance.ChangeScene(2);
        base.OnDisconnected(cause);
    }

    public void UpdateScore(string bestTime, string leaderTime, string leaderName)
    {
        playerStats.text = bestTime;
        leaderBoard.text = leaderName + $" ({leaderTime})";
    }

    public void Click()
    {
        AudioManager.instance.Click();
    }

    public void ToggleBGM()
    {
        AudioManager.instance.ToggleBGM();
    }

    public void ToggleFX()
    {
        AudioManager.instance.ToggleFX();
    }
}
