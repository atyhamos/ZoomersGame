using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPanels : MonoBehaviour
{
    [SerializeField] private GameObject PopUpPanel;
    [SerializeField] private GameObject Panel;
    [SerializeField] private GameObject Text;
    private CheckTutorialPanel checkActivePanel;
    private Animator PopupPanelAnim;
    private Animator PopupTextAnim;
    private bool collided;
    private Coroutine co;

    // Start is called before the first frame update
    void Start()
    {
        AudioManager.instance.Tutorial();
        collided = false;
        PopupPanelAnim = Panel.GetComponent<Animator>();
        PopupTextAnim = Text.GetComponent<Animator>();
    }
    public void DisplayText()
    {
        PopUpPanel.SetActive(true);
        checkActivePanel.SetPanelActive(this);
    }

    public IEnumerator HideText()
    {
        yield return new WaitForSeconds(8f);
        PopupPanelAnim.Play("SlideOutTutorial");
        PopupTextAnim.Play("SlideOutText");
        yield return new WaitForSeconds(2f);
        PopUpPanel.SetActive(false);
        checkActivePanel.SetPanelInactive();
    }

    public void ImmediatelyHide()
    {
        StopCoroutine(co);
        PopupPanelAnim.Play("SlideOutTutorial");
        PopupTextAnim.Play("SlideOutText");
        //    PopUpPanel.SetActive(false);
        StartCoroutine(DisablePanel(PopUpPanel));
        checkActivePanel.SetPanelInactive();
    }

    public IEnumerator DisablePanel(GameObject panel)
    {
        yield return new WaitForSeconds(2f);
        panel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collided)
            return;
        else if (collision.CompareTag("Player"))
        {
            checkActivePanel = collision.gameObject.GetComponent<CheckTutorialPanel>();
            if (checkActivePanel.IsPanelActive())
                // hide the current panel
                checkActivePanel.GetPanel().ImmediatelyHide();
            collided = true;
            DisplayText();
            co = StartCoroutine(HideText());
        }
    }
}
