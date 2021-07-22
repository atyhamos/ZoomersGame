using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerElement : MonoBehaviour
{
    public TMP_Text usernameText;
    public Button AddFriend;
    public TMP_Text friendsAlready;
    public void NewPlayerElement(string _username)
    {
        usernameText.text = _username;
        AddFriend.gameObject.SetActive(true);
    }
    
    public void NewFriendElement(string _username)
    {
        Debug.Log("Loading existing friend element!");
        usernameText.text = _username;
        friendsAlready.gameObject.SetActive(true);
    }

    public void AddFriendButton()
    {
        AudioManager.instance.ButtonPress();
        Debug.Log("Add Friend");
        PlayerData.instance.SubmitFriendRequest(usernameText.text);
        AddFriend.interactable = false;
        AddFriend.GetComponentInChildren<TMP_Text>().text = "Requested";
    }
}
