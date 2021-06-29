using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Transform nextCheckpoint;

    public Transform Next()
    {
        return nextCheckpoint;
    }
}
