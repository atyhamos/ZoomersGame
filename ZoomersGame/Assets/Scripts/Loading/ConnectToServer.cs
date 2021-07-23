using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        //if (PhotonNetwork.IsConnected && PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Leaving) // rejoining
        //    return;
        //else if (PhotonNetwork.InLobby)
        //    SceneManager.LoadScene("MainMenu");
        //else
        //{
        //    PhotonNetwork.SerializationRate = 20;
        //    PhotonNetwork.ConnectUsingSettings();
        //}

        if (PhotonNetwork.IsConnected)
            return;
        PhotonNetwork.SerializationRate = 20;
        PhotonNetwork.ConnectUsingSettings();

    }

    public override void OnConnectedToMaster()
    {
        if (PhotonNetwork.NetworkClientState != Photon.Realtime.ClientState.Joining)
            PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
