using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Transform nextCheckpoint;
    public float orthographicSize = 13;
    public Transform Next()
    {
        return nextCheckpoint;
    }


}
