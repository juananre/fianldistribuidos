using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class SocketUdpManager : MonoBehaviour
{
    SocketUdpController Socket;

    public event Action<string> onConnectedToServer;
    public event Action<string,string> onChatMessage;



    public string Username;


    // Start is called before the first frame update

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        Socket = GameObject.Find("SocketUdpController").GetComponent<SocketUdpController>();

    }

    public void ConnectToServer()
    {
        Socket.Init(new MessageData { Username= Username});

        Socket.On("connect", onConnected); ;
        Socket.On("disconnect", Disconnect);
        Socket.On("chat", ChatMessage);

        
    }
    private void onConnected(string data)
    {
        Debug.Log("conexion exitosa " + Socket.Id);
        onConnectedToServer?.Invoke(data);
    }

    private void ChatMessage(string data)
    {
       MessageData message = JsonUtility.FromJson<MessageData>(data); 
       //Console.WriteLine("El cliente de id " + message.Id + "dice:\n" + message.Message);
       onChatMessage?.Invoke(message.Username, message.Message);
    }

    private void Disconnect(string msg)
    {
        Console.WriteLine("desconexion del servidor " + msg);
    }
    public void SendChatMessage(string msg)
    {
        msg = GameObject.Find("InputFieldChat").GetComponent<TMPro.TMP_InputField>().text;

        Socket.Emit("chat", new MessageData {Message = msg });
    }

    public void SetUsername(string username)
    {
        Username = username;
    }
   
}
[System.Serializable]
public class MessageData
{
    public string Id;
    public string Username;
    public string Message;
}

