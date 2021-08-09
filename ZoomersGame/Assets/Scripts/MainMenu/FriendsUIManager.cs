using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FriendsUIManager : MonoBehaviour
{
    public static FriendsUIManager instance;
    
    [SerializeField] private GameObject friendsUI, addFriendsUI;
    private bool friendsOpen, addFriendsOpen;
    public TMP_InputField friendNameInput;
    public Text addFriendMessage;
    public Transform friendsContent;


    private void Awake()
    {
        instance = this;
    }

    public void RefreshData()
    {
        AudioManager.instance.ButtonPress();
        PlayerData.instance.StartCoroutine("LoadUserData");
    }

    public void ShowHideFriends()
    {
        if (friendsOpen)
        {
            AudioManager.instance.MenuClose();
            friendsUI.SetActive(false);
        }
        else
        {
            if (PlayerData.instance.inMatch)
                PlayerData.instance.LoadFriendsInMatch(friendsContent);
            else 
                PlayerData.instance.LoadFriends();
            AudioManager.instance.MenuOpen();
            friendsUI.SetActive(true);
        }
        friendsOpen = !friendsOpen;
    }

    public void ShowHideAddFriends()
    {
        if (addFriendsOpen)
        {
            AudioManager.instance.MenuClose();
            addFriendsUI.SetActive(false);
            ShowHideFriends();
        }
        else
        {
            ShowHideFriends();
            AudioManager.instance.MenuOpen();
            addFriendsUI.SetActive(true);
        }
        addFriendsOpen = !addFriendsOpen;
    }

    public void ShowHideAddFriendsInMatch()
    {
        if (addFriendsOpen)
        {
            AudioManager.instance.MenuClose();
            addFriendsUI.SetActive(false);
            ShowHideFriends();
        }
        else
        {
            foreach (Transform child in PlayersUIManager.instance.playersContent.transform)
            {
                Debug.Log("Destroying old data!!!!");
                Destroy(child.gameObject);
            }
            MultiplayerManager.instance.LoadPlayerList(PlayersUIManager.instance.playersContent.transform);
            ShowHideFriends();
            AudioManager.instance.MenuOpen();
            addFriendsUI.SetActive(true);
        }
        addFriendsOpen = !addFriendsOpen;
    }

    public void AddFriend()
    {
        PlayerData.instance.SubmitFriendRequest();
    }

}
