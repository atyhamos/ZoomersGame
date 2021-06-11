using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    public void ChangeScene(int _sceneIndex)
    {
        if (_sceneIndex > 0)
        {
            Screen.autorotateToPortrait = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }
        else if (_sceneIndex == 0)
        {
            Screen.autorotateToPortrait = true;
            Screen.autorotateToLandscapeLeft = false;
            Screen.orientation = ScreenOrientation.Portrait;
        }
        SceneManager.LoadSceneAsync(_sceneIndex);
    }
}
