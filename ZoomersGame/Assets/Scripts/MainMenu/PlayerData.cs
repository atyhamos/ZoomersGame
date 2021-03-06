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
    public List<string> friendList, friendRequestList, friendNameList;
    public TMP_InputField friendNameInput;
    public Text addFriendMessage;
    [SerializeField] private GameObject scoreElement;
    [SerializeField] private GameObject friendElement;
    [SerializeField] private GameObject invitation;
    public FirebaseUser user;
    public DatabaseReference DBreference;
    public int totalInstances;
    public bool inviteRequest = false;

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
        InvokeRepeating("CheckInvites", 0f, 5f);
        InvokeRepeating("InMatch", 5f, 10f);
    }

    private void CheckInvites()
    {
        if (inviteRequest)
            return;
        else
        {
        if (!inMatch)
            StartCoroutine(CheckInvitesDatabase());
        }
    }



    private IEnumerator CheckInvitesDatabase()
    {
        var DBTask = DBreference.Child("users").Child(user.UserId).Child("invites").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            loading = false;
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            if (DBTask.Result.Value == null)
            {
                Debug.Log("No invites");
                yield break;
            }
            else
            {
                foreach (DataSnapshot child in DBTask.Result.Children)
                {
                    string inviteUser = child.Key;
                    string inviteCode = child.Value.ToString();
                    GameObject invite = Instantiate(invitation, GameObject.Find("Canvas").transform);
                    invite.GetComponent<Invitation>().NewInvitation(inviteUser, inviteCode);
                    Debug.Log("Got an invite!");
                    inviteRequest = true;
                    yield break;
                }
            }
        }
    }


    public void InvitationAction(string username, string code, bool accepted)
    {
        StartCoroutine(InvitationActionDB(username, code, accepted));
    }

    private IEnumerator InvitationActionDB(string username, string code, bool accepted)
    {
        if (accepted)
        {
            CreateAndJoinRoom.instance.JoinOrCreateRoom(code);
        }
        var DBTask = DBreference.Child("users").Child(user.UserId).Child("invites").Child(username).RemoveValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            loading = false;
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            Debug.Log("Removed invitation");
        }
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
        else if (DBTask.Result.Value == null)
        {
            Debug.Log("User does not exist");
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
            friendList.Clear();
            friendRequestList.Clear();
            foreach (DataSnapshot child in snapshot.Child("friends").Children)
            {
                // is a friend
                if ((bool)child.Value)
                    StartCoroutine(CheckMutualFriends(child.Key));
                else
                    friendRequestList.Add(child.Key);
            }
            loading = false;
            yield return new WaitForSeconds(2f);
            if (!inMatch)
                LoadFriends();
            else
                LoadFriendsInMatch(FriendsUIManager.instance.friendsContent);
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
                    if (!friendList.Contains(userId))
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
            if (!friendNameList.Contains(username))
                friendNameList.Add(username);
            GameObject friendlistElement = Instantiate(friendElement, FriendsUIManager.instance.friendsContent.transform);
            friendlistElement.GetComponent<FriendElement>().NewFriendElement(username, instances == 0 ? "Offline" : "Online", userId);
        }
    }

    private IEnumerator LoadFriendDataInMatch(string userId, Transform content)
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
            bool inMatch = bool.Parse(snapshot.Child("in match").Value.ToString());
            GameObject friendlistElement = Instantiate(friendElement, content.transform);
            if (instances == 0)
            {
                friendlistElement.GetComponent<FriendElement>().NewFriendElementInMatch(username, "Offline", userId);
                yield break;
            }
            if (!inMatch)
            {
                friendlistElement.GetComponent<FriendElement>().NewFriendElementInMatch(username, "Online", userId);
                yield break;
            }
            else
            {
                friendlistElement.GetComponent<FriendElement>().NewFriendElement(username, "In Match", userId);
                yield break;
            }
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
            GameObject friendlistElement = Instantiate(friendElement, MenuUIManager.instance.GetComponent<FriendsUIManager>().friendsContent.transform);
            friendlistElement.GetComponent<FriendElement>().NewFriendReqElement(username, instances == 0 ? "Offline" : "Online", userId);
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
                        Debug.Log(_username);
                        friendRequestList.Remove(childSnapshot.Key);
                        friendNameList.Remove(_username);
                        LoadFriends();
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
                        friendList.Add(childSnapshot.Key);
                        friendRequestList.Remove(childSnapshot.Key);
                        friendNameList.Remove(_username);
                        LoadFriends();
                    }
                }
            }
        }
    }

    private IEnumerator UpdateStatus(bool online)
    {
        int statusUpdate = online ? 1 : -1;
        totalInstances = Mathf.Max(0, totalInstances + statusUpdate);
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
    }

    public void InviteFriend(string username, string code)
    {
        StartCoroutine(InviteFriendDatabase(username, code));
    }

    private IEnumerator InviteFriendDatabase(string username, string code)
    {
        var DBTask = DBreference.Child("users").Child(username).Child("invites").Child(user.DisplayName).SetValueAsync(code);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            Debug.Log("Request sent!");

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
        foreach (Transform child in FriendsUIManager.instance.friendsContent.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (string friendReq in friendRequestList)
        {
            Debug.Log("Loading friend requests...");
            StartCoroutine(LoadFriendReqData(friendReq));
        }
        foreach (string friend in friendList)
        {
            Debug.Log("Loading friends...");
            StartCoroutine(LoadFriendData(friend));
        }
    }

    public void LoadFriendsInMatch(Transform content)
    {
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (string friend in friendList)
        {
            StartCoroutine(LoadFriendDataInMatch(friend, content));
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

    private void OnApplicationFocus(bool focus)
    {
        StartCoroutine(UpdateStatus(focus));
        if (inMatch)
            UpdateInMatch(focus);
    }

    public void SubmitFriendRequest()
    {
        if (addFriendMessage == null)
        {
            friendNameInput = FriendsUIManager.instance.friendNameInput;
            addFriendMessage = FriendsUIManager.instance.addFriendMessage; 
        }
        AudioManager.instance.ButtonPress();
        if (friendNameInput.text.Length < 4)
        {
            addFriendMessage.text = "Name is too short";
            return;
        }
        else if (friendNameInput.text == user.DisplayName)
        {
            addFriendMessage.text = "You cannot add yourself";
            return;
        }
        StartCoroutine(FriendRequestTask(friendNameInput.text));
    }

    public void SubmitFriendRequest(string username)
    {
        AudioManager.instance.ButtonPress();
        StartCoroutine(FriendRequestTask(username));
    }


    private IEnumerator FriendRequestTask(string username)
    {
        if (addFriendMessage == null)
            addFriendMessage = FriendsUIManager.instance.addFriendMessage;
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
