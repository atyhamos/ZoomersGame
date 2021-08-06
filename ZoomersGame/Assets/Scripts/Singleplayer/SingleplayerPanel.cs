using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SingleplayerPanel : MonoBehaviour
{
    [SerializeField] private GameObject PopUpPanel;
    [SerializeField] private Text time;
    private bool collided;

    // Start is called before the first frame update
    void Start()
    {
        collided = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collided)
            return;
        else if (collision.CompareTag("Player"))
        {
            time.text = $"Finished!\nYour Time: {collision.gameObject.GetComponent<Timer>().GiveTime()}\nCurrent Best: {PlayerData.instance.bestTime}"; 
            PopUpPanel.SetActive(true);
        }
    }
}

