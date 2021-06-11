using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameManager : MonoBehaviour
{
    public Text userName;
    private void Start()
    {
        userName.text = FirebaseManager.instance.user.DisplayName;
    }
}
