using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayfabManager : MonoBehaviour
{
    [Header("UI")]
    public Text messageText;
    public static string playerName;
    public InputField emailInput;
    public InputField passwordInput;

    public void RegisterButton()
    {
        if (passwordInput.text.Length < 6)
        {
            messageText.text = "Password too short (<6 characters)";
            return;
        }
        var request = new RegisterPlayFabUserRequest
        {
            Email = emailInput.text,
            Password = passwordInput.text,
            RequireBothUsernameAndEmail = false
        };
        playerName = emailInput.text;
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);

    }

    void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        messageText.text = "Registered and logged in";
        SceneManager.LoadScene("Tutorial");
    }

    public void LoginButton()
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = emailInput.text,
            Password = passwordInput.text
        };
        playerName = emailInput.text;
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }

    void OnLoginSuccess(LoginResult result)
    {
        messageText.text = "Logged in!";
        Debug.Log("Successful login/account create!");
        SceneManager.LoadScene("Tutorial");
    }

    public void ResetPasswordButton()
    {
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = emailInput.text,
            TitleId = " FD4CB"
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordReset, OnError);
    }

    void OnPasswordReset(SendAccountRecoveryEmailResult result)
    {
        messageText.text = "Password reset mail sent!";
    }
    void OnError(PlayFabError error)
    {
        messageText.text = error.ErrorMessage;
        Debug.Log(error.GenerateErrorReport());
    }
 //  // Start is called before the first frame update
 //  void Start()
 //  {
 //      Login();
 //  }
 //
 //  void Login()
 //  {
 //      var request = new LoginWithCustomIDRequest
 //      {
 //          CustomId = SystemInfo.deviceUniqueIdentifier,
 //          CreateAccount = true
 //      };
 //      PlayFabClientAPI.LoginWithCustomID(request, OnSuccess, OnError);
 //  }
 //
 //  void OnSuccess(LoginResult result)
 //  {
 //  }


}

