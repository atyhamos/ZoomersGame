using UnityEngine.Audio;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager instance;

    public Sound currentBgm;

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

    private void Start()
    {
        currentBgm = Play("Main Menu");
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
        return s;
    }

    public void Click()
    {
        Debug.Log("clicked!");
        Play("Click");
    }

    public void ButtonPress()
    {
        Debug.Log("button press!");
        Play("Button Press");
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
    }

    public void Main()
    {
        Clear();
        currentBgm = Play("Main Menu");
    }

    public void Tutorial()
    {
        Clear();
        currentBgm = Play("Tutorial");
    }

    public void Race()
    {
        Clear();
        currentBgm = Play("Race");
    }

    public void GameWin()
    {
        Clear();
        currentBgm = Play("Game Win");
    }
}
