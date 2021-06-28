using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxPowerUp : PowerUp
{

    [SerializeField] private GameObject powerButton;
    [SerializeField] private GameObject prefabBox;
    [SerializeField] private Transform boxPlacement;

    public override void Consume()
    {
        Debug.Log("Placed box!");
        SpawnBox();
        player.hasPowerUp = false;
        player.usingPowerUp = false;
        powerButton.SetActive(false);
    }

    public override void Pickup(CharacterController2D controller, PlayerController2D player)
    {
        this.controller = controller;
        this.player = player;
        powerButton.SetActive(true);
        Debug.Log("Picked up box!");
        //  Destroy(gameObject);
    }
    public override void Cancel()
    {
        player.usingPowerUp = false;
    }

    private void SpawnBox()
    {
        GameObject box = Instantiate(prefabBox) as GameObject;
        box.transform.position = boxPlacement.position;
        StartCoroutine(StartDespawnBox(box));
    }

    private IEnumerator StartDespawnBox(GameObject box)
    {
        yield return new WaitForSeconds(8f);
        Destroy(box);
    }
}
