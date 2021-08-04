using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;

public class FirebaseAutoLogin : MonoBehaviour
{

    public static FirebaseAutoLogin instance;

    [Header("Firebase")]
    public FirebaseAuth auth;
    public FirebaseUser user;
    public DatabaseReference DBreference;


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance == null)
            instance = this;
        else if (instance != this)
        { 
            Destroy(instance.gameObject);
            instance = this;
        }
    }

    private IEnumerator CheckAndFixDependencies()
    {
        var checkAndFixDependenciesTask = FirebaseApp.CheckAndFixDependenciesAsync();

        yield return new WaitUntil(predicate: () => checkAndFixDependenciesTask.IsCompleted);

        var dependencyResult = checkAndFixDependenciesTask.Result;

        if (dependencyResult == DependencyStatus.Available)
            InitializeFirebase();
        else
        {
            Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyResult}");
        }
    }

    private void Start()
    {
        StartCoroutine(CheckAndFixDependencies());
    }

    private IEnumerator CheckAutoLogin()
    {
        yield return new WaitForEndOfFrame();
        if (user != null)
        {
            var reloadUserTask = user.ReloadAsync();

            yield return new WaitUntil(predicate: () => reloadUserTask.IsCompleted);

            AutoLogin();
        }
        else
            GameManager.instance.ChangeScene(1);
    }

    private void AutoLogin()
    {
        if (user != null)
        {
            if (user.IsEmailVerified)
            {
                // Move to Loading page                
                StartCoroutine(UpdateLogin());
                GameManager.instance.ChangeScene(2);
            }
            else
            {
                GameManager.instance.ChangeScene(1);
                StartCoroutine(SendVerificationEmail());
            }
        }
        else
            GameManager.instance.ChangeScene(1);
    }

    private IEnumerator UpdateLogin()
    {
        var DBTask = DBreference.Child("last login time").SetValueAsync(System.DateTime.Now.DayOfYear);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        if (DBTask.Exception != null)
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        else
            Debug.Log("Updated login time");
        var GiftTimeTask = DBreference.Child("tomorrow time").GetValueAsync();
        yield return new WaitUntil(predicate: () => GiftTimeTask.IsCompleted);
        if (GiftTimeTask.Exception != null)
            Debug.LogWarning(message: $"Failed to register task with {GiftTimeTask.Exception}");
        else
        {
            bool updated = false;
            Debug.Log("Retrieved tomorrow gift refresh time");
            int tomorrowTime = int.Parse(GiftTimeTask.Result.Value.ToString());
            while (tomorrowTime - System.DateTime.Now.DayOfYear <= 0)
            {
                tomorrowTime += 1;
                updated = true;
            }
            if (updated)
            {
                DBTask = DBreference.Child("tomorrow time").SetValueAsync(tomorrowTime);
                yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
                if (DBTask.Exception != null)
                    Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
                else
                    Debug.Log("Updated tomorrow gift refresh time");
                StartCoroutine(ResetGifts());
            }
        }
        var LeaderboardTimeTask = DBreference.Child("refresh leaderboard time").GetValueAsync();
        yield return new WaitUntil(predicate: () => LeaderboardTimeTask.IsCompleted);
        if (LeaderboardTimeTask.Exception != null)
            Debug.LogWarning(message: $"Failed to register task with {LeaderboardTimeTask.Exception}");
        else
        {
            bool updated = false;
            Debug.Log("Retrieved leaderboard refresh time");
            int refreshTime = int.Parse(LeaderboardTimeTask.Result.Value.ToString());
            while (refreshTime - System.DateTime.Now.DayOfYear <= 0)
            {
                refreshTime += 7;
                updated = true;
            }
            if (updated)
            {
                DBTask = DBreference.Child("refresh leaderboard time").SetValueAsync(refreshTime);
                yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
                if (DBTask.Exception != null)
                    Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
                else
                    Debug.Log("Updated leaderboard refresh time");
                StartCoroutine(ResetLeaderboard());
            }
        }
    }

    private IEnumerator ResetGifts()
    {
        var DBTask = DBreference.Child("users").GetValueAsync();

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
                DBreference.Child("users").Child(childSnapshot.Key).Child("daily gift").SetValueAsync(true);
            }
        }
    }

    private IEnumerator ResetLeaderboard()
    {
        var DBTask = DBreference.Child("users").OrderByChild("weekly singleplayer raw").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;
            int counter = 10;
            foreach (DataSnapshot childSnapshot in snapshot.Children)
            {
                if (childSnapshot.Child("weekly singleplayer raw").Value == null)
                    continue;
                else
                {
                    if (counter > 0)
                    {
                        // Get coins based off your position
                        DBreference.Child("users").Child(childSnapshot.Key).Child("coins").SetValueAsync(int.Parse(childSnapshot.Child("coins").Value.ToString()) + counter * 20);
                        counter--;
                    }
                }
                DBreference.Child("users").Child(childSnapshot.Key).Child("weekly singleplayer raw").RemoveValueAsync();
                DBreference.Child("users").Child(childSnapshot.Key).Child("weekly singleplayer formatted").RemoveValueAsync();
            }
        }
    }

    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        StartCoroutine(CheckAutoLogin());
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
        DBreference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    private void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log("Signed out");
            }

            user = auth.CurrentUser;

            if (signedIn)
                Debug.Log($"Signed in: {user.DisplayName}");
        }
    }

    private IEnumerator SendVerificationEmail()
    {
        if (user != null)
        {
            var emailTask = user.SendEmailVerificationAsync();

            yield return new WaitUntil(predicate: () => emailTask.IsCompleted);

            if (emailTask.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException)emailTask.Exception.GetBaseException();
                AuthError error = (AuthError)firebaseException.ErrorCode;

                string output = "Unknown error, please try again.";

                switch (error)
                {
                    case AuthError.Cancelled:
                        output = "Verification task was cancelled";
                        break;
                    case AuthError.InvalidRecipientEmail:
                        output = "Invalid email";
                        break;
                    case AuthError.TooManyRequests:
                        output = "Too many requests";
                        break;
                }
                AuthUIManager.instance.AwaitVerification(false, user.Email, output);
            }
            else
            {
                AuthUIManager.instance.AwaitVerification(true, user.Email, null);
                Debug.Log("Email sent successfully");
            }
        }
    }
}
