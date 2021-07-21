using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    public static MenuUIManager instance;

    [Header("References")]
    [SerializeField] private GameObject MainUI;
    [SerializeField] private GameObject LeaderboardUI;
    [SerializeField] private GameObject OnlineUI;
    [SerializeField] private GameObject CreateUI;
    [SerializeField] private GameObject JoinUI;
    [SerializeField] private GameObject menuUI;
    [SerializeField] private GameObject friendsUI;
    [SerializeField] private GameObject addFriendsUI;
    [SerializeField] private GameObject menuButton;
    public Toggle bgmToggle, soundFXToggle;
    public bool menuOpen, friendsOpen, addFriendsOpen;
    public Transform scoreboardContent, friendsContent;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        if (!AudioManager.instance.BGMOn())
        {
            bgmToggle.isOn = false;
            AudioManager.instance.ToggleBGM();
        }
        if (!AudioManager.instance.FXOn())
        {
            soundFXToggle.isOn = false;
            AudioManager.instance.ToggleFX();
        }
    }

    private void ClearUI()
    {
        MainUI.SetActive(false);
        OnlineUI.SetActive(false);
        CreateUI.SetActive(false);
        JoinUI.SetActive(false);
        LeaderboardUI.SetActive(false);
    }

    public void MainScreen()
    {
        AudioManager.instance.ButtonPress();
        ClearUI();
        MainUI.SetActive(true);
    }

    public void LeaderboardScreen()
    {
        AudioManager.instance.ButtonPress();
        ClearUI();  
        LeaderboardUI.SetActive(true);
        PlayerData.instance.LoadScoreboard();
    }

    public void OnlineScreen()
    {
        AudioManager.instance.ButtonPress();
        ClearUI();
        OnlineUI.SetActive(true);
    }

    public void CreateScreen()
    {
        AudioManager.instance.ButtonPress();
        ClearUI();
        CreateUI.SetActive(true);
    }

    public void JoinScreen()
    {
        AudioManager.instance.ButtonPress();
        ClearUI();
        JoinUI.SetActive(true);
    }

    public void ShowHideMenu()
    {
        if (menuOpen)
        {
            AudioManager.instance.MenuClose();
            menuUI.SetActive(false);
        }
        else
        {
            AudioManager.instance.MenuOpen();
            menuUI.SetActive(true);
        }
        menuOpen = !menuOpen;
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
        }
        else
        {
            ShowHideFriends();
            AudioManager.instance.MenuOpen();
            addFriendsUI.SetActive(true);
        }
        addFriendsOpen = !addFriendsOpen;
    }
}