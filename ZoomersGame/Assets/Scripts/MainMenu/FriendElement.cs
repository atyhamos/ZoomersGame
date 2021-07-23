using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class FriendElement : MonoBehaviour
{

    public TMP_Text usernameText;
    public TMP_Text statusText;
    public Button Accept;
    public Button Reject;
    public Button Invite;
    public Button Remove;
    private string userId;
    public void NewFriendElement(string _username, string _status, string userId)
    {
        usernameText.text = _username;
        statusText.text = _status.ToString();
        this.userId = userId;
        if (!PlayerData.instance.inMatch)
            Remove.gameObject.SetActive(true);
    }

    public void NewFriendReqElement(string _username, string _status, string userId)
    {
        usernameText.text = _username;
        statusText.text = _status.ToString();
        this.userId = userId;
        Accept.gameObject.SetActive(true);
        Reject.gameObject.SetActive(true);
    }

    public void NewFriendElementInMatch(string _username, string _status, string userId)
    {
        usernameText.text = _username;
        statusText.text = _status.ToString();
        this.userId = userId;
        Invite.gameObject.SetActive(true);

    }

    public void AcceptButton()
    {
        AudioManager.instance.ButtonPress();
        PlayerData.instance.Accept(usernameText.text);
    }

    public void RejectButton()
    {
        AudioManager.instance.ButtonPress();
        PlayerData.instance.Reject(usernameText.text);
    }

    public void InviteButton()
    {
        AudioManager.instance.ButtonPress();
        PlayerData.instance.InviteFriend(userId, PhotonNetwork.CurrentRoom.Name);
        Invite.interactable = false;
        Invite.GetComponentInChildren<TMP_Text>().text = "Sent";
        Debug.Log("invite friend!");
    }

    public void RemoveButton()
    {
        AudioManager.instance.ButtonPress();
        PlayerData.instance.Reject(usernameText.text);
        Debug.Log("Removed friend!");
    }
}
