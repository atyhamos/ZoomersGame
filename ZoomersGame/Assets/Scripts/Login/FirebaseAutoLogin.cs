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
                // Move to Loading page                
                GameManager.instance.ChangeScene(2);

            else
            {
                GameManager.instance.ChangeScene(1);
                StartCoroutine(SendVerificationEmail());
            }
        }
        else
            GameManager.instance.ChangeScene(1);
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
