using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Database;

public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;
    public string bestTime;
    public float bestRawTime;
    public bool inMatch = false, alreadyInMatch = false;
    public string leaderTime;
    public string leaderName;
    public bool loading, isPaused = false;
    
    [SerializeField] private GameObject scoreElement;
    public FirebaseUser user;
    public DatabaseReference DBreference;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    void Start()
    {
        if (FirebaseManager.instance == null)
        {
            user = FirebaseAutoLogin.instance.user;
            DBreference = FirebaseAutoLogin.instance.DBreference;
        }
        else
        {
            user = FirebaseManager.instance.user;
            DBreference = FirebaseManager.instance.DBreference;
        }
        StartCoroutine(LoadUserData());
        StartCoroutine(LoadLeaderData());
    }

    private IEnumerator LoadUserData()
    {
        var DBTask = DBreference.Child("users").Child(user.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            loading = false;
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null || DBTask.Result.ChildrenCount == 3)
        {
            // Only username, raw time and formatted time available. Create new child
            Debug.Log("Missing in match data, updating now");
            var UpdateDBTask = DBreference.Child("users").Child(user.UserId).Child("in match").SetValueAsync(false);
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (UpdateDBTask.Exception != null)
            {
                loading = false;
                Debug.LogWarning(message: $"Failed to register task with {UpdateDBTask.Exception}");
            }
            DataSnapshot snapshot = DBTask.Result;
            bestTime = snapshot.Child("singleplayer formatted").Value.ToString();
            bestRawTime = (float)(double)snapshot.Child("singleplayer raw").GetValue(false);
            alreadyInMatch = false;
            loading = false;
            yield return null;
        }
        else
        {
            // Data has been retrieved
            Debug.Log("Data retrieved!");
            DataSnapshot snapshot = DBTask.Result;
            bestTime = snapshot.Child("singleplayer formatted").Value.ToString();
            bestRawTime = (float)(double)snapshot.Child("singleplayer raw").GetValue(false);
            alreadyInMatch = bool.Parse(snapshot.Child("in match").Value.ToString());
            loading = false;
        }
    }


    private IEnumerator LoadLeaderData()
    {
        var DBTask = DBreference.Child("users").OrderByChild("singleplayer raw").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;
            float leaderRaw = float.MaxValue;
            foreach (DataSnapshot childSnapshot in snapshot.Children)
            {
                //leaderRaw = (float)(double)childSnapshot.Child("singleplayer raw").GetValue(false);
                //leaderName = childSnapshot.Child("username").Value.ToString();
                //leaderTime = childSnapshot.Child("singleplayer formatted").Value.ToString();
                //break;
                float childRaw = (float)(double)childSnapshot.Child("singleplayer raw").GetValue(false);
                if (childRaw < leaderRaw)
                {
                    leaderRaw = childRaw;
                    leaderName = childSnapshot.Child("username").Value.ToString();
                    leaderTime = childSnapshot.Child("singleplayer formatted").Value.ToString();
                }
                else
                    continue;
            }
        }
    }

    private IEnumerator LoadScoreboardData()
    {
        var DBTask = DBreference.Child("users").OrderByChild("singleplayer raw").GetValueAsync();
   
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
   
        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;
            int rank = 1;
            foreach (Transform child in MenuUIManager.instance.scoreboardContent.transform)
            {
                Destroy(child.gameObject);
            }
            foreach (DataSnapshot childSnapshot in snapshot.Children)
            {
                string username = childSnapshot.Child("username").Value.ToString();
                string bestTime = childSnapshot.Child("singleplayer formatted").Value.ToString();
                GameObject scoreboardElement = Instantiate(scoreElement, MenuUIManager.instance. scoreboardContent);
                scoreboardElement.GetComponent<ScoreElement>().NewScoreElement(rank, username, bestTime);
                rank++;
            }
        }
    }

    private IEnumerator UpdateUsernameDatabase(string _username)
    {
        var DBTask = DBreference.Child("users").Child(user.UserId).Child("username").SetValueAsync(_username);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
    }

    private IEnumerator UpdateInMatchDatabase(bool _inMatch)
    {
        var DBTask = DBreference.Child("users").Child(user.UserId).Child("in match").SetValueAsync(_inMatch);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        Debug.Log("Updated player: " + _inMatch.ToString());
    }


    public void UpdateInMatch(bool inMatch)
    {
        StartCoroutine(UpdateInMatchDatabase(inMatch));
    }
    public void InMatch()
    {
        StartCoroutine(LoadUserData());
    }

    public void RefreshData()
    {
        StartCoroutine(LoadUserData());
        StartCoroutine(LoadLeaderData());
    }

    public void LoadScoreboard()
    {
        StartCoroutine(LoadScoreboardData());
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Application closing...");
        if (inMatch)
            UpdateInMatch(false);
    }

    private void OnDestroy()
    {
        if (inMatch)
        {
            Debug.Log("destroy");
            UpdateInMatch(false);
        }    
    }

    private void OnApplicationPause(bool pause)
    {
        if (inMatch)
        {
            Debug.Log("pause");
            UpdateInMatch(false);
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if (inMatch)
        {
            if (focus)
            {
                Debug.Log("focus");
                UpdateInMatch(true);
            }
            else
            {
                Debug.Log("lose focus");
                UpdateInMatch(false);
            }
        }
    }
}
