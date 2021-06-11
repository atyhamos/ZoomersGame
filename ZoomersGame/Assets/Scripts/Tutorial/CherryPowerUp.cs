using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CherryPowerUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Pickup();
        }
    }

    private void Pickup()
    {
        Debug.Log("Picked up cherry!");
    }
}
