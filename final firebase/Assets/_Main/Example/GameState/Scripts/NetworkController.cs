using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkController : MonoBehaviour
{
    public static NetworkController _Instance {  get; private set; }
    public SocketUdpController Socket { get; private set; }

    public event Action<string> onConnectedToServer;


    public string Username;


    // Start is called before the first frame update
    void Start()
    {
        _Instance = this;
        Socket = GameObject.Find("SocketUdpController").GetComponent<SocketUdpController>();
    }

    public void ConnectToServer()
    {

        Socket.Init(new JsonData { Username = Username });

        Socket.On("connect", onConnected);
        Socket.On("disconnect", Disconnect);

        Socket.On("welcome", (json) =>
        {
            JsonData jsonData = JsonUtility.FromJson<JsonData>(json);

            Debug.Log(jsonData.Message);
            foreach (var player in jsonData.State.Players)
            {
                Debug.Log(player.Username);
                
            }
            GameObject.Find("GameController").GetComponent<GameController>().StartGame(jsonData.State);                       

        });
        Socket.On("newPlayer", (json) => {
            JsonData jsonData = JsonUtility.FromJson<JsonData>(json);
            GameObject.Find("GameController").GetComponent<GameController>().NewPlayer(jsonData.Id,jsonData.Username);
        });

    }

    private void Disconnect(string obj)
    {
        throw new NotImplementedException();
    }

    private void onConnected(string obj)
    {
        Debug.Log("conexion exitosa " + Socket.Id);
    }

    public void SetUsername(string username)
    {
        Username = username;
    }
}

[Serializable]
public class JsonData
{
    public string Username;
    public string Id;
    public string Message;
    public GameState State;
}

[Serializable]
public class GameState
{
    public Player[] Players;
    public Coin[] Coins;

}
[Serializable]
public class Player
{
    public string Id;
    public string Username;
    public int x;
    public int y;
    public int Score;
}

[Serializable]
public class Coin
{
    public string Id;
    public int x;
    public int y;
}
