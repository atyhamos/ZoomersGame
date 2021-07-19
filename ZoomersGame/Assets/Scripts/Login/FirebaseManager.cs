using UnityEngine;
using System.Collections;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;

    [Header("Firebase")]
    public FirebaseAuth auth;
    public FirebaseUser user;
    public DatabaseReference DBreference;
    [Space(5f)]

    [Header("Login References")]
    [SerializeField] private TMP_InputField loginEmail;
    [SerializeField] private TMP_InputField loginPassword;
    [SerializeField] private TMP_Text loginOutputText;
    [Space(5f)]

    [Header("Register References")]
    [SerializeField] private TMP_InputField registerUsername;
    [SerializeField] private TMP_InputField registerEmail;
    [SerializeField] private TMP_InputField registerPassword;
    [SerializeField] private TMP_InputField registerConfirmPassword;
    [SerializeField] private TMP_Text registerOutputText;
    private bool usernameChecked = false;
    private bool usernameInUse = false;
    [Space(5f)]

    [Header("Reset Password References")]
    [SerializeField] private TMP_InputField resetEmail;
    [SerializeField] private TMP_Text resetOutputText;



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
            AuthUIManager.instance.LoginScreen();    
    }

    private void AutoLogin()
    {
        if (user != null)
        {
            if (user.IsEmailVerified)
            // Move to Loading page                
                GameManager.instance.ChangeScene(2);
            
            else
                StartCoroutine(SendVerificationEmail());
        }
        else
            AuthUIManager.instance.LoginScreen();
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

    public void ClearOutputs()
    {
        loginOutputText.text = "";
        registerOutputText.text = "";
    }

    public void LoginButton()
    {
        AudioManager.instance.ButtonPress();
        StartCoroutine(LoginLogic(loginEmail.text, loginPassword.text));
    }

    public void RegisterButton()
    {
        AudioManager.instance.ButtonPress();
        StartCoroutine(RegisterLogic(registerUsername.text, registerEmail.text, registerPassword.text, registerConfirmPassword.text));
    }

    public void ResetButton()
    {
        AudioManager.instance.ButtonPress();
        StartCoroutine(ResetPasswordLogic(resetEmail.text));
    }

    private IEnumerator LoginLogic(string _email, string _password)
    {
        Credential credential = EmailAuthProvider.GetCredential(_email, _password);

        var loginTask = auth.SignInWithCredentialAsync(credential);

        yield return new WaitUntil(predicate: () => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            FirebaseException firebaseException = (FirebaseException)loginTask.Exception.GetBaseException();
            AuthError error = (AuthError)firebaseException.ErrorCode;
            string output = "Unknown error, please try again";

            switch (error)
            {
                case AuthError.MissingEmail:
                    output = "Please enter your email";
                    break;
                case AuthError.MissingPassword:
                    output = "Please enter your password";
                    break;
                case AuthError.InvalidEmail:
                    output = "Invalid email";
                    break;
                case AuthError.WrongPassword:
                    output = "Wrong password";
                    break;
                case AuthError.UserNotFound:
                    output = "Account does not exist";
                    break;
            }
            loginOutputText.text = output;
        }
        else
        {
            if (user.IsEmailVerified)
            { 
                GameManager.instance.ChangeScene(2);
                Debug.Log("Logging in!");
            }
            else
            {
                StartCoroutine(SendVerificationEmail());
            }
        }
    }

    private IEnumerator RegisterLogic(string _username, string _email, string _password, string _confirmPassword)
    {
        if (_username == "")
            registerOutputText.text = "Please enter a username";
        else if (_username.Length > 15)
            registerOutputText.text = "Username cannot exceed 15 characters";
        else
        {
            StartCoroutine(CheckUsername(_username));
            yield return new WaitUntil(predicate: () => usernameChecked);
            if (usernameInUse)
                registerOutputText.text = "Username is already in use";
            else if (_password != _confirmPassword)
                registerOutputText.text = "Passwords do not match";
            else
            {
                var registerTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);

                yield return new WaitUntil(predicate: () => registerTask.IsCompleted);

                if (registerTask.Exception != null)
                {
                    FirebaseException firebaseException = (FirebaseException)registerTask.Exception.GetBaseException();
                    AuthError error = (AuthError)firebaseException.ErrorCode;
                    string output = "Unknown error, please try again.";

                    switch (error)
                    {
                        case AuthError.MissingEmail:
                            output = "Please enter your email";
                            break;
                        case AuthError.MissingPassword:
                            output = "Please enter your password";
                            break;
                        case AuthError.InvalidEmail:
                            output = "Invalid email";
                            break;
                        case AuthError.EmailAlreadyInUse:
                            output = "Email already in use";
                            break;
                        case AuthError.WrongPassword:
                            output = "Wrong password";
                            break;
                        case AuthError.WeakPassword:
                            output = "Weak password";
                            break;
                    }
                    registerOutputText.text = output;
                }
                else
                {
                    UserProfile profile = new UserProfile
                    {
                        DisplayName = _username,
                    };
                    var defaultUserTask = user.UpdateUserProfileAsync(profile);

                    yield return new WaitUntil(predicate: () => defaultUserTask.IsCompleted);
                    if (defaultUserTask.Exception != null)
                    {
                        user.DeleteAsync();
                        FirebaseException firebaseException = (FirebaseException)defaultUserTask.Exception.GetBaseException();
                        AuthError error = (AuthError)firebaseException.ErrorCode;
                        string output = "Unknown error, please try again.";

                        switch (error)
                        {
                            case AuthError.Cancelled:
                                output = "Update user cancelled";
                                break;
                            case AuthError.SessionExpired:
                                output = "Session Expired";
                                break;
                        }
                        registerOutputText.text = output;
                    }
                    else
                    {
                        Debug.Log($"Firebase User Created Successfully: {user.DisplayName} ({user.UserId})");
                        StartCoroutine(SendVerificationEmail());
                        StartCoroutine(UpdateUserDatabase(_username));
                    }
                }
            }
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

    private IEnumerator ResetPasswordLogic(string _email)
    {
        var resetTask = auth.SendPasswordResetEmailAsync(_email);

        yield return new WaitUntil(predicate: () => resetTask.IsCompleted);

        if (resetTask.Exception != null)
        {
            FirebaseException firebaseException = (FirebaseException)resetTask.Exception.GetBaseException();
            AuthError error = (AuthError)firebaseException.ErrorCode;
            string output = "Unknown error, please try again";

            switch (error)
            {
                case AuthError.MissingEmail:
                    output = "Please enter your email";
                    break;
                case AuthError.InvalidEmail:
                    output = "Invalid email";
                    break;
            }
            resetOutputText.text = output;
        }
        else
        {
            resetOutputText.text = $"Instructions on how to reset your password has been sent to {_email}";
        }
    }

    private IEnumerator UpdateUserDatabase(string _username)
    {
        var DBTask = DBreference.Child("users").Child(user.UserId).Child("username").SetValueAsync(_username);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }

        DBTask = DBreference.Child("users").Child(user.UserId).Child("singleplayer formatted").SetValueAsync("Have not attempted");
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }

        DBTask = DBreference.Child("users").Child(user.UserId).Child("singleplayer raw").SetValueAsync(float.MaxValue);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }

        DBTask = DBreference.Child("users").Child(user.UserId).Child("in match").SetValueAsync(false);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
    }

    private IEnumerator UpdateBestTime(float _raw, string _formatted)
    {
        var DBTaskRaw = DBreference.Child("users").Child(user.UserId).Child("singleplayer raw").SetValueAsync(_raw);
        yield return new WaitUntil(predicate: () => DBTaskRaw.IsCompleted);
        if (DBTaskRaw.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTaskRaw.Exception}");
        }
        Debug.Log($"Raw time updated to {_raw}");
        var DBTaskFormat = DBreference.Child("users").Child(user.UserId).Child("singleplayer formatted").SetValueAsync(_formatted);
        yield return new WaitUntil(predicate: () => DBTaskFormat.IsCompleted);
        if (DBTaskFormat.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTaskFormat.Exception}");
        }
        Debug.Log($"Formatted time updated to {_formatted}");
    }


    private IEnumerator CheckUsername(string _username)
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
                if (_username == childSnapshot.Child("username").GetValue(false).ToString())
                {
                    usernameChecked = true;
                    usernameInUse = true;
                    yield return null;
                }
                else
                    continue;
            }
            usernameChecked = true;
            usernameInUse = false;
        }
    }
}
