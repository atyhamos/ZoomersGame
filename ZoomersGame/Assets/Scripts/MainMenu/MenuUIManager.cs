using TMPro;
using UnityEngine;

public class MenuUIManager : MonoBehaviour
{
    public static MenuUIManager instance;

    [Header("References")]
    [SerializeField] private GameObject MainUI;
    [SerializeField] private GameObject LeaderboardUI;
    [SerializeField] private GameObject OnlineUI;
    [SerializeField] private GameObject CreateUI;
    [SerializeField] private GameObject JoinUI;
    public Transform scoreboardContent; 

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
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
        ClearUI();
        MainUI.SetActive(true);
    }

    public void LeaderboardScreen()
    {
        ClearUI();  
        LeaderboardUI.SetActive(true);
        PlayerData.instance.LoadScoreboard();
    }

    public void OnlineScreen()
    {
        ClearUI();
        OnlineUI.SetActive(true);
    }

    public void CreateScreen()
    {
        ClearUI();
        CreateUI.SetActive(true);
    }

    public void JoinScreen()
    {
        ClearUI();
        JoinUI.SetActive(true);
    }

}