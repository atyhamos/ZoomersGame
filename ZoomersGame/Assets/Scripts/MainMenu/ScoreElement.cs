using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreElement : MonoBehaviour
{

    public TMP_Text rankText;
    public TMP_Text usernameText;
    public TMP_Text singleplayerTimeText;


    public void NewScoreElement(int rank, string _username, string _singleplayerTime)
    {
        rankText.text = rank.ToString();
        usernameText.text = _username;
        singleplayerTimeText.text = _singleplayerTime.ToString();
    }

}