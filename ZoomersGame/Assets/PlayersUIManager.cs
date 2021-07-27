using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayersUIManager : MonoBehaviour
{
    public Transform playersContent;
    public static PlayersUIManager instance;

    private void Awake()
    {
        instance = this;
    }

}
