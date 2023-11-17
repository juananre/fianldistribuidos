using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatController : MonoBehaviour
{

    [SerializeField]
    private GameObject PanelConnect;
    [SerializeField]
    private GameObject PanelChat;

    [SerializeField] 
    private ScrollRect ScrollViewMessages;
    [SerializeField]
    private Transform ScrollViewContent;

    [SerializeField] 
    private TMPro.TMP_InputField InputFieldChat;

    [SerializeField]
    private SocketUdpManager socketUdpManager;

    [SerializeField]
    private GameObject ChatMessagePrefab;

    private List<ChatMessage> ChatMessagesList = new List<ChatMessage>();


    void Start()
    {
        GameObject.Find("Canvas").SetActive(true);
        PanelConnect = GameObject.Find("PanelConnect");
        PanelConnect.SetActive(true);



        socketUdpManager = GetComponent<SocketUdpManager>();
        socketUdpManager.onChatMessage += onChatMessage;
        socketUdpManager.onConnectedToServer += OnConnectedToServer;

        ChatMessagesList = new List<ChatMessage>();
    }

    private void OnConnectedToServer(string data)
    {
        PanelConnect.SetActive(false);
        PanelChat.SetActive(true);
    }

    private void onChatMessage(string username, string message)
    {
        Debug.Log(username + ": " + message);
        NewChatMessage(username, message);

    }

    private void NewChatMessage(string username, string message)
    {
        GameObject chatMessageGameObject = Instantiate(ChatMessagePrefab,ScrollViewContent);
        ChatMessage chatMessage = chatMessageGameObject.GetComponent<ChatMessage>();
        chatMessage.SetMessage(username, message);

        ChatMessagesList.Add(chatMessage);
        chatMessageGameObject.transform.localPosition = new Vector3(10, -25 * (ChatMessagesList.Count));
    }

    public void SendChatMessage()
    {
        string message = InputFieldChat.text;
        socketUdpManager.SendChatMessage(message);
        NewChatMessage(socketUdpManager.Username, message);

    }


}
