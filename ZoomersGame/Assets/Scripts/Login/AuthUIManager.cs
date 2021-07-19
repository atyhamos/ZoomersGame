using TMPro;
using UnityEngine;

public class AuthUIManager : MonoBehaviour
{
    public static AuthUIManager instance;

    [Header("References")]
    [SerializeField] private GameObject checkingForAccountUI;
    [SerializeField] private GameObject loginUI;
    [SerializeField] private GameObject registerUI;
    [SerializeField] private GameObject resetUI;
    [SerializeField] private GameObject verifyEmailUI;
    [SerializeField] private TMP_Text verifyEmailText;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    private void ClearUI()
    {
        FirebaseManager.instance.ClearOutputs();
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        resetUI.SetActive(false);
        verifyEmailUI.SetActive(false);
        checkingForAccountUI.SetActive(false);
    }

    public void LoginScreen()
    {
        AudioManager.instance.ButtonPress();
        ClearUI();
        loginUI.SetActive(true);
    }

    public void RegisterScreen()
    {
        AudioManager.instance.ButtonPress();
        ClearUI();
        registerUI.SetActive(true);
    }

    public void ResetScreen()
    {
        AudioManager.instance.ButtonPress();
        ClearUI();
        resetUI.SetActive(true);
    }

    public void Click()
    {
        AudioManager.instance.Click();
    }

    public void ButtonPress()
    {
        AudioManager.instance.ButtonPress();
    }

    public void AwaitVerification(bool _emailSent, string _email, string _output)
    {
        ClearUI();
        verifyEmailUI.SetActive(true);
        if (_emailSent)
        {
            verifyEmailText.text = $"A verification email has been sent to\n{_email}";
        }
        else
        {
            verifyEmailText.text = $"Email not sent: {_output}\nPlease verify {_email}";
        }
    }
}
