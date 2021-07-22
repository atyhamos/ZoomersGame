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
        AudioManager.instance.Play("Cherry");
        Debug.Log("Consumed cherry! Boosting!");
        player.EnableParticles();
        player.usingPowerUp = true;
        co = StartCoroutine(SpeedUp());
    }

    private IEnumerator SpeedUp()
    {
        controller.maxSpeed *= speedBoost;
        yield return new WaitForSeconds(boostTime);
        controller.maxSpeed /= speedBoost;
        player.usingPowerUp = false;
        player.DisableParticles();
        if (controller.maxSpeed != 14f)
            controller.maxSpeed = 14f;
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
        player.DisableParticles();
        controller.maxSpeed /= speedBoost;
        if (controller.maxSpeed != 14f)
            controller.maxSpeed = 14f;
        player.usingPowerUp = false;
    }

}
