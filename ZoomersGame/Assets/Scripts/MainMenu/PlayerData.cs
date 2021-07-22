using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using UnityEngine.UI;

public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;
    public string bestTime;
    public float bestRawTime;
    public bool inMatch = false, alreadyInMatch = false;
    public string leaderTime;
    public string leaderName;
    public bool loading, isPaused = false;
    public List<string> friendList, friendRequestList;
    public TMP_InputField friendNameInput;
    public Text addFriendMessage;
    [SerializeField] private GameObject scoreElement;
    [SerializeField] private GameObject friendElement;
    public FirebaseUser user;
    public DatabaseReference DBreference;
    public int totalInstances;

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
        StartCoroutine(UpdateStatus(true));
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
            totalInstances = int.Parse(snapshot.Child("instances").Value.ToString());
            foreach (DataSnapshot child in snapshot.Child("friends").Children)
            {
                // is a friend
                if ((bool)child.Value)
                    StartCoroutine(CheckMutualFriends(child.Key));
                else
                    friendRequestList.Add(child.Key);
            }
            loading = false;
        }
    }

    public IEnumerator CheckMutualFriends(string userId)
    {
        var DBTask = DBreference.Child("users").Child(userId).GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;
            foreach (DataSnapshot child in snapshot.Child("friends").Children)
            {
                if (child.Key == user.UserId && (bool)child.Value)
                {
                    Debug.Log("Mutual Friends!");
                    friendList.Add(userId);
                    continue;
                }
                Debug.Log("Not mutual friends");
            }
        }
    }

    private IEnumerator LoadFriendData(string userId)
    {
        var DBTask = DBreference.Child("users").Child(userId).GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            Debug.Log("Saving friend data!");
            DataSnapshot snapshot = DBTask.Result;
            string username = snapshot.Child("username").Value.ToString();
            int instances = int.Parse(snapshot.Child("instances").Value.ToString());
            GameObject friendlistElement = Instantiate(friendElement, MenuUIManager.instance.friendsContent);
            friendlistElement.GetComponent<FriendElement>().NewFriendElement(username, instances == 0 ? "Offline" : "Online");
        }
    }

    private IEnumerator LoadFriendReqData(string userId)
    {
        var DBTask = DBreference.Child("users").Child(userId).GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            Debug.Log("Saving friend request data!");
            DataSnapshot snapshot = DBTask.Result;
            string username = snapshot.Child("username").Value.ToString();
            int instances = int.Parse(snapshot.Child("instances").Value.ToString());
            GameObject friendlistElement = Instantiate(friendElement, MenuUIManager.instance.friendsContent);
            friendlistElement.GetComponent<FriendElement>().NewFriendReqElement(username, instances == 0 ? "Offline" : "Online");
        }
    }

    public void Accept(string _username)
    {
        StartCoroutine(FriendRequestAction(_username, true));
    }

    public void Reject(string _username)
    {
        StartCoroutine(FriendRequestAction(_username, false));
    }

    private IEnumerator FriendRequestAction(string _username, bool accepted)
    {
        var FindNameTask = DBreference.Child("users").GetValueAsync();

        yield return new WaitUntil(predicate: () => FindNameTask.IsCompleted);

        if (FindNameTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {FindNameTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = FindNameTask.Result;
            foreach (DataSnapshot childSnapshot in snapshot.Children)
            {
                if (_username == childSnapshot.Child("username").GetValue(false).ToString())
                {
                    if (!accepted)
                    {
                        var RemoveTask = DBreference.Child("users").Child(childSnapshot.Key).Child("friends").Child(user.UserId).RemoveValueAsync();
                        yield return new WaitUntil(predicate: () => RemoveTask.IsCompleted);

                        if (RemoveTask.Exception != null)
                        {
                            Debug.LogWarning(message: $"Failed to register task with {RemoveTask.Exception}");
                            yield break;
                        }
                        RemoveTask = DBreference.Child("users").Child(user.UserId).Child("friends").Child(childSnapshot.Key).RemoveValueAsync();
                        yield return new WaitUntil(predicate: () => RemoveTask.IsCompleted);

                        if (RemoveTask.Exception != null)
                        {
                            Debug.LogWarning(message: $"Failed to register task with {RemoveTask.Exception}");
                            yield break;
                        }
                        Debug.Log("Removed friend request");
                    }
                    else
                    {
                        var AcceptTask = DBreference.Child("users").Child(user.UserId).Child("friends").Child(childSnapshot.Key).SetValueAsync(true);
                        yield return new WaitUntil(predicate: () => AcceptTask.IsCompleted);

                        if (AcceptTask.Exception != null)
                        {
                            Debug.LogWarning(message: $"Failed to register task with {AcceptTask.Exception}");
                            yield break;
                        }
                        Debug.Log("Successfully accepted friend request");
                    }
                    friendRequestList.Remove(childSnapshot.Key);
                    friendList.Add(childSnapshot.Key);
                    LoadFriends();
                }
            }
        }
    }

    private IEnumerator UpdateStatus(bool online)
    {
     //   Debug.Log("Updating status!!!");
     //   var GetInstancesTask = DBreference.Child("users").Child(user.UserId).Child("instances").GetValueAsync();
     //   yield return new WaitUntil(predicate: () => GetInstancesTask.IsCompleted);
     //   if (GetInstancesTask.Exception != null)
     //   {
     //       Debug.LogWarning(message: $"Failed to register task with {GetInstancesTask.Exception}");
     //   }
     //   else
     //   {
     //       totalInstances = int.Parse(GetInstancesTask.Result.Value.ToString());
        int statusUpdate = online ? 1 : -1;
        totalInstances =  Mathf.Max(0, totalInstances + statusUpdate);
        var UpdateInstancesTask = DBreference.Child("users").Child(user.UserId).Child("instances").SetValueAsync(Mathf.Max(0, totalInstances));
        yield return new WaitUntil(predicate: () => UpdateInstancesTask.IsCompleted);
        if (UpdateInstancesTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {UpdateInstancesTask.Exception}");
        }
        else
        {
            Debug.Log("Updated instances!");
        }
            //if (online)
            //{
            //    var DBTask = DBreference.Child("users").Child(user.UserId).Child("status").SetValueAsync("Online");
            //    yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
            //    if (DBTask.Exception != null)
            //    {
            //        Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            //    }
            //    else
            //    {
            //        Debug.Log("Set to Online!");
            //    }            
            //}
            //else
            //{
            //    if (totalInstances == 0)
            //    {
            //        var DBTask = DBreference.Child("users").Child(user.UserId).Child("status").SetValueAsync("Offline");
            //        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
            //        if (DBTask.Exception != null)
            //        {
            //            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            //        }
            //        else
            //        {
            //            Debug.Log("Set to Offline!");
            //        }
            //    }
            //}
            //LoadFriends();
        //}
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

    public void LoadFriends()
    {
        foreach (Transform child in MenuUIManager.instance.friendsContent.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (string friendReq in friendRequestList)
        {
            StartCoroutine(LoadFriendReqData(friendReq));
        }
        foreach (string friend in friendList)
        {
            StartCoroutine(LoadFriendData(friend));
        }
    }

    private void OnApplicationQuit()
    {

        Debug.Log("Application closing...");
        if (inMatch)
        {
            Debug.Log("Update on application quit");
            UpdateInMatch(false);
        }
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
       // StartCoroutine(UpdateStatus(false));
       // if (inMatch)
       // {
       //     Debug.Log("pause");
       //     //UpdateInMatch(false);
       // }
    }

    private void OnApplicationFocus(bool focus)
    {
        StartCoroutine(UpdateStatus(focus));
        if (inMatch)
            UpdateInMatch(focus);
    }

    public void SubmitFriendRequest()
    {
        AudioManager.instance.ButtonPress();
        if (friendNameInput.text.Length < 4)
        {
            addFriendMessage.text = "Name is too short";
            return;
        }
        StartCoroutine(FriendRequestTask(friendNameInput.text));
    }

    private IEnumerator FriendRequestTask(string username)
    {
        var DBTask = DBreference.Child("users").OrderByChild("username").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;
            foreach (DataSnapshot childSnapshot in snapshot.Children)
            {
                if (username == childSnapshot.Child("username").GetValue(false).ToString())
                {
                    Debug.Log("Name exists in database!");
                    if (friendList.Contains(childSnapshot.Key))
                    {
                        Debug.Log("Already friends");
                        addFriendMessage.text = "You are already friends";
                        yield break;
                    }
                    else
                    {
                        foreach (DataSnapshot friend in childSnapshot.Child("friends").Children)
                        {
                            if (friend.Key == user.UserId && !(bool)friend.Value)
                            {
                                Debug.Log("User has not accepted your friend request yet");
                                addFriendMessage.text = "User has yet to accept your friend request";
                                yield break;
                            }
                        }
                        Debug.Log("Not friends. Sending request now");
                        addFriendMessage.text = "Sending friend request...";
                        var friendRequestTask = DBreference.Child("users").Child(childSnapshot.Key).Child("friends").Child(user.UserId).SetValueAsync(false);
                        yield return new WaitUntil(predicate: () => friendRequestTask.IsCompleted);
                        if (friendRequestTask.Exception != null)
                        {
                            loading = false;
                            Debug.LogWarning(message: $"Failed to register task with {friendRequestTask.Exception}");
                        }
                        Debug.Log("Friend request has been sent");
                        addFriendMessage.text = "Friend request sent";

                        var UpdateTask = DBreference.Child("users").Child(user.UserId).Child("friends").Child(childSnapshot.Key).SetValueAsync(true);
                        yield return new WaitUntil(predicate: () => UpdateTask.IsCompleted);
                        if (UpdateTask.Exception != null)
                        {
                            loading = false;
                            Debug.LogWarning(message: $"Failed to register task with {UpdateTask.Exception}");
                        }
                        Debug.Log("Added to own friends list");
                        yield break;
                    }
                }
                Debug.Log("Player does not exist");
                addFriendMessage.text = "Player does not exist";
            }
        }
    }
}
