using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.Threading;
using System.Text;

public class SocketUdpController : MonoBehaviour
{
    public string ServerIp;
    public int ServerPort;

    private UdpClient udpClient;
    private IPEndPoint ServerEndPoint;
    private bool isListening = false;
    private Coroutine ListenCoroutine;
    private bool connected;
    private Dictionary<string, List<Action<string>>> EventsActions;



    public string Id { get; set; }

    void Awake()
    {
        try
        {
            ServerEndPoint = new IPEndPoint(IPAddress.Parse(ServerIp), ServerPort);

        }
        catch (Exception err)
        {

            Debug.LogError(err.Message);
        }

    }


    public void Init(object data)
    {
        StartClient(data);
    }
    public void Init()
    {
        StartClient(null);
    }

    private void StartClient(object data)
    {
        if (ServerEndPoint != null)
        {
            EventsActions = new Dictionary<string, List<Action<string>>>();
            udpClient = new UdpClient();
            udpClient.Connect(ServerEndPoint);

            isListening = true;
            ListenCoroutine = StartCoroutine("Listen");

            data = data == null ? new object() : data;
            Emit("connection", data);
            StartHeartbeat();
        }
        else
        {
            Debug.LogError("Can't start server. The server's IPEndPoint has not been inicialized ");
        }

    }

    private void StartHeartbeat()
    {
        var timer = new Timer((e) => SendHeartbeat(), null, 0, 1000);
    }

    public void SendHeartbeat()
    {
        Emit("Heartbeat_check_connection", "");
    }
    public IEnumerator Listen()
    {
        while (isListening)
        {
            var receiveTask = udpClient.ReceiveAsync();
            yield return new WaitUntil(() => receiveTask.IsCompleted);

            var receivedData = receiveTask.Result.Buffer;
            HandleMessage(receivedData);
        }
        
    }
    private void HandleMessage(byte[] serializedMessage)
    {
        Message receivedMessage = DeserializeMessage(serializedMessage);
        switch (receivedMessage.Header)
        {
            case "connect":
                Id = ((string)receivedMessage.Data);
                connected = true;
                RunActions(receivedMessage);
                break;
            case "disconnect":
                connected = false;
                isListening = false;
                StopCoroutine("Listen");
                udpClient.Close();
                RunActions(receivedMessage);
                break;
            default:
                if (!connected)
                {
                    break;
                }

                RunActions(receivedMessage);
                break;
        }

    }
    private void RunActions(Message receivedMessage)
    {
        if (EventsActions.ContainsKey(receivedMessage.Header))
        {
            foreach (var action in EventsActions[receivedMessage.Header])
            {
                action(receivedMessage.Data);
            }
        }
    }

    public void On(string name, Action<string> action)
    {
        if (!EventsActions.ContainsKey(name))
        {
            EventsActions.Add(name, (new List<Action<string>> { action }));
        }
        else
        {
            EventsActions[name].Add(action);
        }
    }

    public void Emit(string header, object data)
    {
        Send(header, data);
    }

    public void Broadcast(string header, string data)
    {
        Broadcast(header, data);
    }

    private void Send(string header, object data)
    {
        byte[] serializedMessage = SerializeMessage(header, data);
        udpClient.Send(serializedMessage, serializedMessage.Length);
    }

    private static byte[] SerializeMessage(string header, object data)
    {
       
        string json = JsonUtility.ToJson(data);
        byte[] serializedMessage = Encoding.ASCII.GetBytes(header + "|" + json);

        return serializedMessage;
    }
    internal static Message DeserializeMessage(byte[] serializedMessage)
    {
        try
        {
            string receivedData = Encoding.ASCII.GetString(serializedMessage);
            string head = receivedData.Split('|')[0];
            string json = receivedData.Split('|')[1];

            //object data = JsonUtility.FromJson<object>(json);

            return new Message { Header = head, Data = json  };

        }
        catch (Exception e)
        {

            return new Message { Header = "ERROR", Data = "Error Derilizing Data. \n" + e.Message };
        }

    }
}
internal class Message
{
    public string Header { get; set; }
    public string Data { get; set; }
}
