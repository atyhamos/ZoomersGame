using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class MultiplayerManager : MonoBehaviour
{
    public float minX, maxX, minY, maxY;
    public Text PingText;
    public GameObject PlayerPrefab;
    private int numberOfPlayers = PhotonNetwork.CurrentRoom.PlayerCount - 1;
    private Vector2 randomPosition;
    private void Start()
    {
        SpawnPlayer();
    }
    public void SpawnPlayer()
    {
        randomPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        PhotonNetwork.Instantiate(PlayerPrefab.name, randomPosition, Quaternion.identity);
    }

    private void Update()
    {
        PingText.text = "Ping: " + PhotonNetwork.GetPing();
    }
}
