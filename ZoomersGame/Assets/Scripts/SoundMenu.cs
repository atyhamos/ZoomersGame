using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundMenu : MonoBehaviour
{
    public Toggle bgmToggle, soundFXToggle;
    public GameObject menuUI;
    public bool menuOpen = false;

    // Start is called before the first frame update
    void Start()
    {
        if (!AudioManager.instance.BGMOn())
        {
            bgmToggle.isOn = false;
            AudioManager.instance.ToggleBGM();
        }
        if (!AudioManager.instance.FXOn())
        {
            soundFXToggle.isOn = false;
            AudioManager.instance.ToggleFX();
        }
    }

    public void ShowHideMenu()
    {
        if (menuOpen)
        {
            AudioManager.instance.MenuOpen();
            menuUI.SetActive(false);
        }
        else
        {
            AudioManager.instance.MenuClose();
            menuUI.SetActive(true);
        }
        menuOpen = !menuOpen;
    }

    public void ToggleBGM()
    {
        AudioManager.instance.ToggleBGM();
    }

    public void ToggleFX()
    {
        AudioManager.instance.ToggleFX();
    }
}
