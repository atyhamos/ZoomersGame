using Proyecto26;
using FullSerializer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class LoginManager : MonoBehaviour
{
    public Text scoreText;
    public Text messageText;
    public InputField getScoreText;
    public InputField emailText;
    public InputField usernameText;
    public InputField passwordText;
    private System.Random random = new System.Random();

    public static int playerScore;
    public static string playerName;

    User user = new User();

    private string idToken;
    public static string localId;

    private string getLocalId;

    private string databaseURL = "https://zoomers-project-default-rtdb.asia-southeast1.firebasedatabase.app/users";
    private string AuthKey = "AIzaSyAc9KFxLaLMgW8iAXvXeJ7uHVPKjGreA1k";
    
    public static fsSerializer serializer = new fsSerializer();

    // Start is called before the first frame update
    void Start()
    {
        playerScore = random.Next(0, 101);
        scoreText.text = "Score: " + playerScore;
    }

    public void OnSubmit()
    {
        PostToDatabase();
    }

    public void OnGetScore()
    {
        GetLocalId();
    }

    private void UpdateScore()
    {
        scoreText.text = "Score: " + user.userScore;
    }

    private void PostToDatabase(bool emptyScore = false, string idTokenTemp = "")
    {
        if (idTokenTemp == "")
        {
            idTokenTemp = idToken;
        }
        User user = new User();
        if (emptyScore)
        {
            user.userScore = 0;
        }
        RestClient.Put(databaseURL + "/" + localId + ".json?auth=" + idTokenTemp, user);
    }

    private void RetrieveFromDatabase()
    {
        RestClient.Get<User>(databaseURL + "/" + getLocalId + ".json?auth=" + idToken).Then(response =>
        {
            user = response;
            UpdateScore();
        });
    }

   private void SignUpUser(string email, string username, string password)
   {
       string userData = "{\"email\":\"" + email + "\",\"password\":\"" + password + "\",\"returnSecureToken\":true}";
       RestClient.Post<SignInResponse>("https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key=" + AuthKey, userData).Then(
           response =>
           {
//             idToken = response.idToken; // a pass for authenticated users
               string emailVerification = "{\"requestType\":\"VERIFY_EMAIL\",\"idToken\":\"" + response.idToken + "\"}";
               RestClient.Post("https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key=" + AuthKey, emailVerification);
               localId = response.localId; // unique identifier for a user
               playerName = username;
               PostToDatabase(true, response.idToken);
           }).Catch(error =>
           {
               Debug.Log(error);
           });
   }
    public void ChangeSceneRegister()
    {
        for (int i = 4; i < transform.childCount; i++)
        {
            if (i == 7 || i == 8)
            {
                continue;
            }
            transform.GetChild(i).gameObject.SetActive(!transform.GetChild(i).gameObject.activeSelf);
        }
    }

    public void ChangeSceneReset()
    {
        for (int i = 3; i < 9; i++)
        {
            transform.GetChild(i).gameObject.SetActive(!transform.GetChild(i).gameObject.activeSelf);
        }
    }

    public void SignUpUserButton()
    {
        if (usernameText.text.Length < 3)
        {
            messageText.text = "Username must be longer than 3 characters!";
            return;
        }
        if (passwordText.text.Length < 7)
        {
            messageText.text = "Password must be longer than 6 characters!";
            return;
        }
        messageText.text = "Registered successfully! \nPlease check your email for verification before logging in.";
        SignUpUser(emailText.text, usernameText.text, passwordText.text);
    }
    public void SignInUserButton()
    {
        SignInUser(emailText.text, passwordText.text);
    }

    public void ResetPasswordButton()
    {
        ResetPassword(emailText.text);
    }

    private void ResetPassword(string email)
    {
        // check if email is listed in current users
        string passwordReset = "{\"requestType\":\"PASSWORD_RESET\",\"email\":\"" + email + "\"}";
        RestClient.Post("https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key=" + AuthKey, passwordReset);
        messageText.text = "Instructions on how to reset your password has been sent to your inbox.";

    }
    private void SignInUser(string email, string password)
    {
        string userData = "{\"email\":\"" + email + "\",\"password\":\"" + password + "\",\"returnSecureToken\":true}";
        RestClient.Post<SignInResponse>("https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyPassword?key=" + AuthKey, userData).Then(
            response =>
            {
                string emailVerification = "{\"idToken\":\"" + response.idToken + "\"}";
                RestClient.Post("https://identitytoolkit.googleapis.com/v1/accounts:lookup?key=" + AuthKey, emailVerification)
                .Then(emailResponse =>
                {
                    fsData emailConfirmationData = fsJsonParser.Parse(emailResponse.Text);
                    EmailConfirmationInfo emailConfirmationInfo = new EmailConfirmationInfo();
                    serializer.TryDeserialize(emailConfirmationData, ref emailConfirmationInfo).AssertSuccessWithoutWarnings();
                    if (emailConfirmationInfo.users[0].emailVerified)
                    {
                        idToken = response.idToken;
                        localId = response.localId;
                        GetUsername();
                        messageText.text = "Successful login!";
                        Debug.Log("Successful login!");
                        SceneManager.LoadScene("MainMenu");
                        Screen.autorotateToPortrait = false;
                        Screen.autorotateToLandscapeLeft = true;
                        Screen.orientation = ScreenOrientation.LandscapeLeft;
                    }
                    else
                    {
                        messageText.text = "Email not verified.";
                        Debug.Log("Email not verified");
                    }
                });
            }).Catch(error =>
            {
                messageText.text = "Incorrect email or password.";
                Debug.Log("Incorrent email or password.");
                return;
            });
    }



    private void GetUsername()
    {
        RestClient.Get<User>(databaseURL + "/" + localId + ".json?auth=" + idToken).Then(response =>
        {
            playerName = response.userName;
        });
    }

    private void GetLocalId()
    {
        RestClient.Get(databaseURL + ".json?auth=" + idToken).Then(response =>
        {
            var username = getScoreText.text;

            fsData userData = fsJsonParser.Parse(response.Text);
            Dictionary<string, User> users = null;

            serializer.TryDeserialize(userData, ref users);

            foreach (var user in users.Values)
            {
                if (user.userName == username)
                {
                    getLocalId = user.localId;
                    RetrieveFromDatabase();
                    break;
                }
            }
        });
    }
}
