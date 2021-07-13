using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MultiPowerUp : MonoBehaviour
{
    protected MultiCharacterController controller;
    protected MultiplayerController player;

    public abstract void Pickup(MultiCharacterController controller, MultiplayerController player);
    public abstract void Consume();
    public abstract void Cancel();

}