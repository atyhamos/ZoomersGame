using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyPowerUp : PowerUp
{

    [SerializeField] private GameObject powerButton;

    public float flyForce = 1.3f;

    private float flyTime = 5f;

    private bool canFly = false;

    private Coroutine co;

    public override void Consume()
    {
        AudioManager.instance.Play("Wings");
        Debug.Log("Flying!");
        player.hasPowerUp = false;
        player.usingPowerUp = true;
        powerButton.SetActive(false);
        co = StartCoroutine(Fly());
    }

    private IEnumerator Fly()
    {
        canFly = true;
        player.wings.SetActive(true);
        yield return new WaitForSeconds(flyTime);
        canFly = false;
        player.wings.SetActive(false);
        player.usingPowerUp = false;
    }

    public override void Pickup(CharacterController2D controller, PlayerController2D player)
    {
        this.controller = controller;
        this.player = player;
        powerButton.SetActive(true);
        Debug.Log("Picked up fly power!");
    }


    public override void Cancel()
    {
        StopCoroutine(co);
        player.wings.SetActive(false);
        canFly = false;
        player.usingPowerUp = false;
    }

    private void Update()
    {
        if (canFly)
        {
            if (player.jumpButtonDown)
            {
                Debug.Log("here");
                controller.rb.velocity = new Vector2(controller.rb.velocity.x, controller.rb.velocity.y + flyForce);
            }
        }
    }
}