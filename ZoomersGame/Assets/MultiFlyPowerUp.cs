using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiFlyPowerUp : MultiPowerUp
{
    public float flyForce = 1.3f;

    private float flyTime = 4f;

    private bool canFly = false;

    private Coroutine co;

    public override void Consume()
    {
        Debug.Log("Flying!");
        player.usingPowerUp = true;
        co = StartCoroutine(Fly());
    }

    private IEnumerator Fly()
    {
        canFly = true;
        player.ShowWings();
        yield return new WaitForSeconds(flyTime);
        canFly = false;
        player.HideWings();
        player.usingPowerUp = false;
    }

    public override void Pickup(MultiCharacterController controller, MultiplayerController player)
    {
        this.controller = controller;
        this.player = player;
        Debug.Log("Picked up fly power!");
    }


    public override void Cancel()
    {
        StopCoroutine(co);
        player.HideWings();
        canFly = false;
        player.usingPowerUp = false;
    }

    private void Update()
    {
        if (canFly)
        {
            if (player.jumpButtonDown)
            {
                if (controller.rb.velocity.y < 0)
                {
                    controller.rb.velocity = new Vector2(controller.rb.velocity.x, 0);
                }
                else
                {
                    controller.rb.velocity = new Vector2(controller.rb.velocity.x, controller.rb.velocity.y + flyForce);
                    float scale = controller.rb.gravityScale;
                    scale -= 0.003f;
                    controller.rb.gravityScale = scale;
                    Debug.Log(controller.rb.gravityScale);
                }
            }
            else
                controller.rb.gravityScale = 4.5f;
        }
        else if (controller == null)
            return;
        else
            controller.rb.gravityScale = 4.5f;
    }
}