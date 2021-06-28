using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class CreateAndJoinRoom : MonoBehaviourPunCallbacks
{
    public static CreateAndJoinRoom instance;

    [Header("Input References")]
    [SerializeField] private TMP_InputField createInput;
    [SerializeField] private TMP_InputField joinInput;
    [Space(5f)]

    [Header("Message")]
    [SerializeField] private Text createMessage;
    [SerializeField] private Text joinMessage;

    private void Awake()
    {

        DontDestroyOnLoad(gameObject);
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Destroy(instance.gameObject);
            instance = this;
        }
    }

    public void CreateRoom()
    {
        if (createInput.text.Length < 6)
        {
            Debug.Log("Code is too short");
            createMessage.text = "Code should be at least 6 characters long";
            return;
        }
        else
        {
            Debug.Log("Created Room!");
            PhotonNetwork.CreateRoom(createInput.text, new RoomOptions() { MaxPlayers = 4 }, null);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to create room");
        createMessage.text = "Failed to create room";
        base.OnCreateRoomFailed(returnCode, message);
    }

    public void JoinRoom()
    {
        Debug.Log("Joining room...");
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public void JoinOrCreateRoom(string code)
    {
        Debug.Log("Attempting to join with room code: " + code);
        PhotonNetwork.JoinOrCreateRoom(code, new RoomOptions() { MaxPlayers = 4 }, null);
    }


    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        Debug.Log("Leaving room...");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join room");
        joinMessage.text = "Failed to join room";
        base.OnJoinRoomFailed(returnCode, message);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room!");
        PhotonNetwork.LoadLevel("Multiplayer");
    }

    public override void OnConnectedToMaster()
    {
        if (GameManager.instance.rejoinCode != "")
        {
            Debug.Log("Connected to Master");
            JoinOrCreateRoom(GameManager.instance.rejoinCode);
            GameManager.instance.rejoinCode = "";
        }
    }
    public override void OnLeftRoom()
    {
        if (GameManager.instance.rejoinCode != "")
            Debug.Log("Left Room! Attempting to rejoin now...");
        else
            Debug.Log("Left Room!");
    }
}
