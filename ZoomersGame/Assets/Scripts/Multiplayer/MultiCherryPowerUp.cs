using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiCherryPowerUp : MultiPowerUp
{


    [Range(1, 2)] [SerializeField] private float speedBoost = 1.5f;
    private float boostTime = 5f;
    private Coroutine co;

    public override void Consume()
    {
        Debug.Log("Consumed cherry! Boosting!");
        player.usingPowerUp = true;
        co = StartCoroutine(SpeedUp());
    }

    private IEnumerator SpeedUp()
    {
        controller.maxSpeed *= speedBoost;
        yield return new WaitForSeconds(boostTime);
        controller.maxSpeed /= speedBoost;
        player.usingPowerUp = false;
    }

    public override void Pickup(MultiCharacterController controller, MultiplayerController player)
    {
        this.controller = controller;
        this.player = player;
        Debug.Log("Picked up cherry!");
        //  Destroy(gameObject);
    }


    public override void Cancel()
    {
        StopCoroutine(co);
        controller.maxSpeed /= 1.5f;
        player.usingPowerUp = false;
    }

}
