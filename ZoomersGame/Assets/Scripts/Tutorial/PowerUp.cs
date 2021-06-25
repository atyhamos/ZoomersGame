using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PowerUp : MonoBehaviour
{

    protected CharacterController2D controller;
    protected PlayerController2D player;


    public abstract void Consume();
    public abstract void Pickup(CharacterController2D controller, PlayerController2D player);
    public abstract void Cancel();

}
