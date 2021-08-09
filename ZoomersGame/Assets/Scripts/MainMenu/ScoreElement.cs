using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreElement : MonoBehaviour
{

    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_Text singleplayerTimeText;


    public void NewScoreElement(int rank, string _username, string _singleplayerTime)
    {
        rankText.text = rank.ToString();
        usernameText.text = _username;
        singleplayerTimeText.text = _singleplayerTime.ToString();
    }

}