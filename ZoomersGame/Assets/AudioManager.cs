using UnityEngine.Audio;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager instance;

    public Sound currentBgm;

    private bool bgmOn = true, soundEffectsOn = true;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public bool BGMOn()
    {
        return bgmOn;
    }

    public bool FXOn()
    {
        return soundEffectsOn;
    }
    private void Start()
    {
        currentBgm = Play("Main Menu");
    }

    public void ToggleBGM()
    {
        bgmOn = !bgmOn;
        if (!bgmOn)
            currentBgm.source.mute = true;
        else
            currentBgm.source.mute = false;
        Debug.Log("Toggle BGM to " + bgmOn.ToString());
    }

    public void ToggleFX()
    {
        soundEffectsOn = !soundEffectsOn;
        Debug.Log("Toggle FX to " + soundEffectsOn.ToString());

    }


    public Sound Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log("Sound: " + name + " not found!");
            return null;
        }
        s.source.Play();
        if (!soundEffectsOn)
            s.source.mute = true;
        else
            s.source.mute = false;
        return s;
    }

    public void Click()
    {
        Debug.Log("clicked!");
        if (soundEffectsOn)
            Play("Click");
    }

    public void ButtonPress()
    {
        Debug.Log("button press!");
        if (soundEffectsOn)
            Play("Button Press");
    }

    public void MenuOpen()
    {
        Debug.Log("open menu!");
        if (soundEffectsOn)
            Play("Menu Open");
    }

    public void MenuClose()
    {
        Debug.Log("close menu!");
        if (soundEffectsOn)
            Play("Menu Close");
    }

    public void Clear()
    {
        Debug.Log("stop music");
        currentBgm.source.Stop();
    }

    public void HoldingArea()
    {
        Clear();
        currentBgm = Play("Holding Area");
        if (!bgmOn)
            currentBgm.source.mute = true;
        else
            currentBgm.source.mute = false;
    }

    public void Main()
    {
        Clear();
        currentBgm = Play("Main Menu");
        if (!bgmOn)
            currentBgm.source.mute = true;
        else
            currentBgm.source.mute = false;
    }

    public void Tutorial()
    {
        Clear();
        currentBgm = Play("Tutorial");
        if (!bgmOn)
            currentBgm.source.mute = true;
        else
            currentBgm.source.mute = false;
    }

    public void Race()
    {
        Clear();
        currentBgm = Play("Race");
        if (!bgmOn)
            currentBgm.source.mute = true;
        else
            currentBgm.source.mute = false;
    }

    public void GameWin()
    {
        Clear();
        currentBgm = Play("Game Win");
        if (!bgmOn)
            currentBgm.source.mute = true;
        else
            currentBgm.source.mute = false;
    }
}
