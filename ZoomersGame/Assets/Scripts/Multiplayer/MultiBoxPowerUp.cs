using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MultiBoxPowerUp : MultiPowerUp
{

    [SerializeField] private GameObject prefabBox;
    private Rigidbody2D boxRb;
    private Transform boxPlacement;

    public override void Consume()
    {
        Debug.Log("Placed box!");
        SpawnBox();
        player.usingPowerUp = false;
    }

    public override void Pickup(MultiCharacterController controller, MultiplayerController player)
    {
        this.controller = controller;
        this.player = player;
        Debug.Log("Picked up box!");
    }
    public override void Cancel()
    {
        player.usingPowerUp = false;
    }

    private void SpawnBox()
    {
        GameObject box = PhotonNetwork.Instantiate(prefabBox.name, controller.trapPlacement.position, Quaternion.identity);
        StartCoroutine(StartDespawnBox(box));
    }


    private IEnumerator StartDespawnBox(GameObject box)
    {
        yield return new WaitForSeconds(8f);
        if (player.GetComponent<PhotonView>().IsMine)
            PhotonNetwork.Destroy(box);
    }
}
