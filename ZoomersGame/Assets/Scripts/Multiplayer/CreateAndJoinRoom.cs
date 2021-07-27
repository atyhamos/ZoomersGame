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
    [SerializeField] private Text randomMessage;


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
        AudioManager.instance.ButtonPress();
        StartCoroutine(CreateRoomTask());
    }

    public void JoinRandom()
    {
        AudioManager.instance.ButtonPress();
        StartCoroutine(JoinRandomTask());
    }

    public IEnumerator JoinRandomTask()
    {
        PlayerData.instance.loading = true;
        PlayerData.instance.InMatch();
        yield return new WaitForSeconds(1f);
        if (PlayerData.instance.alreadyInMatch)
        {
            randomMessage.text = "You are already running a separate instance of the game!";
        }
        else
        {
            Debug.Log("Joining random room...");
            randomMessage.text = "Joining random room...";
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join random room. Creating one!");
        PhotonNetwork.CreateRoom("", new RoomOptions() { MaxPlayers = 4 });
    }

    public IEnumerator CreateRoomTask()
    {
        if (createInput.text.Length < 6)
        {
            Debug.Log("Code is too short");
            createMessage.text = "Code should be at least 6 characters long";
        }
        else
        {
            PlayerData.instance.loading = true;
            PlayerData.instance.InMatch();
            yield return new WaitForSeconds(1f);
            //yield return new WaitUntil(predicate: () => PlayerData.instance.loading == false);
            if (PlayerData.instance.alreadyInMatch)
            {
                createMessage.text = "You are already running a separate instance of the game!";
            }
            else
            {
                Debug.Log("Created Room!");
                PhotonNetwork.CreateRoom(createInput.text, new RoomOptions() { MaxPlayers = 4 }, null);
            }
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to create room: " + message);
        if (returnCode == 32766)
            createMessage.text = "A game with that code already exists!";
        else
            createMessage.text = message;
        base.OnCreateRoomFailed(returnCode, message);
    }

    public void JoinRoom()
    {
        if (joinInput.text.Length < 6)
        {
            joinMessage.text = "Room codes are at least 6 characters long";
            return;
        }
        AudioManager.instance.ButtonPress();
        StartCoroutine(JoinRoomTask());
    }

    public IEnumerator JoinRoomTask()
    {
        PlayerData.instance.InMatch();
        PlayerData.instance.loading = true;
        yield return new WaitUntil(predicate: () => PlayerData.instance.loading == false);
        if (PlayerData.instance.alreadyInMatch)
        {
            joinMessage.text = "You are already running a separate instance of the game!";
        }
        else
        {
            Debug.Log("Joining room...");
            PhotonNetwork.JoinRoom(joinInput.text);
        }

    }

    public void JoinOrCreateRoom(string code)
    {
        StartCoroutine(JoinOrCreateRoomTask(code));
    }

    public IEnumerator JoinOrCreateRoomTask(string code)
    {
        Debug.Log("Attempting to join with room code: " + code);
        PlayerData.instance.loading = true;
        PlayerData.instance.InMatch();
        yield return new WaitUntil(predicate: () => PlayerData.instance.loading == false);
        if (PlayerData.instance.alreadyInMatch)
        {
            Debug.Log("You are already running a separate instance of the game!");
            GameManager.instance.ChangeScene(3);
        }
        else
            PhotonNetwork.JoinOrCreateRoom(code, new RoomOptions() { MaxPlayers = 4 }, null);
    }


    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        Debug.Log("Leaving room...");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join room: " + message);
        joinMessage.text = message;
        base.OnJoinRoomFailed(returnCode, message);
    }

    public override void OnJoinedRoom()
    {
        PlayerData.instance.UpdateInMatch(true); // database
        PlayerData.instance.inMatch = true; // local variable
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
        {
            Debug.Log("Left Room! Attempting to rejoin now...");
            PlayerData.instance.UpdateInMatch(false);
            PlayerData.instance.inMatch = false;
        }
        else
        {
            Debug.Log("Left Room!");
            PlayerData.instance.UpdateInMatch(false);
            PlayerData.instance.inMatch = false;
        }    
    }
}
