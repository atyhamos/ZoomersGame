using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    public static MenuUIManager instance;

    [Header("References")]
    [SerializeField] private GameObject MainUI;
    [SerializeField] private GameObject WeeklyLeaderboardUI, GlobalLeaderboardUI;
    [SerializeField] private GameObject OnlineUI;
    [SerializeField] private GameObject CreateUI;
    [SerializeField] private GameObject JoinUI;
    [SerializeField] private GameObject menuUI;
    [SerializeField] private GameObject menuButton, globalButton;
    [SerializeField] private GameObject RandomUI;
    [SerializeField] private GameObject ShopUI;
    [SerializeField] private Text coinCount;
    [SerializeField] private Button mushroom, radish, pig, slime, zoomerFox, rewardButton;
    [SerializeField] private GameObject mushroomUI, radishUI, pigUI, slimeUI, foxUI;
    [SerializeField] private Toggle bgmToggle, soundFXToggle;
    [SerializeField] private GameObject insufficient, gift;
    private bool mushroomOpen, radishOpen, pigOpen, slimeOpen, foxOpen;
    private bool menuOpen;
    public Transform weeklyScoreboardContent, globalScoreboardContent;

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
        InvokeRepeating("CheckGift", 1f, 5f);
    }

    private void CheckGift()
    {
        if (PlayerData.instance.dailyGift)
            rewardButton.interactable = true;
    }

    private void ClearUI()
    {
        MainUI.SetActive(false);
        OnlineUI.SetActive(false);
        CreateUI.SetActive(false);
        JoinUI.SetActive(false);
        WeeklyLeaderboardUI.SetActive(false);
        GlobalLeaderboardUI.SetActive(false);
        RandomUI.SetActive(false);
        ShopUI.SetActive(false);
    }

    public void MainScreen()
    {
        AudioManager.instance.ButtonPress();
        ClearUI();
        MainUI.SetActive(true);
        if (PlayerData.instance.dailyGift)
            rewardButton.interactable = true;
    }

    public void LeaderboardScreen()
    {
        AudioManager.instance.ButtonPress();
        ClearUI();  
        WeeklyLeaderboardUI.SetActive(true);
        PlayerData.instance.LoadWeeklyScoreboard();
    }

    public void GlobalScreen()
    {
        AudioManager.instance.ButtonPress();
        ClearUI();
        GlobalLeaderboardUI.SetActive(true);
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

    public void RandomScreen()
    {
        AudioManager.instance.ButtonPress();
        ClearUI();
        RandomUI.SetActive(true);
    }

    public void ShopScreen()
    {
        AudioManager.instance.ButtonPress();
        ClearUI();
        ShopUI.SetActive(true);
        foreach (string skin in PlayerData.instance.skinsList)
        {
            switch (skin)
            {
                case "Mushroom":
                    mushroom.interactable = false;
                    break;
                case "Radish":
                    radish.interactable = false;
                    break;
                case "Angry Pig":
                    pig.interactable = false;
                    break;
                case "Slime":
                    slime.interactable = false;
                    break;
                case "Zoomer Fox":
                    zoomerFox.interactable = false;
                    break;
            }
        }
        coinCount.text = PlayerData.instance.coins.ToString();
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

    public void ShowHideCharacter(string character)
    {
        switch (character)
        {
            case "Mushroom":
                if (mushroomOpen)
                {
                    AudioManager.instance.MenuClose();
                    mushroomUI.SetActive(false);
                }
                else
                {
                    AudioManager.instance.ButtonPress();
                    if (PlayerData.instance.coins < 50)
                    {
                        AudioManager.instance.ButtonPress();
                        Instantiate(insufficient);
                        Debug.Log("Insufficient funds!");
                        return;
                    }    
                    mushroomUI.SetActive(true);
                }
                mushroomOpen = !mushroomOpen;
                break;
            case "Radish":
                if (radishOpen)
                {
                    AudioManager.instance.MenuClose();
                    radishUI.SetActive(false);
                }
                else
                {
                    AudioManager.instance.ButtonPress();
                    if (PlayerData.instance.coins < 150)
                    {
                        AudioManager.instance.ButtonPress();
                        Instantiate(insufficient);
                        Debug.Log("Insufficient funds!");
                        return;
                    }
                    radishUI.SetActive(true);
                }
                radishOpen = !radishOpen;
                break;
            case "Angry Pig":
                if (pigOpen)
                {
                    AudioManager.instance.MenuClose();
                    pigUI.SetActive(false);
                }
                else
                {
                    AudioManager.instance.ButtonPress();
                    if (PlayerData.instance.coins < 500)
                    {
                        AudioManager.instance.ButtonPress();
                        Instantiate(insufficient);
                        Debug.Log("Insufficient funds!");
                        return;
                    }
                    pigUI.SetActive(true);
                }
                pigOpen = !pigOpen;
                break;
            case "Slime":
                if (slimeOpen)
                {
                    AudioManager.instance.MenuClose();
                    slimeUI.SetActive(false);
                }
                else
                {
                    AudioManager.instance.ButtonPress();
                    if (PlayerData.instance.coins < 1000)
                    {
                        Instantiate(insufficient);
                        Debug.Log("Insufficient funds!");
                        return;
                    }
                    slimeUI.SetActive(true);
                }
                slimeOpen = !slimeOpen;
                break;
            case "Zoomer Fox":
                if (foxOpen)
                {
                    AudioManager.instance.MenuClose();
                    foxUI.SetActive(false);
                }
                else
                {
                    AudioManager.instance.ButtonPress();
                    if (PlayerData.instance.friendList.Count < 5)
                    {
                        Instantiate(insufficient);
                        Debug.Log("Insufficient friends! You have " + PlayerData.instance.friendList.Count + " friends.");
                        return;
                    }
                    foxUI.SetActive(true);
                }
                foxOpen = !foxOpen;
                break;
        }
     }

    public void BuyCharacter(string character)
    {
        int cost;
        if (character == "Mushroom")
        {
            cost = 50;
            mushroom.interactable = false;
        }
        else if (character == "Radish")
        {
            cost = 150;
            radish.interactable = false;
        }
        else if (character == "Angry Pig")
        {
            cost = 500;
            pig.interactable = false;
        }
        else if (character == "Slime")
        {
            cost = 1000;
            slime.interactable = false;
        }
        else if (character == "Zoomer Fox")
        {
            cost = 0;
            zoomerFox.interactable = false;
        }
        else
            return;
        PlayerData.instance.BuyCharacter(character, cost);
        coinCount.text = (PlayerData.instance.coins - cost).ToString();
        ShowHideCharacter(character);
    }

    public void ReceiveGift()
    {
        AudioManager.instance.ButtonPress();
        rewardButton.interactable = false;
        Instantiate(gift);
        PlayerData.instance.dailyGift = false;
        PlayerData.instance.ReceiveGift();
    }
}