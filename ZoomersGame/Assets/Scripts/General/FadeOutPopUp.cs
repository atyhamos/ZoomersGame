using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeOutPopUp : MonoBehaviour
{
    [SerializeField] private Text text;
    [SerializeField] private Image image;
    void Start()
    {
        Invoke("FadeOut", 2f);  
    }

    private void FadeOut()
    {
        text.CrossFadeAlpha(0, 1, false);
        image.CrossFadeAlpha(0, 1, false);
        StartCoroutine(Destroy());
    }

    private IEnumerator Destroy()
    {
        yield return new WaitForSeconds(1f);
        Destroy(this.gameObject);
    }
}
