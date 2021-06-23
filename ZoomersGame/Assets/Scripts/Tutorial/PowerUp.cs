using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public bool hasPowerUp = false;

    public GameObject button;

    [SerializeField] private CharacterController2D controller;

    void Start()
    {
        button = GameObject.Find("Boost");
        button.SetActive(false);
    }

    public void Consume()
    {
        StartCoroutine(Boost());
        button.SetActive(false);
    }

    IEnumerator Boost()
    {
        controller.moveForce = 700;
        controller.maxSpeed = 34;

        yield return new WaitForSeconds(8f);

        controller.moveForce = 350;
        controller.maxSpeed = 17;
        hasPowerUp = false;
    }


}
