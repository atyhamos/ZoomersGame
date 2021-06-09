using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuControl : MonoBehaviour
{
    public Text welcomeMessage;
    // Start is called before the first frame update
    void Start()
    {
        welcomeMessage.text += LoginManager.playerName + "!";
    }

    public void Tutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void LogOut()
    {
        SceneManager.LoadScene("Login");
    }
    
    public void Home()
    {
        SceneManager.LoadScene("MainMenu");
    }

}
