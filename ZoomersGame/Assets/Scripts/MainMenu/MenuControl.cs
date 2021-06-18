using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class MenuControl : MonoBehaviour
{
    [SerializeField] private Text welcomeMessage;
    private string userName;
    private void Awake()
    {
        userName = FirebaseManager.instance.user.DisplayName;
        if (userName == null)
            welcomeMessage.text = "Welcome to Zoomers!";
        else
            welcomeMessage.text = $"Welcome, {userName}!";
        PhotonNetwork.NickName = userName; 
    }
    public void Tutorial()
    {
        GameManager.instance.ChangeScene(3);
    }

    public void LogOut()
    {
        FirebaseManager.instance.auth.SignOut();
        PhotonNetwork.Disconnect();
        GameManager.instance.ChangeScene(0);
        Debug.Log(userName + " has signed out");
    }
}
