using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Firebase.Auth;
using Firebase;

public class MenuControl : MonoBehaviour
{
    [SerializeField] private Text welcomeMessage;
    private string userName;
    private void Start()
    {
        userName = FirebaseManager.instance.user.DisplayName;
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
    
    public void Home()
    {
        GameManager.instance.ChangeScene(1);
        PhotonNetwork.Destroy(GetComponent<PhotonView>());
    }

}
