using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatMessage : MonoBehaviour
{

    public TMPro.TMP_Text MessageText { get; set; }

    

    void Awake()
    {
        MessageText = GetComponentInChildren<TMPro.TMP_Text>();
    }

    public void SetMessage(string username, string message)
    {
        MessageText.text =  username+": "+message;
    }
    
}
