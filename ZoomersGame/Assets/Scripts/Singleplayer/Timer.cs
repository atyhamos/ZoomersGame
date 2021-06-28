using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Firebase.Database;
using Firebase.Auth;

public class Timer : MonoBehaviour
{
    [SerializeField] private Text timerText;
    [SerializeField] private GameObject timerBackground;
    [SerializeField] private Text countdownText;
    [SerializeField] private GameObject activeButtons;
    [SerializeField] private GameObject placeholderButtons;

    private float startTime;
    private float timePassed = 0f;
    private bool raceStarted = false;
    private bool isFinished = false;

    private float countdownUntil = 0f;
    private float countdownStart = 3f;

    private DatabaseReference fbDatabase = PlayerData.instance.DBreference;
    private FirebaseUser fbUser = PlayerData.instance.user;
    private bool updated = false;

    // Start is called before the first frame update
    void Start()
    {
        countdownUntil = countdownStart;
        startTime = Time.time; // gives time since application started
    }

    // Update is called once per frame
    void Update()
    {
        if (!raceStarted)
        {
            DisableButtons();
            countdownUntil -= 1 * Time.deltaTime;
            countdownText.text = "Race is starting in " + countdownUntil.ToString("0");
            if (countdownUntil <= 0)
            {
                EnableButtons();
                timerText.gameObject.SetActive(true);
                timerBackground.SetActive(true);
                raceStarted = true;
                countdownText.gameObject.SetActive(false);
            }
        }
        else
        {
            if (isFinished)
            {
                DisableButtons();
                checkDatabase(); // update best time here
                return;
            }
            timePassed += Time.deltaTime; // amount of time since timer has started

            string minutes = ((int)timePassed / 60).ToString();
            string seconds = (timePassed % 60).ToString("f2"); //f2 specifies 2 decimal places
            if (timePassed % 60 < 10)
                seconds = "0" + seconds;
            timerText.text = minutes + ":" + seconds;
        }
    }

    public void Finish()
    {
        isFinished = true;
        timerText.color = Color.green;
    }

    public string GiveTime()
    {
        return timerText.text;
    }

    private void DisableButtons()
    {
        activeButtons.SetActive(false);
        placeholderButtons.SetActive(true);
    }

    private void EnableButtons()
    {
        activeButtons.SetActive(true);
        placeholderButtons.SetActive(false);
    }

    private void checkDatabase()
    {
        if (updated)
            return;
        else
        {
            updated = true;
            Debug.Log(PlayerData.instance.bestRawTime);
            Debug.Log(timePassed);
            // Only update the faster time
            if (timePassed < PlayerData.instance.bestRawTime)
                StartCoroutine(UpdateBestTime(timePassed, timerText.text));
            else
                Debug.Log("Time is slower than fastest time, not updating to database");
        }
    }

    private IEnumerator UpdateBestTime(float _raw, string _formatted)
    {
        var DBTaskRaw = fbDatabase.Child("users").Child(fbUser.UserId).Child("singleplayer raw").SetValueAsync(_raw);
        yield return new WaitUntil(predicate: () => DBTaskRaw.IsCompleted);
        if (DBTaskRaw.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTaskRaw.Exception}");
        }
        Debug.Log($"Raw time updated to {_raw}");
        var DBTaskFormat = fbDatabase.Child("users").Child(fbUser.UserId).Child("singleplayer formatted").SetValueAsync(_formatted);
        yield return new WaitUntil(predicate: () => DBTaskFormat.IsCompleted);
        if (DBTaskFormat.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTaskFormat.Exception}");
        }
        Debug.Log($"Formatted time updated to {_formatted}");
        PlayerData.instance.RefreshData();
    }
}
