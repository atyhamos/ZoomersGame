using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class CreateAndJoinRoom : MonoBehaviourPunCallbacks
{

    public TMP_InputField createInput;
    public TMP_InputField joinInput;

    public void CreateRoom()
    {
        Debug.Log("Created Room!");
        PhotonNetwork.CreateRoom(createInput.text, new RoomOptions() { MaxPlayers = 4 }, null);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room!");
        PhotonNetwork.LoadLevel("Multiplayer");
    }
}
