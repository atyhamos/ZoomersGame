using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FriendElement : MonoBehaviour
{

    public TMP_Text usernameText;
    public TMP_Text statusText;
    public Button Accept;
    public Button Reject;

    public void NewFriendElement(string _username, string _status)
    {
        usernameText.text = _username;
        statusText.text = _status.ToString();
    }

    public void NewFriendReqElement(string _username, string _status)
    {
        usernameText.text = _username;
        statusText.text = _status.ToString();
        Accept.gameObject.SetActive(true);
        Reject.gameObject.SetActive(true);
    }

    public void AcceptButton()
    {
        PlayerData.instance.Accept(usernameText.text);
    }

    public void RejectButton()
    {
        PlayerData.instance.Reject(usernameText.text);
    }
}
