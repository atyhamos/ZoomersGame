using Proyecto26;
using FullSerializer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScores : MonoBehaviour
{
    public Text scoreText;
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

    private void PostToDatabase(bool emptyScore = false)
    {
        User user = new User();
        if (emptyScore)
        {
            user.userScore = 0;
        }
        RestClient.Put(databaseURL + "/" + localId + ".json?auth=" + idToken, user);
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
               idToken = response.idToken;
               localId = response.localId;
               playerName = username;
               PostToDatabase(true);
           }).Catch(error =>
           {
               Debug.Log(error);
           });
   }

    public void SignUpUserButton()
    {
        SignUpUser(emailText.text, usernameText.text, passwordText.text);
    }
    public void SignInUserButton()
    {
        SignInUser(emailText.text, passwordText.text);
    }
    private void SignInUser(string email, string password)
    {
        string userData = "{\"email\":\"" + email + "\",\"password\":\"" + password + "\",\"returnSecureToken\":true}";
        RestClient.Post<SignInResponse>("https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyPassword?key=" + AuthKey, userData).Then(
            response =>
            {
                idToken = response.idToken;
                localId = response.localId;
                GetUsername();
            }).Catch(error =>
            {
                Debug.Log(error);
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
