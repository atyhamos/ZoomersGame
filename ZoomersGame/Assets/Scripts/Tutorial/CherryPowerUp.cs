using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CherryPowerUp : PowerUp
{

    [SerializeField] private GameObject powerButton;

    private float boostTime = 5f;

    private Coroutine co;

    public override void Consume()
    {
        AudioManager.instance.Play("Cherry");
        this.player.particles.SetActive(true);
        Debug.Log("Consumed cherry! Boosting!");
        player.hasPowerUp = false;
        player.usingPowerUp = true;
        powerButton.SetActive(false);
        co = StartCoroutine(SpeedUp());
    }

    private IEnumerator SpeedUp()
    {
        controller.maxSpeed *= 1.5f;
        yield return new WaitForSeconds(boostTime);
        controller.maxSpeed /= 1.5f;
        this.player.particles.SetActive(false);
        player.usingPowerUp = false;
    }

    public override void Pickup(CharacterController2D controller, PlayerController2D player)
    {
        this.controller = controller;
        this.player = player;
        powerButton.SetActive(true);
        Debug.Log("Picked up cherry!");
        //  Destroy(gameObject);
    }


    public override void Cancel()
    {
        StopCoroutine(co);
        controller.maxSpeed /= 1.5f;
        player.usingPowerUp = false;
        this.player.particles.SetActive(false);
    }

}
