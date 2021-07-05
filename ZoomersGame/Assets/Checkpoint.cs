using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Checkpoint nextCheckpoint;
    public bool flipSprite = false;
    public float orthographicSize = 17;
    public Checkpoint Next()
    {
        return nextCheckpoint;
    }


}
