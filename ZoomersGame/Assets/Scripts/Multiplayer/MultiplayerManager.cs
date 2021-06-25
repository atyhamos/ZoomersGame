using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class MultiplayerManager : MonoBehaviour
{
    public float minX, maxX, minY, maxY;
    [SerializeField] private Text PingText;
    [SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private GameObject rejoinUI;
    private Vector2 randomPosition;
    private string roomCode;
    private GameObject player;
    private int pingUpdate = 1; // 1 second

    private void Start()
    {
        SpawnPlayer();
        roomCode = PhotonNetwork.CurrentRoom.Name;
        GameManager.instance.rejoinCode = "";
    }
    public void SpawnPlayer()
    {
        randomPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        player = PhotonNetwork.Instantiate(PlayerPrefab.name, randomPosition, Quaternion.identity);
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
    }

    public void Home()
    {
        CreateAndJoinRoom.instance.LeaveRoom();
        GameManager.instance.ChangeScene(3);
    }


    public void RejoinButton()
    {
        rejoinUI.SetActive(false);
        GameManager.instance.rejoinCode = roomCode;
        SceneManager.LoadSceneAsync("Loading");
        CreateAndJoinRoom.instance.LeaveRoom();
    }

    private void Ping()
    {
        PingText.text = "Ping: " + PhotonNetwork.GetPing();
    }
}
