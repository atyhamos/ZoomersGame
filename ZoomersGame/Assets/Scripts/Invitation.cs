using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Invitation : MonoBehaviour
{
    public string username;
    public string roomCode;
    public Text invitationMessage;

    public void NewInvitation(string username, string roomCode)
    {
        AudioManager.instance.MenuOpen();
        invitationMessage.text = $"{username} is inviting you to join a room";
        this.username = username;
        this.roomCode = roomCode;
    }

    public void Reject()
    {
        AudioManager.instance.ButtonPress();
        PlayerData.instance.InvitationAction(username, roomCode, false);
        Destroy(this.gameObject);
    }

    public void Accept()
    {
        AudioManager.instance.ButtonPress();
        GameManager.instance.ChangeScene(2);
        PlayerData.instance.InvitationAction(username, roomCode, true);
        Destroy(this.gameObject);
    }

    private void OnDestroy()
    {
        PlayerData.instance.inviteRequest = false;
    }
}
