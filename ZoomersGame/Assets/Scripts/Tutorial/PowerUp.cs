using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public bool hasPowerUp = false;
    public bool isPowerUp = false;
    private float boostTime = 5f;
    private float currentTime = 0f;
    public GameObject button;

    [SerializeField] private CharacterController2D controller;

    public void Consume()
    {
        if (isPowerUp)
            controller.maxSpeed /= 1.5f;
        isPowerUp = false;
        hasPowerUp = false;
        isPowerUp = true;
        button.SetActive(false);
        controller.maxSpeed *= 1.5f;
    }

    private void Boost()
    {
        Debug.Log("Boosting!");
        controller.maxSpeed *= 1.5f;
        while (isPowerUp && currentTime + boostTime > Time.time)
        {
            isPowerUp = true;
        }
        Debug.Log(currentTime);
        controller.maxSpeed /= 1.5f;
        isPowerUp = false;
    }

    private void Update()
    {
        if (isPowerUp && boostTime > 0)
            boostTime -= Time.deltaTime;
        else if (isPowerUp && boostTime < 0)
        {
            isPowerUp = false;
            boostTime = 5f;
            controller.maxSpeed /= 1.5f;
        }
    }

    // public void Consume()
    // {
    //     hasPowerUp = false;
    //     //if (isPowerUp)
    //     //    isPowerUp = false; // stop previous powerup
    //     Boost();
    //     button.SetActive(false);
    // }
    //
    // private void Boost()
    // {
    //     Debug.Log("Boosting!");
    //     isPowerUp = true;
    //     float currentTime = 0f;
    //     controller.maxSpeed *= 1.5f;
    //     while (isPowerUp && currentTime < boostTime)
    //     {
    //         Debug.Log("looping");
    //         currentTime += Time.deltaTime;
    //     }
    //     Debug.Log(controller.maxSpeed);
    //     isPowerUp = false;
    //     controller.maxSpeed /= 1.5f;
    // }
}
