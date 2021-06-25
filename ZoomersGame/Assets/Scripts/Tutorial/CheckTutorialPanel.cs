using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckTutorialPanel : MonoBehaviour
{
    private bool hasPanelActive = false;
    private TutorialPanels tutorialPanel;

    public bool IsPanelActive()
    {
        return hasPanelActive;
    }

    public void SetPanelInactive()
    {
        hasPanelActive = false;
    }

    public void SetPanelActive(TutorialPanels tutorialPanel)
    {
        this.tutorialPanel = tutorialPanel;
        hasPanelActive = true;
    }

    public TutorialPanels GetPanel()
    {
        return tutorialPanel;
    }
}
