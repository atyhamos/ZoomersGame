using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class MenuControl : MonoBehaviourPunCallbacks
{
    [SerializeField] private Text welcomeMessage;
    [SerializeField] private Text playerStats;
    [SerializeField] private Text leaderBoard;
    private string userName;
    private void Awake()
    {
        if (FirebaseManager.instance == null)
            userName = FirebaseAutoLogin.instance.user.DisplayName;
        else
            userName = FirebaseManager.instance.user.DisplayName;

        if (userName == null)
            welcomeMessage.text = "Welcome to Zoomers!";
        else
            welcomeMessage.text = $"Welcome, {userName}!";

        PhotonNetwork.NickName = userName;
    }
    public void Tutorial()
    {
        GameManager.instance.ChangeScene(4);
    }

    public void Singleplayer()
    {
        GameManager.instance.ChangeScene(5);
    }

    public void LogOut()
    {
        if (FirebaseManager.instance == null)
            FirebaseAutoLogin.instance.auth.SignOut();
        else
            FirebaseManager.instance.auth.SignOut();

        PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        GameManager.instance.ChangeScene(1);
        Debug.Log(userName + " has signed out");
        base.OnDisconnected(cause);
    }

    private void Start()
    {
        playerStats.text += PlayerData.instance.bestTime;
        leaderBoard.text += PlayerData.instance.leaderName + $" ({PlayerData.instance.leaderTime})";
    }
}
