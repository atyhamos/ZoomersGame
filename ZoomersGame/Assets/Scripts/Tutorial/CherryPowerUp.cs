using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CherryPowerUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.GetComponent<PowerUp>().hasPowerUp == false)
            {
                Pickup(collision);
                collision.GetComponent<PowerUp>().button.SetActive(true);
            }
        }
    }

    void Pickup(Collider2D player)
    {
        Debug.Log("Picked up cherry!");

        PowerUp powerUp = player.GetComponent<PowerUp>();

        powerUp.hasPowerUp = true;

        Destroy(gameObject);
    }
}
